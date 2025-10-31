using System.Linq;
using AasCore.Aas3_0;

namespace AasDemoapp.Import
{
    /// <summary>
    /// Utility-Klasse für die Analyse von Submodels
    /// </summary>
    public class SubmodelAnalyzer
    {
        private const string NameplateId = "https://admin-shell.io/zvei/nameplate/2/0/Nameplate";
        private const string ProductFamilyId = "0173-1#02-AAU731#001";
        private const string CarbonFootprintId =
            "https://admin-shell.io/idta/CarbonFootprint/CarbonFootprint/0/9";

        /// <summary>
        /// Findet das Nameplate-Submodel in einem Environment
        /// </summary>
        public Submodel? GetNameplate(AasCore.Aas3_0.Environment env)
        {
            var nameplate = env.Submodels?.Find(sm =>
                sm.SemanticId != null
                && sm.SemanticId.Keys != null
                && sm.SemanticId.Keys.Exists(id => id.Value == NameplateId)
            );

            return (Submodel?)nameplate;
        }

        /// <summary>
        /// Findet das CarbonFootprint-Submodel in einem Environment
        /// </summary>
        public Submodel? GetCarbonFootprint(AasCore.Aas3_0.Environment env)
        {
            var carbonFootprint = env.Submodels?.Find(sm =>
                sm.SemanticId != null
                && sm.SemanticId.Keys != null
                && sm.SemanticId.Keys.Exists(id => id.Value == CarbonFootprintId)
            );

            return (Submodel?)carbonFootprint;
        }

        /// <summary>
        /// Prüft, ob ein Environment ein CarbonFootprint-Submodel besitzt
        /// </summary>
        public bool HasCarbonFootprintSubmodel(AasCore.Aas3_0.Environment env)
        {
            return GetCarbonFootprint(env) != null;
        }

        /// <summary>
        /// Extrahiert die Produktkategorie aus einem Nameplate-Submodel
        /// </summary>
        public string GetKategorie(Submodel nameplate)
        {
            var mlp = (MultiLanguageProperty?)
                nameplate.SubmodelElements?.Find(sme =>
                    sme.SemanticId != null
                    && sme.SemanticId.Keys != null
                    && sme.SemanticId.Keys.Exists(id => id.Value == ProductFamilyId)
                );

            return mlp?.Value?.FirstOrDefault()?.Text ?? string.Empty;
        }

        /// <summary>
        /// Extrahiert die Produktkategorie aus einem Environment
        /// </summary>
        public string GetKategorie(AasCore.Aas3_0.Environment env)
        {
            var nameplate = GetNameplate(env);
            return nameplate != null ? GetKategorie(nameplate) : string.Empty;
        }
    }
}
