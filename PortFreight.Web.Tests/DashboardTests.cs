using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Web.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Threading.Tasks;
using PortFreight.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;

namespace PortFreight.Web.Tests
{
    [TestClass]
    public class DashboardTests
    {
        private PortFreightContext actualContext;
        private DefaultHttpContext httpContext;
        private ActionContext actionContext;
        private Mock<UserManager<PortFreightUser>> mockUserManager;
        private DashboardModel model;
        private List<PortFreightUser> portFreightUsers;


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

            httpContext = new DefaultHttpContext();
            TempDataDictionary tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            var modelState = new ModelStateDictionary();
            actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            var modelMetadataProvider = new EmptyModelMetadataProvider();
            var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);

            PageContext pageContext = new PageContext(actionContext)
            {
                ViewData = viewData
            };

            portFreightUsers = new List<PortFreightUser>
            {
                new PortFreightUser() { Id="TestUser1", SenderId="Test1234", UserName="TestUser1" },
            };

            mockUserManager = MockUserManager<PortFreightUser>(portFreightUsers);

            model = new DashboardModel(mockUserManager.Object, actualContext)
            {
                PageContext = pageContext,
                Url = new UrlHelper(actionContext)
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task OnGet_WhenUserIsNotLoggedIn_ReturnsRedirectToPageResult()
        {
            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault(x => x.Id is null));

            var result = (RedirectToPageResult)await model.OnGetAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("/Account/Logout", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        [TestMethod]
        public async Task OnGet_WhenContactDetailsIncomplete_ReturnsContactDetailsPage()
        {
            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault());
            var contactDetails = new List<ContactDetails>
            {
                new ContactDetails() {SenderId="TestOther"}
            };
            await actualContext.ContactDetails.AddRangeAsync(contactDetails);
            actualContext.SaveChanges();

            var result = (RedirectToPageResult)await model.OnGetAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("/Profile/ContactDetails", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        [TestMethod]
        public async Task OnGet_WhenRespondentDetailsIncomplete_ReturnsRespondentDetailsPage()
        {
            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault());
            var contactDetails = new List<ContactDetails>
            {
                new ContactDetails() {SenderId="Test1234"}
            };
            await actualContext.ContactDetails.AddRangeAsync(contactDetails);
            actualContext.SaveChanges();

            var senderType = new List<SenderType>
            {
                new SenderType() {SenderId="TestOther"}
            };
            await actualContext.SenderType.AddRangeAsync(senderType);
            actualContext.SaveChanges();

            var result = (RedirectToPageResult)await model.OnGetAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual("/Profile/RespondentDetails", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }
    }
}
