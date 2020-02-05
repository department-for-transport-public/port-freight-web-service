using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Web.Pages.Profile;
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

namespace PortFreight.Web.Tests.Profile
{
    [TestClass]
    public class ContactDetailsTests
    {
        private PortFreightContext actualContext;
        private PageContext pageContext;
        private DefaultHttpContext httpContext;
        private ViewDataDictionary viewData;
        private TempDataDictionary tempData;
        private ActionContext actionContext;
        private Mock<UserManager<PortFreightUser>> mockUserManager;
        private ContactDetailsModel model;
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

            model = new ContactDetailsModel(mockUserManager.Object, actualContext)
            {
                PageContext = pageContext,
                Url = new UrlHelper(actionContext),
                ContactDetails = new ContactDetails()
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
            Assert.IsNull(model.ContactDetails.SenderId);
            Assert.AreEqual("/Account/Logout", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        [TestMethod]
        public async Task OnGet_WhenUserIsLoggedIn_ReturnsPageResultWithSenderId()
        {
            var result = (PageResult)await model.OnGetAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(portFreightUsers.FirstOrDefault().SenderId, model.ContactDetails.SenderId);
            Assert.IsInstanceOfType(result, typeof(PageResult));

        }

        [TestMethod]
        public async Task OnGet_WhenContactDetailsAvailableInDb_PopulateModel()
        {
            var contactDetails = new ContactDetails()
            {
                SenderId="Test1234",
                CompanyName = "A company",
                City = "City"
            };
            await actualContext.ContactDetails.AddAsync(contactDetails);
            actualContext.SaveChanges();

            var result = (PageResult)await model.OnGetAsync();

            Assert.IsNotNull(result);
            Assert.AreEqual(portFreightUsers.FirstOrDefault().SenderId, model.ContactDetails.SenderId);
            Assert.AreEqual(contactDetails.SenderId, model.ContactDetails.SenderId);
            Assert.AreEqual(contactDetails.CompanyName, model.ContactDetails.CompanyName);
            Assert.AreEqual(contactDetails.City, model.ContactDetails.City);
            Assert.IsNull(model.ContactDetails.ContactName);
            Assert.IsInstanceOfType(result, typeof(PageResult));

        }

        [TestMethod]
        public async Task OnPost_WhenModelStateIsInValid_ReturnsPageResult()
        {
            model.ModelState.AddModelError("Input.CompanyName", "Enter a company name");

            var result = await model.OnPostAsync();

            Assert.IsInstanceOfType(result, typeof(PageResult));
        }
        [TestMethod]
        public async Task OnPost_WhenModelStateIsValid_ReturnsARedirectToPageResult()
        {
            model.ContactDetails = new ContactDetails()
            {
                SenderId = "Test123",
                Addr1 = "One street",
                Addr2 = "Addr 2",
                Addr3 = "Addr 3",
                City = "City",
                CompanyName = "A company",
                ContactName = "A contact",
                County = "A county",
                Phone = "01273 747785",
                Postcode = "BN1 2BB"
            };

            var result = (PageResult)await model.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }
    }
}
