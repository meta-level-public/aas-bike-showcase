using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database.Model;

namespace AasDemoapp.Import
{
    public interface IImportService
    {
        Task<string> ImportFromRepository(
            string aasRepositoryUrl,
            string aasRegistryUrl,
            string aasDiscoveryUrl,
            KatalogEintrag katalogEintrag,
            SecuritySetting securitySetting,
            string decodedId,
            bool saveChanges = true
        );

        Task<string> GetImageString(
            string decodedRemoteUrl,
            SecuritySetting securitySetting,
            string decodedId
        );

        Task<AasCore.Aas3_0.Environment> GetEnvironment(
            string decodedRemoteUrl,
            SecuritySetting securitySetting,
            string decodedId
        );

        Submodel? GetNameplate(AasCore.Aas3_0.Environment env);

        string GetKategorie(Submodel nameplate);

        string GetKategorie(AasCore.Aas3_0.Environment env);

        Task PushNewToLocalRepositoryAsync(
            AssetAdministrationShell shell,
            List<Submodel> submodels,
            string localRepositoryUrl,
            SecuritySetting securitySetting,
            byte[]? thumbnailData = null,
            string? thumbnailContentType = null,
            string? thumbnailFilename = null
        );

        Task PushNewToLocalRegistryAsync(
            AssetAdministrationShell shell,
            List<Submodel> submodels,
            string localRepositoryUrl,
            string localRegistryUrl,
            SecuritySetting securitySetting
        );

        Task<bool> PushNewToLocalDiscoveryAsync(
            AssetAdministrationShell shell,
            string localDiscoveryUrl,
            SecuritySetting securitySetting
        );
    }
}
