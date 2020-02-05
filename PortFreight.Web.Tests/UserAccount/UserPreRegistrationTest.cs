using Microsoft.AspNetCore.Http;
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
using PortFreight.Services.User;
using PortFreight.Web.Areas.Identity.Pages.Account;

namespace PortFreight.Web.Tests.UserAccount
{
    [TestClass]
    public class UserPreRegistrationTest
    {
        private UserDbContext _usercontext;
        private PortFreightContext _portfreightContext;
        private IUserService _userService;
        private PageContext pageContext;
        private ViewDataDictionary viewData;
        private TempDataDictionary tempData;
        private ActionContext actionContext;
        private PreRegisterModel pageModel;

        [TestInitialize]
        public void Setup()
        {
            var optionsUserBuilder = new DbContextOptionsBuilder<UserDbContext>()
              .UseInMemoryDatabase("InMemoryUserDb")
              .Options;
            var optionsBuilder = new DbContextOptionsBuilder<PortFreightContext>()
              .UseInMemoryDatabase("InMemoryDb")
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

            _usercontext = new UserDbContext(optionsUserBuilder);
            _portfreightContext = new PortFreightContext(optionsBuilder);
            _userService = new UserService(_usercontext, _portfreightContext);
            pageModel = new PreRegisterModel(_usercontext, _userService)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext),
            };

        }

        [TestCleanup]
        public void Cleanup()
        {
            _usercontext.Database.EnsureDeleted();
            _portfreightContext.Database.EnsureDeleted();
        }


        [TestMethod]
        public void OnPostAsync_WhenModelStateIsInvalid_ReturnsAPageResult()
        {
            pageModel.ModelState.AddModelError("SenderId", "Sender Id is required.");
            pageModel.ModelState.AddModelError("EmailAddress", "EmailAddress is required.");

            var result = pageModel.OnPost();

            Assert.IsNotNull(result);
            Assert.IsFalse(viewData.ModelState.IsValid);
            Assert.AreEqual(2, viewData.ModelState.Count);
            Assert.IsNotNull(pageModel);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [TestMethod]
        public void OnPostAsync_WhenUserIsAlreadyRegisteredPortFreight_ShowErrorMsg()
        {
            pageModel.Input = new PreRegisterModel.InputModel { Email = "Test@Test.co.uk", SenderId = "Test1234" };

            var portFreightUser = new PortFreightUser
            {
                Email = "Test@Test.co.uk",
                SenderId = "Test1234"
            };
            _usercontext.Users.Add(portFreightUser);
            _usercontext.SaveChangesAsync();

            var result = (PageResult)pageModel.OnPost();

            Assert.IsNotNull(result);
            Assert.IsTrue(viewData.ModelState.ContainsKey("EmailAlreadyRegistered"));
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [TestMethod]
        public void OnPostAsync_WhenEmailExistButNotSenderIDInPreApprovedUserList_ShowErroMsg()
        {
            var preApprovedUser = new PreApprovedUser
            {
                EmailAddress = "Test@Test.co.uk",
                SenderId = "Test1234"
            };
            _usercontext.PreApprovedUsers.Add(preApprovedUser);
            _usercontext.SaveChangesAsync();
            pageModel.Input = new PreRegisterModel.InputModel { Email = "Test@Test.co.uk", SenderId = "XX1234" };

            var result = (PageResult)pageModel.OnPost();

            Assert.IsNotNull(result);
            Assert.IsFalse(viewData.ModelState.IsValid);
            Assert.AreEqual(1, viewData.ModelState.Count);
            Assert.IsTrue(viewData.ModelState.ContainsKey("CheckDetailsEntered"));
            Assert.IsTrue(viewData.ModelState.ErrorCount == 1);
            Assert.AreEqual("Please check the details entered match those in your invitation email", viewData.ModelState["CheckDetailsEntered"].Errors[0].ErrorMessage);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [TestMethod]
        public void OnPostAsync_WhenEmailDoesnotExistInPreApprovedUserList_ShowErroMsg()
        {
            var preApprovedUser = new PreApprovedUser
            {
                EmailAddress = "Test@Test.co.uk",
                SenderId = "Test1234"
            };
            _usercontext.PreApprovedUsers.Add(preApprovedUser);
            _usercontext.SaveChangesAsync();
            pageModel.Input = new PreRegisterModel.InputModel { Email = "TestTest@Test.co.uk", SenderId = "Test1234" };

            var result = (PageResult)pageModel.OnPost();

            Assert.IsNotNull(result);
            Assert.IsFalse(viewData.ModelState.IsValid);
            Assert.AreEqual(1, viewData.ModelState.Count);
            Assert.IsTrue(viewData.ModelState.ContainsKey("CheckDetailsEntered"));
            Assert.IsTrue(viewData.ModelState.ErrorCount == 1);
            Assert.AreEqual("We are unable to validate your details, please contact helpdesk", viewData.ModelState["CheckDetailsEntered"].Errors[0].ErrorMessage);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [TestMethod]
        public void OnPostAsync_WhenUserIsValidPreApprovedUser_RedirectToCreatePasswordPage()
        {
            var preApprovedUser = new PreApprovedUser
            {
                EmailAddress = "Test@Test.co.uk",
                SenderId = "Test1234"
            };
            _usercontext.PreApprovedUsers.Add(preApprovedUser);
            _usercontext.SaveChangesAsync();

            pageModel.Input = new PreRegisterModel.InputModel { Email = "Test@Test.co.uk", SenderId = "Test1234" };

            var result = (RedirectToPageResult)pageModel.OnPost();

            Assert.IsNotNull(result);
            Assert.AreEqual("Register", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }

        #region IgnoredTest
        [Ignore]
        [TestMethod]
        public void OnPostAsync_WhenSenderIdNotDataSupplier_ShowErrorMsg()
        {
            pageModel.Input = new PreRegisterModel.InputModel { Email = "Test@Test.co.uk", SenderId = "Test432" };

            var result = pageModel.OnPost();

            Assert.IsNotNull(result);
            Assert.IsFalse(viewData.ModelState.IsValid);
            Assert.AreEqual(1, viewData.ModelState.Count);
            Assert.IsTrue(viewData.ModelState.ContainsKey("CheckDetailsEntered"));
            Assert.AreEqual("Sender Id does not exist", viewData.ModelState["Input.SenderId"].Errors[0].ErrorMessage);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }
        [Ignore]
        [TestMethod]
        public void OnPostAsync_WhenUserIsNotinPreApprovedUserList_ShowInvitationErroMsg()
        {
            var preApprovedUser = new PreApprovedUser
            {
                EmailAddress = "Test@Test.co.uk",
                SenderId = "Test1234"
            };
            _usercontext.PreApprovedUsers.Add(preApprovedUser);

            var portFreightUser = new PortFreightUser
            {
                Email = "Test1@Test.co.uk",
                SenderId = "Test1234",
                EmailConfirmed = true
            };
            _usercontext.Users.Add(portFreightUser);
            _usercontext.SaveChangesAsync();

            var org = new OrgList
            {
                OrgId = "TST001",
                SubmitsMsd1 = false,
                SubmitsMsd2 = true
            };
            _portfreightContext.OrgList.Add(org);
            _portfreightContext.SaveChangesAsync();

            pageModel.Input = new PreRegisterModel.InputModel { Email = "TestTest@Test.co.uk", SenderId = "Test1234" };

            var result = (PageResult)pageModel.OnPost();

            Assert.IsNotNull(result);
            Assert.IsFalse(viewData.ModelState.IsValid);
            Assert.AreEqual(1, viewData.ModelState.Count);
            Assert.IsTrue(viewData.ModelState.ContainsKey("Input.SenderId"));
            Assert.IsTrue(viewData.ModelState.ErrorCount == 1);
            Assert.AreEqual("This Sender ID has already been registered. Ask your colleague to send you an invitation or contact helpdesk​", viewData.ModelState["Input.SenderId"].Errors[0].ErrorMessage);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [Ignore]
        [TestMethod]
        public void OnPostAsync_WhenIncorrectEmailRegistered_AllowNewUser()
        {
            var preApprovedUser = new PreApprovedUser
            {
                EmailAddress = "Alice@Test.co.uk",
                SenderId = "TST001"
            };
            _usercontext.PreApprovedUsers.Add(preApprovedUser);

            var portFreightUser = new PortFreightUser
            {
                Email = "Bob@Test.co.uk",
                SenderId = "TST001",
                EmailConfirmed = false
            };
            _usercontext.Users.Add(portFreightUser);
            _usercontext.SaveChangesAsync();

            var org = new OrgList
            {
                OrgId = "TST001",
                SubmitsMsd1 = false,
                SubmitsMsd2 = true
            };
            _portfreightContext.OrgList.Add(org);
            _portfreightContext.SaveChangesAsync();

            pageModel.Input = new PreRegisterModel.InputModel { Email = "Charlie@Test.co.uk", SenderId = "TST001" };

            var result = (RedirectToPageResult)pageModel.OnPost();

            Assert.IsNotNull(result);
            Assert.AreEqual("Register", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }
        #endregion IgnoredTest

    }
}
