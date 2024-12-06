using Autofac;
using SpotifyPlaylistUtilities.Scheduler;
using SpotifyPlaylistUtilities.SpotifyApiClient.PlaylistBackups;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities;

internal static class Program
{
    internal static async Task Main()
    {
        var dependencyContainer = await DependencyInjectionRoot.GetBuiltContainer();
        await using var scope = dependencyContainer.BeginLifetimeScope();

        // await printAllPlaylistNamesAndIds(scope);

        // await restoreTracksFromJsonBackupFile(scope);
        
        // await inspectDeserializedJsonFileAsPlaylist(scope);     // You'll probably want to set a breakpoint in this method
        
        // Uncomment only one of these at a time
        await shufflePlaylistImmediatelyOnce(scope);
        // await startScheduler(scope);
        
        
    }
    
    private static Task shufflePlaylistImmediatelyOnce(ILifetimeScope scope)
    {
        
        
        return Task.CompletedTask;
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