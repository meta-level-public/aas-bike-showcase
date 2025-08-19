using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace AasDemoapp.AasHandling.SubmodelCreators;

public class PcfEntryCreator
{
    public static SubmodelElementCollection CreateFromJson()
    {
        // JSON-Datei lesen
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AasHandling", "ElementCreators", "pcfEntry.json");
        var jsonString = System.IO.File.ReadAllText(jsonPath);

        // JSON zu AAS Core Submodel konvertieren
        return ConvertJsonToSubmodelElementCollection(jsonString);
    }

    public static SubmodelElementCollection ConvertJsonToSubmodelElementCollection(string jsonString)
    {
        var jsonNode = JsonNode.Parse(jsonString);
        if (jsonNode == null) throw new Exception("Could not parse JSON");

        var submodel = Jsonization.Deserialize.SubmodelElementCollectionFrom(jsonNode);
        
        return submodel;
    }
}