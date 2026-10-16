using System.ComponentModel;
using System.Reflection.Metadata.Ecma335;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Application.Features.LoanEligibility;
using DigiLoan.Application.Interfaces;
using DigiLoan.Domain.Entities;

namespace DigiLoan.Application.Services;

public class LoanHistoryService : ILoanHistoryService
{

    private readonly ILoanApplicationRepository _loanApplicationRepository;
    private readonly IUserAccountRepository _userAccountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoanHistoryService(ILoanApplicationRepository loanApplicationRepository, IUserAccountRepository userAccountRepository, IUnitOfWork unitOfWork)
    {
        _loanApplicationRepository = loanApplicationRepository;
        _userAccountRepository = userAccountRepository;
        _unitOfWork = unitOfWork;

    }
    public async Task<List<LoanApplicationDto>> GetLoanApplicationsAsync(Guid userId)
    {
        var account = await _userAccountRepository.GetByUserIdAsync(userId)
            ?? throw new InvalidOperationException("No account exists");

        var applications = await _loanApplicationRepository.GetByAccountIdAsync(account.Id);
        var changed = false;
        foreach (var application in applications)
        {
            if (application.MarkOverdueIfPastDueDate(DateTime.UtcNow))
            {
                await _loanApplicationRepository.UpdateAsync(application);
                changed = true;
            }
        }
        if (changed) await _unitOfWork.SaveChangesAsync();

        return applications.Select(e => ToDto(e)).ToList();
    }


    private static LoanApplicationDto ToDto(LoanApplication application) => new()
    {
        Id = $"LN-{application.Id.ToString()[..6].ToUpper()}",
        CreatedAtUtc = application.CreatedAtUtc,
        RequestedAmount = application.RequestedAmount,
        InterestRate = application.InterestRate,
        LoanTenor = application.LoanTenor,
        RepaymentDate = application.RepaymentDate,
        Status = application.Status
    };
}

