using System.Net;
using System.Text;
using AasCore.Aas3_0;
using AasDemoapp.Utils.Registry;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AasDemoapp.Utils.Registry
{
    public class DescriptorSerialization
    {
        public static AssetAdministrationShellDescriptor Deserialize(string json)
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented,
                Converters = [new StringEnumConverter()],
                NullValueHandling = NullValueHandling.Ignore
            };

            return JsonConvert.DeserializeObject<AssetAdministrationShellDescriptor>(json, serializerSettings)
                ?? throw new JsonException("Failed to deserialize AssetAdministrationShellDescriptor");
        }
    }
}