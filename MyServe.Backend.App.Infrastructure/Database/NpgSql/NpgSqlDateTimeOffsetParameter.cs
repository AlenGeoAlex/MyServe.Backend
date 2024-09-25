using System.Data;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace MyServe.Backend.App.Infrastructure.Database.NpgSql;

public class NpgSqlDateTimeOffsetParameter : SqlMapper.ICustomQueryParameter
{
    private readonly DateTimeOffset _value;

    public NpgSqlDateTimeOffsetParameter()
    {
        _value = DateTimeOffset.UtcNow;
    }

    public NpgSqlDateTimeOffsetParameter(DateTimeOffset value)
    {
        _value = value.ToUniversalTime();
    }

    public NpgSqlDateTimeOffsetParameter(DateTime value)
    {
        _value = value.ToUniversalTime();
    }

    public void AddParameter(IDbCommand command, string name)
    {
        var parameter = (NpgsqlParameter)command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = _value;
        parameter.NpgsqlDbType = NpgsqlDbType.TimestampTz;
        command.Parameters.Add(parameter);
    }
}