using Quartz;
using Serilog;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities.Scheduler.RecurringJobs;

[DisallowConcurrentExecution]
public class ShufflePlaylistJob(ILogger loggerApplication, PlaylistSearcher playlistSearcher, Shuffler shuffler) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        throw new NotImplementedException("Put this back when done making thing for Ceez");
        
        // loggerApplication.Information("Firing scheduled job {ThisName}", nameof(ShufflePlaylistJob));
        //
        // var dependencyContainer = await DependencyInjectionRoot.GetBuiltContainer();
        // await using var scope = dependencyContainer.BeginLifetimeScope();
        //
        // await Program.ShuffleAllPlaylistsImmediatelyOnce(scope);
        //
        // loggerApplication.Information("Finished scheduled job {ThisName}", nameof(ShufflePlaylistJob));
    }
}