using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.Common.Abstract;
using ILogger = Serilog.ILogger;

namespace MyServe.Backend.Api.Controllers;

[ApiController]
[Route("notes")]
[Authorize]
public class NotesController(ICacheService cacheService, ILogger logger, IRequestContext requestContext) : AbstractController(cacheService, logger, requestContext)
{
    
}