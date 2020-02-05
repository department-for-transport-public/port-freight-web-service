using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Data.Entities;
using PortFreight.Services.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortFreight.Services.Test
{
    [TestClass]
    public class CargoPortValidateServiceTest
    {
        PortFreightContext actualContext;
        public bool valid = false;
        private readonly CargoPortValidateService _cargoPortValidateService;
        private readonly Mock<IHelperService> _helperService;

        public CargoPortValidateServiceTest()
        {
            _helperService = new Mock<IHelperService>();
            var optionsBuilder = new DbContextOptionsBuilder<PortFreightContext>()
                .UseInMemoryDatabase("InMemoryDb")
                .Options;

            actualContext = new PortFreightContext(optionsBuilder);
            _cargoPortValidateService = new CargoPortValidateService(actualContext, _helperService.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            actualContext.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task IsValidPortForCategory_WhenPassed_ShouldReturnTrue()
        {
            byte categoryCode = 12;
            string portCode = "GB221";
            string reportingPort = "Kings Ferry / UNITED KINGDOM (GB221)";
            string associatedPort = "Offshore oil and gas terminal or platform (UK)/  (ZZOF1)";
            bool InBound = true;

            _helperService.Setup(x => x.GetPortCodeByPort(It.IsAny<string>())).Returns("GB221");
            var portCargoOil = new List<PortCargoOil>()
            {
                new PortCargoOil{
                Locode = portCode,
                AllowCategory12 = true,
                AllowCategory13Outward = false }
            };

            var globalPort = new List<GlobalPort>()
            {
                new GlobalPort{
                Locode = portCode,
                PortName = reportingPort,
                CountryCode = "GB" }
            };

            await actualContext.PortCargoOil.AddRangeAsync(portCargoOil);
            await actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();

            var result = _cargoPortValidateService.IsValidPortForCategory(categoryCode, reportingPort, associatedPort, InBound);
            var category = portCargoOil.Find(x => x.AllowCategory12 == true).AllowCategory12;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsTrue(result);
            Assert.IsNotNull(category);
            Assert.AreEqual(result, category);
        }

        [TestMethod]
        public void IsNotValidPortForCategory_WhenFailed_ShouldReturnFalse()
        {
            byte categoryCode = 12;
            string portCode = "GBCRN";
            string reportingPort = "Cromarty Firth / UNITED KINGDOM (GBCRN)";
            string associatedPort = "Offshore oil and gas terminal or platform (UK)/  (ZZOF1)";
            bool InBound = true;

            _helperService.Setup(x => x.GetPortCodeByPort(It.IsAny<string>())).Returns("GBCRN");
            var portCargoOil = new List<PortCargoOil>()
            {
                new PortCargoOil{
                Locode = portCode,
                AllowCategory12 = false,
                AllowCategory13Outward = false }
            };

            var globalPort = new List<GlobalPort>()
            {
                new GlobalPort{
                Locode = portCode,
                PortName = reportingPort,
                CountryCode = "GB" }
            };

            actualContext.PortCargoOil.AddRangeAsync(portCargoOil);
            actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();
            var result = _cargoPortValidateService.IsValidPortForCategory(categoryCode, reportingPort, associatedPort, InBound);
            var category = portCargoOil.Find(x => x.AllowCategory12 == false).AllowCategory12;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsFalse(result);
            Assert.IsNotNull(category);
            Assert.AreEqual(result, category);
        }

        [TestMethod]
        public void IsNotValidPortForCategory_WhenInward_ShouldReturnFalse()
        {
            byte categoryCode = 13;
            string portCode = "ZZ0F1";
            string reportingPort = "Aberdeen / UNITED KINGDOM (GBABD)";
            string associatedPort = "Offshore oil and gas terminal or platform (UK)/  (ZZOF1)";
            bool InBound = true;

            _helperService.Setup(x => x.GetCountryCodeByPort(It.IsAny<string>())).Returns("GB");
            var portCargoOil = new List<PortCargoOil>()
            {
                new PortCargoOil{
                Locode = portCode,
                AllowCategory12 = true,
                AllowCategory13Outward = false }
            };

            var globalPort = new List<GlobalPort>()
            {
                new GlobalPort{
                Locode = portCode,
                PortName = associatedPort,
                CountryCode = "GB" }
            };

            actualContext.PortCargoOil.AddRangeAsync(portCargoOil);
            actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();

            var result = _cargoPortValidateService.IsValidPortForCategory(categoryCode, reportingPort, associatedPort, InBound);
            var category = portCargoOil.Find(x => x.AllowCategory13Outward == false).AllowCategory13Outward;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsFalse(result);
            Assert.IsNotNull(category);
            Assert.AreEqual(result, category);
        }

        [TestMethod]
        public void IsNotValidPortForCategory_WhenInward_ShouldReturnTrue()
        {
            byte categoryCode = 13;
            string portCode = "GBDUN";
            string reportingPort = "Aberdeen / UNITED KINGDOM (GBABD)";
            string associatedPort = "Dundee / UNITED KINGDOM (GBDUN)";
            bool InBound = true;

            _helperService.Setup(x => x.GetCountryCodeByPort(It.IsAny<string>())).Returns("GB");
            _helperService.Setup(x => x.GetPortCodeByPort(It.IsAny<string>())).Returns("GBDUN");
            var portCargoOil = new List<PortCargoOil>()
            {
                new PortCargoOil{
                Locode = portCode,
                AllowCategory12 = false,
                AllowCategory13Outward = true }
            };

            var globalPort = new List<GlobalPort>()
            {
                new GlobalPort{
                Locode = portCode,
                PortName = associatedPort,
                CountryCode = "GB" }
            };

            actualContext.PortCargoOil.AddRangeAsync(portCargoOil);
            actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();

            var result = _cargoPortValidateService.IsValidPortForCategory(categoryCode, reportingPort, associatedPort, InBound);
            var category = portCargoOil.Find(x => x.AllowCategory13Outward == true).AllowCategory13Outward;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsTrue(result);
            Assert.IsNotNull(category);
            Assert.AreEqual(result, category);
        }

        [TestMethod]
        public void IsValidPortForCategory_WhenOutward_ShouldReturnTrue()
        {
            byte categoryCode = 13;
            string portCode = "GBGTY";
            string reportingPort = "Great Yarmouth / UNITED KINGDOM (GBGTY)";
            string associatedPort = "Rivers Hull & Humber / UNITED KINGDOM (GB221)";
            bool InBound = false;

            _helperService.Setup(x => x.GetCountryCodeByPort(It.IsAny<string>())).Returns("GB");
            _helperService.Setup(x => x.GetPortCodeByPort(It.IsAny<string>())).Returns("GBGTY");
            var portCargoOil = new List<PortCargoOil>()
            {
                new PortCargoOil{
                Locode = portCode,
                AllowCategory12 = false,
                AllowCategory13Outward = true }
            };

            var globalPort = new List<GlobalPort>()
            {
                new GlobalPort{
                Locode = portCode,
                PortName = reportingPort,
                CountryCode = "GB" }
            };

            actualContext.PortCargoOil.AddRangeAsync(portCargoOil);
            actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();

            var result = _cargoPortValidateService.IsValidPortForCategory(categoryCode, reportingPort, associatedPort, InBound);
            var category = portCargoOil.Find(x => x.AllowCategory13Outward == true).AllowCategory13Outward;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsTrue(result);
            Assert.IsNotNull(category);
            Assert.AreEqual(result, category);
        }

        [TestMethod]
        public async Task IsValidCategoryForPort_WhenValid_Passed_ShouldReturnTrue()
        {
            string categoryDescription = "Liquefied Gas";
            string portCode = "ZZOF1";
            string associatedPort = "Offshore oil and gas terminal or platform (UK)/  (ZZOF1)";

            _helperService.Setup(x => x.GetPortCodeByPort(It.IsAny<string>())).Returns("ZZOF1");
            var globalPort = new List<GlobalPort>()
            {
                new GlobalPort{
                Locode = portCode,
                PortName = associatedPort,
                CountryCode = "GB" }
            };

            await actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();
            var result = _cargoPortValidateService.IsValidCategoryForPort(categoryDescription, associatedPort);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsTrue(result);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public async Task IsValidCategoryForPort_WhenNotValid_ShouldReturnFalse()
        {
            string categoryDescription = "Iron and Steel Products";
            string portCode = "GBPTB";
            string associatedPort = "Port Talbot / UNITED KINGDOM (GBPTB)";

            _helperService.Setup(x => x.GetPortCodeByPort(It.IsAny<string>())).Returns("GBPTB");
            var globalPort = new List<GlobalPort>()
            {
                new GlobalPort{
                Locode = portCode,
                PortName = associatedPort,
                CountryCode = "GB" }
            };

            await actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();
            var result = _cargoPortValidateService.IsValidCategoryForPort(categoryDescription, associatedPort);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsFalse(result);
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public async Task IsValidCategoryForPort_WhenInavlid_Failed_ShouldReturnFalse()
        {
            string categoryDescription = "Ores including scrap";
            string portCode = "ZZOF1";
            string associatedPort = "Offshore oil and gas terminal or platform (UK)/  (ZZOF1)";

            _helperService.Setup(x => x.GetPortCodeByPort(It.IsAny<string>())).Returns("ZZOF1");
            var globalPort = new List<GlobalPort>()
            {
                new GlobalPort{
                Locode = portCode,
                PortName = associatedPort,
                CountryCode = "GB" }
            };

            await actualContext.GlobalPort.AddRangeAsync(globalPort);
            actualContext.SaveChanges();
            var result = _cargoPortValidateService.IsValidCategoryForPort(categoryDescription, associatedPort);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsFalse(result);
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public async Task HasTonnagePerUnitExceeded_WhenNotExceeded_Passed_ShouldReturnTrue()
        {
            int categoryCode = 31;
            uint totalUnits = 1;
            uint UnitsWithoutCargo = 0;
            uint units = 1;
            uint grossweight = 30;

            var cargoCategory = new List<CargoCategory>()
            {
                new CargoCategory{
                CategoryCode = 31,
                Description = "20' Freight Units",
                MaxWeight = 30
                }
            };

            await actualContext.CargoCategory.AddRangeAsync(cargoCategory);
            actualContext.SaveChanges();
            var result = _cargoPortValidateService.HasTonnagePerUnitExceeded(categoryCode, totalUnits, units, UnitsWithoutCargo, grossweight);

            Assert.IsNotNull(result.Item3);
            Assert.IsNotNull(result.Item2);
            Assert.IsNotNull(result.Item1);
            Assert.IsInstanceOfType(result.Item3, typeof(bool));
            Assert.IsInstanceOfType(result.Item1, typeof(double));
            Assert.IsInstanceOfType(result.Item2, typeof(double));
            Assert.IsFalse(result.Item3);
            Assert.AreNotEqual(result.Item3, true);
            Assert.AreEqual(result.Item1, result.Item2);
        }

        [TestMethod]
        public async Task HasTonnagePerUnitExceeded_WhenExceeded_Failed_ShouldReturnFalse()
        {
            int categoryCode = 31;
            uint totalUnits = 1;
            uint UnitsWithoutCargo = 0;
            uint units = 2;
            uint grossweight = 100;

            var cargoCategory = new List<CargoCategory>()
            {
                new CargoCategory{
                CategoryCode = 31,
                Description = "20' Freight Units",
                MaxWeight = 30
                }
            };

            await actualContext.CargoCategory.AddRangeAsync(cargoCategory);
            actualContext.SaveChanges();
            var result = _cargoPortValidateService.HasTonnagePerUnitExceeded(categoryCode, totalUnits, units, UnitsWithoutCargo, grossweight);

            Assert.IsNotNull(result.Item2);
            Assert.IsNotNull(result.Item1);
            Assert.IsNotNull(result.Item3);
            Assert.IsInstanceOfType(result.Item3, typeof(bool));
            Assert.IsInstanceOfType(result.Item1, typeof(double));
            Assert.IsInstanceOfType(result.Item2, typeof(double));
            Assert.IsTrue(result.Item3);
            Assert.AreEqual(result.Item3, true);
            Assert.AreNotEqual(result.Item1, result.Item2);
        }

        [TestMethod]
        public async Task HasUnitsWithoutCargo_WhenZero_Passed_ShouldReturnTrue()
        {
            int categoryCode = 56;
            uint totalUnits = 2;
            uint UnitsWithoutCargo = 0;
            uint UnitsWithCargo = 2;
            uint grossweight = 1;
            double allowedTonnageHeavyCow = 0.5;

            var cargoCategory = new List<CargoCategory>()
            {
                new CargoCategory{
                CategoryCode = 56,
                Description = "Live Animals on the Hoof",
                MaxWeight = 0
                }
            };

            await actualContext.CargoCategory.AddRangeAsync(cargoCategory);
            actualContext.SaveChanges();
            var result = _cargoPortValidateService.HasTonnagePerUnitExceeded(categoryCode, totalUnits, UnitsWithCargo, UnitsWithoutCargo, grossweight);

            Assert.IsNotNull(result.Item4);
            Assert.IsNotNull(result.Item3);
            Assert.IsInstanceOfType(result.Item3, typeof(bool));
            Assert.IsInstanceOfType(result.Item4, typeof(bool));
            Assert.IsTrue(result.Item4);
            Assert.IsFalse(result.Item3);
            Assert.AreEqual(result.Item3, false);
            Assert.AreEqual(result.Item4, true);
            Assert.AreEqual(result.Item1, result.Item2);
        }

        [TestMethod]
        public async Task HasUnitsWithoutCargo_WhenOverZero_Failed_ShouldReturnFalse()
        {
            int categoryCode = 56;
            uint totalUnits = 2;
            uint UnitsWithoutCargo = 1;
            uint UnitsWithCargo = 1;
            uint grossweight = 1;

            var cargoCategory = new List<CargoCategory>()
            {
                new CargoCategory{
                CategoryCode = 56,
                Description = "Live Animals on the Hoof",
                MaxWeight = 0 
                }
            };

            await actualContext.CargoCategory.AddRangeAsync(cargoCategory);
            actualContext.SaveChanges();
            var result = _cargoPortValidateService.HasTonnagePerUnitExceeded(categoryCode, totalUnits, UnitsWithCargo, UnitsWithoutCargo, grossweight);

            Assert.IsNotNull(result.Item4);
            Assert.IsInstanceOfType(result.Item4, typeof(bool));
            Assert.IsFalse(result.Item4);
            Assert.AreEqual(result.Item4, false);
        }

        [TestMethod]
        public void IsValidVesselCargo_WhenValid_ShouldReturnTrue()
        {
            byte categoryCode = 56;
            uint imo = 7052363;
            sbyte ShipTypeCode = 12;

            var worldFleet = new List<WorldFleet>()
            {
                new WorldFleet{
                Imo = imo,
                ShipTypeCode = ShipTypeCode
                 }
            };

            var shipCargoCategory = new List<ShipCargoCategory>()
            {
                new ShipCargoCategory{
                CargoCategoryCode = 56,
                ShipTypeCode = ShipTypeCode
                 }
            };

            actualContext.WorldFleet.AddRangeAsync(worldFleet);
            actualContext.ShipCargoCategory.AddRangeAsync(shipCargoCategory);
            actualContext.SaveChanges();

            var result = _cargoPortValidateService.IsValidVesselCargo(categoryCode, imo);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsTrue(result);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void IsValidVesselCargo_WhenInValid_ShouldReturnFalse()
        {
            byte categoryCode = 56;
            uint imo = 7052363;
            sbyte ShipTypeCode2 = 101;
            sbyte ShipTypeCode = 56;

            var worldFleet = new List<WorldFleet>()
            {
                new WorldFleet{
                Imo = imo,
                ShipTypeCode = ShipTypeCode
                 }
            };

            var shipCargoCategory = new List<ShipCargoCategory>()
            {
                new ShipCargoCategory{
                CargoCategoryCode = 56,
                ShipTypeCode = ShipTypeCode2
                 }
            };

            actualContext.WorldFleet.AddRangeAsync(worldFleet);
            actualContext.ShipCargoCategory.AddRangeAsync(shipCargoCategory);
            actualContext.SaveChanges();

            var result = _cargoPortValidateService.IsValidVesselCargo(categoryCode, imo);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsFalse(result);
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void IsValidVesselCargo_NoShipCode_WhenNotFoundDefault_ShouldReturnTrue()
        { 
            byte categoryCode = 56;
            uint imo = 7052363;
            sbyte ShipTypeCode2 = 101;
            sbyte ShipTypeCode = 0;

            var worldFleet = new List<WorldFleet>()
            {
                new WorldFleet{
                Imo = imo,
                ShipTypeCode = ShipTypeCode
                 }
            };
            var shipCargoCategory = new List<ShipCargoCategory>()
            {
                new ShipCargoCategory{
                CargoCategoryCode = 56,
                ShipTypeCode = ShipTypeCode2
                 }
            };

            actualContext.WorldFleet.AddRangeAsync(worldFleet);
            actualContext.ShipCargoCategory.AddRangeAsync(shipCargoCategory);
            actualContext.SaveChanges();

            var result = _cargoPortValidateService.IsValidVesselCargo(categoryCode, imo);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsTrue(result);
            Assert.AreEqual(result, true);
        }
    }
}