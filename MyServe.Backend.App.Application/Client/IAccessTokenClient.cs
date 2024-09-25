using MyServe.Backend.Common.Impl;

namespace MyServe.Backend.App.Application.Client;

public interface IAccessTokenClient
{
    Task<string> CreateTokenAsync(TokenGenerationOption options);
}