using Quartz;
using Serilog;

namespace SpotifyPlaylistUtilities.RecurringJobs;

[DisallowConcurrentExecution]
public class ShufflePlaylistJob(ILogger loggerApplication) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        loggerApplication.Information("Firing scheduled job {ThisName}", nameof(ShufflePlaylistJob));
        
        
        loggerApplication.Information("Finished scheduled job {ThisName}", nameof(ShufflePlaylistJob));
        return Task.CompletedTask;
    }
}