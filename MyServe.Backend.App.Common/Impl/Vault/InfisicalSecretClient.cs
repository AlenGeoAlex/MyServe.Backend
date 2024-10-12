using Infisical.Sdk;
using Microsoft.Extensions.Configuration;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Exceptions;

namespace MyServe.Backend.Common.Impl.Vault;

public class InfisicalSecretClient(IConfiguration configuration) : ISecretClient
{
    /*
     * Default Infisical Project Id
     */
    private const string DefaultProjectId = "3742e3b1-cb0e-472c-b557-c11f9e518e58";
    public IConfiguration Configuration { get; } = configuration;
    
    private InfisicalClient? _client = null;
    private string _projectId = string.Empty;
    public async Task InitializeAsync()
    {
        if (_client != null)
            return;
        
        var serviceToken = Configuration["Vault:SecretServiceToken"];
        _projectId = Configuration["Vault:SecretClientProjectId"] ?? throw new ArgumentNullException($"ProjectId is missing");
        
        if(string.IsNullOrWhiteSpace(serviceToken) || string.IsNullOrWhiteSpace(_projectId))
            throw new ApplicationException("Service Token and ProjectId is missing is missing.");
        
        var settings = new ClientSettings
        {
            AccessToken = serviceToken
        };   
        
        this._client = new InfisicalClient(settings);
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        if(_client == null)
            throw new ApplicationException("SecretClient is not initialized.");
        
        var clientEnv = Configuration["Environment"];
        if(string.IsNullOrEmpty(clientEnv))
            throw new ApplicationException("Environment declaration is missing.");
        
        var getSecretOptions = new GetSecretOptions
        {
            SecretName = secretName,
            ProjectId = _projectId,
            Environment = clientEnv,
        };
        try
        {
            var secret = _client.GetSecret(getSecretOptions);
            return secret.SecretValue;
        }
        catch (Exception e)
        {
            throw new MissingSecretException(secretName, e);
        }
    }
}