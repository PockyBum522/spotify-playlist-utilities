using SpotifyAPI.Web;

namespace SpotifyPlaylistUtilities.Models;

public class SpotifyManagedPlaylistTrack(FullTrack originalTrack)
{
    public string Id => originalTrack.Id;
    public string Uri => originalTrack.Uri;
    public string Name => originalTrack.Name;
}
