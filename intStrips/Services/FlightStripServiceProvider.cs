using intStrips.Models;

namespace intStrips.Services
{
    public static class FlightStripServiceProvider
    {
        public static IFlightStripService Service { get; }

        static FlightStripServiceProvider()
        {
            var vatSysConnector = VatSysConnector.Instance;
            var intStripsConnector = IntStripsConnector.Instance;
            
            var vatSysInStripsConnector = new VatSysIntStripsServerDataReader(vatSysConnector, intStripsConnector);
            var vatSysInfoService = new VatSysControlInfoService(vatSysConnector);
            //var mockDataWriter = new MockFlightDataService();
            //var mockInfoService = new MockControlInfoService();
            
            Service = new FlightStripService(vatSysInStripsConnector, vatSysInStripsConnector, vatSysInfoService);
        }
    }
}