using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;
using intStrips.Models;
using intStripsServer;

namespace intStrips.Services
{
    public class IntStripsConnector : IDisposable
    {
        public static IntStripsConnector Instance { get; }

        private bool _disposed;
        private readonly GrpcChannel _grpcChannel;
        private readonly FlightData.FlightDataClient _grpcClient;

        static IntStripsConnector()
        {
            Instance = new IntStripsConnector();
        }

        private IntStripsConnector()
        {
            _grpcChannel = GrpcChannel.ForAddress("https://localhost:7108", new GrpcChannelOptions
            {
                HttpHandler = new GrpcWebHandler(new HttpClientHandler())
            });
            _grpcClient = new FlightData.FlightDataClient(_grpcChannel);

            Task.Run(async () => await StartClientListener());
        }

        private async Task StartClientListener()
        {
            var socket = _grpcClient.OpenUpdateSocket(new Empty());
            while (!_disposed && await socket.ResponseStream.MoveNext())
            {
                var update = socket.ResponseStream.Current;
                FlightDataChanged?.Invoke(this, new FlightDataChangedArgs()
                {
                    Callsign = update.Callsign,
                    Field = update.Field,
                    Update = update.Value
                });
            }
        }
        
        public event EventHandler<FlightDataChangedArgs> FlightDataChanged;

        public FlightInfo[] SendFdrListRequest(IEnumerable<string> callsigns)
        {
            var data = _grpcClient.GetDataForFlights(new FlightInfoRequest()
            {
                Callsign = { callsigns }
            });

           return data.Flights.ToArray();
        }

        public void SendUpdateRequest(FlightUpdateRequest request)
        {
            var data = _grpcClient.SendFlightUpdate(request);
            FlightDataChanged?.Invoke(this, new FlightDataChangedArgs()
            {
                Callsign = data.Callsign,
                Field = data.Field,
                Update = data.Value
            });
        }

        public void Dispose()
        {
            _disposed = true;
            _grpcChannel?.Dispose();
        }
    }
}