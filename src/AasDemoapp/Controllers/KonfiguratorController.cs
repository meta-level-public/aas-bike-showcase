using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Katalog;
using AasDemoapp.Konfigurator;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class KonfiguratorController : ControllerBase
    {
        private readonly KonfiguratorService _konfiguratorService;
        private readonly IMapper _mapper;

        public KonfiguratorController(KonfiguratorService konfiguratorService, IMapper mapper)
        {
            _konfiguratorService = konfiguratorService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<List<ConfiguredProductDto>> GetAll()
        {
            var products = await Task.FromResult(_konfiguratorService.GetAll());
            return _mapper.Map<List<ConfiguredProductDto>>(products);
        }

        [HttpDelete]
        public async Task<ActionResult<ConfigurationResponseDto>> Delete(long id)
        {
            try
            {
                var result = await Task.FromResult(_konfiguratorService.Delete(id));

                if (result)
                {
                    return Ok(
                        new ConfigurationResponseDto
                        {
                            Success = true,
                            Message = "Product deleted successfully",
                        }
                    );
                }
                else
                {
                    return NotFound(
                        new ConfigurationResponseDto
                        {
                            Success = false,
                            Message = "Product not found",
                            Error = $"Product with ID {id} not found",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new ConfigurationResponseDto
                    {
                        Success = false,
                        Message = "Failed to delete product",
                        Error = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        public async Task<ActionResult<ConfigurationResponseDto>> CreateProduct(
            CreateConfiguredProductDto createDto
        )
        {
            try
            {
                // Validation
                if (string.IsNullOrEmpty(createDto.Name))
                {
                    return BadRequest(
                        new ConfigurationResponseDto
                        {
                            Success = false,
                            Message = "Product name is required",
                            Error = "Name cannot be empty",
                        }
                    );
                }

                if (!createDto.Bestandteile.Any())
                {
                    return BadRequest(
                        new ConfigurationResponseDto
                        {
                            Success = false,
                            Message = "At least one component is required",
                            Error = "Bestandteile cannot be empty",
                        }
                    );
                }

                // Create product using service
                var configuredProduct = await _konfiguratorService.CreateProductFromDto(createDto);

                // Map to DTO for response
                var responseDto = _mapper.Map<ConfiguredProductDto>(configuredProduct);

                return Ok(
                    new ConfigurationResponseDto
                    {
                        Success = true,
                        Message = "Product configured successfully",
                        ConfiguredProduct = responseDto,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(
                    new ConfigurationResponseDto
                    {
                        Success = false,
                        Message = "Failed to create configured product",
                        Error = ex.Message,
                    }
                );
            }
        }
    }
}
