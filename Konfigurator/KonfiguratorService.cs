using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
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

        public bool Delete(long id)
        {
            var eintrag = _context.ConfiguredProducts.First(k => k.Id == id);
            _context.Remove(eintrag);
            _context.SaveChanges();

            return true;
        }
    }
}