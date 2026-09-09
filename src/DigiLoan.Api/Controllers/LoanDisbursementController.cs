using System.Security.Claims;
using DigiLoan.Application.Common.Exceptions;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.LoanEligibility;
using Microsoft.AspNetCore.Mvc;

namespace DigiLoan.Api.Controllers;

[ApiController]
[Route("api/disburse")]
public class LoanDisbursementController : ControllerBase
{

    private readonly ILoanDisbursementService _disbursementService;

    public LoanDisbursementController(ILoanDisbursementService disbursementService)
    {
        _disbursementService = disbursementService;
    }
    [HttpPost("disburse")]
    public async Task<IActionResult> Disburse(LoanApplicationRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var result = await _disbursementService.DisburseLoanAsync(userId, request);
            return Ok(result);
        }
        catch (ConcurrencyException error)
        {
            return Conflict(error.Message);
        }
        catch (InvalidOperationException error)
        {
            return BadRequest(error.Message);
        }
    }

}