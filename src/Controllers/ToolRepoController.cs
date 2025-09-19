using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.ToolRepos;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ToolRepoController : ControllerBase
    {
        private readonly ToolRepoService _service;
        private readonly IMapper _mapper;

        public ToolRepoController(ToolRepoService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ToolRepoDto>>> GetAll()
        {
            try
            {
                var items = await _service.GetAllAsync();
                var dtos = _mapper.Map<List<ToolRepoDto>>(items);
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        error = "Fehler beim Abrufen der Tool Repositories",
                        details = ex.Message,
                    }
                );
            }
        }

        [HttpPost]
        public async Task<ActionResult<ToolRepoResponseDto>> Add(CreateToolRepoDto createDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(createDto.Name))
                {
                    return BadRequest(
                        new ToolRepoResponseDto
                        {
                            Success = false,
                            Message = "Name ist erforderlich",
                            Error = "Validation failed",
                        }
                    );
                }

                var entity = _mapper.Map<ToolRepo>(createDto);
                var added = await _service.AddAsync(entity);
                var dto = _mapper.Map<ToolRepoDto>(added);

                return Ok(
                    new ToolRepoResponseDto
                    {
                        Success = true,
                        Message = "Tool Repository erfolgreich hinzugefügt",
                        ToolRepo = dto,
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new ToolRepoResponseDto
                    {
                        Success = false,
                        Message = "Fehler beim Hinzufügen des Tool Repository",
                        Error = ex.Message,
                    }
                );
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ToolRepoDto>> GetById(long id)
        {
            try
            {
                var entity = await _service.GetByIdAsync(id);
                if (entity == null)
                {
                    return NotFound(new { error = $"Tool Repository mit ID {id} nicht gefunden" });
                }

                var dto = _mapper.Map<ToolRepoDto>(entity);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new { error = "Fehler beim Abrufen des Tool Repository", details = ex.Message }
                );
            }
        }

        [HttpPut]
        public async Task<ActionResult<ToolRepoResponseDto>> Update(UpdateToolRepoDto updateDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(updateDto.Name))
                {
                    return BadRequest(
                        new ToolRepoResponseDto
                        {
                            Success = false,
                            Message = "Name ist erforderlich",
                            Error = "Validation failed",
                        }
                    );
                }

                var entity = _mapper.Map<ToolRepo>(updateDto);
                var updated = await _service.UpdateAsync(entity);

                if (updated == null)
                {
                    return NotFound(
                        new ToolRepoResponseDto
                        {
                            Success = false,
                            Message = $"Tool Repository mit ID {updateDto.Id} nicht gefunden",
                            Error = "Not found",
                        }
                    );
                }

                var dto = _mapper.Map<ToolRepoDto>(updated);

                return Ok(
                    new ToolRepoResponseDto
                    {
                        Success = true,
                        Message = "Tool Repository erfolgreich aktualisiert",
                        ToolRepo = dto,
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new ToolRepoResponseDto
                    {
                        Success = false,
                        Message = "Fehler beim Aktualisieren des Tool Repository",
                        Error = ex.Message,
                    }
                );
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ToolRepoResponseDto>> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);

                if (!success)
                {
                    return NotFound(
                        new ToolRepoResponseDto
                        {
                            Success = false,
                            Message = $"Tool Repository mit ID {id} nicht gefunden",
                            Error = "Not found",
                        }
                    );
                }

                return Ok(
                    new ToolRepoResponseDto
                    {
                        Success = true,
                        Message = "Tool Repository erfolgreich gelöscht",
                    }
                );
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new ToolRepoResponseDto
                    {
                        Success = false,
                        Message = "Fehler beim Löschen des Tool Repository",
                        Error = ex.Message,
                    }
                );
            }
        }
    }
}
