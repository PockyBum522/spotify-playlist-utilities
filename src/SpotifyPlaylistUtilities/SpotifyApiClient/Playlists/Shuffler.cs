using System.Security.Cryptography;
using Serilog;
using SpotifyPlaylistUtilities.Models;
using SpotifyPlaylistUtilities.Models.Serializable;
using SpotifyPlaylistUtilities.SpotifyApiClient.PlaylistBackups;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class Shuffler(ILogger _logger, BackupOperator _backupOperator, TracksRemover _tracksRemover, TracksAdder _tracksAdder)
{
    /// <summary>
    /// Gets all tracks in a playlist, backs them up, removes them, then re-adds them all in a random order
    /// </summary>
    /// <param name="spotifyPlaylist">ManagedPlaylist to shuffle all tracks in</param>
    /// <param name="allowDuplicates">Whether to allow duplicate tracks in the shuffled list of tracks, if the original playlist had duplicates</param>
    /// <param name="backupPlaylist">Whether to backup the playlist tracks to disk, true by default, highly recommended</param>
    public async Task ShuffleAllIn(SpotifyManagedPlaylist spotifyPlaylist, bool allowDuplicates, bool backupPlaylist = true)
    {
        if (backupPlaylist)
            await _backupOperator.BackupTracksToJsonFile(spotifyPlaylist);
        
        var allTracksShuffled = 
            randomizeTracksOrder(spotifyPlaylist.FetchedTracks);

        if (!allowDuplicates)
            allTracksShuffled = allTracksShuffled.DistinctBy(t => t.Id).ToList();
        
        await _tracksRemover.DeleteAllSpotifyPlaylistTracks(spotifyPlaylist);
        
        await _tracksAdder.AddTracksToSpotifyPlaylist(spotifyPlaylist, allTracksShuffled);
    }
    
    // private List<SerializableManagedPlaylistTrack> getRandomTracksConsideringWeights(List<SpotifyManagedPlaylistTrack> spotifyPlaylistFetchedTracks, int numberOfTracksToGet)
    // {
    //     // Make sure track weights file exists, if not, create it
    //     
    //     // Grab a random track from spotifyPlaylistFetchedTracks (MAKE SURE THERE'S A GOOD TRUE RANDOM WAY TO DO THIS)
    //     
    //     // Get that track's weight in the weights list JSON
    //     
    //     // Roll to see if the track sticks
    //     
    //     //      If it gets OVER the weight, add it to return tracks and increment weight in json weights file
    //     
    //     //      If it gets UNDER the weight, do nothing
    //     
    //     
    //     // Decrement all track weights in weights file when finished rolling
    // }
    
    private List<SerializableManagedPlaylistTrack> randomizeTracksOrder(List<SpotifyManagedPlaylistTrack> spotifyPlaylistFetchedTracks, int? numberOfTracks = null)
    {
        var randomizedTracks = new List<SerializableManagedPlaylistTrack>();

        for (var i = 0; i < spotifyPlaylistFetchedTracks.Count; i++)
        {
            var track = spotifyPlaylistFetchedTracks[i];
            var returnTrack = new SerializableManagedPlaylistTrack()
            {
                Id = track.Id,
                Name = track.Name,
                Uri = track.Uri,
                RandomShuffleNumber = RandomNumberGenerator.GetInt32(0, 10000000)
            };

            randomizedTracks.Add(returnTrack);
        }
        
        randomizedTracks = randomizedTracks.OrderBy(t => t.RandomShuffleNumber).ToList();
        
        var returnTracks = new List<SerializableManagedPlaylistTrack>();

        numberOfTracks ??= randomizedTracks.Count;
        
        while (numberOfTracks-- > 0)
        {
            returnTracks.Add(randomizedTracks[(int)numberOfTracks]);
        }
        
        return returnTracks;
    }
}
