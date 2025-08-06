using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AasDemoapp.Konfigurator
{
    public class KonfiguratorService
    {
        private readonly AasDemoappContext _context;

        public KonfiguratorService(AasDemoappContext aasDemoappContext)
        {
            _context = aasDemoappContext;
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

        public ConfiguredProduct CreateProductFromDto(CreateConfiguredProductDto createDto)
        {
            var configuredProduct = new ConfiguredProduct
            {
                Name = createDto.Name,
                Price = createDto.Price,
                AasId = createDto.AasId,
                GlobalAssetId = createDto.GlobalAssetId,
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

            // Create TypeAAS for Bike

            // Reload with includes for complete data
            return _context.ConfiguredProducts
                .Include(p => p.Bestandteile)
                .ThenInclude(x => x.KatalogEintrag)
                .First(p => p.Id == configuredProduct.Id);
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