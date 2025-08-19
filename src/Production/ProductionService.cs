using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling;
using AasDemoapp.AasHandling.SubmodelCreators;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Import;
using AasDemoapp.Proxy;
using AasDemoapp.Settings;
using AasDemoapp.Utils.Shells;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace AasDemoapp.Production
{
    public class ProductionService
    {
        private readonly AasDemoappContext _context;
        private readonly ImportService _importService;
        private readonly SettingService _settingService;
        private readonly ProxyService _proxyService;
        private readonly ILogger<ProductionService> _logger;

        public ProductionService(AasDemoappContext aasDemoappContext, ImportService importService, SettingService settingService, ProxyService proxyService, ILogger<ProductionService> logger)
        {
            _context = aasDemoappContext;
            _importService = importService;
            _settingService = settingService;
            _proxyService = proxyService;
            _logger = logger;
        }

        public async Task<ProducedProduct> CreateProduct(ProducedProductRequest producedProductRequest)
        {

            var aasId = IdGenerationUtil.GenerateId(IdType.Aas, "https://oi4-nextbike.de");
            var globalAssetId = IdGenerationUtil.GenerateId(IdType.Asset, "https://oi4-nextbike.de");
            var securitySetting = _settingService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);
            var semanticID = "https://admin-shell.io/idta/CarbonFootprint/CarbonFootprint/1/0";

            // Produkt zusammenbauen
            var accumulatedPCF = 0.0; // TODO: kann der Wert schon vorbefüllt sein?
            foreach (var component in producedProductRequest.BestandteilRequests)
            {
                var ids = _proxyService.Discover(_settingService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "",
                    securitySetting, component.GlobalAssetId);
                var aas_id = ids.Result[0];
                var submodelRepositoryUrl = _settingService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.Value ?? "";
                var aasRegistryUrl = _settingService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "";
                var submodelRegistryUrl = _settingService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.Value ?? "";
                var aasRepositoryUrl = _settingService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "";
                LoadShellResult componentAAS = await ShellLoader.LoadAsync(
                    new AasUrls
                    {
                        AasRepositoryUrl = aasRepositoryUrl,
                        SubmodelRepositoryUrl = submodelRepositoryUrl,
                        AasRegistryUrl = aasRegistryUrl,
                        SubmodelRegistryUrl = submodelRegistryUrl
                    }, securitySetting, aas_id, default);
                var componentPCF = 10.0;
                //var  pcfSubmodel = componentAAS.Environment.Submodels.Find(submodel => submodel.SemanticId == semanticID);
                // SubmodelElementList pcfList = (SubmodelElementList)pcfSubmodel.SubmodelElements.Find(elem => elem.IdShort == "ProductCarbonFootprints");

                // todo: check if semantic ID = semanticID
                // todo: foreach SMC in (shortID = ProductCarbonFootprints)
                // todo: get value of property with idShort = PcfCO2eq and add it to overall PCD of bike
                accumulatedPCF += componentPCF * component.Amount;
            }

            accumulatedPCF *= 1.2; // 20 % extra for mounting
            
            var producedProduct = new ProducedProduct()
            {
                ConfiguredProductId = producedProductRequest.ConfiguredProductId,
                ProductionDate = DateTime.Now,
                AasId = aasId,
                GlobalAssetId = globalAssetId,
                PCFValue = accumulatedPCF
            };

            var configuredProduct = await _context.ConfiguredProducts.FirstAsync(c => c.Id == producedProductRequest.ConfiguredProductId);

            producedProductRequest.BestandteilRequests.ForEach((bestandteil) =>
            {
                var katalogEintrag = _context.KatalogEintraege.First(b => b.GlobalAssetId == bestandteil.GlobalAssetId);
                producedProduct.Bestandteile.Add(new ProductPart()
                {
                    Amount = bestandteil.Amount,
                    Name = katalogEintrag.Name,
                    Price = katalogEintrag.Price,
                    UsageDate = bestandteil.UsageDate,
                    KatalogEintrag = katalogEintrag
                });
                katalogEintrag.Amount = katalogEintrag.Amount - bestandteil.Amount;
            });

            _context.ProducedProducts.Add(producedProduct);
            _context.SaveChanges();

            var product = await _context.ProducedProducts
                .AsNoTracking()
                .Include(p => p.ConfiguredProduct)
                .Include(p => p.Bestandteile)
                .ThenInclude(b => b.KatalogEintrag)
                .FirstAsync(p => p.Id == producedProduct.Id);

            try
            {
                // Create InstanceAAS for Bike
                var aas = await InstanceAasCreator.CreateBikeInstanceAas(product, _importService, _settingService);
                producedProduct.AasId = aasId;
                producedProduct.GlobalAssetId = globalAssetId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating InstanceAAS for product {ProductId}", producedProduct.Id);
            }

            return producedProduct;
        }

    }
}