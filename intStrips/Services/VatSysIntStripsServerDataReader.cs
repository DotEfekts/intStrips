using System;
using System.Collections.Generic;
using System.Linq;
using intStripsServer;
using intStrips.Models;
using intStripsShared.Models;

namespace intStrips.Services
{
    public class VatSysIntStripsServerDataReader : IFlightDataReader, IFlightDataWriter
    {
        private readonly VatSysConnector _vatSysConnector;
        private readonly IntStripsConnector _intStripsConnector;
        private List<FlightDataModel> _flightData = new List<FlightDataModel>();

        private static string[] _vatSysFields =
        {
            "DepartureRunway",
            "ArrivalRunway",
            "AssignedAltitude",
            "GlobalRemarks",
            "RequestedAltitude",
            "SelectedSid"
        };

        private static string[] _intStripsFields =
        {
            "FlightStage",
            "AssignedFrequency",
            "AssignedHeading",
            "DestinationGate",
            "EnrouteRemarks",
            "FlightRemarks",
            "OriginGate",
            "TmaRemarks",
            "TowerRemarks",
            "ArrivalHoldingPoint",
            "DepartureHoldingPoint"
        };

        public VatSysIntStripsServerDataReader(VatSysConnector vatSysConnector, IntStripsConnector intStripsConnector)
        {
            _vatSysConnector = vatSysConnector;
            _intStripsConnector = intStripsConnector;
            _vatSysConnector.DataRefreshComplete += RefreshVatSysFdrData;
            _vatSysConnector.FlightDataAdded += AddVatSysFdrData;
            _vatSysConnector.FlightDataChanged += UpdateVatSysFdrData;
            _vatSysConnector.FlightDataRemoved += RemoveVatSysFdrData;
        }


        private void AddVatSysFdrData(object sender, VatSysFlightDataModel f)
        {
            var flightInfo = _intStripsConnector.SendFdrListRequest(new[] { f.Callsign }).FirstOrDefault();
            flightInfo = flightInfo ?? new FlightInfo();
            
            FlightDataAdded?.Invoke(this, new FlightDataAddedArgs
            {
                Callsign = f.Callsign,
                FlightData = new FlightDataModel
                {
                    Callsign = f.Callsign,
                    DepartureRunway = f.DepartureRunway,
                    ArrivalRunway = f.ArrivalRunway,
                    Origin = f.Origin,
                    Destination = f.Destination,
                    AircraftType = f.AircraftType,
                    ApproachCategory = f.ApproachCategory,
                    AssignedAltitude = f.AssignedAltitude,
                    FlightRoute = f.FlightRoute,
                    FlightRules = f.FlightRules,
                    FlightType = "S",
                    GlobalRemarks = f.GlobalRemarks,
                    RequestedAltitude = f.RequestedAltitude,
                    SelectedSid = f.SelectedSid,
                    SidTransition = "",
                    SsrCode = f.SsrCode,
                    SquawkingCode = f.SquawkingCode,
                    WakeClass = f.WakeClass,
                    EstimatedDepartureTime = f.EstimatedDepartureTime,
                    ActualDepartureTime = f.ActualDepartureTime,
                    EstimatedArrivalTime = f.EstimatedArrivalTime,
                    ActualArrivalTime = f.ActualArrivalTime,
                    OnGround = f.OnGround,
                    
                    FlightStage = flightInfo.FlightStage,
                    AssignedFrequency = flightInfo.AssignedFrequency,
                    AssignedHeading = flightInfo.AssignedHeading,
                    DestinationGate = flightInfo.DestinationGate,
                    EnrouteRemarks = flightInfo.EnrouteRemarks,
                    FlightRemarks = flightInfo.FlightRemarks,
                    OriginGate = flightInfo.OriginGate,
                    TmaRemarks = flightInfo.TmaRemarks,
                    TowerRemarks = flightInfo.TmaRemarks,
                    ArrivalHoldingPoint = flightInfo.ArrivalHoldingPoint,
                    DepartureHoldingPoint = flightInfo.DepartureHoldingPoint
                }
            });
        }
        
