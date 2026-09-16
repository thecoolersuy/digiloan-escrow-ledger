using DigiLoan.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using DigiLoan.Domain.Enums;

namespace DigiLoan.Api.Controllers;

[ApiController]
[Route("api/dev")]
public class DevController : ControllerBase
{
    private readonly IDataSeederService _seederService;
    private readonly IWebHostEnvironment _env;

    public DevController(IDataSeederService seederService, IWebHostEnvironment environment)
    {
        _seederService = seederService;
        _env = environment;
    }

    [Authorize]
    [HttpPost("seed-data")]
    public async Task<IActionResult> SeedData([FromQuery] SeedProfile profile)
    {
        if (!_env.IsDevelopment() && !_env.IsEnvironment("Demo"))
        {
            return NotFound();
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        await _seederService.SeedAsync(userId, profile);
        return Ok(new { mesage = "Seed data generated successfully" });

    }
}
