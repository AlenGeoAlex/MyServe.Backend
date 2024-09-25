using System.Data;
using System.Net;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Extensions;
using MyServe.Backend.App.Domain.Exceptions;
using MyServe.Backend.App.Domain.Models.User;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Abstract;
using MyServe.Backend.App.Infrastructure.Database.NpgSql;
using Npgsql;

namespace MyServe.Backend.App.Infrastructure.Repositories;

public class UserOtpRepository([FromKeyedServices("read-only-connection")]NpgsqlConnection readOnlyConnection, [FromKeyedServices("read-write-connection")] NpgsqlConnection readWriteDatabase) : AbstractRepository<UserOtp>(readOnlyConnection, readWriteDatabase), IUserOtpRepository
{

    private const string OtpCountColumnName = "otp_count";
    private const string IsLockedColumnName = "is_locked";
    private const string IdColumnName = "id";
    
    public override Task<UserOtp?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public override async Task<UserOtp> AddAsync(UserOtp entity)
    {
        await readWriteDatabase.ExecuteAsync(UserOtpSql.InsertOtp, new
        {
            Id = Guid.NewGuid(),
            UserId = entity.UserId,
            Otp = entity.Otp,
            Expiry = new NpgSqlDateTimeOffsetParameter(entity.Expiry),
            Device = entity.Device,
        });

        return entity;
    }

    public override Task UpdateAsync(UserOtp entity)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteAsync(UserOtp entity)
    {
        throw new NotImplementedException();
    }

    public override Task DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ValidateOtp(string otp, string emailAddress, string device)
    {
        var count = await readWriteDatabase.ExecuteAsync(UserOtpSql.ValidateOtp, new
        {
            Code = otp,
            EmailAddress = emailAddress,
            Device = device,
            Expiry = new NpgSqlDateTimeOffsetParameter(),
        });

        return count == 1;
    }

    public async Task<(int, bool, Guid)> GetOtpCreationCriteria(string emailAddress)
    {
        var enumerable = (await readOnlyConnection.QueryAsync(UserOtpSql.ActiveOtpOfUser, new
        {
            EmailAddress = emailAddress,
            Expiry = new NpgSqlDateTimeOffsetParameter()
        })).ToList();

        if (!enumerable.Any())
        {
            return (-1, true, Guid.Empty); 
        }

        IDictionary<string, object> activeUserOtp = enumerable.First();

        // if (!MiscExtensions.DoesPropertyExist(activeUserOtp, OtpCountColumnName))
        //     throw new UnknownException("An unknown exception has occured while creating the otp.", 0001, HttpStatusCode.InternalServerError, $"No {OtpCountColumnName} has been found in the sql response");
        //
        // if(!MiscExtensions.DoesPropertyExist(activeUserOtp, IsLockedColumnName))
        //     throw new UnknownException("An unknown exception has occured while creating the otp.", 0001, HttpStatusCode.InternalServerError, $"No {IsLockedColumnName} has been found in the sql response");
        //
        // if(!MiscExtensions.DoesPropertyExist(activeUserOtp, IdColumnName))
        //     throw new UnknownException("An unknown exception has occured while creating the otp.", 0001, HttpStatusCode.InternalServerError, $"No {IsLockedColumnName} has been found in the sql response");
        //
        var otpCount = activeUserOtp[OtpCountColumnName];
        if (!int.TryParse(otpCount.ToString(), out int activeOtp))
            throw new UnknownException("An unknown exception has occured while creating the otp.", 0001, HttpStatusCode.InternalServerError, $"Not a valid integer has been passed on {OtpCountColumnName}");

        var dynamicLockedColumn = activeUserOtp[IsLockedColumnName];
        if(!bool.TryParse(dynamicLockedColumn.ToString(), out bool isLocked))
            throw new UnknownException("An unknown exception has occured while creating the otp.", 0001, HttpStatusCode.InternalServerError, $"Not a valid bool has been passed on {IsLockedColumnName}");
        
        string dynamicIdColumn = activeUserOtp[IdColumnName]?.ToString() ?? string.Empty;
        if(!Guid.TryParse(dynamicIdColumn, out Guid parsedId))
            throw new UnknownException("An unknown exception has occured while creating the otp.", 0001, HttpStatusCode.InternalServerError, $"Not a valid bool has been passed on {IdColumnName}");
        
        return (activeOtp, isLocked, parsedId);
    }

    private static class UserOtpSql
    {
        public const string InsertOtp = "INSERT INTO otp (id, user_id, otp, expiry, device) VALUES (@Id, @UserId, @Otp, @Expiry, @Device);";

        public const string ValidateOtp = """
                                          DELETE FROM otp o
                                          USING "user" u
                                          WHERE u.id = o.user_id AND u.email_address = @EmailAddress
                                          AND u.is_locked = false AND o.expiry >= @Expiry AND o.otp = @Code AND o.device = @Device 
                                          """;
        public const string ActiveOtpOfUser = $"""
                                               SELECT
                                                   COUNT(o.id) AS {OtpCountColumnName},
                                                   u.id AS {IdColumnName},
                                                   u.is_locked AS {IsLockedColumnName}
                                               FROM "user" u
                                               LEFT JOIN
                                                   otp o ON u.id = o.user_id AND o.expiry >= @Expiry
                                               WHERE
                                                   u.email_address = @EmailAddress
                                               GROUP BY
                                                   u.id, u.is_locked;
                                               """;
    }
}