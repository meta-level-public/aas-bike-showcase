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
using AasDemoapp.Utils.Shells;

namespace AasDemoapp.Konfigurator
{
    public class InstanceAasCreator
    {
        public static async Task CreateBikeTypeAas(
            ConfiguredProduct configuredProduct,
            ImportService importService,
            SettingService settingsService
        )
        {
            var assetInformation = new AssetInformation(
                AssetKind.Type,
                configuredProduct.GlobalAssetId,
                null,
                configuredProduct.GlobalAssetId
            );
            var aas = new AssetAdministrationShell(
                configuredProduct.AasId ?? Guid.NewGuid().ToString(),
                assetInformation,
                null,
                null,
                configuredProduct.Name
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

            var hierarchicalStructures = CreateHierarchicalStructuresSubmodel(configuredProduct);
            aas.Submodels.Add(
                new Reference(
                    ReferenceTypes.ModelReference,
                    [new Key(KeyTypes.Submodel, hierarchicalStructures.Id)]
                )
            );

            await SaveAasToRepositories(
                aas,
                nameplate,
                handoverdoc,
                hierarchicalStructures,
                importService,
                settingsService
            );
        }

        private static async Task SaveAasToRepositories(
            AssetAdministrationShell aas,
            Submodel nameplate,
            Submodel handoverdoc,
            Submodel hierarchicalStructures,
            ImportService importService,
            SettingService settingsService
        )
        {
            var aasRepositoryUrl =
                settingsService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "";
            var securitySetting = settingsService.GetSecuritySetting(
                SettingTypes.InfrastructureSecurity
            );
            await importService.PushNewToLocalRepositoryAsync(
                aas,
                [nameplate, handoverdoc, hierarchicalStructures],
                aasRepositoryUrl,
                securitySetting
            );

            var env = new AasCore.Aas3_0.Environment
            {
                AssetAdministrationShells = [aas],
                Submodels = [nameplate, handoverdoc, hierarchicalStructures],
            };
            var plainJson = AasCore.Aas3_0.Jsonization.Serialize.ToJsonObject(env).ToJsonString();

            var submodelRepositoryUrl =
                settingsService.GetSetting(SettingTypes.SubmodelRepositoryUrl)?.Value ?? "";
            var aasRegistryUrl =
                settingsService.GetSetting(SettingTypes.AasRegistryUrl)?.Value ?? "";
            var submodelRegistryUrl =
                settingsService.GetSetting(SettingTypes.SubmodelRegistryUrl)?.Value ?? "";
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
            PropertyValueChanger.SetPropertyValueByPath(
                "ManufacturerName",
                "OI4 Nextbike",
                nameplate
            );

            return nameplate;
        }

        private static Submodel CreateHierarchicalStructuresSubmodel(
            ConfiguredProduct configuredProduct
        )
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
            entryNode.IdShort = configuredProduct.Name.Replace(" ", "_");
            entryNode.GlobalAssetId = configuredProduct.GlobalAssetId;
            entryNode.Statements = [];
            var first = new Reference(
                ReferenceTypes.ModelReference,
                [
                    new Key(KeyTypes.Submodel, hierarchicalStructures.Id),
                    new Key(KeyTypes.Entity, entryNode.IdShort),
                ]
            );
            // die elemente einfügen, die in der Konfiguration des Produkts enthalten sind
            foreach (var part in configuredProduct.Bestandteile)
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
    }
}
