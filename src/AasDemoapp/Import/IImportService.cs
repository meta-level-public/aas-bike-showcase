using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database.Model;

namespace AasDemoapp.Import
{
    public interface IImportService
    {
        Task<string> ImportFromRepository(
            string decodedLocalUrl,
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
    }
}
