using Serilog;
using SpotifyPlaylistUtilities.Models;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Tracks;

public class TracksSearcher(ILogger logger, ClientManager spotifyClientManager)
{
    public async Task<List<SpotifyManagedPlaylistTrack>> GetAllTracksForArtist(string artistName)
    {
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();
        
        var returnTracks = new List<SpotifyManagedPlaylistTrack>();
        
        
        
        return returnTracks;
    }
}