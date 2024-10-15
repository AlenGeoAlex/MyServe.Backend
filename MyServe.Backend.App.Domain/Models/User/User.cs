namespace MyServe.Backend.App.Domain.Models.User;

public class User : IEntity
{
    public Guid Id { get; set; }
    public string EmailAddress { get; set; }
    public string Provide { get; set; }
    public bool IsLocked { get; set; }
    public DateTimeOffset? LastLogin { get; set; }
}