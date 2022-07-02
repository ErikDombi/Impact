namespace Impact.Logging;

public class Logger : ILogger
{
    private LogLevel _level;

    public Logger(LogLevel level)
    {
        _level = level;
    }
    
    public void Log(LogLevel level, string message)
    {
        if(level > _level)
            Console.WriteLine($"[Impact / {level}] {message}");
    }

    public void LogCritical(string message)
        => Log(LogLevel.Critical, message);

    public void LogError(string message)
        => Log(LogLevel.Error, message);

    public void LogWaring(string message)
        => Log(LogLevel.Warning, message);

    public void LogInformation(string message)
        => Log(LogLevel.Information, message);

    public void LogDebug(string message)
        => Log(LogLevel.Debug, message);

    public void LogTrace(string message)
        => Log(LogLevel.Trace, message);

    public void Exception(Exception ex)
        => LogTrace(ex.ToString());
}