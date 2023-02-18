namespace intStrips.Models
{
    public class FlightStripsRefreshedArgs
    {
        public FlightStripModel[] Strips { get; set; }
    }
    
    public class FlightStripChangedArgs
    {
        public string Callsign { get; set; }
        public FlightStripModel Strip { get; set; }
    }
    
    public class FlightStripRemovedArgs
    {
        public string Callsign { get; set; }
    }
}