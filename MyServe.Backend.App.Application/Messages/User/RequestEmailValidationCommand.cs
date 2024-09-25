namespace MyServe.Backend.App.Application.Messages.User;

public class RequestEmailValidationCommand
{
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Code { get; set; }
    public string Device { get; set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow; 
}