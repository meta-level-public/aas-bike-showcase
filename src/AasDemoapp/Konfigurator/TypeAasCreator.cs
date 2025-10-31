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
            AasLocalPublisher localPublisher,
            ISettingService settingsService
        )
        {
            var idPrefix =
                settingsService.GetSetting(SettingTypes.AasIdPrefix)?.Value
                ?? "https://oi4-nextbike.de";
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

            var nameplate = CreateNameplateSubmodel(idPrefix);
            aas.Submodels =
            [
                new Reference(
                    ReferenceTypes.ModelReference,
                    [new Key(KeyTypes.Submodel, nameplate.Id)]
                ),
            ];

            var handoverdoc = CreateHandoverDocumentation(idPrefix);
            aas.Submodels.Add(
                new Reference(
                    ReferenceTypes.ModelReference,
                    [new Key(KeyTypes.Submodel, handoverdoc.Id)]
                )
            );

            var hierarchicalStructures = CreateHierarchicalStructuresSubmodel(
                configuredProduct,
                idPrefix
            );
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
                localPublisher,
                settingsService
            );
        }

        private static async Task SaveAasToRepositories(
            AssetAdministrationShell aas,
            Submodel nameplate,
            Submodel handoverdoc,
            Submodel hierarchicalStructures,
            AasLocalPublisher localPublisher,
            ISettingService settingsService
        )
        {
            var aasRepositoryUrl =
                settingsService.GetSetting(SettingTypes.AasRepositoryUrl)?.Value ?? "";
            var securitySetting = settingsService.GetSecuritySetting(
                SettingTypes.InfrastructureSecurity
            );

            var submodels = new List<Submodel> { nameplate, handoverdoc, hierarchicalStructures };

            await localPublisher.PushToRepositoryAsync(
                aas,
                submodels,
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

        private static Submodel CreateHandoverDocumentation(string idPrefix)
        {
            var handoverdoc = HandoverDocumentationCreator.CreateFromJson(idPrefix);
            handoverdoc.Description =
            [
                new LangStringTextType("de", "Handover documentation for the configured product"),
            ];

            return handoverdoc;
        }

        private static Submodel CreateNameplateSubmodel(string idPrefix)
        {
            var nameplate = NameplateCreator.CreateFromJson(idPrefix);
            PropertyValueChanger.SetPropertyValueByPath(
                "ManufacturerName",
                "OI4 Nextbike",
                nameplate
            );

            return nameplate;
        }

        private static Submodel CreateHierarchicalStructuresSubmodel(
            ConfiguredProduct configuredProduct,
            string idPrefix
        )
        {
            var hierarchicalStructures = HierarchicalStructuresCreator.CreateFromJson(idPrefix);
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
