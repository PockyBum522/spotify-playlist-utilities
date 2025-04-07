using Autofac;
using Quartz;
using Serilog;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities.Scheduler.RecurringJobs;

[DisallowConcurrentExecution]
public class ShufflePlaylistJob(ILogger loggerApplication, Searcher searcher, Shuffler shuffler) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        loggerApplication.Information("Firing scheduled job {ThisName}", nameof(ShufflePlaylistJob));
        
        var dependencyContainer = await DependencyInjectionRoot.GetBuiltContainer();
        await using var scope = dependencyContainer.BeginLifetimeScope();
        
        await Program.ShuffleAllPlaylistsImmediatelyOnce(scope);
        
        loggerApplication.Information("Finished scheduled job {ThisName}", nameof(ShufflePlaylistJob));
    }
}