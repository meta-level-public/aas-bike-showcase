using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AasDemoapp.Suppliers
{
    public class SupplierService
    {
        private readonly AasDemoappContext _context;

        public SupplierService(AasDemoappContext aasDemoappContext)
        {
            _context = aasDemoappContext;
        }

        public Database.Model.Supplier Add(Database.Model.Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            _context.SaveChanges();

            return supplier;
        }

        public List<Database.Model.Supplier> GetAll()
        {
            return _context.Suppliers.ToList();
        }

        public bool Delete(int id)
        {
            var supplier = _context.Suppliers.FirstOrDefault(s => s.Id == id);
            if (supplier == null)
            {
                return false;
            }

            _context.Suppliers.Remove(supplier);
            _context.SaveChanges();
            return true;
        }
    }
}