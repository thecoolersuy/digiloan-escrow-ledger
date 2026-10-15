

namespace DigiLoan.Domain.Common;

public static class RepaymentAmountCalculator
{
    public static decimal InterestAmount(decimal principal, decimal interest, int tenorMonths) =>
       Math.Round(principal * (interest / 100m) * (tenorMonths / 12m), 2);

    public static decimal RepaymentAmount(decimal principal, decimal interest, int tenorMonths) =>
     principal + InterestAmount(principal, interest, tenorMonths);
}