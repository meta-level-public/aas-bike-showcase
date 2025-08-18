using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Import;
using AasDemoapp.Proxy;
using AasDemoapp.Settings;
using AasDemoapp.Utils;
using AasDemoapp.Utils.Registry;
using AasDemoapp.Utils.Shells;
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
            return _context.KatalogEintraege
                .Include(k => k.Supplier)
                .Where(k => k.KatalogEintragTyp == typ)
                .ToList();
        }

        public async Task<KatalogEintrag> ImportRohteilTyp(KatalogEintrag katalogEintrag)
        {
            _context.KatalogEintraege.Add(katalogEintrag);
            katalogEintrag.KatalogEintragTyp = KatalogEintragTyp.RohteilTyp;
            var securitySetting = _settingsService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);

            if (katalogEintrag.Supplier == null)
            {
                throw new ArgumentNullException(nameof(katalogEintrag.Supplier));
            }

            var securitySettingSupplier = katalogEintrag.Supplier.SecuritySetting ?? new SecuritySetting();


            _context.Suppliers.Update(katalogEintrag.Supplier);

            katalogEintrag.Image = await _importService.GetImageString(katalogEintrag.Supplier.RemoteAasRepositoryUrl, securitySettingSupplier, katalogEintrag.AasId);

            var kategorie = "unklassifiziert";

            var env = await _importService.GetEnvironment(katalogEintrag.Supplier.RemoteAasRepositoryUrl, securitySettingSupplier, katalogEintrag.AasId);
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
            var image = string.Empty;
            foreach (var suppl in suppliers)
            {
                try
                {
                    var aasIds = await _proxyService.Discover(suppl.RemoteDiscoveryUrl, suppl.SecuritySetting, instanzGlobalAssetId);

                    foreach (var id in aasIds)
                    {
                        try
                        {
                            var env = await _importService.GetEnvironment(suppl.RemoteAasRepositoryUrl, securitySetting, id);
                            if (env != null && env.AssetAdministrationShells?[0].AssetInformation.GlobalAssetId == instanzGlobalAssetId)
                            {
                                parentGlobalAssetId = env.AssetAdministrationShells?[0].AssetInformation.AssetType;
                                aasId = env.AssetAdministrationShells?[0].Id ?? string.Empty;

                                // Bild laden
                                var imageResult = await _importService.GetImageString(suppl.RemoteAasRepositoryUrl, securitySetting, aasId);
                                if (!string.IsNullOrEmpty(imageResult))
                                {
                                    // Bild erfolgreich geladen
                                    image = imageResult;
                                }
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
                GlobalAssetId = instanzGlobalAssetId,
                Image = image
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
                if (katalogEintrag.Supplier != null)
                {
                    _context.Suppliers.Update(katalogEintrag.Supplier);
                }

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

        public async Task<string> GetInstanzIdByType(string typeGlobalAssetId, Supplier supplier)
        {
            var instanzGlobalAssetId = string.Empty;
            if (string.IsNullOrWhiteSpace(supplier.RemoteAasRegistryUrl)) return instanzGlobalAssetId;

            try
            {
                var client = HttpClientCreator.CreateHttpClient(supplier.SecuritySetting);
                var url = supplier.RemoteAasRegistryUrl.AppendSlash() + "shell-descriptors?assetKind=Instance&assetType=" + typeGlobalAssetId.ToBase64UrlEncoded(Encoding.UTF8);
                var registryResponse = await client.GetAsync(url, CancellationToken.None);

                if (registryResponse.IsSuccessStatusCode)
                {
                    var registryContent = await registryResponse.Content.ReadAsStringAsync();

                    // Extract the result array from the JSON
                    var jsonDoc = JsonDocument.Parse(registryContent);
                    var resultArray = jsonDoc.RootElement.GetProperty("result");

                    // Take the first descriptor from the array
                    if (resultArray.GetArrayLength() > 0)
                    {
                        var firstDescriptor = resultArray[0];
                        var descriptorString = JsonSerializer.Serialize(firstDescriptor);

                        var descriptor = DescriptorSerialization.Deserialize(descriptorString);
                        instanzGlobalAssetId = descriptor.GlobalAssetId;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching instance ID for {supplier.Name}: {ex.Message}");
            }

            return instanzGlobalAssetId ?? string.Empty;
        }
    }
}