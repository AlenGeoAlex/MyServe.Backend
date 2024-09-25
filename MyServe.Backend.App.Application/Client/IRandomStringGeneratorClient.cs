namespace MyServe.Backend.App.Application.Client;

public interface IRandomStringGeneratorClient
{
    string Generate(int digit = 6);
}