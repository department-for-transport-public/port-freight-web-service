using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Web.Pages.Msd1;
using PortFreight.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static PortFreight.Web.Pages.Msd1.CompanyDetailsModel;
using PortFreight.Services.Common;
using Microsoft.AspNetCore.Identity;

namespace PortFreight.Web.Tests.Msd1
{
    [TestClass]
    public class CompanyDetailsTests
    {
        PortFreightContext actualContext;
        UserDbContext actualUserContext;
        List<PortFreightUser> portFreightUsers;
        Mock<UserManager<PortFreightUser>> mockUserManager;
        PageContext pageContext;
        ViewDataDictionary viewData;
        TempDataDictionary tempData;
        ActionContext actionContext;
        HelperService CommonFunction;
        CompanyDetailsModel pageModel;
        DefaultHttpContext httpContext;
        private EmptyModelMetadataProvider modelMetadataProvider;

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
            var optionsBuilderUser = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase("InMemoryUserDb")
                .Options;

            httpContext = new DefaultHttpContext();
            var modelState = new ModelStateDictionary();
            actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
            modelMetadataProvider = new EmptyModelMetadataProvider();
            viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
            tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            pageContext = new PageContext(actionContext)
            {
                ViewData = viewData
            };

            actualContext = new PortFreightContext(optionsBuilder);
            actualUserContext = new UserDbContext(optionsBuilderUser);

            CommonFunction = new HelperService(actualContext);

            portFreightUsers = new List<PortFreightUser>
            {
                new PortFreightUser() { Id = "1", Email ="naresh.hooda@gmail.com",SenderId = "DFT007",UserName = "naresh.hooda@gmail.com" }
            };

            mockUserManager = MockUserManager(portFreightUsers);
            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault());

            MSD1 Msd1 = new MSD1();
            tempData["MSD1Key"] = JsonConvert.SerializeObject(Msd1);

            var orgList = new List<OrgList>()
            {
                new OrgList{OrgId="ABC123", OrgName="The ABC Company", IsAgent=true, IsLine=false, IsPort=false},
                new OrgList{OrgId="ABC007", OrgName="Another ABC Company", IsAgent=false, IsLine=true, IsPort=false},
                new OrgList{OrgId="XYZ123", OrgName="X Y Z ltd", IsAgent=false, IsLine=false, IsPort=true}
            };
            actualContext.OrgList.AddRange(orgList);

            var contactDetails = new List<ContactDetails>()
            {
                new ContactDetails{CompanyName ="Sun and Send Cargo Ltd", SenderId="DFT007"},
                new ContactDetails{CompanyName ="blahblah", SenderId="DFT001"},
                new ContactDetails{CompanyName ="Monshur Industries", SenderId="DFT003"}
            };
            actualContext.ContactDetails.AddRange(contactDetails);
            actualContext.SaveChanges();
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualContext.Database.EnsureDeleted();
            actualUserContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task OnGet_GoodTempData_GoodContext_ReturnsResult()
        {
            var portFreightUser = new PortFreightUser
            {
                Id = "1",
                Email = "naresh.hooda@gmail.com",
                SenderId = "DFT007"
            };
            actualUserContext.Users.Add(portFreightUser);
            await actualUserContext.SaveChangesAsync();

            var senderType = new List<SenderType>()
            {
               new SenderType{SenderId="DFT007",IsAgent=true,IsLine=false,IsPort=true}
            };

            actualContext.SenderType.AddRange(senderType);
            await actualContext.SaveChangesAsync();

            MSD1 Msd1 = new MSD1();
            tempData["MSD1Key"] = JsonConvert.SerializeObject(Msd1);

            pageModel = new CompanyDetailsModel(actualContext, CommonFunction, mockUserManager.Object)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };

            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault(x => x.Id == "1"));

            var result = await pageModel.OnGetAsync();

            Assert.IsNotNull(result);
            Assert.IsNotNull(pageModel.AgentSenders);
            Assert.IsNotNull(pageModel.LineSenders);
            Assert.IsNotNull(pageModel.Input.AgentSenderId);
            Assert.IsNull(pageModel.Input.LineSenderId == "" ? null : pageModel.Input.LineSenderId);
            Assert.IsNotNull(pageModel.User);
            Assert.AreEqual(pageModel.loggedInUser.Email, portFreightUser.Email);
        }

        [TestMethod]
        public async Task OnPost_ModelStateInvalid_ReturnsPageResult()
        {
            var portFreightUser = new PortFreightUser
            {
                Id = "1",
                Email = "naresh.hooda@gmail.com",
                SenderId = "DFT007"
            };
            actualUserContext.Users.Add(portFreightUser);
            await actualUserContext.SaveChangesAsync();

            var senderType = new List<SenderType>()
            {
               new SenderType{SenderId="DFT007",IsAgent=true,IsLine=false,IsPort=true}
            };
            actualContext.SenderType.AddRange(senderType);
            actualContext.SaveChanges();

            MSD1 Msd1 = new MSD1();
            tempData["MSD1Key"] = JsonConvert.SerializeObject(Msd1);

            pageModel = new CompanyDetailsModel(actualContext, CommonFunction, mockUserManager.Object)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };

            mockUserManager.Setup(p => p.GetUserAsync(httpContext.User)).ReturnsAsync(portFreightUsers.FirstOrDefault(x => x.Id == "1"));

            pageModel.ModelState.AddModelError("LineSenderId", "Line's Sender Id is required.");
            var result = pageModel.OnPost();

            Assert.IsNotNull(result);
            Assert.IsNull(pageModel.Input.LineSenderId == "" ? null : pageModel.Input.LineSenderId);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [TestMethod]
        [Ignore]
        public void OnPost_ModelStateValid_ReturnsRedirectToPageResult()
        {

        }
    }
}
