using System.Text;
using AasCore.Aas3_0;
using Microsoft.AspNetCore.StaticFiles;

namespace AasDemoapp.Utils;

public static class FilesFromAasResolver
{
    public static List<AasFileContent> GetAllAasFiles(AasCore.Aas3_0.Environment env, string submodelEndpoint, string aasEndpoint)
    {
        List<AasFileContent> aasFiles = [];

        env.Submodels?.ForEach(sm =>
        {
            var smEndpoint = submodelEndpoint.AppendSlash() + "submodels/" + sm.Id.ToBase64UrlEncoded(Encoding.UTF8).AppendSlash() + "submodel-elements";
            if (sm.SubmodelElements != null) GetAllAasFilesRecursively(sm.SubmodelElements, aasFiles, smEndpoint, string.Empty, sm.Id);
        });

        env.AssetAdministrationShells?.ForEach(aas =>
        {
            if (aas.AssetInformation == null || aas.AssetInformation.DefaultThumbnail == null) return;
            var thumbnailEndpoint = aasEndpoint.AppendSlash() + "shells/" + aas.Id.ToBase64UrlEncoded(Encoding.UTF8).AppendSlash() + "asset-information/thumbnail";
            aasFiles.Add(new AasFileContent
            {
                Filename = Path.GetFileName(aas.AssetInformation.DefaultThumbnail.Path),
                Endpoint = thumbnailEndpoint,
                ContentType = aas.AssetInformation.DefaultThumbnail.ContentType ?? "image/png",
                idShortPath = string.Empty,
                IsThumbnail = true,
                Path = aas.AssetInformation.DefaultThumbnail.Path,
                SubmodelId = string.Empty
            });
        });


        return aasFiles;
    }

    private static void GetAllAasFilesRecursively(List<ISubmodelElement> smElements, List<AasFileContent> aasFiles, string submodelEndpoint, string idShortPath, string submodelId, long count = 0, bool indexed = false)
    {
        var counter = 0;
        smElements.ForEach(smEl =>
        {
            count++;
            if (smEl is SubmodelElementCollection submodelElementCollection)
            {
                var newPath =  idShortPath.AppendIdShortPath(submodelElementCollection.IdShort ?? string.Empty);
                if (indexed)
                {
                    newPath = idShortPath.AppendIndexPath(counter);
                    counter++;
                }
                if (submodelElementCollection.Value != null) GetAllAasFilesRecursively(submodelElementCollection.Value, aasFiles, submodelEndpoint,newPath, submodelId);
            }
            if (smEl is SubmodelElementList submodelElementList)
            {
                var newPath =  idShortPath.AppendIdShortPath(submodelElementList.IdShort ?? string.Empty);
                if (indexed)
                {
                    newPath = idShortPath.AppendIndexPath(counter);
                    counter++;
                }
                if (submodelElementList.Value != null) GetAllAasFilesRecursively(submodelElementList.Value, aasFiles, submodelEndpoint, newPath, submodelId, count, true);
            }
            if (smEl is AasCore.Aas3_0.File smFile)
            {
                if (!string.IsNullOrWhiteSpace(smFile.Value) && !string.IsNullOrWhiteSpace(smEl.IdShort))
                {
                    var endpoint = submodelEndpoint.AppendSlash() + idShortPath.AppendIdShortPath(smEl.IdShort) + "/attachment";
                    if (indexed)
                    {
                        endpoint = submodelEndpoint.AppendSlash() + idShortPath.AppendIndexPath(count) + "/attachment";
                    }
                    aasFiles.Add(new AasFileContent
                    {
                        Filename = Path.GetFileName(smFile.Value),
                        Endpoint = endpoint,
                        ContentType = GetContentType(smFile),
                        idShortPath = idShortPath.AppendIdShortPath(smEl.IdShort),
                        IsThumbnail = false,
                        Path = smFile.Value,
                        SubmodelId = submodelId
                    });
                }
            }
        });
    }

    private static string GetContentType(AasCore.Aas3_0.File file)
    {
        if (!string.IsNullOrWhiteSpace(file.ContentType)) return file.ContentType;
        else
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(file.Value ?? string.Empty, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            if (string.IsNullOrWhiteSpace(contentType)) contentType = "application/octet-stream";

            return contentType;
        }
    }
}