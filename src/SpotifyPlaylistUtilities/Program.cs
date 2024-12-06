using Autofac;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities;

internal static class Program
{
    internal static async Task Main()
    {
        var dependencyContainer = await DependencyInjectionRoot.GetBuiltContainer();
        await using var scope = dependencyContainer.BeginLifetimeScope();
        
        
        // Print all playlist names and ids
        // var infoPrinter = scope.Resolve<InfoPrinter>();
        // await infoPrinter.PrintAllPlaylists();
        
        
    }
}