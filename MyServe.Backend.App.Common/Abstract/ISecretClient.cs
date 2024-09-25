using Microsoft.Extensions.Configuration;

namespace MyServe.Backend.Common.Abstract;

public interface ISecretClient
{
    protected IConfiguration Configuration { get; }

    /**
     * Initialize the client
     * This method is pure, and will only initialize once
     */
    public Task InitializeAsync();
    
    /**
     * Gets the value of the requested secret
     */
    public Task<string> GetSecretAsync(string secretName);

}