using System.Net;
using System.Text.RegularExpressions;
using Impact.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Impact;

public class ImpactServer
{
    internal int Port;
    internal LogLevel LogLevel = LogLevel.Information;
    internal ILogger? Logger;
    internal List<string> Uris = new();
    internal ServiceCollection collection = new();

    private Router _router;
    private readonly HttpListener _listener;
    private ServiceProvider _services;
    
    internal ImpactServer()
    {
        _listener = new HttpListener();
    }

    private ServiceProvider BuildServices()
    {
        collection.AddSingleton(new Router(Logger!, collection));
        
        return collection
            .AddSingleton(Logger!)
            .BuildServiceProvider();
    }
    
    public ImpactServer Start()
    {
        Logger ??= new Logger(LogLevel);
        Logger.LogInformation("Starting Impact Server...");
        
        Logger.LogDebug("Building Services...");
        _services = BuildServices();
        _router = _services.GetRequiredService<Router>();
        
        if(!Uris.Any())
            Uris.Add("http://localhost");

        Regex regex = new Regex(":[0-9]+");
        Uris.ForEach(x =>
        {
            var prefix = x.TrimEnd('/') + (regex.IsMatch(x) ? "/" : $":{Port}/");
            _listener.Prefixes.Add(prefix);
            Logger.LogInformation("Now Listening: " + prefix);
        });
        
        _listener.Start();
        Logger.LogInformation("Impact Server Started!");
        
        Logger.LogInformation("Waiting for connections...");
        Receive();
        return this;
    }

    private void Receive()
    {
        _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
    }

    private void ListenerCallback(IAsyncResult result)
    {
        if (_listener.IsListening)
        {
            var context = _listener.EndGetContext(result);

            _ = _router.Route(context, _services);
            
            Receive();
        }
    }
}