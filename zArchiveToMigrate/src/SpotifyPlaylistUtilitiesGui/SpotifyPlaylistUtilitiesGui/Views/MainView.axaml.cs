using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Serilog;
using SpotifyAPI.Web;
using SpotifyPlaylistUtilities;
using SpotifyPlaylistUtilities.Logging;
using SpotifyPlaylistUtilities.Models;
using SpotifyPlaylistUtilities.Playlists;

namespace SpotifyPlaylistUtilitiesGui.Views;

public partial class MainView : UserControl
{
    private readonly ILogger _logger;

    public MainView()
    {
        InitializeComponent();
        
        var loggerConfiguration = LoggerSetup.ConfigureLogger();
        
        LoggerSetup.Logger = loggerConfiguration
            .MinimumLevel.Debug()
            .CreateLogger();

        _logger = LoggerSetup.Logger ?? throw new NullReferenceException();
        
        Task.Run(async () =>
        {
            await setupQuartzSchedulerAndJobs();
        });
    }

    private async Task setupQuartzSchedulerAndJobs()
    {
        LogProvider.SetCurrentLogProvider(new CustomSerilogLogProvider(_logger));

        // Grab the Scheduler instance from the Factory
        var factory = new StdSchedulerFactory();
        var scheduler = await factory.GetScheduler();

        // and start it off
        await scheduler.Start();

        // define the job and tie it to our ShufflePlaylistsJob class
        var weebletdaysDailyJob = JobBuilder.Create<ShufflePlaylistsJob>()
            .WithIdentity("weebletdaysDailyJob", "group1")
            .Build();

        
        
        
        Console.WriteLine($"Will run next at: {runAt.ToString()}");
        
        // Schedule for 3am, repeating
        var trigger = TriggerBuilder.Create()
            .WithIdentity("weebletdaysDailyJobTrigger", "group1")
            .StartAt(runAt)
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(24)
                .RepeatForever())
            .Build();

        // Tell Quartz to schedule the job using our trigger
        await scheduler.ScheduleJob(weebletdaysDailyJob, trigger);

        await alsoRunImmediately(scheduler);
    }

    private async Task alsoRunImmediately(IScheduler scheduler)
    {
        // To run immediately, uncomment this:
        var runAt = DateTimeOffset.Now + TimeSpan.FromSeconds(20);
 
        var weebletdaysImmediateJob = JobBuilder.Create<ShufflePlaylistsJob>()
            .WithIdentity("weebletdaysImmediateJob", "group2")
            .Build();
        
        // Trigger the job to run now
        var triggerImmediate = TriggerBuilder.Create()
            .WithIdentity("weebletdaysDailyJobImmediateTrigger", "group2")
            .StartAt(runAt)
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(1)
                .WithRepeatCount(1))
            .Build();

        // Tell Quartz to schedule the job using our trigger
        await scheduler.ScheduleJob(weebletdaysImmediateJob, triggerImmediate);
    }
}

public class ShufflePlaylistsJob : IJob
{
    private List<SerializableWeightedTrack> _savedTrackData = new();
    
    private readonly ILogger _logger = LoggerSetup.Logger ?? throw new NullReferenceException();
    private SpotifyClient? _spotifyClient;
    private PlaylistManager? _playlistManager;
    private PlaylistShuffler? _playlistShuffler;
    
