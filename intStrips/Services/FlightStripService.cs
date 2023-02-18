using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using intStrips.Helpers;
using intStrips.Models;
using intStripsShared.Models;

namespace intStrips.Services
{
    public class FlightStripService : IFlightStripService
    {
        private readonly IFlightDataReader _dataReader;
        private readonly IFlightDataWriter _dataWriter;
        private readonly IControlInfoService _infoService;

        private Dictionary<string, FlightDataModel> _data = new Dictionary<string, FlightDataModel>();
        private Dictionary<string, FlightStripModel> _strips = new Dictionary<string, FlightStripModel>();

        public FlightStripService(
            IFlightDataReader dataReader, 
            IFlightDataWriter dataWriter,
            IControlInfoService infoService)
        {
            _dataReader = dataReader;
            _dataWriter = dataWriter;
            _infoService = infoService;

            _dataReader.FlightDataChanged += DataChanged;
            _dataReader.FlightDataRefreshed += DataRefreshed;
            _dataReader.FlightDataAdded += DataAdded;
            _dataReader.FlightDataRemoved += DataRemoved;
            _infoService.ControlInfoChanged += HandleControlInfoChanged;
        }

        private void DataRemoved(object sender, FlightDataRemovedArgs e)
        {
            _data.Remove(e.Callsign);
            if (_strips.ContainsKey(e.Callsign))
                _strips[e.Callsign].PropertyChanged -= NotifyStripChanged;
            _strips.Remove(e.Callsign);
            
            FlightStripRemoved?.Invoke(this, new FlightStripRemovedArgs
            {
                Callsign = e.Callsign
            });
        }

        private void DataAdded(object sender, FlightDataAddedArgs f)
        {
            _data[f.Callsign] = f.FlightData;
            var controlling = _infoService.LastKnownInfo().AerodromeSource.Select(s => s.AerodromeCode).ToArray();
            _strips[f.Callsign] = GenerateStrip(f.FlightData, controlling);
            _strips[f.Callsign].PropertyChanged += NotifyStripChanged;
            
            FlightStripChanged?.Invoke(this, new FlightStripChangedArgs
            {
                Callsign = f.Callsign,
                Strip = _strips[f.Callsign]
            });
        }

        public void RequestAllFlightData()
        {
            _dataReader.RequestAllFlightData();
        }

        private void HandleControlInfoChanged(object sender, ControlInfoModel e)
        {
            ProcessStrips();
        }

        private void DataRefreshed(object sender, FlightDataModel[] data)
        {
            _data = data.ToDictionary(d => d.Callsign, d => d);
            ProcessStrips();
        }

        private void ProcessStrips()
        {
            foreach(var strip in _strips.Values)
                strip.PropertyChanged -= NotifyStripChanged;
            
            var controlling = _infoService.LastKnownInfo().AerodromeSource.Select(s => s.AerodromeCode).ToArray();
            var strips = _data.Select(d => new
            {
                Data = d.Value,
                Strip = GenerateStrip(d.Value, controlling)
            }).ToArray();

            foreach (var s in strips)
                s.Strip.PropertyChanged += NotifyStripChanged;

            _strips = strips.ToDictionary(s => s.Strip.Callsign, s => s.Strip);
            
            FlightStripsRefreshed?.Invoke(this, new FlightStripsRefreshedArgs
            {
              Strips = strips.Select(s => s.Strip).ToArray()
            });
        }

