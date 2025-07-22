using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;
using AasDemoapp.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SupplierController : ControllerBase
    {
        private readonly SupplierService _supplierService;

        public SupplierController(SupplierService supplierService)
        {
            _supplierService = supplierService;
        }


        [HttpGet]
        public async Task<List<Supplier>> GetAll()
        {
            return await Task.FromResult(_supplierService.GetAll());
        }

        [HttpPost]
        public async Task<Database.Model.Supplier> Add(Supplier supplier)
        {
            return await Task.FromResult(_supplierService.Add(supplier));
        }

        [HttpDelete("{id}")]
        public async Task<bool> Delete(int id)
        {
            return await Task.FromResult(_supplierService.Delete(id));
        }
    }
}