using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Dpp;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DppController : ControllerBase
    {
        private readonly DppService _dppService;
        private readonly IMapper _mapper;

        public DppController(DppService dppService, IMapper mapper)
        {
            _dppService = dppService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<List<ProducedProductDto>> GetAll()
        {
            var products = await Task.FromResult(_dppService.GetAll());
            return _mapper.Map<List<ProducedProductDto>>(products);
        }
    }
}