using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Data.Entities;

namespace PortFreight.Services.Test
{
    [TestClass]
    public class ApiKeyServiceTest
    {
        private ApiKey _apiKey;
        private Mock<IApiKeyDataStore> mockIApiKeyDataStore;
        private IApiKeyService _IApiKeyService;

        [TestInitialize]
        public void SetUp()
        {
            _apiKey = new ApiKey
            {
                Id = "Service1234",
                Token = "1328d3d1-c371-491c-932e-25767d7service",
                Source = "ASCII"
            };
            mockIApiKeyDataStore = new Mock<IApiKeyDataStore>();
            _IApiKeyService = new ApiKeyService(mockIApiKeyDataStore.Object);

        }
        [TestMethod]
        public void OnCreateUpdateApiKey_whenPassingApiKey_ShouldAddOrModifyApiKey()
        {
            mockIApiKeyDataStore.Setup(repo => repo.Get(_apiKey.Id)).Returns(_apiKey);


            mockIApiKeyDataStore.Setup(repo => repo.Upsert(_apiKey)).Verifiable();

            var result = _IApiKeyService.CreateApiKey(_apiKey.Id, DataSource.WEB);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(bool));
            Assert.IsTrue(result);
            Assert.IsNotNull(_IApiKeyService.GetApiKey(_apiKey.Id));
            Assert.AreEqual(_apiKey.Token, _IApiKeyService.GetApiKey(_apiKey.Id).Token);
        }


        [TestMethod]
        public void OnGetApiKey_whenPassingApiKeyId_ShoulReturnApiKey()
        {
            var id = "Service1234";
            mockIApiKeyDataStore.Setup(repo => repo.Get(_apiKey.Id)).Returns(_apiKey);


            mockIApiKeyDataStore.Setup(repo => repo.Upsert(_apiKey)).Verifiable();

            var result = _IApiKeyService.GetApiKey(id);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ApiKey));
            Assert.AreEqual(id, result.Id);
            Assert.AreEqual(_apiKey.Token, result.Token);
        }
    }
}
