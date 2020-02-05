using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
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
using Newtonsoft.Json;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using PortFreight.Web.Models;
using PortFreight.Web.Pages.Msd1;
using static PortFreight.Web.Pages.Msd1.VoyageDetailsModel;
using PortFreight.Services.Common;
using PortFreight.Web.Utilities;

namespace PortFreight.Web.Tests.Pages.Msd1
{
    [TestClass]
    public class VoyageDetailsTests
    {
        PortFreightContext actualContext;
        PageContext pageContext;
        ViewDataDictionary viewData;
        TempDataDictionary tempData;
        ActionContext actionContext;
        InputModel input;
        ValidationContext ValidationContext;
        private HelperService CommonFunction;

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

            MSD1 Msd1 = new MSD1();
            tempData["MSD1Key"] = JsonConvert.SerializeObject(Msd1);

            CommonFunction = new HelperService(actualContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualContext.Dispose();
        }

        [TestMethod]
        public async Task OnGet_GoodTempData_GoodContext_ReturnsPageResult()
        {
            List<GlobalPort> ReportingPorts = new List<GlobalPort>()
            {
                new GlobalPort { Locode = "LOCODE_TEST", PortName = "PORTNAME_TEST", CountryCode = "GB" }
            };

            await actualContext.GlobalPort.AddRangeAsync(ReportingPorts);
            actualContext.SaveChanges();

            MSD1 Msd1 = new MSD1();
            tempData["MSD1Key"] = JsonConvert.SerializeObject(Msd1);

            var model = new VoyageDetailsModel(actualContext, CommonFunction)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext)
            };

            var result = model.OnGet();

