using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using AasCore.Aas3_0;

namespace AasDemoapp.AasHandling.SubmodelCreators
{
    public class ContactInformationCreator
    {
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

        public static SubmodelElementCollection CreateFromJson(
            string CompanyName,
            string street,
            string city,
            string zip,
            string countryCode
        )
        {
            // JSON-Datei lesen
            var jsonPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "AasHandling",
                "SubmodelCreators",
                "contactInfo.json"
            );
            var jsonString = System.IO.File.ReadAllText(jsonPath);

            // TODO: besser über semanticIds suchen

            // JSON zu AAS Core SubmodelElementCollection konvertieren
            var smc = ConvertJsonToSubmodelElementCollection(jsonString);

            if (smc == null || smc.Value == null)
                throw new Exception("Could not convert JSON to SubmodelElementCollection");

            // Werte Setzen
            var companyProp = (MultiLanguageProperty?)smc.Value.Find(p => p.IdShort == "Company");
            // semanticId 0173-1#02-AAW001#001
            if (companyProp != null)
            {
                SetMultiLanguageValue(companyProp, CompanyName);
            }

            var streetProp = (MultiLanguageProperty?)smc.Value.Find(p => p.IdShort == "Street");
            // semanticId 0173-1#02-AAO128#002
            if (streetProp != null)
            {
                SetMultiLanguageValue(streetProp, street);
            }

            var cityProp = (MultiLanguageProperty?)smc.Value.Find(p => p.IdShort == "CityTown");
            // semanticId 0173-1#02-AAO132#002
            if (cityProp != null)
            {
                SetMultiLanguageValue(cityProp, city);
            }

            var zipProp = (MultiLanguageProperty?)smc.Value.Find(p => p.IdShort == "Zipcode");
            // semanticId 0173-1#02-AAO129#002
            if (zipProp != null)
            {
                SetMultiLanguageValue(zipProp, zip);
            }

            var nationalCode = (MultiLanguageProperty?)
                smc.Value.Find(p => p.IdShort == "NationalCode");
            // semanticID: 0173-1#02-AAO134#002
            if (nationalCode != null)
            {
                SetMultiLanguageValue(nationalCode, countryCode);
            }

            return smc;
        }

        public static SubmodelElementCollection ConvertJsonToSubmodelElementCollection(
            string jsonString
        )
        {
            var jsonNode = JsonNode.Parse(jsonString);
            if (jsonNode == null)
                throw new Exception("Could not parse JSON");

            var smc = Jsonization.Deserialize.SubmodelElementCollectionFrom(jsonNode);

            return smc;
        }
    }
}
