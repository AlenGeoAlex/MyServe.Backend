using Microsoft.AspNetCore.JsonPatch;

namespace MyServe.Backend.App.Domain.Abstracts;

public interface IAppRepository<T> where T : class
{
    
    /**
     * Get an entity by id
     */
    Task<T?> GetByIdAsync(Guid id);
    
    /**
     * Adds entity async
     */
    Task<T> AddAsync(T entity);
    
    /**
     * Updates entity async
     */
    Task UpdateAsync(T entity);
    
    /**
     * Deletes an entity
     */
    Task DeleteAsync(T entity);
    
    /**
     * Deletes an entity by id
     */
    Task DeleteByIdAsync(Guid id);
    
    /**
     * Path the document
     */
    Task<T> PatchAsync(Guid id, JsonPatchDocument<T> entity);
}