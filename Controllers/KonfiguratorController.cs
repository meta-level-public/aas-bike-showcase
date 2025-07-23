using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Katalog;
using AasDemoapp.Konfigurator;
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
        public async Task<bool> Delete(long id)
        {
            return await Task.FromResult(_konfiguratorService.Delete(id));
        }

        [HttpPost]
        public async Task<ConfiguredProduct?> CreateProduct(ConfiguredProduct configuredProduct)
        {
            return await Task.FromResult(_konfiguratorService.CreateProduct(configuredProduct));
        }
    }
}