        private void UpdateVatSysFdrData(object sender, FlightDataChangedArgs e)
        {
            FlightDataChanged?.Invoke(this, e);
        }

        private void RemoveVatSysFdrData(object sender, string e)
        { 
            FlightDataRemoved?.Invoke(this, new FlightDataRemovedArgs
            {
                Callsign = e
            });
        }

        public void RequestAllFlightData()
        {
            _vatSysConnector.SendFdrListRequest();
        }

        private void RefreshVatSysFdrData(object _, VatSysFlightDataModel[] data)
        {
            var flightInfo = _intStripsConnector.SendFdrListRequest(data.Select(d => d.Callsign));

            _flightData = data.Select(f => new
                {
                    d = f,
                    i = flightInfo.FirstOrDefault(t => t.Callsign == f.Callsign) ?? new FlightInfo()
                })
                .Select(f => new FlightDataModel
                {
                    Callsign = f.d.Callsign,
                    DepartureRunway = f.d.DepartureRunway,
                    ArrivalRunway = f.d.ArrivalRunway,
                    Origin = f.d.Origin,
                    Destination = f.d.Destination,
                    AircraftType = f.d.AircraftType,
                    ApproachCategory = f.d.ApproachCategory,
                    AssignedAltitude = f.d.AssignedAltitude,
                    FlightRoute = f.d.FlightRoute,
                    FlightRules = f.d.FlightRules,
                    GlobalRemarks = f.d.GlobalRemarks,
                    RequestedAltitude = f.d.RequestedAltitude,
                    SelectedSid = f.d.SelectedSid,
                    SidTransition = "",
                    SsrCode = f.d.SsrCode,
                    SquawkingCode = f.d.SquawkingCode,
                    WakeClass = f.d.WakeClass,
                    EstimatedDepartureTime = f.d.EstimatedDepartureTime,
                    ActualDepartureTime = f.d.ActualDepartureTime,
                    EstimatedArrivalTime = f.d.EstimatedArrivalTime,
                    ActualArrivalTime = f.d.ActualArrivalTime,
                    OnGround = f.d.OnGround,
                    
                    FlightStage = f.i.FlightStage,
                    AssignedFrequency = f.i.AssignedFrequency,
                    AssignedHeading = f.i.AssignedHeading,
                    DestinationGate = f.i.DestinationGate,
                    EnrouteRemarks = f.i.EnrouteRemarks,
                    FlightRemarks = f.i.FlightRemarks,
                    OriginGate = f.i.OriginGate,
                    TmaRemarks = f.i.TmaRemarks,
                    TowerRemarks = f.i.TmaRemarks,
                    ArrivalHoldingPoint = f.i.ArrivalHoldingPoint,
                    DepartureHoldingPoint = f.i.DepartureHoldingPoint
                }).ToList();
            
            FlightDataRefreshed?.Invoke(this, _flightData.ToArray());
        }
        
        public event EventHandler<FlightDataModel[]> FlightDataRefreshed;
        public event EventHandler<FlightDataAddedArgs> FlightDataAdded;
        public event EventHandler<FlightDataChangedArgs> FlightDataChanged;
        public event EventHandler<FlightDataRemovedArgs> FlightDataRemoved;

        public void Dispose()
        {
            _vatSysConnector?.Dispose();
        }

        public void SetFlightDataField(object sender, string callsign, string field, string value)
        {
            if (_vatSysFields.Contains(field))
                _vatSysConnector.SendUpdateRequest(callsign, field, value);
            else if (_intStripsFields.Contains(field))
                _intStripsConnector.SendUpdateRequest(new FlightUpdateRequest
                {
                    Callsign = callsign,
                    Field = field,
                    Value = value
                });
        }
    }
}