using System.Collections;
using System.Net;
using System.Text;

namespace Impact;

internal class Request
{
    public string Route { get; private set; }
    public string Action { get; private set; }
    public string Path => System.IO.Path.Combine(Route, Action) + "/";
    public Dictionary<string, string> Params { get; private set; }
    public HttpListenerContext Context { get; private set; }

    public Request(HttpListenerContext context)
    {
        this.Context = context;
        var request = context.Request;
        var uri = request.Url;

        string url = uri!.LocalPath;
        string? paramString = uri.PathAndQuery.Contains("?") ? uri.PathAndQuery.Split("?").LastOrDefault() : null;
        string pathString = url.Split("?").First().TrimEnd('/');
        
        Queue<string> urlPortions = new Queue<string>(pathString.Split("/").Reverse());
        Action = urlPortions.Dequeue();
        urlPortions = new Queue<string>(urlPortions.Reverse());

        StringBuilder routeBuilder = new StringBuilder("/");
        while (urlPortions.Any())
        {
            routeBuilder.Append(urlPortions.Dequeue());
            routeBuilder.Append("/");
        }

        Route = routeBuilder.ToString().Replace("//", "/");

        Params = paramString?.TrimStart('?').Split("&").Select(x => x.Split("=")).ToDictionary(x => x[0], x=> x[1]) ?? new Dictionary<string, string>();
    }

    public override string ToString()
    {
        return System.IO.Path.Combine(Route, Action) + $" ({Route} : {Action})";
    }

    public void ShiftAction()
    {
        Route += Action + "/";
        Action = "Index";
    }
}