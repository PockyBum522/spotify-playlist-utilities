using Serilog;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Artists;

public class ArtistsAdder(ILogger _logger, PlaylistSearcher playlistSearcher, TracksRemover _tracksRemover, TracksAdder _tracksAdder)
{
    public async Task AddFromFileToPlaylistNamed(string artistsFilePath, string playlistName)
    {
        var spotifyPlaylist = await playlistSearcher.GetPlaylistByName(playlistName);
        
        if (spotifyPlaylist is null) throw new NullReferenceException(
            $"Could not find spotify playlist named: {playlistName} in {nameof(AddFromFileToPlaylistNamed)}");
        
        // Maybe only enable this if ceez explicitly asks for it
        // await _tracksRemover.DeleteAllSpotifyPlaylistTracks(spotifyPlaylist);
        
        // ReSharper disable once MethodHasAsyncOverload because performance reeeeally does not matter here
        var artistsList = File.ReadAllLines(artistsFilePath);
        
        foreach (var artistName in artistsList)
        {
            // Search for all their songs
            
            
            
            // Add each song to playlist
            
        }
        
        //await _tracksAdder.AddTracksToSpotifyPlaylist(spotifyPlaylist, convertedPlaylist.FetchedTracks);
    }
}
