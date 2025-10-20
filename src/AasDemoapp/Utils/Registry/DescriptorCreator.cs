using System.Text;
using AasCore.Aas3_0;
using AasDemoapp.Utils.Shells;

namespace AasDemoapp.Utils.Registry;

public class DescriptorCreator
{
    public static AssetAdministrationShellDescriptor CreateDescriptor(
        IAssetAdministrationShell aas,
        List<ISubmodel> submodels,
        AasUrls aasUrls
    )
    {
        // TODO: die Urls ggf aus dem EditorDescriptor holen
        var descriptor = new AssetAdministrationShellDescriptor()
        {
            Id = aas.Id,
            Endpoints =
            [
                CreateAasEndpoint(aasUrls.AasRepositoryUrl.AppendSlash() + "shells", aas.Id),
            ],
        };

        descriptor.IdShort = aas.IdShort;
        descriptor.DisplayName = aas.DisplayName?.Cast<LangStringNameType>().ToList();
        descriptor.Description = aas.Description?.Cast<LangStringTextType>().ToList();
        descriptor.AssetKind = aas.AssetInformation.AssetKind;
        descriptor.AssetType = aas.AssetInformation.AssetType;
        descriptor.GlobalAssetId = aas.AssetInformation.GlobalAssetId;
        aas.AssetInformation.SpecificAssetIds?.ForEach(
            (specificAssetId) =>
            {
                descriptor.SpecificAssetIds?.Add(new MySpecificAssetId(specificAssetId));
            }
        );

        descriptor.SubmodelDescriptors = new List<SubmodelDescriptor>();
        submodels.ForEach(submodel =>
            descriptor.SubmodelDescriptors.Add(
                CreateSubmodelDescriptor(submodel, aasUrls.SubmodelRepositoryUrl)
            )
        );
        descriptor.Administration = new MyAdministrativeInformation(aas.Administration);

        return descriptor;
    }

    public static SubmodelDescriptor CreateSubmodelDescriptor(
        ISubmodel submodel,
        string submodelRepositoryUrl
    )
    {
        var descriptor = new SubmodelDescriptor()
        {
            Id = submodel.Id,
            Endpoints =
            [
                CreateSmEndpoint(submodelRepositoryUrl.AppendSlash() + "submodels", submodel.Id),
            ],
        };

        descriptor.IdShort = submodel.IdShort;
        descriptor.DisplayName = submodel.DisplayName?.Cast<LangStringNameType>().ToList();
        descriptor.Description = submodel.Description?.Cast<LangStringTextType>().ToList();
        if (submodel.SemanticId != null)
        {
            var refObj = new MyReference(submodel.SemanticId);
            if (refObj != null)
                descriptor.SemanticId = refObj;
        }
        submodel.SupplementalSemanticIds?.ForEach(
            (supplmentalSemanticId) =>
            {
                descriptor.SupplementalSemanticIds?.Add(new MyReference(supplmentalSemanticId));
            }
        );

        return descriptor;
    }

    public static Endpoint CreateAasEndpoint(string baseHref, string id)
    {
        return new Endpoint()
        {
            Interface = "AAS-3.0",
            ProtocolInformation = new ProtocolInformation()
            {
                Href = baseHref.AppendSlash() + id.ToBase64UrlEncoded(Encoding.UTF8),
                // EndpointProtocol = baseHref.Split("://")[0] + "://",
                EndpointProtocol = baseHref.Split("://")[0],
            },
        };
    }

    public static Endpoint CreateSmEndpoint(string baseHref, string id)
    {
        return new Endpoint()
        {
            Interface = "SUBMODEL-3.0",
            ProtocolInformation = new ProtocolInformation()
            {
                Href = baseHref.AppendSlash() + id.ToBase64UrlEncoded(Encoding.UTF8),
                // EndpointProtocol = baseHref.Split("://")[0] + "://",
                EndpointProtocol = baseHref.Split("://")[0],
            },
        };
    }

    public static SubmodelDescriptor CreateSubmodelDescriptorWithFullUrl(
        Submodel submodel,
        string url
    )
    {
        var descriptor = new SubmodelDescriptor()
        {
            Id = submodel.Id,
            Endpoints =
            [
                new Endpoint()
                {
                    Interface = "SUBMODEL-3.0",
                    ProtocolInformation = new ProtocolInformation()
                    {
                        Href = url,
                        // EndpointProtocol = url.Split("://")[0] + "://",
                        EndpointProtocol = url.Split("://")[0],
                    },
                },
            ],
        };

        descriptor.IdShort = submodel.IdShort;
        descriptor.DisplayName = submodel.DisplayName?.Cast<LangStringNameType>().ToList();
        descriptor.Description = submodel.Description?.Cast<LangStringTextType>().ToList();
        if (descriptor.SemanticId != null)
        {
            var refObj = new MyReference(submodel.SemanticId);
            if (refObj != null)
                descriptor.SemanticId = refObj;
        }
        submodel.SupplementalSemanticIds?.ForEach(
            (supplmentalSemanticId) =>
            {
                descriptor.SupplementalSemanticIds?.Add(new MyReference(supplmentalSemanticId));
            }
        );

        return descriptor;
    }
}
