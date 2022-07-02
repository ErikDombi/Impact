using System.Net;
using Newtonsoft.Json;

namespace Impact.Controllers;

public class ControllerBase
{
    public HttpListenerContext Context { get; set; }
    
    internal Controller Controller { get; set; }
    protected Response Ok(string msg = "") => new Response(msg);
    
    protected Response Unauthorized(string msg = "") => new Response(msg, 401);

    protected Response Forbidden(string msg = "") => new Response(msg, 403);

    protected Response Redirect(string msg = "") => new Response(msg, 301);

    protected Response StatusCode(int code) => new Response(string.Empty, code);

    protected Response View(string? name = null) => new Response(name, -1);

    protected Response JSON(object? obj, int code = 200) => new Response(JsonConvert.SerializeObject(obj, Formatting.Indented), code);
}