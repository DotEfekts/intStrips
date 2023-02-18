using System;
using intStrips.Models;

namespace intStrips.Services
{
    public interface IFlightStripService : IDisposable
    {
        void RequestAllFlightData();
        event EventHandler<FlightStripsRefreshedArgs> FlightStripsRefreshed;
        event EventHandler<FlightStripChangedArgs> FlightStripChanged;
        event EventHandler<FlightStripRemovedArgs> FlightStripRemoved;
        void UpdateStripData(string callsign, string property, string value);
    }
}