using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using vatsys;
using vatsys.Plugin;
using System.ComponentModel.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using intStripsShared.Models;

namespace intStripsPlugins
{
    [Export(typeof(IPlugin))]   
    public class IntStripsDataConnector : IPlugin, IDisposable
    {
        private readonly BinaryFormatter _formatter = new BinaryFormatter();
        private readonly Semaphore _clientSemaphore;

        private readonly Dictionary<string, FDP2.FDR> _trackedFdrs = new Dictionary<string, FDP2.FDR>();
        private readonly Dictionary<string, TrackData> _radarTrackData = new Dictionary<string, TrackData>();

        private bool _disposed;
        private NamedPipeServerStream _serverStream;
        private NamedPipeClientStream _clientStream;
        
        public string Name => "IntStripsDataConnector";
        
        public IntStripsDataConnector()
        {
            var serverThread = new Thread(StartPipeServer);
            serverThread.Start();
            
            _clientSemaphore = new Semaphore(1, 1);

            Network.Connected += SendConnectionInfo;
            MMI.PrimePositonChanged += SendConnectionInfo;
            Network.Disconnected += ClearFdrs;
        }

        public void OnFDRUpdate(FDP2.FDR updated)
        {
            if (!_trackedFdrs.ContainsKey(updated.Callsign) &&
                FDP2.GetFDRIndex(updated.Callsign) != -1)
            {
                _trackedFdrs[updated.Callsign] = updated;
                updated.PropertyChanged += HandleFdrPropertyChange;
                SendNewFdr(updated);
            }
            else if (FDP2.GetFDRIndex(updated.Callsign) == -1)
            {
                _trackedFdrs.Remove(updated.Callsign);
                _radarTrackData.Remove(updated.Callsign);
                updated.PropertyChanged -= HandleFdrPropertyChange;
                SendRemovedFdr(updated.Callsign);
            }
        }

        private void ClearFdrs(object sender, EventArgs e)
        {
            var fdrs = _trackedFdrs.Values;
            foreach (var record in fdrs)
            {
                _radarTrackData.Remove(record.Callsign);
                record.PropertyChanged -= HandleFdrPropertyChange;
                SendRemovedFdr(record.Callsign);
            }
            
            _trackedFdrs.Clear();
        }

        private void SendConnectionInfo(object sender, EventArgs e)
        {
            var aerodromes = MMI.PrimePosition.ArrivalListAirports.ToArray();
            var parsedAerodromes = new List<AerodromeModel>();
            
            foreach (var aerodrome in aerodromes)
            {
                var runways = Airspace2.GetRunways(aerodrome);
                parsedAerodromes.Add(new AerodromeModel()
                {
                    AerodromeCode = aerodrome,
                    Runways = runways.Select(r => new RunwaysModel
                    {
                        Runway = r.Name,
                        SidStars = r.SIDs.Select(s => new SidStarsModel
                        {
                            Code = s.sidStar.Name,
                            Transitions = s.sidStar.Transitions.Keys.ToArray()
                        }).Concat(r.STARApproaches.Select(s => new SidStarsModel
                        {
                            Code = s.Key.sidStar.Name,
                            Transitions = s.Key.sidStar.Transitions.Keys.ToArray(),
                            Star = true
                        })).ToArray()
                    }).ToArray()
                });
            }
            
            ControlPosition position;
            switch (MMI.PrimePosition.Group)
            {
                case "TCU":
                    position = ControlPosition.TMA;
                    break;
                case "":
                case "Oceanic":
                    position = ControlPosition.ENROUTE;
                    break;
                // case "Class D TWR":
                // case "Procedural TWR":
                // case "Radar TWR":
                default:
                    position = ControlPosition.TOWER;
                    break;
                    
            }

            var requestId = Guid.NewGuid().ToString();
            SendClientMessage(new ControlInfoChanged()
            {
                RequestId = requestId,
                Data = new ControlInfoModel
                {
                    AerodromeSource = parsedAerodromes.ToArray(),
                    ControlPosition = position
                }
            });
        }

