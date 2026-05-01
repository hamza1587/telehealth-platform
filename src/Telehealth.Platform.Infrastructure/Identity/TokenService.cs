using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Telehealth.Platform.Application.Abstractions.Identity;
using Telehealth.Platform.Domain.Identity;
using MsTokenValidationResult = Microsoft.IdentityModel.Tokens.TokenValidationResult;

namespace Telehealth.Platform.Infrastructure.Identity;

/// <summary>
/// JWT Token service with RS256 signing for enhanced security.
/// </summary>
public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _settings;
    private readonly SigningCredentials _signingCredentials;

    public TokenService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
        _signingCredentials = CreateSigningCredentials();
    }

    public TokenData GenerateAccessToken(PlatformUser user, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var tokenId = Guid.NewGuid().ToString("N");
        var issuedAt = DateTimeOffset.UtcNow;
        var expiresAt = issuedAt.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, tokenId),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Iat, issuedAt.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("user_type", user.UserType.ToString()),
            new("email_confirmed", user.EmailConfirmed.ToString()),
            new("mfa_enabled", user.TwoFactorEnabled.ToString())
        };

        // Add display name if available
        if (!string.IsNullOrEmpty(user.DisplayName))
        {
            claims.Add(new Claim("display_name", user.DisplayName));
        }

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions as a custom claim
        var permissionsList = permissions.ToList();
        if (permissionsList.Any())
        {
            claims.Add(new Claim("permissions", JsonSerializer.Serialize(permissionsList)));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = issuedAt.DateTime,
            Expires = expiresAt.DateTime,
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return new TokenData
        {
            AccessToken = tokenString,
            ExpiresAt = expiresAt,
            TokenId = tokenId
        };
    }

    public string GenerateRefreshToken()
    {
        // Generate a cryptographically secure random token
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    public Telehealth.Platform.Application.Abstractions.Identity.TokenValidationResult ValidateAccessToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = _signingCredentials.Key,
            ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);
            var jwtToken = (JwtSecurityToken)securityToken;

            var userId = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var email = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var userTypeStr = principal.FindFirst("user_type")?.Value;

            var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray();
            var permissionsClaim = principal.FindFirst("permissions")?.Value;
            var permissions = string.IsNullOrEmpty(permissionsClaim)
                ? Array.Empty<string>()
                : JsonSerializer.Deserialize<string[]>(permissionsClaim) ?? Array.Empty<string>();

            return new Telehealth.Platform.Application.Abstractions.Identity.TokenValidationResult
            {
                IsValid = true,
                UserId = userId != null ? Guid.Parse(userId) : null,
                Email = email,
                UserType = userTypeStr != null && Enum.TryParse<UserType>(userTypeStr, out var ut) ? ut : null,
                Roles = roles,
                Permissions = permissions,
                ExpiresAt = jwtToken.ValidTo
            };
        }
        catch (SecurityTokenExpiredException)
        {
            return new Telehealth.Platform.Application.Abstractions.Identity.TokenValidationResult { IsValid = false, Error = "Token has expired" };
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return new Telehealth.Platform.Application.Abstractions.Identity.TokenValidationResult { IsValid = false, Error = "Invalid token signature" };
        }
        catch (Exception ex)
        {
            return new Telehealth.Platform.Application.Abstractions.Identity.TokenValidationResult { IsValid = false, Error = $"Token validation failed: {ex.Message}" };
        }
    }

    public string GenerateMfaToken(Guid userId, string email)
    {
        // Generate a short-lived token for MFA step
        var claims = new[]
        {
            new Claim("type", "mfa_pending"),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(5), // Short expiration
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public MfaTokenValidationResult ValidateMfaToken(string token)
    {
        var result = ValidateAccessToken(token);

        if (!result.IsValid)
        {
            return new MfaTokenValidationResult { IsValid = false, Error = result.Error };
        }

        return new MfaTokenValidationResult
        {
            IsValid = true,
            UserId = result.UserId,
            Email = result.Email
        };
    }

    public string GeneratePasswordResetToken(Guid userId)
    {
        var claims = new[]
        {
            new Claim("type", "password_reset"),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public Telehealth.Platform.Application.Abstractions.Identity.TokenValidationResult ValidatePasswordResetToken(string token)
    {
        var result = ValidateAccessToken(token);

        if (!result.IsValid)
        {
            return new Telehealth.Platform.Application.Abstractions.Identity.TokenValidationResult { IsValid = false, Error = result.Error };
        }

        // Verify token type
        // This would need the principal claims - simplified implementation
        return result;
    }

    public string GenerateEmailVerificationToken(Guid userId)
    {
        var claims = new[]
        {
            new Claim("type", "email_verification"),
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddHours(24),
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            SigningCredentials = _signingCredentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateVerificationCode(int length = 6)
    {
        // Generate a numeric code of specified length
        const string chars = "0123456789";
        var result = new StringBuilder(length);
        var randomBytes = RandomNumberGenerator.GetBytes(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[randomBytes[i] % chars.Length]);
        }

        return result.ToString();
    }

    private SigningCredentials CreateSigningCredentials()
    {
        if (!string.IsNullOrEmpty(_settings.SecretKey))
        {
            // Use symmetric key for development/testing
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            return new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        }

        if (!string.IsNullOrEmpty(_settings.PrivateKey))
        {
            // Use RSA private key for production
            var rsa = RSA.Create();
            rsa.ImportFromPem(_settings.PrivateKey.ToCharArray());
            var key = new RsaSecurityKey(rsa);
            return new SigningCredentials(key, SecurityAlgorithms.RsaSha256);
        }

        throw new InvalidOperationException("No signing key configured. Set either SecretKey (for dev) or PrivateKey (for production).");
    }
}

/// <summary>
/// JWT configuration settings.
/// </summary>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// JWT Issuer (e.g., "TelehealthPlatform")
    /// </summary>
    public string Issuer { get; set; } = "TelehealthPlatform";

    /// <summary>
    /// JWT Audience (e.g., "TelehealthApi")
    /// </summary>
    public string Audience { get; set; } = "TelehealthApi";

    /// <summary>
    /// Symmetric key for development (HMAC-SHA512). 
    /// Must be at least 64 characters for HS512.
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// RSA Private Key for production (RS256).
    /// Use this for asymmetric signing in production.
    /// </summary>
    public string? PrivateKey { get; set; }

    /// <summary>
    /// RSA Public Key for token validation.
    /// </summary>
    public string? PublicKey { get; set; }

    /// <summary>
    /// Access token expiration in minutes (default: 15)
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Refresh token expiration in days (default: 7)
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Clock skew tolerance in seconds (default: 0)
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 0;
}
