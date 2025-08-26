using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling;
using AasDemoapp.AasHandling.SubmodelCreators;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Import;
using AasDemoapp.Proxy;
using AasDemoapp.Settings;
using AasDemoapp.Utils;
using AasDemoapp.Utils.Serialization;
using AasDemoapp.Utils.Shells;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Namotion.Reflection;

namespace AasDemoapp.Production
{
    public class ProductionService
    {
        private readonly AasDemoappContext _context;
        private readonly ImportService _importService;
        private readonly SettingService _settingService;
        private readonly ProxyService _proxyService;
        private readonly ILogger<ProductionService> _logger;

        public ProductionService(AasDemoappContext aasDemoappContext, ImportService importService, SettingService settingService, ProxyService proxyService, ILogger<ProductionService> logger)
        {
            _context = aasDemoappContext;
            _importService = importService;
            _settingService = settingService;
            _proxyService = proxyService;
            _logger = logger;
        }

        private double getPCFValueV09(Submodel pcfSubmodel)
        {
            double componentPCF = 0.0;
            foreach (SubmodelElementCollection elem in pcfSubmodel.SubmodelElements)
            {
                List<ISubmodelElement> elems = elem.Value;
                var pcfElem = (IProperty)elems.Find(property => property.SemanticId.Keys.First().Value == "0173-1#02-ABG855#001");
                componentPCF += double.Parse(pcfElem.Value, System.Globalization.CultureInfo.InvariantCulture);

            }
            return componentPCF;
        }

        private double getPCFValueV10(Submodel pcfSubmodel)
        {
            double componentPCF = 0.0;
            var productFootprints = (SubmodelElementList)pcfSubmodel.SubmodelElements.Find(submodel => submodel.SemanticId.Keys.First().Value == "https://admin-shell.io/idta/CarbonFootprint/ProductCarbonFootprints/1/0");
            foreach (SubmodelElementCollection elem in pcfSubmodel.SubmodelElements)
            {
                Property pcfProperty = (Property)elem.Value.Find(property => property.SemanticId.Keys.First().Value == "0173-1#02-ABG855#003");
                componentPCF += double.Parse(pcfProperty.Value, System.Globalization.CultureInfo.InvariantCulture);
            }

            return componentPCF;
        }

        public async Task<ProducedProduct> CreateProduct(ProducedProductRequest producedProductRequest)
        {

            var aasId = IdGenerationUtil.GenerateId(IdType.Aas, "https://oi4-nextbike.de");
            var globalAssetId = IdGenerationUtil.GenerateId(IdType.Asset, "https://oi4-nextbike.de");
            var securitySetting = _settingService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);

