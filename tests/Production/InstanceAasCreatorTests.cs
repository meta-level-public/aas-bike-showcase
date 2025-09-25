using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database.Model;
using AasDemoapp.Import;
using AasDemoapp.Production;
using AasDemoapp.Settings;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AasDemoapp.Tests.Production
{
    public class InstanceAasCreatorTests
    {
        private readonly Mock<ImportService> _importServiceMock;
        private readonly Mock<SettingService> _settingServiceMock;
        private readonly ILogger<InstanceAasCreator> _logger;

        public InstanceAasCreatorTests()
        {
            _importServiceMock = new Mock<ImportService>();
            _settingServiceMock = new Mock<SettingService>();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug().AddConsole());
            _logger = loggerFactory.CreateLogger<InstanceAasCreator>();
            InstanceAasCreator.InitializeLogger(_logger);
        }

        private ProducedProduct CreateMinimalProducedProduct()
        {
            var configured = new ConfiguredProduct
            {
                Id = 1,
                Name = "TestBike",
                GlobalAssetId = "urn:demo:configured-product:1",
            };

            var order = new ProductionOrder
            {
                Id = 10,
                ConfiguredProduct = configured,
                ConfiguredProductId = 1,
                Address = new Address
                {
                    Land = "DE",
                    Ort = "Berlin",
                    Plz = "10115",
                    Strasse = "Teststrasse 1",
                },
            };

            return new ProducedProduct
            {
                Id = 100,
                ConfiguredProduct = configured,
                ConfiguredProductId = 1,
                Bestandteile = new List<ProductPart>(),
                GlobalAssetId = "urn:demo:produced-product:100",
                AasId = "urn:aas:produced-product:100",
                Order = order,
                PCFValue = 1.23,
            };
        }

        [Fact]
        public async Task CreateBikeInstanceAas_SetsThumbnailResource()
        {
            // Arrange
            var product = CreateMinimalProducedProduct();

            // Settings: CompanyAddress leer lassen -> default
            _settingServiceMock
                .Setup(s => s.GetSetting(SettingTypes.CompanyAddress))
                .Returns((Setting?)null);
            _settingServiceMock
                .Setup(s => s.GetSetting(SettingTypes.AasRepositoryUrl))
                .Returns((Setting?)null);
            _settingServiceMock
                .Setup(s => s.GetSetting(SettingTypes.SubmodelRepositoryUrl))
                .Returns((Setting?)null);
            _settingServiceMock
                .Setup(s => s.GetSetting(SettingTypes.AasRegistryUrl))
                .Returns((Setting?)null);
            _settingServiceMock
                .Setup(s => s.GetSetting(SettingTypes.SubmodelRegistryUrl))
                .Returns((Setting?)null);
            _settingServiceMock
                .Setup(s => s.GetSecuritySetting(SettingTypes.InfrastructureSecurity))
                .Returns((SecuritySetting?)null);

            // ImportService Mock: SaveAasToRepositories ruft SaveShellSaver.SaveSingle direkt static auf -> wir mocken hier nichts weiter

            // Act
            AssetAdministrationShell aas = await InstanceAasCreator.CreateBikeInstanceAas(
                product,
                _importServiceMock.Object,
                _settingServiceMock.Object
            );

            // Assert
            Assert.NotNull(aas.AssetInformation);
            Assert.NotNull(aas.AssetInformation.DefaultThumbnail);
            Assert.Equal("thumbnail.jpg", aas.AssetInformation.DefaultThumbnail.Path); // Resource("thumbnail.jpg","image/jpeg")
            Assert.Equal("image/jpeg", aas.AssetInformation.DefaultThumbnail.ContentType);
        }
    }
}
