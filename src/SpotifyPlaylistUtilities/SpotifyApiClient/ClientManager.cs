using System.Security.Authentication;
using Newtonsoft.Json;
using Serilog;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace SpotifyPlaylistUtilities.SpotifyApiClient;

public class ClientManager(ILogger _logger)
{
    private SpotifyClient? _spotifyClient;
    private readonly EmbedIOAuthServer _oAuthServer = new(new Uri("http://localhost:5543/callback"), 5543);

    /// <summary>
    /// This is the only way to access the spotify client. Every time this is called, the client is tested,
    /// and always gives back a connected, authenticated client or throws
    /// </summary>
    /// <returns>Connected, authenticated spotify client</returns>
    /// <exception cref="NullReferenceException">If there was a fatal problem setting the client up</exception>
    public async Task<SpotifyClient> GetSpotifyClient()
    {
        var clientWasAlreadyConnected = await ensureClientIsConnected();
        
        if (_spotifyClient is null) throw new NullReferenceException($"_spotifyClient was null after setup in {nameof(ClientManager)}");
        
        // Lazy rate-limiting because I do not care how long this takes;
        //      and we don't have to do it if we didn't perform any API operations because it was already connected
        if (!clientWasAlreadyConnected)
            await Task.Delay(2000);
        
        return _spotifyClient;
    }

    private async Task<bool> ensureClientIsConnected()
    {
        _logger.Information("Checking for credentials file at: {CredentialsFullPath}", AppInfo.Paths.CredentialsFullPath);
        
        if (!File.Exists(AppInfo.Paths.CredentialsFullPath))
            await completeOAuth();

        var clientWasAlreadyConnected = await spotifyClientIsReady(); 
        
        if (!clientWasAlreadyConnected)
            await connectNewSpotifyClient();

        return clientWasAlreadyConnected;
    }

    private async Task connectNewSpotifyClient()
    {
        // Configure spotify client now that we've authed
        var credentialsJson = await File.ReadAllTextAsync(AppInfo.Paths.CredentialsFullPath);
        var oAuthToken = JsonConvert.DeserializeObject<PKCETokenResponse>(credentialsJson);

        var pkceAuthenticator = new PKCEAuthenticator(SECRETS.SPOTIFY_CLIENT_ID_DAVID, oAuthToken!);
        pkceAuthenticator.TokenRefreshed += (_, refreshedToken) => File.WriteAllText(AppInfo.Paths.CredentialsFullPath, JsonConvert.SerializeObject(refreshedToken));

        var config = SpotifyClientConfig.CreateDefault().WithAuthenticator(pkceAuthenticator);
        
        _spotifyClient = new SpotifyClient(config);
        
        if (_spotifyClient is null)
            throw new NullReferenceException($"_spotifyClient was not set up in {nameof(ClientManager)} and is null");
    }

    private async Task<bool> spotifyClientIsReady()
    {
        if (_spotifyClient is null) return false;
        
        var clientIsReady = false;

        try
        {
            // Quick check that everything is behaving
            var me = await _spotifyClient.UserProfile.Current();
            _logger.Information("Welcome {DisplayName} with User ID:({Id}), you're authenticated!", me.DisplayName, me.Id);

            await Task.Delay(2000);
            
            clientIsReady = true;
        }
        catch (AuthenticationException aex)
        {
            _logger.Warning(aex, "Authentication exception when testing spotify client. This *may* be okay");
        }
        
        return clientIsReady;
    }

    private async Task completeOAuth()
    {
        if (string.IsNullOrEmpty(SECRETS.SPOTIFY_CLIENT_ID_DAVID))
            throw new NullReferenceException("Please set SPOTIFY_CLIENT_ID_DAVID via SECRETS.cs before starting the program");
        
        var completedAuth = false;
        
        var (verifier, challenge) = PKCEUtil.GenerateCodes();

        await _oAuthServer.Start();
        _oAuthServer.AuthorizationCodeReceived += async (_, response) =>
        {
            await _oAuthServer.Stop();
            var token = await new OAuthClient().RequestToken(
                new PKCETokenRequest(SECRETS.SPOTIFY_CLIENT_ID_DAVID, response.Code, _oAuthServer.BaseUri, verifier)
            );

            await File.WriteAllTextAsync(AppInfo.Paths.CredentialsFullPath, JsonConvert.SerializeObject(token));
            
            completedAuth = true;
        };

        var request = new LoginRequest(_oAuthServer.BaseUri, SECRETS.SPOTIFY_CLIENT_ID_DAVID, LoginRequest.ResponseType.Code)
        {
            CodeChallenge = challenge,
            CodeChallengeMethod = "S256",
            Scope = new List<string> { "user-read-email", "user-read-private", "playlist-read-private", "playlist-read-collaborative", "playlist-modify-private", "playlist-modify-public" }
        };

        var uri = request.ToUri();
        
        try
        {
            BrowserUtil.Open(uri);
        }
        catch (Exception)
        {
            _logger.Information("Unable to open URL, manually open: {Uri}", uri);
        }

        // Wait until auth is done so program doesn't try to continue without auth
        while (!completedAuth)
        {
            await Task.Delay(500);
        }
        
        if (!File.Exists(AppInfo.Paths.CredentialsFullPath))
        {
            throw new AuthenticationException(
                $"Could not find credentials file after authentication: {AppInfo.Paths.CredentialsFullPath}");
        }
    }
}