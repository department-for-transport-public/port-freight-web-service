using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.EmailSender;
using PortFreight.Web.Areas.Identity.Pages.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortFreight.Web.Tests.UserAccount
{
    [TestClass]
    public class UserLoginTest
    {
        private PageContext pageContext;
        private ViewDataDictionary viewData;
        private TempDataDictionary tempData;
        private ActionContext actionContext;
        private Mock<ILogger<LoginModel>> logger;
        private Mock<IConfiguration> mockConfig;
        private Mock<IEmailSender> mockEmailSender = new Mock<IEmailSender>();
        private Mock<FakeSignInManager> mockfakeSignInManager = new Mock<FakeSignInManager>();
        private Mock<FakeUserManager> mockUserManager = new Mock<FakeUserManager>();
        private LoginModel loginModel;
        private PortFreightContext actualContext;
        private List<PortFreightUser> portFreightUsersList;

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

        [TestInitialize]
        public void Setup()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PortFreightContext>()
               .UseInMemoryDatabase("InMemoryDb")
               .Options;
            actualContext = new PortFreightContext(optionsBuilder);

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


            mockConfig = new Mock<IConfiguration>();
            logger = new Mock<ILogger<LoginModel>>();

            loginModel = new LoginModel(
                mockfakeSignInManager.Object,
                logger.Object,
                mockConfig.Object,
                mockUserManager.Object,
                actualContext,
                mockEmailSender.Object)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };

        }

        [TestCleanup]
        public void Cleanup()
        {
            actualContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task OnLogin_WhenPassSkipProfileUsers_RedirectToApiKey()
        {
            var portFreightUser = new PortFreightUser()
            {
                UserName = "TestUser1",
                Email = "user1@dft.gov.uk",
                PasswordHash = "TestTest1!",
                SenderId = "T12345"
            };
            portFreightUsersList = new List<PortFreightUser>();
            portFreightUsersList.Add(portFreightUser);

            loginModel.Input = new LoginModel.InputModel
            {
                Email = portFreightUser.Email,
                Password = portFreightUser.PasswordHash,
                RememberMe = true
            };

            mockConfig.Setup(x => x[It.IsAny<string>()]).Returns("T12345");

            mockfakeSignInManager.Setup(x => x.PasswordSignInAsync(loginModel.Input.Email, loginModel.Input.Password, loginModel.Input.RememberMe, true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(portFreightUsersList.FirstOrDefault());

            var result = (RedirectToPageResult)await loginModel.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("/ApiKey/ApiKey", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        [TestMethod]
        public async Task OnLogin_WhenContactDetailsUnavailable_RedirectToContactDetails()
        {
            var portFreightUser = new PortFreightUser()
            {
                UserName = "TestUser1",
                Email = "user1@dft.gov.uk",
                PasswordHash = "TestTest1!",
                SenderId = "T12345"
            };
            portFreightUsersList = new List<PortFreightUser>();
            portFreightUsersList.Add(portFreightUser);

            loginModel.Input = new LoginModel.InputModel
            {
                Email = portFreightUser.Email,
                Password = portFreightUser.PasswordHash,
                RememberMe = true
            };

            mockfakeSignInManager.Setup(x => x.PasswordSignInAsync(loginModel.Input.Email, loginModel.Input.Password, loginModel.Input.RememberMe, true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(portFreightUsersList.FirstOrDefault());

            var result = (RedirectToPageResult)await loginModel.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("/Profile/ContactDetails", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        [TestMethod]
        public async Task OnLogin_WhenRespondentDetailsUnavailable_RedirectToRespondentDetails()
        {
            var portFreightUser = new PortFreightUser()
            {
                UserName = "TestUser1",
                Email = "user1@dft.gov.uk",
                PasswordHash = "TestTest1!",
                SenderId = "T12345"
            };
            portFreightUsersList = new List<PortFreightUser>();
            portFreightUsersList.Add(portFreightUser);

            loginModel.Input = new LoginModel.InputModel
            {
                Email = portFreightUser.Email,
                Password = portFreightUser.PasswordHash,
                RememberMe = true
            };

            mockfakeSignInManager.Setup(x => x.PasswordSignInAsync(loginModel.Input.Email, loginModel.Input.Password, loginModel.Input.RememberMe, true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(portFreightUsersList.FirstOrDefault());

            var ContactDetails = new ContactDetails {SenderId = "T12345"};
            await actualContext.ContactDetails.AddAsync(ContactDetails);
            actualContext.SaveChanges();

            var result = (RedirectToPageResult)await loginModel.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("/Profile/RespondentDetails", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        [TestMethod]
        public async Task OnLogin_WhenProfileComplete_RedirectToDashboard()
        {
            var portFreightUser = new PortFreightUser()
            {
                UserName = "TestUser1",
                Email = "user1@dft.gov.uk",
                PasswordHash = "TestTest1!",
                SenderId = "T12345"
            };
            portFreightUsersList = new List<PortFreightUser>();
            portFreightUsersList.Add(portFreightUser);

            loginModel.Input = new LoginModel.InputModel
            {
                Email = portFreightUser.Email,
                Password = portFreightUser.PasswordHash,
                RememberMe = true
            };

            mockfakeSignInManager.Setup(x => x.PasswordSignInAsync(loginModel.Input.Email, loginModel.Input.Password, loginModel.Input.RememberMe, true)).ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(portFreightUsersList.FirstOrDefault());

            var ContactDetails = new ContactDetails {SenderId = "T12345"};
            await actualContext.ContactDetails.AddAsync(ContactDetails);
            actualContext.SaveChanges();

            var SenderType = new SenderType {SenderId = "T12345"};
            await actualContext.SenderType.AddAsync(SenderType);
            actualContext.SaveChanges();

            var result = (RedirectToPageResult)await loginModel.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("/Dashboard", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }
    }
}
