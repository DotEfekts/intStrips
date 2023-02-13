namespace intStrips.Models
{
    public class FlightDataChangedArgs
    {
        public string Callsign { get; set; }
        public string Field { get; set; }
        public string Update { get; set; }
    }
    
    public class FlightDataAddedArgs
    {
        public string Callsign { get; set; }
        public FlightDataModel FlightData { get; set; }
    }
    
    public class FlightDataRemovedArgs
    {
        public string Callsign { get; set; }
    }
}