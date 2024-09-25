using Microsoft.AspNetCore.Mvc;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.App.Application.Client;
using ILogger = Serilog.ILogger;

namespace MyServe.Backend.Api.Controllers;

[ApiController]
[Route("user")]
public class UserController(ICacheService cacheService, ILogger logger, IRequestContext requestContext) : AbstractController(cacheService, logger, requestContext)
{

    [HttpGet("{id:guid}", Name = "GetUser")]
    public async Task<IActionResult> Get()
    {
        return Ok();
    }
    
}