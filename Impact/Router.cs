using System.Net;
using System.Reflection;
using System.Text;
using Impact.Attributes;
using Impact.Controllers;
using Impact.Helpers;
using Impact.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Impact;

internal class Router
{
    private ILogger _logger;
    private List<Controller> controllers;

    public Router(ILogger logger, IServiceCollection services)
    {
        _logger = logger;
        var type = typeof(ControllerBase);
        controllers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => type.IsAssignableFrom(p) && p != type && (p.Namespace?.Contains("Controllers") ?? false))
            .Select(t => new Controller(t))
            .ToList();

        controllers.ForEach(x =>
        {
            services.AddTransient(x.Type);
            logger.LogDebug("Found Controller: " + x.Name + $" ({x.Route})");
        });
    }

    private Controller? GetController(ref Request req)
    {
        var route = req.Route;
        var path = req.Path;
        
        // Search by [Route(...)] == Request.Route
        Controller? controller = controllers.FirstOrDefault(x =>
        {
            string? routeValue = ((Route)x.Type.GetCustomAttribute(typeof(Route), true)!)?.Routing;
            
            if (!routeValue?.EndsWith("/") ?? false)
                routeValue += "/";
            if (!routeValue?.StartsWith("/") ?? false)
                routeValue = "/" + routeValue;
            
            return routeValue?.Equals(route, StringComparison.CurrentCultureIgnoreCase) ?? false;
        });
        
        // Search by [Route(...)] == Request.Path
        if(controller is null)
        { 
            controller = controllers.FirstOrDefault(x =>
            {
                string? routeValue = ((Route)x.Type.GetCustomAttribute(typeof(Route), true)!)?.Routing;
            
                if (!routeValue?.EndsWith("/") ?? false)
                    routeValue += "/";
                if (!routeValue?.StartsWith("/") ?? false)
                    routeValue = "/" + routeValue;
            
                return routeValue?.Equals(path, StringComparison.CurrentCultureIgnoreCase) ?? false;
            });

            if(controller is not null)
                req.ShiftAction();
        }

        // Search by Controller Name == Request.Route
        controller ??= controllers.FirstOrDefault(x => x.Route.Equals(route, StringComparison.CurrentCultureIgnoreCase) && x.Type.GetCustomAttribute(typeof(Route), true) is null);
        
        // Search by Controller Name == Request.Path
        if(controller is null)
        {
            controller = controllers.FirstOrDefault(x => x.Route.Equals(path, StringComparison.CurrentCultureIgnoreCase) && x.Type.GetCustomAttribute(typeof(Route), true) is null);
            if (controller is not null)
                req.ShiftAction();
        }

        return controller;
    }
    
    public async Task Route(HttpListenerContext context, ServiceProvider serviceProvider)
    {
        var request = context.Request;
        var uri = request.Url!;
        
        Request req = new Request(context);

        Controller? controller = GetController(ref req);
        
        HttpListenerResponse response = context.Response;
        
        if (controller is null)
        {
            response.StatusCode = 404;

            response.OutputStream.Close();
            _logger.LogInformation($"{request.HttpMethod.ToUpper()}: {uri.LocalPath} {response.StatusCode}");
            return;
        }

        try
        {
            var actionResponse = controller.HandleRequest(req, serviceProvider);

            if (actionResponse.StatusCode == 301)
                response.Redirect(actionResponse.Body);

            if (actionResponse.StatusCode == -1)
            {
                byte[] viewBuffer = Encoding.UTF8.GetBytes(
                    System.IO.File.ReadAllText(
                        "./Views/" + (!string.IsNullOrWhiteSpace(actionResponse.Body)
                            ? (actionResponse.Body + (actionResponse.Body.EndsWith(".ihtml") ? "" : ".ihtml"))
                            : Path.Combine(controller.Route, (req.Action + ".ihtml")))
                    )
                );
                response.StatusCode = 200;
                await response.OutputStream.WriteAsync(viewBuffer, 0, viewBuffer.Length);
            }
            else
            {
                byte[] buffer = Encoding.UTF8.GetBytes(actionResponse.Body);
                response.StatusCode = actionResponse.StatusCode;
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
        catch (Exception ex)
        {
            response.StatusCode = 500;
            #if DEBUG
            byte[] buffer = Encoding.UTF8.GetBytes(ex.ToString());
            #else
            byte[] buffer = Encoding.UTF8.GetBytes("An unhandled exception occured!");
            #endif
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        }

        response.OutputStream.Close();
        _logger.LogInformation($"{request.HttpMethod.ToUpper()}: {uri.LocalPath} {response.StatusCode}");
    }
}