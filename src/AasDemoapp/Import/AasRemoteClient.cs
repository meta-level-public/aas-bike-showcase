using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database.Model;
using AasDemoapp.Utils;

namespace AasDemoapp.Import
{
    /// <summary>
    /// Client für das Abrufen von AAS-Daten aus Remote-Repositories
    /// </summary>
    public class AasRemoteClient
    {
        /// <summary>
        /// Ruft eine Shell und alle ihre Submodels von einem Remote-Repository ab
        /// </summary>
        public async Task<(
            AssetAdministrationShell shell,
            List<Submodel> submodels
        )> FetchShellWithSubmodelsAsync(
            string remoteAasRepositoryUrl,
            string remoteSmRepositoryUrl,
            string shellId,
            SecuritySetting securitySetting
        )
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);

            // Shell abrufen
            using var shellResponse = await client.GetAsync(
                remoteAasRepositoryUrl + $"/shells/{shellId.ToBase64()}"
            );
            shellResponse.EnsureSuccessStatusCode();
            string responseBody = await shellResponse.Content.ReadAsStringAsync();

            var shellNode = JsonNode.Parse(responseBody);
            if (shellNode == null)
            {
                throw new InvalidOperationException("Failed to parse shell JSON response");
            }
            var shell = Jsonization.Deserialize.AssetAdministrationShellFrom(shellNode);

            // Submodels abrufen
            var submodels = new List<Submodel>();
            foreach (var smRef in shell.Submodels ?? [])
            {
                using var smResponse = await client.GetAsync(
                    remoteSmRepositoryUrl + $"/submodels/{smRef.Keys?[0]?.Value.ToBase64()}"
                );
                smResponse.EnsureSuccessStatusCode();
                responseBody = await smResponse.Content.ReadAsStringAsync();

                var smNode = JsonNode.Parse(responseBody);
                if (smNode == null)
                {
                    throw new InvalidOperationException("Failed to parse submodel JSON response");
                }
                var sm = Jsonization.Deserialize.SubmodelFrom(smNode);
                submodels.Add(sm);
            }

            return (shell, submodels);
        }

        /// <summary>
        /// Lädt ein Thumbnail von einem Remote-Repository herunter
        /// </summary>
        public async Task<(
            byte[]? data,
            string? contentType,
            string? filename
        )> DownloadThumbnailAsync(
            string remoteAasRepositoryUrl,
            string shellId,
            SecuritySetting securitySetting,
            AssetAdministrationShell shell
        )
        {
            if (shell.AssetInformation?.DefaultThumbnail == null)
            {
                return (null, null, null);
            }

            try
            {
                using var client = HttpClientCreator.CreateHttpClient(securitySetting);
                using var response = await client.GetAsync(
                    remoteAasRepositoryUrl
                        + $"/shells/{shellId.ToBase64()}/asset-information/thumbnail"
                );

                if (!response.IsSuccessStatusCode)
                {
                    return (null, null, null);
                }

                var data = await response.Content.ReadAsByteArrayAsync();
                var contentType =
                    shell.AssetInformation.DefaultThumbnail.ContentType ?? "image/png";
                var filename =
                    Path.GetFileName(shell.AssetInformation.DefaultThumbnail.Path)
                    ?? "thumbnail.png";

                return (data, contentType, filename);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AasRemoteClient] Failed to download thumbnail: {ex.Message}");
                return (null, null, null);
            }
        }

        /// <summary>
        /// Erstellt ein Environment-Objekt aus einer Remote-Shell
        /// </summary>
        public async Task<AasCore.Aas3_0.Environment> GetEnvironmentAsync(
            string remoteUrl,
            SecuritySetting securitySetting,
            string shellId
        )
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            var url = remoteUrl + $"/shells/{shellId.ToBase64UrlEncoded(Encoding.UTF8)}";
            using var response = await client.GetAsync(url);
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
                    remoteUrl + $"/submodels/{smRef.Keys?[0]?.Value.ToBase64()}"
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

            return new AasCore.Aas3_0.Environment
            {
                AssetAdministrationShells = [shell],
                Submodels = submodels,
            };
        }

        /// <summary>
        /// Ruft ein Thumbnail als Base64-String ab
        /// </summary>
        public async Task<string> GetThumbnailAsBase64Async(
            string remoteUrl,
            SecuritySetting securitySetting,
            string shellId
        )
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            using var response = await client.GetAsync(
                remoteUrl + $"/shells/{shellId.ToBase64()}/asset-information/thumbnail"
            );
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsByteArrayAsync();

            return Convert.ToBase64String(responseBody);
        }
    }
}
