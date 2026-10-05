using DigiLoan.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace DigiLoan.Application.Common.Interfaces
{
    public class UserIdentityService : IUserIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserIdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GetFullNameAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            return user!.FullName;
        }
    }
}


