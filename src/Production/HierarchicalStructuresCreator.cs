using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;

namespace AasDemoapp.Production
{
    public class HierarchicalStructuresCreator
    {
        public static Submodel CreateFromJson()
        {
            // JSON-Datei lesen
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Production", "hierarchicalStructures.json");
            var jsonString = System.IO.File.ReadAllText(jsonPath);

            // JSON zu AAS Core Submodel konvertieren
            return ConvertJsonToSubmodel(jsonString);
        }

        private static Submodel ConvertJsonToSubmodel(string jsonString)
        {
            var jsonNode = JsonNode.Parse(jsonString);
            if (jsonNode == null) throw new Exception("Could not parse JSON");

            var submodel = Jsonization.Deserialize.SubmodelFrom(jsonNode);
            return submodel;
        }

    }
}