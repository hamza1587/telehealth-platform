using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Identity;

public class DeviceManagementService : IDeviceManagementService
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<DeviceManagementService> _logger;

    public DeviceManagementService(
        PlatformDbContext dbContext,
        ILogger<DeviceManagementService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<DeviceRegistrationResult> RegisterDeviceAsync(
        PlatformUser user,
        string deviceId,
        string deviceType,
        string userAgent,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existingDevice = await _dbContext.UserDevices
                .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == user.Id, cancellationToken);

            if (existingDevice != null)
            {
                existingDevice.UpdateLastSeen(userAgent, ipAddress);
                await _dbContext.SaveChangesAsync(cancellationToken);
                return new DeviceRegistrationResult
                {
                    Success = true,
                    DeviceId = deviceId,
                    RequiresVerification = false
                };
            }

            var device = UserDevice.Create(
                userId: user.Id,
                deviceId: deviceId,
                deviceName: deviceType,
                deviceType: deviceType,
                userAgent: userAgent,
                ipAddress: ipAddress);

            _dbContext.UserDevices.Add(device);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Device registered for user {UserId}: {DeviceId}", user.Id, deviceId);

            return new DeviceRegistrationResult
            {
                Success = true,
                DeviceId = deviceId,
                RequiresVerification = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device for user {UserId}", user.Id);
            return new DeviceRegistrationResult
            {
                Success = false,
                ErrorMessage = "Failed to register device"
            };
        }
    }

    public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserDevices
            .Where(d => d.UserId == userId && d.IsBlocked == false)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsDeviceTrustedAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var device = await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId, cancellationToken);

        return device?.IsTrusted == true && device.IsBlocked == false;
    }

    public async Task<bool> BlockDeviceAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var device = await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId, cancellationToken);

        if (device == null)
            return false;

        device.Block("User requested or admin action");
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnblockDeviceAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var device = await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId, cancellationToken);

        if (device == null)
            return false;

        device.Unblock();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveDeviceAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var device = await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId, cancellationToken);

        if (device == null)
            return false;

        _dbContext.UserDevices.Remove(device);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UpdateDeviceLastSeenAsync(
        string deviceId,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var device = await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken);

        if (device == null)
            return false;

        var userAgent = string.Empty;
        device.UpdateLastSeen(userAgent, ipAddress);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> TrustDeviceAsync(
        Guid userId,
        string deviceId,
        CancellationToken cancellationToken = default)
    {
        var device = await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId && d.UserId == userId, cancellationToken);

        if (device == null)
            return false;

        device.SetTrusted(true);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}