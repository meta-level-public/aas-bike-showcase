using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database.Model;

namespace AasDemoapp.Import
{
    /// <summary>
    /// Interface für den Import-Service
    /// </summary>
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

        Submodel? GetCarbonFootprint(AasCore.Aas3_0.Environment env);

        bool HasCarbonFootprintSubmodel(AasCore.Aas3_0.Environment env);
    }
}
