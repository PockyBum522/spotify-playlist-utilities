namespace SpotifyPlaylistUtilities.SpotifyApiClient.Utilities;

public class FilenameSafer
{
    public string GetSafeFilename(string rawString)
    {
        var returnFilename = rawString;
        
        returnFilename = returnFilename.Replace(" ", "-");
        returnFilename = returnFilename.Replace("<", "_");
        returnFilename = returnFilename.Replace(">", "_");
        
        return returnFilename;
    }
}