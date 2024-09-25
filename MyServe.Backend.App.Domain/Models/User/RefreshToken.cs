namespace MyServe.Backend.App.Domain.Models.User;

public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset Expiry { get; set; }
    
}