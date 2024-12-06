using Autofac;
using SpotifyPlaylistUtilities.Scheduler;
using SpotifyPlaylistUtilities.SpotifyApiClient.PlaylistBackups;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities;

internal static class Program
{
    public static readonly string[] PlaylistNamesToShuffle = [ "Curated Weebletdays", "Beeblet Chill", "Weebletdays Reserves", "Our Songs <3", "Jazz", "Muzicalz", "Tally Hall and Stuf", "Crimbus", "Metal", "Art Music", "Pixel Gardener", "PG - To Check", "What fresh hell is this", "WTF are you listening to David", "Dirty Songs", "Best of the best" ];
    
    internal static async Task Main()
    {
        var dependencyContainer = await DependencyInjectionRoot.GetBuiltContainer();
        await using var scope = dependencyContainer.BeginLifetimeScope();

        // await printAllPlaylistNamesAndIds(scope);

        // Uncomment only one of these at a time
        // await shuffleAllPlaylistsImmediatelyOnce(scope);
        // await startScheduler(scope);

        
        // await restoreTracksFromJsonBackupFile(scope);
        
        // await inspectDeserializedJsonFileAsPlaylist(scope);     // You'll probably want to set a breakpoint in this method

        await makeWeebletdaysSelectDaily(scope);
    }

    private static async Task makeWeebletdaysSelectDaily(ILifetimeScope scope)
    {
        var shuffler = scope.Resolve<Shuffler>();

        await shuffler.MakeSelectDaily();
    }

    private static async Task shuffleAllPlaylistsImmediatelyOnce(ILifetimeScope scope)
    {
        var searcher = scope.Resolve<Searcher>();
        var shuffler = scope.Resolve<Shuffler>();

        foreach (var playlistName in PlaylistNamesToShuffle)
        {
            var spotifyPlaylist = await searcher.GetPlaylistByName(playlistName);
        
            await shuffler.ShuffleAllIn(spotifyPlaylist, false);    
        }
    }

    private static async Task startScheduler(ILifetimeScope scope)
    {
        var scheduler = scope.Resolve<JobScheduler>();
        await scheduler.Start();
    }

    private static async Task inspectDeserializedJsonFileAsPlaylist(ILifetimeScope scope)
    {
        var restoreOperator = scope.Resolve<RestoreOperator>();
        var deserializedPlaylist = await restoreOperator.DeserializeOnlyFromJsonFile("/home/david/Desktop/Beeblet Chill/2024-12-02_T03-16-53_1gZqNgs8xccDNTXbBhZphq.json");
        Console.WriteLine(deserializedPlaylist.Name);
    }

    private static async Task restoreTracksFromJsonBackupFile(ILifetimeScope scope)
    {
        var restoreOperator = scope.Resolve<RestoreOperator>();
        await restoreOperator.RestorePlaylistFromJsonFile("/home/david/Desktop/Pixel Gardener/2024-12-02_T03-13-35_7pnXJ7jWswV32QGjJwyuFY.json");
    }

    private static async Task printAllPlaylistNamesAndIds(ILifetimeScope scope)
    {
        var infoPrinter = scope.Resolve<InfoPrinter>();
        await infoPrinter.PrintAllPlaylists();
    }
}