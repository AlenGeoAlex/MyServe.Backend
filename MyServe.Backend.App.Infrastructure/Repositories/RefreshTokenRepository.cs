using System.Data;
using Dapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.App.Domain.Models.User;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Abstract;
using MyServe.Backend.App.Infrastructure.Database.NpgSql;
using Npgsql;

namespace MyServe.Backend.App.Infrastructure.Repositories;

public class RefreshTokenRepository([FromKeyedServices("read-only-connection")]NpgsqlConnection readOnlyConnection, [FromKeyedServices("read-write-connection")] NpgsqlConnection readWriteDatabase) : AbstractRepository<RefreshToken>(readOnlyConnection, readWriteDatabase), IRefreshTokenRepository
{
    public override async Task<RefreshToken?> GetByIdAsync(Guid id)
    {
        var token = await readOnlyConnection.QueryAsync(RefreshTokenSql.GetRefreshTokenById, new { RefreshTokenId = id });
        return token.Select(x => new RefreshToken()
        {
            Id = x.id,
            CreatedAt = x.created_at,
            Expiry = x.expiry,
            UserId = x.user_id,
        }).FirstOrDefault();
    }

    public override async Task<RefreshToken> AddAsync(RefreshToken entity)
    {
        await readWriteDatabase.ExecuteAsync(RefreshTokenSql.InsertRefreshToken, new
        {
            entity.Id,
            entity.UserId,
            CreatedAt = new NpgSqlDateTimeOffsetParameter(entity.CreatedAt),
            Expiry = new NpgSqlDateTimeOffsetParameter(entity.Expiry)
        });

        return entity;
    }

    public override Task UpdateAsync(RefreshToken entity)
    {
        throw new NotImplementedException();
    }

    public override async Task DeleteAsync(RefreshToken entity)
    {
        await DeleteByIdAsync(entity.Id);
    }

    public override async Task DeleteByIdAsync(Guid id)
    {
        await readWriteDatabase.ExecuteAsync(RefreshTokenSql.DeleteTokenById, new { RefreshTokenId = id });
    }

    public override Task<RefreshToken> PatchAsync(Guid id, JsonPatchDocument<RefreshToken> entity)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ValidateRefreshToken(RefreshToken token)
    {
        var deletedCount = await readOnlyConnection.ExecuteAsync(RefreshTokenSql.ValidateRefreshToken, new
        {
            Expiry = new NpgSqlDateTimeOffsetParameter(),
            RefreshTokenId = token.Id
        });

        return deletedCount == 0;
    }

    private static class RefreshTokenSql
    {

        public const string GetRefreshTokenById = """
                                                  SELECT * FROM refresh_token WHERE id = @RefreshTokenId
                                                  """;
        
        public const string InsertRefreshToken = """
                                                 INSERT INTO refresh_token (id, user_id, created_at, expiry)
                                                 VALUES (@Id, @UserId, @CreatedAt, @Expiry)
                                                 """;
        
        public const string ValidateRefreshToken = """
                                                   DELETE FROM refresh_token r
                                                   USING "user" u
                                                   WHERE u.id = r.user_id
                                                   AND u.is_locked = false AND r.expiry >= @Expiry AND r.id = @RefreshTokenId
                                                   """;

        public const string DeleteTokenById = """
                                              DELETE FROM refresh_token WHERE id = @RefreshTokenId
                                              """;
    }
}