using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database;
using Microsoft.EntityFrameworkCore;

namespace AasDemoapp.Suppliers
{
    public class SupplierService
    {
        private readonly AasDemoappContext _context;

        public SupplierService(AasDemoappContext aasDemoappContext)
        {
            _context = aasDemoappContext;
        }

        public async Task<Database.Model.Supplier> AddAsync(Database.Model.Supplier supplier)
        {
            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            return supplier;
        }

        public async Task<List<Database.Model.Supplier>> GetAllAsync()
        {
            return await _context.Suppliers.ToListAsync();
        }

        public async Task<Database.Model.Supplier?> GetByIdAsync(long id)
        {
            return await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Database.Model.Supplier?> UpdateAsync(Database.Model.Supplier supplier)
        {
            var existingSupplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == supplier.Id);
            if (existingSupplier == null)
            {
                return null;
            }

            existingSupplier.Name = supplier.Name;
            existingSupplier.Logo = supplier.Logo;
            existingSupplier.RemoteAasRepositoryUrl = supplier.RemoteAasRepositoryUrl;
            existingSupplier.RemoteSmRepositoryUrl = supplier.RemoteSmRepositoryUrl;
            existingSupplier.RemoteAasRegistryUrl = supplier.RemoteAasRegistryUrl;
            existingSupplier.RemoteSmRegistryUrl = supplier.RemoteSmRegistryUrl;
            existingSupplier.RemoteDiscoveryUrl = supplier.RemoteDiscoveryUrl;
            existingSupplier.RemoteCdRepositoryUrl = supplier.RemoteCdRepositoryUrl;
            existingSupplier.SecuritySetting = supplier.SecuritySetting;

            await _context.SaveChangesAsync();
            return existingSupplier;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null)
            {
                return false;
            }

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}