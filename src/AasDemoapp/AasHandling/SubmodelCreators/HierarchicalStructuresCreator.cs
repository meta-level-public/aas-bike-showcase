using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;

namespace AasDemoapp.AasHandling.SubmodelCreators;

public class HierarchicalStructuresCreator
{
    public static Submodel CreateFromJson(string idPrefix)
    {
        // JSON-Datei lesen
        var jsonPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "AasHandling",
            "SubmodelCreators",
            "hierarchicalStructures.json"
        );
        var jsonString = System.IO.File.ReadAllText(jsonPath);

        // JSON zu AAS Core Submodel konvertieren
        return ConvertJsonToSubmodel(jsonString, idPrefix);
    }

    private static Submodel ConvertJsonToSubmodel(string jsonString, string idPrefix)
    {
        var jsonNode = JsonNode.Parse(jsonString);
        if (jsonNode == null)
            throw new Exception("Could not parse JSON");

        var submodel = Jsonization.Deserialize.SubmodelFrom(jsonNode);
        submodel.Kind = ModellingKind.Instance;
        submodel.Id = IdGenerationUtil.GenerateId(IdType.Submodel, idPrefix); // Generiere eine korrekte ID

        return submodel;
    }
}
