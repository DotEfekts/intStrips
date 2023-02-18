using System;
using intStrips.Models;

namespace intStrips.Services
{
    public interface IFlightDataReader : IDisposable
    {
        void RequestAllFlightData();
        event EventHandler<FlightDataModel[]> FlightDataRefreshed;
        event EventHandler<FlightDataAddedArgs> FlightDataAdded;
        event EventHandler<FlightDataChangedArgs> FlightDataChanged;
        event EventHandler<FlightDataRemovedArgs> FlightDataRemoved;

    }
}