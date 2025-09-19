using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;

namespace AasDemoapp.AasHandling.SubmodelCreators;

public class HandoverDocumentationCreator
{
    public static Submodel CreateFromJson()
    {
        // JSON-Datei lesen
        var jsonPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "AasHandling",
            "SubmodelCreators",
            "handoverdoc.json"
        );
        var jsonString = System.IO.File.ReadAllText(jsonPath);

        // JSON zu AAS Core Submodel konvertieren
        return ConvertJsonToSubmodel(jsonString);
    }

    public static Submodel ConvertJsonToSubmodel(string jsonString)
    {
        var jsonNode = JsonNode.Parse(jsonString);
        if (jsonNode == null)
            throw new Exception("Could not parse JSON");

        var submodel = Jsonization.Deserialize.SubmodelFrom(jsonNode);
        submodel.Kind = ModellingKind.Instance;
        submodel.Id = IdGenerationUtil.GenerateId(IdType.Submodel, "https://oi4-nextbike.de"); // Generiere eine korrekte ID

        return submodel;
    }
}
