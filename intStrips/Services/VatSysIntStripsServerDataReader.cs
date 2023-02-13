using System;
using intStrips.Models;

namespace intStrips.Services
{
    public class VatSysIntStripsServerDataReader : IFlightDataReader
    {
        public FlightDataModel[] GetAllFlightData()
        {
            throw new NotImplementedException();
        }

        public event EventHandler<FlightDataAddedArgs> FlightDataAdded;
        public event EventHandler<FlightDataChangedArgs> FlightDataChanged;
        public event EventHandler<FlightDataRemovedArgs> FlightDataRemoved;
    }
}