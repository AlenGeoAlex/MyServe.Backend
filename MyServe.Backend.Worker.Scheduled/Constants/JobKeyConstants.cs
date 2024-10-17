using Quartz;

namespace MyServe.Backend.Worker.Scheduled.Constants;

public class JobKeyConstants
{
    public static JobKey Cleanup => new JobKey(nameof(Cleanup));
}