            // Produkt zusammenbauen
            var accumulatedPCF = 0.0; // TODO: could this value be pre-filled from the configuredProduct?
            foreach (var component in producedProductRequest.BestandteilRequests)
            {
                try
                {
                    var ids = await _proxyService.Discover(
                        _settingService.GetSetting(SettingTypes.DiscoveryUrl)?.Value ?? "",
                        securitySetting, component.GlobalAssetId);
                    var aas_id = ids[0];
                    var submodelRepositoryUrl =
                        _settingService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.Value ?? "";
                    var aasRegistryUrl = _settingService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "";
                    var submodelRegistryUrl = _settingService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.Value ?? "";
                    var aasRepositoryUrl = _settingService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "";
                    LoadShellResult componentAAS = await ShellLoader.LoadAsync(
                        new AasUrls
                        {
                            AasRepositoryUrl = aasRepositoryUrl,
                            SubmodelRepositoryUrl = submodelRepositoryUrl,
                            AasRegistryUrl = aasRegistryUrl,
                            SubmodelRegistryUrl = submodelRegistryUrl
                        }, securitySetting, aas_id, default);
                    Submodel pcfSubmodel = (Submodel)componentAAS.Environment.Submodels.Find(submodel => submodel.SemanticId.Keys.First().Value == "https://admin-shell.io/idta/CarbonFootprint/CarbonFootprint/1/0");
                    if (pcfSubmodel != null)
                    {
                        accumulatedPCF += getPCFValueV10(pcfSubmodel) * component.Amount;
                    }
                    else
                    {
                        pcfSubmodel = (Submodel)componentAAS.Environment.Submodels.Find(submodel => submodel.SemanticId.Keys.First().Value == "0173-1#01-AHE712#001");
                        if (pcfSubmodel != null)
                        {
                            accumulatedPCF += getPCFValueV09(pcfSubmodel) * component.Amount;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }

            accumulatedPCF *= 1.2; // 20 % extra for mounting

            var producedProduct = new ProducedProduct()
            {
                ConfiguredProductId = producedProductRequest.ConfiguredProductId,
                ProductionDate = DateTime.Now,
                AasId = aasId,
                GlobalAssetId = globalAssetId,
                PCFValue = accumulatedPCF
            };

            var configuredProduct = await _context.ConfiguredProducts.FirstAsync(c => c.Id == producedProductRequest.ConfiguredProductId);
            var order = _context.ProductionOrders.FirstAsync(c => c.Id == producedProduct.Id);

            producedProductRequest.BestandteilRequests.ForEach((bestandteil) =>
            {
                var katalogEintrag = _context.KatalogEintraege.First(b => b.GlobalAssetId == bestandteil.GlobalAssetId);
                producedProduct.Bestandteile.Add(new ProductPart()
                {
                    Amount = bestandteil.Amount,
                    Name = katalogEintrag.Name,
                    Price = katalogEintrag.Price,
                    UsageDate = bestandteil.UsageDate,
                    KatalogEintrag = katalogEintrag
                });
                katalogEintrag.Amount = katalogEintrag.Amount - bestandteil.Amount;
            });

            _context.ProducedProducts.Add(producedProduct);
            _context.SaveChanges();

            var product = await _context.ProducedProducts
                .AsNoTracking()
                .Include(p => p.ConfiguredProduct)
                .Include(p => p.Bestandteile)
                .ThenInclude(b => b.KatalogEintrag)
                .FirstAsync(p => p.Id == producedProduct.Id);

            try
            {
                // Create InstanceAAS for Bike
                var aas = await InstanceAasCreator.CreateBikeInstanceAas(product, _importService, _settingService);
                producedProduct.AasId = aasId;
                producedProduct.GlobalAssetId = globalAssetId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating InstanceAAS for product {ProductId}", producedProduct.Id);
            }

            return producedProduct;
        }

        public async Task<string> GetAssemblyPropertiesSubmodel(long partId)
        {
            var part = _context.ProductParts.Include(p => p.KatalogEintrag).FirstOrDefault(p => p.Id == partId);
            if (part == null) return string.Empty;

            var aasId = part.KatalogEintrag.AasId;

            var res = await GetRequiredToolSubmodel(aasId);

            var smString = string.Empty;
            if (res != null)
            {
                smString = Jsonization.Serialize.ToJsonObject(res).ToString();
            }

            return smString;
        }

        public async Task<Submodel?> GetRequiredToolSubmodel(string aasId)
        {
            var aasUrls = new AasUrls
            {
                AasRepositoryUrl = _settingService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "",
                SubmodelRepositoryUrl = _settingService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.Value ?? "",
                AasRegistryUrl = _settingService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "",
                SubmodelRegistryUrl = _settingService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.Value ?? ""
            };
            var securitySetting = _settingService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);

            var res = await ShellLoader.LoadAsync(aasUrls, securitySetting, aasId, CancellationToken.None);

            var smString = string.Empty;

            if (res != null)
            {
                var sm = res.Environment?.Submodels?.Find(sm => sm.IdShort == "AssemblyProperties");
                return sm as Submodel;
            }

            return null;
        }

        public async Task<bool> SetRequiredToolProperty(string aasId, string propertyIdShortPath, string propertyValue)
        {
            var submodelRepositoryUrl = _settingService.GetSetting(SettingTypes.ToolsSubmodelRepositoryUrl)?.Value ?? "";
            var aasRegistryUrl = _settingService.GetSetting(SettingTypes.ToolsAASRegistryUrl)?.Value ?? "";
            var submodelRegistryUrl = _settingService.GetSetting(SettingTypes.ToolsSubmodelRegistryUrl)?.Value ?? "";
            var aasRepositoryUrl = _settingService.GetSetting(SettingTypes.ToolsAASRepositoryUrl)?.Value ?? "";
            var securitySetting = _settingService.GetSecuritySetting(SettingTypes.ToolsRepositorySecurity);

            var aasUrls = new AasUrls
            {
                AasRepositoryUrl = aasRepositoryUrl,
                SubmodelRepositoryUrl = submodelRepositoryUrl,
                AasRegistryUrl = aasRegistryUrl,
                SubmodelRegistryUrl = submodelRegistryUrl
            };

            var res = await ShellLoader.LoadAsync(aasUrls, securitySetting, aasId, CancellationToken.None);
            var client = HttpClientCreator.CreateHttpClient(securitySetting);

            // wie finden wir das korrekte submodel?
            foreach (var ism in res.Environment?.Submodels ?? [])
            {
                var sm = ism as Submodel;
                if (sm == null) continue;

                if (sm.IdShort == "RequiredTool") // TODO: ID des submodels finden -> hendrik fragen
                {
                    PropertyValueChanger.SetPropertyValueByPath(propertyIdShortPath, propertyValue, sm);
                    var smUrl = submodelRepositoryUrl.AppendSlash() + "submodels/" + sm.Id.ToBase64UrlEncoded(Encoding.UTF8);
                    var smJsonString = BasyxSerializer.Serialize(sm);
                    var smResponse = await client.PutAsync(smUrl, new StringContent(smJsonString, Encoding.UTF8, "application/json"), CancellationToken.None);
                    if (smResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        // POST dann ohne ID ...
                        // für den AASX Server hängen wir noch die aasId an ...
                        smUrl = submodelRepositoryUrl.AppendSlash() + "submodels";
                        smResponse = await client.PostAsync(smUrl, new StringContent(smJsonString, Encoding.UTF8, "application/json"), CancellationToken.None);
                        if (!smResponse.IsSuccessStatusCode)
                        {
                            // throw new Exception($"Request to {smUrl} failed with status code {smResponse.StatusCode}");
                            Console.WriteLine("Error saving submodel: " + smResponse.StatusCode);
                        }
                    }
                    else if (!smResponse.IsSuccessStatusCode)
                    {
                        // throw new Exception($"Request to {smUrl} failed with status code {smResponse.StatusCode}");
                        Console.WriteLine("Error saving submodel: " + smResponse.StatusCode);
                    }
                }
            }

            return true;
        }

        public async Task<string> GetToolData(string aasId)
        {
            var submodelRepositoryUrl = _settingService.GetSetting(SettingTypes.ToolsSubmodelRepositoryUrl)?.Value ?? "";
            var aasRegistryUrl = _settingService.GetSetting(SettingTypes.ToolsAASRegistryUrl)?.Value ?? "";
            var submodelRegistryUrl = _settingService.GetSetting(SettingTypes.ToolsSubmodelRegistryUrl)?.Value ?? "";
            var aasRepositoryUrl = _settingService.GetSetting(SettingTypes.ToolsAASRepositoryUrl)?.Value ?? "";
            var securitySetting = _settingService.GetSecuritySetting(SettingTypes.ToolsRepositorySecurity);

            var aasUrls = new AasUrls
            {
                AasRepositoryUrl = aasRepositoryUrl,
                SubmodelRepositoryUrl = submodelRepositoryUrl,
                AasRegistryUrl = aasRegistryUrl,
                SubmodelRegistryUrl = submodelRegistryUrl
            };

            var res = await ShellLoader.LoadAsync(aasUrls, securitySetting, aasId, CancellationToken.None);
            var client = HttpClientCreator.CreateHttpClient(securitySetting);

            // wie finden wir das korrekte submodel?
            foreach (var ism in res.Environment?.Submodels ?? [])
            {
                var sm = ism as Submodel;
                if (sm == null) continue;

                if (sm.IdShort == "RequiredTool") // TODO: ID des submodels finden -> hendrik fragen
                {
                    return Jsonization.Serialize.ToJsonObject(sm).ToString();
                }
            }

            return string.Empty;
        }
    }
}