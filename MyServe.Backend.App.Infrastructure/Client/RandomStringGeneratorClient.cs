using System.Text;
using MyServe.Backend.App.Application.Client;

namespace MyServe.Backend.App.Infrastructure.Client;

public class RandomStringGeneratorClient : IRandomStringGeneratorClient
{
    
    private static readonly char[] Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    private static readonly Random Random = new();
    
    public string Generate(int digit = 6)
    {
        if (digit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(digit), "The number of characters must be greater than zero.");
        }

        var stringBuilder = new StringBuilder(digit);
        
        for (int i = 0; i < digit; i++)
        {
            int index = Random.Next(Characters.Length);
            stringBuilder.Append(Characters[index]);
        }

        return stringBuilder.ToString();
    }
}