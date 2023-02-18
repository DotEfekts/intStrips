using Grpc.Core;

namespace intStripsServer.Services;

public class UpdateStreamHandler
{
    private readonly List<IServerStreamWriter<FlightUpdateReply>> _outStreams = new();

    public void AddStream(IServerStreamWriter<FlightUpdateReply> stream)
    {
        _outStreams.Add(stream);
    }
    
    public void RemoveStream(IServerStreamWriter<FlightUpdateReply> stream)
    {
        _outStreams.Remove(stream);
    }

    public async Task SendUpdate(FlightUpdateReply update)
    {
        var toRemove = new List<IServerStreamWriter<FlightUpdateReply>>();
        foreach (var stream in _outStreams)
            try
            {
                await stream.WriteAsync(update);
            }
            catch (Exception)
            {
                toRemove.Add(stream);
            }

        foreach (var stream in toRemove)
            _outStreams.Remove(stream);
    }
}