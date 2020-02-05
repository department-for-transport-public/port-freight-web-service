using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PortFreight.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PortFreight.Web.Tests.UserAccount.FakeRepos
{
    public class FakeUserManager : UserManager<ApplicationUser>
    {
        public FakeUserManager()
            : base(new Mock<IUserStore<ApplicationUser>>().Object,
                  new Mock<IOptions<IdentityOptions>>().Object,
                  new Mock<IPasswordHasher<ApplicationUser>>().Object,
                  new IUserValidator<ApplicationUser>[0],
                  new IPasswordValidator<ApplicationUser>[0],
                  new Mock<ILookupNormalizer>().Object,
                  new Mock<IdentityErrorDescriber>().Object,
                  new Mock<IServiceProvider>().Object,
                  new Mock<ILogger<UserManager<ApplicationUser>>>().Object)
        { }

        public override Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return Task.FromResult(new ApplicationUser { Email = email });
        }

        public override Task<bool> IsEmailConfirmedAsync(ApplicationUser user)
        {
            return Task.FromResult(user.Email == "test@test.com");
        }

        public override Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
        {
            return Task.FromResult("---------------");
        }
    }
}
