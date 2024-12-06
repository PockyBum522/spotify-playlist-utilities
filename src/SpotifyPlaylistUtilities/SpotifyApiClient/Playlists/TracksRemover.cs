using Serilog;
using SpotifyAPI.Web;
using SpotifyPlaylistUtilities.Models;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class TracksRemover(ILogger logger, ClientManager spotifyClientManager)
{
    public async Task DeleteAllSpotifyPlaylistTracks(SpotifyManagedPlaylist spotifyPlaylistToClear)
    {
        var urisToRemove = new List<string>();
        
        foreach (var trackToRemove in spotifyPlaylistToClear.FetchedTracks)
        {
            logger.Debug("Attempting to remove track: {TrackName} from playlist: {PlaylistName}", trackToRemove.Name, spotifyPlaylistToClear.Name);

            if (string.IsNullOrWhiteSpace(trackToRemove.Uri))
                throw new ArgumentException("Track URI was empty");
            
            urisToRemove.Add(trackToRemove.Uri);
    
            if (urisToRemove.Count < 100) continue;
            
            // If we hit capacity, add and clear
            await remove100ItemsFrom(spotifyPlaylistToClear, urisToRemove);
        }
         
        if (urisToRemove.Count > 0)
            await remove100ItemsFrom(spotifyPlaylistToClear, urisToRemove);
    }

    private async Task remove100ItemsFrom(SpotifyManagedPlaylist spotifyPlaylist, List<string> urisToRemove)
    {
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();

        // Otherwise, since we can only remove 100 at a time:
        var itemsToRemoveRequest = new PlaylistRemoveItemsRequest
        {
            Tracks = new List<PlaylistRemoveItemsRequest.Item>()
        };

        foreach (var itemToRemove in urisToRemove)
        {
            itemsToRemoveRequest.Tracks.Add(
                new PlaylistRemoveItemsRequest.Item()
                {
                    Uri = itemToRemove
                });
        }
        
        await spotifyClient.Playlists.RemoveItems(spotifyPlaylist.Id, itemsToRemoveRequest);
    
        urisToRemove.Clear();
        
        // Lazy rate-limiting because I do not care how long this takes, not to mention track deletes are slow anyways
        await Task.Delay(2000);
    }
}
