using Dapper;
using MassTransit.Initializers;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Application.Features.Files.List;
using MyServe.Backend.App.Domain.Exceptions;
using MyServe.Backend.App.Domain.Extensions;
using MyServe.Backend.App.Domain.Models.Files;
using MyServe.Backend.App.Domain.Models.Profile;
using MyServe.Backend.App.Domain.Repositories;
using MyServe.Backend.App.Infrastructure.Abstract;
using MyServe.Backend.App.Infrastructure.Database.NpgSql;
using MyServe.Backend.Common.Options;
using Npgsql;
using File = MyServe.Backend.App.Domain.Models.Files.File;

namespace MyServe.Backend.App.Infrastructure.Repositories;

public class FileRepository([FromKeyedServices("read-only-connection")]NpgsqlConnection readOnlyConnection, [FromKeyedServices("read-write-connection")] NpgsqlConnection readWriteDatabase) : AbstractRepository<File>(readOnlyConnection, readWriteDatabase), IFileRepository
{
    public override async Task<File?> GetByIdAsync(Guid id)
    {
        var file = (await readOnlyConnection.QueryAsync(FileSql.GetById, new {Id = id}))
            .Select(x =>
            {
                string fileTypeRaw = x.type.ToString();
                return new File()
                {
                    Id = x.id,
                    Name = x.name,
                    Type = fileTypeRaw!.GetFileTypeFromString()!.Value,
                    Owner = x.owner,
                    OwnerProfile = new Profile()
                    {
                        Id = x.owner,
                        FirstName = x.first_name,
                        LastName = x.last_name
                    },
                    TargetSize = x.target_size,
                    MimeType = x.mime_type,
                    CreatedAt = x.created_at,
                    TargetUrl = x.target_url,
                    Favourite = x.favourite
                };
            }).FirstOrDefault();

        return file;
    }

    public override async Task<File> AddAsync(File entity)
    {
        try
        {
            await readWriteDatabase.ExecuteAsync(FileSql.Add, new
            {
                Id = entity.Id,
                Name = entity.Name,
                ParentId = entity.ParentId,
                Type = entity.Type.GetFileTypeAsString(),
                Owner = entity.Owner,
                CreatedAt = new NpgSqlDateTimeOffsetParameter(entity.CreatedAt),
                TargetUrl = entity.Type == FileType.Dir ? null : entity.TargetUrl,
                TargetSize = entity.TargetSize,
                MimeType = entity.MimeType,
                Created = entity.Owner,
            });
        }
        catch (Exception e)
        {
            throw new DataWriteFailedException(typeof(File), e.Message, e);
        }
        return entity;
    }

    public override async Task UpdateAsync(File entity)
    {
        try
        {
            var updatedFileCount = await readWriteDatabase.ExecuteAsync(FileSql.UpdateFile, new
            {
                entity.Id,
                entity.Name,
                entity.Favourite,
                ModifiedAt = new NpgSqlDateTimeOffsetParameter(),
            });
            
            if(updatedFileCount <= 0)
                throw new DataWriteFailedException(typeof(File), "Non existing");
        }
        catch (Exception e)
        {
            if (e is DataWriteFailedException)
                throw;
            
            throw new DataWriteFailedException(typeof(File), e.Message, e);
        }
    }

    public override Task DeleteAsync(File entity)
    {
        throw new NotImplementedException();
    }

    public override async Task DeleteByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public override async Task<File> PatchAsync(Guid id, JsonPatchDocument<File> entity)
    {
        try
        {
            var idEntity = await GetByIdAsync(id);
            if (idEntity is null)
                throw new NotFoundException(id, typeof(File));
        
            entity.ApplyTo(idEntity);
            await UpdateAsync(idEntity);
            return idEntity;
        }
        catch (Exception e)
        {
            throw new DataWriteFailedException(typeof(File), e.Message, e);
        }
    }

    public async Task<List<File>> ListFiles(Guid ownerId, Guid? parentId = null, ListOptions? listOptions = null)
    {
        listOptions ??= new ListFileOptions();
        var files = await readOnlyConnection.QueryAsync(FileSql.ListFile(parentId.HasValue, listOptions.OrderBy, listOptions.OrderDirection), new
        {
            Owner = ownerId,
            ParentId = parentId,
            OrderColumn = listOptions.OrderBy,
            OrderDirection = listOptions.OrderDirection,
            Skip = listOptions.Skip,
            Take = listOptions.Take,
        });

        return HydrateListFiles(files);
    }

    public async Task<(List<File> files, List<File> parents)> ListFilesWithParent(Guid ownerId, Guid? parentId = null, ListOptions? listOptions = null)
    {
        listOptions ??= new ListFileOptions();

        var queryCombined = FileSql.ListFile(parentId.HasValue, listOptions.OrderBy, listOptions.OrderDirection) + ";\n";
        queryCombined += FileSql.GetParentsOfChild;

        await using var multGridReader = await readOnlyConnection.QueryMultipleAsync(queryCombined, new
        {
            Owner = ownerId,
            ParentId = parentId,
            OrderColumn = listOptions.OrderBy,
            OrderDirection = listOptions.OrderDirection,
            Skip = listOptions.Skip,
            Take = listOptions.Take,
        });

        var filesEnumerable = await multGridReader.ReadAsync();
        var files = HydrateListFiles(filesEnumerable);
        var parentEnumerable = await multGridReader.ReadAsync();
        var parentList = HydrateParents(parentEnumerable);
        return (files, parentList);
    }

