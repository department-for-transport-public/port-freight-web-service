using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Services.EmailSender;
using PortFreight.Web.Pages.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace PortFreight.Web.Tests.Profile
{
    public class FakeSignInManager : SignInManager<PortFreightUser>
    {
        public FakeSignInManager()
                : base(new Mock<FakeUserManager>().Object,
                     new Mock<IHttpContextAccessor>().Object,
                     new Mock<IUserClaimsPrincipalFactory<PortFreightUser>>().Object,
                     new Mock<IOptions<IdentityOptions>>().Object,
                     new Mock<ILogger<SignInManager<PortFreightUser>>>().Object,
                     new Mock<IAuthenticationSchemeProvider>().Object)
        { }

    }

    public class FakeUserManager : UserManager<PortFreightUser>
    {
        public FakeUserManager()
            : base(new Mock<IUserStore<PortFreightUser>>().Object,
              new Mock<IOptions<IdentityOptions>>().Object,
              new Mock<IPasswordHasher<PortFreightUser>>().Object,
              new IUserValidator<PortFreightUser>[0],
              new IPasswordValidator<PortFreightUser>[0],
              new Mock<ILookupNormalizer>().Object,
              new Mock<IdentityErrorDescriber>().Object,
              new Mock<IServiceProvider>().Object,
              new Mock<ILogger<UserManager<PortFreightUser>>>().Object)
        { }

        public override Task<IdentityResult> CreateAsync(PortFreightUser user, string password)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        public override Task<string> GenerateEmailConfirmationTokenAsync(PortFreightUser user)
        {
            return Task.FromResult(Guid.NewGuid().ToString());
        }
    }

    [TestClass]
    public class InviteColleagueTests
    {
        private PageContext pageContext;
        private ViewDataDictionary viewData;
        private TempDataDictionary tempData;
        private ActionContext actionContext;
        private Mock<ILogger<InviteColleagueModel>> logger;
        private Mock<IEmailSender> emailSender;
        private InviteColleagueModel inviteColleagueModel;
        private UserDbContext userContext;

        [TestInitialize]
        public void Setup()
        {
            var optionsBuilderUser = new DbContextOptionsBuilder<UserDbContext>()
              .UseInMemoryDatabase("InMemoryUserDb")
              .Options;

            var httpContext = new DefaultHttpContext();
            tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var modelState = new ModelStateDictionary();
            actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

            pageContext = new PageContext(actionContext)
            {
                ViewData = viewData
            };


            logger = new Mock<ILogger<InviteColleagueModel>>();
            emailSender = new Mock<IEmailSender>();
            userContext = new UserDbContext(optionsBuilderUser);

            FakeUserManager fakeUserManager = new FakeUserManager();
            FakeSignInManager fakeSignInManager = new FakeSignInManager();

            inviteColleagueModel = new InviteColleagueModel(
                                   fakeUserManager,
                                   userContext,
                                   logger.Object,
                                   emailSender.Object)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };
        }

        public static Mock<UserManager<TUser>> MockUserManager<TUser>(List<TUser> ls) where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());

            mgr.Setup(x => x.DeleteAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.CreateAsync(It.IsAny<TUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success).Callback<TUser, string>((x, y) => ls.Add(x));
            mgr.Setup(x => x.UpdateAsync(It.IsAny<TUser>())).ReturnsAsync(IdentityResult.Success);

            return mgr;
        }

        [TestMethod]
        [Ignore]
        public async Task OnInvite_WhenPassInvalidUser_ModelStateThrowsanError()
        {
            var user = new PortFreightUser() { UserName = "TestUser1", Email = "user1@dft.gov.uk", PasswordHash = "TestTest1!" };

            var inputModel = new InviteColleagueModel.InputModel
            {
                Email = user.Email
            };

            inviteColleagueModel.Input = inputModel;
            inviteColleagueModel.ModelState.AddModelError("Sender Id", "Sender Id is required.");

            var result = await inviteColleagueModel.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }
    }
}
