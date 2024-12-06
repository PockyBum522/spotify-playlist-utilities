using Newtonsoft.Json;
using SpotifyPlaylistUtilities.Models.Serializable;
using SpotifyPlaylistUtilities.SpotifyApiClient.Utilities;

namespace SpotifyPlaylistUtilities.SpotifyApiClient.WeightsFile;

public class WeightsFileManager(FilenameSafer _filenameSafer)
{
    private JsonSerializerSettings _jsonSerializerSettings = new()
    {
        DefaultValueHandling = DefaultValueHandling.Include,
        TypeNameHandling = TypeNameHandling.None,
        Formatting = Formatting.Indented
    };

    public int GetTrackWeight(string playlistName, string trackId)
    {
        var fullPath = Path.Join(AppInfo.Paths.TrackWeightsDirectory, $"{_filenameSafer.GetSafeFilename(playlistName)}_{Path.GetFileName(AppInfo.Paths.TrackWeightsJsonFullPath)}");
        ensureFileExists(fullPath);
        
        var weights = getAllTrackWeights(playlistName);

        var track = weights.FirstOrDefault(t => t.Id == trackId);
        
        return track?.Weight ?? 1;
    }
    
    public void IncrementTrackWeight(string playlistName, SerializableManagedPlaylistTrack trackToIncrement)
    {
        var trackWeights = getAllTrackWeights(playlistName);

        var trackInWeights = trackWeights.Where(t => t.Id == trackToIncrement.Id);

        if (!trackInWeights.Any())
        {
            // If we didn't fetch it, then it doesn't exist in the file. Add a default 
            trackWeights.Add(new WeightedTrack()
            {
                Id = trackToIncrement.Id, 
                Weight = 10, 
                Name = trackToIncrement.Name,
                TotalPickCount = 1
            });
        }
        else
        {
            for (var i = 0; i < trackWeights.Count; i++)
            {
                var track = trackWeights[i];

                if (track.Id == trackToIncrement.Id)
                {
                    track.Weight += 10;
                    track.TotalPickCount++;
                }
                
                if (track.Weight > 100) track.Weight = 100;
            }
        }

        saveWeightsToFile(playlistName, trackWeights);
    }

    public void DecrementAllTracks(string playlistName)
    {
        var trackWeights = getAllTrackWeights(playlistName);
        
        for (var i = 0; i < trackWeights.Count; i++)
        {
            var track = trackWeights[i];

            track.Weight -= 1;
            
            if (track.Weight < 1) track.Weight = 1;
        }
        
        saveWeightsToFile(playlistName, trackWeights);
    }

    private void saveWeightsToFile(string playlistName, List<WeightedTrack> trackWeights)
    {
        var fullPath = Path.Join(AppInfo.Paths.TrackWeightsDirectory, $"{_filenameSafer.GetSafeFilename(playlistName)}_{Path.GetFileName(AppInfo.Paths.TrackWeightsJsonFullPath)}");
        ensureFileExists(fullPath);
        
        var json = JsonConvert.SerializeObject(trackWeights, _jsonSerializerSettings);
        File.WriteAllText(fullPath, json);
    }

    private void ensureFileExists(string fullPath)
    {
        // Make sure file exists, create if no
        if (!File.Exists(fullPath))
        {
            var minJson = JsonConvert.SerializeObject(new List<WeightedTrack>(), _jsonSerializerSettings);
            File.WriteAllText(fullPath, minJson);
        }
    }

    private List<WeightedTrack> getAllTrackWeights(string playlistName)
    {
        var jsonSerializerSettings = new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Include,
            TypeNameHandling = TypeNameHandling.None,
            Formatting = Formatting.Indented
        };

        var fullPath = 
            Path.Join(
                AppInfo.Paths.TrackWeightsDirectory, 
                $"{_filenameSafer.GetSafeFilename(playlistName)}_{Path.GetFileName(AppInfo.Paths.TrackWeightsJsonFullPath)}");
        
        ensureFileExists(fullPath);
        
        // Open file
        var fileJson = File.ReadAllText(fullPath);

        // Deserialize file to list
        return JsonConvert.DeserializeObject<List<WeightedTrack>>(fileJson, jsonSerializerSettings) ?? [];
    }
}