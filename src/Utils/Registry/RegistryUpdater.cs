using System.Net;
using System.Text;
using AasCore.Aas3_0;
using AasDemoapp.Utils.Registry;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AasDemoapp.Utils.Shells;

public class RegistryUpdater
{
    private static ILogger<RegistryUpdater>? _logger;

    static RegistryUpdater()
    {
        // Dienstanbieter für Dependency Injection abrufen
        var serviceProvider = new ServiceCollection()
            .AddLogging(configure =>
            {
                configure.AddConsole();
                configure.AddDebug();
            })
            .BuildServiceProvider();

        // Logger initialisieren
        _logger = serviceProvider.GetService<ILogger<RegistryUpdater>>();
    }

    public static string SerializeAasDesc(AssetAdministrationShellDescriptor aasDescriptor)
    {
        DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy(),
        };
        var serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented,
            Converters = [new StringEnumConverter()],
            NullValueHandling = NullValueHandling.Ignore,
        };

        return JsonConvert.SerializeObject(aasDescriptor, serializerSettings);
    }

    public static string SerializeSmDesc(SubmodelDescriptor smDescriptor)
    {
        DefaultContractResolver contractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy(),
        };
        var serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented,
            Converters = [new StringEnumConverter()],
            NullValueHandling = NullValueHandling.Ignore,
        };

        return JsonConvert.SerializeObject(smDescriptor, serializerSettings);
    }

    public static async Task UpdateRegistryAsync(
        AasUrls aasUrls,
        AasCore.Aas3_0.Environment environment,
        CancellationToken cancellationToken,
        HttpClient client,
        EditorDescriptor editorDescriptor
    )
    {
        if (
            environment.AssetAdministrationShells == null
            || environment.AssetAdministrationShells.Count == 0
        )
            return;
        foreach (var aas in environment.AssetAdministrationShells)
        {
            // schauen ob vorhanden
            var aasRegistryUrl =
                aasUrls.AasRegistryUrl.AppendSlash()
                + "shell-descriptors/"
                + aas.Id.ToBase64UrlEncoded(Encoding.UTF8);
            _logger?.LogDebug("AAS registry URL: {aasRegistryUrl}", aasRegistryUrl);
            List<Submodel> submodelsToAdd = [];
            aas.Submodels?.ForEach(sm =>
            {
                var submodel = environment.Submodels?.Find(s =>
                    s.Id == sm.Keys.FirstOrDefault()?.Value
                );
                if (submodel != null)
                {
                    submodelsToAdd.Add((Submodel)submodel);
                }
            });
            // aas Registry updaten
            try
            {
                var registryResponse = await client.GetAsync(aasRegistryUrl, cancellationToken);
                if (registryResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    // ist also schonmal nicht da -> anlegen

                    var aasDescriptor = DescriptorCreator.CreateDescriptor(
                        aas,
                        environment.Submodels ?? [],
                        aasUrls
                    );

                    var jsonString = SerializeAasDesc(aasDescriptor);
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(
                        aasUrls.AasRegistryUrl.AppendSlash() + "shell-descriptors",
                        content,
                        cancellationToken
                    );
                    if (!response.IsSuccessStatusCode)
                    {
                        var c = await response.Content.ReadAsStringAsync();
                        _logger?.LogError(
                            "Error updating aas registry (create new descriptor): {ReasonPhrase} {StatusCode} {content}",
                            response.ReasonPhrase,
                            response.StatusCode,
                            c
                        );
                    }
                }
                else if (registryResponse.IsSuccessStatusCode)
                {
                    var descriptorString = await registryResponse.Content.ReadAsStringAsync();
                    var descriptor =
                        JsonConvert.DeserializeObject<AssetAdministrationShellDescriptor>(
                            descriptorString
                        );
                    if (descriptor == null)
                    {
                        _logger?.LogError(
                            "Error updating aas registry (update descriptor): Descriptor could not be deserialized"
                        );
                        return;
                    }
                    // ist da -> updaten
                    _logger?.LogInformation("Updating existing AAS in registry");

                    DescriptorUpdater.UpdateAasDescriptor(
                        descriptor,
                        editorDescriptor,
                        (AssetAdministrationShell)aas,
                        submodelsToAdd
                    );
                    var jsonString = SerializeAasDesc(descriptor);
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = await client.PutAsync(
                        aasUrls.AasRegistryUrl.AppendSlash()
                            + "shell-descriptors/"
                            + aas.Id.ToBase64UrlEncoded(Encoding.UTF8),
                        content,
                        cancellationToken
                    );
                    if (!response.IsSuccessStatusCode)
                    {
                        var c = await response.Content.ReadAsStringAsync();
                        _logger?.LogError(
                            "Error updating aas registry (update descriptor): {ReasonPhrase} {StatusCode} {content}",
                            response.ReasonPhrase,
                            response.StatusCode,
                            c
                        );
                    }
                }
                else
                {
                    _logger?.LogError(
                        "Error updating aas registry: {ReasonPhrase} {StatusCode}",
                        registryResponse.ReasonPhrase,
                        registryResponse.StatusCode
                    );
                }
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error reading registry: {Message}", e.Message);
            }
            // sm registry updaten
            foreach (var sm in environment.Submodels ?? [])
            {
                var smRegistryUrl =
                    aasUrls.SubmodelRegistryUrl.AppendSlash()
                    + "submodel-descriptors/"
                    + sm.Id.ToBase64UrlEncoded(Encoding.UTF8);
                _logger?.LogDebug("SM registry URL: {smRegistryUrl}", smRegistryUrl);
                if (smRegistryUrl == null)
                    continue;

                // aas Registry updaten
                try
                {
                    var registryResponse = await client.GetAsync(smRegistryUrl, cancellationToken);
                    if (registryResponse.StatusCode == HttpStatusCode.NotFound)
                    {
                        // ist also schonmal nicht da -> anlegen

                        var smDescriptor = DescriptorCreator.CreateSubmodelDescriptor(
                            sm,
                            aasUrls.SubmodelRepositoryUrl
                        );

                        var jsonString = SerializeSmDesc(smDescriptor);
                        var content = new StringContent(
                            jsonString,
                            Encoding.UTF8,
                            "application/json"
                        );
                        var response = await client.PostAsync(
                            aasUrls.SubmodelRegistryUrl.AppendSlash() + "submodel-descriptors",
                            content,
                            cancellationToken
                        );
                        if (!response.IsSuccessStatusCode)
                        {
                            var c = await response.Content.ReadAsStringAsync();
                            _logger?.LogError(
                                "Error updating sm registry (create new descriptor): {ReasonPhrase} {StatusCode} {content}",
                                response.ReasonPhrase,
                                response.StatusCode,
                                c
                            );
                        }
                    }
                    else if (registryResponse.IsSuccessStatusCode)
                    {
                        var descriptorString = await registryResponse.Content.ReadAsStringAsync();
                        var descriptor = JsonConvert.DeserializeObject<SubmodelDescriptor>(
                            descriptorString
                        );
                        if (descriptor == null)
                        {
                            _logger?.LogError(
                                "Error updating sm registry: Descriptor could not be deserialized"
                            );
                            return;
                        }
                        // ist da -> updaten
                        _logger?.LogInformation("Updating existing AAS in registry");

                        DescriptorUpdater.UpdateSubmodelDescriptor(
                            descriptor,
                            (Submodel)sm,
                            descriptor.Id
                        );
                        var jsonString = SerializeSmDesc(descriptor);
                        var content = new StringContent(
                            jsonString,
                            Encoding.UTF8,
                            "application/json"
                        );
                        var response = await client.PutAsync(
                            aasUrls.SubmodelRegistryUrl.AppendSlash()
                                + "submodel-descriptors/"
                                + sm.Id.ToBase64UrlEncoded(Encoding.UTF8),
                            content,
                            cancellationToken
                        );
                        if (!response.IsSuccessStatusCode)
                        {
                            var c = await response.Content.ReadAsStringAsync();
                            _logger?.LogError(
                                "Error updating sm registry (create new descriptor): {ReasonPhrase} {StatusCode} {content}",
                                response.ReasonPhrase,
                                response.StatusCode,
                                c
                            );
                        }
                    }
                    else
                    {
                        var c = await registryResponse.Content.ReadAsStringAsync();
                        _logger?.LogError(
                            "Error updating aas registry (create new descriptor): {ReasonPhrase} {StatusCode} {content}",
                            registryResponse.ReasonPhrase,
                            registryResponse.StatusCode,
                            c
                        );
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, "Error reading registry: {Message}", e.Message);
                }
            }
        }
    }

    public static async Task RemoveFromAasRegistryAsync(
        string registryUrl,
        string aasId,
        CancellationToken cancellationToken,
        HttpClient client
    )
    {
        var url =
            registryUrl.AppendSlash()
            + "shell-descriptors/"
            + aasId.ToBase64UrlEncoded(Encoding.UTF8);
        try
        {
            var registryResponse = await client.DeleteAsync(url, cancellationToken);
            if (!registryResponse.IsSuccessStatusCode)
            {
                _logger?.LogError(
                    "Error removing from registry: {ReasonPhrase}",
                    registryResponse.ReasonPhrase
                );
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing from registry: {Message}", e.Message);
        }
    }

    public static async Task RemoveFromSmRegistryAsync(
        string registryUrl,
        string smId,
        CancellationToken cancellationToken,
        HttpClient client
    )
    {
        var url =
            registryUrl.AppendSlash()
            + "submodel-descriptors/"
            + smId.ToBase64UrlEncoded(Encoding.UTF8);
        try
        {
            var registryResponse = await client.DeleteAsync(url, cancellationToken);
            if (!registryResponse.IsSuccessStatusCode)
            {
                _logger?.LogError(
                    "Error removing from registry: {ReasonPhrase}",
                    registryResponse.ReasonPhrase
                );
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error removing from registry: {Message}", e.Message);
        }
    }
}
