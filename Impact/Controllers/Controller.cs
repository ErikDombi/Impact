using System.Reflection;
using Impact.Attributes;
using Impact.Attributes.HttpMethods;
using Impact.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Impact.Controllers;

public class Controller
{
    internal Type Type { get; private set; }
    internal string Name { get; private set; }
    internal string Route { get; private set; }

    public Controller(Type type)
    {
        Type = type;
        var typeName = type.Name;
        Name = typeName.Remove(typeName.IndexOf("Controller"));
        Route = (
                    type.Namespace?.Split("Controllers")?
                        .LastOrDefault()?
                        .Replace(".", "/")
                        .ToLower()
                    ?? ""
                ) +
                "/" +
                Name.ToLower() + "/";
    }

    private MethodInfo? GetAction(ControllerBase controller, Request request)
    {
        var type = (request.Context.Request.HttpMethod.ToUpper()) switch
        {
            "GET" => typeof(HttpGet),
            "POST" => typeof(HttpPost),
            "PATCH" => typeof(HttpPatch),
            "DELETE" => typeof(HttpDelete),
            "PUT" => typeof(HttpPut),
            "HEAD" => typeof(HttpHead),
            "OPTIONS" => typeof(HttpOptions),
            _ => typeof(HttpGet)
        };

        var methods = controller
            .GetType()
            .GetMethods()
            .Where(x =>
                x.IsPublic &&
                (
                    x.GetCustomAttributes().Any(attr => attr.GetType() == type) ||
                    (
                        !x.GetCustomAttributes().Any(attr => attr.GetType().BaseType == typeof(HttpAttribute)) &&
                        type == typeof(HttpGet)
                    )
                )
            ).ToList(); // Select methods where IsPublic AND either Matches HTTP type OR has no http type and current request is HTTP/GET (the default)

        if (!methods.Any())
            return null;

        // Search by [Route(...)]
        var action = methods.FirstOrDefault(x =>
            ((Route)x.GetCustomAttribute(typeof(Route), true)!)?.Routing?.Equals(request.Action,
                StringComparison.CurrentCultureIgnoreCase) ?? false);

        // Search by [Http{?}(...)]
        action ??= methods.FirstOrDefault(x =>
            ((HttpAttribute)x.GetCustomAttribute(typeof(HttpAttribute), true)!)?.Routing?.Equals(request.Action,
                StringComparison.CurrentCultureIgnoreCase) ?? false);

        // Search by method name
        action ??= methods.FirstOrDefault(x =>
            x.Name.Equals(request.Action, StringComparison.CurrentCultureIgnoreCase) &&
            ((HttpAttribute)x.GetCustomAttribute(typeof(HttpAttribute), true)!)?.Routing == null);

        return action;
    }

    private object[] BuildParams(MethodInfo action, Request request)
    {
        var parameters = action.GetParameters();
        List<object> paramsToPass = new();
        foreach (var param in parameters)
        {
            KeyValuePair<string, string> pair = request.Params.FirstOrDefault(x =>
                x.Key.Equals(param.Name, StringComparison.CurrentCultureIgnoreCase));
            paramsToPass.Add(
                (pair.Value is null ? default : Convert.ChangeType(pair.Value, param.ParameterType)!)!
            );
        }

        return paramsToPass.ToArray();
    }

    internal Response HandleRequest(Request request, ServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetService<ILogger>()!;

        ControllerBase? controller = (ControllerBase?)serviceProvider.GetService(Type);

        if (controller is null)
        {
            logger.LogCritical(
                $"Failed to create controller instance '{Type.Name}' from Service Provider despite it being bound!");
            return new Response("", 500);
        }

        controller.Context = request.Context;

        var action = GetAction(controller, request);

        if (action is null)
            return new Response("", 404);

        var parameters = BuildParams(action, request);
        var response = action.Invoke(controller, parameters)!;

        // Cast Task<Response> OR Task<String> to Response
        if (response.GetType().IsGenericType && response.GetType().GetGenericTypeDefinition() == typeof(Task<>))
        {
            var result = response.GetType().GetProperty("Result")?.GetValue(response);
            if (result?.GetType() == typeof(string))
                return new Response((string)result);
            return (Response)result!;
        }

        // Cast String to Response
        if (response?.GetType() == typeof(string))
            return new Response((string)response);

        return (Response)response!;
    }
}