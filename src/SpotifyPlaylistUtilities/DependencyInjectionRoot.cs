﻿using System.Collections.Specialized;
using Autofac;
using Autofac.Extras.Quartz;
using Quartz;
using Quartz.Impl;
using Quartz.Logging;
using Serilog;
using SpotifyPlaylistUtilities.RecurringJobs;
using SpotifyPlaylistUtilities.Scheduler;

namespace SpotifyPlaylistUtilities;

// ReSharper disable once ClassNeverInstantiated.Global because it actually is
public class DependencyInjectionRoot
{
    public static readonly ILogger LoggerApplication = new LoggerConfiguration()
        .Enrich.WithProperty(AppInfo.AppName + "Application", AppInfo.AppName + "SerilogContext")
        .MinimumLevel.Information()
        //.MinimumLevel.Debug()
        .WriteTo.File(
            Path.Join(AppInfo.Paths.ApplicationLoggingDirectory, "log_.log"), rollingInterval: RollingInterval.Day)
        .WriteTo.Debug()
        .CreateLogger();
    
    private static readonly ContainerBuilder DependencyContainerBuilder = new ();
    
    public static async Task<IContainer> GetBuiltContainer(bool useFullscreen, bool useMocks)
    {
        DependencyContainerBuilder.RegisterInstance(LoggerApplication).As<ILogger>().SingleInstance();
        
        // Log unobserved task exceptions
        TaskScheduler.UnobservedTaskException += (_, eventArgs) =>
        {
            eventArgs.SetObserved();
                
            eventArgs.Exception.Handle(ex =>
            {
                LoggerApplication.Error("Unhandled exception of type: {ExType} with message: {ExMessage}", ex.GetType(), ex.Message);
                    
                return true;
            });
        };

        // Scheduler and jobs
        var quartzScheduler = await setupQuartzScheduler(LoggerApplication);
        DependencyContainerBuilder.RegisterInstance(quartzScheduler).As<IScheduler>().SingleInstance();

        var schedulerConfig = new NameValueCollection {
            {"quartz.threadPool.threadCount", "2"},
            {"quartz.scheduler.threadName", "MyScheduler"}
        };
        
        DependencyContainerBuilder.RegisterModule(new QuartzAutofacFactoryModule()
        {
            ConfigurationProvider = _ => schedulerConfig
        });

        DependencyContainerBuilder.RegisterModule(new QuartzAutofacJobsModule(typeof(ShufflePlaylistJob).Assembly));
        DependencyContainerBuilder.RegisterType<JobScheduler>().AsSelf().SingleInstance();
        
        var container = DependencyContainerBuilder.Build();

        var scheduler = container.Resolve<JobScheduler>();
        await scheduler.Start();
        
        return container;
    }

    private static async Task<IScheduler> setupQuartzScheduler(ILogger logger)
    {
        LogProvider.SetCurrentLogProvider(new QuartzCustomSerilogLogProvider(logger));

        // Grab the Scheduler instance from the Factory
        var factory = new StdSchedulerFactory();
        var scheduler = await factory.GetScheduler();

        // and start it off
        await scheduler.Start();

        return scheduler;
    }
}