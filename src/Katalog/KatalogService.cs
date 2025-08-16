using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Import;
using AasDemoapp.Proxy;
using AasDemoapp.Settings;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace AasDemoapp.Katalog
{
    public class KatalogService
    {
        private readonly AasDemoappContext _context;
        private readonly ImportService _importService;
        private readonly ProxyService _proxyService;
        private readonly SettingService _settingsService;

        public KatalogService(AasDemoappContext aasDemoappContext, ImportService importService, ProxyService proxyService, SettingService settingsService)
        {
            _context = aasDemoappContext;
            _importService = importService;
            _proxyService = proxyService;
            _settingsService = settingsService;
        }

        public List<KatalogEintrag> GetAll(KatalogEintragTyp typ)
        {
            return _context.KatalogEintraege.Where(k => k.KatalogEintragTyp == typ).ToList();
        }

        public async Task<KatalogEintrag> ImportRohteilTyp(KatalogEintrag katalogEintrag)
        {
            _context.KatalogEintraege.Add(katalogEintrag);
            katalogEintrag.KatalogEintragTyp = KatalogEintragTyp.RohteilTyp;
            var securitySetting = _settingsService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);
            var securitySettingSupplier = katalogEintrag.Supplier.SecuritySetting;

            _context.Suppliers.Update(katalogEintrag.Supplier);

            katalogEintrag.Image = await _importService.GetImageString(katalogEintrag.Supplier.RemoteRepositoryUrl, securitySettingSupplier, katalogEintrag.AasId);

            var kategorie = "unklassifiziert";

            var env = await _importService.GetEnvironment(katalogEintrag.Supplier.RemoteRepositoryUrl, securitySettingSupplier, katalogEintrag.AasId);
            if (env != null)
            {
                var nameplate = _importService.GetNameplate(env);
                if (nameplate != null)
                {
                    kategorie = _importService.GetKategorie(nameplate);
                }
            }
            katalogEintrag.Kategorie = kategorie;
            var aasRepositoryUrl = _settingsService?.GetSetting(SettingTypes.AasRepositoryUrl);
            katalogEintrag.LocalAasId = await _importService.ImportFromRepository(aasRepositoryUrl?.Value ?? "", katalogEintrag, securitySetting, katalogEintrag.AasId);

            _context.SaveChanges();
            return katalogEintrag;
        }

        public async Task<RohteilLookupResult> LookupRohteil(string instanzGlobalAssetId)
        {
            var suppliers = _context.Suppliers.ToList();
            var parentGlobalAssetId = string.Empty;
            var aasId = string.Empty;
            var securitySetting = _settingsService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);
            foreach (var suppl in suppliers)
            {
                try
                {
                    var aasIds = await _proxyService.Discover(suppl.RemoteRepositoryUrl, securitySetting, instanzGlobalAssetId);

                    foreach (var id in aasIds)
                    {
                        try
                        {
                            var env = await _importService.GetEnvironment(suppl.RemoteRepositoryUrl, securitySetting, id);
                            if (env != null && env.AssetAdministrationShells?[0].AssetInformation.GlobalAssetId == instanzGlobalAssetId)
                            {
                                parentGlobalAssetId = env.AssetAdministrationShells?[0].AssetInformation.AssetType;
                                aasId = env.AssetAdministrationShells?[0].Id ?? string.Empty;
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            // ignore
                            Console.WriteLine(ex);
                        }
                    }
                }
                catch
                {
                    // nicht gefunden, nächsten Lieferanten testen
                }
            }

            return new RohteilLookupResult()
            {
                TypeKatalogEintrag = _context.KatalogEintraege.FirstOrDefault(k => k.GlobalAssetId == parentGlobalAssetId),
                AasId = aasId,
                GlobalAssetId = instanzGlobalAssetId
            };
        }

        public async Task<KatalogEintrag?> ImportRohteilInstanz(KatalogEintrag katalogEintrag)
        {

            var existing = _context.KatalogEintraege.FirstOrDefault(k => k.GlobalAssetId == katalogEintrag.GlobalAssetId);
            var securitySetting = _settingsService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);
            if (existing == null)
            {
                _context.KatalogEintraege.Add(katalogEintrag);
                katalogEintrag.KatalogEintragTyp = KatalogEintragTyp.RohteilInstanz;
                _context.Suppliers.Update(katalogEintrag.Supplier);


                try
                {
                    katalogEintrag.Image = await _importService.GetImageString(katalogEintrag.RemoteRepositoryUrl, securitySetting, katalogEintrag.AasId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching image: {ex.Message}");
                    katalogEintrag.Image = string.Empty; // Set to empty if image fetch fails
                }

                var kategorie = "unklassifiziert";

                var env = await _importService.GetEnvironment(katalogEintrag.RemoteRepositoryUrl, securitySetting, katalogEintrag.AasId);
                if (env != null)
                {
                    var nameplate = _importService.GetNameplate(env);
                    if (nameplate != null)
                    {
                        kategorie = _importService.GetKategorie(nameplate);
                    }
                }
                katalogEintrag.Kategorie = kategorie;
                if (katalogEintrag.ReferencedType != null)
                {
                    _context.KatalogEintraege.Attach(katalogEintrag.ReferencedType);
                }

                var aasRepositoryUrl = _settingsService?.GetSetting(SettingTypes.AasRepositoryUrl);
                katalogEintrag.LocalAasId = await _importService.ImportFromRepository(aasRepositoryUrl?.Value ?? "", katalogEintrag, securitySetting, katalogEintrag.AasId, false);
            }
            else
            {
                existing.Amount += katalogEintrag.Amount;
                katalogEintrag = existing;
            }
            _context.SaveChanges();

            return katalogEintrag;
        }

        public async Task Delete(long id)
        {
            var eintrag = _context.KatalogEintraege.First(k => k.Id == id);
            var securitySetting = _settingsService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);
            var transaction = _context.Database.BeginTransaction();
            try
            {

                _context.Remove(eintrag);
                _context.SaveChanges();
                try
                {
                    var aasRepositoryUrl = _settingsService?.GetSetting(SettingTypes.AasRepositoryUrl);
                    await _proxyService.Delete(aasRepositoryUrl?.Value ?? "", securitySetting, eintrag.LocalAasId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting AAS from proxy: {ex.Message}");
                }
                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting catalog entry: {ex.Message}");
                // Rollback the transaction in case of an error
                transaction.Rollback();
                throw;
            }
        }

        public KatalogEintrag? GetRohteilKatalogEintrag(string globalAssetId)
        {
            return _context.KatalogEintraege.Include(k => k.ReferencedType).FirstOrDefault(k => k.GlobalAssetId == globalAssetId && k.KatalogEintragTyp == KatalogEintragTyp.RohteilInstanz);
        }
    }
}