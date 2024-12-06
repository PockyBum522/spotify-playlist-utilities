using Serilog;
using SpotifyAPI.Web;
using SpotifyPlaylistUtilities.Models;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class TracksAdder(ILogger logger, ClientManager spotifyClientManager)
{
    public async Task AddTracksToSpotifyPlaylist(ManagedPlaylist playlist, List<ManagedPlaylistTrack> tracks)
    {
        var itemsToAdd = new List<string>();
        
        foreach (var track in tracks)
        {
            logger.Debug("Attempting to add: {TrackName}", track.Name);
    
            itemsToAdd.Add(track.Uri);
    
            if (itemsToAdd.Count < 100) continue;
            
            // If we hit capacity, add and clear
            await add100ItemsToPlaylist(itemsToAdd, playlist.Id);
        }
        
        // Add any remaining
        if (itemsToAdd.Count > 0)
            await add100ItemsToPlaylist(itemsToAdd, playlist.Id);
    }
    
    private async Task add100ItemsToPlaylist(List<string> itemsToAdd, string playlistIdToAddTo)
    {
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();

        // More lazy rate-limiting
        await Task.Delay(2000);

        // Otherwise, since we can only add 100 at a time:
        var itemsToAddRequest = new PlaylistAddItemsRequest(itemsToAdd);

        await spotifyClient.Playlists.AddItems(playlistIdToAddTo, itemsToAddRequest);

        itemsToAdd.Clear();
    }
}
