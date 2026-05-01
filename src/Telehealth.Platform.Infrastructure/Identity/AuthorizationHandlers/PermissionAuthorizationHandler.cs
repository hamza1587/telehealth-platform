using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Identity.AuthorizationHandlers;

/// <summary>
/// Authorization requirement for checking user permissions.
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Authorization handler that validates if the user has the required permission.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly PlatformDbContext _dbContext;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        PlatformDbContext dbContext,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? context.User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Permission check failed: No user ID found in claims");
            return;
        }

        if (!Guid.TryParse(userId, out var userGuid))
        {
            _logger.LogWarning("Permission check failed: Invalid user ID format");
            return;
        }

        // Check if user has the required permission through their roles
        var hasPermission = await _dbContext.PlatformUsers
            .Where(u => u.Id == userGuid)
            .SelectMany(u => u.UserRoles)
            .SelectMany(ur => ur.Role.RolePermissions)
            .AnyAsync(rp => rp.Permission.Name == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogInformation(
                "User {UserId} does not have permission {Permission}",
                userId,
                requirement.Permission);
        }
    }
}

/// <summary>
/// Extension methods for adding permission-based authorization.
/// </summary>
public static class AuthorizationPolicyExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(this AuthorizationPolicyBuilder builder, string permission)
    {
        return builder.AddRequirements(new PermissionRequirement(permission));
    }
}
