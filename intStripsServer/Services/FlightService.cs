using System.Text.RegularExpressions;
using Google.Protobuf.Collections;
using Grpc.Core;
using intStripsServer;
using intStripsServer.Helpers;
using intStripsServer.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;

namespace intStripsServer.Services;

public class FlightService : FlightData.FlightDataBase
{
    private readonly UpdateStreamHandler _streamHandler;
    private readonly SqliteLogContext _logContext;
    private readonly IDistributedCache _cache;

    private static readonly string[] FlightStages = 
    {
        "CLEARANCE", "TAXI", "READY", "LINE_UP", "TAKEOFF", "AIRBORNE", "LANDED"
    };
    
    public FlightService(UpdateStreamHandler streamHandler, SqliteLogContext context, IDistributedCache cache)
    {
        _streamHandler = streamHandler;
        _logContext = context;
        _cache = cache;
    }

    public override async Task<FlightUpdateReply> SendFlightUpdate(FlightUpdateRequest request, ServerCallContext context)
    {
        var valid = false;
        string? property = null;
        string? value = null;

        request.Callsign = request.Callsign.ToUpper();
        request.Value = request.Value.ToUpper();

        _cache.TryGet<FlightInfo>(request.Callsign, out var currentValue);
        currentValue ??= new FlightInfo();
        currentValue.Callsign = request.Callsign;
        
        if (request.Field == nameof(FlightInfo.OriginGate))
        {
            valid = Regex.IsMatch(request.Value, "^[A-Z0-9]{0,5}$");
            property = nameof(FlightInfo.OriginGate);
            value = valid ? request.Value : currentValue.OriginGate;
            currentValue.OriginGate = value;
        }
        
        if (request.Field == nameof(FlightInfo.DestinationGate))
        {
            valid = Regex.IsMatch(request.Value, "^[A-Z0-9]{0,5}$");
            property = nameof(FlightInfo.DestinationGate);
            value = valid ? request.Value : currentValue.DestinationGate;
            currentValue.DestinationGate = value;
        }
        
        if (request.Field == nameof(FlightInfo.DepartureHoldingPoint))
        {
            valid = Regex.IsMatch(request.Value, "^[A-Z0-9]{0,3}$");
            property = nameof(FlightInfo.DepartureHoldingPoint);
            value = valid ? request.Value : currentValue.DepartureHoldingPoint;
            currentValue.DepartureHoldingPoint = value;
        }
        
        if (request.Field == nameof(FlightInfo.ArrivalHoldingPoint))
        {
            valid = Regex.IsMatch(request.Value, "^[A-Z0-9]{0,3}$");
            property = nameof(FlightInfo.ArrivalHoldingPoint);
            value = valid ? request.Value : currentValue.ArrivalHoldingPoint;
            currentValue.ArrivalHoldingPoint = value;
        }
        
        if (request.Field == nameof(FlightInfo.AssignedHeading))
        {
            valid = Regex.IsMatch(request.Value, "^[RL](0?[0-9]?[1-9]|0?[1-9][0-9]?|[1-2][0-9]{1,2}|3[0-5][0-9]|360)$");
            property = nameof(FlightInfo.AssignedHeading);
            value = valid ? request.Value : currentValue.AssignedHeading;
            currentValue.AssignedHeading = value;
        }
        
        if (request.Field == nameof(FlightInfo.FlightStage))
        {
            valid = FlightStages.Contains(request.Value);
            property = nameof(FlightInfo.FlightStage);
            value = valid ? request.Value : currentValue.FlightStage;
            currentValue.FlightStage = value;
        }
        
        if (request.Field == nameof(FlightInfo.AssignedFrequency))
        {
            valid = Regex.IsMatch(request.Value, "^1(1[89]|2[0-9]|3[0-6])\\.[0-9]{1,3}$");
            property = nameof(FlightInfo.AssignedFrequency);
            value = valid ? request.Value : currentValue.AssignedFrequency;
            currentValue.AssignedFrequency = value;
        }
        
        if (request.Field == nameof(FlightInfo.FlightRemarks))
        {
            valid = true;
            property = nameof(FlightInfo.FlightRemarks);
            value = request.Value;
            currentValue.FlightRemarks = value;
        }
        
        if (request.Field == nameof(FlightInfo.TowerRemarks))
        {
            valid = true;
            property = nameof(FlightInfo.TowerRemarks);
            value = request.Value;
            currentValue.TowerRemarks = value;
        }
        
        if (request.Field == nameof(FlightInfo.TmaRemarks))
        {
            valid = true;
            property = nameof(FlightInfo.TmaRemarks);
            value = request.Value;
            currentValue.TmaRemarks = value;
        }
        
        if (request.Field == nameof(FlightInfo.EnrouteRemarks))
        {
            valid = true;
            property = nameof(FlightInfo.EnrouteRemarks);
            value = request.Value;
            currentValue.EnrouteRemarks = value;
        }
        
        if(!valid || property == null) 
            return new FlightUpdateReply
            {
                Callsign = request.Callsign,
                Field = property,
                Value = value
            };
        
        await _cache.SetAsync(request.Callsign, currentValue);
        await _streamHandler.SendUpdate(new FlightUpdateReply()
        {
            Callsign = request.Callsign,
            Field = property,
            Value = value
        });

        _logContext.FieldUpdates.Add(new FieldUpdate()
        {
            Cid = "111111",
            Callsign = request.Callsign,
            Field = property,
            Update = value
        });
        await _logContext.SaveChangesAsync();
        
        return new FlightUpdateReply
        {
            Callsign = request.Callsign,
            Field = property,
            Value = value
        };
    }

    public override Task<FlightInfoReply> GetDataForFlights(FlightInfoRequest request, ServerCallContext context)
    {
        var flights = new List<FlightInfo>();

        foreach(var callsign in request.Callsign)
            if (_cache.TryGet<FlightInfo>(callsign, out var value))
                flights.Add(value!);
                    
        return Task.FromResult(new FlightInfoReply
        {
            Flights = { flights }
        });
    }

    public override async Task OpenUpdateSocket(Empty _, IServerStreamWriter<FlightUpdateReply> responseStream, ServerCallContext context)
    {
        _streamHandler.AddStream(responseStream);

        while (!context.CancellationToken.IsCancellationRequested)
        {
            await Task.Delay(100);
        }
        
        _streamHandler.RemoveStream(responseStream);
    }
}