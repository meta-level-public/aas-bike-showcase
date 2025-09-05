# AAS Demoapp Tests

Dieses Projekt enthält Unit-Tests für die AAS Demoapp mit der gleichen Verzeichnisstruktur wie das Hauptprojekt.

## Verzeichnisstruktur

```
tests/
├── AasDemoapp.Tests.csproj              # Test-Projekt-Datei
├── GlobalUsings.cs                      # Globale Using-Direktiven
├── README.md                           # Diese Dokumentation
└── AasHandling/
    ├── SubmodelCreators/
    │   ├── HandoverDocumentationCreatorTests.cs
    │   ├── HierarchicalStructuresCreatorTests.cs
    │   ├── NameplateCreatorTests.cs
    │   ├── handoverdoc.json             # Test-Daten
    │   ├── hierarchicalStructures.json  # Test-Daten
    │   ├── nameplate.json              # Test-Daten
    │   └── pcf.json                    # Test-Daten
    └── SubmodelElementCreators/
        ├── TcfCollectionCreatorTests.cs
        └── tcfCollection               # Test-Daten
```

Die Test-Verzeichnisstruktur spiegelt die Struktur des Hauptprojekts wider:

- `src/AasHandling/SubmodelCreators/` → `tests/AasHandling/SubmodelCreators/`
- `src/AasHandling/SubmodelElementCreators/` → `tests/AasHandling/SubmodelElementCreators/`

## Test-Framework

- **xUnit**: Als Test-Framework
- **Moq**: Für Mocking (falls benötigt)
- **.NET 9.0**: Target Framework

## Ausführen der Tests

### Alle Tests ausführen

```bash
cd tests
dotnet test
```

### Tests mit detaillierter Ausgabe ausführen

```bash
cd tests
dotnet test --verbosity normal
```

### Tests mit Coverage-Report ausführen

```bash
cd tests
dotnet test --collect:"XPlat Code Coverage"
```

## Test-Struktur

### TcfCollectionCreatorTests

Tests für die `TcfCollectionCreator` Klasse:

1. **CreateFromJson_ShouldReturnSubmodelElementCollection**: Überprüft, dass eine `SubmodelElementCollection` zurückgegeben wird
2. **CreateFromJson_ShouldHaveCorrectIdShort**: Überprüft die korrekte IdShort "TransportCarbonFootprint"
3. **CreateFromJson_ShouldHaveDisplayName**: Überprüft die Display-Namen in deutscher und englischer Sprache
4. **CreateFromJson_ShouldHaveDescription**: Überprüft die Beschreibung
5. **CreateFromJson_ShouldHaveSemanticId**: Überprüft die semantische ID und deren Referenz
6. **CreateFromJson_ShouldHaveValue**: Überprüft, dass die Value-Sammlung existiert und nicht leer ist
7. **CreateFromJson_ShouldContainTCFCalculationMethodElement**: Überprüft das spezifische Element "TCFCalculationMethod"
8. **CreateFromJson_WhenFileNotExists_ShouldThrowException**: Überprüft das Verhalten bei fehlender JSON-Datei

### HandoverDocumentationCreatorTests

Tests für die `HandoverDocumentationCreator` Klasse:

