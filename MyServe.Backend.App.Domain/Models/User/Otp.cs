namespace MyServe.Backend.App.Domain.Models.User;

public class UserOtp
{
    public Guid Id { get; set; }
    public User? User { get; set; }
    public string Otp { get; set; }
    public string Device { get; set; }
    public DateTimeOffset Expiry { get; set; }
    public Guid UserId { get; set; }
}