namespace Impact.Logging;

public interface ILogger
{
    public void Log(LogLevel level, string message);
    public void LogCritical(string message);
    public void LogError(string message);
    public void LogWaring(string message);
    public void LogInformation(string message);
    public void LogDebug(string message);
    public void LogTrace(string message);
    public void Exception(Exception ex);
}