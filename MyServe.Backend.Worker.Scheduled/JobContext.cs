using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common.Abstract;
using Quartz;
using Serilog;

namespace MyServe.Backend.Worker.Scheduled;

public abstract class JobContext(IRequestContext requestContext, ICacheService cacheService, ILogger logger) : IJob
{
    
    protected abstract Task PrepareJob(IJobExecutionContext context);
    protected abstract Task<bool> ShouldRun(IJobExecutionContext context);
    protected abstract Task Run(IJobExecutionContext context);
    protected abstract Task DisposeJob(IJobExecutionContext context);
    protected abstract bool HasCacheControl { get; set; }

    
    public async Task Execute(IJobExecutionContext context)
    {
        DateTime startedAt = DateTime.UtcNow;
        logger.Information("Starting {JobKey} at {DateTime}", context.JobDetail.Key, startedAt);
        bool shouldTheJobRun = true;
        try
        { 
            shouldTheJobRun = await ShouldRun(context);
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
            await PrepareJob(context);
        }
        catch (Exception e)
        {
            logger.Error(e, "An unknown error occured while preparing the job [{JobKey}]", context.JobDetail.Key);
            logger.Error("Execution has been aborted");
            return;
        }

        try
        {
            await Run(context);
        }
        catch (Exception e)
        {
            logger.Error(e, "An unknown error occured while running the job [{JobKey}]", context.JobDetail.Key);
            logger.Error("Execution has been aborted");
            return;
        }
        
        try
        {
            await DisposeJob(context);
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
        await CleanCache(context);
    }

    private async Task CleanCache(IJobExecutionContext context)
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