using Serilog;
using SpotifyPlaylistUtilities.Models;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class Searcher(ILogger logger, ClientManager spotifyClientManager)
{
    public async Task<ManagedPlaylist> GetPlaylistByName(string playlistName)
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

            var returnManagedPlaylist = new ManagedPlaylist(logger, spotifyClient, playlist);

            await returnManagedPlaylist.FetchAllTracks();
            
            return returnManagedPlaylist;
        }
        
        throw new ArgumentException("Couldn't find playlist with name of: {SuppliedName}", playlistName);
    }
    
    public async Task<ManagedPlaylist> GetPlaylistById(string playlistId)
    {
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();
        
        var playlists = await spotifyClient.PaginateAll(await spotifyClient.Playlists.CurrentUsers().ConfigureAwait(false));
        
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

            var returnManagedPlaylist = new ManagedPlaylist(logger, spotifyClient, playlist);

            await returnManagedPlaylist.FetchAllTracks();
            
            return returnManagedPlaylist;
        }

        // Lazy rate limiting, sort of
        await Task.Delay(2000);
        
        throw new ArgumentException("Couldn't find playlist with name of: {SuppliedName}", playlistId);
    }
    
    public async Task PrintAllPlaylists()
    {
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();
        
        var playlists = await spotifyClient.PaginateAll(await spotifyClient.Playlists.CurrentUsers().ConfigureAwait(false));
        
        foreach (var playlist in playlists)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract because: nullable types are somehow lying, this absolutely happens
            if (playlist is null) continue;     
            
            logger.Information("In all playlists - Got: {PlaylistName} with ID: {PlaylistId}", playlist.Name, playlist.Id);
        }
    }
}
