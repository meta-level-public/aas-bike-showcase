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

        public ProductionOrderService(AasDemoappContext context, ILogger<ProductionOrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<Database.Model.ProductionOrder> GetAll()
        {
            return _context.ProductionOrders
                .Include(po => po.ConfiguredProduct)
                .Include(po => po.Address)
                .OrderByDescending(po => po.CreatedAt)
                .ToList();
        }

        public Database.Model.ProductionOrder? GetById(long id)
        {
            return _context.ProductionOrders
                .Include(po => po.ConfiguredProduct)
                .Include(po => po.Address)
                .FirstOrDefault(po => po.Id == id);
        }

        public Database.Model.ProductionOrder Create(Database.Model.ProductionOrder productionOrder)
        {
            productionOrder.CreatedAt = DateTime.Now;
            _context.ProductionOrders.Add(productionOrder);
            _context.SaveChanges();
            
            return GetById(productionOrder.Id!.Value)!;
        }

        public Database.Model.ProductionOrder? MarkProductionCompleted(long id)
        {
            var productionOrder = GetById(id);
            if (productionOrder == null)
            {
                return null;
            }

            productionOrder.ProduktionAbgeschlossen = true;
            productionOrder.FertigstellungsDatum = DateTime.Now;
            productionOrder.UpdatedAt = DateTime.Now;
            
            _context.SaveChanges();
            return productionOrder;
        }

        public Database.Model.ProductionOrder? MarkAsShipped(long id)
        {
            var productionOrder = GetById(id);
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
            
            _context.SaveChanges();
            return productionOrder;
        }
    }
}
