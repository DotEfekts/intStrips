using System;

namespace intStripsShared.Models
{
    [Serializable]
    public class VatSysFlightDataModel
    {
        public string Callsign { get; set; }

        public string Origin { get; set; }
        public string Destination { get; set; }
        
        public string ApproachCategory { get; set; }
        public string AircraftType { get; set; }
        public string WakeClass { get; set; }
        
        public string FlightType { get; set; }
        public string FlightRules { get; set; }
        public string SquawkingCode { get; set; }
        public string SsrCode { get; set; }

        public string DepartureRunway { get; set; }
        public string ArrivalRunway { get; set; }

        public string SelectedSid { get; set; }
        public string SidTransition { get; set; }

        public string RequestedAltitude { get; set; }
        public string AssignedAltitude { get; set; }
        
        public string TrackAvailable { get; set; }
        public string OnGround { get; set; }

        public string FlightRoute { get; set; }
        public string GlobalRemarks { get; set; }

        public string EstimatedDepartureTime { get; set; }
        public string ActualDepartureTime { get; set; }
        public string EstimatedArrivalTime { get; set; }
        public string ActualArrivalTime { get; set; }
    }
}