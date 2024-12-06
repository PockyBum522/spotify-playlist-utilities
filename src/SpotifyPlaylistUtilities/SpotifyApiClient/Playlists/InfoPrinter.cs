using Serilog;
using SpotifyAPI.Web;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class InfoPrinter(ILogger logger, ClientManager spotifyClientManager)
{
    public async Task PrintAllPlaylists()
    {
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();
        
        var playlists = await spotifyClient.PaginateAll(await spotifyClient.Playlists.CurrentUsers().ConfigureAwait(false));
        
        foreach (var playlist in playlists)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract because: nullable types are somehow lying, this absolutely happens
            if (playlist is null) continue;     
            
            logger.Information("In all playlists - Got: {PlaylistName} with ID: {PlaylistId}", playlist.Name, playlist.Id);
        }
    }
}
