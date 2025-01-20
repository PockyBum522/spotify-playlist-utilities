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
        
        foreach (var playlistName in Program.PlaylistNamesToShuffle)
        {
            var spotifyPlaylist = await searcher.GetPlaylistByName(playlistName);
        
            await shuffler.ShuffleAllIn(spotifyPlaylist, false);    
        }
        
        await shuffler.MakeSelectDaily();
        
        loggerApplication.Information("Finished scheduled job {ThisName}", nameof(ShufflePlaylistJob));
    }
}