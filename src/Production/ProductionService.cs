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
using AasDemoapp.Settings;
using AasDemoapp.Utils.Shells;
using Microsoft.EntityFrameworkCore;

namespace AasDemoapp.Production
{
    public class ProductionService
    {
        private readonly AasDemoappContext _context;
        private readonly ImportService _importService;
        private readonly SettingService _settingService;
        private readonly ILogger<ProductionService> _logger;

        public ProductionService(AasDemoappContext aasDemoappContext, ImportService importService, SettingService settingService, ILogger<ProductionService> logger)
        {
            _context = aasDemoappContext;
            _importService = importService;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task<ProducedProduct> CreateProduct(ProducedProductRequest producedProductRequest)
        {

            var aasId = IdGenerationUtil.GenerateId(IdType.Aas, "https://oi4-nextbike.de");
            var globalAssetId = IdGenerationUtil.GenerateId(IdType.Asset, "https://oi4-nextbike.de");

            // Produkt zusammenbauen
            var accumulatedPCF = 0; // TODO: kann der Wert schon vorbefüllt sein?
            foreach (var component in producedProductRequest.BestandteilRequests)
            {
                var componentPCF = 0; // TODO: get PCF for component.globalAssetID
                /*
                 * ProxyService.Discover() -> AASID (1. Element in Liste)
                 * AASUrls Objekt erstellen aus ... (siehe Beispiel in InstanceAASCreator.SaveAasToRepositories()
                 * Utils.ShellLoader.LoadAsync()
                 * hole korrektes Submodel über semanticID
                 */
                accumulatedPCF += componentPCF * component.Amount;
            }
            // todo: accumulatedPCF speichern in ProducedProduct
            var producedProduct = new ProducedProduct()
            {
                ConfiguredProductId = producedProductRequest.ConfiguredProductId,
                ProductionDate = DateTime.Now,
                AasId = aasId,
                GlobalAssetId = globalAssetId
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