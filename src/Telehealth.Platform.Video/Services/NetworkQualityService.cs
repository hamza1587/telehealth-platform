namespace Telehealth.Platform.Video.Services;

public class NetworkQualityService : INetworkQualityService
{
    private readonly Dictionary<Guid, List<NetworkQualityMetrics>> _roomMetrics;

    public NetworkQualityService()
    {
        _roomMetrics = new Dictionary<Guid, List<NetworkQualityMetrics>>();
    }

    public async Task<NetworkQualityMetrics> GetQualityMetricsAsync(Guid roomId)
    {
        if (!_roomMetrics.ContainsKey(roomId) || _roomMetrics[roomId].Count == 0)
        {
            return new NetworkQualityMetrics(
                0, 0, 0, 0, 0, 0, 0, 0, 0, DateTimeOffset.UtcNow
            );
        }

        var metrics = _roomMetrics[roomId];
        var latest = metrics.Last();
        return await Task.FromResult(latest);
    }

    public async Task RecordQualityMetricsAsync(Guid roomId, NetworkQualityMetrics metrics)
    {
        if (!_roomMetrics.ContainsKey(roomId))
        {
            _roomMetrics[roomId] = new List<NetworkQualityMetrics>();
        }

        _roomMetrics[roomId].Add(metrics);

        // Keep only last 100 metrics per room
        if (_roomMetrics[roomId].Count > 100)
        {
            _roomMetrics[roomId].RemoveAt(0);
        }

        await Task.CompletedTask;
    }
}
