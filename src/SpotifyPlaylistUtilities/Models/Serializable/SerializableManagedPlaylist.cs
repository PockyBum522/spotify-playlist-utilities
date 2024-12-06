using Serilog;
using SpotifyAPI.Web;

namespace SpotifyPlaylistUtilities.Models.Serializable;

public class SerializableManagedPlaylist(ILogger logger, SpotifyClient spotifyClient, FullPlaylist nativePlaylist)
{
    public string Name { get; set; } = "ERROR DESERIALIZING PLAYLIST NAME"; 
    public string Id { get; set; } = "ERROR DESERIALIZING PLAYLIST ID";
    public List<SerializableManagedPlaylistTrack> FetchedTracks { get; set; } = []; 
}
