using System.Text;
using System.Text.Json.Nodes;
using AasCore.Aas3_0;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AasDemoapp.Utils.ConceptDescriptions;

public class CdLoader
{
    public static ConceptDescription? GetCdFromDescriptor(ConceptDescriptionMetadata cdDescriptor, string cdEndpoint, HttpClient client)
    {
        var url = cdEndpoint.AppendSlash() + "concept-descriptions".AppendSlash() + cdDescriptor.Irdi.ToBase64UrlEncoded(Encoding.UTF8);
        return GetSingle(url, client);
    }

    public static ConceptDescription? GetSingle(string url, HttpClient client)
    {
        if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {

            try
            {
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                    // nicht schlimm - kennen wir eben nicht
                }

                var responseContent = response.Content.ReadAsStringAsync().Result;

                JObject? res = JsonConvert.DeserializeObject<JObject>(responseContent);

                if (res != null)
                {
                    var jsonNode = JsonNode.Parse(res.ToString());
                    if (jsonNode == null) throw new Exception("Could not parse JSON");

                    return Jsonization.Deserialize.ConceptDescriptionFrom(jsonNode);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return null;
    }
}