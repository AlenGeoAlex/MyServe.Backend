using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Domain.Models.Profile;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Abstract;
using MyServe.Backend.App.Infrastructure.Database.NpgSql;
using Npgsql;

namespace MyServe.Backend.App.Infrastructure.Repositories;

public class ProfileRepository([FromKeyedServices("read-only-connection")]NpgsqlConnection readOnlyConnection, [FromKeyedServices("read-write-connection")] NpgsqlConnection readWriteDatabase) : AbstractRepository<Profile>(readOnlyConnection, readWriteDatabase), IProfileRepository
{
    public override async Task<Profile?> GetByIdAsync(Guid id)
    {
        var profileList = await readOnlyConnection.QueryAsync<Profile?>(ProfileSql.SelectById, new { Id = id });
        return profileList.FirstOrDefault();
    }

    public override async Task<Profile> AddAsync(Profile entity)
    {
        await readWriteDatabase.ExecuteAsync(ProfileSql.Add, new
        {
            entity.Id,
            entity.FirstName,
            entity.LastName,
            Settings = new JsonBParameter<ProfileSettings>(entity.Settings),
            entity.CreatedAt,
            Email = entity.EmailAddress
        });

        return entity;
    }

    public override Task UpdateAsync(Profile entity)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteAsync(Profile entity)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ExistsAsync(string emailAddress)
    {
        var emailList = await readOnlyConnection.QueryAsync(ProfileSql.Exists, new { Email = emailAddress });
        return emailList.Any();
    }

    private static class ProfileSql
    {
        public const string SelectById = """
                                         SELECT * FROM profile p join "user" u on p.id = u.id WHERE p.id = @Id;
                                         """;
       public const string Add = "INSERT INTO public.profile (id, first_name, last_name, settings, created_at, email) VALUES (@Id, @FirstName, @LastName, @Settings, @CreatedAt, @Email)";
       public const string Exists = "SELECT 1 FROM profile WHERE email = @Email LIMIT 1";
    }
}