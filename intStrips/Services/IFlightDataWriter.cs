using System;

namespace intStrips.Services
{
    public interface IFlightDataWriter: IDisposable
    {
        void SetFlightDataField(object sender, string callsign, string field, string value);
    }
}