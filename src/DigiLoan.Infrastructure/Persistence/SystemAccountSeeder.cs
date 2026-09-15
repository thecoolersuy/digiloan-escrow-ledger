using DigiLoan.Domain.Common;
using DigiLoan.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client.NativeInterop;

namespace DigiLoan.Infrastructure.Persistence;

public static class SystemAccountSeeder
{
    public static async Task SeedSystemAccountsAsync(AppDbContext context)
    {
        var systemAccountsId = new[]
        {
            SystemAccounts.Employer,
            SystemAccounts.MerchantPool,
            SystemAccounts.UtilityProvider,
            SystemAccounts.EscrowAccount,
            SystemAccounts.OpeningBalance
        };

        foreach (var accountId in systemAccountsId)
        {
            var exists = await context.UserAccounts.AnyAsync(a => a.Id == accountId);
            if (!exists)
            {
                context.UserAccounts.Add(new UserAccount
                {
                    Id = accountId,
                    UserId = Guid.Empty,
                    AccountNumber = $"SYS-{accountId.ToString()[^4..]}",
                    CurrentBalance = 0
                });
            }
        }

        await context.SaveChangesAsync();
    }
}