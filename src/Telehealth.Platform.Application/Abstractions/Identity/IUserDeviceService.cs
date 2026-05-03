using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Application.Abstractions.Identity;

public interface IUserDeviceService
{
    Task<UserDevice?> GetDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDevice> RegisterDeviceAsync(
        Guid userId,
        string deviceId,
        string deviceName,
        string deviceType,
        string userAgent,
        string ipAddress,
        CancellationToken cancellationToken = default);
    Task UpdateDeviceLastSeenAsync(Guid userId, string deviceId, string userAgent, string ipAddress, CancellationToken cancellationToken = default);
    Task<bool> TrustDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    Task<bool> BlockDeviceAsync(Guid userId, string deviceId, string reason, CancellationToken cancellationToken = default);
    Task<bool> UnblockDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    Task<bool> RemoveDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    Task<bool> IsDeviceTrustedAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);
    Task<string> GenerateDeviceFingerprintAsync(string deviceId, string userAgent, string ipAddress);
}