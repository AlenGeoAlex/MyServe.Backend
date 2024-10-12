using Microsoft.Extensions.Configuration;
using MyServe.Backend.Common.Abstract;
using Serilog;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;

namespace MyServe.Backend.Common.Impl.Vault;

public class HashiCorpSecretClient(IConfiguration configuration) : ISecretClient
{
    public IConfiguration Configuration { get; } = configuration;
    
    private VaultClient? _vaultClient = null;
    private string _secretPath = null!;
    private string _mountPoint = null!;
    public async Task InitializeAsync()
    {
        if (_vaultClient != null)
            return;
        
        var address = Configuration["Vault:Address"] ?? throw new ApplicationException("Vault address not found");
        var secretToken = Configuration["Vault:SecretServiceToken"] ?? throw new ApplicationException("Vault secret service token not found");
        _secretPath = Configuration["Vault:SecretClientProjectId"] ?? throw new ApplicationException("Vault secret client project id not found");
        _mountPoint = Configuration["Vault:MountPoint"] ?? throw new ApplicationException("Vault secret mount point not found");

        Log.Logger.Information("Initializing hashicorp on "+address);
        _vaultClient = new VaultClient(new VaultClientSettings(address, new TokenAuthMethodInfo(secretToken)));
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        var secret = await _vaultClient!.V1.Secrets.KeyValue.V2.ReadSecretAsync(_secretPath, null, _mountPoint);
    
        if (secret == null || secret.Data == null || secret.Data.Data == null)
            throw new ApplicationException($"Failed to fetch secret '{secretName}' from Vault.");
        
        return secret.Data.Data[secretName].ToString() ?? throw new ApplicationException($"Failed to fetch secret '{secretName}'."); 
    }


}