using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling;
using AasDemoapp.AasHandling.SubmodelCreators;
using AasDemoapp.Database.Model;
using AasDemoapp.Import;
using AasDemoapp.Settings;
using AasDemoapp.Utils;
using AasDemoapp.Utils.Shells;
using Namotion.Reflection;

namespace AasDemoapp.Production;

public class InstanceAasCreator
{
    public static async Task<AssetAdministrationShell> CreateBikeInstanceAas(
        ProducedProduct producedProduct,
        ImportService importService,
        SettingService settingsService
    )
    {
        var assetInformation = new AssetInformation(
            AssetKind.Instance,
            producedProduct.GlobalAssetId,
            null,
            producedProduct.GlobalAssetId
        );
        assetInformation.AssetType = producedProduct.ConfiguredProduct.GlobalAssetId; // Set the asset type to the configured product's global asset ID
        var aas = new AssetAdministrationShell(
            producedProduct.AasId,
            assetInformation,
            null,
            null,
            producedProduct.ConfiguredProduct.Name
        );

        var nameplate = CreateNameplateSubmodel();
        aas.Submodels =
        [
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, nameplate.Id)]
            ),
        ];

        var handoverdoc = CreateHandoverDocumentation();
        aas.Submodels.Add(
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, handoverdoc.Id)]
            )
        );

        var weight = 5.4; // default gewicht in kg

        var technicalData = CreateTechnicalData(weight);
        aas.Submodels.Add(
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, technicalData.Id)]
            )
        );

        var hierarchicalStructures = CreateHierarchicalStructuresSubmodel(producedProduct);
        aas.Submodels.Add(
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, hierarchicalStructures.Id)]
            )
        );

        var productCarbonFootprint = CreateProductCarbonFootprintSubmodel(
            producedProduct,
            settingsService,
            weight
        );
        aas.Submodels.Add(
            new Reference(
                ReferenceTypes.ModelReference,
                [new Key(KeyTypes.Submodel, productCarbonFootprint.Id)]
            )
        );

        await SaveAasToRepositories(
            aas,
            [nameplate, handoverdoc, technicalData, hierarchicalStructures, productCarbonFootprint],
            importService,
            settingsService
        );

        return aas;
    }

    private static async Task SaveAasToRepositories(
        AssetAdministrationShell aas,
        List<ISubmodel> submodels,
        ImportService importService,
        SettingService settingsService
    )
    {
        var securitySetting = settingsService.GetSecuritySetting(
            SettingTypes.InfrastructureSecurity
        );
        var aasRepositoryUrl =
            settingsService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "";
        var submodelRepositoryUrl =
            settingsService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.Value ?? "";
        var aasRegistryUrl = settingsService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "";
        var submodelRegistryUrl =
            settingsService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.Value ?? "";

        var env = new AasCore.Aas3_0.Environment
        {
            AssetAdministrationShells = [aas],
            Submodels = submodels,
        };
        var plainJson = AasCore.Aas3_0.Jsonization.Serialize.ToJsonObject(env).ToJsonString();

        await SaveShellSaver.SaveSingle(
            new AasUrls
            {
                AasRepositoryUrl = aasRepositoryUrl,
                SubmodelRepositoryUrl = submodelRepositoryUrl,
                AasRegistryUrl = aasRegistryUrl,
                SubmodelRegistryUrl = submodelRegistryUrl,
            },
            securitySetting,
            plainJson,
            [],
            default
        );
    }

    private static Submodel CreateHandoverDocumentation()
    {
        var handoverdoc = HandoverDocumentationCreator.CreateFromJson();
        handoverdoc.Description =
        [
            new LangStringTextType("de", "Handover documentation for the configured product"),
        ];

        return handoverdoc;
    }

    private static Submodel CreateNameplateSubmodel()
    {
        var nameplate = NameplateCreator.CreateFromJson();
        PropertyValueChanger.SetPropertyValueByPath("ManufacturerName", "OI4 Nextbike", nameplate);

        return nameplate;
    }

    private static Submodel CreateTechnicalData(double weight)
    {
        var technicalData = TechnicalDataCreator.CreateFromJson();

        PropertyValueChanger.SetPropertyValueByPath(
            "TechnicalPropertyAreas[0].Weight",
            weight.ToString(System.Globalization.CultureInfo.InvariantCulture),
            technicalData
        );

        return technicalData;
    }

    private static Submodel CreateHierarchicalStructuresSubmodel(ProducedProduct producedProduct)
    {
        var hierarchicalStructures = HierarchicalStructuresCreator.CreateFromJson();
        // entryNode ist das konfigurierte Produkt
        var entryNode = new Entity(EntityType.SelfManagedEntity);
        entryNode.SemanticId = new Reference(
            ReferenceTypes.ExternalReference,
            [
                new Key(
                    KeyTypes.GlobalReference,
                    "https://admin-shell.io/idta/HierarchicalStructures/EntryNode/1/0"
                ),
            ]
        );
        entryNode.IdShort = producedProduct.ConfiguredProduct.Name.Replace(" ", "_");
        entryNode.GlobalAssetId = producedProduct.GlobalAssetId;
        entryNode.Statements = [];
        var first = new Reference(
            ReferenceTypes.ModelReference,
            [
                new Key(KeyTypes.Submodel, hierarchicalStructures.Id),
                new Key(KeyTypes.Entity, entryNode.IdShort),
            ]
        );
        // die elemente einfügen, die in der Konfiguration des Produkts enthalten sind
        foreach (var part in producedProduct.Bestandteile)
        {
            var partNode = new Entity(EntityType.SelfManagedEntity);
            partNode.IdShort = part.Name.Replace(" ", "_");
            partNode.SemanticId = new Reference(
                ReferenceTypes.ExternalReference,
                [
                    new Key(
                        KeyTypes.GlobalReference,
                        "https://admin-shell.io/idta/HierarchicalStructures/Node/1/0"
                    ),
                ]
            );
            partNode.Statements = [];
            partNode.GlobalAssetId = part.KatalogEintrag.GlobalAssetId;

            var second = new Reference(
                ReferenceTypes.ModelReference,
                [
                    new Key(KeyTypes.Submodel, hierarchicalStructures.Id),
                    new Key(KeyTypes.Entity, entryNode.IdShort),
                    new Key(KeyTypes.Entity, partNode.IdShort),
                ]
            );

            var hasPartRelation = new RelationshipElement(first, second);
            hasPartRelation.SemanticId = new Reference(
                ReferenceTypes.ExternalReference,
                [
                    new Key(
                        KeyTypes.GlobalReference,
                        "https://admin-shell.io/idta/HierarchicalStructures/HasPart/1/0"
                    ),
                ]
            );
            hasPartRelation.IdShort = "HasPart";

            var bulkCount = new Property(DataTypeDefXsd.Integer);
            bulkCount.SemanticId = new Reference(
                ReferenceTypes.ExternalReference,
                [
                    new Key(
                        KeyTypes.GlobalReference,
                        "https://admin-shell.io/idta/HierarchicalStructures/BulkCount/1/0"
                    ),
                ]
            );
            bulkCount.IdShort = "BulkCount";
            bulkCount.Value = part.Amount.ToString();

            partNode.Statements.Add(bulkCount);
            entryNode.Statements.Add(partNode);
            entryNode.Statements.Add(hasPartRelation);
        }

        hierarchicalStructures.SubmodelElements = [entryNode];

        return hierarchicalStructures;
    }

    private static Submodel CreateProductCarbonFootprintSubmodel(
        ProducedProduct producedProduct,
        SettingService settingsService,
        double weight
    )
    {
        var pcf = PCFCreator.CreateFromJsonPrefilled();
        var productFootprints = (SubmodelElementList?)
            pcf.SubmodelElements?.Find(submodel => submodel.IdShort == "ProductCarbonFootprints");
        // add pcf value, publication date and address for phases A1 - A3
        var pcfComponent = (SubmodelElementCollection?)productFootprints?.Value?.FirstOrDefault();
        var currentAddressSetting =
            settingsService.GetSetting(SettingTypes.CompanyAddress)?.Value ?? "";

        Address? addressCompany = null;
        if (!string.IsNullOrEmpty(currentAddressSetting))
        {
            try
            {
                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                };
                addressCompany = System.Text.Json.JsonSerializer.Deserialize<Address>(
                    currentAddressSetting,
                    options
                );
            }
            catch (Exception)
            {
                // if deserialization fails, we just leave the address as null
            }
        }

        if (pcfComponent != null)
        {
            var pcfValue = (producedProduct.PCFValue * 0.2);
            if (pcfValue == 0)
            {
                // zufälligen Wert zwischen 0 und 5 erzeugen mit 3 nachkommastellen
                var rand = new Random();
                pcfValue = Math.Round(rand.NextDouble() * 5, 3);
            }
            CompletePCFData(pcfComponent, pcfValue.ToString(), addressCompany);
        }

        // add pcf value, publication date and address for phase A4, if applicable (else ignore)
        try
        {
            // Transport PCF ist 120g pro Tonnenkilometer
            // Distanz zwischen producedProduct.Order.Address und companyAddress berechnen
            var transportDistance =
                DistanceCalculator.CalculateDistanceKm(
                    addressCompany,
                    producedProduct.Order.Address
                ) ?? 100; // Fallback auf 100km wenn keine Koordinaten verfügbar

            var pcfComponentTransport = (SubmodelElementCollection?)productFootprints?.Value?[1];
            if (pcfComponentTransport != null)
            {
                // Transport PCF Berechnung: 120g CO2 pro Tonnenkilometer
                // Annahme: Fahrrad wiegt ca. 15kg = 0.015 Tonnen
                var weightInTonnes = weight / 1000;
                var transportPcfValue = transportDistance * weightInTonnes * 0.12; // 120g = 0.12kg CO2 pro Tonnenkilometer

                CompletePCFData(
                    pcfComponentTransport,
                    transportPcfValue.ToString("F3"), // 3 Dezimalstellen
                    producedProduct.Order.Address
                );
            }
        }
        catch (IndexOutOfRangeException) { } // transport pcf not applilcable (yet)
        return pcf;
    }

    private static SubmodelElementCollection CompletePCFData(
        SubmodelElementCollection pcfComponent,
        String pcfValue,
        Address? address
    )
    {
        var pcfElem = (Property?)
            pcfComponent.Value?.Find(property => property.IdShort == "PcfCO2eq");
        if (pcfElem != null)
            pcfElem.Value = pcfValue;
        var publicationDate = (Property?)
            pcfComponent.Value?.Find(property => property.IdShort == "PublicationDate");
        if (publicationDate != null)
            // iso string schreiben
            publicationDate.Value = DateTime.Now.ToString("O");
        var goodsHandoverAddress = (SubmodelElementCollection?)
            pcfComponent.Value?.Find(smc => smc.IdShort == "GoodsHandoverAddress");

        if (goodsHandoverAddress != null)
        {
            // Adresse setzen
            var contactInfo = ContactInformationCreator.CreateFromJson(
                (address?.Name == null && address?.Vorname == null)
                    ? string.Empty
                    : (address?.Name ?? string.Empty) + " " + (address?.Vorname ?? string.Empty),
                address?.Strasse ?? string.Empty,
                address?.Ort ?? string.Empty,
                address?.Plz ?? string.Empty,
                address?.Land ?? string.Empty
            );
            goodsHandoverAddress.Value = contactInfo.Value;
        }
        return pcfComponent;
    }
}
