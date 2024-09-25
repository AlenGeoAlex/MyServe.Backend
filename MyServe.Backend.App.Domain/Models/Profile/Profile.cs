using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace MyServe.Backend.App.Domain.Models.Profile;

public class Profile
{
    public Profile()
    {
    }

    public Guid Id { get; set;  }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public ProfileSettings Settings { get; set; }
    public string EmailAddress { get; set; }
    public string? ProfileImageUrl { get; set; }
}