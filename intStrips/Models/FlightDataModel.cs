namespace intStrips.Models
{
    public class FlightDataModel
    {
        public string Callsign { get; set; }

        public string OriginGate { get; set; }
        public string DestinationGate { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        
        public string ApproachCategory { get; set; }
        public string AircraftType { get; set; }
        public string WakeClass { get; set; }
        
        public string FlightType { get; set; }
        public string FlightRules { get; set; }
        public string SsrCode { get; set; }

        public string DepartureRunway { get; set; }
        public string ArrivalRunway { get; set; }
        public string DepartureHoldingPoint { get; set; }
        public string ArrivalHoldingPoint { get; set; }

        public string SelectedSid { get; set; }
        public string SidTransition { get; set; }
        public string AssignedHeading { get; set; }

        public string RequestedAltitude { get; set; }
        public string AssignedAltitude { get; set; }

        public string FlightStage { get; set; }
        public string AssignedFrequency { get; set; }

        public string FlightRoute { get; set; }
        public string FlightRemarks { get; set; }
        public string TowerRemarks { get; set; }
        public string TmaRemarks { get; set; }
        public string EnrouteRemarks { get; set; }
        public string GlobalRemarks { get; set; }

        public string EstimatedDepartureTime { get; set; }
        public string ActualDepartureTime { get; set; }
        public string EstimatedArrivalTime { get; set; }
        public string ActualArrivalTime { get; set; }
    }
}