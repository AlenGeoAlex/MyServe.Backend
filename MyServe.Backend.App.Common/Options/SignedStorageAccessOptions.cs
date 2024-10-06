namespace MyServe.Backend.Common.Options;

public class SignedStorageAccessOptions
{
    public const string Upload = "PUT";
    public const string Download = "GET";

    private readonly string _action = null!;
    public required string Action
    {
        get => _action;
        init
        {
            if(value != Upload && value != Download)
                throw new ArgumentException($"'{nameof(Action)}' must be of type '{Upload}' or '{Download}'. Please use the const variables of {nameof(SignedStorageAccessOptions)}.");

            _action = value;
        }
    }
    
    public required TimeSpan TimeToLive { get; init; }

    public DateTime Expiry => DateTime.UtcNow.Add(TimeToLive);
}