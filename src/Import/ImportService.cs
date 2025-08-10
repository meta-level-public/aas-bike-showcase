using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Utils;
using IO.Swagger.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AasDemoapp.Import
{
    public class ImportService
    {
        private readonly AasDemoappContext _AasDemoappContext;
        private const string nameplateId = "https://admin-shell.io/zvei/nameplate/2/0/Nameplate";
        private const string productFamiliyId = "0173-1#02-AAU731#001";

        public ImportService(AasDemoappContext AasDemoappContext)
        {
            _AasDemoappContext = AasDemoappContext;

        }

        public async Task<string> ImportFromRepository(string decodedLocalUrl, KatalogEintrag katalogEintrag, SecuritySetting securitySetting, string decodedId, bool saveChanges = true)
        {
            using var clientSource = HttpClientCreator.CreateHttpClient(securitySetting);
            using HttpResponseMessage response = await clientSource.GetAsync(katalogEintrag.Supplier.RemoteRepositoryUrl + $"/shells/{decodedId.ToBase64()}");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var shell = Jsonization.Deserialize.AssetAdministrationShellFrom(JsonNode.Parse(responseBody));
            var submodels = new List<Submodel>();

            foreach (var smRef in shell.Submodels ?? [])
            {
                using var resp = await clientSource.GetAsync(katalogEintrag.Supplier.RemoteRepositoryUrl + $"/submodels/{smRef.Keys?[0]?.Value.ToBase64()}");
                resp.EnsureSuccessStatusCode();
                responseBody = await resp.Content.ReadAsStringAsync();

                var sm = Jsonization.Deserialize.SubmodelFrom(JsonNode.Parse(responseBody));
                submodels.Add(sm);
            }
            // Id umschreiben
            if (shell.DerivedFrom == null)
            {
                shell.DerivedFrom = new Reference(ReferenceTypes.ExternalReference, new List<IKey>(), null);
            }
            shell.DerivedFrom.Keys.Add(new Key(KeyTypes.AssetAdministrationShell, shell.Id));
            shell.Id = Guid.NewGuid().ToString();

            await PushNewToLocalRepositoryAsync(shell, submodels, decodedLocalUrl, securitySetting);
            await PushNewToLocalRegistryAsync(shell, submodels, decodedLocalUrl, securitySetting);
            await PushNewToLocalDiscoveryAsync(shell, submodels, decodedLocalUrl, securitySetting);

            ImportedShell importedShell = new()
            {
                RemoteRegistryUrl = katalogEintrag.Supplier.RemoteRepositoryUrl,
            };

            _AasDemoappContext.Add(importedShell);
            if (saveChanges) _AasDemoappContext.SaveChanges();

            return shell.Id;
        }

        public async Task<string> GetImageString(string decodedRemoteUrl, SecuritySetting securitySetting, string decodedId)
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            using HttpResponseMessage response = await client.GetAsync(decodedRemoteUrl + $"/shells/{decodedId.ToBase64()}/asset-information/thumbnail");
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsByteArrayAsync();

            return Convert.ToBase64String(responseBody);
        }

        public async Task<AasCore.Aas3_0.Environment> GetEnvironment(string decodedRemoteUrl, SecuritySetting securitySetting, string decodedId)
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            var url = decodedRemoteUrl + $"/shells/{decodedId.ToBase64UrlEncoded(Encoding.UTF8)}";
            using HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var shell = Jsonization.Deserialize.AssetAdministrationShellFrom(JsonNode.Parse(responseBody));
            var submodels = new List<ISubmodel>();

            foreach (var smRef in shell.Submodels ?? [])
            {
                using var resp = await client.GetAsync(decodedRemoteUrl + $"/submodels/{smRef.Keys?[0]?.Value.ToBase64()}");
                resp.EnsureSuccessStatusCode();
                responseBody = await resp.Content.ReadAsStringAsync();

                var sm = Jsonization.Deserialize.SubmodelFrom(JsonNode.Parse(responseBody));
                submodels.Add(sm);
            }

            var env = new AasCore.Aas3_0.Environment();
            env.AssetAdministrationShells = [shell];
            env.Submodels = submodels;

            return env;
        }

        public Submodel? GetNameplate(AasCore.Aas3_0.Environment env)
        {
            var nameplate = env.Submodels?.Find(sm => sm.SemanticId != null && sm.SemanticId.Keys != null && sm.SemanticId.Keys.Exists(id => id.Value == nameplateId));

            return (Submodel?)nameplate;

        }

        public string GetKategorie(Submodel nameplate)
        {
            var kategorie = string.Empty;

            if (nameplate != null)
            {
                var mlp = (MultiLanguageProperty?)nameplate.SubmodelElements?.Find(sme => sme.SemanticId != null && sme.SemanticId.Keys != null && sme.SemanticId.Keys.Exists(id => id.Value == productFamiliyId));
                if (mlp != null)
                {
                    kategorie = mlp.Value?.FirstOrDefault()?.Text ?? string.Empty;
                }
            }

            return kategorie;
        }

        public string GetKategorie(AasCore.Aas3_0.Environment env)
        {
            var kategorie = string.Empty;

            var nameplate = (Submodel?)GetNameplate(env);
            return nameplate != null ? GetKategorie(nameplate) : kategorie;
        }

        public async Task PushNewToLocalRegistryAsync(AssetAdministrationShell shell, List<Submodel> submodels, string localRegistryUrl, SecuritySetting securitySetting)
        {
            var jsonString = CreateShellDescriptorString(shell, submodels, localRegistryUrl);

            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{localRegistryUrl}/shell-descriptors");

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            request.Content = content;
            var result = await client.SendAsync(request);
            var resultContent = await result.Content.ReadAsStringAsync();

            Console.WriteLine(resultContent);

        }

        public async Task<bool> PushNewToLocalDiscoveryAsync(AssetAdministrationShell shell, List<Submodel> submodels, string localRegistryUrl, SecuritySetting securitySetting)
        {
            var jsonString = CreateShellDescriptorString(shell, submodels, localRegistryUrl);

            var client = HttpClientCreator.CreateHttpClient(securitySetting);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{localRegistryUrl}/registry/shell-descriptors");

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            request.Content = content;
            var result = await client.SendAsync(request);
            var resultContent = await result.Content.ReadAsStringAsync();

            Console.WriteLine(resultContent);
            return await Task.FromResult(true);

        }

        public async Task PushNewToLocalRepositoryAsync(AssetAdministrationShell shell, List<Submodel> submodels, string localRepositoryUrl, SecuritySetting securitySetting)
        {
            var jsonString = Jsonization.Serialize.ToJsonObject(shell).ToJsonString();

            var client = HttpClientCreator.CreateHttpClient(securitySetting);
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{localRepositoryUrl.AppendSlash()}shells");

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            request.Content = content;
            var result = await client.SendAsync(request);
            var resultContent = await result.Content.ReadAsStringAsync();

            Console.WriteLine(resultContent);

            submodels.ForEach(async sm =>
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, $"{localRepositoryUrl.AppendSlash()}submodels");
                jsonString = Jsonization.Serialize.ToJsonObject(sm).ToJsonString();

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                request.Content = content;
                result = await client.SendAsync(request);
                resultContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(resultContent);


            });

        }


        private string CreateShellDescriptorString(AssetAdministrationShell aas, List<Submodel> submodels, string localRegistryUrl)
        {
            var shellEndpoint = $"{localRegistryUrl}/shells/{aas.Id.ToBase64()}";

            var iEndpoint = new IO.Swagger.Model.Endpoint()
            {
                _Interface = "AssetAdministrationShell",
                ProtocolInformation = new ProtocolInformation()
                {
                    Href = shellEndpoint
                }
            };

            var endpointList = new List<IO.Swagger.Model.Endpoint>
        {
            iEndpoint
        };

            var submodelDescriptors = new List<SubmodelDescriptor>();

            submodels.ForEach(sm =>
            {
                submodelDescriptors.Add(CreateSubmodelDescriptors(sm, shellEndpoint));
            });

            var aasDescriptor = new AssetAdministrationShellDescriptor()
            {
                Administration = (AdministrativeInformation?)aas.Administration ?? new AdministrativeInformation(),
                AssetKind = aas.AssetInformation.AssetKind,
                AssetType = aas.AssetInformation.AssetType ?? aas.AssetInformation.AssetKind.ToString() ?? string.Empty,
                Description = aas.Description ?? [],
                DisplayName = aas.DisplayName ?? [],
                Endpoints = endpointList,
                Extensions = aas.Extensions ?? [],
                GlobalAssetId = aas.AssetInformation.GlobalAssetId ?? string.Empty,
                Id = aas.Id,
                IdShort = aas.IdShort ?? string.Empty,
                SpecificAssetIds = aas.AssetInformation.SpecificAssetIds ?? [],
                SubmodelDescriptors = submodelDescriptors
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>
                        {
                            new StringEnumConverter()
                        },
                NullValueHandling = NullValueHandling.Ignore
            };
            return JsonConvert.SerializeObject(aasDescriptor, serializerSettings);
        }

        private SubmodelDescriptor CreateSubmodelDescriptors(Submodel sm, string shellEndpoint)
        {
            var iEndpoint = new IO.Swagger.Model.Endpoint()
            {
                _Interface = "Submodel",
                ProtocolInformation = new ProtocolInformation()
                {
                    Href = $"{shellEndpoint}/submodels/{sm.Id.ToBase64()}"
                }
            };

            var endpointList = new List<IO.Swagger.Model.Endpoint>
                {
                    iEndpoint
                };

            var submodelDescriptor = new SubmodelDescriptor()
            {
                Administration = sm.Administration ?? new AdministrativeInformation(),
                Description = sm.Description ?? [],
                DisplayName = sm.DisplayName ?? [],
                Endpoints = endpointList,
                Extensions = sm.Extensions ?? [],
                IdShort = sm.IdShort ?? string.Empty,
                Id = sm.Id,
                SemanticId = sm.SemanticId ?? new Reference(ReferenceTypes.ExternalReference, []),
                SupplementalSemanticId = sm.SupplementalSemanticIds ?? []
            };

            return submodelDescriptor;
        }

    }
}