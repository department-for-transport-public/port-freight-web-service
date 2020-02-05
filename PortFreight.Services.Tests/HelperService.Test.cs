using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortFreight.Services.Tests
{
    [TestClass]
    public class HelperServiceTest
    {
        PortFreightContext actualContext;
        private readonly IHelperService _helperService;

        public HelperServiceTest()
        {
            DbContextOptions<PortFreightContext> optionsBuilder = new DbContextOptionsBuilder<PortFreightContext>()
                .UseInMemoryDatabase("InMemoryDb")
                .Options;

            actualContext = new PortFreightContext(optionsBuilder);
            _helperService = new HelperService(actualContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task IsValidCategory_WhenPassed_ShouldReturnValidCode()
        {
            string strcategory1 = "Crude Oil";
            string strcategory2 = "Oil Products";
            byte categoryCode12 = 12;
            byte categoryCode13 = 13;
            byte groupCode = 10;

            var CargoGroup = new CargoGroup()
            {
                Description = strcategory1,
                IsUnitised = true,
                GroupCode = groupCode,
                CargoCategory = new List<CargoCategory>()
            };

            var CargoCategories = new List<CargoCategory>()
                {
                    new CargoCategory {GroupCode = groupCode, CategoryCode = categoryCode12, Description = strcategory1},
                    new CargoCategory {GroupCode = groupCode, CategoryCode = categoryCode13, Description = strcategory2}
                };

            await actualContext.CargoGroup.AddRangeAsync(CargoGroup);
            await actualContext.CargoCategory.AddRangeAsync(CargoCategories);
            actualContext.SaveChanges();

            var result = _helperService.GetCategoryCodeByDescription(strcategory1);
            var category = CargoCategories.Find(x => x.Description == strcategory1).CategoryCode;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(byte));
            Assert.IsNotNull(category);
            Assert.AreEqual(result, category);
        }

        [TestMethod]
        public async Task IsValidPort_Whentrue_ShouldReturnValidPortCode()
        {
            string portCodeGB = "GBABD";
            string portCodeAU = "AU888";
            string portNameGB = "Aberdeen / UNITED KINGDOM (GBABD)";
            string portNameAU = "AUSTRALIA - Not Specified / AUSTRALIA (AU888)";

            var globalPort = new List<GlobalPort>()
            {
                    new GlobalPort{
                Locode = portCodeGB,
                PortName = portNameGB,
                CountryCode = "GB" },

                    new GlobalPort{
                Locode = portCodeAU,
                PortName = portNameAU,
                CountryCode = "AU" }
            };
            await actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();

            var result = _helperService.GetPortCodeByPort(portNameGB);
            var category = globalPort.Find(x => x.PortName == portNameGB).Locode;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(string));
            Assert.IsNotNull(category);
            Assert.AreEqual(result, category);
        }

        [TestMethod]
        public async Task ReturnValidCountryCode_WhenPassed_ShouldReturnTrue()
        {
            string portCodeGB = "GBABD";
            string portNameGB = "Aberdeen / UNITED KINGDOM (GBABD)";
            string portCode1 = "GB070";
            string portCode2 = "GB085";
            string portName1 = "Red Bay / UNITED KINGDOM (GB070)";
            string portName2 = "Arran / UNITED KINGDOM (GB085)";

            var globalPort = new List<GlobalPort>()
            {
                    new GlobalPort{
                Locode = portCodeGB,
                PortName = portNameGB,
                CountryCode = "GB" },

                    new GlobalPort{
                Locode = portCode2,
                PortName = portName2,
                CountryCode = "GB" }
            };
            await actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();

            var result = _helperService.GetCountryCodeByPort(portName2);
            var category = globalPort.Find(x => x.PortName == portName2).CountryCode;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(string));
            Assert.IsNotNull(category);
            Assert.AreEqual(result, category);
        }
    }
}
