using System.Text;
using System.Text.Json.Nodes;
using AasCore.Aas3_0;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using AasDemoapp.Utils.ConceptDescriptions;
using AasDemoapp.Database.Model;

namespace AasDemoapp.Utils.Shells;

public class LoadShellResult
{
    public AasCore.Aas3_0.Environment? Environment { get; set; }
    public List<ConceptDescriptionMetadata> ConceptDescriptionMetadata { get; set; } = [];
    public EditorDescriptor EditorDescriptor { get; set; } = new();
}

public class EditorDescriptor
{
    public EditorDescriptorEntry AasDescriptorEntry { get; set; } = new EditorDescriptorEntry();
    public List<EditorDescriptorEntry> SubmodelDescriptorEntries { get; set; } = [];
}

public class EditorDescriptorEntry
{
    public string OldId { get; set; } = string.Empty;
    public string NewId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string IdShort { get; set; } = string.Empty;
}

public static class ShellLoader
{
    public static async Task<LoadShellResult> LoadAsync(AasUrls aasUrls, SecuritySetting securitySetting, string aasIdentifier, CancellationToken cancellationToken)
    {
        var result = new LoadShellResult();

        // jetzt den BasyxServer anfragen
        using var client = HttpClientCreator.CreateHttpClient(securitySetting);

        // zunächst in aas-Registry die aas-url laden, falls das nicht klappt, direkten zugriff aufs repo versuchen
        var aasUrl = string.Empty;
        try
        {
            aasUrl = await GetAasUrl(aasUrls, securitySetting, aasIdentifier, cancellationToken);
        }
        catch (Exception e)
        {
            aasUrl = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + aasIdentifier.ToBase64UrlEncoded(Encoding.UTF8);
        }

        HttpResponseMessage response = await client.GetAsync(aasUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Request to {aasUrl} failed with status code {response.StatusCode}");
        }

        result.EditorDescriptor.AasDescriptorEntry.OldId = aasIdentifier;
        result.EditorDescriptor.AasDescriptorEntry.NewId = aasIdentifier;
        result.EditorDescriptor.AasDescriptorEntry.Endpoint = aasUrl;

        var responseBody = await response.Content.ReadAsStringAsync();

        JObject? res = JsonConvert.DeserializeObject<JObject>(responseBody);

        if (res != null)
        {

            var jsonNode = JsonNode.Parse(res.ToString());
            if (jsonNode == null) throw new Exception("Could not parse JSON");
            var aas = Jsonization.Deserialize.AssetAdministrationShellFrom(jsonNode);

            result.EditorDescriptor.AasDescriptorEntry.IdShort = aas.IdShort ?? string.Empty;

            var environment = new AasCore.Aas3_0.Environment
            {
                AssetAdministrationShells = [aas]
            };

            // submodels ladem
            environment.Submodels = [];
            foreach (var smRef in aas.Submodels ?? [])
            {
                // submodelUrl aus SM-Registry laden
                var submodelUrl = await GetSmUrl(aasUrls, securitySetting, aasIdentifier, smRef.Keys[0].Value, cancellationToken);
                // var submodelUrl = aasInfrastructureSettings.SubmodelRepositoryUrl.AppendSlash() + "submodels/" + smRef.Keys[0].Value.ToBase64UrlEncoded(Encoding.UTF8);
                HttpResponseMessage submodelResponse = await client.GetAsync(submodelUrl, cancellationToken);

                var smDescriptorEntry = new EditorDescriptorEntry();
                smDescriptorEntry.Endpoint = submodelUrl;
                result.EditorDescriptor.SubmodelDescriptorEntries.Add(smDescriptorEntry);

                if (!submodelResponse.IsSuccessStatusCode)
                {
                    //throw new Exception($"Request to {submodelUrl} failed with status code {submodelResponse.StatusCode}");
                    // nicht gefunden... Schade
                    Console.WriteLine($"Request to {submodelUrl} failed with status code {submodelResponse.StatusCode}");
                }
                else
                {
                    var smResponseContent = await submodelResponse.Content.ReadAsStringAsync();
                    smResponseContent = EmbeddedDataspecFixUtil.FixEmbeddedDataspec(smResponseContent);

                    JObject? submodelRes = JsonConvert.DeserializeObject<JObject>(smResponseContent);

                    if (submodelRes != null)
                    {
                        var submodelJsonNode = JsonNode.Parse(submodelRes.ToString());
                        if (submodelJsonNode != null)
                        {
                            try
                            {
                                var submodel = Jsonization.Deserialize.SubmodelFrom(submodelJsonNode);
                                environment.Submodels.Add(submodel);
                                smDescriptorEntry.OldId = submodel.Id;
                                smDescriptorEntry.NewId = submodel.Id;
                                smDescriptorEntry.IdShort = submodel.IdShort ?? string.Empty;
                            }
                            catch (Exception e)
                            {
                                // TODO: Hier müssen wir etwas tun, damit das Frontend dem Nutzer das Problem anzeigen kann
                                smDescriptorEntry.OldId = smRef.Keys[0].Value;
                                // nicht gefunden... Schade
                                Console.WriteLine(e);
                            }
                        }
                    }
                }
            }
            List<ConceptDescriptionMetadata> conceptDescriptionMetadata = [];

            if (aasUrls.ConceptDescriptionRepositoryUrl != null)
            {
                conceptDescriptionMetadata = CDsFromAasResolver.GetAllConceptDescriptions(environment, aasUrls.ConceptDescriptionRepositoryUrl.AppendSlash());
                try
                {

                    conceptDescriptionMetadata.ForEach(cdDescriptor =>
                    {
                        var cd = CdLoader.GetCdFromDescriptor(cdDescriptor, aasUrls.ConceptDescriptionRepositoryUrl.AppendSlash(), client);
                        if (cd != null)
                        {
                            if (environment.ConceptDescriptions == null) environment.ConceptDescriptions = [];
                            if (!environment.ConceptDescriptions.Any(c => cd.Id == c.Id))
                            {
                                environment.ConceptDescriptions.Add(cd);
                            }
                        }
                    });
                    environment.ConceptDescriptions = environment.ConceptDescriptions?.OrderBy(cd => cd.IdShort).ToList();
                }
                catch (Exception e)
                {
                    // ignor
                    Console.WriteLine(e);
                }
            }
            result.Environment = environment;
            result.ConceptDescriptionMetadata = conceptDescriptionMetadata;
        }
        return result;
    }

