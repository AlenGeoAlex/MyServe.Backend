using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MyServe.Backend.App.Application.Features.Auth.CreateOtp;
using MyServe.Backend.App.Application.Features.Auth.OAuth;
using MyServe.Backend.App.Application.Features.Auth.Refresh;
using MyServe.Backend.App.Application.Features.Auth.ValidateOtp;

namespace MyServe.Backend.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IMediator mediator) : ControllerBase
{

    [HttpPost("otp")]
    public async Task<ActionResult<CreateOtpResponse>> CreateOtp([FromBody] CreateOtpCommand createOtpCommand)
    {
        createOtpCommand.Origin = Request.Headers.Origin.ToString();
        var response = await mediator.Send(createOtpCommand);
        if (!response.Success)
            return BadRequest(response);
        
        return Created("/auth/validate", response);
    }

    [HttpPost("otp/validate")]
    public async Task<ActionResult<ValidateOtpResponse>> ValidateOtp([FromBody] ValidateOtpCommand validateOtpCommand)
    {
        validateOtpCommand.Origin = Request.Headers.Origin.ToString();
        var response = await mediator.Send(validateOtpCommand);
        if (!response.Success)
            return BadRequest(response);

        return response;
    }

    [HttpPost("oauth")]
    public async Task<ActionResult<OAuthResponse>> OAuth([FromBody] OAuthCommand oAuthCommand)
    {

        oAuthCommand.Origin = Request.Headers.Origin.ToString();
        var response = await mediator.Send(oAuthCommand);
        if (!response.Success)
            return BadRequest(response);

        return response;
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenCommand refreshTokenCommand)
    {
        var response = await mediator.Send(refreshTokenCommand);
        if (response == null)
            return Unauthorized();

        return Ok(response);
    }
}