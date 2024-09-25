using System.Data;
using Dapper;
using MyServe.Backend.App.Domain.Models.Profile;
using Newtonsoft.Json;

namespace MyServe.Backend.App.Infrastructure.TypeHandler;

public class ProfileSettingsTypeHandler : SqlMapper.TypeHandler<ProfileSettings?>
{
    public override void SetValue(IDbDataParameter parameter, ProfileSettings? value)
    {
        parameter.Value = JsonConvert.SerializeObject(value);
    }

    public override ProfileSettings? Parse(object value)
    {
        return value is string json ? JsonConvert.DeserializeObject<ProfileSettings>(json) : null;
    }
}