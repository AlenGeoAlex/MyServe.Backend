namespace MyServe.Backend.Common.Options.OAuth;

public record GoogleOAuthOptions(
    string ClientId,
    string ClientSecret,
    string Issuer = "https://accounts.google.com"
)
{
    public static GoogleOAuthOptions Empty => new GoogleOAuthOptions(string.Empty, string.Empty, string.Empty);

    public virtual bool Equals(GoogleOAuthOptions? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return ClientId == other.ClientId && ClientSecret == other.ClientSecret && Issuer == other.Issuer;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ClientId, ClientSecret, Issuer);
    }
}