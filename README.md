# spotify-playlists-shuffle-merge

Takes two large spotify playlists, basically pulls N songs from each (customizable) and then refreshes a destination playlist with those (N * 2) random songs. 

Good for running nightly to always have a shuffled selection of two people's playlists, or expand it to more playlists and always have a shuffled daily selection if you have a very large set of playlists for your library.

Names are for my specific playlists, change them for yours.


## Improvements

Needs a better paginator with a delay between requests, but works fine for now. However, this can drain your request limit quite fast, so consider using a custom paginator with delays. You can add one in each call to spotifyClient.PaginateAll()


# SECRETS.cs format

```
// ReSharper disable InconsistentNaming because I like having secret constants be capitalized 
namespace SpotifyPlaylistUtilities;

public static class SECRETS
{
    public static string SPOTIFY_CLIENT_ID => "[The client ID you got from your spotify developer dashboard]";
}
