using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database.Model;
using AasDemoapp.Utils;
using AasDemoapp.Utils.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static AasCore.Aas3_0.Jsonization;

namespace AasDemoapp.Utils.Shells;

public class SaveShellSaver
{
    public static async Task<SaveShellResult> SaveSingle(AasUrls aasUrls, SecuritySetting securitySetting, string plainJson, List<ProvidedFile> providedFileStreams, CancellationToken cancellationToken)
    {
        var result = new SaveShellResult();

        var jsonNode = JsonNode.Parse(plainJson) ?? throw new System.Exception("Could not parse JSON");
        AasCore.Aas3_0.Environment? environment = null;
        // De-serialize from the JSON node
        environment = Deserialize.EnvironmentFrom(jsonNode);

        using var client = HttpClientCreator.CreateHttpClient(securitySetting);

        var aasId = string.Empty;

        foreach (var aas in environment.AssetAdministrationShells ?? [])
        {
            if (aasId == string.Empty)
            {
                aasId = aas.Id;
            }
            var editorDescriptor = new EditorDescriptor()
            {
                AasDescriptorEntry = new EditorDescriptorEntry()
                {
                    Endpoint = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + aas.Id.ToBase64UrlEncoded(Encoding.UTF8),
                    NewId = aas.Id,
                    OldId = aas.Id,
                    IdShort = aas.IdShort ?? "",
                }
            };
            var url = aasUrls.AasRepositoryUrl.AppendSlash() + "shells";

            var aasJsonString = BasyxSerializer.Serialize(aas);
            var response = await client.PostAsync(url, new StringContent(aasJsonString, Encoding.UTF8, "application/json"), cancellationToken);
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            foreach (var smRef in aas.Submodels ?? [])
            {
                var id = smRef.Keys.FirstOrDefault()?.Value ?? string.Empty;

                var sm = GetSubmodelFromEnv(environment, id);
                if (sm == null) continue;

                var smUrl = aasUrls.SubmodelRepositoryUrl.AppendSlash() + "submodels/" + id.ToBase64UrlEncoded(Encoding.UTF8);
                var smJsonString = BasyxSerializer.Serialize(sm);
                var smResponse = await client.PutAsync(smUrl, new StringContent(smJsonString, Encoding.UTF8, "application/json"), cancellationToken);
                if (smResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    // POST dann ohne ID ... 
                    // für den AASX Server hängen wir noch die aasId an ...
                    smUrl = aasUrls.SubmodelRepositoryUrl.AppendSlash() + "submodels?aasIdentifier=" + aas.Id.ToBase64UrlEncoded(Encoding.UTF8);
                    smResponse = await client.PostAsync(smUrl, new StringContent(smJsonString, Encoding.UTF8, "application/json"), cancellationToken);
                    if (!smResponse.IsSuccessStatusCode)
                    {
                        // throw new Exception($"Request to {smUrl} failed with status code {smResponse.StatusCode}");
                        Console.WriteLine("Error saving submodel: " + smResponse.StatusCode);
                    }
                }
                else if (!smResponse.IsSuccessStatusCode)
                {
                    // throw new Exception($"Request to {smUrl} failed with status code {smResponse.StatusCode}");
                    Console.WriteLine("Error saving submodel: " + smResponse.StatusCode);
                }
                var smDescriptorEntry = new EditorDescriptorEntry()
                {
                    Endpoint = aasUrls.SubmodelRepositoryUrl.AppendSlash() + "submodels/" + sm.Id.ToBase64UrlEncoded(Encoding.UTF8),
                    NewId = sm.Id,
                    OldId = id,
                    IdShort = sm.IdShort ?? "",
                };
                editorDescriptor.SubmodelDescriptorEntries.Add(smDescriptorEntry);
            }

            await DiscoveryUpdater.UpdateDiscoveryAsync(aasUrls.DiscoveryUrl, (AssetAdministrationShell)aas, cancellationToken, client);
            await RegistryUpdater.UpdateRegistryAsync(aasUrls, environment, cancellationToken, client, editorDescriptor);
        }

        var aasFiles = FilesFromAasResolver.GetAllAasFiles(environment, aasUrls.SubmodelRepositoryUrl.AppendSlash(), aasUrls.AasRepositoryUrl.AppendSlash());

        if (providedFileStreams != null)
        {
            foreach (var providedFile in providedFileStreams)
            {
                // do something with the file
                // datei in der aasFiles-Liste finden und an den endpunkt schicken
                var aasFilesMatched = aasFiles.Where(f => f.Filename.EndsWith(providedFile.Filename)).ToList();

                foreach (var aasFile in aasFilesMatched)
                {
                    if (aasFile == null || environment.AssetAdministrationShells == null || providedFile.Stream == null || !providedFile.Stream.CanRead || providedFile.Stream.Length <= 0) continue;
                    // datei zum endpunkt schicken

                    if (aasFile.IsThumbnail)
                    {
                        var thumbUrl = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + environment.AssetAdministrationShells[0].Id.ToBase64UrlEncoded(Encoding.UTF8).AppendSlash() + "asset-information/thumbnail?fileName=" + providedFile.Filename;
                        using var httpRequestThumb = new HttpRequestMessage(HttpMethod.Put, thumbUrl);
                        var streamContentThumb = new StreamContent(providedFile.Stream);
                        var contentType = !string.IsNullOrEmpty(aasFile.ContentType) ? aasFile.ContentType : "application/octet-stream";
                        streamContentThumb.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                        using var thumbContent = new MultipartFormDataContent
                        {
                            { streamContentThumb, "file", providedFile.Filename },
                        };

                        httpRequestThumb.Content = thumbContent;

                        var thumbResponse = await client.SendAsync(httpRequestThumb);
                        Console.WriteLine("ThumbResponse: " + thumbResponse.StatusCode);
                    }
                    else
                    {
                        var fileUrl = aasFile.Endpoint + "?fileName=" + providedFile.Filename;
                        using var httpRequest = new HttpRequestMessage(HttpMethod.Put, fileUrl);
                        var streamContent = new StreamContent(providedFile.Stream);
                        var contentType = !string.IsNullOrEmpty(aasFile.ContentType) ? aasFile.ContentType : "application/octet-stream";

                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                        using var content = new MultipartFormDataContent
                    {
                        { streamContent, "file", providedFile.Filename },
                    };

                        httpRequest.Content = content;

                        var fileResponse = await client.SendAsync(httpRequest);
                        if (!fileResponse.IsSuccessStatusCode)
                        {
                            // throw new Exception($"Request to {fileUrl} failed with status code {fileResponse.StatusCode}");
                            Console.WriteLine("Error saving file: " + fileResponse.StatusCode);
                        }
                        else
                        {

                            // versuchen den neuen Namen zu bekommen!
                            var fileUrlLoad = aasFile.Endpoint.Replace("/attachment", "/$value");
                            var fileResponseLoad = await client.GetAsync(fileUrlLoad, cancellationToken);
                            var contLoad = fileResponseLoad.Content.ReadAsStringAsync(cancellationToken);

                            var contJson = JsonConvert.DeserializeObject<JObject>(contLoad.Result);
                            var filenameNew = contJson?["value"]?.ToString() ?? "newFile";

                            // Liste bauen mit alt/neu
                            if (!result.OldNewFileNames.ContainsKey(providedFile.Filename))
                            {
                                result.OldNewFileNames.Add(providedFile.Filename, filenameNew);
                            }
                        }
                    }
                }
            }
        }

        foreach (var cd in environment.ConceptDescriptions ?? [])
        {
            var cdUrl = aasUrls.ConceptDescriptionRepositoryUrl.AppendSlash() + "concept-descriptions".AppendSlash() + cd.Id.ToBase64UrlEncoded(Encoding.UTF8);
            var cdJsonString = BasyxSerializer.Serialize(cd);
            var cdResponse = await client.PutAsync(cdUrl, new StringContent(cdJsonString, Encoding.UTF8, "application/json"), cancellationToken);
            if (cdResponse.StatusCode == HttpStatusCode.NotFound)
            {
                // POST dann ohne ID ...
                cdUrl = aasUrls.ConceptDescriptionRepositoryUrl.AppendSlash() + "concept-descriptions";
                cdResponse = await client.PostAsync(cdUrl, new StringContent(cdJsonString, Encoding.UTF8, "application/json"), cancellationToken);
                if (!cdResponse.IsSuccessStatusCode)
                {
                    // throw new Exception($"Request to {smUrl} failed with status code {smResponse.StatusCode}");
                    Console.WriteLine("Error saving ConceptDescription: " + cdResponse.StatusCode);
                }
            }
        }
        result.AasId = aasId;
        result.Environment = environment;
        return result;
    }


