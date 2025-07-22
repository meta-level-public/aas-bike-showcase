using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Dashboard;
using AasDemoapp.Database.Model;
using AasDemoapp.Katalog;
using AasDemoapp.Production;
using Microsoft.AspNetCore.Mvc;
using AasCore.Aas3_0;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ProductionController : ControllerBase
    {
        private readonly ProductionService _productionService;

        public ProductionController(ProductionService productionService)
        {
            _productionService = productionService;
        }

        [HttpPost]
        public async Task<ProducedProduct> CreateProduct(ProducedProductRequest producedProduct)
        {
            return await _productionService.CreateProduct(producedProduct);
        }

        [HttpGet]
        public IActionResult TestHandoverDocumentation()
        {
            try
            {
                var submodel = HandoverDocumentationCreator.CreateHandoverDocumentationFromJson();
                
                return Ok(new
                {
                    success = true,
                    message = "HandoverDocumentation submodel created successfully",
                    submodel = new
                    {
                        id = submodel.Id,
                        idShort = submodel.IdShort,
                        description = submodel.Description?.FirstOrDefault()?.Text,
                        version = submodel.Administration?.Version,
                        revision = submodel.Administration?.Revision,
                        elementCount = submodel.SubmodelElements?.Count ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to create HandoverDocumentation submodel",
                    error = ex.Message
                });
            }
        }
    }
}