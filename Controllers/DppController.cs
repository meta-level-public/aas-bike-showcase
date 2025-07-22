using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;
using AasDemoapp.Dpp;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DppController : ControllerBase
    {
        private readonly DppService _dppService;

        public DppController(DppService dppService)
        {
            _dppService = dppService;
        }

        [HttpGet]
        public async Task<List<ProducedProduct>> GetAll()
        {
            return await Task.FromResult(_dppService.GetAll());
        }
    }
}