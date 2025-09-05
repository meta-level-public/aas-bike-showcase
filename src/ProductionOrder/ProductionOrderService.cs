using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using Microsoft.EntityFrameworkCore;

namespace AasDemoapp.Services.ProductionOrder
{
    public class ProductionOrderService
    {
        private readonly AasDemoappContext _context;
        private readonly ILogger<ProductionOrderService> _logger;

        public ProductionOrderService(
            AasDemoappContext context,
            ILogger<ProductionOrderService> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<Database.Model.ProductionOrder>> GetAllAsync()
        {
            return await _context
                .ProductionOrders.Include(po => po.ConfiguredProduct)
                .Include(po => po.Address)
                .OrderByDescending(po => po.CreatedAt)
                .ToListAsync();
        }

        public async Task<Database.Model.ProductionOrder?> GetByIdAsync(long id)
        {
            return await _context
                .ProductionOrders.Include(po => po.ConfiguredProduct)
                .Include(po => po.Address)
                .FirstOrDefaultAsync(po => po.Id == id);
        }

        public async Task<Database.Model.ProductionOrder> CreateAsync(
            Database.Model.ProductionOrder productionOrder
        )
        {
            productionOrder.CreatedAt = DateTime.Now;
            _context.ProductionOrders.Add(productionOrder);
            await _context.SaveChangesAsync();

            return (await GetByIdAsync(productionOrder.Id!.Value))!;
        }

        public async Task<Database.Model.ProductionOrder?> MarkProductionCompletedAsync(long id)
        {
            var productionOrder = await GetByIdAsync(id);
            if (productionOrder == null)
            {
                return null;
            }

            productionOrder.ProduktionAbgeschlossen = true;
            productionOrder.FertigstellungsDatum = DateTime.Now;
            productionOrder.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return productionOrder;
        }

        public async Task<Database.Model.ProductionOrder?> MarkAsShippedAsync(long id)
        {
            var productionOrder = await GetByIdAsync(id);
            if (productionOrder == null)
            {
                return null;
            }

            if (!productionOrder.ProduktionAbgeschlossen)
            {
                throw new InvalidOperationException("Production must be completed before shipping");
            }

            productionOrder.Versandt = true;
            productionOrder.VersandDatum = DateTime.Now;
            productionOrder.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return productionOrder;
        }
    }
}
