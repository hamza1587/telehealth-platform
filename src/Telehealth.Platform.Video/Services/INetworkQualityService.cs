namespace Telehealth.Platform.Video.Services;

public interface INetworkQualityService
{
    Task<NetworkQualityMetrics> GetQualityMetricsAsync(Guid roomId);
    Task RecordQualityMetricsAsync(Guid roomId, NetworkQualityMetrics metrics);
}

public record NetworkQualityMetrics(
    double VideoLatencyMs,
    double AudioLatencyMs,
    double PacketLossPercentage,
    double JitterMs,
    int VideoBitrateKbps,
    int AudioBitrateKbps,
    int VideoResolutionWidth,
    int VideoResolutionHeight,
    int FrameRate,
    DateTimeOffset Timestamp
);
