using Autofac;
using SpotifyPlaylistUtilities.Scheduler;
using SpotifyPlaylistUtilities.SpotifyApiClient.Artists;
using SpotifyPlaylistUtilities.SpotifyApiClient.PlaylistBackups;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities;

internal static class Program
{
    private const string ArtistsFilePath = "/home/david/Desktop/artists.txt";
    private const string PlaylistNameExact = "Artist Test 01";
    
    internal static async Task Main()
    {
        
        var dependencyContainer = await DependencyInjectionRoot.GetBuiltContainer();
        await using var scope = dependencyContainer.BeginLifetimeScope();
        
        await printAllPlaylistNamesAndIds(scope);
        
        var artistsAdder = scope.Resolve<ArtistsAdder>();
        await artistsAdder.AddFromFileToPlaylistNamed(ArtistsFilePath, PlaylistNameExact);
        
        Console.WriteLine();
    }
    
    
    // public static readonly string[] PlaylistNamesToShuffle = [ "Curated Weebletdays", "Beeblet Chill", "Weebletdays Reserves", "Our Songs <3", "Jazz", "Muzicalz", "Tally Hall and Stuf", "Crimbus", "Metal", "Art Music", "Pixel Gardener", "PG - To Check", "What fresh hell is this", "WTF are you listening to David", "Dirty Songs", "Best of the best" ];
    //
    // internal static async Task Main()
    // {
    //     var dependencyContainer = await DependencyInjectionRoot.GetBuiltContainer();
    //     await using var scope = dependencyContainer.BeginLifetimeScope();
    //
    //     // await printAllPlaylistNamesAndIds(scope);
    //
    //     // Uncomment only one of these at a time
    //     await ShuffleAllPlaylistsImmediatelyOnce(scope); // await makeWeebletdaysSelectDaily(scope); // (Although shuffleAllPlaylistsImmediatelyOnce() already does this)
    //     //await startScheduler(scope);
    //
    //     //await restoreTracksFromJsonBackupFile(scope, "/home/david/SpotifyPlaylistUtilities/playlist-backups/Metal/2025-04-06_T19-10-28_7zBTbIZz2lMy31TQlZvI5m.json");
    //     
    //     // await inspectDeserializedJsonFileAsPlaylist(scope);     // You'll probably want to set a breakpoint in this method
    // }

    private static async Task makeWeebletdaysSelectDaily(ILifetimeScope scope)
    {
        var shuffler = scope.Resolve<Shuffler>();

        await shuffler.MakeSelectDaily();
    }

    // public static async Task ShuffleAllPlaylistsImmediatelyOnce(ILifetimeScope scope)
    // {
    //     var searcher = scope.Resolve<PlaylistSearcher>();
    //     var shuffler = scope.Resolve<Shuffler>();
    //
    //     foreach (var playlistName in PlaylistNamesToShuffle)
    //     {
    //         var spotifyPlaylist = await searcher.GetPlaylistByName(playlistName);
    //     
    //         await shuffler.ShuffleAllIn(spotifyPlaylist, false);    
    //     }
    //     
    //     await makeWeebletdaysSelectDaily(scope);
    // }

    private static async Task startScheduler(ILifetimeScope scope)
    {
        var scheduler = scope.Resolve<JobScheduler>();
        await scheduler.Start();

        while (true)
        {
            await Task.Delay(999);
        }
    }

    private static async Task inspectDeserializedJsonFileAsPlaylist(ILifetimeScope scope)
    {
        var restoreOperator = scope.Resolve<RestoreOperator>();
        var deserializedPlaylist = await restoreOperator.DeserializeOnlyFromJsonFile("/home/david/Desktop/Beeblet Chill/2024-12-02_T03-16-53_1gZqNgs8xccDNTXbBhZphq.json");
        Console.WriteLine(deserializedPlaylist.Name);
    }

    private static async Task restoreTracksFromJsonBackupFile(ILifetimeScope scope, string fullPathJson)
    {
        var restoreOperator = scope.Resolve<RestoreOperator>();
        await restoreOperator.RestorePlaylistFromJsonFile(fullPathJson);
    }

    private static async Task printAllPlaylistNamesAndIds(ILifetimeScope scope)
    {
        var infoPrinter = scope.Resolve<InfoPrinter>();
        await infoPrinter.PrintAllPlaylists();
    }
}