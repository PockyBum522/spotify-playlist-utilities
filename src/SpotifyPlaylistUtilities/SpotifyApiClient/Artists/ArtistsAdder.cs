using Serilog;
using SpotifyAPI.Web;
using SpotifyPlaylistUtilities.Models;
using SpotifyPlaylistUtilities.Models.Serializable;
using SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Artists;

public class ArtistsAdder(ILogger _logger, PlaylistSearcher playlistSearcher, ClientManager spotifyClientManager, TracksAdder _tracksAdder)
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
        
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();
        
        foreach (var artistName in artistsList)
        {
            _logger.Information("Searching for artist: {ArtistName}", artistName);
            
            // Find artist ID
            var searchRequest = new SearchRequest(SearchRequest.Types.Artist, artistName);

            var artistSearchResponse = spotifyClient.Search.Item(searchRequest).Result;

            if (artistSearchResponse.Artists.Items is null ||
                artistSearchResponse.Artists.Items.Count < 1)
            {
                _logger.Information("Couldn't find anything matching artist:  {ArtistName}", artistName);
                
                continue;
            }

            var foundArtistId = artistSearchResponse.Artists.Items.First().Id;
            _logger.Information("Got ID: {ArtistId} for artist: {ArtistName}", foundArtistId, artistName);
            
            _logger.Information("About to query all tracks for artist ID: {ArtistId}", foundArtistId);
            
            // Search for all their songs - Daft Punk should be "4tZwfgrHOc3mvqYlEYSvVi"
            var allTracks = await getAllTracksForArtist(foundArtistId, artistName);          
            
            _logger.Information("About to add all found tracks to specified playlist: {PlaylistName}", playlistName);
            
            // Add each song to playlist
            await _tracksAdder.AddTracksToSpotifyPlaylist(spotifyPlaylist, allTracks);
        }
    }

    private async Task<List<SerializableManagedPlaylistTrack>> getAllTracksForArtist(string artistId, string artistName)
    {
        var returnTracks = new List<SerializableManagedPlaylistTrack>();
        
        var spotifyClient = await spotifyClientManager.GetSpotifyClient();
        
        var artistAlbums = await spotifyClient.PaginateAll(await spotifyClient.Artists.GetAlbums(artistId).ConfigureAwait(false));
        
        await Task.Delay(5000);         // Lazy rate-limiting
        
        foreach (var artistAlbum in artistAlbums)
        {
            var albumId = artistAlbum.Id;
            
            var albumTracks = await spotifyClient.PaginateAll(await spotifyClient.Albums.GetTracks(albumId).ConfigureAwait(false));
            
            await Task.Delay(5000);         // Lazy rate-limiting
            
            foreach (var albumTrack in albumTracks)
            {
                var foundArtistOnTrack = false;
                
                foreach (var trackArtist in albumTrack.Artists)
                {
                    if (trackArtist.Id == artistId)
                        foundArtistOnTrack = true;
                }

                if (foundArtistOnTrack)
                {
                    _logger.Information("Found valid track: {ArtistName} - {TrackName} [Adding track to internal list]", artistName, albumTrack.Name);
                    
                    returnTracks.Add(
                        new SerializableManagedPlaylistTrack()
                        {
                            Id = albumTrack.Id,
                            Name = albumTrack.Name,
                            Uri = albumTrack.Uri
                        });    
                }
            }
        }
        
        return returnTracks;
    }
}