    public static async Task<AssetAdministrationShell?> LoadShellOnly(AasUrls aasUrls, SecuritySetting securitySetting, string aasIdentifier, CancellationToken cancellationToken)
    {
        using var client = HttpClientCreator.CreateHttpClient(securitySetting);

        // zunächst in aas-Registry die aas-url laden, falls das nicht klappt, direkten zugriff aufs repo versuchen
        var url = string.Empty;
        try
        {
            url = await GetAasUrl(aasUrls, securitySetting, aasIdentifier, cancellationToken);
        }
        catch (Exception e)
        {
            url = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + aasIdentifier.ToBase64UrlEncoded(Encoding.UTF8);
        }


        HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Request to {url} failed with status code {response.StatusCode}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        JObject? res = JsonConvert.DeserializeObject<JObject>(responseBody);

        if (res != null)
        {

            var jsonNode = JsonNode.Parse(res.ToString());
            if (jsonNode == null) throw new Exception("Could not parse JSON");
            var aas = Jsonization.Deserialize.AssetAdministrationShellFrom(jsonNode);

            return aas;
        }
        return null;
    }

    public static async Task<AssetInformation?> LoadAssetInformationOnly(AasUrls aasUrls, SecuritySetting securitySetting, string aasIdentifier, CancellationToken cancellationToken)
    {
        using var client = HttpClientCreator.CreateHttpClient(securitySetting);

        // zunächst in aas-Registry die aas-url laden, falls das nicht klappt, direkten zugriff aufs repo versuchen
        var url = string.Empty;
        try
        {
            url = await GetAasUrl(aasUrls, securitySetting, aasIdentifier, cancellationToken) + '/' + "asset-information";
        }
        catch (Exception e)
        {
            url = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + aasIdentifier.ToBase64UrlEncoded(Encoding.UTF8) + '/' + "asset-information";
        }


        HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Request to {url} failed with status code {response.StatusCode}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();

        JObject? res = JsonConvert.DeserializeObject<JObject>(responseBody);

        if (res != null)
        {
            var jsonNode = JsonNode.Parse(res.ToString());
            if (jsonNode == null) throw new Exception("Could not parse JSON");
            var aas = Jsonization.Deserialize.AssetInformationFrom(jsonNode);

            return aas;
        }
        return null;
    }

