using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;

namespace AasDemoapp.AasHandling.SubmodelElementCreators;

/// <summary>
/// Creator-Klasse für Transport Carbon Footprint (TCF) SubmodelElementCollection.
/// Liest die JSON-Datei "tcfCollection" ein und konvertiert sie in eine SubmodelElementCollection.
/// </summary>
public class TcfCollectionCreator
{
    /// <summary>
    /// Erstellt eine SubmodelElementCollection aus der tcfCollection JSON-Datei.
    /// </summary>
    /// <returns>Eine SubmodelElementCollection mit Transport Carbon Footprint Daten</returns>
    public static SubmodelElementCollection CreateFromJson()
    {
        // JSON-Datei lesen
        var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AasHandling", "SubmodelElementCreators", "tcfCollection");
        var jsonString = System.IO.File.ReadAllText(jsonPath);

        // JSON zu AAS Core SubmodelElementCollection konvertieren
        return ConvertJsonToSubmodelElementCollection(jsonString);
    }

    /// <summary>
    /// Konvertiert JSON-String zu SubmodelElementCollection.
    /// </summary>
    /// <param name="jsonString">JSON-String der SubmodelElementCollection</param>
    /// <returns>Eine SubmodelElementCollection</returns>
    private static SubmodelElementCollection ConvertJsonToSubmodelElementCollection(string jsonString)
    {
        var jsonNode = JsonNode.Parse(jsonString);
        if (jsonNode == null) throw new Exception("Could not parse JSON");

        var submodelElementCollection = Jsonization.Deserialize.SubmodelElementCollectionFrom(jsonNode);
        
        return submodelElementCollection;
    }
}
