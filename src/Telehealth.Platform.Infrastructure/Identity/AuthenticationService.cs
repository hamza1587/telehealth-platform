using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telehealth.Platform.Application.Abstractions.ClinicalRecords;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Domain.Identity;
using Telehealth.Platform.Infrastructure.Persistence;

namespace Telehealth.Platform.Infrastructure.Identity;

/// <summary>
/// Implementation of the authentication service with comprehensive security features.
/// </summary>
public sealed class AuthenticationService : IAuthenticationService
{
    private readonly PlatformDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMfaService _mfaService;
    private readonly ILoginAttemptLogger _loginAttemptLogger;
    private readonly IClinicalRecordGateway _clinicalRecordGateway;
    private readonly IOptions<JwtSettings> _jwtOptions;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        PlatformDbContext dbContext,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMfaService mfaService,
        ILoginAttemptLogger loginAttemptLogger,
        IClinicalRecordGateway clinicalRecordGateway,
        IOptions<JwtSettings> jwtOptions,
        ILogger<AuthenticationService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mfaService = mfaService;
        _loginAttemptLogger = loginAttemptLogger;
        _clinicalRecordGateway = clinicalRecordGateway;
        _jwtOptions = jwtOptions;
        _logger = logger;
    }

    private JwtSettings JwtSettings => _jwtOptions.Value;

    public async Task<AuthenticationResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        var existingUser = await _dbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.NormalizedEmail == request.Email.ToUpperInvariant(), cancellationToken);

        if (existingUser != null)
        {
            return new AuthenticationResult
            {
                Success = false,
                Error = "Email already registered",
                ErrorCode = "EMAIL_EXISTS"
            };
        }

        // Validate password strength
        var passwordValidation = ValidatePasswordStrength(request.Password);
        if (!passwordValidation.IsValid)
        {
            return new AuthenticationResult
            {
                Success = false,
                Error = passwordValidation.Error,
                ErrorCode = "WEAK_PASSWORD"
            };
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user
        var user = new PlatformUser(
            id: Guid.NewGuid(),
            email: request.Email,
            passwordHash: passwordHash,
            userType: request.UserType,
            phoneNumber: request.PhoneNumber,
            firstName: request.FirstName,
            lastName: request.LastName,
            displayName: $"{request.FirstName} {request.LastName}".Trim(),
            countryCode: request.CountryCode,
            preferredLanguage: request.PreferredLanguage);

        // Assign default role based on user type
        var defaultRole = GetDefaultRoleForUserType(request.UserType);
        var role = await _dbContext.Roles
            .FirstOrDefaultAsync(r => r.NormalizedName == defaultRole.ToUpperInvariant(), cancellationToken);

        if (role != null)
        {
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                AssignedAt = DateTimeOffset.UtcNow
            });
        }

        // Generate email verification token
        var emailToken = _tokenService.GenerateEmailVerificationToken(user.Id);
        user.SetEmailVerificationToken(emailToken, DateTimeOffset.UtcNow.AddHours(24));

        _dbContext.PlatformUsers.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Create user in Medplum (FHIR resource)
        try
        {
            if (request.UserType == UserType.Patient)
            {
                var medplumResult = await _clinicalRecordGateway.CreatePatientAsync(user, cancellationToken);
                if (medplumResult.Success)
                {
                    _logger.LogInformation("Created Patient in Medplum with ID: {MedplumId}", medplumResult.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to create Patient in Medplum: {Error}", medplumResult.Error);
                }
            }
            else if (request.UserType == UserType.Doctor)
            {
                var medplumResult = await _clinicalRecordGateway.CreatePractitionerAsync(user, cancellationToken);
                if (medplumResult.Success)
                {
                    _logger.LogInformation("Created Practitioner in Medplum with ID: {MedplumId}", medplumResult.Data);
                }
                else
                {
                    _logger.LogWarning("Failed to create Practitioner in Medplum: {Error}", medplumResult.Error);
                }
            }
        }
        catch (Exception ex)
        {
            // Log but don't fail registration if Medplum is unavailable
            _logger.LogError(ex, "Exception creating user in Medplum");
        }

        return new AuthenticationResult
        {
            Success = true,
            UserId = user.Id,
            Email = user.Email,
            UserType = user.UserType,
            EmailConfirmed = false,
            MfaRequired = false,
            Roles = new[] { defaultRole }
        };
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == request.Email.ToUpperInvariant(), cancellationToken);

        // Failed login - user not found
        if (user == null)
        {
            await _loginAttemptLogger.LogFailedAttemptAsync(
                null, request.Email, request.IpAddress, request.UserAgent, request.DeviceId,
                LoginAttemptResult.UserNotFound, "User not found");

            return new AuthenticationResult
            {
                Success = false,
                Error = "Invalid credentials",
                ErrorCode = "INVALID_CREDENTIALS"
            };
        }

        // Check if account is locked
        if (user.IsLockedOut())
        {
            await _loginAttemptLogger.LogFailedAttemptAsync(
                user.Id.ToString(), user.Email, request.IpAddress, request.UserAgent, request.DeviceId,
                LoginAttemptResult.AccountLocked, "Account is locked");

            return new AuthenticationResult
            {
                Success = false,
                Error = "Account is locked. Please try again later.",
                ErrorCode = "ACCOUNT_LOCKED"
            };
        }

        // Check if account is disabled
        if (user.Status == UserStatus.Suspended || user.Status == UserStatus.Deactivated)
        {
            await _loginAttemptLogger.LogFailedAttemptAsync(
                user.Id.ToString(), user.Email, request.IpAddress, request.UserAgent, request.DeviceId,
                LoginAttemptResult.AccountDisabled, $"Account status: {user.Status}");

            return new AuthenticationResult
            {
                Success = false,
                Error = "Account is not active",
                ErrorCode = "ACCOUNT_DISABLED"
            };
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin();

            // Lock account after 5 failed attempts
            if (user.AccessFailedCount >= 5)
            {
                user.Lockout(TimeSpan.FromMinutes(30));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loginAttemptLogger.LogFailedAttemptAsync(
                user.Id.ToString(), user.Email, request.IpAddress, request.UserAgent, request.DeviceId,
                LoginAttemptResult.InvalidCredentials, "Invalid password");

            return new AuthenticationResult
            {
                Success = false,
                Error = "Invalid credentials",
                ErrorCode = "INVALID_CREDENTIALS"
            };
        }

        // Check if MFA is required
        if (user.TwoFactorEnabled)
        {
            var mfaToken = _tokenService.GenerateMfaToken(user.Id, user.Email);

            await _loginAttemptLogger.LogMfaRequiredAsync(
                user.Id.ToString(), user.Email, request.IpAddress, request.UserAgent, request.DeviceId);

            return new AuthenticationResult
            {
                Success = true,
                MfaRequired = true,
                MfaToken = mfaToken,
                UserId = user.Id,
                Email = user.Email,
                UserType = user.UserType
            };
        }

        // Complete login
        return await CompleteLoginAsync(user, request.DeviceId, request.DeviceName, request.UserAgent, request.IpAddress, cancellationToken);
    }

    public async Task<AuthenticationResult> VerifyMfaAndCompleteLoginAsync(MfaVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var mfaResult = _tokenService.ValidateMfaToken(request.MfaToken);

        if (!mfaResult.IsValid)
        {
            return new AuthenticationResult
            {
                Success = false,
                Error = "Invalid or expired MFA token",
                ErrorCode = "INVALID_MFA_TOKEN"
            };
        }

        var user = await _dbContext.PlatformUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == mfaResult.UserId, cancellationToken);

        if (user == null)
        {
            return new AuthenticationResult
            {
                Success = false,
                Error = "User not found",
                ErrorCode = "USER_NOT_FOUND"
            };
        }

        // Validate TOTP code
        if (!_mfaService.ValidateCode(user.TwoFactorSecret!, request.Code))
        {
            // Check if it's a recovery code
            if (user.TwoFactorRecoveryCodes != null && _mfaService.ValidateRecoveryCode(user.TwoFactorRecoveryCodes, request.Code))
            {
                // Remove used recovery code
                var updatedCodes = _mfaService.RemoveUsedRecoveryCode(user.TwoFactorRecoveryCodes, request.Code);
                user.DisableTwoFactor(); // Require re-setup after recovery code use
                user.TwoFactorRecoveryCodes = updatedCodes;
            }
            else
            {
                await _loginAttemptLogger.LogFailedAttemptAsync(
                    user.Id.ToString(), user.Email, request.IpAddress, request.UserAgent, request.DeviceId,
                    LoginAttemptResult.MfaFailed, "Invalid MFA code");

                return new AuthenticationResult
                {
                    Success = false,
                    Error = "Invalid MFA code",
                    ErrorCode = "INVALID_MFA_CODE"
                };
            }
        }

        return await CompleteLoginAsync(user, request.DeviceId, request.DeviceName, request.UserAgent, request.IpAddress, cancellationToken);
    }

    private async Task<AuthenticationResult> CompleteLoginAsync(
        PlatformUser user,
        string? deviceId,
        string? deviceName,
        string? userAgent,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        // Get roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);

        // Generate tokens
        var tokenData = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        // Store refresh token hash
        var refreshTokenHash = HashToken(refreshToken);
        var refreshTokenEntity = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            refreshTokenHash,
            deviceId ?? Guid.NewGuid().ToString("N"),
            deviceName,
            ipAddress,
            userAgent,
            DateTimeOffset.UtcNow.AddDays(JwtSettings.RefreshTokenExpirationDays));

        _dbContext.RefreshTokens.Add(refreshTokenEntity);

        // Update device authorization
        if (!string.IsNullOrEmpty(deviceId))
        {
            var device = await _dbContext.DeviceAuthorizations
                .FirstOrDefaultAsync(d => d.UserId == user.Id && d.DeviceId == deviceId, cancellationToken);

            if (device != null)
            {
                device.UpdateLastSeen(ipAddress ?? "unknown", userAgent);
            }
            else
            {
                _dbContext.DeviceAuthorizations.Add(new DeviceAuthorization(
                    Guid.NewGuid(),
                    user.Id,
                    deviceId,
                    deviceName,
                    null,
                    null,
                    null,
                    ipAddress ?? "unknown",
                    userAgent));
            }
        }

        // Update user login info
        user.RecordLogin(ipAddress ?? "unknown");

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loginAttemptLogger.LogSuccessfulAttemptAsync(
            user.Id.ToString(), user.Email, ipAddress ?? "unknown", userAgent, deviceId,
            user.TwoFactorEnabled ? "TOTP" : null);

        return new AuthenticationResult
        {
            Success = true,
            UserId = user.Id,
            Email = user.Email,
            UserType = user.UserType,
            Roles = roles.ToArray(),
            AccessToken = tokenData.AccessToken,
            RefreshToken = refreshToken,
            ExpiresAt = tokenData.ExpiresAt,
            EmailConfirmed = user.EmailConfirmed,
            PhoneConfirmed = user.PhoneNumberConfirmed,
            MfaRequired = false
        };
    }

    public async Task<TokenResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .Include(t => t.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken == null || !storedToken.IsValid())
        {
            return new TokenResult
            {
                Success = false,
                Error = "Invalid or expired refresh token"
            };
        }

        var user = storedToken.User;

        // Mark old token as used
        storedToken.MarkAsUsed();

        // Generate new tokens
        var permissions = await GetUserPermissionsAsync(user.Id, cancellationToken);
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();

        var tokenData = _tokenService.GenerateAccessToken(user, roles, permissions);
        var newRefreshToken = _tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = HashToken(newRefreshToken);

        // Store new refresh token
        var newTokenEntity = new RefreshToken(
            Guid.NewGuid(),
            user.Id,
            newRefreshTokenHash,
            request.DeviceId ?? storedToken.DeviceId,
            storedToken.DeviceName,
            request.IpAddress ?? storedToken.IpAddress,
            storedToken.UserAgent,
            DateTimeOffset.UtcNow.AddDays(JwtSettings.RefreshTokenExpirationDays));

        storedToken.ReplaceWith(newTokenEntity.Id.ToString());
        _dbContext.RefreshTokens.Add(newTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new TokenResult
        {
            Success = true,
            AccessToken = tokenData.AccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = tokenData.ExpiresAt
        };
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(refreshToken);

        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (storedToken != null)
        {
            storedToken.Revoke("User requested logout");
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task LogoutAsync(Guid userId, string? deviceId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.RefreshTokens.Where(t => t.UserId == userId && !t.IsRevoked);

        if (!string.IsNullOrEmpty(deviceId))
        {
            query = query.Where(t => t.DeviceId == deviceId);
        }

        var tokens = await query.ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke("User logout");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PasswordResetResult> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant(), cancellationToken);

        if (user == null)
        {
            // Return success even if user not found (security best practice)
            return new PasswordResetResult
            {
                Success = true,
                Message = "If the email exists, a password reset link has been sent."
            };
        }

        var token = _tokenService.GeneratePasswordResetToken(user.Id);

        // In a real implementation, send email here
        // await _emailService.SendPasswordResetAsync(user.Email, token);

        return new PasswordResetResult
        {
            Success = true,
            Message = "If the email exists, a password reset link has been sent."
        };
    }

    public async Task<PasswordResetResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = _tokenService.ValidatePasswordResetToken(request.Token);

        if (!validationResult.IsValid)
        {
            return new PasswordResetResult
            {
                Success = false,
                Error = "Invalid or expired token"
            };
        }

        var passwordValidation = ValidatePasswordStrength(request.NewPassword);
        if (!passwordValidation.IsValid)
        {
            return new PasswordResetResult
            {
                Success = false,
                Error = passwordValidation.Error
            };
        }

        var user = await _dbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.Id == validationResult.UserId, cancellationToken);

        if (user == null)
        {
            return new PasswordResetResult
            {
                Success = false,
                Error = "User not found"
            };
        }

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.ChangePassword(newPasswordHash);

        // Revoke all refresh tokens
        var tokens = await _dbContext.RefreshTokens
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke("Password reset");
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new PasswordResetResult
        {
            Success = true,
            Message = "Password has been reset successfully"
        };
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new InvalidOperationException("Current password is incorrect");
        }

        var passwordValidation = ValidatePasswordStrength(request.NewPassword);
        if (!passwordValidation.IsValid)
        {
            throw new InvalidOperationException(passwordValidation.Error);
        }

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.ChangePassword(newPasswordHash);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<MfaSetupResult> SetupMfaAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return new MfaSetupResult { Success = false, Error = "User not found" };
        }

        if (user.TwoFactorEnabled)
        {
            return new MfaSetupResult { Success = false, Error = "MFA is already enabled" };
        }

        var secretKey = _mfaService.GenerateSecretKey();
        var qrCodeUri = _mfaService.GenerateQrCodeUri(secretKey, user.Email);
        var recoveryCodes = _mfaService.GenerateRecoveryCodes();

        // Store encrypted recovery codes temporarily - will be confirmed after verification
        return new MfaSetupResult
        {
            Success = true,
            SecretKey = secretKey,
            QrCodeUri = qrCodeUri,
            BackupCodes = recoveryCodes
        };
    }

    public async Task<bool> VerifyMfaSetupAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        // This would need to temporarily store the secret key between setup and verification
        // For production, use a temporary cache or database field
        // Implementation simplified for this example
        return true;
    }

    public async Task DisableMfaAsync(Guid userId, string password, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (!user.TwoFactorEnabled)
        {
            throw new InvalidOperationException("MFA is not enabled");
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            throw new InvalidOperationException("Password is incorrect");
        }

        user.DisableTwoFactor();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string[]> GenerateMfaRecoveryCodesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !user.TwoFactorEnabled)
        {
            return Array.Empty<string>();
        }

        var codes = _mfaService.GenerateRecoveryCodes();
        // Store hashed codes
        return codes;
    }

    public async Task<bool> VerifyEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        // Decode and validate token
        // For now, simplified implementation
        return true;
    }

    public async Task ResendVerificationEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpperInvariant(), cancellationToken);

        if (user != null && !user.EmailConfirmed)
        {
            var token = _tokenService.GenerateEmailVerificationToken(user.Id);
            user.SetEmailVerificationToken(token, DateTimeOffset.UtcNow.AddHours(24));
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Send email
        }
    }

    public async Task<bool> VerifyPhoneAsync(Guid userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return false;
        }

        if (user.PhoneVerificationCode != code)
        {
            return false;
        }

        if (user.PhoneVerificationCodeExpiresAt < DateTimeOffset.UtcNow)
        {
            return false;
        }

        user.ConfirmPhone();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<CurrentUserInfo?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return null;
        }

        return new CurrentUserInfo
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            UserType = user.UserType,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray(),
            TwoFactorEnabled = user.TwoFactorEnabled,
            EmailConfirmed = user.EmailConfirmed,
            PhoneConfirmed = user.PhoneNumberConfirmed,
            CountryCode = user.CountryCode,
            PreferredLanguage = user.PreferredLanguage,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string role, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user?.UserRoles.Any(ur => ur.Role.NormalizedName == role.ToUpperInvariant()) ?? false;
    }

    public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken cancellationToken = default)
    {
        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
        return permissions.Contains(permission);
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.PlatformUsers
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            return Array.Empty<string>();
        }

        return user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Name)
            .Distinct()
            .ToList();
    }

    private static string GetDefaultRoleForUserType(UserType userType)
    {
        return userType switch
        {
            UserType.Patient => Roles.Patient,
            UserType.Doctor => Roles.Doctor,
            UserType.Admin => Roles.Admin,
            UserType.SupportAgent => Roles.SupportAgent,
            UserType.ComplianceOfficer => Roles.ComplianceOfficer,
            UserType.FinanceOperator => Roles.FinanceOperator,
            UserType.ResearchReviewer => Roles.ResearchReviewer,
            _ => Roles.Patient
        };
    }

    private static PasswordValidationResult ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
        {
            return new PasswordValidationResult(false, "Password must be at least 12 characters long");
        }

        if (!password.Any(char.IsUpper))
        {
            return new PasswordValidationResult(false, "Password must contain at least one uppercase letter");
        }

        if (!password.Any(char.IsLower))
        {
            return new PasswordValidationResult(false, "Password must contain at least one lowercase letter");
        }

        if (!password.Any(char.IsDigit))
        {
            return new PasswordValidationResult(false, "Password must contain at least one digit");
        }

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
        {
            return new PasswordValidationResult(false, "Password must contain at least one special character");
        }

        return new PasswordValidationResult(true, null);
    }

    private static string HashToken(string token)
    {
        // Use SHA-256 to hash the token for storage
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private record PasswordValidationResult(bool IsValid, string? Error);
}

/// <summary>
/// Service for logging login attempts for audit purposes.
/// </summary>
public interface ILoginAttemptLogger
{
    Task LogSuccessfulAttemptAsync(string userId, string email, string? ipAddress, string? userAgent, string? deviceId, string? mfaMethod);
    Task LogFailedAttemptAsync(string? userId, string? email, string? ipAddress, string? userAgent, string? deviceId, LoginAttemptResult result, string reason);
    Task LogMfaRequiredAsync(string userId, string email, string? ipAddress, string? userAgent, string? deviceId);
}
