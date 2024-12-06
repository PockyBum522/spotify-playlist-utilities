using Newtonsoft.Json;
using Serilog;
using SpotifyPlaylistUtilities.Models;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class BackupOperator(ILogger _logger, Searcher _searcher, TracksRemover _tracksRemover, TracksAdder _tracksAdder)
{
    public async Task BackupTracksToJsonFile(ManagedPlaylist playlistToBackup)
    {
        var jsonString = JsonConvert.SerializeObject(playlistToBackup);
    
        var safePlaylistName = playlistToBackup.Name;

        foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
            safePlaylistName = safePlaylistName.Replace(invalidFileNameChar.ToString(), "_");
        
        var playlistBackupFolderPath = Path.Join(AppInfo.Paths.PlaylistBackupsDirectory, safePlaylistName);
    
        Directory.CreateDirectory(playlistBackupFolderPath);
        
        var filename =
            DateTimeOffset.Now.ToString("s")
                .Replace("T", "_T")
                .Replace(":", "-")
            + $"_{playlistToBackup.Id}.json";
    
        var fullFilePath = Path.Join(playlistBackupFolderPath, filename);
        
        await File.WriteAllTextAsync(fullFilePath, jsonString);
    }
    
    public async Task RestorePlaylistFromJsonFile(string jsonFilePath)
    {
        var rawJson = await File.ReadAllTextAsync(jsonFilePath);

        var convertedPlaylist = JsonConvert.DeserializeObject<ManagedPlaylist>(rawJson);
        
        if (convertedPlaylist is null) 
            throw new NullReferenceException($"convertedPlaylist is null in {nameof(RestorePlaylistFromJsonFile)}");

        var spotifyPlaylist = await _searcher.GetPlaylistByName(convertedPlaylist.Name);
        
        if (spotifyPlaylist is null) throw new NullReferenceException(
            $"Could not find spotify playlist named: {convertedPlaylist.Name} in {nameof(RestorePlaylistFromJsonFile)}");

        if (spotifyPlaylist.Id != convertedPlaylist.Id)
            warnUserPlaylistIdsDiffer(convertedPlaylist.Name);
        
        await _tracksRemover.DeleteAllSpotifyPlaylistTracks(spotifyPlaylist);
        
        await _tracksAdder.AddTracksToSpotifyPlaylist(spotifyPlaylist, convertedPlaylist.FetchedTracks);
    }

    private void warnUserPlaylistIdsDiffer(string playlistName)
    {
        _logger.Warning("Although the names are the same, when trying to restore playlist '{PlaylistName}', the IDs " +
                        "are different in the backup JSON file and the playlist on spotify. This may be fine," +
                        "if you made a new playlist with the same name, but you may want to verify the restore is " +
                        "correct when it finishes", playlistName);
    }
}
