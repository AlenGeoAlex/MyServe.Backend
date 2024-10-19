using Dapper;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using Npgsql;
using Quartz;
using Serilog;

namespace MyServe.Backend.Worker.Scheduled.Workers.Cleanup;

[DisallowConcurrentExecution]
public class CleanupWorker(
    IRequestContext requestContext,
    ICacheService cacheService,
    ILogger logger,
    [FromKeyedServices(ServiceKeyConstants.Database.ReadWriteDatabase)] NpgsqlConnection readWriteDatabase
    ) : JobContext(requestContext, cacheService, logger)
{
    protected override bool HasCacheControl { get; set; } = false;

    protected override Task PrepareJobAsync(IJobExecutionContext context)
    {
        return Task.CompletedTask;
    }

    protected override Task<bool> ShouldRunAsync(IJobExecutionContext context)
    {
        return Task.FromResult(true);
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await readWriteDatabase.ExecuteAsync("SELECT public.cleanup()");
    }

    protected override async Task DisposeJobAsync(IJobExecutionContext context)
    {
        return;
    }
}