using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Impl;
using MyServe.Backend.App.Application.Client;

namespace MyServe.Backend.App.Infrastructure.Client;

public class JwtAccessTokenClient(ISecretClient secretClient) : IAccessTokenClient
{
    
    private SigningCredentials? _signingProfile = null;
    private EncryptingCredentials? _encryptionProfile = null;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();
    private async Task Initialize()
    {
        List<Task<string>> vaultTasks = [secretClient.GetSecretAsync(VaultConstants.Jwt.SigningKey), secretClient.GetSecretAsync(VaultConstants.Jwt.EncryptionKey)];
        var vaultResponse = await Task.WhenAll(vaultTasks);
        var signingKey = vaultResponse[0];
        var encryptionKey = vaultResponse[1];
        
    // Validate keys
    if (string.IsNullOrEmpty(signingKey) || signingKey.Length != 32)
        throw new ApplicationException("Signing key must be at least 256 bits (32 characters)");

    if (string.IsNullOrEmpty(encryptionKey) || encryptionKey.Length != 32)
        throw new ApplicationException("Encryption key must be at least 256 bits (32 characters)");
    
        _signingProfile = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            SecurityAlgorithms.HmacSha256);
        
        _encryptionProfile = new EncryptingCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encryptionKey)),
            SecurityAlgorithms.Aes256KW,
            SecurityAlgorithms.Aes128CbcHmacSha256);
    }
    
    public async Task<string> CreateTokenAsync(TokenGenerationOption options)
    {
        if (_signingProfile == null || _encryptionProfile == null)
            await Initialize();

        List<Claim> claims =
        [
            new(ClaimTypes.Email, options.Email),
            new(ClaimTypes.NameIdentifier, options.UserId.ToString()),
            new("device", options.Device ?? "WebApp"),
        ];
        var identity = new ClaimsIdentity(claims);
        var securityTokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = identity,
            Expires = DateTime.UtcNow.AddMinutes(options.TokenLifetime.Minutes),
            SigningCredentials = _signingProfile!,
            Issuer = TokenGenerationOption.JwtIssuer,
            Audience = TokenGenerationOption.JwtAudience,
            EncryptingCredentials = _encryptionProfile
        };

        var securityToken = _tokenHandler.CreateToken(securityTokenDescriptor);
        return _tokenHandler.WriteToken(securityToken);
    }
}