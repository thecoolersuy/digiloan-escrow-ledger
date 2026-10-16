using System.Security.Claims;
using DigiLoan.Application.Common.Exceptions;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.LoanEligibility;
using DigiLoan.Application.Interfaces;
using DigiLoan.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigiLoan.Api.Controllers;

[ApiController]
[Route("api/disburse")]
[Authorize]
public class LoanDisbursementController : ControllerBase
{

    private readonly ILoanDisbursementService _disbursementService;
    private readonly ILoanHistoryService _loanHistoryService;

    public LoanDisbursementController(ILoanDisbursementService disbursementService, ILoanHistoryService loanHistoryService)
    {
        _disbursementService = disbursementService;
        _loanHistoryService = loanHistoryService;
    }

    [HttpPost("quote")]
    public async Task<IActionResult> Quote(LoanApplicationRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }
        return Ok(await _disbursementService.QuoteAsync(userId, request));
    }

    [HttpGet("applications")]
    public async Task<IActionResult> GetApplications()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        return Ok(await _loanHistoryService.GetLoanApplicationsAsync(userId));
    }
    [HttpPost("disburse")]
    public async Task<IActionResult> Disburse(LoanApplicationRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await _disbursementService.DisburseLoanAsync(userId, request);
        return Ok(result);
    }

}