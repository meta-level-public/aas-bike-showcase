using AasCore.Aas3_0;

namespace AasDemoapp.Utils.Registry;

public class ProtocolInformation
{
    public required string Href { get; set; }
    public string? EndpointProtocol { get; set; }
    public List<string>? EndpointProtocolVersion { get; set; }
    public string? Subprotocol { get; set; }
    public string? SubprotocolBody { get; set; }
    public string? SubprotocolBodyEncoding { get; set; }
    public object? SecurityAttributes { get; set; }
}

public class ProtocolInformationSecurityAttribute
{
    public required ProtocolInformationSecurityAttributeType Type { get; set; }
    public required string Key { get; set; }
    public required string Value { get; set; }
}

public enum ProtocolInformationSecurityAttributeType
{
    NONE,
    RFC_TLSA,
    W3C_DID,
}

public class Endpoint
{
    public string Interface { get; set; } = "AAS-3.0";
    public ProtocolInformation? ProtocolInformation { get; set; }
}

public class SubmodelDescriptor
{
    public List<LangStringTextType>? Description { get; set; }
    public List<LangStringNameType>? DisplayName { get; set; }
    public MyAdministrativeInformation? Administration { get; set; }
    public string? IdShort { get; set; }
    public required string Id { get; set; }
    public MyReference? SemanticId { get; set; }
    public List<MyReference>? SupplementalSemanticIds { get; set; }
    public required List<Endpoint> Endpoints { get; set; }
}

public class AssetAdministrationShellDescriptor
{
    public List<LangStringTextType>? Description { get; set; }
    public List<LangStringNameType>? DisplayName { get; set; }
    public MyAdministrativeInformation? Administration { get; set; }
    public AssetKind AssetKind { get; set; }
    public string? AssetType { get; set; }
    public required List<Endpoint> Endpoints { get; set; }
    public string? GlobalAssetId { get; set; }
    public string? IdShort { get; set; }
    public required string Id { get; set; }
    public List<MySpecificAssetId>? SpecificAssetIds { get; set; }
    public List<SubmodelDescriptor>? SubmodelDescriptors { get; set; }
}

public class MyAdministrativeInformation
{
    public MyAdministrativeInformation(IAdministrativeInformation? administration)
    {
        if (administration == null)
            return;
        EmbeddedDataSpecifications = administration
            .EmbeddedDataSpecifications?.Cast<EmbeddedDataSpecification>()
            .ToList();
        if (administration.Creator != null)
        {
            var refObj = new MyReference(administration.Creator);
            if (refObj != null)
                Creator = refObj;
        }

        Revision = administration.Revision;
        TemplateId = SanitizeTemplateId(administration.TemplateId);
        Version = administration.Version;
    }

    private static string? SanitizeTemplateId(string? templateId)
    {
        if (string.IsNullOrEmpty(templateId))
            return templateId;

        // The BaSyx registry has a very restrictive validation pattern for templateId
        // that appears to reject standard URL characters (colon, slash, hyphen, etc.)
        // According to the AAS spec, templateId should be a simple string identifier,
        // not a full URL. If the templateId contains URL-like content, we need to
        // either extract just the identifier part or omit it entirely.

        // If templateId looks like a URL (contains ://), return null to omit it
        // rather than sending invalid data to the registry
        if (templateId.Contains("://"))
        {
            Console.WriteLine(
                $"[AssetAdministrationShellDescriptor] Omitting templateId '{templateId}' - contains URL format not accepted by registry"
            );
            return null;
        }

        // Fix common typos in template IDs:
        // 1. Fix missing dot: "admin-shell-io" -> "admin-shell.io"
        templateId = templateId.Replace("admin-shell-io/", "admin-shell.io/");

        // 2. Fix spaces that should be dashes (e.g., "IDTA 02023-1-0" -> "IDTA-02023-1-0")
        // Match pattern: IDTA followed by space and digits
        if (templateId.Contains("IDTA "))
        {
            templateId = System.Text.RegularExpressions.Regex.Replace(
                templateId,
                @"(IDTA)\s+(\d)",
                "$1-$2"
            );
        }

        // 3. Remove any invisible or special Unicode characters that might cause validation issues
        // Keep only visible ASCII and common URL characters
        templateId = System.Text.RegularExpressions.Regex.Replace(templateId, @"[^\x20-\x7E]", "");

        // 4. Normalize the templateId to ensure it's properly encoded
        templateId = templateId.Normalize(System.Text.NormalizationForm.FormC);

        return templateId;
    }

    public List<EmbeddedDataSpecification>? EmbeddedDataSpecifications { get; set; }
    public string? Version { get; set; }
    public string? Revision { get; set; }
    public MyReference? Creator { get; set; }
    public string? TemplateId { get; set; }
}

public class MySpecificAssetId
{
    public MySpecificAssetId(ISpecificAssetId specificAssetId)
    {
        if (specificAssetId == null)
            return;
        SemanticId = new MyReference(specificAssetId.SemanticId);
        Name = specificAssetId.Name;
        Value = specificAssetId.Value;
        ExternalSubjectId = new MyReference(specificAssetId.ExternalSubjectId);
    }

    public MyReference? SemanticId { get; set; }
    public List<MyReference>? SupplementalSemanticIds { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
    public MyReference? ExternalSubjectId { get; set; }
}

public class MyReference
{
    public MyReference(IReference? externalSubjectId)
    {
        if (externalSubjectId == null || externalSubjectId.Keys == null)
            return;
        Type = externalSubjectId.Type;
        ReferredSemanticId = (MyReference?)externalSubjectId.ReferredSemanticId;

        // Ensure Keys list is not empty - registry requires at least 1 key
        var keysList = externalSubjectId.Keys?.Cast<Key>().ToList();
        if (keysList != null && keysList.Count > 0)
        {
            Keys = keysList;
        }
        else
        {
            // Don't set Keys if it would be empty - leave it null instead
            Keys = null;
        }
    }

    public ReferenceTypes Type { get; set; }
    public MyReference? ReferredSemanticId { get; set; }
    public List<Key>? Keys { get; set; }
}
