using System.Security.Claims;
using System.Text;
using ECommerce.Application.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Identity;

/// <summary>
/// Binds JWT validation parameters from <see cref="JwtOptions"/> so signing keys rotate with configuration reload.
/// </summary>
public sealed class ConfigureJwtBearerOptions : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly IOptionsMonitor<JwtOptions> _jwt;

    public ConfigureJwtBearerOptions(IOptionsMonitor<JwtOptions> jwt) => _jwt = jwt;

    public void Configure(string? name, JwtBearerOptions options)
    {
        if (!string.Equals(name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal))
            return;

        var jwt = _jwt.CurrentValue;
        options.SaveToken = false;
        // Symmetric-key JWT has no metadata endpoint; keep false so local HTTP dev works. Terminate TLS at the reverse proxy in production.
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ValidateTokenReplay = false,
            ClockSkew = TimeSpan.FromSeconds(60),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    }

    public void Configure(JwtBearerOptions options) => Configure(JwtBearerDefaults.AuthenticationScheme, options);
}
