using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.App.Domain.Models;
using MyServe.Backend.App.Domain.Models.Profile;

namespace MyServe.Backend.App.Domain.Repositories;

public interface IProfileRepository : IAppRepository<Profile>
{
    Task<bool> ExistsAsync(string emailAddress);
    Task<List<IEntity>> SearchAcrossProfileAsync(string search, Guid userId);
}