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
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PortFreight.Web.Utilities;
using PortFreight.Services;
using PortFreight.Services.Common;

namespace PortFreight.Web.Tests.Pages.Msd1
{
    [TestClass]
    public class CargoDetailsTests
    {
        PortFreightContext actualContext;
        CargoPortValidateService cargoPortValidateService;
        PageContext pageContext;
        ViewDataDictionary viewData;
        TempDataDictionary tempData;
        ActionContext actionContext;
        HelperService helperService;

        [TestInitialize]
        public void Setup()
        {
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

            actualContext = new PortFreightContext(optionsBuilder);

            cargoPortValidateService = new CargoPortValidateService(null, helperService);
            helperService = new HelperService(actualContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualContext.Dispose();
        }

        [TestMethod]
        public async Task OnGet_GoodTempData_GoodContext_ReturnsPageResult()
        {
            var CargoGroups = new List<CargoGroup>()
            {
                new CargoGroup {Description = "TEST_DESCRIPTION" }
            };

            await actualContext.CargoGroup.AddRangeAsync(CargoGroups);
            actualContext.SaveChanges();
            MSD1 Msd1 = new MSD1();
            tempData["MSD1Key"] = JsonConvert.SerializeObject(Msd1);

            var model = new CargoDetailsModel(actualContext, cargoPortValidateService)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };

            var result = await model.OnGetAsync();
            Assert.IsInstanceOfType(result, typeof(PageResult));
        }

        [TestMethod]
        public async Task OnGetCargoCategories_GoodData()
        {
            byte testGroupCode = 45;
            byte testGroupCode2 = 46;
            var testDescription = "TEST_DESCRIPTION";
            var testDescription2 = "TEST_DESCRIPTION2";

            var CargoGroups = new List<CargoGroup>()
            {
                new CargoGroup {Description = testDescription, GroupCode = testGroupCode, CargoCategory = new List<CargoCategory>() },
                new CargoGroup {Description = testDescription2, GroupCode = testGroupCode2, CargoCategory = new List<CargoCategory>() }
            };

            var CargoCategories = new List<CargoCategory>()
            {
                new CargoCategory {GroupCode = testGroupCode, CategoryCode = 123},
                new CargoCategory {GroupCode = testGroupCode2, CategoryCode = 124}
            };

            await actualContext.CargoGroup.AddRangeAsync(CargoGroups);
            await actualContext.CargoCategory.AddRangeAsync(CargoCategories);
            actualContext.SaveChanges();

            var model = new CargoDetailsModel(actualContext, cargoPortValidateService)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };

            var result = model.OnGetCargoCategories(testDescription);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
        }

        [TestMethod]
        public async Task OnGetUiFormat_GoodData_ExpectNoCargo()
        {
            byte testGroupCode = 101;
            byte testGroupCode2 = 102;
            var testDescription = "TEST_DESCRIPTION";
            var testDescription2 = "TEST_DESCRIPTION2";
            string CargoGroupKey = "CargoGroupKey";
            string CargoCategoryKey = "CargoCategoryKey";
            string expectedResult = "NoCargo";
            CargoGroup CargoGroup = new CargoGroup()

            { Description = testDescription, IsUnitised = true, GroupCode = testGroupCode, CargoCategory = new List<CargoCategory>() };

            var CargoCategories = new List<CargoCategory>()
            {
                new CargoCategory {GroupCode = testGroupCode, CategoryCode = 25, Description = testDescription},
                new CargoCategory {GroupCode = testGroupCode2, CategoryCode = 26, Description = testDescription2}
            };

            await actualContext.CargoGroup.AddRangeAsync(CargoGroup);
            await actualContext.CargoCategory.AddRangeAsync(CargoCategories);
            actualContext.SaveChanges();

            tempData.Put(CargoGroupKey, CargoGroup);
            tempData.Put(CargoCategoryKey, CargoCategories);

            var model = new CargoDetailsModel(actualContext, cargoPortValidateService)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };

            var result = model.OnGetUiFormat(testDescription);
            Assert.AreEqual(result.Value, expectedResult);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
        }

        [TestMethod]
        public async Task OnGetUiFormat_GoodData_ExpectCargoWeight()
        {
            byte testGroupCode = 103;
            byte testGroupCode2 = 104;
            var testDescription = "TEST_DESCRIPTION";
            var testDescription2 = "TEST_DESCRIPTION2";
            string CargoGroupKey = "CargoGroupKey";
            string CargoCategoryKey = "CargoCategoryKey";
            string expectedResult = "CargoWeight";
            var CargoGroup = new CargoGroup()
            { Description = testDescription, IsUnitised = true, GroupCode = testGroupCode, CargoCategory = new List<CargoCategory>() };

            var CargoCategories = new List<CargoCategory>()
            {
                new CargoCategory {GroupCode = testGroupCode, CategoryCode = 27, Description = testDescription, TakesCargo = true, HasWeight = true},
                new CargoCategory {GroupCode = testGroupCode2, CategoryCode = 28, Description = testDescription2}
            };

            await actualContext.CargoGroup.AddRangeAsync(CargoGroup);
            await actualContext.CargoCategory.AddRangeAsync(CargoCategories);
            actualContext.SaveChanges();

            tempData.Put(CargoGroupKey, CargoGroup);
            tempData.Put(CargoCategoryKey, CargoCategories);

            var model = new CargoDetailsModel(actualContext, cargoPortValidateService)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };

            var result = model.OnGetUiFormat(testDescription);
            Assert.AreEqual(result.Value, expectedResult);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
        }

        [TestMethod]
        [Ignore]
        public void OnPostValidation_GoodData_ExpectCargoWeight()
        {
        }
        [TestMethod]
        [Ignore]
        public void OnPostValidation_BadData_ExpectValidation()
        {
        }

    }
}
