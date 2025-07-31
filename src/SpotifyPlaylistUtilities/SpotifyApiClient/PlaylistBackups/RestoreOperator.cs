using Newtonsoft.Json;
using Serilog;
using SpotifyPlaylistUtilities.Models.Serializable;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.PlaylistBackups;

public class RestoreOperator(ILogger _logger, PlaylistSearcher playlistSearcher, TracksRemover _tracksRemover, TracksAdder _tracksAdder)
{
    public async Task RestorePlaylistFromJsonFile(string jsonFilePath)
    {
        var convertedPlaylist = await DeserializeOnlyFromJsonFile(jsonFilePath);
        
        var spotifyPlaylist = await playlistSearcher.GetPlaylistByName(convertedPlaylist.Name);
        
        if (spotifyPlaylist is null) throw new NullReferenceException(
            $"Could not find spotify playlist named: {convertedPlaylist.Name} in {nameof(RestorePlaylistFromJsonFile)}");

        if (spotifyPlaylist.Id != convertedPlaylist.Id)
            warnUserPlaylistIdsDiffer(convertedPlaylist.Name);
        
        await _tracksRemover.DeleteAllSpotifyPlaylistTracks(spotifyPlaylist);
        
        await _tracksAdder.AddTracksToSpotifyPlaylist(spotifyPlaylist, convertedPlaylist.FetchedTracks);
    }
    
    public async Task<SerializableManagedPlaylist> DeserializeOnlyFromJsonFile(string jsonFilePath)
    {
        var rawJson = await File.ReadAllTextAsync(jsonFilePath);

        var convertedPlaylist = JsonConvert.DeserializeObject<SerializableManagedPlaylist>(rawJson);
        
        if (convertedPlaylist is null) 
            throw new NullReferenceException($"convertedPlaylist is null in {nameof(RestorePlaylistFromJsonFile)}");

        return convertedPlaylist;
    }

    private void warnUserPlaylistIdsDiffer(string playlistName)
    {
        _logger.Warning("Although the names are the same, when trying to restore playlist '{PlaylistName}', the IDs " +
                        "are different in the backup JSON file and the playlist on spotify. This may be fine," +
                        "if you made a new playlist with the same name, but you may want to verify the restore is " +
                        "correct when it finishes", playlistName);
    }
}