1. **CreateFromJson_ShouldReturnSubmodel**: Überprüft, dass ein `Submodel` zurückgegeben wird
2. **CreateFromJson_ShouldHaveCorrectIdShort**: Überprüft die korrekte IdShort "HandoverDocumentation"
3. **CreateFromJson_ShouldHaveDescription**: Überprüft die englische Beschreibung
4. **CreateFromJson_ShouldHaveInstanceKind**: Überprüft, dass `ModellingKind.Instance` gesetzt ist
5. **CreateFromJson_ShouldHaveGeneratedId**: Überprüft, dass eine gültige ID mit der Domain generiert wird
6. **CreateFromJson_ShouldHaveSemanticId**: Überprüft die semantische ID
7. **CreateFromJson_ShouldHaveSubmodelElements**: Überprüft, dass SubmodelElements vorhanden sind
8. **CreateFromJson_ShouldContainNumberOfDocumentsElement**: Überprüft das spezifische Element "numberOfDocuments"
9. **ConvertJsonToSubmodel_WithValidJson_ShouldReturnSubmodel**: Überprüft die direkte JSON-Konvertierung
10. **ConvertJsonToSubmodel_WithInvalidJson_ShouldThrowException**: Überprüft die Fehlerbehandlung bei ungültigem JSON
11. **ConvertJsonToSubmodel_WithNullJson_ShouldThrowException**: Überprüft die Fehlerbehandlung bei null-Eingabe
12. **CreateFromJson_WhenFileNotExists_ShouldThrowException**: Überprüft das Verhalten bei fehlender JSON-Datei

### HierarchicalStructuresCreatorTests

Tests für die `HierarchicalStructuresCreator` Klasse:

1. **CreateFromJson_ShouldReturnSubmodel**: Überprüft, dass ein `Submodel` zurückgegeben wird
2. **CreateFromJson_ShouldHaveCorrectIdShort**: Überprüft die korrekte IdShort "HierarchicalStructures"
3. **CreateFromJson_ShouldHaveInstanceKind**: Überprüft, dass `ModellingKind.Instance` gesetzt ist
4. **CreateFromJson_ShouldHaveGeneratedId**: Überprüft, dass eine gültige ID mit der Domain generiert wird
5. **CreateFromJson_ShouldHaveSemanticId**: Überprüft die semantische ID
6. **CreateFromJson_ShouldHaveSubmodelElements**: Überprüft, dass SubmodelElements vorhanden sind
7. **CreateFromJson_ShouldHaveDescription**: Überprüft die Beschreibung
8. **CreateFromJson_WhenFileNotExists_ShouldThrowException**: Überprüft das Verhalten bei fehlender JSON-Datei

### NameplateCreatorTests

Tests für die `NameplateCreator` Klasse:

1. **CreateFromJson_ShouldReturnSubmodel**: Überprüft, dass ein `Submodel` zurückgegeben wird
2. **CreateFromJson_ShouldHaveCorrectIdShort**: Überprüft die korrekte IdShort "DigitalNameplate"
3. **CreateFromJson_ShouldHaveInstanceKind**: Überprüft, dass `ModellingKind.Instance` gesetzt ist
4. **CreateFromJson_ShouldHaveGeneratedId**: Überprüft, dass eine gültige ID mit der Domain generiert wird
5. **CreateFromJson_ShouldHaveSemanticId**: Überprüft die semantische ID und die spezifische Nameplate-Referenz
6. **CreateFromJson_ShouldHaveSubmodelElements**: Überprüft, dass SubmodelElements vorhanden sind
7. **CreateFromJson_ShouldHaveDescription**: Überprüft die Beschreibung
8. **CreateFromJson_ShouldHaveAdministration**: Überprüft Version und Revision der Administration
9. **CreateFromJson_ShouldContainManufacturerNameElement**: Überprüft Nameplate-spezifische Elemente
10. **CreateFromJson_WhenFileNotExists_ShouldThrowException**: Überprüft das Verhalten bei fehlender JSON-Datei

## Hinweise

- Die Tests verwenden die originalen JSON-Dateien aus dem Hauptprojekt
- **Gesamt: 38 Tests** laufen erfolgreich und decken die wichtigsten Funktionen ab:
  - **TcfCollectionCreator**: 8 Tests
  - **HandoverDocumentationCreator**: 12 Tests
  - **HierarchicalStructuresCreator**: 8 Tests
  - **NameplateCreator**: 10 Tests
- Das Test-Projekt verwendet das gleiche .NET Framework wie das Hauptprojekt (net9.0)
- Alle Creator-Klassen für AAS Submodels sind vollständig getestet
