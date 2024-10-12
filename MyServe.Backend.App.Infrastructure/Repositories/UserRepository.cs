using Dapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Domain.Models.User;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Abstract;
using Npgsql;

namespace MyServe.Backend.App.Infrastructure.Repositories;

public class UserRepository([FromKeyedServices("read-only-connection")]NpgsqlConnection readOnlyConnection, [FromKeyedServices("read-write-connection")] NpgsqlConnection readWriteDatabase) : AbstractRepository<User>(readOnlyConnection, readWriteDatabase), IUserRepository
{
    public override async Task<User?> GetByIdAsync(Guid id)
    {
        var userByEmail = await readOnlyConnection.QueryAsync(UserSql.GetUserById, new
        {
            Id = id 
        });
        
        return HydrateUserFromEnumerable(userByEmail);
    }

    public override Task<User> AddAsync(User entity)
    {
        throw new NotImplementedException();
    }

    public override Task UpdateAsync(User entity)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteAsync(User entity)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public override Task<User> PatchAsync(Guid id, JsonPatchDocument<User> entity)
    {
        throw new NotImplementedException();
    }

    public async Task<User?> GetByEmail(string email)
    {
        var userByEmail = await readOnlyConnection.QueryAsync(UserSql.GetUserByEmail, new
        {
            EmailAddress = email
        });
        
        return HydrateUserFromEnumerable(userByEmail);
    }

    public Task UpdateLastLogin(Guid userId, DateTimeOffset? lastLogin = null)
    {
        throw new NotImplementedException();
    }

    private User? HydrateUserFromEnumerable(IEnumerable<dynamic> rows)
    {
        return rows.Select(x => new User()
        {
            EmailAddress = x.email_address,
            Id = x.id,
            IsLocked = x.is_locked,
            LastLogin = x.last_login,
        }).FirstOrDefault();
    }

    private static class UserSql
    {
        public const string GetUserByEmail = "SELECT * FROM \"user\" WHERE email_address = @EmailAddress LIMIT 1";
        public const string GetUserById = "SELECT * FROM \"user\" WHERE id = @Id LIMIT 1";
    }
}