        private void NotifyStripChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is FlightStripModel strip && _strips.ContainsValue(strip))
            {
                // Discard updates that originated from DataChanged
                if (e is PropertyChangedCallerEventArgs callerArgs && callerArgs.Caller == nameof(FlightStripService))
                    return;
                
                string newData = null;
                string property = null;
                
                //public bool Active { get; set; } = true;
            
                if(e.PropertyName == nameof(strip.Gate))
                {
                    if (strip.StripType == StripType.DEPARTURE)
                    {
                        newData = strip.Gate;
                        property = nameof(FlightDataModel.OriginGate);
                    }
                    
                    if (strip.StripType == StripType.ARRIVAL)
                    {
                        newData = strip.Gate;
                        property = nameof(FlightDataModel.DestinationGate);
                    }
                }

                if (e.PropertyName == nameof(strip.Runway))
                {
                    if (strip.StripType == StripType.DEPARTURE)
                    {
                        newData = strip.Runway;
                        property = nameof(FlightDataModel.DepartureRunway);
                    }
                    
                    if (strip.StripType == StripType.ARRIVAL)
                    {
                        newData = strip.Runway;
                        property = nameof(FlightDataModel.ArrivalRunway);
                    }
                }

                if (e.PropertyName == nameof(strip.SelectedSid))
                {
                    if (strip.StripType == StripType.DEPARTURE)
                    {
                        newData = strip.SelectedSid;
                        property = nameof(FlightDataModel.SelectedSid);
                    }
                }

                if (e.PropertyName == nameof(strip.AssignedHeading))
                {
                    if (strip.StripType == StripType.DEPARTURE)
                    {
                        newData = strip.AssignedHeading;
                        property = nameof(FlightDataModel.AssignedHeading);
                    }
                }

                if (e.PropertyName == nameof(strip.RequestedAltitude))
                {
                    newData = strip.RequestedAltitude.ToString();
                    property = nameof(FlightDataModel.RequestedAltitude);
                }

                if (e.PropertyName == nameof(strip.AssignedAltitude))
                {
                    newData = strip.AssignedAltitude?.ToString() ?? "";
                    property = nameof(FlightDataModel.AssignedAltitude);
                }

                if (e.PropertyName == nameof(strip.FlightStage))
                {
                    switch (strip.FlightStage)
                    {
                        case FlightStage.CLEARANCE:
                            newData = "CLEARANCE";
                            break;
                        case FlightStage.TAXI:
                            newData = "TAXI";
                            break;
                        case FlightStage.READY:
                            newData = "READY";
                            break;
                        case FlightStage.LINE_UP:
                            newData = "LINE_UP";
                            break;
                        case FlightStage.TAKEOFF:
                            newData = "TAKEOFF";
                            break;
                        case FlightStage.AIRBORNE:
                            newData = "AIRBORNE";
                            break;
                        case FlightStage.LANDED:
                            newData = "LANDED";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    property = nameof(FlightDataModel.FlightStage);
                }

                if (e.PropertyName == nameof(strip.AssignedFrequency))
                {
                    newData = strip.AssignedFrequency;
                    property = nameof(FlightDataModel.AssignedFrequency);
                }

                if (e.PropertyName == nameof(strip.FlightRemarks))
                {
                    newData = strip.FlightRemarks;
                    property = nameof(FlightDataModel.FlightRemarks);
                }

                if (e.PropertyName == nameof(strip.LevelRemarks))
                {
                    newData = strip.LevelRemarks;

                    var controlLevel = _infoService.LastKnownInfo().ControlPosition;
                    switch (controlLevel)
                    {
                        case ControlPosition.TOWER:
                            property = nameof(FlightDataModel.TowerRemarks);
                            break;
                        case ControlPosition.TMA:
                            property = nameof(FlightDataModel.TmaRemarks);
                            break;
                        case ControlPosition.ENROUTE:
                            property = nameof(FlightDataModel.EnrouteRemarks);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (e.PropertyName == nameof(strip.GlobalRemarks))
                {
                    newData = strip.GlobalRemarks;
                    property = nameof(FlightDataModel.GlobalRemarks);
                }

                if(newData != null)
                    UpdateStripData(strip.Callsign, property, newData);
            }
        }

        private void DataChanged(object _, FlightDataChangedArgs e)
        {
            if (!_strips.ContainsKey(e.Callsign))
                return;
            var stripChanged = _strips[e.Callsign];
            string property = null;
            
            var controlInfo = _infoService.LastKnownInfo();
            var controlling = controlInfo.AerodromeSource.Select(s => s.AerodromeCode).ToArray();;

            if (e.Field == nameof(FlightDataModel.OriginGate) && stripChanged.StripType == StripType.DEPARTURE)
            {
                stripChanged.Gate = e.Update;
                property = nameof(stripChanged.Gate);
            }
            if (e.Field == nameof(FlightDataModel.DestinationGate) && stripChanged.StripType == StripType.ARRIVAL)
            {
                stripChanged.Gate = e.Update;
                property = nameof(stripChanged.Gate);
            }

            if (e.Field == nameof(FlightDataModel.Origin))
            {
                stripChanged.Origin = e.Update;
                property = nameof(stripChanged.Origin);
                
                if (stripChanged.StripType == StripType.DEPARTURE && !controlling.Contains(e.Update))
                {
                    stripChanged.StripType = StripType.LOCAL;
                    stripChanged.Active = false;
                    stripChanged.Gate = "";
                    stripChanged.OnGround = false;
                    stripChanged.StripTime = DateTime.MinValue;
                }
                else if (stripChanged.StripType != StripType.DEPARTURE && !controlling.Contains(e.Update))
                {
                    stripChanged.StripType = StripType.DEPARTURE;
                    stripChanged.Active = true;
                    stripChanged.Gate = "";
                    stripChanged.OnGround = stripChanged.FlightStage != FlightStage.AIRBORNE;
                    stripChanged.StripTime = DateTime.MinValue;
                }
            }

            if (e.Field == nameof(FlightDataModel.Destination))
            {
                stripChanged.Destination = e.Update;
                property = nameof(stripChanged.Destination);
                
                if (stripChanged.StripType == StripType.ARRIVAL && !controlling.Contains(e.Update))
                {
                    stripChanged.StripType = StripType.LOCAL;
                    stripChanged.Active = false;
                    stripChanged.Gate = "";
                    stripChanged.OnGround = false;
                    stripChanged.StripTime = DateTime.MinValue;
                }
                else if (stripChanged.StripType != StripType.ARRIVAL && !controlling.Contains(e.Update))
                {
                    stripChanged.StripType = StripType.ARRIVAL;
                    stripChanged.Active = true;
                    stripChanged.Gate = "";
                    stripChanged.OnGround = stripChanged.FlightStage == FlightStage.LANDED;
                }
            }

            if (e.Field == nameof(FlightDataModel.ApproachCategory))
            {
                stripChanged.ApproachCategory = Utilities.ParseApproachCategory(e.Update);
                property = nameof(stripChanged.ApproachCategory);
            }
            
            if (e.Field == nameof(FlightDataModel.AircraftType))
            {
                stripChanged.AircraftType = e.Update;
                property = nameof(stripChanged.AircraftType);
            }

            if (e.Field == nameof(FlightDataModel.WakeClass))
            {
                stripChanged.WakeClass =  Utilities.ParseWakeClass(e.Update);
                property = nameof(stripChanged.WakeClass);
            }
        
            if (e.Field == nameof(FlightDataModel.FlightType))
            {
                stripChanged.FlightType = Utilities.ParseFlightType(e.Update);
                property = nameof(stripChanged.FlightType);
            }
        
            if (e.Field == nameof(FlightDataModel.FlightRules))
            {
                stripChanged.FlightRules = Utilities.ParseFlightRules(e.Update);
                property = nameof(stripChanged.FlightRules);
            }
        
            if (e.Field == nameof(FlightDataModel.SsrCode))
            {
                stripChanged.SsrCode = int.TryParse(e.Update, out var ssr) ? (int?)ssr : null;
                property = nameof(stripChanged.SsrCode);
            }
        
            if (e.Field == nameof(FlightDataModel.DepartureRunway))
                if(stripChanged.StripType == StripType.DEPARTURE)
                {
                    stripChanged.Runway = e.Update;
                    property = nameof(stripChanged.Runway);
                }
            
            if (e.Field == nameof(FlightDataModel.ArrivalRunway))
                if(stripChanged.StripType == StripType.ARRIVAL)
                {
                    stripChanged.Runway = e.Update;
                    property = nameof(stripChanged.Runway);
                }
            
            if (e.Field == nameof(FlightDataModel.DepartureHoldingPoint))
                if(stripChanged.StripType == StripType.DEPARTURE)
                {
                    stripChanged.HoldingPoint = e.Update;
                    property = nameof(stripChanged.HoldingPoint);
                }
            
            if (e.Field == nameof(FlightDataModel.ArrivalHoldingPoint))
                if(stripChanged.StripType == StripType.ARRIVAL)
                {
                    stripChanged.HoldingPoint = e.Update;
                    property = nameof(stripChanged.HoldingPoint);
                }
            
            if (e.Field == nameof(FlightDataModel.SelectedSid) && stripChanged.StripType == StripType.DEPARTURE)
            {
                stripChanged.SelectedSid = e.Update;
                property = nameof(stripChanged.SelectedSid);
            }
            
            if (e.Field == nameof(FlightDataModel.SidTransition) && stripChanged.StripType == StripType.DEPARTURE)
            {
                stripChanged.SidTransition = e.Update;
                property = nameof(stripChanged.SidTransition);
            }
            
            if (e.Field == nameof(FlightDataModel.AssignedHeading) && stripChanged.StripType == StripType.DEPARTURE)
            {
                stripChanged.AssignedHeading = e.Update;
                property = nameof(stripChanged.AssignedHeading);
            }
        
            if (e.Field == nameof(FlightDataModel.RequestedAltitude))
            {
                stripChanged.RequestedAltitude = int.TryParse(e.Update, out var reqAlt) ? reqAlt : 7;
                property = nameof(stripChanged.RequestedAltitude);
            }

            if (e.Field == nameof(FlightDataModel.AssignedAltitude))
            {
                stripChanged.AssignedAltitude = int.TryParse(e.Update, out var asnAlt) ? (int?)asnAlt : null;
                property = nameof(stripChanged.AssignedAltitude);
            }
            
            if (e.Field == nameof(FlightDataModel.FlightStage))
            {
                stripChanged.FlightStage = Utilities.ParseFlightStage(e.Update);
                property = nameof(stripChanged.FlightStage);
            }

            if (e.Field == nameof(FlightDataModel.AssignedFrequency))
            {
                stripChanged.AssignedFrequency = e.Update;
                property = nameof(stripChanged.AssignedFrequency);
            }
        
            if (e.Field == nameof(FlightDataModel.FlightRoute))
            {
                stripChanged.FlightRoute = e.Update;
                property = nameof(stripChanged.FlightRoute);
            }

            if (e.Field == nameof(FlightDataModel.FlightRemarks))
            {
                stripChanged.FlightRemarks = e.Update;
                property = nameof(stripChanged.FlightRemarks);
            }
        
            if (e.Field == nameof(FlightDataModel.TowerRemarks) && controlInfo.ControlPosition == ControlPosition.TOWER)
            {
                stripChanged.LevelRemarks = e.Update;
                property = nameof(stripChanged.LevelRemarks);
            }
        
            if (e.Field == nameof(FlightDataModel.TmaRemarks) && controlInfo.ControlPosition == ControlPosition.TMA)
            {
                stripChanged.LevelRemarks = e.Update;
                property = nameof(stripChanged.LevelRemarks);
            }
        
            if (e.Field == nameof(FlightDataModel.EnrouteRemarks) && controlInfo.ControlPosition == ControlPosition.ENROUTE)
            {
                stripChanged.LevelRemarks = e.Update;
                property = nameof(stripChanged.LevelRemarks);
            }
        
            if (e.Field == nameof(FlightDataModel.GlobalRemarks))
            {
                stripChanged.GlobalRemarks = e.Update;
                property = nameof(stripChanged.GlobalRemarks);
            }
        
            if (e.Field == nameof(FlightDataModel.EstimatedDepartureTime) && stripChanged.StripType == StripType.DEPARTURE && stripChanged.OnGround)
            {
                if(DateTime.TryParse(e.Update, out var estimatedDeparture))
                    stripChanged.StripTime = estimatedDeparture;
                property = nameof(stripChanged.StripTime);
            }
            
            if (e.Field == nameof(FlightDataModel.ActualDepartureTime) && stripChanged.StripType == StripType.DEPARTURE)
            {
                if(DateTime.TryParse(e.Update, out var actualDeparture))
                    stripChanged.StripTime = actualDeparture;
                property = nameof(stripChanged.StripTime);
            }
            
            if (e.Field == nameof(FlightDataModel.EstimatedArrivalTime) && stripChanged.StripType == StripType.ARRIVAL && !stripChanged.OnGround)
            {
                if(DateTime.TryParse(e.Update, out var estimatedArrival))
                    stripChanged.StripTime = estimatedArrival;
                property = nameof(stripChanged.StripTime);
            }
            
            if (e.Field == nameof(FlightDataModel.ActualArrivalTime) && stripChanged.StripType == StripType.ARRIVAL)
            {
                if(DateTime.TryParse(e.Update, out var actualArrival))
                    stripChanged.StripTime = actualArrival;
                property = nameof(stripChanged.StripTime);
            }
            
            if (e.Field == nameof(FlightDataModel.SquawkingCode))
            {
                stripChanged.SquawkingCode = e.Update == "true";
                property = nameof(stripChanged.SquawkingCode);
            }
            
            if (e.Field == nameof(FlightDataModel.OnGround))
            {
                if(stripChanged.StripType == StripType.DEPARTURE)
                    stripChanged.OnGround = string.IsNullOrWhiteSpace(e.Update) || e.Update == "true";
                else
                    stripChanged.OnGround = !string.IsNullOrWhiteSpace(e.Update) && e.Update == "true";

                property = nameof(stripChanged.OnGround);
            }

            if (property != null)
            {
                FlightStripChanged?.Invoke(this, new FlightStripChangedArgs
                {
                    Callsign = stripChanged.Callsign,
                    Strip = stripChanged
                });
            }
        }

        public event EventHandler<FlightStripsRefreshedArgs> FlightStripsRefreshed;
        public event EventHandler<FlightStripChangedArgs> FlightStripChanged;
        public event EventHandler<FlightStripRemovedArgs> FlightStripRemoved;

        public void UpdateStripData(string callsign, string property, string value)
        {
            _dataWriter.SetFlightDataField(this, callsign, property, value);
        }

        public void Dispose()
        {
            _dataReader?.Dispose();
            _dataWriter?.Dispose();
        }

        public static FlightStripModel GenerateStrip(FlightDataModel d, string[] controlling)
        {
            var strip = new FlightStripModel
            {
                Callsign = d.Callsign,
                Origin = d.Origin,
                Destination = d.Destination,
                ApproachCategory = Utilities.ParseApproachCategory(d.ApproachCategory),
                AircraftType = d.AircraftType,
                WakeClass = Utilities.ParseWakeClass(d.WakeClass),
                FlightType = Utilities.ParseFlightType(d.FlightType),
                FlightRules = Utilities.ParseFlightRules(d.FlightRules),
                SsrCode = int.TryParse(d.SsrCode, out var ssr) ? (int?)ssr : null,
                SquawkingCode = d.SquawkingCode == "True",
                SelectedSid = d.SelectedSid,
                SidTransition = d.SidTransition,
                AssignedHeading = d.AssignedHeading,
                RequestedAltitude = int.TryParse(d.RequestedAltitude, out var reqAlt) ? reqAlt : 7,
                AssignedAltitude = int.TryParse(d.AssignedAltitude, out var asnAlt) ? (int?)asnAlt : null,
                FlightStage = Utilities.ParseFlightStage(d.FlightStage),
                AssignedFrequency = d.AssignedFrequency,
                FlightRoute = d.FlightRoute,
                FlightRemarks = d.FlightRemarks,
                LevelRemarks = d.TowerRemarks,
                GlobalRemarks = d.GlobalRemarks
            };
            
            if (controlling.Contains(strip.Destination) && (!controlling.Contains(strip.Origin) || d.ActualDepartureTime != ""))
            {
                strip.StripType = StripType.ARRIVAL;
                strip.Active = true;
                strip.Gate = d.DestinationGate;
                strip.Runway = d.ArrivalRunway;
                strip.HoldingPoint = d.ArrivalHoldingPoint;
                strip.OnGround = !string.IsNullOrWhiteSpace(d.OnGround) && d.OnGround == "True";
                    
                strip.StripTime =
                    DateTime.TryParse(d.ActualArrivalTime, out var actualArrival) ? 
                        actualArrival :
                        DateTime.TryParse(d.EstimatedArrivalTime, out var estimatedArrival) ?
                            estimatedArrival : DateTime.MinValue;
            } 
            else if (controlling.Contains(strip.Origin))
            {
                strip.StripType = StripType.DEPARTURE;
                strip.Active = true;
                strip.Gate = d.OriginGate;
                strip.Runway = d.DepartureRunway;
                strip.HoldingPoint = d.DepartureHoldingPoint;
                strip.OnGround = string.IsNullOrWhiteSpace(d.OnGround) || d.OnGround == "True";

                strip.StripTime =
                    DateTime.TryParse(d.ActualDepartureTime, out var actualArrival) ? 
                        actualArrival :
                        DateTime.TryParse(d.EstimatedDepartureTime, out var estimatedArrival) ?
                            estimatedArrival : DateTime.MinValue;
            }
            else
            {
                strip.StripType = StripType.LOCAL;
                strip.Active = false;
            }

            return strip;
        }
    }
}