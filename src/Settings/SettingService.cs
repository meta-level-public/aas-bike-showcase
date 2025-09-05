using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AasDemoapp.Settings
{
    public class SettingService
    {
        private readonly AasDemoappContext _context;

        public SettingService(AasDemoappContext aasDemoappContext)
        {
            _context = aasDemoappContext;
        }

        public Setting Save(Setting setting)
        {
            var dbSetting = _context.Settings.FirstOrDefault(s => s.Name == setting.Name);
            if (dbSetting == null)
            {
                _context.Settings.Add(setting);
            }
            else
            {
                dbSetting.Value = setting.Value;
            }
            _context.SaveChanges();

            return setting;
        }

        public List<Setting> GetAll()
        {
            return _context.Settings.ToList();
        }

        public Setting? GetSetting(string name)
        {
            return _context.Settings.FirstOrDefault(s => s.Name == name);
        }

        public SecuritySetting GetSecuritySetting(string name)
        {
            var setting = _context.Settings.FirstOrDefault(s => s.Name == name);
            if (setting == null)
                return new SecuritySetting();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var securitySetting = JsonSerializer.Deserialize<SecuritySetting>(
                setting.Value,
                options
            );
            return securitySetting ?? new SecuritySetting();
        }
    }
}
