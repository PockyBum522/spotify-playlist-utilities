using Autofac;
using Serilog;
using SpotifyPlaylistUtilities.Scheduler;
using SpotifyPlaylistUtilities.SpotifyApiClient.Artists;
using SpotifyPlaylistUtilities.SpotifyApiClient.PlaylistBackups;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities;

internal static class Program
{
    private static string PlaylistNameExact = "ERROR GETTING PLAYLIST NAME";
    private static string ArtistsFilePath = "ERROR GETTING ARTIST FILE PATH";
    
    internal static async Task Main()
    {
        if (!allRequiredArgumentsPresent())
        {
            printCommandLineHelp();

            exitAfterUserPressesKey();
        }
        
        PlaylistNameExact = getPlaylistNameFromCommandLineArguments();
        ArtistsFilePath = getFilePathFromCommandLineArguments();

        printArguments(PlaylistNameExact, ArtistsFilePath);
        validateArguments(PlaylistNameExact, ArtistsFilePath);

        var dependencyContainer = await DependencyInjectionRoot.GetBuiltContainer();
        await using var scope = dependencyContainer.BeginLifetimeScope();
        
        var logger = dependencyContainer.Resolve<ILogger>();
        
        await printAllPlaylistNamesAndIds(scope);
        
        var artistsAdder = scope.Resolve<ArtistsAdder>();
        await artistsAdder.AddFromFileToPlaylistNamed(ArtistsFilePath, PlaylistNameExact);
        
        logger.Information("Finished adding all tracks for all artists in file: {FilePath}", ArtistsFilePath);
        exitAfterUserPressesKey();
    }

    private static void validateArguments(string playlistNameExact, string artistsFilePath)
    {
        if (!File.Exists(artistsFilePath))
        {
            Console.WriteLine();
            Console.WriteLine("FILE NOT FOUND at specified path:");
            Console.WriteLine(artistsFilePath);
            Console.WriteLine();
            
            exitAfterUserPressesKey();
        }
    }

    private static void exitAfterUserPressesKey()
    {
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
            
        Environment.Exit(0);
    }

    private static bool allRequiredArgumentsPresent()
    {
        var rawArguments = Environment.GetCommandLineArgs();

        if (rawArguments.Contains("--playlist-name") &&
            rawArguments.Contains("--artists-file"))
        {
            return true;
        }
        
        return false;
    }

    private static void printArguments(string playlistNameArgument, string filePathArgument)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"Playlist name: {playlistNameArgument}");
        Console.WriteLine($"File path: {filePathArgument}");
        Console.WriteLine();
        Console.WriteLine();
    }

    private static void printCommandLineHelp()
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("ONE OR MORE REQUIRED ARGUMENTS MISSING, SEE BELOW:");
        Console.WriteLine();
        Console.WriteLine("Specify playlist name to match exactly with:");
        Console.WriteLine("--playlist-name \"Your Playlist Name to Match\"");
        Console.WriteLine();
        Console.WriteLine("Specify full path to file containing a single artist to search on each line with:");
        Console.WriteLine("--artists-file /home/david/Desktop/artists.txt");
        Console.WriteLine();
        Console.WriteLine("(Double quotes around passed values are only required if the value has spaces.)");
        Console.WriteLine();
    }

    private static string getPlaylistNameFromCommandLineArguments()
    {
        var rawArguments = Environment.GetCommandLineArgs();

        // For debug/adding arguments later:
        // for (var i = 0; i < rawArguments.Length; i++)
        // {
        //     Console.WriteLine($"Arg #{i} = {rawArguments[i]}");
        // }

        for (var i = 0; i < rawArguments.Length; i++)
        {
            if (rawArguments[i].ToLower() == "--playlist-name")
                return rawArguments[i + 1].Trim();
        }
        
        return "ERROR GETTING PLAYLIST NAME FROM COMMAND LINE ARGUMENTS";
    }

    private static string getFilePathFromCommandLineArguments()
    {
        var rawArguments = Environment.GetCommandLineArgs();

        for (var i = 0; i < rawArguments.Length; i++)
        {
            if (rawArguments[i].ToLower() == "--artists-file")
                return rawArguments[i + 1].Trim();
        }
        
        return "ERROR GETTING ARTIST FILE PATH FROM COMMAND LINE ARGUMENTS";
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