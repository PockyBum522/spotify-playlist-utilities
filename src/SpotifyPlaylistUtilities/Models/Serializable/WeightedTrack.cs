namespace SpotifyPlaylistUtilities.Models.Serializable;

public class WeightedTrack()
{
    public string Id { get; set; } = "ERROR GETTING TRACK ID";
    public string Name { get; set; } = "ERROR GETTING TRACK NAME";

    public int TotalPickCount { get; set; } = 0;
    
    public int Weight { get; set; } = 1;
}