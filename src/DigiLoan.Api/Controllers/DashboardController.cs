using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.Account;
using DigiLoan.Application.Interfaces;
using DigiLoan.Application.Services;
using DigiLoan.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DigiLoan.Api.Controllers;

[ApiController]
[Route("api/account")]
[Authorize]

public class DashboardController : ControllerBase
{

    private readonly IAccountOverviewService _dashboardService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITransactionHistoryService _transactionHistoryService;

    public DashboardController(
        IAccountOverviewService dashboardService,
        ITransactionHistoryService transactionHistoryService,
        UserManager<ApplicationUser> userManager
        )
    {
        _dashboardService = dashboardService;
        _userManager = userManager;
        _transactionHistoryService = transactionHistoryService;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null || !Guid.TryParse(userId, out Guid id))
        {
            return Unauthorized();
        }

        var result = await _dashboardService.GetOverviewDataAsync(id);

        return Ok(result);

    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] TransactionQuery query)
    {
        var userId = _userManager.GetUserId(User);
        if (userId is null || !Guid.TryParse(userId, out Guid id))
        {
            return Unauthorized();
        }
        return Ok(await _transactionHistoryService.GetPaginationAsync(id, query));

    }
}
