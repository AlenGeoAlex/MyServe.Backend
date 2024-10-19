using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyServe.Backend.Api.Controllers;

[ApiController]
[Route("secrets")]
[Authorize]
public class SecretsController
{
    
}