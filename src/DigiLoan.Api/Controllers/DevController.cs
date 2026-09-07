using DigiLoan.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DigiLoan.Api.Controllers;

[ApiController]
[Route("api/dev")]
public class DevController : ControllerBase
{
    private readonly IDataSeederService _seederService;
    private readonly IWebHostEnvironment _environment;

    public DevController(IDataSeederService seederService, IWebHostEnvironment environment)
    {
        _seederService = seederService;
        _environment = environment;
    }

    [Authorize]
    [HttpPost("seed-date")]
    public async Task<IActionResult> SeedData()
    {
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        await _seederService.SeedAsync(userId);
        return Ok(new { mesage = "Seed data generated successfully" });

    }
}