        private void SendNewFdr(FDP2.FDR record)
        {
            var requestId = Guid.NewGuid().ToString();
            SendClientMessage( new AddRequestModel()
            {
                RequestId = requestId,
                Data = DataModelFromFdr(record)
            });
        }
        
        private void HandleFdrPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            if (sender is FDP2.FDR record)
            {
                string property = null;
                string value = null;

                if (e.PropertyName == nameof(FDP2.FDR.DepartureRunway))
                {
                    property = nameof(VatSysFlightDataModel.DepartureRunway);
                    value = record.DepartureRunway?.Name ?? "";
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.ArrivalRunway))
                {
                    property = nameof(VatSysFlightDataModel.ArrivalRunway);
                    value = record.ArrivalRunway?.Name ?? "";
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.DepAirport))
                {
                    property = nameof(VatSysFlightDataModel.Origin);
                    value = record.DepAirport;
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.DesAirport))
                {
                    property = nameof(VatSysFlightDataModel.Destination);
                    value = record.DesAirport;
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.AircraftType))
                {
                    property = nameof(VatSysFlightDataModel.AircraftType);
                    value = record.AircraftType;
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.CFLString))
                {
                    property = nameof(VatSysFlightDataModel.AssignedAltitude);
                    value = record.CFLString;
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.Route))
                {
                    property = nameof(VatSysFlightDataModel.FlightRoute);
                    value = record.Route;
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.FlightRules))
                {
                    property = nameof(VatSysFlightDataModel.FlightRules);
                    value = record.FlightRules;
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.GlobalOpData))
                {
                    property = nameof(VatSysFlightDataModel.GlobalRemarks);
                    value = record.GlobalOpData;
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.RFL))
                {
                    property = nameof(VatSysFlightDataModel.RequestedAltitude);
                    value = record.RFL.ToString("D5").Substring(0, 3);
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.SIDSTARString))
                {
                    property = nameof(VatSysFlightDataModel.SelectedSid);
                    value = record.SIDSTARString;
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.AssignedSSRCode))
                {
                    property = nameof(VatSysFlightDataModel.SsrCode);
                    value = record.AssignedSSRCode == -1 ? "" : Convert.ToString(record.AssignedSSRCode, 8).PadLeft(4, '0');
                }

                if (e.PropertyName == nameof(FDP2.FDR.AircraftWake))
                {
                    property = nameof(VatSysFlightDataModel.WakeClass);
                    value = record.AircraftWake;
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.ETD))
                {
                    property = nameof(VatSysFlightDataModel.EstimatedDepartureTime);
                    value = record.ETD.ToString("o");
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.ATD))
                {
                    property = nameof(VatSysFlightDataModel.ActualDepartureTime);
                    value = record.ATD == DateTime.MaxValue ? "" : record.ATD.ToString("o");
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.CoupledTrack))
                {
                    property = nameof(VatSysFlightDataModel.TrackAvailable);
                    value = (record.CoupledTrack != null).ToString();
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.ParsedRoute))
                {
                    property = nameof(VatSysFlightDataModel.EstimatedArrivalTime);
                    value = record.ParsedRoute.LastOrDefault()?.ETO.ToString("o") ?? "";
                }
                
                if (e.PropertyName == nameof(FDP2.FDR.ParsedRoute))
                {
                    property = nameof(VatSysFlightDataModel.ActualArrivalTime);
                    value = record.ParsedRoute.LastOrDefault()?.ATO == DateTime.MaxValue
                        ? ""
                        : record.ParsedRoute.LastOrDefault()?.ATO.ToString("o") ?? "";
                }

                if (property == null) return;
                
                var requestId = Guid.NewGuid().ToString();
                SendClientMessage(new UpdateRequestModel()
                {
                    RequestId = requestId,
                    Callsign = record.Callsign,
                    Field = property,
                    Value = value
                });
            }
        }

        private void SendRemovedFdr(string callsign)
        {
            var requestId = Guid.NewGuid().ToString();
            SendClientMessage(new RemoveRequestModel()
            {
                RequestId = requestId,
                Callsign = callsign
            });
        }

        public void OnRadarTrackUpdate(RDP.RadarTrack updated) 
        {
            if (_radarTrackData.ContainsKey(updated.CoupledFDR?.Callsign ?? ""))
            {
                var trackData = _radarTrackData[updated.CoupledFDR.Callsign];

                if (trackData.SquawkingCode !=
                    (updated.ActualAircraft?.TransponderCode == updated.CoupledFDR.AssignedSSRCode))
                {
                    trackData.SquawkingCode =
                        updated.ActualAircraft?.TransponderCode == updated.CoupledFDR.AssignedSSRCode;
                    
                    var requestId = Guid.NewGuid().ToString();
                    SendClientMessage(new UpdateRequestModel()
                    {
                        RequestId = requestId,
                        Callsign = updated.CoupledFDR.Callsign,
                        Field = nameof(VatSysFlightDataModel.SquawkingCode),
                        Value = trackData.SquawkingCode.ToString()
                    });
                }
                
                if (trackData.OnGround != updated.OnGround)
                {
                    trackData.OnGround = updated.OnGround;
                    
                    var requestId = Guid.NewGuid().ToString();
                    SendClientMessage(new UpdateRequestModel()
                    {
                        RequestId = requestId,
                        Callsign = updated.CoupledFDR.Callsign,
                        Field = nameof(VatSysFlightDataModel.OnGround),
                        Value = trackData.OnGround.ToString()
                    });
                }
            }
        }

        private void StartPipeServer()
        {
            while (!_disposed)
            {
                try
                {
                    _serverStream = new NamedPipeServerStream("intStripsServer", PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    _serverStream.WaitForConnection();
                    
                    while (_serverStream.IsConnected)
                    {
                        try
                        {
                            var message = (PipeMessageModel)_formatter.Deserialize(_serverStream);

                            if (message.ModelType == ModelType.COMMAND_REQUEST)
                            {
                                var command = message as CommandRequestModel;
                                Debug.Assert(command != null, nameof(command) + " != null");

                                if (command.Command == CommandRequestModel.CommandType.CONTROL_INFO_REFRESH)
                                {
                                    var aerodromes = MMI.PrimePosition.ArrivalListAirports.ToArray();
                                    var parsedAerodromes = new List<AerodromeModel>();
                                    
                                    foreach (var aerodrome in aerodromes)
                                    {
                                        var runways = Airspace2.GetRunways(aerodrome);
                                        parsedAerodromes.Add(new AerodromeModel()
                                        {
                                            AerodromeCode = aerodrome,
                                            Runways = runways.Select(r => new RunwaysModel
                                            {
                                                Runway = r.Name,
                                                SidStars = r.SIDs.Select(s => new SidStarsModel
                                                {
                                                    Code = s.sidStar.Name,
                                                    Transitions = s.sidStar.Transitions.Keys.ToArray()
                                                }).Concat(r.STARApproaches.Select(s => new SidStarsModel
                                                {
                                                    Code = s.Key.sidStar.Name,
                                                    Transitions = s.Key.sidStar.Transitions.Keys.ToArray(),
                                                    Star = true
                                                })).ToArray()
                                            }).ToArray()
                                        });
                                    }
                                    
                                    ControlPosition position;
                                    switch (MMI.PrimePosition.Group)
                                    {
                                        case "TCU":
                                            position = ControlPosition.TMA;
                                            break;
                                        case "":
                                        case "Oceanic":
                                            position = ControlPosition.ENROUTE;
                                            break;
                                        // case "Class D TWR":
                                        // case "Procedural TWR":
                                        // case "Radar TWR":
                                        default:
                                            position = ControlPosition.TOWER;
                                            break;
                                            
                                    }
                                    
                                    _formatter.Serialize(_serverStream, new RequestResponseModel
                                    {
                                        RequestId = message.RequestId,
                                        Success = true,
                                        Data = new ControlInfoModel
                                        {
                                            AerodromeSource = parsedAerodromes.ToArray(),
                                            ControlPosition = position
                                        }
                                    });
                                }

                                if (command.Command == CommandRequestModel.CommandType.OPEN_FLIGHT_PLAN)
                                {
                                    var record = FDP2.GetFDRs.FirstOrDefault(f => f.Callsign == command.Data);
                                    if (record == null)
                                    {
                                        _formatter.Serialize(_serverStream, new RequestResponseModel
                                        {
                                            RequestId = message.RequestId,
                                            Success = false
                                        });
                                        continue;
                                    }

                                    MMI.InvokeOnGUI(() => MMI.OpenFPWindow(record));

                                    _formatter.Serialize(_serverStream, new RequestResponseModel
                                    {
                                        RequestId = message.RequestId,
                                        Success = true
                                    });
                                    continue;
                                }

                                if (command.Command == CommandRequestModel.CommandType.AUTO_ASSIGN_SSR)
                                {
                                    var record = FDP2.GetFDRs.FirstOrDefault(f => f.Callsign == command.Data);
                                    if (record == null)
                                    {
                                        _formatter.Serialize(_serverStream, new RequestResponseModel
                                        {
                                            RequestId = message.RequestId,
                                            Success = false
                                        });
                                        continue;
                                    }

                                    var ssr = Convert.ToString(SquawkAssignment.AssignCode(record), 8).PadLeft(4, '0');

                                    _formatter.Serialize(_serverStream, new RequestResponseModel
                                    {
                                        RequestId = message.RequestId,
                                        Success = true
                                    });
                                    continue;
                                }

                                if (command.Command == CommandRequestModel.CommandType.FDR_REFRESH)
                                {
                                    var records = FDP2.GetFDRs;
                                    foreach (var record in records)
                                    {
                                        if (!_trackedFdrs.ContainsKey(record.Callsign))
                                        {
                                            _trackedFdrs[record.Callsign] = record;
                                            record.PropertyChanged += HandleFdrPropertyChange;
                                        }
                                    }
                                    
                                    _formatter.Serialize(_serverStream, new RequestResponseModel
                                    {
                                        RequestId = message.RequestId,
                                        Success = true,
                                        Data = records.Select(DataModelFromFdr).ToArray()
                                    });
                                }

                            }

                            if (message.ModelType == ModelType.UPDATE_REQUEST)
                            {
                                var update = message as UpdateRequestModel;
                                Debug.Assert(update != null, nameof(update) + " != null");

                                var record = FDP2.GetFDRs.FirstOrDefault(f => f.Callsign == update.Callsign);

                                if (record == null || !record.HavePermission)
                                {
                                    _formatter.Serialize(_serverStream, new RequestResponseModel
                                    {
                                        RequestId = message.RequestId,
                                        Success = false
                                    });
                                    continue;
                                }

                                if (update.Field == nameof(FDP2.FDR.GlobalOpData))
                                    record.GlobalOpData = update.Value;

                                if (update.Field == nameof(FDP2.FDR.DepartureRunway))
                                {
                                    var runway = Airspace2.GetRunway(record.DepAirport, update.Value);
                                    if (runway != null)
                                        record.DepartureRunway = runway;
                                }

                                if (update.Field == nameof(FDP2.FDR.ArrivalRunway))
                                {
                                    var runway = Airspace2.GetRunway(record.DesAirport, update.Value) ??
                                                 Airspace2.GetRunway(record.AltAirport, update.Value);
                                    if (runway != null)
                                        record.ArrivalRunway = runway;

                                }
                                
                                _formatter.Serialize(_serverStream, new RequestResponseModel
                                {
                                    RequestId = message.RequestId,
                                    Success = true
                                });
                            }
                        }
                        catch (SerializationException ex)
                        {
                            Debug.WriteLine(ex.Message);
                            Debug.WriteLine(ex.StackTrace);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                }
                finally
                {
                    _serverStream.Dispose();
                }
            }
        }

        private void SendClientMessage(PipeMessageModel message)
        {
            if (!_clientSemaphore.WaitOne()) return;
            try
            {
                if (!EnsureClientConnected())
                    return;
                
                var tokenSource = new CancellationTokenSource();
                Task.Run(() => StartTimeout(tokenSource.Token), tokenSource.Token);

                try
                {
                    _formatter.Serialize(_clientStream, message);
                    _formatter.Deserialize(_clientStream);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
                
                tokenSource.Cancel();
            }
            finally
            {
                _clientSemaphore.Release();
            }
        }

        private void StartTimeout(CancellationToken token)
        {
            Thread.Sleep(1000);
            if(!token.IsCancellationRequested)
                _clientStream.Dispose();
        }

        private bool EnsureClientConnected()
        {
            if (!_disposed && (_clientStream == null || !_clientStream.IsConnected))
            {
                _clientStream = new NamedPipeClientStream(".", "intStripsClient", PipeDirection.InOut, PipeOptions.Asynchronous);
                try
                {
                    _clientStream.Connect(1000);
                }
                catch
                {
                    // ignored
                }
            }

            return _clientStream != null && _clientStream.IsConnected;
        }

        public void Dispose()
        {
            _disposed = true;
            _serverStream?.Dispose();
            _clientStream?.Dispose();
        }

        private VatSysFlightDataModel DataModelFromFdr(FDP2.FDR f)
        {
            _radarTrackData[f.Callsign] = new TrackData()
            {
                SquawkingCode = f.CoupledTrack?.ActualAircraft?.TransponderCode == f.AssignedSSRCode,
                OnGround = f.CoupledTrack?.OnGround
            };
            
            return new VatSysFlightDataModel
            {
                Callsign = f.Callsign,
                DepartureRunway = f.DepartureRunway?.Name ?? "",
                ArrivalRunway = f.ArrivalRunway?.Name ?? "",
                Origin = f.DepAirport,
                Destination = f.DesAirport,
                AircraftType = f.AircraftType,
                AssignedAltitude = f.CFLString,
                FlightRoute = f.Route,
                FlightRules = f.FlightRules,
                FlightType = "S",
                GlobalRemarks = f.GlobalOpData,
                RequestedAltitude = f.RFL.ToString("D5").Substring(0, 3),
                SelectedSid = f.SIDSTARString,
                SidTransition = "",
                SsrCode = f.AssignedSSRCode == -1 ? "" : Convert.ToString(f.AssignedSSRCode, 8).PadLeft(4, '0'),
                SquawkingCode = (f.CoupledTrack?.ActualAircraft?.TransponderCode == f.AssignedSSRCode).ToString(),
                TrackAvailable = (f.CoupledTrack != null).ToString(),
                WakeClass = f.AircraftWake,
                EstimatedDepartureTime = f.ETD.ToString("o"),
                ActualDepartureTime = f.ATD == DateTime.MaxValue ? "" : f.ATD.ToString("o"),
                EstimatedArrivalTime = f.ParsedRoute.LastOrDefault()?.ETO.ToString("o") ?? "",
                ActualArrivalTime = f.ParsedRoute.LastOrDefault()?.ATO == DateTime.MaxValue
                    ? ""
                    : f.ParsedRoute.LastOrDefault()?.ATO.ToString("o") ?? "",
                OnGround = (f.CoupledTrack?.OnGround).ToString()
            };
        }
    }

    internal class TrackData
    {
        public bool SquawkingCode { get; set; }
        public bool? OnGround { get; set; }
    }
}