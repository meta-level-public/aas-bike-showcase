using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling.SubmodelCreators;
using AasDemoapp.Dashboard;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Katalog;
using AasDemoapp.Production;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ProductionController : ControllerBase
    {
        private readonly ProductionService _productionService;
        private readonly IMapper _mapper;

        public ProductionController(ProductionService productionService, IMapper mapper)
        {
            _productionService = productionService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ProductionResponseDto>> CreateProduct(
            ProducedProductRequestDto requestDto
        )
        {
            try
            {
                // DTO zu Entity Model konvertieren
                var request = _mapper.Map<ProducedProductRequest>(requestDto);

                // Produktion ausführen
                var producedProduct = await _productionService.CreateProduct(request);

                // Entity Model zu DTO konvertieren
                var responseDto = _mapper.Map<ProducedProductDto>(producedProduct);

                return Ok(
                    new ProductionResponseDto
                    {
                        Success = true,
                        Message = "Product created successfully",
                        ProducedProduct = responseDto,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new ProductionResponseDto
                    {
                        Success = false,
                        Message = "Failed to create product",
                        Error = ex.Message,
                    }
                );
            }
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetAssemblyPropertiesSubmodel(long partId)
        {
            var smString = await _productionService.GetAssemblyPropertiesSubmodel(partId);
            return Ok(smString);
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetToolData(string aasId)
        {
            var smString = await _productionService.GetToolData(aasId);
            return Ok(smString);
        }

        public class ToolData
        {
            public string AasId { get; set; } = string.Empty;
            public string PropertyIdShortPath { get; set; } = string.Empty;
            public string PropertyValue { get; set; } = string.Empty;
        }

        [HttpPost]
        public async Task<ActionResult<bool>> InitializeTool(ToolData toolData)
        {
            var result = await _productionService.SetRequiredToolProperty(
                toolData.AasId,
                toolData.PropertyIdShortPath,
                toolData.PropertyValue
            );
            return Ok(result);
        }
    }
}
