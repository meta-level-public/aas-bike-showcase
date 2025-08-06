using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AasDemoapp.Dashboard;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
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
        private readonly IMapper _mapper;

        public ProductionController(ProductionService productionService, IMapper mapper)
        {
            _productionService = productionService;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<ProductionResponseDto>> CreateProduct(ProducedProductRequestDto requestDto)
        {
            try
            {
                // DTO zu Entity Model konvertieren
                var request = _mapper.Map<ProducedProductRequest>(requestDto);
                
                // Produktion ausführen
                var producedProduct = await _productionService.CreateProduct(request);
                
                // Entity Model zu DTO konvertieren
                var responseDto = _mapper.Map<ProducedProductDto>(producedProduct);
                
                return Ok(new ProductionResponseDto
                {
                    Success = true,
                    Message = "Product created successfully",
                    ProducedProduct = responseDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ProductionResponseDto
                {
                    Success = false,
                    Message = "Failed to create product",
                    Error = ex.Message
                });
            }
        }

        [HttpGet]
        public ActionResult<HandoverDocumentationDto> TestHandoverDocumentation()
        {
            try
            {
                var submodel = HandoverDocumentationCreator.CreateFromJson();
                
                var submodelDto = new SubmodelSummaryDto
                {
                    Id = submodel.Id,
                    IdShort = submodel.IdShort,
                    Description = submodel.Description?.FirstOrDefault()?.Text,
                    Version = submodel.Administration?.Version,
                    Revision = submodel.Administration?.Revision,
                    ElementCount = submodel.SubmodelElements?.Count ?? 0
                };

                return Ok(new HandoverDocumentationDto
                {
                    Success = true,
                    Message = "HandoverDocumentation submodel created successfully",
                    Submodel = submodelDto
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new HandoverDocumentationDto
                {
                    Success = false,
                    Message = "Failed to create HandoverDocumentation submodel",
                    Error = ex.Message
                });
            }
        }
    }
}