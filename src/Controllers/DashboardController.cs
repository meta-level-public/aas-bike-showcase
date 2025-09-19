using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Dashboard;
using AasDemoapp.Database.Model;
using AasDemoapp.Katalog;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DashboardController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<int> GetCountAvailableUpdateCount()
        {
            return await Task.FromResult(_dashboardService.GetCountAvailableUpdateCount());
        }

        [HttpGet]
        public async Task<int> GetCountContainedShells()
        {
            return await Task.FromResult(_dashboardService.GetCountContainedShells());
        }

        [HttpGet]
        public async Task<int> GetCountProducts()
        {
            return await Task.FromResult(_dashboardService.GetCountProducts());
        }

        [HttpGet]
        public async Task<int> GetCountProduced()
        {
            return await Task.FromResult(_dashboardService.GetCountProduced());
        }
    }
}