            Assert.IsInstanceOfType(result, typeof(PageResult));
        }
        [Ignore]
        [TestMethod]
        public void OnPost_WhenModelStateIsValid_ReturnsARedirectToPageResult()
        {

        input = new InputModel()
            {
                ReportingPort = "Newport, Isle of Wight / UNITED KINGDOM (GBNPO)",
                IsInbound = true,
                AssociatedPort = "Newcastle-Upon-Tyne / UNITED KINGDOM (GBNCL)",
                NumVoyages = "4",
                Year = "2018",
                Quarter = "1",
                RecordRef = "qewtweieorpr",
                VoyageDateDay = 1,
                VoyageDateMonth = 1,
                VoyageDateYear = 2019
            };

            var model = new VoyageDetailsModel(actualContext, CommonFunction)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext),
                Input = input
            };

            var result = model.OnPost("Newcastle-Upon-Tyne / UNITED KINGDOM (GBNCL)", "Newport, Isle of Wight / UNITED KINGDOM (GBNPO)");

            Assert.IsInstanceOfType(result, typeof(RedirectToPageResult));
        }
        [Ignore]
        [TestMethod]
        public void OnPost_WhenModelStateIsInValid_ReturnsPageResult()
        {
            var model = new VoyageDetailsModel(actualContext, CommonFunction)
            {
                PageContext = pageContext,
                TempData = tempData,
                Url = new UrlHelper(actionContext),
                Input = input
            };

            model.ModelState.AddModelError("Input.ReportingPort", "Enter a valid Reporting port");
            model.ModelState.AddModelError("Input.IsInbound", "Select a direction of travel");
            model.ModelState.AddModelError("Input.Year", "Select a year");

            var result = model.OnPost("Newcastle-Upon-Tyne / UNITED KINGDOM (GBNCL)", "Newport, Isle of Wight / UNITED KINGDOM (GBNPO)");

            Assert.IsTrue(viewData.ModelState.ContainsKey("Input.ReportingPort"));
            Assert.IsTrue(viewData.ModelState.ContainsKey("Input.IsInbound"));
            Assert.IsTrue(viewData.ModelState.ContainsKey("Input.Year"));
            Assert.IsTrue(viewData.ModelState.ErrorCount == 3);
            Assert.IsInstanceOfType(result, typeof(PageResult));

        }

        [TestMethod]
        public void OnPost_WhenNumVoyagesIsInValid_ReturnErrorMessage()
        {
            NumVoyagesAttribute numVoyagesAttribute = new NumVoyagesAttribute();

            input = new InputModel()
            {
                NumVoyages = "1001",
            };

            ValidationContext = new ValidationContext(input);

            var result = numVoyagesAttribute.GetValidationResult(null, ValidationContext);

            Assert.AreEqual("Enter number of voyages between 1 - 1000", result.ErrorMessage);
        }

        [TestMethod]
        public void OnPost_WhenAssociatedPortIsInValid_ReturnErrorMessage()
        {
            AssociatedPortAttribute associatedPortAttribute = new AssociatedPortAttribute();

            input = new InputModel()
            {
                AssociatedPort = "Tilbury",
                ReportingPort = "Tilbury"
            };

            ValidationContext = new ValidationContext(input);

            var result = associatedPortAttribute.GetValidationResult(null, ValidationContext);

            Assert.AreEqual("The two ports must be different", result.ErrorMessage);
        }

        [TestMethod]
        public void OnPost_WhenQuarterIsInValid_ReturnErrorMessage()
        {
            QuarterAttribute quarterAttribute = new QuarterAttribute()
            {
                currentQuarter = 1,
                currentYear = "2018"
            };

            input = new InputModel()
            {
                Quarter = "3",
                Year = "2018"
            };

            ValidationContext = new ValidationContext(input);

            var result = quarterAttribute.GetValidationResult(null, ValidationContext);

            Assert.AreEqual("Voyage must be in the past", result.ErrorMessage);
        }

        [TestMethod]
        public void OnPost_WhenDayDateIsInValid_ReturnErrorMessage()
        {
            DateDayAttribute DateDayAttribute = new DateDayAttribute();

            input = new InputModel()
            {
                VoyageDateDay = 32
            };

            ValidationContext = new ValidationContext(input);

            var result = DateDayAttribute.GetValidationResult(null, ValidationContext);

            Assert.AreEqual("The day must be between 1 and 31", result.ErrorMessage);
        }

        [TestMethod]
        public void OnPost_WhenMonthDateIsInValid_ReturnErrorMessage()
        {
            DateMonthAttribute DateMonthAttribute = new DateMonthAttribute();

            input = new InputModel()
            {
                VoyageDateMonth = 13
            };

            ValidationContext = new ValidationContext(input);

            var result = DateMonthAttribute.GetValidationResult(null, ValidationContext);

            Assert.AreEqual("The month must be between 1 and 12", result.ErrorMessage);
        }

        [TestMethod]
        public void OnPost_WhenQuarterAndDatesAreInValid_ReturnErrorMessage()
        {
            DateCheckQuarterAttribute voyageDateCheckQuarterAttribute = new DateCheckQuarterAttribute();

            input = new InputModel()
            {
                Quarter = "3",
                VoyageDateDay = 1,
                VoyageDateMonth = 1,
                VoyageDateYear = 2019
            };

            ValidationContext = new ValidationContext(input);

            var result = voyageDateCheckQuarterAttribute.GetValidationResult(null, ValidationContext);

            Assert.AreEqual("The date entered is not within Quarter 3", result.ErrorMessage);
        }

        [TestMethod]
        public void OnPost_WhenYearAndYearInputAreDifferent_ReturnErrorMessage()
        {
            DateYearAttribute voyageDateYearAttribute = new DateYearAttribute();

            input = new InputModel()
            {
                Year = "2018",
                VoyageDateYear = 2019
            };

            ValidationContext = new ValidationContext(input);

            var result = voyageDateYearAttribute.GetValidationResult(null, ValidationContext);

            Assert.AreEqual("The year does not match the year you previously selected", result.ErrorMessage);
        }

        [TestMethod]
        public void OnPost_IfOnlyPartOfTheDateIsFilledIn_ReturnErrorMessage()
        {
            ValidateEitherNoneOrAllEntriesAreFilled voyageValidateEitherNoneOrAllEntriesAreFilled = new ValidateEitherNoneOrAllEntriesAreFilled();

            input = new InputModel()
            {
                VoyageDateDay = 11
            };

            ValidationContext = new ValidationContext(input);

            var result = voyageValidateEitherNoneOrAllEntriesAreFilled.GetValidationResult(null, ValidationContext);

            Assert.AreEqual("You must either leave all date fields blank or fill all date fields in", result.ErrorMessage);
        }

    }

}