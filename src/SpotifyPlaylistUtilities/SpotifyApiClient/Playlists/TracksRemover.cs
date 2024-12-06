using Serilog;
using SpotifyAPI.Web;
using SpotifyPlaylistUtilities.Models;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class TracksRemover(ILogger logger, ClientManager spotifyClientManager)
{
    public async Task DeleteAllSpotifyPlaylistTracks(ManagedPlaylist playlistToClear)
    {
        var itemsToRemove = new List<string>();
        
        foreach (var trackToRemove in playlistToClear.FetchedTracks)
        {
            logger.Debug("Attempting to remove track: {TrackName} from playlist: {PlaylistName}", trackToRemove.Name, playlistToClear.Name);

            if (string.IsNullOrWhiteSpace(trackToRemove.Uri))
                throw new ArgumentException("Track URI was empty");
            
            itemsToRemove.Add(trackToRemove.Uri);
    
            if (itemsToRemove.Count < 100) continue;
            
            // If we hit capacity, add and clear
            await remove100ItemsFrom(playlistToClear, itemsToRemove);
        }
         
        if (itemsToRemove.Count > 0)
            await remove100ItemsFrom(playlistToClear, itemsToRemove);
    }

    private async Task remove100ItemsFrom(ManagedPlaylist playlist, List<string> itemsToRemove)
    {
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();

        // More lazy rate-limiting
        await Task.Delay(2000);
            
        // Otherwise, since we can only remove 100 at a time:
        var itemsToRemoveRequest = new PlaylistRemoveItemsRequest();
    
        itemsToRemoveRequest.Tracks = new List<PlaylistRemoveItemsRequest.Item>();
        
        foreach (var itemToRemove in itemsToRemove)
        {
            itemsToRemoveRequest.Tracks.Add(
                new PlaylistRemoveItemsRequest.Item()
                {
                    Uri = itemToRemove
                });
        }
        
        await spotifyClient.Playlists.RemoveItems(playlist.Id, itemsToRemoveRequest);
    
        itemsToRemove.Clear();
    }
}
