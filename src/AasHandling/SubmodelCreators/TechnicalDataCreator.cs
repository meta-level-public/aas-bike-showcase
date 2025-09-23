using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;

namespace AasDemoapp.AasHandling.SubmodelCreators
{
    public class TechnicalDataCreator
    {
        public static Submodel CreateFromJson()
        {
            // JSON-Datei lesen
            var jsonPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "AasHandling",
                "SubmodelCreators",
                "technicalData.json"
            );
            var jsonString = System.IO.File.ReadAllText(jsonPath);

            // JSON zu AAS Core Submodel konvertieren
            return ConvertJsonToSubmodel(jsonString);
        }

        private static Submodel ConvertJsonToSubmodel(string jsonString)
        {
            var jsonNode = JsonNode.Parse(jsonString);
            if (jsonNode == null)
                throw new Exception("Could not parse JSON");

            var submodel = Jsonization.Deserialize.SubmodelFrom(jsonNode);
            submodel.Kind = ModellingKind.Instance;
            submodel.Id = IdGenerationUtil.GenerateId(IdType.Submodel, "https://oi4-nextbike.de"); // Generiere eine korrekte ID

            return submodel;
        }

        /// <summary>
        /// Setzt den Wert einer Property
        /// </summary>
        private static void SetPropertyValue(Property property, string value)
        {
            if (property != null)
            {
                property.Value = value;
            }
        }

        /// <summary>
        /// Setzt den Wert einer MultiLanguageProperty sowohl für "de" als auch "en".
        /// Ergänzt fehlende LangStringTexts falls notwendig.
        /// </summary>
        private static void SetMultiLanguageValue(MultiLanguageProperty mlp, string value)
        {
            if (mlp?.Value == null)
                return;

            // Prüfe ob bereits "de" und "en" vorhanden sind
            var deLang = mlp.Value.FirstOrDefault(l => l.Language == "de");
            var enLang = mlp.Value.FirstOrDefault(l => l.Language == "en");

            // Ergänze fehlende LangStringTexts
            if (deLang == null)
            {
                var newDeLang = new LangStringTextType("de", value);
                mlp.Value.Add(newDeLang);
            }
            else
            {
                deLang.Text = value;
            }

            if (enLang == null)
            {
                var newEnLang = new LangStringTextType("en", value);
                mlp.Value.Add(newEnLang);
            }
            else
            {
                enLang.Text = value;
            }
        }

        /// <summary>
        /// Findet ein SubmodelElement basierend auf dem IdShort
        /// </summary>
        private static T? FindElementByIdShort<T>(IList<ISubmodelElement> elements, string idShort)
            where T : class, ISubmodelElement
        {
            return elements?.FirstOrDefault(e => e.IdShort == idShort) as T;
        }

        /// <summary>
        /// Findet ein SubmodelElement in einer SubmodelElementCollection basierend auf dem IdShort
        /// </summary>
        private static T? FindElementInCollection<T>(
            SubmodelElementCollection collection,
            string idShort
        )
            where T : class, ISubmodelElement
        {
            return collection?.Value?.FirstOrDefault(e => e.IdShort == idShort) as T;
        }
    }
}
