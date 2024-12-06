using Serilog;
using SpotifyAPI.Web;
using SpotifyPlaylistUtilities.Models;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class TracksAdder(ILogger logger, ClientManager spotifyClientManager)
{
    public async Task AddTracksToSpotifyPlaylist(ManagedPlaylist spotifyPlaylist, List<ManagedPlaylistTrack> tracksToAdd)
    {
        var urisToAdd = new List<string>();
        
        foreach (var track in tracksToAdd)
        {
            logger.Debug("Attempting to add: {TrackName}", track.Name);
    
            urisToAdd.Add(track.Uri);
    
            if (urisToAdd.Count < 100) continue;
            
            // If we hit capacity, add and clear
            await add100ItemsToPlaylist(spotifyPlaylist, urisToAdd);
        }
        
        // Add any remaining
        if (urisToAdd.Count > 0)
            await add100ItemsToPlaylist(spotifyPlaylist, urisToAdd);
    }
    
    private async Task add100ItemsToPlaylist(ManagedPlaylist spotifyPlaylist, List<string> urisToAdd)
    {
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();
        
        // Otherwise, since we can only add 100 at a time:
        var tracksAddRequest = new PlaylistAddItemsRequest(urisToAdd);

        await spotifyClient.Playlists.AddItems(spotifyPlaylist.Id, tracksAddRequest);

        urisToAdd.Clear();
        
        // Lazy rate-limiting because I do not care how long this takes, not to mention track deletes are slow anyways
        await Task.Delay(2000);
    }
}
