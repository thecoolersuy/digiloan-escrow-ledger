using Microsoft.AspNetCore.Identity;

namespace DigiLoan.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
}