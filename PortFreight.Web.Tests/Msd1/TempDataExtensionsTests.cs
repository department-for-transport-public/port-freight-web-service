using Microsoft.VisualStudio.TestTools.UnitTesting;

using PortFreight.Web.Utilities;
using Moq;
using PortFreight.Web.Models;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using PortFreight.Data;
using PortFreight.Data.Entities;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace PortFreight.Web.UnitTests.Tests
{
    [TestClass]
    public class TempDataExtensionsTests
    {
        PortFreightContext actualContext;

        [TestInitialize]
        public void Setup()
        {
            var optionsBuilder = new DbContextOptionsBuilder<PortFreightContext>()
                 .UseInMemoryDatabase("InMemoryDb")
                 .Options;

            actualContext = new PortFreightContext(optionsBuilder);
        }


        [TestMethod]
        public void SingleWriteReadReturnsCorrectValue()
        {
            var MSD1 = new MSD1();
            uint vesselImo = 1234;
            MSD1.Imo = vesselImo;

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            tempData.Put("testKey", MSD1);

            var MSD1_out = tempData.Get<MSD1>("testkey");

            Assert.AreEqual(vesselImo, MSD1_out.Imo);
        }

        [TestMethod]
        public void SingleWriteMultipleReadReturnsCorrectValues()
        {
            var MSD1 = new MSD1();
            uint vesselImo = 1234;
            MSD1.Imo = vesselImo;

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            tempData.Put("testKey", MSD1);

            var MSD1_out = tempData.Get<MSD1>("testkey");
            var MSD1_out_again = tempData.Get<MSD1>("testkey");

            Assert.AreEqual(vesselImo, MSD1_out.Imo);
            Assert.AreEqual(vesselImo, MSD1_out_again.Imo);
        }

        [TestMethod]
        public void SingleWriteBadReadReturnsNull()
        {
            var MSD1 = new MSD1();
            uint vesselImo = 1234;
            MSD1.Imo = vesselImo;

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            tempData.Put("testKey", MSD1);

            var MSD1_out = tempData.Get<MSD1>("testkey_junk");
            
            Assert.IsNull(MSD1_out);  
        }

        [TestMethod]
        public async Task WriteReadTypesWithCircularDependency()
        {
            byte testGroupCode = 99;
            byte testGroupCode2 = 100;
            

            var CargoCategories = new List<CargoCategory>()
            {
                new CargoCategory {GroupCode = testGroupCode,
                                   CategoryCode = 23,
                                   GroupCodeNavigation = new CargoGroup() {CargoCategory = new List<CargoCategory>(), GroupCode = testGroupCode }
                                  },
                                   
                new CargoCategory {GroupCode = testGroupCode2,
                                   CategoryCode = 24,
                                   GroupCodeNavigation = new CargoGroup() {CargoCategory = new List<CargoCategory>(), GroupCode = testGroupCode2 }
                                  }
            };

            await actualContext.CargoCategory.AddRangeAsync(CargoCategories);
            actualContext.SaveChanges();

            var cargoCategory = actualContext.CargoCategory.Where(x => x.GroupCode == testGroupCode);

            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            tempData.Put("testKey", cargoCategory);

            var CargoOut = tempData.Get<List<CargoCategory>>("testkey");

            Assert.IsTrue(cargoCategory.Count() == 1);
        }
    }
}
