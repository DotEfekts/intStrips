using System;
using intStrips.Models;

namespace intStrips.Services
{
    public interface IFlightDataReader
    {
        FlightDataModel[] GetAllFlightData();
        event EventHandler<FlightDataAddedArgs> FlightDataAdded;
        event EventHandler<FlightDataChangedArgs> FlightDataChanged;
        event EventHandler<FlightDataRemovedArgs> FlightDataRemoved;

    }
}