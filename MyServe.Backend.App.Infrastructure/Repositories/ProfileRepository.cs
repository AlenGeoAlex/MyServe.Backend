using Dapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Domain.Extensions;
using MyServe.Backend.App.Domain.Models;
using MyServe.Backend.App.Domain.Models.Profile;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Abstract;
using MyServe.Backend.App.Infrastructure.Database.NpgSql;
using Newtonsoft.Json;
using Npgsql;
using File = MyServe.Backend.App.Domain.Models.Files.File;

namespace MyServe.Backend.App.Infrastructure.Repositories;

public class ProfileRepository([FromKeyedServices("read-only-connection")]NpgsqlConnection readOnlyConnection, [FromKeyedServices("read-write-connection")] NpgsqlConnection readWriteDatabase) : AbstractRepository<Profile>(readOnlyConnection, readWriteDatabase), IProfileRepository
{
    public override async Task<Profile?> GetByIdAsync(Guid id)
    {
        var profileList = await readOnlyConnection.QueryAsync(ProfileSql.SelectById, new { Id = id });
        return profileList.Select(x => new Profile()
        {
            Id = x.id,
            FirstName = x.first_name,
            LastName = x.last_name,
            ProfileImageUrl = x.profile_image,
            ProfileSettings = JsonConvert.DeserializeObject<ProfileSettings>(x.profile_settings),
            CreatedAt = x.created_at,
            EmailAddress = x.email_address,
            EncryptionKey = x.encryption_key,
        }).FirstOrDefault();
    }

    public override async Task<Profile> AddAsync(Profile entity)
    {
        await readWriteDatabase.ExecuteAsync(ProfileSql.Add, new
        {
            entity.Id,
            entity.FirstName,
            entity.LastName,
            Settings = new JsonBParameter<ProfileSettings>(entity.ProfileSettings),
            entity.CreatedAt,
            ProfileImage = entity.ProfileImageUrl,
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

    public override Task<Profile> PatchAsync(Guid id, JsonPatchDocument<Profile> entity)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ExistsAsync(string emailAddress)
    {
        var emailList = await readOnlyConnection.QueryAsync(ProfileSql.Exists, new { Email = emailAddress });
        return emailList.Any();
    }

    public async Task<List<IEntity>> SearchAcrossProfileAsync(string search, Guid userId)
    {
        List<IEntity> entities = [];
        var searchParam = search.Split(" ").Select(x => $"%{x}%").ToList();
        var gridReader = await readOnlyConnection.QueryMultipleAsync(ProfileSql.SearchEntities, new
        {
            UserId = userId,
            @Search = searchParam,
        });

        entities.AddRange(from file in await gridReader.ReadAsync()
            let fileTypeRaw = file.type.ToString()
            select new File()
            {
                Id = file.id,
                Name = file.name,
                ParentId = file.parent,
                MimeType = file.mime_type,
                TargetSize = file.target_size,
                Type = ((string)fileTypeRaw).GetFileTypeFromString()!.Value
            });
        return entities;
    }

    private static class ProfileSql
    {
        public const string SelectById = """
                                         SELECT p.id as "id", first_name, last_name, profile_image, profile_settings, created_at, email_address FROM profile p join "user" u on p.id = u.id WHERE p.id = @Id;
                                         """;

        public const string Add = """
                                  INSERT INTO public.profile (id, first_name, last_name, "profile_settings", "created_at", "profile_image") VALUES (@Id, @FirstName, @LastName, @Settings, @CreatedAt, @ProfileImage);;
                                  """;
       public const string Exists = "SELECT 1 FROM profile WHERE id = @Email LIMIT 1";

       public const string SearchEntities = """
                                            
                                            SELECT id AS "id", "name" AS name, parent AS "parent", "mime_type", "type", target_size
                                            FROM files.file f WHERE f.owner = @UserId AND is_deleted = false AND (
                                                "name" ILIKE ANY(@Search) OR mime_type ILIKE ANY(@Search)
                                            )
                                            ORDER BY f.name, f.created
                                            
                                            """;
    }
}