using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly IImportService _importService;
        private readonly ISettingService _settingService;
        private readonly ProxyService _proxyService;
        private readonly ILogger<ProductionService> _logger;

        public ProductionService(
            AasDemoappContext aasDemoappContext,
            IImportService importService,
            ISettingService settingService,
            ProxyService proxyService,
            ILogger<ProductionService> logger
        )
        {
            _context = aasDemoappContext;
            _importService = importService;
            _settingService = settingService;
            _proxyService = proxyService;
            _logger = logger;
        }

        private double getPCFValueV09(Submodel pcfSubmodel)
        {
            if (pcfSubmodel.SubmodelElements == null)
                return 0.0;
            double componentPCF = 0.0;
            foreach (var elem in pcfSubmodel.SubmodelElements.OfType<SubmodelElementCollection>())
            {
                var elems = elem.Value;
                if (elems == null)
                    continue;
                var pcfElem = elems
                    .OfType<IProperty>()
                    .FirstOrDefault(p =>
                        p.SemanticId?.Keys?.FirstOrDefault()?.Value == "0173-1#02-ABG855#001"
                    );
                if (pcfElem?.Value == null)
                    continue;
                if (
                    double.TryParse(
                        pcfElem.Value,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var parsed
                    )
                )
                {
                    componentPCF += parsed;
                }
            }
            return componentPCF;
        }

        private double getPCFValueV10(Submodel pcfSubmodel)
        {
            if (pcfSubmodel.SubmodelElements == null)
                return 0.0;
            double componentPCF = 0.0;
            foreach (var elem in pcfSubmodel.SubmodelElements.OfType<SubmodelElementCollection>())
            {
                var values = elem.Value;
                if (values == null)
                    continue;
                var pcfProperty = values
                    .OfType<Property>()
                    .FirstOrDefault(p =>
                        p.SemanticId?.Keys?.FirstOrDefault()?.Value == "0173-1#02-ABG855#003"
                    );
                if (pcfProperty?.Value == null)
                    continue;
                if (
                    double.TryParse(
                        pcfProperty.Value,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out var parsed
                    )
                )
                {
                    componentPCF += parsed;
                }
            }
            return componentPCF;
        }

        public async Task<(
            ProducedProduct product,
            byte[]? pdfData,
            string? pdfFileName
        )> CreateProduct(ProducedProductRequest producedProductRequest)
        {
            var idPrefix =
                _settingService.GetSetting(SettingTypes.AasIdPrefix)?.Value
                ?? "https://oi4-nextbike.de";
            var aasId = IdGenerationUtil.GenerateId(IdType.Aas, idPrefix);
            var globalAssetId = IdGenerationUtil.GenerateId(IdType.Asset, idPrefix);
            var securitySetting = _settingService.GetSecuritySetting(
                SettingTypes.InfrastructureSecurity
            );

            // Produkt zusammenbauen
            var accumulatedPCF = 0.0; // TODO: could this value be pre-filled from the configuredProduct?
            foreach (var component in producedProductRequest.BestandteilRequests)
            {
                try
                {
                    var ids = await _proxyService.Discover(
                        _settingService.GetSetting(SettingTypes.DiscoveryUrl)?.Value ?? "",
                        securitySetting,
                        component.GlobalAssetId
                    );
                    if (ids == null || ids.Length == 0)
                    {
                        _logger.LogWarning(
                            "No AAS ids discovered for component {GlobalAssetId}",
                            component.GlobalAssetId
                        );
                        continue;
                    }
                    var aas_id = ids[0];
                    var submodelRepositoryUrl =
                        _settingService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.Value ?? "";
                    var aasRegistryUrl =
                        _settingService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "";
                    var submodelRegistryUrl =
                        _settingService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.Value ?? "";
                    var aasRepositoryUrl =
                        _settingService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "";
                    LoadShellResult componentAAS = await ShellLoader.LoadAsync(
                        new AasUrls
                        {
                            AasRepositoryUrl = aasRepositoryUrl,
                            SubmodelRepositoryUrl = submodelRepositoryUrl,
                            AasRegistryUrl = aasRegistryUrl,
                            SubmodelRegistryUrl = submodelRegistryUrl,
                        },
                        securitySetting,
                        aas_id,
                        default
                    );
                    var submodels = componentAAS?.Environment?.Submodels;
                    Submodel? pcfSubmodelV10 = null;
                    if (submodels != null)
                    {
                        pcfSubmodelV10 = submodels
                            .OfType<Submodel>()
                            .FirstOrDefault(sm =>
                                sm.SemanticId?.Keys?.FirstOrDefault()?.Value
                                == "https://admin-shell.io/idta/CarbonFootprint/CarbonFootprint/1/0"
                            );
                    }
                    if (pcfSubmodelV10 != null)
                    {
                        accumulatedPCF += getPCFValueV10(pcfSubmodelV10) * component.Amount;
                    }
                    else
                    {
                        Submodel? pcfSubmodelV09 = null;
                        if (submodels != null)
                        {
                            pcfSubmodelV09 = submodels
                                .OfType<Submodel>()
                                .FirstOrDefault(sm =>
                                    sm.SemanticId?.Keys?.FirstOrDefault()?.Value
                                    == "0173-1#01-AHE712#001"
                                );
                        }
                        if (pcfSubmodelV09 != null)
                        {
                            accumulatedPCF += getPCFValueV09(pcfSubmodelV09) * component.Amount;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(
                        e,
                        "Error while accumulating PCF for component {GlobalAssetId}",
                        component.GlobalAssetId
                    );
                }
            }

            accumulatedPCF *= 1.2; // 20 % extra for mounting

            var order = await _context.ProductionOrders.FirstOrDefaultAsync(c =>
                c.Id == producedProductRequest.ProductionOrderId
            );

            if (order == null)
            {
                _logger.LogError("Order not found");
                throw new Exception("Order not found");
            }

            var producedProduct = new ProducedProduct()
            {
                ConfiguredProductId = producedProductRequest.ConfiguredProductId,
                ProductionDate = DateTime.Now,
                AasId = aasId,
                GlobalAssetId = globalAssetId,
                PCFValue = accumulatedPCF,
                Order = order,
            };

            var configuredProduct = await _context.ConfiguredProducts.FirstAsync(c =>
                c.Id == producedProductRequest.ConfiguredProductId
            );

            producedProductRequest.BestandteilRequests.ForEach(
                (bestandteil) =>
                {
                    var katalogEintrag = _context.KatalogEintraege.First(b =>
                        b.GlobalAssetId == bestandteil.GlobalAssetId
                    );
                    producedProduct.Bestandteile.Add(
                        new ProductPart()
                        {
                            Amount = bestandteil.Amount,
                            Name = katalogEintrag.Name,
                            Price = katalogEintrag.Price,
                            UsageDate = bestandteil.UsageDate,
                            KatalogEintrag = katalogEintrag,
                        }
                    );
                    katalogEintrag.Amount = katalogEintrag.Amount - bestandteil.Amount;
                }
            );

            _context.ProducedProducts.Add(producedProduct);
            _context.SaveChanges();

            var product = await _context
                .ProducedProducts.AsNoTracking()
                .Include(p => p.ConfiguredProduct)
                .Include(p => p.Bestandteile)
                .ThenInclude(b => b.KatalogEintrag)
                .Include(p => p.Order)
                .ThenInclude(o => o.Address)
                .FirstAsync(p => p.Id == producedProduct.Id);

            byte[]? pdfData = null;
            string? pdfFileName = null;

            try
            {
                // Create InstanceAAS for Bike
                var aasResult = await InstanceAasCreator.CreateBikeInstanceAas(
                    product,
                    _importService,
                    _settingService
                );
                producedProduct.AasId = aasId;
                producedProduct.GlobalAssetId = globalAssetId;

                // Speichere PDF-Daten für Response
                if (aasResult.PdfData != null && !string.IsNullOrEmpty(aasResult.PdfFileName))
                {
                    pdfData = aasResult.PdfData;
                    pdfFileName = aasResult.PdfFileName;

                    _logger.LogInformation(
                        "PDF data prepared for product {ProductId}, size: {PdfSize} bytes",
                        producedProduct.Id,
                        pdfData.Length
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "No PDF data returned for product {ProductId}",
                        producedProduct.Id
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating InstanceAAS for product {ProductId}",
                    producedProduct.Id
                );
            }

            return (producedProduct, pdfData, pdfFileName);
        }

        public async Task<string> GetAssemblyPropertiesSubmodel(long partId)
        {
            var part = _context
                .ProductParts.Include(p => p.KatalogEintrag)
                .FirstOrDefault(p => p.Id == partId);
            if (part?.KatalogEintrag == null)
                return string.Empty;

            var aasId = part.KatalogEintrag.LocalAasId ?? string.Empty;

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
                AasRepositoryUrl =
                    _settingService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "",
                SubmodelRepositoryUrl =
                    _settingService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.Value ?? "",
                AasRegistryUrl =
                    _settingService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "",
                SubmodelRegistryUrl =
                    _settingService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.Value ?? "",
            };
            var securitySetting = _settingService.GetSecuritySetting(
                SettingTypes.InfrastructureSecurity
            );

            var res = await ShellLoader.LoadAsync(
                aasUrls,
                securitySetting,
                aasId,
                CancellationToken.None
            );

            var smString = string.Empty;

            if (res != null)
            {
                var sm = res.Environment?.Submodels?.Find(sm => sm.IdShort == "AssemblyProperties");
                return sm as Submodel;
            }

            return null;
        }

        public async Task<bool> SetRequiredToolProperty(
            string aasId,
            string propertyIdShortPath,
            string propertyValue
        )
        {
            var submodelRepositoryUrl =
                _settingService.GetSetting(SettingTypes.ToolsSubmodelRepositoryUrl)?.Value ?? "";
            var aasRegistryUrl =
                _settingService.GetSetting(SettingTypes.ToolsAASRegistryUrl)?.Value ?? "";
            var submodelRegistryUrl =
                _settingService.GetSetting(SettingTypes.ToolsSubmodelRegistryUrl)?.Value ?? "";
            var aasRepositoryUrl =
                _settingService.GetSetting(SettingTypes.ToolsAASRepositoryUrl)?.Value ?? "";
            var securitySetting = _settingService.GetSecuritySetting(
                SettingTypes.ToolsRepositorySecurity
            );

            var aasUrls = new AasUrls
            {
                AasRepositoryUrl = aasRepositoryUrl,
                SubmodelRepositoryUrl = submodelRepositoryUrl,
                AasRegistryUrl = aasRegistryUrl,
                SubmodelRegistryUrl = submodelRegistryUrl,
            };

            var res = await ShellLoader.LoadAsync(
                aasUrls,
                securitySetting,
                aasId,
                CancellationToken.None
            );
            var client = HttpClientCreator.CreateHttpClient(securitySetting);

            // wie finden wir das korrekte submodel?
            foreach (var ism in res.Environment?.Submodels ?? [])
            {
                var sm = ism as Submodel;
                if (sm == null)
                    continue;

                if (sm.IdShort == "RequiredTool" || sm.IdShort == "ProductionOperations") // TODO: ID des submodels finden -> hendrik fragen
                {
                    var smUrl = await ShellLoader.GetSmUrl(
                        aasUrls,
                        securitySetting,
                        aasId,
                        sm.Id,
                        CancellationToken.None
                    );
                    var updateUrl =
                        smUrl.AppendSlash()
                        + "submodel-elements/"
                        + propertyIdShortPath
                        + "/$value";

                    // PATCH Aufruf implementieren
                    var jsonContent = new StringContent(
                        $"\"{propertyValue}\"",
                        Encoding.UTF8,
                        "application/json"
                    );

                    var patchRequest = new HttpRequestMessage(HttpMethod.Patch, updateUrl)
                    {
                        Content = jsonContent,
                    };

                    var patchResponse = await client.SendAsync(patchRequest);

                    if (patchResponse.IsSuccessStatusCode)
                    {
                        _logger.LogInformation(
                            $"Successfully updated property {propertyIdShortPath} with value {propertyValue}"
                        );
                        return true;
                    }
                    else
                    {
                        _logger.LogError(
                            $"Failed to update property {propertyIdShortPath}. Status: {patchResponse.StatusCode}"
                        );
                        return false;
                    }
                }
            }

            return true;
        }

        public async Task<string> GetToolData(string aasId)
        {
            var submodelRepositoryUrl =
                _settingService.GetSetting(SettingTypes.ToolsSubmodelRepositoryUrl)?.Value ?? "";
            var aasRegistryUrl =
                _settingService.GetSetting(SettingTypes.ToolsAASRegistryUrl)?.Value ?? "";
            var submodelRegistryUrl =
                _settingService.GetSetting(SettingTypes.ToolsSubmodelRegistryUrl)?.Value ?? "";
            var aasRepositoryUrl =
                _settingService.GetSetting(SettingTypes.ToolsAASRepositoryUrl)?.Value ?? "";
            var securitySetting = _settingService.GetSecuritySetting(
                SettingTypes.ToolsRepositorySecurity
            );

            var aasUrls = new AasUrls
            {
                AasRepositoryUrl = aasRepositoryUrl,
                SubmodelRepositoryUrl = submodelRepositoryUrl,
                AasRegistryUrl = aasRegistryUrl,
                SubmodelRegistryUrl = submodelRegistryUrl,
            };

            var res = await ShellLoader.LoadAsync(
                aasUrls,
                securitySetting,
                aasId,
                CancellationToken.None
            );
            var client = HttpClientCreator.CreateHttpClient(securitySetting);

            // wie finden wir das korrekte submodel?
            foreach (var ism in res.Environment?.Submodels ?? [])
            {
                var sm = ism as Submodel;
                if (sm == null)
                    continue;

                if (sm.IdShort == "RequiredTool" || sm.IdShort == "ProductionOperations") // TODO: ID des submodels finden -> hendrik fragen
                {
                    return Jsonization.Serialize.ToJsonObject(sm).ToString();
                }
            }

            return string.Empty;
        }
    }
}
