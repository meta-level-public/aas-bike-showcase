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
    public class SettingService : ISettingService
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
            var baseSetting =
                setting == null ? new SecuritySetting() : DeserializeSecuritySetting(setting.Value);

            // Apply environment-based overrides
            ApplyEnvironmentOverrides(baseSetting);

            return baseSetting;
        }

        private SecuritySetting DeserializeSecuritySetting(string settingValue)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<SecuritySetting>(settingValue, options)
                ?? new SecuritySetting();
        }

        private void ApplyEnvironmentOverrides(SecuritySetting setting)
        {
            // Debug: Log environment variables
            Console.WriteLine(
                $"[DEBUG] IGNORE_SSL_ERRORS: {Environment.GetEnvironmentVariable("IGNORE_SSL_ERRORS")}"
            );
            Console.WriteLine(
                $"[DEBUG] ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}"
            );

            // Check for environment variable to ignore SSL errors
            var ignoreSslEnv = Environment.GetEnvironmentVariable("IGNORE_SSL_ERRORS");
            if (
                !string.IsNullOrEmpty(ignoreSslEnv)
                && bool.TryParse(ignoreSslEnv, out bool ignoreSsl)
            )
            {
                setting.IgnoreSslErrors = ignoreSsl;
                Console.WriteLine(
                    $"[DEBUG] Set IgnoreSslErrors to {ignoreSsl} from environment variable"
                );
            }
            else
            {
                // Default behavior: ignore SSL errors in development/non-production environments
                var environment =
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                if (
                    environment.Equals("Development", StringComparison.OrdinalIgnoreCase)
                    || environment.Equals("Staging", StringComparison.OrdinalIgnoreCase)
                )
                {
                    setting.IgnoreSslErrors = true;
                    Console.WriteLine(
                        $"[DEBUG] Set IgnoreSslErrors to true because environment is {environment}"
                    );
                }
            }

            // TEMPORARY: Force SSL errors to be ignored until we solve this
            setting.IgnoreSslErrors = true;
            Console.WriteLine("[DEBUG] FORCED IgnoreSslErrors to true");

            // Check for timeout override
            var timeoutEnv = Environment.GetEnvironmentVariable("HTTP_TIMEOUT_SECONDS");
            if (!string.IsNullOrEmpty(timeoutEnv) && int.TryParse(timeoutEnv, out int timeout))
            {
                setting.TimeoutSeconds = timeout;
            }
            else if (setting.TimeoutSeconds <= 0)
            {
                setting.TimeoutSeconds = 30; // Default timeout
            }

            Console.WriteLine(
                $"[DEBUG] Final SecuritySetting: IgnoreSslErrors={setting.IgnoreSslErrors}, TimeoutSeconds={setting.TimeoutSeconds}"
            );
        }
    }
}
