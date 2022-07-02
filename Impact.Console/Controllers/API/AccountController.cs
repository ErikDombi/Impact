using Impact.Attributes;
using Impact.Attributes.HttpMethods;
using Impact.Controllers;
using Impact.Logging;

namespace Impact.Console.Controllers.API;

[Route("api/test")]
public class AccountController : ControllerBase
{
    private ILogger _logger;

    public AccountController(ILogger logger)
    {
        _logger = logger;
    }
    
    
    public async Task<Response> Login()
    {
        _logger.LogInformation("Using ImpactServer's logger instance");
        return Ok("Login()");
    }

    [HttpPost]
    public async Task<Response> Register()
    {
        return JSON("This will be returned as a JSON object when using HTTP POST to /api/test/register");
    }
}