using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common.Abstract;
using Quartz;
using Serilog;

namespace MyServe.Backend.Worker.Scheduled;

public abstract class JobContext(IRequestContext requestContext, ICacheService cacheService, ILogger logger) : IJob
{
    protected abstract Task PrepareJobAsync(IJobExecutionContext context);
    protected abstract Task<bool> ShouldRunAsync(IJobExecutionContext context);
    protected abstract Task RunAsync(IJobExecutionContext context);
    protected abstract Task DisposeJobAsync(IJobExecutionContext context);
    protected abstract bool HasCacheControl { get; set; }
    
    public async Task Execute(IJobExecutionContext context)
    {
        DateTime startedAt = DateTime.UtcNow;
        logger.Information("Starting {JobKey} at {DateTime}", context.JobDetail.Key, startedAt);
        bool shouldTheJobRun = true;
        try
        { 
            shouldTheJobRun = await ShouldRunAsync(context);
        }
        catch (Exception e)
        {
            logger.Error(e, "An unknown error occured while determining whether Job [{JobKey}] execution should be done.", context.JobDetail.Key);
            logger.Fatal(e.Message);
        }

        if (!shouldTheJobRun)
        {
            logger.Information("Job execution has been skipped for {JobKey} since ShouldRun has been failed", context.JobDetail.Key);
            return;
        }

        try
        {
            await PrepareJobAsync(context);
        }
        catch (Exception e)
        {
            logger.Error(e, "An unknown error occured while preparing the job [{JobKey}]", context.JobDetail.Key);
            logger.Error("Execution has been aborted");
            return;
        }

        try
        {
            await RunAsync(context);
        }
        catch (Exception e)
        {
            logger.Error(e, "An unknown error occured while running the job [{JobKey}]", context.JobDetail.Key);
            logger.Error("Execution has been aborted");
            return;
        }
        
        try
        {
            await DisposeJobAsync(context);
        }
        catch (Exception e)
        {
            logger.Error(e, "An unknown error occured while disposing the job [{JobKey}]", context.JobDetail.Key);
            logger.Error("Execution has been aborted");
            return;
        }
        
        DateTime endedAt = DateTime.UtcNow;
        logger.Information("Ending {JobKey} at {DateTime}", context.JobDetail.Key, endedAt);
        logger.Information("Execution of {JobKey} took {Duration} sec", context.JobDetail.Key, (endedAt - startedAt).TotalSeconds);
        await CleanCacheAsync(context);
    }

    private async Task CleanCacheAsync(IJobExecutionContext context)
    {
        if (!HasCacheControl)
            return;

        logger.Information("Starting cache clearing of {JobKey}", context.JobDetail.Key);
        try
        {
            if (requestContext.CacheControl.ExpiryKeys.Count == 0)
                return;

            await cacheService.ClearMatchingAsync((HashSet<string>)requestContext.CacheControl.ExpiryKeys);
            logger.Information("Cache has been cleared for {JobKey} at {DateTime}", context.JobDetail.Key, DateTime.UtcNow);
        }
        catch (Exception e)
        {
            logger.Error(e, e.Message);
        }
    }
}