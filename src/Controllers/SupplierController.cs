using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Suppliers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SupplierController : ControllerBase
    {
        private readonly SupplierService _supplierService;
        private readonly IMapper _mapper;

        public SupplierController(SupplierService supplierService, IMapper mapper)
        {
            _supplierService = supplierService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<SupplierDto>>> GetAll()
        {
            try
            {
                var suppliers = await Task.FromResult(_supplierService.GetAll());
                var supplierDtos = _mapper.Map<List<SupplierDto>>(suppliers);
                return Ok(supplierDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Fehler beim Abrufen der Lieferanten", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<SupplierResponseDto>> Add(CreateSupplierDto createSupplierDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createSupplierDto.Name))
                {
                    return BadRequest(new SupplierResponseDto
                    {
                        Success = false,
                        Message = "Name ist erforderlich",
                        Error = "Validation failed"
                    });
                }

                var supplier = _mapper.Map<Supplier>(createSupplierDto);
                var addedSupplier = await Task.FromResult(_supplierService.Add(supplier));
                var supplierDto = _mapper.Map<SupplierDto>(addedSupplier);

                return Ok(new SupplierResponseDto
                {
                    Success = true,
                    Message = "Lieferant erfolgreich hinzugefügt",
                    Supplier = supplierDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SupplierResponseDto
                {
                    Success = false,
                    Message = "Fehler beim Hinzufügen des Lieferanten",
                    Error = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<SupplierResponseDto>> Delete(int id)
        {
            try
            {
                var success = await Task.FromResult(_supplierService.Delete(id));
                
                if (!success)
                {
                    return NotFound(new SupplierResponseDto
                    {
                        Success = false,
                        Message = $"Lieferant mit ID {id} nicht gefunden",
                        Error = "Not found"
                    });
                }

                return Ok(new SupplierResponseDto
                {
                    Success = true,
                    Message = "Lieferant erfolgreich gelöscht"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new SupplierResponseDto
                {
                    Success = false,
                    Message = "Fehler beim Löschen des Lieferanten",
                    Error = ex.Message
                });
            }
        }
    }
}