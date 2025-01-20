using Quartz;
using Serilog;
using SpotifyPlaylistUtilities.Scheduler.RecurringJobs;

namespace SpotifyPlaylistUtilities.Scheduler;

public class JobScheduler(ILogger logger, IScheduler scheduler)
{
    // Pick one:
    private DateTimeOffset _firstRunAt = getNextTimeToRunAt();
    //private DateTimeOffset _firstRunAt = DateTimeOffset.Now + TimeSpan.FromSeconds(10);
    
    public async Task Start()
    {
        // To add a new scheduled job, first configure it by copying any of the methods below
        // Configure it for new job name and type
        // Then register it near the top of DependencyInjectionRoot 
        
        await setupRecurringJob<ShufflePlaylistJob>("AlarmJobs", _firstRunAt, 24);
        
        await scheduler.Start();
    }
    
    // Recurring
    private async Task setupRecurringJob<T>(string jobFriendlyGroupName, DateTimeOffset firstRunAtTime, int repeatEveryHours) where T : IJob
    {
        var jobFriendlyName = typeof(T).ToString();

        var lastIndexOfPeriodCharacter = jobFriendlyName.LastIndexOf('.') + 1;

        jobFriendlyName = jobFriendlyName[lastIndexOfPeriodCharacter..];

        var jobIdentityName = jobFriendlyName.Replace("Job", "");
        
        var jobDetails = JobBuilder.Create<T>()
            .WithIdentity(jobIdentityName, jobFriendlyGroupName)
            .Build();
        
        logger.Information("Setting up {ThisName} schedule, will begin running at: {RunAt}", jobFriendlyName, firstRunAtTime);
        
        // Trigger the job to run now, and then repeat
        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{jobIdentityName}Triggers", $"{jobFriendlyGroupName}Triggers")
            .StartAt(firstRunAtTime)
            .WithSimpleSchedule(x => x
                .WithIntervalInHours(repeatEveryHours)
                .RepeatForever())
            .Build();

        logger.Information("Setting up new (recurring) scheduled job, jobFriendlyName: {JobName}, jobFriendlyGroupName: {GroupName}, firstRunAtSeconds: {FirstRunAt}, repeatEverySeconds: {RepeatEvery}", jobFriendlyName, jobFriendlyGroupName, firstRunAtTime, repeatEveryHours);
        
        // Tell Quartz to schedule the job using our trigger
        await scheduler.ScheduleJob(jobDetails, trigger);
    }
    
    private static DateTimeOffset getNextTimeToRunAt()
    {
        // Set up when to run it (at 3am)
        var runAt = new DateTimeOffset(
            DateTimeOffset.Now.Year,
            DateTimeOffset.Now.Month,
            DateTimeOffset.Now.Day,
            3,
            0,
            0,
            new TimeSpan(-5, 0, 0)
        );

        // If we're beyond 3am, then set it to the next 3am (Tomorrow)
        if (DateTimeOffset.Now > runAt)
            runAt += TimeSpan.FromDays(1);
        
        return runAt;
    }
}