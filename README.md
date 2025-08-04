# spotify-playlists-shuffle-merge

Takes two large spotify playlists, basically pulls N songs from each (customizable) and then refreshes a destination playlist with those (N * 2) random songs. 

Good for running nightly to always have a shuffled selection of two people's playlists, or expand it to more playlists and always have a shuffled daily selection if you have a very large set of playlists for your library.

Names are for my specific playlists, change them for yours.


## Improvements

Needs a better paginator with a delay between requests, but works fine for now. However, this can drain your request limit quite fast, so consider using a custom paginator with delays. You can add one in each call to spotifyClient.PaginateAll()


# SECRETS.cs format

(I'm switching between two users depending on detected username. If you go to use this code, you can just switch to one client ID that you get from your spotify developer dashboard. All code to switch between users should be in ClientManager.cs)

```
namespace SpotifyPlaylistUtilities;

// ReSharper disable InconsistentNaming
public static class SECRETS
{
    public static string SPOTIFY_CLIENT_ID_DAVID => "36fadc3687";
    public static string SPOTIFY_CLIENT_ID_CEEZ => "a8fd9b67d9";
}
```
