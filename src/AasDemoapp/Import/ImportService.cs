using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Settings;

namespace AasDemoapp.Import
{
    /// <summary>
    /// Hauptservice für das Importieren von AAS-Daten aus Remote-Repositories
    /// </summary>
    public class ImportService : IImportService
    {
        private readonly AasDemoappContext _AasDemoappContext;
        private readonly ISettingService _settingService;
        private readonly AasRemoteClient _remoteClient;
        private readonly AasLocalPublisher _localPublisher;
        private readonly SubmodelAnalyzer _submodelAnalyzer;

        public ImportService(
            AasDemoappContext AasDemoappContext,
            ISettingService settingService,
            AasRemoteClient remoteClient,
            AasLocalPublisher localPublisher,
            SubmodelAnalyzer submodelAnalyzer
        )
        {
            _AasDemoappContext = AasDemoappContext;
            _settingService = settingService;
            _remoteClient = remoteClient;
            _localPublisher = localPublisher;
            _submodelAnalyzer = submodelAnalyzer;
        }

        /// <summary>
        /// Importiert eine AAS aus einem Remote-Repository in lokale Repositories
        /// </summary>
        public async Task<string> ImportFromRepository(
            string aasRepositoryUrl,
            string aasRegistryUrl,
            string aasDiscoveryUrl,
            KatalogEintrag katalogEintrag,
            SecuritySetting securitySetting,
            string decodedId,
            bool saveChanges = true
        )
        {
            if (katalogEintrag.Supplier?.RemoteAasRepositoryUrl == null)
            {
                throw new ArgumentException(
                    "Supplier or RemoteAasRepositoryUrl is null",
                    nameof(katalogEintrag)
                );
            }

            // Shell und Submodels vom Remote-Repository abrufen
            var (shell, submodels) = await _remoteClient.FetchShellWithSubmodelsAsync(
                katalogEintrag.Supplier.RemoteAasRepositoryUrl,
                katalogEintrag.Supplier.RemoteSmRepositoryUrl,
                decodedId,
                securitySetting
            );

            // Original-ID für DerivedFrom speichern
            if (shell.DerivedFrom == null)
            {
                shell.DerivedFrom = new Reference(
                    ReferenceTypes.ExternalReference,
                    new List<IKey>(),
                    null
                );
            }
            shell.DerivedFrom.Keys.Add(new Key(KeyTypes.AssetAdministrationShell, shell.Id));

            // Neue IDs generieren
            var idPrefix =
                _settingService.GetSetting(SettingTypes.AasIdPrefix)?.Value
                ?? "https://oi4-nextbike.de";

            // Neue Shell-ID generieren
            shell.Id = IdGenerationUtil.GenerateId(IdType.Aas, idPrefix);

            // Neue Submodel-IDs generieren und Mapping für Referenzen erstellen
            var submodelIdMapping = new Dictionary<string, string>();
            foreach (var submodel in submodels)
            {
                var oldId = submodel.Id;
                var newId = IdGenerationUtil.GenerateId(IdType.Submodel, idPrefix);
                submodel.Id = newId;
                submodelIdMapping[oldId] = newId;
            }

            // Submodel-Referenzen in der Shell aktualisieren
            if (shell.Submodels != null)
            {
                foreach (var smRef in shell.Submodels)
                {
                    if (smRef.Keys != null && smRef.Keys.Count > 0)
                    {
                        var oldSmId = smRef.Keys[0].Value;
                        if (submodelIdMapping.ContainsKey(oldSmId))
                        {
                            smRef.Keys[0] = new Key(KeyTypes.Submodel, submodelIdMapping[oldSmId]);
                        }
                    }
                }
            }

            // Thumbnail herunterladen
            var (thumbnailData, thumbnailContentType, thumbnailFilename) =
                await _remoteClient.DownloadThumbnailAsync(
                    katalogEintrag.Supplier.RemoteAasRepositoryUrl,
                    decodedId,
                    securitySetting,
                    shell
                );

            // Zu lokalen Repositories pushen
            await _localPublisher.PushToRepositoryAsync(
                shell,
                submodels,
                aasRepositoryUrl,
                securitySetting,
                thumbnailData,
                thumbnailContentType,
                thumbnailFilename
            );

            await _localPublisher.PushToRegistryAsync(
                shell,
                submodels,
                aasRepositoryUrl,
                aasRegistryUrl,
                securitySetting
            );

            await _localPublisher.PushToDiscoveryAsync(shell, aasDiscoveryUrl, securitySetting);

            // Import in Datenbank speichern
            ImportedShell importedShell = new()
            {
                RemoteAasRegistryUrl = katalogEintrag.Supplier.RemoteAasRegistryUrl,
                RemoteSmRegistryUrl = katalogEintrag.Supplier.RemoteSmRegistryUrl,
            };

            _AasDemoappContext.Add(importedShell);
            if (saveChanges)
                _AasDemoappContext.SaveChanges();

            return shell.Id;
        }

        /// <summary>
        /// Ruft ein Thumbnail als Base64-String ab
        /// </summary>
        public async Task<string> GetImageString(
            string decodedRemoteUrl,
            SecuritySetting securitySetting,
            string decodedId
        )
        {
            return await _remoteClient.GetThumbnailAsBase64Async(
                decodedRemoteUrl,
                securitySetting,
                decodedId
            );
        }

        /// <summary>
        /// Erstellt ein Environment-Objekt aus einer Remote-Shell
        /// </summary>
        public async Task<AasCore.Aas3_0.Environment> GetEnvironment(
            string decodedRemoteUrl,
            SecuritySetting securitySetting,
            string decodedId
        )
        {
            return await _remoteClient.GetEnvironmentAsync(
                decodedRemoteUrl,
                securitySetting,
                decodedId
            );
        }

        /// <summary>
        /// Findet das Nameplate-Submodel in einem Environment
        /// </summary>
        public Submodel? GetNameplate(AasCore.Aas3_0.Environment env)
        {
            return _submodelAnalyzer.GetNameplate(env);
        }

        /// <summary>
        /// Extrahiert die Produktkategorie aus einem Nameplate-Submodel
        /// </summary>
        public string GetKategorie(Submodel nameplate)
        {
            return _submodelAnalyzer.GetKategorie(nameplate);
        }

        /// <summary>
        /// Extrahiert die Produktkategorie aus einem Environment
        /// </summary>
        public string GetKategorie(AasCore.Aas3_0.Environment env)
        {
            return _submodelAnalyzer.GetKategorie(env);
        }

        /// <summary>
        /// Findet das CarbonFootprint-Submodel in einem Environment
        /// </summary>
        public Submodel? GetCarbonFootprint(AasCore.Aas3_0.Environment env)
        {
            return _submodelAnalyzer.GetCarbonFootprint(env);
        }

        /// <summary>
        /// Prüft, ob ein Environment ein CarbonFootprint-Submodel besitzt
        /// </summary>
        public bool HasCarbonFootprintSubmodel(AasCore.Aas3_0.Environment env)
        {
            return _submodelAnalyzer.HasCarbonFootprintSubmodel(env);
        }
    }
}
