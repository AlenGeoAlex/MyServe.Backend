using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Features.Profile.Create;
using MyServe.Backend.App.Application.Features.Profile.Me;
using ILogger = Serilog.ILogger;

namespace MyServe.Backend.Api.Controllers;

[ApiController]
[Route("me")]
public class MeController(ICacheService cacheService, ILogger logger, IRequestContext requestContext, IMediator mediator) : AbstractController(cacheService, logger, requestContext)
{

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get()
    {
        var requesterUserId = requestContext.Requester.UserId;
        requestContext.CacheControl.FrameEndpointCacheKey(CacheConstants.UserCacheKey, requesterUserId.ToString());
        var response = await ScanAsync<MeResponse>();
        if (response is not null)
            return Ok(response);
        

        var meResponse = await mediator.Send(new MeByIdQuery(requesterUserId));
        if (meResponse is null)
            return NotFound();
        else
            await CacheAsync(meResponse);
        
        return Ok(meResponse);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateProfileCommand createProfileCommand)
    {
        //TODO
        //createProfileCommand.ProfileId = requestContext.Requester.UserId;
        //createProfileCommand.Email = requestContext.Requester.EmailAddress;
        createProfileCommand.ProfileId = Guid.Parse("63dae594-63fb-4eb6-a68f-3607a1317df6");
        createProfileCommand.Email = "alengeoalex@gmail.com";

        var createProfileResponse = await mediator.Send(createProfileCommand);
        if(createProfileResponse.Response == CreateProfileResponse.Duplicate)
            return Conflict();
        
        return CreatedAtRoute("GetUser", new { controller = "User", id = createProfileResponse.Id }, null);
    }
    
}