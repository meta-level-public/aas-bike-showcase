using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;
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

        public KonfiguratorController(KonfiguratorService konfiguratorService)
        {
            _konfiguratorService = konfiguratorService;
        }

        [HttpGet]
        public async Task<List<ConfiguredProduct>> GetAll()
        {
            return await Task.FromResult(_konfiguratorService.GetAll());
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