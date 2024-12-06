using Quartz.Logging;
using Serilog;
using LogContext = Serilog.Context.LogContext;

namespace SpotifyPlaylistUtilities.Logging;

public class CustomSerilogLogProvider(ILogger logger) : ILogProvider
{
    public ILogger Logger { get; set; } = logger ?? throw new ArgumentNullException(nameof(logger));

    public Quartz.Logging.Logger GetLogger(string name)
    {
        return new SerilogLogger(Logger.ForContext("SourceContext", name, destructureObjects: false)).Log;   
    }

    private object forContext(string name)
    {
        return Logger.ForContext("SourceContext", name, destructureObjects: false);
    }

    public IDisposable OpenNestedContext(string message)
    {
        return LogContext.PushProperty("NDC", message);            
    }

    public IDisposable OpenMappedContext(string key, object value, bool destructure = false)
    {
        return LogContext.PushProperty(key, value, false);   
    }
    
    internal class SerilogLogger
    {
        private ILogger _logger;        

        public SerilogLogger(ILogger logger)
        {
            this._logger = logger;
        }

        public bool Log(LogLevel logLevel, Func<string> messageFunc, Exception exception, params object[] formatParameters)
        {
            var translatedLevel = translateLevel(logLevel);
            
            if (messageFunc == null)
            {
                return _logger.IsEnabled(translatedLevel);
            }

            if (!_logger.IsEnabled(translatedLevel))
            {
                return false;
            }

            if (exception != null)
            {
                logException(translatedLevel, messageFunc, exception, formatParameters);
            }
            else
            {
                logMessage(translatedLevel, messageFunc, formatParameters);
            }

            return true;
        }

        private void logMessage(Serilog.Events.LogEventLevel logLevel, Func<string> messageFunc, object[] formatParameters)
        {
            _logger.Write(logLevel, messageFunc(), formatParameters);            
        }

        private void logException(Serilog.Events.LogEventLevel logLevel, Func<string> messageFunc, Exception exception, object[] formatParams)
        {            
            _logger.Write(logLevel, exception, messageFunc(), formatParams);
        }

        private static Serilog.Events.LogEventLevel translateLevel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Fatal:
                    return Serilog.Events.LogEventLevel.Fatal;
                case LogLevel.Error:
                    return Serilog.Events.LogEventLevel.Error; 
                case LogLevel.Warn:
                    return Serilog.Events.LogEventLevel.Warning; 
                case LogLevel.Info:
                    return Serilog.Events.LogEventLevel.Information; 
                case LogLevel.Trace:
                    return Serilog.Events.LogEventLevel.Verbose; 
                default:
                    return Serilog.Events.LogEventLevel.Debug; 
            }
        }
    }                      
}