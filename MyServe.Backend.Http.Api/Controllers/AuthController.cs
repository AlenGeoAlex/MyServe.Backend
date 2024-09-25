using MediatR;
using Microsoft.AspNetCore.Mvc;
using MyServe.Backend.App.Application.Features.Auth.CreateOtp;
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
        var response = await mediator.Send(createOtpCommand);
        if (!response.Success)
            return BadRequest(response);
        
        return Created("/auth/validate", response);
    }

    [HttpPost("otp/validate")]
    public async Task<ActionResult<ValidateOtpResponse>> ValidateOtp([FromBody] ValidateOtpCommand validateOtpCommand)
    {
        var response = await mediator.Send(validateOtpCommand);
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