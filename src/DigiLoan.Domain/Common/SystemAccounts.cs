namespace DigiLoan.Domain.Common;

public static class SystemAccounts
{
    public static readonly Guid Employer = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid MerchantPool = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid UtilityProvider = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid EscrowAccount = Guid.Parse("00000000-0000-0000-0000-000000000004");
}