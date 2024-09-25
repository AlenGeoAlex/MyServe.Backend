using System.Data;
using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Domain.Abstracts;
using Npgsql;

namespace MyServe.Backend.App.Infrastructure.Abstract;

public abstract class AbstractRepository<T>([FromKeyedServices("read-only-connection")]IDbConnection readOnlyConnection, [FromKeyedServices("read-write-connection")] IDbConnection readWriteDatabase) : IAppRepository<T>
{
    public abstract Task<T?> GetByIdAsync(Guid id);
    public abstract Task<T> AddAsync(T entity);
    public abstract Task UpdateAsync(T entity);
    public abstract Task DeleteAsync(T entity);
    public abstract Task DeleteByIdAsync(Guid id);
}