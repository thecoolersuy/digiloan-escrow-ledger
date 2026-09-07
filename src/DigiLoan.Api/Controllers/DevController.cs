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
    private readonly IWebHostEnvironment _environment;

    public DevController(IDataSeederService seederService, IWebHostEnvironment environment)
    {
        _seederService = seederService;
        _environment = environment;
    }

    [Authorize]
    [HttpPost("seed-data")]
    public async Task<IActionResult> SeedData([FromQuery] SeedProfile profile = SeedProfile.Spender)
    {
        if (!_environment.IsDevelopment())
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