    public static async Task<bool> CheckIfExists(AasUrls aasUrls, SecuritySetting securitySetting, string aasIdentifier, CancellationToken cancellationToken)
    {
        using var client = HttpClientCreator.CreateHttpClient(securitySetting);

        var url = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + aasIdentifier.ToBase64UrlEncoded(Encoding.UTF8);

        HttpResponseMessage response = await client.GetAsync(url, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
        else if (response.IsSuccessStatusCode)
        {
            return true;
        }

        throw new Exception($"Request to {url} failed with status code {response.StatusCode}");

    }

    public static async Task<string> GetAasUrl(AasUrls aasUrls, SecuritySetting securitySetting, string aasIdentifier, CancellationToken cancellationToken)
    {
        var result = string.Empty;

        if (!string.IsNullOrWhiteSpace(aasUrls.AasRegistryUrl))
        {
            var url = aasUrls.AasRegistryUrl.AppendSlash() + "shell-descriptors/" + aasIdentifier.ToBase64UrlEncoded(Encoding.UTF8);

            using var client = HttpClientCreator.CreateHttpClient(securitySetting);

            HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode && !responseBody.StartsWith("<"))
            {
                JObject? res = JsonConvert.DeserializeObject<JObject>(responseBody);

                if (res != null)
                {
                    var endpoints = res.Value<JArray>("endpoints") ?? [];
                    endpoints.ToList().ForEach(endpoint =>
                    {
                        var protocolInfo = endpoint.Value<JObject>("protocolInformation") ?? new JObject();

                        result = protocolInfo.Value<string>("href") ?? string.Empty;
                    });
                }
            }
            else
            {
                // prüfen, ob es überhaupt eine registry gibt. Falls nicht, direkt aufs repo gehen
                var urlReg = aasUrls.SubmodelRegistryUrl.AppendSlash() + "shell-descriptors?limit=1";

                HttpResponseMessage responseReg = await client.GetAsync(url, cancellationToken);
                var responseBodyReg = await responseReg.Content.ReadAsStringAsync();
                if (responseReg.StatusCode == HttpStatusCode.NotFound || responseBodyReg.StartsWith("<"))
                {
                    result = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + aasIdentifier.ToBase64UrlEncoded(Encoding.UTF8);
                }
                else
                {
                    throw new Exception($"Request to {urlReg} failed with status code {responseReg.StatusCode}");
                }
            }
        }
        else
        {
            result = aasUrls.AasRepositoryUrl.AppendSlash() + "shells/" + aasIdentifier.ToBase64UrlEncoded(Encoding.UTF8);
        }

        return result;
    }


    public static async Task<string> GetSmUrl(AasUrls aasUrls, SecuritySetting securitySetting, string aasIdentifier, string smIdentifier, CancellationToken cancellationToken)
    {
        var result = string.Empty;
        using var client = HttpClientCreator.CreateHttpClient(securitySetting);
        if (!string.IsNullOrWhiteSpace(aasUrls.AasRegistryUrl))
        {
            var url = aasUrls.AasRegistryUrl.AppendSlash() + "shell-descriptors/" + aasIdentifier.ToBase64UrlEncoded(Encoding.UTF8);


            HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode && !responseBody.StartsWith("<"))
            {
                JObject? res = JsonConvert.DeserializeObject<JObject>(responseBody);

                if (res != null)
                {
                    var smDescriptors = res.Value<JArray>("submodelDescriptors") ?? [];
                    foreach (var smDescriptor in smDescriptors)
                    {
                        var smId = smDescriptor.Value<string>("id") ?? string.Empty;
                        if (smId == smIdentifier)
                        {
                            var endpoints = smDescriptor.Value<JArray>("endpoints") ?? [];
                            endpoints.ToList().ForEach(endpoint =>
                            {
                                var protocolInfo = endpoint.Value<JObject>("protocolInformation") ?? new JObject();
                                result = protocolInfo.Value<string>("href") ?? string.Empty;
                            });
                        }
                    }
                }
            }
        }

        if (string.IsNullOrWhiteSpace(result))
        {

            if (!string.IsNullOrWhiteSpace(aasUrls.SubmodelRegistryUrl))
            {
                var url = aasUrls.SubmodelRegistryUrl.AppendSlash() + "submodel-descriptors/" + smIdentifier.ToBase64UrlEncoded(Encoding.UTF8);

                HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

                var responseBody = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode && !responseBody.StartsWith("<"))
                {
                    JObject? res = JsonConvert.DeserializeObject<JObject>(responseBody);
                    if (res != null)
                    {

                        var endpoints = res.Value<JArray>("endpoints") ?? [];
                        endpoints.ToList().ForEach(endpoint =>
                        {
                            var protocolInfo = endpoint.Value<JObject>("protocolInformation") ?? new JObject();

                            result = protocolInfo.Value<string>("href") ?? string.Empty;
                        });
                    }
                }
                else
                {
                    // prüfen, ob es überhaupt eine registry gibt. Falls nicht, direkt aufs repo gehen
                    var urlReg = aasUrls.SubmodelRegistryUrl.AppendSlash() + "submodel-descriptors?limit=1";

                    HttpResponseMessage responseReg = await client.GetAsync(url, cancellationToken);
                    var responseBodyReg = await responseReg.Content.ReadAsStringAsync();
                    if (responseReg.StatusCode == HttpStatusCode.NotFound || responseBodyReg.StartsWith("<"))
                    {
                        result = aasUrls.SubmodelRepositoryUrl.AppendSlash() + "submodels/" + smIdentifier.ToBase64UrlEncoded(Encoding.UTF8);
                    }
                    else
                    {
                        throw new Exception($"Request to {urlReg} failed with status code {responseReg.StatusCode}");
                    }
                }
            }
            else
            {
                result = aasUrls.SubmodelRepositoryUrl.AppendSlash() + "submodels/" + smIdentifier.ToBase64UrlEncoded(Encoding.UTF8);
            }
        }
        // replace double slashes in url, but not in protocol
        result = result.Replace("//", "/").Replace(":/", "://");
        return result;
    }
}
