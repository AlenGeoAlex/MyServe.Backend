using MyServe.Backend.App.Domain.Abstracts;
using MyServe.Backend.Common.Options;
using File = MyServe.Backend.App.Domain.Models.Files.File;

namespace MyServe.Backend.App.Domain.Repositories;

public interface IFileRepository : IAppRepository<File>
{
    Task<List<File>> ListFiles(Guid ownerId, Guid? parentId = null, ListOptions? listOptions = null);
    
    Task<List<File>> GetParents(Guid childId);

    Task<(List<File> files, List<File> parents)> ListFilesWithParent(Guid ownerId, Guid? parentId = null, ListOptions? listOptions = null);
}