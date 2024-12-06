namespace SpotifyPlaylistUtilities.Models.Serializable;

public class SerializableManagedPlaylistTrack
{
    public string Id { get; set; } = "ERROR DESERIALIZING TRACK ID";
    public string Uri { get; set; } = "ERROR DESERIALIZING TRACK URI";
    public string Name { get; set; } = "ERROR DESERIALIZING TRACK NAME";
    public double PickWeight { get; set; }
    public int RandomShuffleNumber { get; private set; } = 0;
    
    //private readonly FullTrack _originalTrack;
}
