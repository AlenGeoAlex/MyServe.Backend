using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Features.Profile.Create;
using MyServe.Backend.App.Application.Features.Profile.Me;
using MyServe.Backend.App.Application.Features.Profile.Search;
using Newtonsoft.Json;
using ILogger = Serilog.ILogger;

namespace MyServe.Backend.Api.Controllers;

[ApiController]
[Route("me")]
public class MeController(ICacheService cacheService, ILogger logger, IRequestContext requestContext, IMediator mediator) : AbstractController(cacheService, logger, requestContext)
{

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<MeResponse>> Get()
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
    [Authorize]
    public async Task<ActionResult<CreateProfileResponse>> Post()
    {
        var formCollection = await Request.ReadFormAsync();
        if (!formCollection.ContainsKey("body"))
            return BadRequest("Body is missing on the request");

        var bodyRaw = formCollection["body"];
        if(string.IsNullOrWhiteSpace(bodyRaw))
            return BadRequest("Invalid body provided in the form. The body must be a valid JSON");
        
        var createProfileCommand = JsonConvert.DeserializeObject<CreateProfileCommand>(bodyRaw.ToString());
        
        if(createProfileCommand is null)
            return BadRequest("Invalid JSON body provided in the form. Failed to parse the body object");
        
        createProfileCommand.ProfileId = requestContext.Requester.UserId;
        createProfileCommand.Email = requestContext.Requester.EmailAddress;

        var fileAsStream = GetFileAsStream("profileImage");
        createProfileCommand.ProfileImageStream = fileAsStream;

        var createProfileResponse = await mediator.Send(createProfileCommand);
        if(createProfileResponse.Response == CreateProfileResponse.Duplicate)
            return Conflict();
        
        return CreatedAtRoute("GetUser", new { controller = "User", id = createProfileResponse.Id }, null);
    }

    [HttpGet("search")]
    public async Task<ActionResult<MeSearchResponse>> Search([FromQuery] MeSearchQuery searchQuery)
    {
        searchQuery.UserId = requestContext.Requester.UserId;
        var response = await mediator.Send(searchQuery);
        return Ok(response);
    }
    
}