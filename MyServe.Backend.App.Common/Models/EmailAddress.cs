namespace MyServe.Backend.Common.Models;

public record EmailAddress(
    string Address,
    string Name
)
{
    public static EmailAddress Empty => new EmailAddress(string.Empty, string.Empty);
    
    public static EmailAddress Parse(string emailAddress) => new(emailAddress, string.Empty);
    
    public static EmailAddress Parse(string emailAddress, string name) => new(emailAddress, name);
}