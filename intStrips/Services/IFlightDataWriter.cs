namespace intStrips.Services
{
    public interface IFlightDataWriter
    {
        void SetFlightDataField(object sender, string callsign, string field, string value);
    }
}