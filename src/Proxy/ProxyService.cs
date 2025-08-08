using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database.Model;
using AasDemoapp.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AasDemoapp.Proxy
{
    public class ProxyService
    {
        public async Task<string[]> Discover(string registryUrl, SecuritySetting securitySetting, string assetId)
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            var specificAssetId = new SpecificAssetId("globalAssetId", assetId);
            var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var jsonString = JsonConvert.SerializeObject(specificAssetId, Formatting.Indented, settings).ToBase64UrlEncoded(Encoding.UTF8);


            var url = registryUrl + $"/lookup/shells?assetIds={jsonString}";
            using HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseBody))
            {
                return [];
            }

            try
            {
                var jsonDocument = JsonDocument.Parse(responseBody);
                var root = jsonDocument.RootElement;

                if (root.TryGetProperty("result", out var resultElement))
                {
                    var resultArray = new List<string>();
                    foreach (var item in resultElement.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.String)
                        {
                            resultArray.Add(item.GetString() ?? string.Empty);
                        }
                    }
                    return resultArray.ToArray();
                }

                return [];
            }
            catch (System.Text.Json.JsonException)
            {
                // Falls JSON nicht geparst werden kann, leeres Array zurückgeben
                return [];
            }
        }

        public async Task<bool> Delete(string registryUrl, SecuritySetting securitySetting, string aasId)
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            var url = registryUrl + $"/shells/{aasId.ToBase64()}";
            using HttpResponseMessage response = await client.DeleteAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            return true;
        }
    }
}