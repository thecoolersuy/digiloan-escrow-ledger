namespace DigiLoan.Domain.Common;

public static class SystemAccounts
{
    public static readonly Guid Employer = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid MerchantPool = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid UtilityProvider = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid EscrowAccount = Guid.Parse("00000000-0000-0000-0000-000000000004");

    public static readonly Guid OpeningBalance = Guid.Parse("00000000-0000-0000-0000-000000000005");

    public static readonly Dictionary<Guid, string> Labels = new()
    {
        [Employer] = "Employer",
        [MerchantPool] = "Merchant Pool",
        [UtilityProvider] = "Utility Provider",
        [EscrowAccount] = "Escrow Account",
        [OpeningBalance] = "Opening Balance"
    };

    public static string? GetLabel(Guid accountId) => Labels.TryGetValue(accountId, out var label) ? label : null;

    public static string GetAccountNumber(Guid accountId) => $"SYS-{accountId.ToString()[^4..]}";
}