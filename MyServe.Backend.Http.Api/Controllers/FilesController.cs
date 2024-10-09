using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyServe.Backend.App.Application.Client;
using MyServe.Backend.App.Application.Features.Files.ById;
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
    public async Task<ActionResult<CreateFileResponse>> CreateFile([FromBody] CreateFileCommand command)
    {
        command.Owner = requestContext.Requester.UserId;
        var fileResponse = await mediator.Send(command);

        if (fileResponse.File is not null)
        {
            return CreatedAtAction(nameof(GetFiles), fileResponse, new { id = fileResponse.File.Id });
        }

        if (fileResponse.Message is not null && fileResponse.Message == CreateFileResponse.Duplicate)
            return Conflict();

        return BadRequest(fileResponse.Message);
    }

    [HttpGet("{fileId:guid}")]
    public async Task<ActionResult<GetFileByIdResponse>> GetFile([FromRoute] Guid fileId)
    {
        requestContext.CacheControl.FrameEndpointCacheKey(CacheConstants.FileIdCacheKey, fileId.ToString());
        var cache = await ScanAsync<GetFileByIdResponse>();
        if (cache is not null)
            return cache;

        var response = await mediator.Send(new GetFileByIdQuery {Id = fileId});
        if (response is null)
            return NotFound();

        await CacheAsync(response, TimeSpan.FromMinutes(30));
        return Ok(response);
    }

    [HttpPost("signed")]
    public async Task<ActionResult<CreateSignedUrlResponse>> GetSignedUrl([FromBody] CreateSignedUrlCommand command)
    {
        var response = await mediator.Send(command);
        if(response.Success)
            return Ok(response);
        
        return BadRequest(response);
    }

    [HttpGet]
    public async Task<ActionResult<ListFileResponse>> GetFiles([FromQuery] ListFileOptions fileOptions)
    {
        fileOptions.OwnerId = requestContext.Requester.UserId;
        requestContext.CacheControl.FrameEndpointCacheKey(CacheConstants.FileListCacheKey, fileOptions.OwnerId.ToString(), fileOptions.ParentId.HasValue ? fileOptions.ParentId.Value.ToString() : "undefined", Request.QueryString.ToString());
        var cache = await ScanAsync<ListFileResponse>();
        if (cache is not null)
            return Ok(cache);

        var listFileResponse = await mediator.Send(fileOptions);

        await CacheAsync(listFileResponse);
        return Ok(listFileResponse);
    }
}