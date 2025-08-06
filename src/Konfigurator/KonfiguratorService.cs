using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.AasHandling;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Import;
using AasDemoapp.Settings;
using Microsoft.EntityFrameworkCore;

namespace AasDemoapp.Konfigurator
{
    public class KonfiguratorService
    {
        private readonly AasDemoappContext _context;
        private readonly ImportService _importService;
        private readonly SettingService _settingsService;
        private readonly ILogger<KonfiguratorService> _logger;

        public KonfiguratorService(AasDemoappContext aasDemoappContext, ImportService importService, SettingService settingsService, ILogger<KonfiguratorService> logger)
        {
            _context = aasDemoappContext;
            _importService = importService;
            _settingsService = settingsService;
            _logger = logger;
        }

        public List<ConfiguredProduct> GetAll()
        {
            return _context.ConfiguredProducts.Include(p => p.Bestandteile).ThenInclude(x => x.KatalogEintrag).ToList();
        }

        public ConfiguredProduct CreateProduct(ConfiguredProduct configuredProduct)
        {
            _context.KatalogEintraege.AttachRange(configuredProduct.Bestandteile.Select(b => b.KatalogEintrag).ToList());
            _context.ConfiguredProducts.Add(configuredProduct);

            _context.SaveChanges();
            return configuredProduct;
        }

        public async Task<ConfiguredProduct> CreateProductFromDto(CreateConfiguredProductDto createDto)
        {
            var aasId = IdGenerationUtil.GenerateId(IdType.Aas, "https://oi4-nextbike.de");
            var globalAssetId = IdGenerationUtil.GenerateId(IdType.Asset, "https://oi4-nextbike.de");

            var configuredProduct = new ConfiguredProduct
            {
                Name = createDto.Name,
                Price = createDto.Price,
                AasId = aasId,
                GlobalAssetId = globalAssetId,
                Bestandteile = new List<ProductPart>()
            };

            // Bestandteile hinzufügen
            if (createDto.Bestandteile != null)
            {
                foreach (var partDto in createDto.Bestandteile)
                {
                    var katalogEintrag = _context.KatalogEintraege.FirstOrDefault(k => k.Id == partDto.KatalogEintragId);
                    if (katalogEintrag == null)
                    {
                        throw new InvalidOperationException($"KatalogEintrag with ID {partDto.KatalogEintragId} not found");
                    }

                    var productPart = new ProductPart
                    {
                        KatalogEintragId = partDto.KatalogEintragId,
                        KatalogEintrag = katalogEintrag,
                        Name = partDto.Name,
                        Price = partDto.Price,
                        Amount = partDto.Amount,
                        UsageDate = partDto.UsageDate
                    };
                    configuredProduct.Bestandteile.Add(productPart);
                }
            }

            _context.ConfiguredProducts.Add(configuredProduct);
            _context.SaveChanges();
            // Reload with includes for complete data
            var product = await _context.ConfiguredProducts
                .AsNoTracking()
                .Include(p => p.Bestandteile)
                .ThenInclude(x => x.KatalogEintrag)
                .FirstAsync(p => p.Id == configuredProduct.Id);

            try
            {

                // Create TypeAAS for Bike
                await InstanceAasCreator.CreateBikeTypeAas(product, _importService, _settingsService);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating TypeAAS for product {ProductId}", product.Id);
            }

            return product;
        }

        public bool Delete(long id)
        {
            var eintrag = _context.ConfiguredProducts.First(k => k.Id == id);
            _context.Remove(eintrag);
            _context.SaveChanges();

            return true;
        }
    }
}