    public static async Task<SaveShellResult> UpdateSingle(AasUrls aasUrls, SecuritySetting securitySetting, string plainJson, List<ProvidedFile> providedFileStreams, CancellationToken cancellationToken, EditorDescriptor editorDescriptor, List<string>? deletedSubmodels = null)
    {
        var jsonNode = JsonNode.Parse(plainJson) ?? throw new System.Exception("Could not parse JSON");
        // De-serialize from the JSON node
        AasCore.Aas3_0.Environment? environment = Deserialize.EnvironmentFrom(jsonNode);

        return await UpdateSingle(aasUrls, securitySetting, environment, providedFileStreams, cancellationToken, editorDescriptor, deletedSubmodels);
    }


    public static async Task<SaveShellResult> UpdateSingle(AasUrls aasUrls, SecuritySetting securitySetting, AasCore.Aas3_0.Environment environment, List<ProvidedFile> providedFileStreams, CancellationToken cancellationToken, EditorDescriptor editorDescriptor, List<string>? deletedSubmodels = null)
    {
        var result = new SaveShellResult();

        using var client = HttpClientCreator.CreateHttpClient(securitySetting);
        var aasId = string.Empty;

        foreach (var aas in environment.AssetAdministrationShells ?? [])
        {
            if (aasId == string.Empty)
            {
                aasId = aas.Id;
            }

            var url = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + aas.Id.ToBase64UrlEncoded(Encoding.UTF8);
            var existingAasResponse = await client.GetAsync(url, cancellationToken);

            if (!existingAasResponse.IsSuccessStatusCode)
            {
                throw new System.Exception($"Request to {url} failed with status code {existingAasResponse.StatusCode}");
            }

            var responseRegistry = await existingAasResponse.Content.ReadAsStringAsync();

            JObject? res = JsonConvert.DeserializeObject<JObject>(responseRegistry);

            AssetAdministrationShell? existingAas = null;
            if (res != null)
            {
                var jsonNode = JsonNode.Parse(res.ToString());
                if (jsonNode == null) throw new System.Exception("Could not parse JSON");

                existingAas = Deserialize.AssetAdministrationShellFrom(jsonNode);
            }

            var aasJsonString = BasyxSerializer.Serialize(aas);
            var response = await client.PutAsync(url, new StringContent(aasJsonString, Encoding.UTF8, "application/json"), cancellationToken);
            var content = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            foreach (var smRef in aas.Submodels ?? [])
            {
                var id = smRef.Keys.FirstOrDefault()?.Value ?? string.Empty;

                var sm = GetSubmodelFromEnv(environment, id);
                if (sm == null) continue;

                var smUrl = aasUrls.SubmodelRepositoryUrl.AppendSlash() + "submodels/" + id.ToBase64UrlEncoded(Encoding.UTF8);
                var smJsonString = BasyxSerializer.Serialize(sm);
                var smResponse = await client.PutAsync(smUrl, new StringContent(smJsonString, Encoding.UTF8, "application/json"), cancellationToken);
                if (smResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    // POST dann ohne ID ...
                    // für den AASX Server hängen wir noch die aasId an ...
                    smUrl = aasUrls.SubmodelRepositoryUrl.AppendSlash() + "submodels?aasIdentifier=" + aas.Id.ToBase64UrlEncoded(Encoding.UTF8);
                    smResponse = await client.PostAsync(smUrl, new StringContent(smJsonString, Encoding.UTF8, "application/json"), cancellationToken);
                    if (!smResponse.IsSuccessStatusCode)
                    {
                        // throw new Exception($"Request to {smUrl} failed with status code {smResponse.StatusCode}");
                        Console.WriteLine("Error saving submodel: " + smResponse.StatusCode);
                    }
                }
                else if (!smResponse.IsSuccessStatusCode)
                {
                    // throw new Exception($"Request to {smUrl} failed with status code {smResponse.StatusCode}");
                    Console.WriteLine("Error saving submodel: " + smResponse.StatusCode);
                }
            }

            foreach (var smRef in existingAas?.Submodels ?? [])
            {
                var id = smRef.Keys.FirstOrDefault()?.Value ?? string.Empty;
                if (!HasSubmodelReference(aas, id) && deletedSubmodels != null && !deletedSubmodels.Contains(id))
                {
                    // Wenn explizites Löschen angegeben, dann Löschen, sonst stehenlassen!
                    var smUrl = aasUrls.SubmodelRepositoryUrl.AppendSlash() + "submodels/" + id.ToBase64UrlEncoded(Encoding.UTF8);
                    var smResponse = await client.DeleteAsync(smUrl, cancellationToken);
                    if (!smResponse.IsSuccessStatusCode)
                    {
                        // throw new Exception($"Request to {smUrl} failed with status code {smResponse.StatusCode}");
                        Console.WriteLine("Error deleting submodel: " + smResponse.StatusCode);
                    }
                    Console.WriteLine("Submodel " + id + " is not referenced anymore. Keeping it in the repository.");
                }
            }

            await DiscoveryUpdater.UpdateDiscoveryAsync(aasUrls.DiscoveryUrl, (AssetAdministrationShell)aas, cancellationToken, client);
            await RegistryUpdater.UpdateRegistryAsync(aasUrls, environment, cancellationToken, client, editorDescriptor);
        }

        var aasFiles = FilesFromAasResolver.GetAllAasFiles(environment, aasUrls.SubmodelRepositoryUrl.AppendSlash(), aasUrls.AasRepositoryUrl.AppendSlash());

        if (providedFileStreams != null)
        {
            foreach (var providedFile in providedFileStreams.Where(f => f.Type != ProvidedFileType.Deleted))
            {
                if (environment?.AssetAdministrationShells?[0] == null) continue;
                if (providedFile.Type == ProvidedFileType.Thumbnail)
                {
                    var thumbUrl = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + environment.AssetAdministrationShells[0].Id.ToBase64UrlEncoded(Encoding.UTF8).AppendSlash() + "asset-information/thumbnail?fileName=" + providedFile.Filename;
                    using var httpRequestThumb = new HttpRequestMessage(HttpMethod.Put, thumbUrl);
                    var streamContentThumb = new StreamContent(providedFile.Stream);
                    streamContentThumb.Headers.ContentType = new MediaTypeHeaderValue(providedFile.ContentType);
                    using var thumbContent = new MultipartFormDataContent
                    {
                        { streamContentThumb, "file", providedFile.Filename },
                    };

                    httpRequestThumb.Content = thumbContent;

                    var thumbResponse = await client.SendAsync(httpRequestThumb);
                    Console.WriteLine("ThumbResponse: " + thumbResponse.StatusCode);
                }
                else
                {
                    // do something with the file
                    // datei in der aasFiles-Liste finden und an den endpunkt schicken
                    var aasFilesMatched = aasFiles.Where(f => f.Filename.EndsWith(providedFile.Filename)).ToList();

                    foreach (var aasFile in aasFilesMatched)
                    {
                        try
                        {

                            if (aasFile == null || environment.AssetAdministrationShells == null) continue;
                            // datei zum endpunkt schicken

                            var fileUrl = aasFile.Endpoint + "?fileName=" + providedFile.Filename;
                            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, fileUrl);
                            var streamContent = new StreamContent(providedFile.Stream);
                            streamContent.Headers.ContentType = new MediaTypeHeaderValue(providedFile.ContentType);
                            using var content = new MultipartFormDataContent
                        {
                            { streamContent, "file", providedFile.Filename },
                        };

                            httpRequest.Content = content;

                            var fileResponse = await client.SendAsync(httpRequest);
                            if (!fileResponse.IsSuccessStatusCode)
                            {
                                // throw new Exception($"Request to {fileUrl} failed with status code {fileResponse.StatusCode}");
                                Console.WriteLine("Error saving file: " + fileResponse.StatusCode);
                            }
                            else
                            {
                                // versuchen den neuen Namen zu bekommen!
                                var fileUrlLoad = aasFile.Endpoint.Replace("/attachment", "/$value");
                                var fileResponseLoad = await client.GetAsync(fileUrlLoad, cancellationToken);
                                var contLoad = fileResponseLoad.Content.ReadAsStringAsync(cancellationToken);

                                var contJson = JsonConvert.DeserializeObject<JObject>(contLoad.Result);
                                var filenameNew = contJson?["value"]?.ToString() ?? "newFile";

                                // Liste bauen mit alt/neu
                                result.OldNewFileNames.Add(providedFile.Filename, filenameNew);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine("Error saving file: " + ex.Message);
                        }
                    }
                }
            }
        }

        foreach (var cd in environment?.ConceptDescriptions ?? [])
        {
            var cdUrl = aasUrls.ConceptDescriptionRepositoryUrl.AppendSlash() + "concept-descriptions".AppendSlash() + cd.Id.ToBase64UrlEncoded(Encoding.UTF8);
            var cdJsonString = BasyxSerializer.Serialize(cd);
            var cdResponse = await client.PutAsync(cdUrl, new StringContent(cdJsonString, Encoding.UTF8, "application/json"), cancellationToken);
            if (cdResponse.StatusCode == HttpStatusCode.NotFound)
            {
                // POST dann ohne ID ...
                cdUrl = aasUrls.ConceptDescriptionRepositoryUrl.AppendSlash() + "concept-descriptions";
                cdResponse = await client.PostAsync(cdUrl, new StringContent(cdJsonString, Encoding.UTF8, "application/json"), cancellationToken);
                if (!cdResponse.IsSuccessStatusCode)
                {
                    // throw new Exception($"Request to {smUrl} failed with status code {smResponse.StatusCode}");
                    Console.WriteLine("Error saving ConceptDescription: " + cdResponse.StatusCode);
                }
            }
        }
        result.AasId = aasId;

        return result;
    }

    private static bool HasSubmodelReference(IAssetAdministrationShell aas, string id)
    {
        return aas.Submodels?.Any(sm => sm.Keys.FirstOrDefault()?.Value == id) ?? false;
    }

    private static Submodel? GetSubmodelFromEnv(AasCore.Aas3_0.Environment environment, string id)
    {
        return (Submodel?)environment.Submodels?.FirstOrDefault(sm => sm.Id == id);
    }
}