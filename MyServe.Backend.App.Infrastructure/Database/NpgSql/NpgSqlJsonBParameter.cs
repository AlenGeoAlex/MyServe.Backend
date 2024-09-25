using System.Data;
using Dapper;
using Newtonsoft.Json;
using Npgsql;
using NpgsqlTypes;

namespace MyServe.Backend.App.Infrastructure.Database.NpgSql;

public class JsonBParameter<T>(T data) : SqlMapper.ICustomQueryParameter
{
    public void AddParameter(IDbCommand command, string name)
    {
        var parameter = (NpgsqlParameter)command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = JsonConvert.SerializeObject(data);
        parameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        command.Parameters.Add(parameter);
    }
}