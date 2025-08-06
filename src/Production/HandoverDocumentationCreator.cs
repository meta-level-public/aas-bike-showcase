using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using System.Text.Json;

namespace AasDemoapp.Production
{
    public class HandoverDocumentationCreator
    {
        public static Submodel CreateFromJson()
        {
            // JSON-Datei lesen
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Production", "handoverdoc.json");
            var jsonString = System.IO.File.ReadAllText(jsonPath);
            
            // JSON zu AAS Core Submodel konvertieren
            return ConvertJsonToSubmodel(jsonString);
        }

        public static Submodel ConvertJsonToSubmodel(string jsonString)
        {
            // JSON parsen
            var jsonDoc = JsonDocument.Parse(jsonString);
            var root = jsonDoc.RootElement;

            // Grundlegende Submodel-Eigenschaften extrahieren
            var id = root.GetProperty("id").GetString()!;
            var submodelElements = ParseSubmodelElements(root.GetProperty("submodelElements"));

            // Submodel erstellen
            var submodel = new Submodel(id: id, submodelElements: submodelElements);

            // Optionale Eigenschaften setzen
            if (root.TryGetProperty("idShort", out var idShort))
                submodel.IdShort = idShort.GetString();

            if (root.TryGetProperty("description", out var description))
                submodel.Description = ParseLangStringSet(description);

            if (root.TryGetProperty("semanticId", out var semanticId))
                submodel.SemanticId = ParseReference(semanticId);

            if (root.TryGetProperty("administration", out var administration))
                submodel.Administration = ParseAdministration(administration);

            return submodel;
        }

        private static List<ISubmodelElement> ParseSubmodelElements(JsonElement elementsArray)
        {
            var elements = new List<ISubmodelElement>();

            foreach (var element in elementsArray.EnumerateArray())
            {
                var modelType = element.GetProperty("modelType").GetString();
                
                switch (modelType)
                {
                    case "Property":
                        elements.Add(ParseProperty(element));
                        break;
                    case "SubmodelElementCollection":
                        elements.Add(ParseSubmodelElementCollection(element));
                        break;
                    case "MultiLanguageProperty":
                        elements.Add(ParseMultiLanguageProperty(element));
                        break;
                    case "File":
                        elements.Add(ParseFile(element));
                        break;
                    case "ReferenceElement":
                        elements.Add(ParseReferenceElement(element));
                        break;
                    case "Entity":
                        elements.Add(ParseEntity(element));
                        break;
                }
            }

            return elements;
        }

        private static Property ParseProperty(JsonElement element)
        {
            var idShort = element.GetProperty("idShort").GetString()!;
            var valueType = element.TryGetProperty("valueType", out var vt) ? vt.GetString() : "xs:string";
            
            var property = new Property(ParseDataType(valueType), idShort: idShort);

            // Optionale Eigenschaften setzen
            if (element.TryGetProperty("category", out var cat))
                property.Category = cat.GetString();

            if (element.TryGetProperty("value", out var val))
                property.Value = val.GetString();

            if (element.TryGetProperty("semanticId", out var semId))
                property.SemanticId = ParseReference(semId);

            if (element.TryGetProperty("description", out var desc))
                property.Description = ParseLangStringSet(desc);

            return property;
        }

        private static SubmodelElementCollection ParseSubmodelElementCollection(JsonElement element)
        {
            var idShort = element.GetProperty("idShort").GetString()!;
            var value = element.TryGetProperty("value", out var val) ? ParseSubmodelElements(val) : new List<ISubmodelElement>();
            
            var collection = new SubmodelElementCollection(idShort: idShort, value: value);

            // Optionale Eigenschaften setzen
            if (element.TryGetProperty("category", out var cat))
                collection.Category = cat.GetString();

            if (element.TryGetProperty("semanticId", out var semId))
                collection.SemanticId = ParseReference(semId);

            if (element.TryGetProperty("description", out var desc))
                collection.Description = ParseLangStringSet(desc);

            return collection;
        }

        private static MultiLanguageProperty ParseMultiLanguageProperty(JsonElement element)
        {
            var idShort = element.GetProperty("idShort").GetString()!;
            var multiLangProp = new MultiLanguageProperty(idShort: idShort);

            // Optionale Eigenschaften setzen
            if (element.TryGetProperty("category", out var cat))
                multiLangProp.Category = cat.GetString();

            if (element.TryGetProperty("semanticId", out var semId))
                multiLangProp.SemanticId = ParseReference(semId);

            if (element.TryGetProperty("description", out var desc))
                multiLangProp.Description = ParseLangStringSet(desc);

            if (element.TryGetProperty("value", out var val))
                multiLangProp.Value = ParseLangStringSet(val);

            return multiLangProp;
        }

        private static AasCore.Aas3_0.File ParseFile(JsonElement element)
        {
            var idShort = element.GetProperty("idShort").GetString()!;
            var contentType = element.GetProperty("contentType").GetString()!;
            
            var file = new AasCore.Aas3_0.File(contentType: contentType, idShort: idShort);

            // Optionale Eigenschaften setzen
            if (element.TryGetProperty("category", out var cat))
                file.Category = cat.GetString();

            if (element.TryGetProperty("value", out var val))
                file.Value = val.GetString();

            if (element.TryGetProperty("semanticId", out var semId))
                file.SemanticId = ParseReference(semId);

            if (element.TryGetProperty("description", out var desc))
                file.Description = ParseLangStringSet(desc);

            return file;
        }

