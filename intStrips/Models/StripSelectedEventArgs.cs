using System.Windows;

namespace intStrips.Models
{
    public class StripSelectedEventArgs
    {
        public string Callsign { get; set; }
        public FlightStripModel Strip { get; set; }
    }
}