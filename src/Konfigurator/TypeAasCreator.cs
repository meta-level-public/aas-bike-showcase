using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database.Model;
using AasDemoapp.Import;
using AasDemoapp.Production;
using AasDemoapp.Settings;
using AasDemoapp.Utils.Shells;

namespace AasDemoapp.Konfigurator
{
    public class TypeAasCreator
    {
        public async Task CreateBikeTypeAas(ConfiguredProduct configuredProduct, ImportService importService, SettingService settingsService)
        {
            // TODO: jetzt die neue Verwaltungsschale bauen
            var assetInformation = new AssetInformation(AssetKind.Type, Guid.NewGuid().ToString(), null, configuredProduct.GlobalAssetId);
            var aas = new AssetAdministrationShell(Guid.NewGuid().ToString(), assetInformation, null, null, configuredProduct.Name);

            var nameplate = CreateNameplateSubmodel();
            aas.Submodels = [new Reference(ReferenceTypes.ModelReference, [new Key(KeyTypes.Submodel, nameplate.Id)])];

            var handoverdoc = CreateHandoverDocumentation();
            aas.Submodels.Add(new Reference(ReferenceTypes.ModelReference, [new Key(KeyTypes.Submodel, handoverdoc.Id)]));

            var hierarchicalStructures = CreateHierarchicalStructuresSubmodel(configuredProduct);
            aas.Submodels.Add(new Reference(ReferenceTypes.ModelReference, [new Key(KeyTypes.Submodel, hierarchicalStructures.Id)]));

            await SaveAasToRepositories(aas, nameplate, handoverdoc, hierarchicalStructures, importService, settingsService);
        }

        private async Task SaveAasToRepositories(AssetAdministrationShell aas, Submodel nameplate, Submodel handoverdoc, Submodel hierarchicalStructures, ImportService importService, SettingService settingsService)
        {
            var aasRepositoryUrl = settingsService.GetSetting(SettingTypes.AasRepositoryUrl)?.value ?? "";
            await importService.PushNewToLocalRepositoryAsync(aas, [nameplate, handoverdoc, hierarchicalStructures], aasRepositoryUrl);

            var env = new AasCore.Aas3_0.Environment
            {
                AssetAdministrationShells = [aas],
                Submodels = [nameplate, handoverdoc, hierarchicalStructures]
            };
            var plainJson = AasCore.Aas3_0.Jsonization.Serialize.ToJsonObject(env).ToJsonString();

            var submodelRepositoryUrl = settingsService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.value ?? "";
            var aasRegistryUrl = settingsService.GetSetting(SettingTypes.AasRegistryUrl)?.value ?? "";
            var submodelRegistryUrl = settingsService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.value ?? "";
            await SaveShellSaver.SaveSingle(
                new AasUrls
                {
                    AasRepositoryUrl = aasRepositoryUrl,
                    SubmodelRepositoryUrl = submodelRepositoryUrl,
                    AasRegistryUrl = aasRegistryUrl,
                    SubmodelRegistryUrl = submodelRegistryUrl
                },
                plainJson,
                [],
                default);
        }

        private Submodel CreateHandoverDocumentation()
        {
            var handoverdoc = HandoverDocumentationCreator.CreateFromJson();
            handoverdoc.Id = Guid.NewGuid().ToString();
            handoverdoc.IdShort = "HandoverDocumentation";
            if (handoverdoc.Administration != null)
            {
                handoverdoc.Administration.Version = "1.0";
                handoverdoc.Administration.Version = "1.0";
            }
            handoverdoc.Description = [new LangStringTextType("de", "Handover documentation for the configured product")];
            
            return handoverdoc;
        }

        private Submodel CreateNameplateSubmodel()
        {
            var nameplate = new Submodel(Guid.NewGuid().ToString(), null, null, "Nameplate", null, null, null, ModellingKind.Instance, new Reference(ReferenceTypes.ExternalReference, [new Key(KeyTypes.GlobalReference, "0173-1#02-AAO677#002")]));
            var smeManufacturer = new MultiLanguageProperty(null, null, "ManufacturerName", null, null, new Reference(ReferenceTypes.ExternalReference, [new Key(KeyTypes.GlobalReference, "0173-1#02-AAO677#002")]))
            {
                Value = [new LangStringTextType("de", "ML DEMO App")]
            };
            nameplate.SubmodelElements = [smeManufacturer];
            
            return nameplate;
        }

        private Submodel CreateHierarchicalStructuresSubmodel(ConfiguredProduct configuredProduct)
        {
            var hierarchicalStructures = new Submodel(
                Guid.NewGuid().ToString(),
                null,
                null,
                "HierarchicalStructures",
                null,
                null,
                null,
                ModellingKind.Instance,
                new Reference(ReferenceTypes.ExternalReference, [new Key(KeyTypes.GlobalReference, "https://admin-shell.io/zvei/nameplate/1/0/Nameplate/Markings/Marking/ExplosionSafety")])
            );

            // BOM Structure Element
            var bomStructure = new SubmodelElementCollection(
                null,
                null,
                "BillOfMaterial",
                null,
                null,
                new Reference(ReferenceTypes.ExternalReference, [new Key(KeyTypes.GlobalReference, "0173-1#02-AAQ424#005")])
            );

            // Beispiel BOM Eintrag
            var bomEntry = new SubmodelElementCollection(
                null,
                null,
                "BOMEntry_001",
                null,
                null,
                null
            );

            var partNumber = new Property(
                DataTypeDefXsd.String,
                null,
                "PartNumber",
                null,
                null,
                null,
                new Reference(ReferenceTypes.ExternalReference, [new Key(KeyTypes.GlobalReference, "0173-1#02-AAO739#001")])
            )
            {
                Value = "BIKE-PART-001"
            };

            var quantity = new Property(
                DataTypeDefXsd.Int,
                null,
                "Quantity",
                null,
                null,
                null,
                new Reference(ReferenceTypes.ExternalReference, [new Key(KeyTypes.GlobalReference, "0173-1#02-AAO738#001")])
            )
            {
                Value = "1"
            };

            bomEntry.Value = [partNumber, quantity];
            bomStructure.Value = [bomEntry];
            hierarchicalStructures.SubmodelElements = [bomStructure];

            return hierarchicalStructures;
        }
    }
}