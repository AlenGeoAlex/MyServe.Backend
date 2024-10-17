using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Worker.Scheduled.Constants;
using MyServe.Backend.Worker.Scheduled.Workers.Cleanup;
using Quartz;

namespace MyServe.Backend.Worker.Scheduled.Extensions;

public static class BootstrapExtensions
{
    public static async Task<IServiceCollection> ConfigureJob(this IServiceCollection services, IConfiguration configuration, ISecretClient secretClient)
    {
        await services.ScheduleJobs(configuration, secretClient);
        return services;
    }

    private async static Task ScheduleJobs(this IServiceCollection services, IConfiguration configuration,
        ISecretClient secretClient)
    {
        services.AddQuartz(config =>
        {
            config.AddJob<CleanupWorker>(JobKeyConstants.Cleanup).AddTrigger(x =>
            {
                x.ForJob(JobKeyConstants.Cleanup).WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever());
            });
        });
    }
    
}