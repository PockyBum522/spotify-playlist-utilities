using System.Security.Cryptography;
using Newtonsoft.Json;
using Serilog;
using SpotifyPlaylistUtilities.Models;
using SpotifyPlaylistUtilities.Models.Serializable;
using SpotifyPlaylistUtilities.SpotifyApiClient.PlaylistBackups;
using SpotifyPlaylistUtilities.SpotifyApiClient.WeightsFile;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.Playlists;

public class Shuffler(ILogger _logger, Searcher _searcher, BackupOperator _backupOperator, TracksRemover _tracksRemover, TracksAdder _tracksAdder, WeightsFileManager _weightsReader, WeightsFileManager _weightsManager)
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
    
    public async Task MakeSelectDaily()
    {
        var spotifySelections = await _searcher.GetPlaylistByName("Select Selections");
        var spotifyCurated = await _searcher.GetPlaylistByName("Curated Weebletdays");
        var spotifySelectDaily = await _searcher.GetPlaylistByName("Weebletdays Select Daily");
        
        var selectSelectionsRandom = randomizeTracksOrder(spotifySelections.FetchedTracks);
        var curatedRandom = randomizeTracksOrder(spotifyCurated.FetchedTracks);

        _logger.Information("Curated Weebletdays track count: {CuratedCount}, Select Selections track count: {SelectCount}", curatedRandom.Count, selectSelectionsRandom.Count);
        
        var trimmedCuratedRandom = removeIdenticalTracks(selectSelectionsRandom, curatedRandom);

        _logger.Information("Trimmed Curated Weebletdays track count: {CuratedCount}", trimmedCuratedRandom.Count);

        var selectionsToMerge = getPlaylistTracksWithWeightsConsidered("Weebletdays Select Daily", selectSelectionsRandom, 200);
        var curatedToMerge = getPlaylistTracksWithWeightsConsidered("Weebletdays Select Daily", trimmedCuratedRandom, 200);
        
        var allTracks = selectionsToMerge.Concat(curatedToMerge).ToList();
        
        var randomizedDaily = randomizeTracksOrder(allTracks);
        
        await _tracksRemover.DeleteAllSpotifyPlaylistTracks(spotifySelectDaily);

        await _tracksAdder.AddTracksToSpotifyPlaylist(spotifySelectDaily, allTracks);
    }

    private List<SerializableManagedPlaylistTrack> removeIdenticalTracks(List<SerializableManagedPlaylistTrack> tracksToRemove, List<SerializableManagedPlaylistTrack> removeFrom)
    {
        var returnTracks = new List<SerializableManagedPlaylistTrack>();
        
        foreach (var track in removeFrom)
        {
            if (tracksToRemove.Any(t => t.Id == track.Id))
                continue;
                
            returnTracks.Add(track);
        }

        return returnTracks;
    }

    private List<SerializableManagedPlaylistTrack> getPlaylistTracksWithWeightsConsidered(string playlistName, List<SerializableManagedPlaylistTrack> allTracksRandomized, int count)
    {
        var returnTracks = new List<SerializableManagedPlaylistTrack>();

        for (var i = 0; i < allTracksRandomized.Count; i++)
        {
            var track = allTracksRandomized[i];
            
            var trackWeight = _weightsReader.GetTrackWeight(playlistName, track.Id);

            var randomDie = RandomNumberGenerator.GetInt32(0, 100);
            
            _logger.Information("\nRolling: {Die} for track weight: {Weight} on track: {Name}", randomDie, trackWeight, track.Name);
            
            if (randomDie < trackWeight) continue;

            // Otherwise, if we rolled OVER track weight:
            _logger.Information("Adding track!");

            returnTracks.Add(track);

            _weightsManager.IncrementTrackWeight(playlistName, track);
            
            _logger.Information(
                "Now incrementing weight for track {Name}, new weight is {NewWeight}",
                track.Name, _weightsManager.GetTrackWeight(playlistName, track.Id));
            
            if (--count < 1) break;
        }

        _weightsManager.DecrementAllTracks(playlistName);
        
        return returnTracks;
    }
    
    private List<SerializableManagedPlaylistTrack> randomizeTracksOrder(List<SpotifyManagedPlaylistTrack> spotifyPlaylistFetchedTracks)
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

        var numberOfTracks = randomizedTracks.Count;
        
        while (numberOfTracks-- > 0)
            returnTracks.Add(randomizedTracks[numberOfTracks]);
        
        return returnTracks;
    }
    
    private List<SerializableManagedPlaylistTrack> randomizeTracksOrder(List<SerializableManagedPlaylistTrack> spotifyPlaylistFetchedTracks)
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

        var numberOfTracks = randomizedTracks.Count;
        
        while (numberOfTracks-- > 0)
            returnTracks.Add(randomizedTracks[numberOfTracks]);
        
        return returnTracks;
    }
}
