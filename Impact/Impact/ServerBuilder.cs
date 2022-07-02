using Impact.Logging;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Impact;

public class ServerBuilder
{
    private ImpactServer _server;

    public ServerBuilder()
    {
        _server = new ImpactServer();
    }

    public ServerBuilder WithPort(int port)
    {
        _server.Port = port;
        return this;
    }

    public ServerBuilder WithLogLevel(LogLevel level)
    {
        _server.LogLevel = level;
        return this;
    }

    public ServerBuilder WithLogger(ILogger logger)
    {
        _server.Logger = logger;
        return this;
    }

    public ServerBuilder WithPrefixes(params string[] uris)
    {
        foreach (var uri in uris)
        {
            _server.Uris.Add(uri);
        }
        return this;
    }

    public ServerBuilder AddSingleton<T>() where T : class
    {
        _server.collection.AddSingleton<T>();
        return this;
    }
    
    public ServerBuilder AddSingleton(Type t)
    {
        _server.collection.AddSingleton(t);
        return this;
    }
    
    public ServerBuilder AddTransient<T>() where T : class
    {
        _server.collection.AddTransient<T>();
        return this;
    }
    
    public ServerBuilder AddTransient(Type t)
    {
        _server.collection.AddTransient(t);
        return this;
    }
    
    public ImpactServer Build() =>
        _server;

    public ImpactServer BuildAndStart() =>
        _server.Start();
}