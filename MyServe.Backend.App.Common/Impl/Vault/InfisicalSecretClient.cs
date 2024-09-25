using Infisical.Sdk;
using Microsoft.Extensions.Configuration;
using MyServe.Backend.Common.Abstract;
using Supabase.Interfaces;

namespace MyServe.Backend.Common.Impl;

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
        
        var serviceToken = Configuration["SecretServiceToken"];
        _projectId = Configuration["SecretClientProjectId"] ?? DefaultProjectId;
        
        if(string.IsNullOrWhiteSpace(serviceToken))
            throw new ApplicationException("Service Token is missing.");
        
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
        var secret = _client.GetSecret(getSecretOptions);

        return secret.SecretValue;
    }
}