        private static ReferenceElement ParseReferenceElement(JsonElement element)
        {
            var idShort = element.GetProperty("idShort").GetString()!;
            var refElement = new ReferenceElement(idShort: idShort);

            // Optionale Eigenschaften setzen
            if (element.TryGetProperty("category", out var cat))
                refElement.Category = cat.GetString();

            if (element.TryGetProperty("semanticId", out var semId))
                refElement.SemanticId = ParseReference(semId);

            if (element.TryGetProperty("description", out var desc))
                refElement.Description = ParseLangStringSet(desc);

            if (element.TryGetProperty("value", out var val))
                refElement.Value = ParseReference(val);

            return refElement;
        }

        private static Entity ParseEntity(JsonElement element)
        {
            var idShort = element.GetProperty("idShort").GetString()!;
            var entityTypeStr = element.GetProperty("entityType").GetString()!;
            var entityType = ParseEntityType(entityTypeStr);
            
            var entity = new Entity(entityType: entityType, idShort: idShort);

            // Optionale Eigenschaften setzen
            if (element.TryGetProperty("category", out var cat))
                entity.Category = cat.GetString();

            if (element.TryGetProperty("semanticId", out var semId))
                entity.SemanticId = ParseReference(semId);

            if (element.TryGetProperty("description", out var desc))
                entity.Description = ParseLangStringSet(desc);

            return entity;
        }

        // Helper-Methoden
        private static EntityType ParseEntityType(string entityType)
        {
            return entityType switch
            {
                "CoManagedEntity" => EntityType.CoManagedEntity,
                "SelfManagedEntity" => EntityType.SelfManagedEntity,
                _ => EntityType.CoManagedEntity
            };
        }

        private static DataTypeDefXsd ParseDataType(string? dataType)
        {
            return dataType switch
            {
                "xs:string" => DataTypeDefXsd.String,
                "xs:integer" => DataTypeDefXsd.Integer,
                "xs:boolean" => DataTypeDefXsd.Boolean,
                "xs:date" => DataTypeDefXsd.Date,
                "xs:decimal" => DataTypeDefXsd.Decimal,
                _ => DataTypeDefXsd.String
            };
        }

        private static IReference? ParseReference(JsonElement? element)
        {
            if (!element.HasValue || element.Value.ValueKind == JsonValueKind.Null)
                return null;

            var refElement = element.Value;
            if (!refElement.TryGetProperty("keys", out var keysElement))
                return null;

            var keys = ParseKeys(keysElement);
            var type = refElement.TryGetProperty("type", out var typeElement) ? typeElement.GetString() : "ExternalReference";

            var referenceType = type switch
            {
                "ModelReference" => ReferenceTypes.ModelReference,
                "ExternalReference" => ReferenceTypes.ExternalReference,
                _ => ReferenceTypes.ExternalReference
            };

            return new Reference(referenceType, keys);
        }

        private static List<IKey> ParseKeys(JsonElement keysArray)
        {
            var keys = new List<IKey>();
            
            foreach (var keyElement in keysArray.EnumerateArray())
            {
                if (keyElement.TryGetProperty("type", out var typeElement) && 
                    keyElement.TryGetProperty("value", out var valueElement))
                {
                    var keyType = ParseKeyType(typeElement.GetString()!);
                    var value = valueElement.GetString()!;
                    
                    keys.Add(new Key(keyType, value));
                }
            }

            return keys;
        }

        private static KeyTypes ParseKeyType(string keyType)
        {
            return keyType switch
            {
                "Submodel" => KeyTypes.Submodel,
                "GlobalReference" => KeyTypes.GlobalReference,
                "Property" => KeyTypes.Property,
                "SubmodelElementCollection" => KeyTypes.SubmodelElementCollection,
                "SubmodelElement" => KeyTypes.SubmodelElement,
                "Entity" => KeyTypes.Entity,
                "File" => KeyTypes.File,
                "MultiLanguageProperty" => KeyTypes.MultiLanguageProperty,
                "ReferenceElement" => KeyTypes.ReferenceElement,
                _ => KeyTypes.GlobalReference
            };
        }

        private static AdministrativeInformation? ParseAdministration(JsonElement? element)
        {
            if (!element.HasValue || element.Value.ValueKind == JsonValueKind.Null)
                return null;

            var adminElement = element.Value;
            var administration = new AdministrativeInformation();

            if (adminElement.TryGetProperty("version", out var ver))
                administration.Version = ver.GetString();

            if (adminElement.TryGetProperty("revision", out var rev))
                administration.Revision = rev.GetString();

            return administration;
        }

        private static List<ILangStringTextType>? ParseLangStringSet(JsonElement? element)
        {
            if (!element.HasValue || element.Value.ValueKind == JsonValueKind.Null)
                return null;

            var langStrings = new List<ILangStringTextType>();

            if (element.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var langElement in element.Value.EnumerateArray())
                {
                    if (langElement.TryGetProperty("language", out var langProp) && 
                        langElement.TryGetProperty("text", out var textProp))
                    {
                        var language = langProp.GetString()!;
                        var text = textProp.GetString()!;
                        langStrings.Add(new LangStringTextType(language, text));
                    }
                }
            }

            return langStrings.Count > 0 ? langStrings : null;
        }
    }
}