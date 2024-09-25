using System.Data;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Exceptions;
using MyServe.Backend.App.Application.Abstract;
using MyServe.Backend.App.Domain.Abstracts;
using Npgsql;

namespace MyServe.Backend.App.Infrastructure.Abstract.UnitOfWork;

public class ReadOnlyUnitOfWork(IServiceProvider provider) : AbstractUnitOfWork(provider), IReadOnlyUnitOfWork
{
    
    private NpgsqlConnection _connection = null!;
    private readonly IServiceProvider _serviceProvider = provider;

    protected override void OpenConnection()
    {
        try
        {
            _connection = _serviceProvider.GetRequiredKeyedService<NpgsqlConnection>("read-write-connection");
            if(_connection.State != ConnectionState.Open)
                return;
            
            _connection.Open();
        }
        catch (Exception e)
        {
            throw new InfrastructureException(InfrastructureSource.Database, $"Failed to connect to database due to ${e.Message}" , e);
        }
    }

    protected override Task CloseConnection()
    {
        try
        {
            return _connection.State == ConnectionState.Open ? _connection!.CloseAsync() : Task.CompletedTask;
        }
        catch (Exception e)
        {
            throw new InfrastructureException(InfrastructureSource.Database, $"Failed to disconnect from database due to ${e.Message}" , e);
        }
    }

    public override Task<IUnitOfWork> StartTransactionAsync()
    {
        return Task.FromResult<IUnitOfWork>(this);
    }

    public override Task CommitAsync()
    {
        return Task.CompletedTask;
    }

    public override Task RollbackAsync()
    {
        return Task.CompletedTask;
    }

}