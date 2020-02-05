using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services;
using PortFreight.Web.Pages;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PortFreight.Web.Tests
{
    [TestClass]
    public class ApiKeyWebTest
    {
        private ApiKeyModel pageModel;
        private ApiKey apiKey;
        private Mock<IApiKeyService> mockIApiKeyService;
        private Mock<IConfiguration> mockConfig;
        private List<PortFreightUser> portFreightUsers;
        private Mock<UserManager<PortFreightUser>> mockUserManager;
        private DefaultHttpContext httpContext;
        private ModelStateDictionary modelState;
        private ActionContext actionContext;
        private EmptyModelMetadataProvider modelMetadataProvider;
        private ViewDataDictionary viewData;
        private TempDataDictionary tempData;
        private PageContext pageContext;

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
        public void SetUp()
        {
            httpContext = new DefaultHttpContext();
            modelState = new ModelStateDictionary();
            actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            modelMetadataProvider = new EmptyModelMetadataProvider();
            viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
            tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            pageContext = new PageContext(actionContext)
            {
                ViewData = viewData
            };

            mockIApiKeyService = new Mock<IApiKeyService>();
            mockConfig = new Mock<IConfiguration>();
            portFreightUsers = new List<PortFreightUser>
                 {
                      new PortFreightUser() { Id = "TestUser1", Email ="TestUser1@dft.gov.uk",SenderId = "Test1234",UserName = "TestUser1" },
                      new PortFreightUser() { Id = "TestUser2", Email ="TestUser2@dft.gov.uk",SenderId = "Test5678",UserName = "TestUser2" }
                 };

            mockUserManager = MockUserManager<PortFreightUser>(portFreightUsers);
            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault());

            pageModel = new ApiKeyModel(mockIApiKeyService.Object, mockUserManager.Object, mockConfig.Object)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };

        }

        [TestMethod]
        public void OnGet_whenUserNotExistOrLoggedIn_ReturnPageResultWithouthApiKey()
        {
            var id = "Test5678";
            apiKey = new ApiKey() { Id = "Test1234", Token = "1328d3d1-c371-491c-932e-25767d7webTest", Source = "GESMES" };
            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault(x => x.Id is null));

            mockIApiKeyService.Setup(m => m.GetApiKey(id)).Returns(apiKey);
            var result = pageModel.OnGet();

            Assert.IsNotNull(result);
            Assert.IsNull(pageModel.ApiKey);
        }

        [TestMethod]
        public void OnGet_whenPassingExistingApiKeyId_ReturnSuccessWithApiKey()
        {
            var id = "Test1234";
            apiKey = new ApiKey() { Id = "Test1234", Token = "1328d3d1-c371-491c-932e-25767d7webTest", Source = "ASCII" };

            mockIApiKeyService.Setup(m => m.GetApiKey(id)).Returns(apiKey);
            var result = pageModel.OnGet();

            Assert.IsNotNull(result);
            Assert.IsNotNull(pageModel.ApiKey);
            Assert.AreEqual(apiKey, pageModel.ApiKey);
        }

        [TestMethod]
        public void OnGet_whenPassingNonExistingApiKeyId_ReturnNullAsApiKey()
        {
            var id = "Test5678";
            apiKey = new ApiKey() { Id = "Test1234", Token = "1328d3d1-c371-491c-932e-25767d7webTest" };

            mockIApiKeyService.Setup(m => m.GetApiKey(id)).Returns(apiKey);
            var result = pageModel.OnGet();

            Assert.IsNotNull(result);
            Assert.IsNull(pageModel.ApiKey);
        }

        [TestMethod]
        public async Task OnPostAsync_WhenModelStateIsInvalid_ReturnsAPageResult()
        {
            apiKey = new ApiKey() { Id = "Test1234", Token = "1328d3d1-c371-491c-932e-25767d7webTest" };

            pageModel.ModelState.AddModelError("ApiKey", "Sender Id is required.");

            var result = await pageModel.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.IsNull(pageModel.ApiKey);
            Assert.IsInstanceOfType(result,typeof(PageResult));
        }

        [TestMethod]
        public async Task OnPostAsync_WhenModelStateIsValid_ReturnsSuceesMessageResult()
        {
            pageModel.ApiKey = new ApiKey
                {
                    Id = "Test1234",
                    Token = "API Token"
                };

            mockIApiKeyService.Setup(m => m.CreateApiKey(pageModel.ApiKey.Id, DataSource.ASCII)).Returns(true);
            mockIApiKeyService.Setup(m => m.GetApiKey(pageModel.ApiKey.Id)).Returns(pageModel.ApiKey);

            var result = await pageModel.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(pageModel.ApiKey);
            Assert.IsTrue(pageModel.ModelState.IsValid);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [TestMethod]
        public async Task OnPostAsync_WhenModelStateIsValid_ReturnsModelErrorePageResult()
        {
            pageModel.ApiKey = new ApiKey();

            pageModel.ModelState.AddModelError("ApiKeyError", "Failed to generate a new key");

            var result = await pageModel.OnPostAsync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(pageModel.ApiKey);
            Assert.IsFalse(pageModel.ModelState.IsValid);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }
    }
}
