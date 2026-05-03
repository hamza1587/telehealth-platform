using Microsoft.EntityFrameworkCore;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Identity;

public class UserDeviceService : IUserDeviceService
{
    private readonly PlatformDbContext _dbContext;

    public UserDeviceService(PlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserDevice?> GetDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserDevices
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceId == deviceId, cancellationToken);
    }

    public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserDevices
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserDevice> RegisterDeviceAsync(
        Guid userId,
        string deviceId,
        string deviceName,
        string deviceType,
        string userAgent,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var existingDevice = await GetDeviceAsync(userId, deviceId, cancellationToken);
        if (existingDevice != null)
        {
            existingDevice.UpdateLastSeen(userAgent, ipAddress);
            existingDevice.MarkAsUsed();
            _dbContext.UserDevices.Update(existingDevice);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return existingDevice;
        }

        var device = UserDevice.Create(userId, deviceId, deviceName, deviceType, userAgent, ipAddress);
        await _dbContext.UserDevices.AddAsync(device, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return device;
    }

    public async Task UpdateDeviceLastSeenAsync(Guid userId, string deviceId, string userAgent, string ipAddress, CancellationToken cancellationToken = default)
    {
        var device = await GetDeviceAsync(userId, deviceId, cancellationToken);
        if (device != null)
        {
            device.UpdateLastSeen(userAgent, ipAddress);
            device.MarkAsUsed();
            _dbContext.UserDevices.Update(device);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> TrustDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default)
    {
        var device = await GetDeviceAsync(userId, deviceId, cancellationToken);
        if (device == null) return false;

        device.SetTrusted(true);
        _dbContext.UserDevices.Update(device);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> BlockDeviceAsync(Guid userId, string deviceId, string reason, CancellationToken cancellationToken = default)
    {
        var device = await GetDeviceAsync(userId, deviceId, cancellationToken);
        if (device == null) return false;

        device.Block(reason);
        _dbContext.UserDevices.Update(device);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> UnblockDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default)
    {
        var device = await GetDeviceAsync(userId, deviceId, cancellationToken);
        if (device == null) return false;

        device.Unblock();
        _dbContext.UserDevices.Update(device);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default)
    {
        var device = await GetDeviceAsync(userId, deviceId, cancellationToken);
        if (device == null) return false;

        _dbContext.UserDevices.Remove(device);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> IsDeviceTrustedAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default)
    {
        var device = await GetDeviceAsync(userId, deviceId, cancellationToken);
        return device?.IsTrusted ?? false;
    }

    public async Task<string> GenerateDeviceFingerprintAsync(string deviceId, string userAgent, string ipAddress)
    {
        return $"{deviceId}|{userAgent}|{ipAddress}";
    }
}