    public async Task Execute(IJobExecutionContext context)
    {
        await EnsureAuthenticatedSpotifyClient();
        
        if (_playlistManager is null || _playlistShuffler is null) throw new NullReferenceException();
        
        // If you need to get a list of playlists, uncomment this:
        // await _playlistManager.PrintAllPlaylistData();
        
        var curated = await _playlistManager.GetPlaylistByName("Curated Weebletdays");
        var selectSelections = await _playlistManager.GetPlaylistByName("Select Selections");
        var selectDaily = await _playlistManager.GetPlaylistByName("Weebletdays Select Daily");
        
        // Backup all playlists
        _playlistManager.BackupTracksToJsonFile(curated);
        _playlistManager.BackupTracksToJsonFile(selectSelections);
        _playlistManager.BackupTracksToJsonFile(selectDaily);
        
        await _playlistManager.DeleteAllPlaylistTracksOnSpotify(selectDaily);

        var curatedRandomTracks = await _playlistShuffler.GetRandomTracksWithPickWeightsFrom(curated, 160);
        var selectSelectionsRandomTracks = await _playlistShuffler.GetRandomTracksWithPickWeightsFrom(selectSelections, 160);

        var mergedTracks = curatedRandomTracks.Concat(selectSelectionsRandomTracks).ToList();

        var strippedOfDuplicatesTracks = removeDuplicateTracks(mergedTracks);

        await _playlistManager.AddTracksToPlaylistOnSpotify(selectDaily, strippedOfDuplicatesTracks);
        
        _playlistShuffler.IncrementPickWeightsForTracks(strippedOfDuplicatesTracks);
        
        _playlistShuffler.DecrementPickWeightsForAllSavedTracks();

        _logger.Information("Finished ShuffleWeebletdays()");
        
        
        await shufflePlaylistById("4M8XUja3GZw92EsD1QF9SI"); // Tally Hall and Stuf
        await shufflePlaylistById("0ImDFHxZXgJ1m88ogSHU4L"); // Crimbus
        await shufflePlaylistById("2SK04Gnd7kd92X1NjNSLHr"); // Curated Weebletdays
        await shufflePlaylistById("26icuIX7tMnCk1KMSqAUsq"); // Art Music
        await shufflePlaylistById("7pnXJ7jWswV32QGjJwyuFY"); // Pixel Gardner
        await shufflePlaylistById("6RECxevyNJ1ysJaAjhHSmQ"); // Pixel Gardner - To Check
        await shufflePlaylistById("1gZqNgs8xccDNTXbBhZphq"); // Beeblet Chill
        await shufflePlaylistById("1N5tVO8jvxkXeckWCulp4G"); // Weebletdays Reserves
        await shufflePlaylistById("3SIDzKeTUDDni499NE3tWr"); // Jazz
        await shufflePlaylistById("5iyF8fuEdbdd1IWkYvycds"); // Muzicalz
        await shufflePlaylistById("7zBTbIZz2lMy31TQlZvI5m"); // Metal
        await shufflePlaylistById("6tx5BB9sVWpnbORkYX8Fqn"); // Our Songs <3
    }
    
    private List<ManagedPlaylistTrack> removeDuplicateTracks(List<ManagedPlaylistTrack> mergedTracks)
    {
        var strippedOfDuplicatesTracks = mergedTracks
            .GroupBy(x => new {Id = x.Id})
            .Select(grp => grp.First())
            .ToList();
    
        return strippedOfDuplicatesTracks;
    }
    
    private async Task shufflePlaylistById(string playlistIdToShuffle)
    {
        await EnsureAuthenticatedSpotifyClient();
    
        if (_playlistManager is null || _playlistShuffler is null) 
            throw new NullReferenceException();
        
        var playlist = await _playlistManager.GetPlaylistById(playlistIdToShuffle);
        
        _playlistManager.BackupTracksToJsonFile(playlist);
        
        await _playlistShuffler.ShuffleAllIn(playlist, false);
    }

    public async Task EnsureAuthenticatedSpotifyClient()
    {
        var spotifyAuthenticationManager = new SpotifyAuthenticationManager(_logger);
        
        _spotifyClient ??= await spotifyAuthenticationManager.getAuthenticatedSpotifyClient();
        
        if (_spotifyClient is null) throw new NullReferenceException("You may need to call EnsureAuthenticatedSpotifyClient() first");
        
        _playlistManager ??= new PlaylistManager(
            _logger, 
            spotifyClient: _spotifyClient ?? throw new NullReferenceException());

        _playlistShuffler ??= new PlaylistShuffler(_logger, spotifyClient: _spotifyClient ?? throw new NullReferenceException());
    }
    
}
