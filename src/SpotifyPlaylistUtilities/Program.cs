using Autofac;
using SpotifyPlaylistUtilities.SpotifyApiClient.PlaylistBackups;

namespace SpotifyPlaylistUtilities;

internal static class Program
{
    internal static async Task Main()
    {
        var dependencyContainer = await DependencyInjectionRoot.GetBuiltContainer();
        await using var scope = dependencyContainer.BeginLifetimeScope();
        
        
        // Print all playlist names and ids:
        // var infoPrinter = scope.Resolve<InfoPrinter>();
        // await infoPrinter.PrintAllPlaylists();
        
        // Restore tracks from a JSON backup file:
        // var restoreOperator = scope.Resolve<RestoreOperator>();
        // await restoreOperator.RestorePlaylistFromJsonFile("/home/david/Desktop/Pixel Gardener/2024-12-02_T03-13-35_7pnXJ7jWswV32QGjJwyuFY.json");
    }
}