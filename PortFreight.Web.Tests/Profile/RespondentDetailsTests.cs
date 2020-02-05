using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Web.Pages.Profile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortFreight.Web.Tests.Profile
{
    [TestClass]
    public class RespondentDetailsTests
    {
        private PortFreightContext actualContext;
        private PageContext pageContext;
        private DefaultHttpContext httpContext;
        private ViewDataDictionary viewData;
        private TempDataDictionary tempData;
        private ActionContext actionContext;
        private Mock<UserManager<PortFreightUser>> mockUserManager;
        private RespondentDetailsModel model;
        private List<PortFreightUser> portFreightUsers;
        HelperService commonFunction;

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

            httpContext = new DefaultHttpContext();
            tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var modelState = new ModelStateDictionary();
            actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

            pageContext = new PageContext(actionContext)
            {
                ViewData = viewData
            };

            portFreightUsers = new List<PortFreightUser>
                 {
                      new PortFreightUser() { Id = "TestUser1", Email ="TestUser1@dft.gov.uk",SenderId = "Test1234",UserName = "TestUser1" },
                      new PortFreightUser() { Id = "TestUser2", Email ="TestUser2@dft.gov.uk",SenderId = "Test5678",UserName = "TestUser2" }
                 };

            mockUserManager = MockUserManager<PortFreightUser>(portFreightUsers);
            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault());

            actualContext = new PortFreightContext(optionsBuilder);

            commonFunction = new HelperService(actualContext);

            model = new RespondentDetailsModel(mockUserManager.Object, actualContext, commonFunction)
            {
                PageContext = pageContext,
                Url = new UrlHelper(actionContext),
                SenderType = new SenderType(),
                SenderIdPort = new SenderIdPort()
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task OnGet_WhenUserIsNotLoggedIn_RedirectsToLogoutPage()
        {
            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault(x => x.Id is null));

            var result = (RedirectToPageResult)await model.OnGetAsync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.PageName);
            Assert.IsNull(model.SenderType.SenderId);
            Assert.AreEqual("/Account/Logout", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        [TestMethod]
        public async Task OnGet_WhenUserIsLoggedIn_ReturnsPageResultWithSenderId()
        {

            var result = (PageResult)await model.OnGetAsync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(model.SenderType.SenderId);
            Assert.AreEqual(portFreightUsers.FirstOrDefault().SenderId, model.SenderType.SenderId);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [Ignore]
        [TestMethod]
        public async Task OnGet_WhenRespondentDetailsAvailableInDb_PopulateModel()
        {
            var senderType = new SenderType()
            {
                SenderId = "Test1234",
                IsAgent = true,
                IsLine = false,
                IsPort = false
            };
            var senderIdPort = new SenderIdPort
            {
                SenderId = "Test1234",
                Locode = "ABC456"
            };
            await actualContext.SenderType.AddAsync(senderType);
            await actualContext.SenderIdPort.AddAsync(senderIdPort);
            await actualContext.SaveChangesAsync();

            var result = (PageResult)await model.OnGetAsync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(model.SenderType.SenderId);
            Assert.IsTrue(model.SenderType.IsAgent);
            Assert.IsFalse(model.SenderType.IsLine);
            Assert.AreEqual(portFreightUsers.FirstOrDefault().SenderId, model.SenderType.SenderId);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [TestMethod]
        public async Task OnPost_WhenModelStateIsInValid_ReturnsPageResult()
        {
            model.ModelState.AddModelError("Input.RespondentDetails", "Select Respondent");

            var result = (PageResult)await model.OnPostAsync();

            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [Ignore]
        [TestMethod]
        public async Task OnPost_WhenModelStateIsValid_ReturnsARedirectToPageResult()
        {
            model.SenderIdPort = new SenderIdPort
            {
                SenderId = "Test1234",
                Locodes = new List<string>
                {
                    "AAA123" , "ABC456"
                }
            };

            model.SenderType = new SenderType
            {
                SenderId = "Test1234",
                IsAgent = true,
                IsLine = false,
                IsPort = true
            };

            var result = (RedirectToPageResult)await model.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("/Dashboard", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        [Ignore]
        [TestMethod]
        public async Task OnPost_WhenEditingSenderType_ReturnsARedirectToPageResult()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PortFreightContext>()
                .UseInMemoryDatabase("InMemoryDb")
                .Options;
            var context = new PortFreightContext(optionsBuilder);
            var senderType = new SenderType()
            {
                SenderId = "Test5678",
                IsAgent = true,
                IsLine = false,
                IsPort = false
            };
            await context.SenderType.AddAsync(senderType);
            await context.SaveChangesAsync();

            model.SenderIdPort = new SenderIdPort
            {
                SenderId = "Test5678",
                Locodes = new List<string>
                {
                    "AAA123" , "ABC456"
                }
            };

            model.SenderType = new SenderType
            {
                SenderId = "Test5678",
                IsAgent = true,
                IsLine = false,
                IsPort = true
            };

            var result = (RedirectToPageResult)await model.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("/Dashboard", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        [Ignore]
        [TestMethod]
        public async Task OnPost_WhenPortWithoutLocodes_ReturnsPageResult()
        {
            model.SenderType = new SenderType
            {
                SenderId = "Test5678",
                IsAgent = true,
                IsLine = false,
                IsPort = true
            };

            var result = (PageResult)await model.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.IsFalse(viewData.ModelState.IsValid);
            Assert.IsTrue(viewData.ModelState.ErrorCount == 1);
            Assert.AreEqual("Select the ports for which you submit data", viewData.ModelState["PortError"].Errors[0].ErrorMessage);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }
    }
}
