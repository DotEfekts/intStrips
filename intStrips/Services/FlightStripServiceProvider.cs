using intStrips.Models;

namespace intStrips.Services
{
    public static class FlightStripServiceProvider
    {
        public static IFlightStripService Service { get; }

        static FlightStripServiceProvider()
        {
            var mockData = new MockFlightDataService();
            Service = new FlightStripService(mockData, mockData, new MockControlInfoService());
        }
    }
}