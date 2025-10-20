using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;

namespace AasDemoapp.Dashboard
{
    public class DashboardService
    {
        private readonly AasDemoappContext _context;

        public DashboardService(AasDemoappContext aasDemoappContext)
        {
            _context = aasDemoappContext;
        }

        public int GetCountAvailableUpdateCount()
        {
            return _context.UpdateableShells.Where(p => !p.IsDeleted).Count();
        }

        public int GetCountContainedShells()
        {
            return _context.KatalogEintraege.Where(p => !p.IsDeleted).GroupBy(k => k.AasId).Count();
        }

        public int GetCountProducts()
        {
            return _context.ConfiguredProducts.Where(p => !p.IsDeleted).Count();
        }

        public int GetCountProduced()
        {
            return _context.ProducedProducts.Where(p => !p.IsDeleted).Count();
        }
    }
}
