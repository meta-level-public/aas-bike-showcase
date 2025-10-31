using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;
using AasDemoapp.Settings;
using AasDemoapp.Suppliers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SettingController : ControllerBase
    {
        private readonly ISettingService _settingService;

        public SettingController(ISettingService settingService)
        {
            _settingService = settingService;
        }

        [HttpGet]
        public async Task<List<Setting>> GetAll()
        {
            return await Task.FromResult(_settingService.GetAll());
        }

        [HttpPost]
        public async Task<Setting> Save(Setting setting)
        {
            return await Task.FromResult(_settingService.Save(setting));
        }
    }
}
