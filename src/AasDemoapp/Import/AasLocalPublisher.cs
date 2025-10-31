using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database.Model;
using AasDemoapp.Utils;
using Newtonsoft.Json;

namespace AasDemoapp.Import
{
    /// <summary>
    /// Service für das Pushen von AAS-Daten zu lokalen Repositories, Registries und Discovery
    /// </summary>
    public class AasLocalPublisher
    {
        private readonly DescriptorFactory _descriptorFactory;

        public AasLocalPublisher(DescriptorFactory descriptorFactory)
        {
            _descriptorFactory = descriptorFactory;
        }

        /// <summary>
        /// Pusht Shell, Submodels und optional Thumbnail zu einem lokalen Repository
        /// </summary>
        public async Task PushToRepositoryAsync(
            AssetAdministrationShell shell,
            List<Submodel> submodels,
            string localRepositoryUrl,
            SecuritySetting securitySetting,
            byte[]? thumbnailData = null,
            string? thumbnailContentType = null,
            string? thumbnailFilename = null
        )
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);

            // Shell pushen
            await PushShellAsync(client, shell, localRepositoryUrl);

            // Submodels pushen
            await PushSubmodelsAsync(client, submodels, localRepositoryUrl);

            // Thumbnail hochladen
            if (
                thumbnailData != null
                && thumbnailData.Length > 0
                && !string.IsNullOrEmpty(thumbnailFilename)
            )
            {
                await UploadThumbnailAsync(
                    client,
                    shell,
                    localRepositoryUrl,
                    thumbnailData,
                    thumbnailContentType,
                    thumbnailFilename
                );
            }
        }

        /// <summary>
        /// Pusht Shell-Descriptor zur lokalen Registry
        /// </summary>
        public async Task PushToRegistryAsync(
            AssetAdministrationShell shell,
            List<Submodel> submodels,
            string localRepositoryUrl,
            string localRegistryUrl,
            SecuritySetting securitySetting
        )
        {
            var jsonString = _descriptorFactory.CreateShellDescriptorJson(
                shell,
                submodels,
                localRepositoryUrl
            );

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

                if (result.IsSuccessStatusCode)
                {
                    var resultContent = await result.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AasLocalPublisher] Registry success: {resultContent}");
                }
                else
                {
                    string errorContent = await TryReadErrorContentAsync(result);
                    Console.WriteLine(
                        $"[AasLocalPublisher] Registry push failed with status {result.StatusCode}: {errorContent}"
                    );
                    throw new HttpRequestException(
                        $"Registry push failed with status {result.StatusCode}: {errorContent}"
                    );
                }
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (TaskCanceledException ex)
            {
                throw new TimeoutException("Request to registry timed out", ex);
            }
        }

        /// <summary>
        /// Registriert Shell bei der lokalen Discovery
        /// </summary>
        public async Task<bool> PushToDiscoveryAsync(
            AssetAdministrationShell shell,
            string localDiscoveryUrl,
            SecuritySetting securitySetting
        )
        {
            // Validate discovery service URL
            if (string.IsNullOrWhiteSpace(localDiscoveryUrl))
            {
                Console.WriteLine(
                    "[AasLocalPublisher] Warning: Discovery service URL is not configured. Skipping discovery update."
                );
                return false;
            }

            // Validate that URL is absolute
            if (!Uri.TryCreate(localDiscoveryUrl, UriKind.Absolute, out _))
            {
                Console.WriteLine(
                    $"[AasLocalPublisher] Warning: Discovery service URL '{localDiscoveryUrl}' is not a valid absolute URL. Skipping discovery update."
                );
                return false;
            }

            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            var discoveryUrl =
                localDiscoveryUrl.AppendSlash()
                + "lookup/shells/"
                + shell.Id.ToBase64UrlEncoded(Encoding.UTF8);

            // Alte Einträge löschen
            try
            {
                await client.DeleteAsync(discoveryUrl, CancellationToken.None);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[AasLocalPublisher] Error deleting discovery: {e.Message}");
            }

            // GlobalAssetId registrieren
            if (shell.AssetInformation.GlobalAssetId != null)
            {
                await RegisterDiscoveryElementAsync(
                    client,
                    discoveryUrl,
                    "globalAssetId",
                    shell.AssetInformation.GlobalAssetId
                );
            }

            // SpecificAssetIds registrieren
            foreach (var id in shell.AssetInformation.SpecificAssetIds ?? [])
            {
                await RegisterDiscoveryElementAsync(client, discoveryUrl, id.Name, id.Value);
            }

            return true;
        }

        private async Task PushShellAsync(
            HttpClient client,
            AssetAdministrationShell shell,
            string localRepositoryUrl
        )
        {
            var jsonString = Jsonization.Serialize.ToJsonObject(shell).ToJsonString();

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{localRepositoryUrl.AppendSlash()}shells"
            );

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            request.Content = content;

            try
            {
                var result = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead
                );

                if (result.IsSuccessStatusCode)
                {
                    var resultContent = await result.Content.ReadAsStringAsync();
                    Console.WriteLine($"[AasLocalPublisher] Shell pushed: {resultContent}");
                }
                else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    Console.WriteLine($"[AasLocalPublisher] Shell already exists in repository");
                    throw new InvalidOperationException(
                        "Der Asset Administration Shell Eintrag existiert bereits im Repository."
                    );
                }
                else
                {
                    string errorContent = await TryReadErrorContentAsync(result);
                    Console.WriteLine(
                        $"[AasLocalPublisher] Shell push failed with status {result.StatusCode}: {errorContent}"
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
        }

        private async Task PushSubmodelsAsync(
            HttpClient client,
            List<Submodel> submodels,
            string localRepositoryUrl
        )
        {
            foreach (var sm in submodels)
            {
                try
                {
                    using var request = new HttpRequestMessage(
                        HttpMethod.Post,
                        $"{localRepositoryUrl.AppendSlash()}submodels"
                    );
                    var jsonString = Jsonization.Serialize.ToJsonObject(sm).ToJsonString();

                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    request.Content = content;
                    var result = await client.SendAsync(
                        request,
                        HttpCompletionOption.ResponseHeadersRead
                    );

                    if (result.IsSuccessStatusCode)
                    {
                        var resultContent = await result.Content.ReadAsStringAsync();
                        Console.WriteLine($"[AasLocalPublisher] Submodel pushed: {resultContent}");
                    }
                    else if (result.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        Console.WriteLine(
                            $"[AasLocalPublisher] Submodel already exists in repository"
                        );
                        throw new InvalidOperationException(
                            "Das Teil existiert bereits im Repository."
                        );
                    }
                    else
                    {
                        string errorContent = await TryReadErrorContentAsync(result);
                        Console.WriteLine(
                            $"[AasLocalPublisher] Submodel push failed with status {result.StatusCode}: {errorContent}"
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
        }

        private async Task UploadThumbnailAsync(
            HttpClient client,
            AssetAdministrationShell shell,
            string localRepositoryUrl,
            byte[] thumbnailData,
            string? thumbnailContentType,
            string thumbnailFilename
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

                using var thumbnailRequest = new HttpRequestMessage(HttpMethod.Put, thumbnailUrl);
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
                    Console.WriteLine($"[AasLocalPublisher] Thumbnail uploaded successfully");
                }
                else
                {
                    Console.WriteLine(
                        $"[AasLocalPublisher] Thumbnail upload failed with status {thumbnailResult.StatusCode}"
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AasLocalPublisher] Failed to upload thumbnail: {ex.Message}");
                // Thumbnail upload failure should not break the import
            }
        }

        private async Task RegisterDiscoveryElementAsync(
            HttpClient client,
            string discoveryUrl,
            string name,
            string value
        )
        {
            try
            {
                var element = new DiscoveryElement { name = name, value = value };
                var jsonString = JsonConvert.SerializeObject(element);
                await client.PostAsync(
                    discoveryUrl,
                    new StringContent(jsonString, Encoding.UTF8, "application/json"),
                    CancellationToken.None
                );
            }
            catch (Exception e)
            {
                Console.WriteLine($"[AasLocalPublisher] Error saving discovery: {e.Message}");
            }
        }

        private async Task<string> TryReadErrorContentAsync(HttpResponseMessage result)
        {
            try
            {
                return await result.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[AasLocalPublisher] Failed to read error content: {ex.Message}"
                );
                return "Unable to read error response";
            }
        }
    }

    public class DiscoveryElement
    {
        public string name { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
    }
}
