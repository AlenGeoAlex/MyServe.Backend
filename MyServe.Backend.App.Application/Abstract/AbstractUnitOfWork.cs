using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Domain.Abstracts;

namespace MyServe.Backend.App.Application.Abstract;

public abstract class AbstractUnitOfWork : IUnitOfWork
{
    private readonly IServiceProvider serviceProvider;

    protected AbstractUnitOfWork(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        OpenConnection();
    }

    protected abstract void OpenConnection();
    protected abstract Task CloseConnection();
    public abstract Task<IUnitOfWork> StartTransactionAsync();
    public abstract Task CommitAsync();
    public abstract Task RollbackAsync();

    public IAppRepository<T> GetAppRepository<T>() where T : class
    {
        var repository = serviceProvider.GetService<IAppRepository<T>>();
        if (repository == null)
        {
            throw new InvalidOperationException($"Repository of type {typeof(T).Name} is not registered.");
        }
        return repository;
    }

    public void Dispose()
    {
        CloseConnection().Wait();
    }

    public async ValueTask DisposeAsync()
    {
        await CloseConnection();
    }
}