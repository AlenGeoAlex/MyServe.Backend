using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Features.Files.Create;
using MyServe.Backend.App.Application.Features.Files.List;
using MyServe.Backend.App.Application.Features.Files.Signed;
using MyServe.Backend.Common.Abstract;
using MyServe.Backend.Common.Constants;
using ILogger = Serilog.ILogger;

namespace MyServe.Backend.Api.Controllers;

[Controller]
[Route("files")]
[Authorize]
public class FilesController(IMediator mediator, ICacheService cacheService, ILogger logger, IRequestContext requestContext) : AbstractController(cacheService, logger, requestContext)
{

    [HttpPost]
    public async Task<IActionResult> CreateFile([FromBody] CreateFileCommand command)
    {
        command.Owner = requestContext.Requester.UserId;
        var fileResponse = await mediator.Send(command);
        
        if(fileResponse.File is not null)
            return CreatedAtAction(nameof(GetFiles), fileResponse, new { id = fileResponse.File.Id });

        if (fileResponse.Message is not null && fileResponse.Message == CreateFileResponse.Duplicate)
            return Conflict();

        return BadRequest(fileResponse.Message);
    }

    [HttpGet]
    public async Task<IActionResult> GetFiles([FromQuery] ListFileOptions fileOptions)
    {
        requestContext.CacheControl.FrameEndpointCacheKey(CacheConstants.FileListCacheKey, Request.QueryString.ToString());
        fileOptions.OwnerId = requestContext.Requester.UserId;

        var listFileResponse = await mediator.Send(fileOptions);
        return Ok(listFileResponse);
    }

    [HttpPost("signed")]
    public async Task<IActionResult> GetSignedUrl([FromBody] CreateSignedUrlCommand command)
    {
        return Ok();
    }
}