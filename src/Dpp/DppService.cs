using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace AasDemoapp.Dpp
{
    public class DppService
    {
        private readonly AasDemoappContext _context;

        public DppService(AasDemoappContext AasDemoappContext)
        {
            _context = AasDemoappContext;
        }

        public List<ProducedProduct> GetAll() {
            return _context.ProducedProducts.Include(p => p.ConfiguredProduct).ToList();
        } 
    }
}