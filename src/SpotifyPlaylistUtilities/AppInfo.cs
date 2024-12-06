using System.Runtime.InteropServices;

namespace SpotifyPlaylistUtilities;

public static class AppInfo
{
    public const string AppName = "SpotifyPlaylistUtilities";

    public static class Paths
    {
        static Paths()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var basePath = @"C:\Users\Public\Documents";

                basePath = Path.Combine(basePath, AppName);

                setAllPaths(basePath);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var basePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), AppName);

                setAllPaths(basePath);
            }

            if (string.IsNullOrWhiteSpace(PlaylistCountsLoggingDirectory) ||
                string.IsNullOrWhiteSpace(PlaylistBackupsDirectory) ||
                string.IsNullOrWhiteSpace(ApplicationLoggingDirectory) ||
                string.IsNullOrWhiteSpace(TrackWeightsDirectory) ||
                string.IsNullOrWhiteSpace(TrackWeightsJsonFullPath))
            {
                throw new Exception("OS Could not be detected automatically");
            }
        }

        private static void setAllPaths(string basePath)
        {
            var logBasePath = Path.Join(basePath, "Logs");
            
            ApplicationLoggingDirectory = Path.Join(logBasePath, "application");
            PlaylistCountsLoggingDirectory = Path.Join(logBasePath, "playlist-counts");
            PlaylistBackupsDirectory = Path.Join(basePath, "playlist-backups");
            TrackWeightsDirectory = Path.Join(basePath, "track-weights");
            CacheDirectory = Path.Join(basePath, "cache");
            
            TrackWeightsJsonFullPath = Path.Join(basePath, "known-track-weights.json");
            CredentialsFullPath = Path.Join(CacheDirectory, "credentials.json");
            
            Directory.CreateDirectory(PlaylistCountsLoggingDirectory);
            Directory.CreateDirectory(ApplicationLoggingDirectory);
            Directory.CreateDirectory(PlaylistBackupsDirectory);
            Directory.CreateDirectory(TrackWeightsDirectory);
            Directory.CreateDirectory(CacheDirectory);
        }

        public static string CacheDirectory { get; private set; }
        public static string PlaylistCountsLoggingDirectory { get; private set; }
        public static string ApplicationLoggingDirectory { get; private set; }
        public static string PlaylistBackupsDirectory { get; private set; }
        public static string TrackWeightsDirectory { get; private set; }
        
        public static string TrackWeightsJsonFullPath { get; private set; }
        public static string CredentialsFullPath { get; private set; }
    }
}