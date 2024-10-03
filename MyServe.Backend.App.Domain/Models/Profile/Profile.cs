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
    public ProfileSettings ProfileSettings { get; set; }
    public string EmailAddress { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string EncryptionKey { get; set; }

}