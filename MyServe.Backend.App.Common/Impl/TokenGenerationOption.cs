using System.Globalization;

namespace MyServe.Backend.Common.Impl;

public class TokenGenerationOption
{
    public const string JwtIssuer = "myserve.alenalex.me";
    public const string JwtAudience = $"{MyServConstants.Project.ProjectName}-default-audience";
    
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public TimeSpan TokenLifetime { get; set; } = TimeSpan.FromMinutes(5);
    public string? Device { get; set; } = "WebApp";
}