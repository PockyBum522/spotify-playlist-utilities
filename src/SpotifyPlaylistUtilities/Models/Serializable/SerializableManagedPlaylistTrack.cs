namespace SpotifyPlaylistUtilities.Models.Serializable;

public class SerializableManagedPlaylistTrack
{
    public string Id { get; set; } = "ERROR DESERIALIZING TRACK ID";
    public string Uri { get; set; } = "ERROR DESERIALIZING TRACK URI";
    public string Name { get; set; } = "ERROR DESERIALIZING TRACK NAME";

    public int PickWeight { get; set; } = 1;
    
    public int RandomShuffleNumber { get; set; }
    
    //private readonly FullTrack _originalTrack;
}
