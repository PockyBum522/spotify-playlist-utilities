using Newtonsoft.Json;
using Serilog;
using SpotifyPlaylistUtilities.Models;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.PlaylistBackups;

public class BackupOperator(ILogger _logger)
{
    public async Task BackupTracksToJsonFile(SpotifyManagedPlaylist playlistToBackup)
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
}