    public Task HardDeleteAsync(List<Guid> ids)
    {
        throw new NotImplementedException();
    }

    public async Task<List<File>> SoftDeleteAsync(Guid id)
    {
        try
        {
            var files = (await readWriteDatabase.QueryAsync(FileSql.SoftDeleteById, new {Id = id}))
                .Select(x =>
                {
                    string fileTypeRaw = x.type.ToString();
                    return new File()
                    {
                        Id = x.id,
                        Name = x.name,
                        Type = fileTypeRaw!.GetFileTypeFromString()!.Value,
                    };
                }).ToList();

            return files;
        }
        catch (Exception e)
        {
            throw new DataWriteFailedException(typeof(File), e.Message, e);
        }
    }

    public Task<List<File>> GetParents(Guid childId)
    {
        throw new NotImplementedException();
    }



    #region Hydrations

    private List<File> HydrateParents(IEnumerable<dynamic> dynamic)
    {
        return dynamic.Select(x => new File()
        {
            Id = x.id,
            Name = x.name,
            ParentId = x.parent
        }).ToList();
    }

    private List<File> HydrateListFiles(IEnumerable<dynamic> dynamic)
    {
        return dynamic.Select(x =>
        {
            string fileTypeRaw = x.type.ToString();
            var file = new File()
            {
                Id = x.id,
                Name = x.name,
                Type = fileTypeRaw!.GetFileTypeFromString()!.Value,
                Owner = x.owner,
                OwnerProfile = new Profile()
                {
                    Id = x.owner,
                    FirstName = x.first_name,
                    LastName = x.last_name
                },
                TargetSize = x.target_size,
                MimeType = x.mime_type,
                CreatedAt = x.created_at,
                Favourite = x.favourite,
            };


            if (x.parent_id == null || x.parent_id == Guid.Empty) return file;
            file.ParentId = x.parent_id;
            file.Parent = new File()
            {
                Id = x.parent_id,
                Name = x.parent_name,
            };

            return file;
        }).ToList();
    }

    #endregion
    
    private static class FileSql
    {
        public const string GetById = """
                                        SELECT f."id", "name", parent, "type", "owner", f.created_at, target_url, target_size, mime_type, created, f.favourite ,
                                        p.first_name, p.last_name
                                        FROM files.file f
                                        JOIN public.profile p on p.id = f.owner
                                        WHERE f.id = @id AND is_deleted = false;
                                      """;

        public const string Add = """
                                    INSERT INTO files.file ("id", "name", parent, "type", "owner", created_at, target_url, target_size, mime_type, created)
                                    VALUES (@Id, @Name, @ParentId, @Type::files.filetype, @Owner, @CreatedAt, @TargetUrl, @TargetSize, @MimeType, @Created)
                                  """;
        
        public const string GetParentsOfChild = """
                                         WITH RECURSIVE parent_dir AS (
                                             SELECT id, name, parent, 0 AS depth
                                             FROM files.file
                                             WHERE id = @ParentId and type = 'dir'
                                             
                                             UNION ALL
                                             
                                             SELECT f.id, f.name, f.parent, pd.depth + 1
                                             FROM files.file f
                                             JOIN parent_dir pd ON f.id = pd.parent and f.type = 'dir'
                                         )
                                         SELECT id, name, parent
                                         FROM parent_dir
                                         ORDER BY depth DESC;
                                         """;        

        public static readonly Func<bool, string, string, string> ListFile =
            ((isParentPresent, orderBy, orderByDirection) =>
                {
                    var query = """
                                SELECT f."id", f."name", f."type", f."owner", p.first_name, p.last_name, f."target_size", f."mime_type", f."created_at", pf.id as "parent_id", pf.name as "parent_name", f.favourite
                                FROM "files"."file" f
                                join "public".profile p on p.id = f.owner
                                left join "files".file pf on pf.id = f.parent
                                WHERE f."owner" = @Owner AND f."is_deleted" = false
                                """;

                    if (isParentPresent)
                        query += " AND f.\"parent\" = @ParentId ";
                    else
                        query += " AND f.\"parent\" IS NULL";
                    
                    query += $" ORDER BY f.\"{orderBy}\" {orderByDirection}";
                    query += " OFFSET @Skip LIMIT @Take";
                    
                    return query;
                }
            );

        public const string UpdateFile = """
                                            UPDATE files.file SET "name" = @Name, "favourite" = @Favourite
                                            WHERE "id" = @Id AND is_deleted = false
                                          """;

        public const string SoftDeleteById = """
                                                SELECT * FROM files."delete_recursive"(@Id);
                                             """;
    }
}