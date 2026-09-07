using System.Runtime.CompilerServices;
using System.Security.Claims;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace DigiLoan.Api.Controllers;

[ApiController]
[Route("api/credit-scoring")]
[Authorize]
public class CreditScoringController : ControllerBase
{
    private readonly ICreditScoringService _creditScoringService;

    public CreditScoringController(ICreditScoringService creditScoringService)
    {
        _creditScoringService = creditScoringService;
    }

    [HttpGet("eligibility")]
    public async Task<IActionResult> GetEligibility()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("Invalid UserId");

        var result = await _creditScoringService.EvaluateResultAsync(Guid.Parse(userIdClaim));

        return Ok(result);
    }
}