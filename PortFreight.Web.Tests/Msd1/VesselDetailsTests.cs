using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Web.Pages.Msd1;
using PortFreight.Web.Models;
using PortFreight.Web.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PortFreight.Web.Tests.Pages.Msd1
{
    [TestClass]
    public class VesselDetailsTests
    {
        PortFreightContext actualContext;
        PageContext pageContext;
        ViewDataDictionary viewData;
        TempDataDictionary tempData;
        ActionContext actionContext;
        HelperService helperService;
        private Mock<ILogger<VesselDetailsModel>> logger;
        private VesselDetailsModel vesselDetailsModel;

        [TestInitialize]
        public void Setup()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PortFreightContext>()
                .UseInMemoryDatabase("InMemoryDb")
                .Options;
            var optionsBuilderUser = new DbContextOptionsBuilder<UserDbContext>()
                .UseInMemoryDatabase("InMemoryUserDb")
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

            MSD1 Msd1 = new MSD1();
            tempData["MSD1Key"] = JsonConvert.SerializeObject(Msd1);
            logger = new Mock<ILogger<VesselDetailsModel>>();
            helperService = new HelperService(actualContext);
            vesselDetailsModel = new VesselDetailsModel(actualContext, logger.Object, helperService)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualContext.Dispose();
        }

        [TestMethod]
        public void OnGet_NoChecks_ReturnsPageResult()
        {
            var result = vesselDetailsModel.OnGet();

            Assert.IsInstanceOfType(result, typeof(PageResult));
        }


        [TestMethod]
        public void OnPost_FailsImoCheck_ReturnsAPageResult()
        {
            vesselDetailsModel.ModelState.AddModelError("Vessel", "Enter a valid IMO number or Vessel Name");

            var result = vesselDetailsModel.OnPost(null);

            Assert.IsTrue(viewData.ModelState.ContainsKey("Vessel"));
            Assert.IsFalse(viewData.ModelState.IsValid);
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [Ignore]
        [TestMethod]
        public async Task OnPost_PassesImoCheck_ReturnsARedirectToPageResult()
        {
            uint Imo = 1234567;
            var WorldFleet = new List<WorldFleet>()
            {
                new WorldFleet {Imo = Imo }
            };
            await actualContext.WorldFleet.AddRangeAsync(WorldFleet);
            actualContext.SaveChanges();

            vesselDetailsModel.Vessel = "1234567 - Ship Name (FLG)";

            var result = (RedirectToPageResult)vesselDetailsModel.OnPost("1234567 - Ship Name (FLG)");

            Assert.AreEqual("./VoyageDetails", result.PageName);
            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }
    }
}