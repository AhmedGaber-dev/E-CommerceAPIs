using System.ComponentModel.DataAnnotations;

namespace ECommerce.Application.Options;

/// <summary>
/// JWT settings bound from configuration (override secrets with environment variables or a vault in production).
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(1)]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public string Audience { get; set; } = string.Empty;

    /// <summary>Symmetric signing key for HS256. Use a cryptographically random string; minimum 256 bits recommended for HS256.</summary>
    [Required]
    [MinLength(32, ErrorMessage = "Jwt:Key must be at least 32 characters.")]
    public string Key { get; set; } = string.Empty;

    [Range(5, 1440)]
    public int AccessTokenMinutes { get; set; } = 60;
}
