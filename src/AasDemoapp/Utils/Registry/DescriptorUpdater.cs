using System.Text;
using AasCore.Aas3_0;
using AasDemoapp.Utils.Shells;

namespace AasDemoapp.Utils.Registry;

public class DescriptorUpdater
{
    public static void UpdateAasDescriptor(
        AssetAdministrationShellDescriptor aasDescriptor,
        EditorDescriptor editorDescriptor,
        AssetAdministrationShell aas,
        List<Submodel> submodels
    )
    {
        aasDescriptor.Id = aas.Id;
        aasDescriptor.IdShort = aas.IdShort;
        aasDescriptor.DisplayName = aas.DisplayName?.Cast<LangStringNameType>().ToList();
        aasDescriptor.Description = aas.Description?.Cast<LangStringTextType>().ToList();
        aasDescriptor.AssetKind = aas.AssetInformation.AssetKind;
        aasDescriptor.AssetType = aas.AssetInformation.AssetType;
        aasDescriptor.GlobalAssetId = aas.AssetInformation.GlobalAssetId;
        aas.AssetInformation.SpecificAssetIds?.ForEach(
            (specificAssetId) =>
            {
                aasDescriptor.SpecificAssetIds?.Add(new MySpecificAssetId(specificAssetId));
            }
        );
        aasDescriptor.Administration = new MyAdministrativeInformation(aas.Administration);

        var smDescriptorsToDelete = new List<SubmodelDescriptor>();
        // SubmodelDescriptoren aktualisieren
        aasDescriptor.SubmodelDescriptors?.ForEach(
            (smDescriptor) =>
            {
                var editorDescriptorEntry = editorDescriptor.SubmodelDescriptorEntries.Find(
                    (entry) => entry.OldId == smDescriptor.Id
                );
                string oldId = editorDescriptorEntry?.OldId ?? "";
                string newId = editorDescriptorEntry?.NewId ?? "";
                var submodel = submodels.Find((submodel) => submodel.Id == newId);
                if (submodel != null)
                {
                    UpdateSubmodelDescriptor(smDescriptor, submodel, oldId);
                }
                else
                {
                    // smDescriptor muss gelöscht werden, da submodel nicht mehr existiert!
                    smDescriptorsToDelete.Add(smDescriptor);
                }
            }
        );
        aasDescriptor.SubmodelDescriptors?.RemoveAll(smDescriptorsToDelete.Contains);
        // jetzt könnte es noch neue Submodels geben
        submodels.ForEach(
            (submodel) =>
            {
                if (
                    aasDescriptor.SubmodelDescriptors?.Find(
                        (smDescriptor) => smDescriptor.Id == submodel.Id
                    ) == null
                )
                {
                    var editorDescriptorEntry = editorDescriptor.SubmodelDescriptorEntries.Find(
                        (entry) => entry.OldId == submodel.Id
                    );
                    if (editorDescriptorEntry != null)
                    {
                        aasDescriptor.SubmodelDescriptors?.Add(
                            DescriptorCreator.CreateSubmodelDescriptorWithFullUrl(
                                submodel,
                                editorDescriptorEntry.Endpoint
                            )
                        );
                    }
                }
            }
        );

        // Endpunkt anpassen
        var oldIdBase64 = editorDescriptor.AasDescriptorEntry.OldId.ToBase64UrlEncoded(
            Encoding.UTF8
        );
        var newIdBase64 = editorDescriptor.AasDescriptorEntry.NewId.ToBase64UrlEncoded(
            Encoding.UTF8
        );
        aasDescriptor.Endpoints.ForEach(
            (endpoint) =>
            {
                if (endpoint.ProtocolInformation?.Href.Contains(oldIdBase64) == true)
                {
                    endpoint.ProtocolInformation.EndpointProtocol =
                        endpoint.ProtocolInformation.EndpointProtocol?.Split("://")[0] ?? "https";
                    endpoint.Interface = "AAS-3.0";
                    endpoint.ProtocolInformation.Href = endpoint.ProtocolInformation.Href.Replace(
                        oldIdBase64,
                        newIdBase64
                    );
                }
            }
        );
    }

    public static void UpdateSubmodelDescriptor(
        SubmodelDescriptor smDescriptor,
        Submodel submodel,
        string oldId
    )
    {
        smDescriptor.Id = submodel.Id;
        smDescriptor.IdShort = submodel.IdShort;
        smDescriptor.DisplayName = submodel.DisplayName?.Cast<LangStringNameType>().ToList();
        smDescriptor.Description = submodel.Description?.Cast<LangStringTextType>().ToList();
        smDescriptor.SemanticId = new MyReference(submodel.SemanticId);
        submodel.SupplementalSemanticIds?.ForEach(
            (supplmentalSemanticId) =>
            {
                smDescriptor.SupplementalSemanticIds?.Add(new MyReference(supplmentalSemanticId));
            }
        );

        // Endpunkt anpassen
        var oldIdBase64 = oldId.ToBase64UrlEncoded(Encoding.UTF8);
        var newIdBase64 = submodel.Id.ToBase64UrlEncoded(Encoding.UTF8);
        smDescriptor.Endpoints.ForEach(
            (endpoint) =>
            {
                if (endpoint.ProtocolInformation?.Href.Contains(oldIdBase64) == true)
                {
                    endpoint.ProtocolInformation.EndpointProtocol =
                        endpoint.ProtocolInformation.EndpointProtocol?.Split("://")[0] ?? "https";
                    endpoint.Interface = "SUBMODEL-3.0";
                    endpoint.ProtocolInformation.Href = endpoint.ProtocolInformation.Href.Replace(
                        oldIdBase64,
                        newIdBase64
                    );
                }
            }
        );
    }

    public static Endpoint CreateSmEndpoint(string baseHref, string id)
    {
        return new Endpoint()
        {
            Interface = "SUBMODEL-3.0",
            ProtocolInformation = new ProtocolInformation()
            {
                Href = baseHref + id.ToBase64UrlEncoded(Encoding.UTF8),
                // EndpointProtocol = baseHref.Split("://")[0] + "://",
                EndpointProtocol = baseHref.Split("://")[0],
            },
        };
    }

    public static Endpoint CreateAasEndpoint(string baseHref, string id)
    {
        return new Endpoint()
        {
            Interface = "AAS-3.0",
            ProtocolInformation = new ProtocolInformation()
            {
                Href = baseHref + id.ToBase64UrlEncoded(Encoding.UTF8),
                // EndpointProtocol = baseHref.Split("://")[0] + "://",
                EndpointProtocol = baseHref.Split("://")[0],
            },
        };
    }
}
