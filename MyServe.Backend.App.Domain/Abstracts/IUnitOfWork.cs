namespace MyServe.Backend.App.Domain.Abstracts;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    Task<IUnitOfWork> StartTransactionAsync();
    
    Task CommitAsync();
    
    Task RollbackAsync();
    
    IAppRepository<T> GetAppRepository<T>() where T : class;
}

public interface IReadOnlyUnitOfWork : IUnitOfWork;
public interface IReadWriteUnitOfWork : IUnitOfWork;