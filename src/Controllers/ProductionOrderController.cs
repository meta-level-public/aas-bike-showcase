using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Services.ProductionOrder;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ProductionOrderController : ControllerBase
    {
        private readonly ProductionOrderService _productionOrderService;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductionOrderController> _logger;

        public ProductionOrderController(
            ProductionOrderService productionOrderService,
            IMapper mapper,
            ILogger<ProductionOrderController> logger
        )
        {
            _productionOrderService = productionOrderService;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<List<ProductionOrderDto>> GetAll()
        {
            try
            {
                var productionOrders = await _productionOrderService.GetAllAsync();
                return _mapper.Map<List<ProductionOrderDto>>(productionOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all production orders");
                return new List<ProductionOrderDto>();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductionOrderDto>> GetById(long id)
        {
            try
            {
                var productionOrder = await _productionOrderService.GetByIdAsync(id);
                if (productionOrder == null)
                {
                    return NotFound();
                }

                return _mapper.Map<ProductionOrderDto>(productionOrder);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting production order {Id}", id);
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductionOrderResponseDto>> Create(
            CreateProductionOrderDto createDto
        )
        {
            try
            {
                var productionOrder = _mapper.Map<Database.Model.ProductionOrder>(createDto);

                // Address separat behandeln falls vorhanden
                if (createDto.Address != null)
                {
                    productionOrder.Address = _mapper.Map<Address>(createDto.Address);
                }

                var createdOrder = await _productionOrderService.CreateAsync(productionOrder);

                return Ok(
                    new ProductionOrderResponseDto
                    {
                        Success = true,
                        Message = "Production order created successfully",
                        ProductionOrder = _mapper.Map<ProductionOrderDto>(createdOrder),
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating production order");
                return BadRequest(
                    new ProductionOrderResponseDto
                    {
                        Success = false,
                        Message = "Failed to create production order",
                        Error = ex.Message,
                    }
                );
            }
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<ProductionOrderResponseDto>> MarkProductionCompleted(long id)
        {
            try
            {
                var productionOrder = await _productionOrderService.MarkProductionCompletedAsync(
                    id
                );
                if (productionOrder == null)
                {
                    return NotFound(
                        new ProductionOrderResponseDto
                        {
                            Success = false,
                            Message = "Production order not found",
                            Error = $"Production order with ID {id} not found",
                        }
                    );
                }

                return Ok(
                    new ProductionOrderResponseDto
                    {
                        Success = true,
                        Message = "Production marked as completed",
                        ProductionOrder = _mapper.Map<ProductionOrderDto>(productionOrder),
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking production as completed for order {Id}", id);
                return BadRequest(
                    new ProductionOrderResponseDto
                    {
                        Success = false,
                        Message = "Failed to mark production as completed",
                        Error = ex.Message,
                    }
                );
            }
        }

        [HttpPost("{id}")]
        public async Task<ActionResult<ProductionOrderResponseDto>> MarkAsShipped(long id)
        {
            try
            {
                var productionOrder = await _productionOrderService.MarkAsShippedAsync(id);
                if (productionOrder == null)
                {
                    return NotFound(
                        new ProductionOrderResponseDto
                        {
                            Success = false,
                            Message = "Production order not found",
                            Error = $"Production order with ID {id} not found",
                        }
                    );
                }

                return Ok(
                    new ProductionOrderResponseDto
                    {
                        Success = true,
                        Message = "Order marked as shipped",
                        ProductionOrder = _mapper.Map<ProductionOrderDto>(productionOrder),
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(
                    new ProductionOrderResponseDto
                    {
                        Success = false,
                        Message = "Cannot ship order",
                        Error = ex.Message,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order as shipped {Id}", id);
                return BadRequest(
                    new ProductionOrderResponseDto
                    {
                        Success = false,
                        Message = "Failed to mark order as shipped",
                        Error = ex.Message,
                    }
                );
            }
        }
    }
}
