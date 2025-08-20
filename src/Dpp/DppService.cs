using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Settings;
using AasDemoapp.Utils.Shells;
using Microsoft.EntityFrameworkCore;

namespace AasDemoapp.Dpp
{
    public class DppService
    {
        private readonly AasDemoappContext _context;
        private readonly SettingService _settingService;

        public DppService(AasDemoappContext AasDemoappContext, SettingService settingService)
        {
            _context = AasDemoappContext;
            _settingService = settingService;
        }

        public List<ProducedProduct> GetAll()
        {
            return _context.ProducedProducts.Include(p => p.ConfiguredProduct).ToList();
        }

        internal async Task Delete(long idProducedProduct)
        {
            var product = await _context.ProducedProducts.FindAsync(idProducedProduct);
            if (product != null)
            {
                _context.ProducedProducts.Remove(product);
                await _context.SaveChangesAsync();
            }
        }

        internal async Task<string> GetShell(long idProducedProduct)
        {
            var product = _context.ProducedProducts
                .Include(p => p.ConfiguredProduct)
                .FirstOrDefault(p => p.Id == idProducedProduct);

            if (product == null)
            {
                throw new ArgumentException("Invalid product ID");
            }

            var securitySetting = _settingService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);
            var aasRepositoryUrl = _settingService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "";
            var submodelRepositoryUrl = _settingService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.Value ?? "";
            var aasRegistryUrl = _settingService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "";
            var submodelRegistryUrl = _settingService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.Value ?? "";


            var aasUrls = new AasUrls
            {
                AasRepositoryUrl = aasRepositoryUrl,
                SubmodelRepositoryUrl = submodelRepositoryUrl,
                AasRegistryUrl = aasRegistryUrl,
                SubmodelRegistryUrl = submodelRegistryUrl
            };


            var res = await ShellLoader.LoadShellOnly(aasUrls, securitySetting, product.AasId, CancellationToken.None);
            if (res != null)
            {
                return AasCore.Aas3_0.Jsonization.Serialize.ToJsonObject(res).ToJsonString();
            }
            else
            {
                return string.Empty;
            }
        }

        // TODO: hier eine Methode GetPCFSubmodel(string globalAssetID)?

    }
}