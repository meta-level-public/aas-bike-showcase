using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.AasHandling;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Settings;
using AasDemoapp.Utils;
using IO.Swagger.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AasDemoapp.Import
{
    public class ImportService : IImportService
    {
        private readonly AasDemoappContext _AasDemoappContext;
        private readonly ISettingService _settingService;
        private const string nameplateId = "https://admin-shell.io/zvei/nameplate/2/0/Nameplate";
        private const string productFamiliyId = "0173-1#02-AAU731#001";
        private const string carbonFootprintId =
            "https://admin-shell.io/idta/CarbonFootprint/CarbonFootprint/0/9";

        public ImportService(AasDemoappContext AasDemoappContext, ISettingService settingService)
        {
            _AasDemoappContext = AasDemoappContext;
            _settingService = settingService;
        }

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

            using var clientSource = HttpClientCreator.CreateHttpClient(securitySetting);
            using HttpResponseMessage response = await clientSource.GetAsync(
                katalogEintrag.Supplier.RemoteAasRepositoryUrl + $"/shells/{decodedId.ToBase64()}"
            );
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var shellNode = JsonNode.Parse(responseBody);
            if (shellNode == null)
            {
                throw new InvalidOperationException("Failed to parse shell JSON response");
            }
            var shell = Jsonization.Deserialize.AssetAdministrationShellFrom(shellNode);
            var submodels = new List<Submodel>();

            foreach (var smRef in shell.Submodels ?? [])
            {
                using var resp = await clientSource.GetAsync(
                    katalogEintrag.Supplier.RemoteSmRepositoryUrl
                        + $"/submodels/{smRef.Keys?[0]?.Value.ToBase64()}"
                );
                resp.EnsureSuccessStatusCode();
                responseBody = await resp.Content.ReadAsStringAsync();

                var smNode = JsonNode.Parse(responseBody);
                if (smNode == null)
                {
                    throw new InvalidOperationException("Failed to parse submodel JSON response");
                }
                var sm = Jsonization.Deserialize.SubmodelFrom(smNode);
                submodels.Add(sm);
            }
            // Id umschreiben
            if (shell.DerivedFrom == null)
            {
                shell.DerivedFrom = new Reference(
                    ReferenceTypes.ExternalReference,
                    new List<IKey>(),
                    null
                );
            }
            shell.DerivedFrom.Keys.Add(new Key(KeyTypes.AssetAdministrationShell, shell.Id));

            // Generiere neue ID mit IdGenerationUtil und dem konfigurierten Präfix
            var idPrefix =
                _settingService.GetSetting(SettingTypes.AasIdPrefix)?.Value
                ?? "https://oi4-nextbike.de";
            shell.Id = IdGenerationUtil.GenerateId(IdType.Aas, idPrefix);

            // Thumbnail von der originalen AAS herunterladen
            byte[]? thumbnailData = null;
            string? thumbnailContentType = null;
            string? thumbnailFilename = null;
            if (shell.AssetInformation?.DefaultThumbnail != null)
            {
                try
                {
                    using var thumbnailResponse = await clientSource.GetAsync(
                        katalogEintrag.Supplier.RemoteAasRepositoryUrl
                            + $"/shells/{decodedId.ToBase64()}/asset-information/thumbnail"
                    );
                    if (thumbnailResponse.IsSuccessStatusCode)
                    {
                        thumbnailData = await thumbnailResponse.Content.ReadAsByteArrayAsync();
                        thumbnailContentType =
                            shell.AssetInformation.DefaultThumbnail.ContentType ?? "image/png";
                        thumbnailFilename =
                            Path.GetFileName(shell.AssetInformation.DefaultThumbnail.Path)
                            ?? "thumbnail.png";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[ImportFromRepository] Failed to download thumbnail: {ex.Message}"
                    );
                }
            }

            await PushNewToLocalRepositoryAsync(
                shell,
                submodels,
                aasRepositoryUrl,
                securitySetting,
                thumbnailData,
                thumbnailContentType,
                thumbnailFilename
            );
            await PushNewToLocalRegistryAsync(
                shell,
                submodels,
                aasRepositoryUrl,
                aasRegistryUrl,
                securitySetting
            );
            await PushNewToLocalDiscoveryAsync(shell, aasDiscoveryUrl, securitySetting);

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

        public async Task<string> GetImageString(
            string decodedRemoteUrl,
            SecuritySetting securitySetting,
            string decodedId
        )
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            using HttpResponseMessage response = await client.GetAsync(
                decodedRemoteUrl + $"/shells/{decodedId.ToBase64()}/asset-information/thumbnail"
            );
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsByteArrayAsync();

            return Convert.ToBase64String(responseBody);
        }

        public async Task<AasCore.Aas3_0.Environment> GetEnvironment(
            string decodedRemoteUrl,
            SecuritySetting securitySetting,
            string decodedId
        )
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            var url = decodedRemoteUrl + $"/shells/{decodedId.ToBase64UrlEncoded(Encoding.UTF8)}";
            using HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            var shellNode = JsonNode.Parse(responseBody);
            if (shellNode == null)
            {
                throw new InvalidOperationException("Failed to parse shell JSON response");
            }
            var shell = Jsonization.Deserialize.AssetAdministrationShellFrom(shellNode);
            var submodels = new List<ISubmodel>();

            foreach (var smRef in shell.Submodels ?? [])
            {
                using var resp = await client.GetAsync(
                    decodedRemoteUrl + $"/submodels/{smRef.Keys?[0]?.Value.ToBase64()}"
                );
                resp.EnsureSuccessStatusCode();
                responseBody = await resp.Content.ReadAsStringAsync();

                var smNode = JsonNode.Parse(responseBody);
                if (smNode == null)
                {
                    throw new InvalidOperationException("Failed to parse submodel JSON response");
                }
                var sm = Jsonization.Deserialize.SubmodelFrom(smNode);
                submodels.Add(sm);
            }

            var env = new AasCore.Aas3_0.Environment();
            env.AssetAdministrationShells = [shell];
            env.Submodels = submodels;

            return env;
        }

        public Submodel? GetNameplate(AasCore.Aas3_0.Environment env)
        {
            var nameplate = env.Submodels?.Find(sm =>
                sm.SemanticId != null
                && sm.SemanticId.Keys != null
                && sm.SemanticId.Keys.Exists(id => id.Value == nameplateId)
            );

            return (Submodel?)nameplate;
        }

        public string GetKategorie(Submodel nameplate)
        {
            var kategorie = string.Empty;

            if (nameplate != null)
            {
                var mlp = (MultiLanguageProperty?)
                    nameplate.SubmodelElements?.Find(sme =>
                        sme.SemanticId != null
                        && sme.SemanticId.Keys != null
                        && sme.SemanticId.Keys.Exists(id => id.Value == productFamiliyId)
                    );
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

        public Submodel? GetCarbonFootprint(AasCore.Aas3_0.Environment env)
        {
            var carbonFootprint = env.Submodels?.Find(sm =>
                sm.SemanticId != null
                && sm.SemanticId.Keys != null
                && sm.SemanticId.Keys.Exists(id => id.Value == carbonFootprintId)
            );

            return (Submodel?)carbonFootprint;
        }

        public bool HasCarbonFootprintSubmodel(AasCore.Aas3_0.Environment env)
        {
            return GetCarbonFootprint(env) != null;
        }

        public async Task PushNewToLocalRegistryAsync(
            AssetAdministrationShell shell,
            List<Submodel> submodels,
            string localRepositoryUrl,
            string localRegistryUrl,
            SecuritySetting securitySetting
        )
        {
            var jsonString = CreateShellDescriptorString(shell, submodels, localRepositoryUrl);

            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{localRegistryUrl}/shell-descriptors"
            );

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            request.Content = content;

            try
            {
                var result = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead
                );

                // Check if the response indicates success before reading content
                if (result.IsSuccessStatusCode)
                {
                    var resultContent = await result.Content.ReadAsStringAsync();
                    Console.WriteLine($"[PushNewToLocalRegistryAsync] Success: {resultContent}");
                }
                else
                {
                    // Try to read error content, but handle if it fails
                    string errorContent = "Unable to read error response";
                    try
                    {
                        errorContent = await result.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[PushNewToLocalRegistryAsync] Failed to read error content: {ex.Message}"
                        );
                    }

                    Console.WriteLine(
                        $"[PushNewToLocalRegistryAsync] Failed with status {result.StatusCode}: {errorContent}"
                    );
                    throw new HttpRequestException(
                        $"Registry push failed with status {result.StatusCode}: {errorContent}"
                    );
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"[PushNewToLocalRegistryAsync] HTTP Request Exception: {ex.Message}"
                );
                throw;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine($"[PushNewToLocalRegistryAsync] Request timeout: {ex.Message}");
                throw new TimeoutException("Request to registry timed out", ex);
            }
        }

        public async Task<bool> PushNewToLocalDiscoveryAsync(
            AssetAdministrationShell shell,
            string localDiscoveryUrl,
            SecuritySetting securitySetting
        )
        {
            // Discovery befüllen

            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            var discoveryUrl =
                localDiscoveryUrl.AppendSlash()
                + "lookup/shells/"
                + shell.Id.ToBase64UrlEncoded(Encoding.UTF8);
            try
            {
                var discoveryDeleteResponse = await client.DeleteAsync(
                    discoveryUrl,
                    CancellationToken.None
                );
            }
            catch (Exception e)
            {
                Console.WriteLine("Error deleting discovery: " + e.Message);
            }

            if (shell.AssetInformation.GlobalAssetId != null)
            {
                try
                {
                    var globalElem = new DiscoveryElement
                    {
                        name = "globalAssetId",
                        value = shell.AssetInformation.GlobalAssetId,
                    };
                    var discoveryJsonString = JsonConvert.SerializeObject(globalElem);
                    var discoveryResponse = await client.PostAsync(
                        discoveryUrl,
                        new StringContent(discoveryJsonString, Encoding.UTF8, "application/json"),
                        CancellationToken.None
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error saving discovery: " + e.Message);
                }
            }

            foreach (var id in shell.AssetInformation.SpecificAssetIds ?? [])
            {
                var globalElem = new DiscoveryElement { name = id.Name, value = id.Value };
                var discoveryJsonString = JsonConvert.SerializeObject(globalElem);
                try
                {
                    var discoveryResponse = await client.PostAsync(
                        discoveryUrl,
                        new StringContent(discoveryJsonString, Encoding.UTF8, "application/json"),
                        CancellationToken.None
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error saving discovery: " + e.Message);
                }
            }

            return true;
        }

        public async Task PushNewToLocalRepositoryAsync(
            AssetAdministrationShell shell,
            List<Submodel> submodels,
            string localRepositoryUrl,
            SecuritySetting securitySetting,
            byte[]? thumbnailData = null,
            string? thumbnailContentType = null,
            string? thumbnailFilename = null
        )
        {
            var jsonString = Jsonization.Serialize.ToJsonObject(shell).ToJsonString();

            using var client = HttpClientCreator.CreateHttpClient(securitySetting);

            // Push shell
            try
            {
                using var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{localRepositoryUrl.AppendSlash()}shells"
                );

                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                request.Content = content;
                var result = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead
                );

                if (result.IsSuccessStatusCode)
                {
                    var resultContent = await result.Content.ReadAsStringAsync();
                    Console.WriteLine(
                        $"[PushNewToLocalRepositoryAsync] Shell pushed: {resultContent}"
                    );
                }
                else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    string errorContent = "Shell already exists";
                    try
                    {
                        errorContent = await result.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[PushNewToLocalRepositoryAsync] Failed to read shell error: {ex.Message}"
                        );
                    }
                    Console.WriteLine(
                        $"[PushNewToLocalRepositoryAsync] Shell already exists in repository"
                    );
                    throw new InvalidOperationException(
                        "Der Asset Administration Shell Eintrag existiert bereits im Repository."
                    );
                }
                else
                {
                    string errorContent = "Unable to read error response";
                    try
                    {
                        errorContent = await result.Content.ReadAsStringAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[PushNewToLocalRepositoryAsync] Failed to read shell error: {ex.Message}"
                        );
                    }
                    Console.WriteLine(
                        $"[PushNewToLocalRepositoryAsync] Shell push failed with status {result.StatusCode}: {errorContent}"
                    );
                    throw new HttpRequestException(
                        $"Shell push failed with status {result.StatusCode}: {errorContent}"
                    );
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException(
                    "Request to repository timed out while pushing shell",
                    ex
                );
            }

            // Push submodels
            foreach (var sm in submodels)
            {
                try
                {
                    using var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        $"{localRepositoryUrl.AppendSlash()}submodels"
                    );
                    jsonString = Jsonization.Serialize.ToJsonObject(sm).ToJsonString();

                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    request.Content = content;
                    var result = await client.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead
                    );

                    if (result.IsSuccessStatusCode)
                    {
                        var resultContent = await result.Content.ReadAsStringAsync();
                        Console.WriteLine(
                            $"[PushNewToLocalRepositoryAsync] Submodel pushed: {resultContent}"
                        );
                    }
                    else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        string errorContent = "Submodel already exists";
                        try
                        {
                            errorContent = await result.Content.ReadAsStringAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"[PushNewToLocalRepositoryAsync] Failed to read submodel error: {ex.Message}"
                            );
                        }
                        Console.WriteLine(
                            $"[PushNewToLocalRepositoryAsync] Submodel already exists in repository"
                        );
                        throw new InvalidOperationException(
                            "Das Teil existiert bereits im Repository."
                        );
                    }
                    else
                    {
                        string errorContent = "Unable to read error response";
                        try
                        {
                            errorContent = await result.Content.ReadAsStringAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"[PushNewToLocalRepositoryAsync] Failed to read submodel error: {ex.Message}"
                            );
                        }
                        Console.WriteLine(
                            $"[PushNewToLocalRepositoryAsync] Submodel push failed with status {result.StatusCode}: {errorContent}"
                        );
                        throw new HttpRequestException(
                            $"Submodel push failed with status {result.StatusCode}: {errorContent}"
                        );
                    }
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                catch (HttpRequestException)
                {
                    throw;
                }
                catch (TaskCanceledException ex)
                {
                    throw new TimeoutException(
                        "Request to repository timed out while pushing submodel",
                        ex
                    );
                }
            }

            // Upload thumbnail if available
            if (
                thumbnailData != null
                && thumbnailData.Length > 0
                && !string.IsNullOrEmpty(thumbnailFilename)
            )
            {
                try
                {
                    var thumbnailUrl =
                        localRepositoryUrl.AppendSlash()
                        + "shells/"
                        + shell.Id.ToBase64UrlEncoded(Encoding.UTF8).AppendSlash()
                        + "asset-information/thumbnail?fileName="
                        + thumbnailFilename;

                    using var thumbnailRequest = new HttpRequestMessage(
                        HttpMethod.Put,
                        thumbnailUrl
                    );
                    using var thumbnailStream = new MemoryStream(thumbnailData);
                    var streamContent = new StreamContent(thumbnailStream);
                    streamContent.Headers.ContentType =
                        new System.Net.Http.Headers.MediaTypeHeaderValue(
                            thumbnailContentType ?? "image/png"
                        );

                    using var multipartContent = new MultipartFormDataContent
                    {
                        { streamContent, "file", thumbnailFilename },
                    };

                    thumbnailRequest.Content = multipartContent;
                    var thumbnailResult = await client.SendAsync(thumbnailRequest);

                    if (thumbnailResult.IsSuccessStatusCode)
                    {
                        Console.WriteLine(
                            $"[PushNewToLocalRepositoryAsync] Thumbnail uploaded successfully"
                        );
                    }
                    else
                    {
                        Console.WriteLine(
                            $"[PushNewToLocalRepositoryAsync] Thumbnail upload failed with status {thumbnailResult.StatusCode}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"[PushNewToLocalRepositoryAsync] Failed to upload thumbnail: {ex.Message}"
                    );
                    // Don't throw - thumbnail upload failure should not break the import
                }
            }
        }

        private string CreateShellDescriptorString(
            AssetAdministrationShell aas,
            List<Submodel> submodels,
            string localRegistryUrl
        )
        {
            var shellEndpoint = $"{localRegistryUrl}/shells/{aas.Id.ToBase64()}";

            var iEndpoint = new IO.Swagger.Model.Endpoint()
            {
                _Interface = "AssetAdministrationShell",
                ProtocolInformation = new ProtocolInformation() { Href = shellEndpoint },
            };

            var endpointList = new List<IO.Swagger.Model.Endpoint> { iEndpoint };

            var submodelDescriptors = new List<SubmodelDescriptor>();

            submodels.ForEach(sm =>
            {
                submodelDescriptors.Add(CreateSubmodelDescriptors(sm, shellEndpoint));
            });

            var aasDescriptor = new AssetAdministrationShellDescriptor()
            {
                Administration =
                    (AdministrativeInformation?)aas.Administration
                    ?? new AdministrativeInformation(),
                AssetKind = aas.AssetInformation.AssetKind,
                AssetType =
                    aas.AssetInformation.AssetType
                    ?? aas.AssetInformation.AssetKind.ToString() ?? string.Empty,
                Description = aas.Description ?? [],
                DisplayName = aas.DisplayName ?? [],
                Endpoints = endpointList,
                Extensions = aas.Extensions ?? null,
                GlobalAssetId = aas.AssetInformation.GlobalAssetId ?? string.Empty,
                Id = aas.Id,
                IdShort = aas.IdShort ?? string.Empty,
                SpecificAssetIds = aas.AssetInformation.SpecificAssetIds ?? [],
                SubmodelDescriptors = submodelDescriptors,
            };

            DefaultContractResolver contractResolver = new DefaultContractResolver
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

        private SubmodelDescriptor CreateSubmodelDescriptors(Submodel sm, string shellEndpoint)
        {
            var iEndpoint = new IO.Swagger.Model.Endpoint()
            {
                _Interface = "Submodel",
                ProtocolInformation = new ProtocolInformation()
                {
                    Href = $"{shellEndpoint}/submodels/{sm.Id.ToBase64()}",
                },
            };

            var endpointList = new List<IO.Swagger.Model.Endpoint> { iEndpoint };

            var submodelDescriptor = new SubmodelDescriptor()
            {
                Administration = sm.Administration ?? new AdministrativeInformation(),
                Description = sm.Description ?? null,
                DisplayName = sm.DisplayName ?? null,
                Endpoints = endpointList,
                Extensions = sm.Extensions ?? null,
                IdShort = sm.IdShort ?? string.Empty,
                Id = sm.Id,
                // Don't create empty Reference - registry requires at least 1 key if SemanticId is present
                SemanticId = sm.SemanticId,
                SupplementalSemanticId = sm.SupplementalSemanticIds ?? null,
            };

            return submodelDescriptor;
        }
    }

    public class DiscoveryElement
    {
        public string name { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }
}
