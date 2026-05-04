using Telehealth.Platform.Domain.Identity;

namespace Telehealth.Platform.Application.Abstractions.Identity;

public interface IDeviceManagementService
{
    Task<DeviceRegistrationResult> RegisterDeviceAsync(
        PlatformUser user,
        string deviceId,
        string deviceType,
        string userAgent,
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<UserDevice>> GetUserDevicesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> IsDeviceTrustedAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default);

    Task<bool> BlockDeviceAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default);

    Task<bool> UnblockDeviceAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveDeviceAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateDeviceLastSeenAsync(
        string deviceId,
        string ipAddress,
        CancellationToken cancellationToken = default);

    Task<bool> TrustDeviceAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default);
}

public class DeviceRegistrationResult
{
    public bool Success { get; set; }
    public string? DeviceId { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresVerification { get; set; }
}