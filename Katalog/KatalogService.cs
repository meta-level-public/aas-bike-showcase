using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Import;
using AasDemoapp.Proxy;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace AasDemoapp.Katalog
{
    public class KatalogService
    {
        private readonly AasDemoappContext _context;
        private readonly ImportService _importService;
        private readonly ProxyService _proxyService;

        public KatalogService(AasDemoappContext aasDemoappContext, ImportService importService, ProxyService proxyService)
        {
            _context = aasDemoappContext;
            _importService = importService;
            _proxyService = proxyService;
        }

        public List<KatalogEintrag> GetAll(KatalogEintragTyp typ)
        {
            return _context.KatalogEintraege.Where(k => k.KatalogEintragTyp == typ).ToList();
        }

        public async Task<KatalogEintrag> ImportRohteilTyp(KatalogEintrag katalogEintrag)
        {
            _context.KatalogEintraege.Add(katalogEintrag);
            katalogEintrag.KatalogEintragTyp = KatalogEintragTyp.RohteilTyp;

            katalogEintrag.Image = await _importService.GetImageString(katalogEintrag.RemoteRepositoryUrl, katalogEintrag.AasId);

            var kategorie = "unklassifiziert";

            var env = await _importService.GetEnvironment(katalogEintrag.RemoteRepositoryUrl, katalogEintrag.AasId);
            if (env != null)
            {
                var nameplate = _importService.GetNameplate(env);
                if (nameplate != null)
                {
                    kategorie = _importService.GetKategorie(nameplate);
                }
            }
            katalogEintrag.Kategorie = kategorie;

            katalogEintrag.LocalAasId = await _importService.ImportFromRepository("http://localhost:9421", katalogEintrag.RemoteRepositoryUrl, katalogEintrag.AasId);
            _context.SaveChanges();
            return katalogEintrag;
        }

        public async Task<RohteilLookupResult> LookupRohteil(string instanzGlobalAssetId)
        {
            var suppliers = _context.Suppliers.ToList();
            var parentGlobalAssetId = string.Empty;
            var aasId = string.Empty;
            foreach (var suppl in suppliers)
            {
                try
                {
                    var aasIds = await _proxyService.Discover(suppl.RemoteRepositoryUrl, instanzGlobalAssetId);

                    foreach (var id in aasIds)
                    {
                        try
                        {
                            var env = await _importService.GetEnvironment(suppl.RemoteRepositoryUrl, id);
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
            if (existing == null)
            {
                _context.KatalogEintraege.Add(katalogEintrag);
                katalogEintrag.KatalogEintragTyp = KatalogEintragTyp.RohteilInstanz;

                try
                {
                    katalogEintrag.Image = await _importService.GetImageString(katalogEintrag.RemoteRepositoryUrl, katalogEintrag.AasId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching image: {ex.Message}");
                    katalogEintrag.Image = string.Empty; // Set to empty if image fetch fails
                }

                var kategorie = "unklassifiziert";

                var env = await _importService.GetEnvironment(katalogEintrag.RemoteRepositoryUrl, katalogEintrag.AasId);
                if (env != null)
                {
                    var nameplate = _importService.GetNameplate(env);
                    if (nameplate != null)
                    {
                        kategorie = _importService.GetKategorie(nameplate);
                    }
                }
                katalogEintrag.Kategorie = kategorie;

                _context.KatalogEintraege.Attach(katalogEintrag.ReferencedType);

                katalogEintrag.LocalAasId = await _importService.ImportFromRepository("http://localhost:9421", katalogEintrag.RemoteRepositoryUrl, katalogEintrag.AasId, false);
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
            var transaction = _context.Database.BeginTransaction();
            try
            {

                _context.Remove(eintrag);
                _context.SaveChanges();
                try
                {
                    await _proxyService.Delete("http://localhost:9421", eintrag.LocalAasId);
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