using Google.Cloud.Datastore.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PortFreight.Data;
using PortFreight.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PortFreight.Data.Tests.ApiKeyTest
{
    [TestClass]
    public class ApiKeyDataStoreTest
    {
        private string _projectId;
        private DatastoreDb _db;
        private Entity _apiKeyTest;
        private string _kind;
        private KeyFactory _keyFactory;
        private ApiKey _apiKey;
        private IApiKeyDataStore _apiKeyDataStoreTest;

        [TestInitialize]
        public void ApiKeyDataStoreTestSetup()
        {
            _apiKey = new ApiKey()
            {
                Id = "Test1234",
                Token = "1328d3d1-c371-491c-932e-25767d7b85f6"
            };
            _apiKeyTest = new Entity()
            {
                ["Id"] = _apiKey.Id,
                ["Token"] = _apiKey.Token,
            };
            var configuration = new ConfigurationBuilder()
                       .AddJsonFile("appsettings.json", optional: true)
                       .AddEnvironmentVariables()
                       .Build();

            _projectId = configuration["GOOGLE_CLOUD_PROJECT"] ?? configuration["Datastore:ProjectId"];

            _apiKeyDataStoreTest = new ApiKeyDataStore(configuration);
        }

        private bool IsValidKey(Key key)
        {
            foreach (var element in key.Path)
            {
                if (element.Id == 0 && string.IsNullOrEmpty(element.Name))
                    return false;
                if (string.IsNullOrEmpty(element.Kind))
                    return false;
            }
            return true;
        }

        [TestMethod]
        public void OnAddingEntity_WhenPassingKey_ValidateNamedKey()
        {
            _db = DatastoreDb.Create(_projectId, "ApiKeyDataStoreTest");
            _kind = "ApiKey";
            _keyFactory = _db.CreateKeyFactory(_kind);

            Key key = _keyFactory.CreateKey(_apiKey.Id);

            Assert.IsTrue(IsValidKey(key));
            Assert.IsNotNull(key.Path.FirstOrDefault(x => x.Kind == _kind).Kind);
        }

        [TestMethod]
        public void OnUpsert_WhenPassValidEntity_ShouldAddedOrUpdatedToDataStore()
        {
            _apiKey = new ApiKey
            {
                Id = "Test1234",
                Token = "1328d3d1-c371-491c-932e-25767d7btest"
            };

            _apiKeyDataStoreTest.Upsert(_apiKey);

            Assert.IsNotNull(_apiKeyDataStoreTest.Get(_apiKey.Id));
            Assert.AreEqual(_apiKey.Id, _apiKeyDataStoreTest.Get(_apiKey.Id).Id);
            Assert.AreEqual(_apiKey.Token, _apiKeyDataStoreTest.Get(_apiKey.Id).Token);
        }

        [TestMethod]
        public void OnDelete_WhenEntityIsExist_ShouldDeleteFromDataStore()
        {
            _apiKey = new ApiKey
            {
                Id = "Test1234",
                Token = "1328d3d1-c371-491c-932e-25767d7btest"
            };
            _apiKeyDataStoreTest.Upsert(_apiKey);

            _apiKeyDataStoreTest.Delete(_apiKey.Id);

            Assert.IsNull(_apiKeyDataStoreTest.Get(_apiKey.Id));
        }
    }
}
