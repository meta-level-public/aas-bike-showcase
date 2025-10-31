using System.Collections.Generic;
using AasCore.Aas3_0;
using AasDemoapp.Utils;
using AasDemoapp.Utils.Registry;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AasDemoapp.Import
{
    /// <summary>
    /// Factory für das Erstellen von Shell- und Submodel-Descriptors
    /// </summary>
    public class DescriptorFactory
    {
        /// <summary>
        /// Erstellt einen JSON-String für einen Shell-Descriptor
        /// </summary>
        public string CreateShellDescriptorJson(
            AssetAdministrationShell aas,
            List<Submodel> submodels,
            string localRepositoryUrl
        )
        {
            var shellEndpoint = $"{localRepositoryUrl}/shells/{aas.Id.ToBase64()}";

            var endpoint = new AasDemoapp.Utils.Registry.Endpoint()
            {
                Interface = "AssetAdministrationShell",
                ProtocolInformation = new ProtocolInformation() { Href = shellEndpoint },
            };

            var endpointList = new List<AasDemoapp.Utils.Registry.Endpoint> { endpoint };

            var submodelDescriptors = new List<SubmodelDescriptor>();
            foreach (var sm in submodels)
            {
                submodelDescriptors.Add(CreateSubmodelDescriptor(sm, localRepositoryUrl));
            }

            var aasDescriptor = new AssetAdministrationShellDescriptor()
            {
                Administration =
                    aas.Administration != null
                        ? new MyAdministrativeInformation(aas.Administration)
                        : null,
                AssetKind = aas.AssetInformation.AssetKind,
                AssetType =
                    aas.AssetInformation.AssetType
                    ?? aas.AssetInformation.AssetKind.ToString() ?? string.Empty,
                Description = aas.Description?.Cast<LangStringTextType>().ToList(),
                DisplayName = aas.DisplayName?.Cast<LangStringNameType>().ToList(),
                Endpoints = endpointList,
                GlobalAssetId = aas.AssetInformation.GlobalAssetId ?? string.Empty,
                Id = aas.Id,
                IdShort = aas.IdShort ?? string.Empty,
                SpecificAssetIds = aas
                    .AssetInformation.SpecificAssetIds?.Select(x => new MySpecificAssetId(x))
                    .ToList(),
                SubmodelDescriptors = submodelDescriptors,
            };

            var contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy(),
            };

            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter> { new StringEnumConverter() },
                NullValueHandling = NullValueHandling.Ignore,
            };

            return JsonConvert.SerializeObject(aasDescriptor, serializerSettings);
        }

        /// <summary>
        /// Erstellt einen Submodel-Descriptor
        /// </summary>
        private SubmodelDescriptor CreateSubmodelDescriptor(Submodel sm, string smRepoUrl)
        {
            var endpoint = new AasDemoapp.Utils.Registry.Endpoint()
            {
                Interface = "Submodel",
                ProtocolInformation = new ProtocolInformation()
                {
                    Href = $"{smRepoUrl}/submodels/{sm.Id.ToBase64()}",
                },
            };

            var endpointList = new List<AasDemoapp.Utils.Registry.Endpoint> { endpoint };

            return new SubmodelDescriptor()
            {
                Administration =
                    sm.Administration != null
                        ? new MyAdministrativeInformation(sm.Administration)
                        : null,
                Description = sm.Description?.Cast<LangStringTextType>().ToList(),
                DisplayName = sm.DisplayName?.Cast<LangStringNameType>().ToList(),
                Endpoints = endpointList,
                IdShort = sm.IdShort ?? string.Empty,
                Id = sm.Id,
                SemanticId = sm.SemanticId != null ? new MyReference(sm.SemanticId) : null,
                SupplementalSemanticIds = sm
                    .SupplementalSemanticIds?.Select(x => new MyReference(x))
                    .ToList(),
            };
        }
    }
}
