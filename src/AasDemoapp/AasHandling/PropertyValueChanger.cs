using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;

namespace AasDemoapp.AasHandling
{
    public class PropertyValueChanger
    {
        public static void SetPropertyValueByPath(
            string propertyPath,
            string value,
            Submodel submodel
        )
        {
            // Hier die Logik implementieren, um den Wert des angegebenen Properties im Submodel zu setzen
            var property = GetSubmodelElementByPath(propertyPath, submodel);
            if (property == null)
            {
                Console.WriteLine(
                    $"Property with path '{propertyPath}' not found in the submodel."
                );
            }

            if (property is Property prop)
            {
                prop.Value = value; // Setzen des Wertes
            }
            else if (property is MultiLanguageProperty multiLangProp)
            {
                var en = multiLangProp.Value?.Find(match => match.Language == "en");
                if (en != null)
                    en.Text = value; // Setzen des Wertes für die Sprache "en"
                var de = multiLangProp.Value?.Find(match => match.Language == "de");
                if (de != null)
                    de.Text = value; // Setzen des Wertes für die Sprache "de"
            }
            else
            {
                Console.WriteLine($"Element at path '{propertyPath}' is not a Property.");
            }
        }

        private static ISubmodelElement? GetSubmodelElementByPath(
            string propertyPath,
            Submodel submodel
        )
        {
            if (string.IsNullOrEmpty(propertyPath))
                return null;

            var pathSegments = propertyPath.Split('.');
            return FindElementRecursive(pathSegments, 0, submodel.SubmodelElements);
        }

        private static ISubmodelElement? FindElementRecursive(
            string[] pathSegments,
            int currentIndex,
            List<ISubmodelElement>? elements
        )
        {
            if (currentIndex >= pathSegments.Length || elements == null)
                return null;

            var currentSegment = pathSegments[currentIndex];

            // Prüfe ob es sich um einen Index-Zugriff handelt (z.B. [2])
            if (currentSegment.StartsWith("[") && currentSegment.EndsWith("]"))
            {
                var indexString = currentSegment.Substring(1, currentSegment.Length - 2);
                if (
                    int.TryParse(indexString, out int index)
                    && index >= 0
                    && index < elements.Count
                )
                {
                    var element = elements[index];

                    // Wenn wir am Ende des Pfades sind, geben wir das Element zurück
                    if (currentIndex == pathSegments.Length - 1)
                        return element;

                    // Rekursive Suche in Unterelementen
                    var childElements = GetChildElements(element);
                    if (childElements != null)
                    {
                        return FindElementRecursive(pathSegments, currentIndex + 1, childElements);
                    }
                }
                return null;
            }

            // Normale Suche nach Element mit passender idShort
            var foundElement = elements.FirstOrDefault(e => e.IdShort == currentSegment);

            if (foundElement == null)
                return null;

            // Wenn wir am Ende des Pfades sind, geben wir das Element zurück
            if (currentIndex == pathSegments.Length - 1)
                return foundElement;

            // Rekursive Suche in Unterelementen
            var childElementsForFound = GetChildElements(foundElement);
            if (childElementsForFound != null)
            {
                return FindElementRecursive(pathSegments, currentIndex + 1, childElementsForFound);
            }

            return null;
        }

        private static List<ISubmodelElement>? GetChildElements(ISubmodelElement element)
        {
            // Prüfe verschiedene Element-Typen die Unterelemente haben können
            return element switch
            {
                SubmodelElementCollection collection => collection.Value,
                SubmodelElementList list => list.Value,
                Entity entity => entity.Statements,
                // Weitere Container-Typen können hier hinzugefügt werden
                _ => null,
            };
        }
    }
}
