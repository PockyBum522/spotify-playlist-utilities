using Serilog;
using SpotifyPlaylistUtilities.Models;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class PlaylistSearcher(ILogger logger, ClientManager spotifyClientManager)
{
    public async Task<SpotifyManagedPlaylist> GetPlaylistByName(string playlistName)
    {
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();
        
        var playlists = await spotifyClient.PaginateAll(await spotifyClient.Playlists.CurrentUsers().ConfigureAwait(false));
        
        foreach (var playlist in playlists)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract because: nullable types are somehow lying, this absolutely happens
            if (playlist is null) continue;     
            
            logger.Debug("In all playlists - Got: {PlaylistName} with ID: {PlaylistId} [Trying to match: {MatchName}]", playlist.Name, playlist.Id, playlistName);
        }
        
        foreach (var playlist in playlists)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract because: nullable types are somehow lying, this absolutely happens
            if (playlist is null) continue;     
            
            if (playlist.Name != playlistName) continue;

            var returnManagedPlaylist = new SpotifyManagedPlaylist(logger, spotifyClient, playlist);

            await returnManagedPlaylist.FetchAllTracks();
            
            return returnManagedPlaylist;
        }
        
        throw new ArgumentException("Couldn't find playlist with name of: {SuppliedName}", playlistName);
    }
    
    public async Task<SpotifyManagedPlaylist> GetPlaylistById(string playlistId)
    {
        // Lazy rate-limiting because I do not care how long this takes
        await Task.Delay(2000);
        
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();
        
        await Task.Delay(2000);
        
        var playlists = await spotifyClient.PaginateAll(await spotifyClient.Playlists.CurrentUsers().ConfigureAwait(false));
        
        await Task.Delay(2000);
        
        foreach (var playlist in playlists)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract because: nullable types are somehow lying, this absolutely happens
            if (playlist is null) continue;     
            
            logger.Debug("In all playlists - Got: {PlaylistName} with ID: {PlaylistId} [Trying to match: {MatchId}]", playlist.Name, playlist.Id, playlistId);
        }
        
        foreach (var playlist in playlists)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract because: nullable types are somehow lying, this absolutely happens
            if (playlist is null) continue;     

            var returnManagedPlaylist = new SpotifyManagedPlaylist(logger, spotifyClient, playlist);

            await returnManagedPlaylist.FetchAllTracks();
            
            return returnManagedPlaylist;
        }
        
        throw new ArgumentException("Couldn't find playlist with name of: {SuppliedName}", playlistId);
    }
}
