using System.Data;
using System.Transactions;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.Common.Exceptions;
using MyServe.Backend.App.Application.Abstract;
using MyServe.Backend.App.Domain.Abstracts;
using Npgsql;
using Serilog;

namespace MyServe.Backend.App.Infrastructure.Abstract.UnitOfWork;

public class ReadWriteUnitOfWork(IServiceProvider serviceProvider, ILogger logger) : AbstractUnitOfWork(serviceProvider), IReadWriteUnitOfWork
{
    private NpgsqlTransaction? _transaction = null;
    private NpgsqlConnection _connection = null!;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    
    protected override void OpenConnection()
    {
        try
        {
            _connection = _serviceProvider.GetRequiredKeyedService<NpgsqlConnection>("read-write-connection");
            if(_connection.State == ConnectionState.Open)
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

    public override async Task<IUnitOfWork> StartTransactionAsync()
    {
        _transaction = await _connection.BeginTransactionAsync();
        return this;
    }

    public override async Task CommitAsync()
    {
        if(_connection.State == ConnectionState.Closed)
            throw new InfrastructureException(InfrastructureSource.Database, "The connection is closed");

        if (_transaction == null)
            return;
        try
        {
            await _transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred during commit.", ex);
        }
        finally
        {
            await _transaction.DisposeAsync();
        }
    }

    public override async Task RollbackAsync()
    {
        bool shouldClose = false;
        if(_transaction is null)
            return;
        try
        {
            if (_connection.State != ConnectionState.Open)
            {
                shouldClose = true;
                _connection.Open();
            }

            await _transaction.RollbackAsync();
        }
        catch (Exception e)
        {
            logger.Error(e, "An error occurred during rollback");
            logger.Error(e.ToString());
        }
        finally
        {
            if (shouldClose)
                await CloseConnection();
        }
    }

}