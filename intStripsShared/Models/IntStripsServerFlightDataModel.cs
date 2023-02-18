using System;

namespace intStripsShared.Models
{
    [Serializable]
    public class IntStripsServerFlightDataModel
    {
        public string Callsign { get; set; }

        public string OriginGate { get; set; }
        public string DestinationGate { get; set; }

        public string DepartureHoldingPoint { get; set; }
        public string ArrivalHoldingPoint { get; set; }

        public string AssignedHeading { get; set; }

        public string FlightStage { get; set; }
        public string AssignedFrequency { get; set; }

        public string FlightRemarks { get; set; }
        public string TowerRemarks { get; set; }
        public string TmaRemarks { get; set; }
        public string EnrouteRemarks { get; set; }
    }
}