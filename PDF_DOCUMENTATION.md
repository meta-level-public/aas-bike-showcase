# PDF-Übergabedokumentation

Diese Funktionalität erstellt automatisch ein PDF-Dokument für die Übergabe von Produkten mit dem aktuellen Datum, der Firmenadresse und einem Fahrradbild.

## Implementierte Komponenten

### 1. PdfService.cs

- **Ort**: `src/Production/PdfService.cs`
- **Funktion**: Erstellt PDF-Dokumente mit iText7
- **Methode**: `CreateHandoverPdf(Address? companyAddress = null)`

**Features:**

- ✅ Aktuelles Datum wird automatisch eingefügt
- ✅ Firmenadresse wird angezeigt (falls vorhanden)
- ✅ Fahrradbild wird aus dem images-Ordner geladen
- ✅ Professionelles PDF-Layout mit Titel und Formatierung
- ✅ Automatische Fehlerbehandlung bei fehlendem Bild

### 2. Erweiterte InstanceAasCreator.cs

- **Ort**: `src/Production/InstanceAasCreator.cs`
- **Änderung**: `CreateHandoverDocumentation` Methode akzeptiert jetzt `Address`-Parameter
- **Funktion**: Generiert PDF und bindet es in das AAS-Submodel ein

### 3. API-Endpunkt

- **Ort**: `src/Controllers/ProductionController.cs`
- **Endpunkt**: `POST /api/Production/GenerateHandoverPdf`
- **Funktion**: Direkte PDF-Generierung über HTTP-API

## Verwendung

### Via API

```bash
# POST-Request an den API-Endpunkt
curl -X POST "https://localhost:7063/api/Production/GenerateHandoverPdf" \
     -H "Content-Type: application/json" \
     -d '{
       "name": "OI4 Nextbike",
       "strasse": "Musterstraße 123",
       "plz": "12345",
       "ort": "Musterstadt",
       "land": "Deutschland"
     }' \
     --output handover_documentation.pdf
```

### Programmatische Verwendung

```csharp
using AasDemoapp.Production;
using AasDemoapp.Database.Model;

// Erstelle Adresse (optional)
var address = new Address
{
    Name = "OI4 Nextbike",
    Strasse = "Musterstraße 123",
    Plz = "12345",
    Ort = "Musterstadt",
    Land = "Deutschland"
};

// Generiere PDF
var pdfData = PdfService.CreateHandoverPdf(address);

// Speichere als Datei
File.WriteAllBytes("handover.pdf", pdfData);
```

### In AAS-Integration

Die PDF-Generierung ist automatisch in die `CreateHandoverDocumentation` Methode integriert:

```csharp
// Wird automatisch aufgerufen bei der Erstellung einer Bike-Instanz
var aas = await InstanceAasCreator.CreateBikeInstanceAas(
    producedProduct,
    importService,
    settingsService
);
```

## Abhängigkeiten

- **iText7** (Version 8.0.5): PDF-Generierung
- **System.IO**: Dateioperation
- **AasCore.Aas3_0**: AAS-Integration

## PDF-Inhalt

Das generierte PDF enthält:

1. **Titel**: "Übergabe-Dokumentation"
2. **Datum**: Aktuelles Datum im Format dd.MM.yyyy
3. **Firmenadresse**:
   - Straße
   - PLZ und Ort
   - Land
4. **Produktbild**: Fahrradbild aus `src/images/IMG_4957.png`
5. **Footer**: Automatisch generiert Hinweis

## Fehlerbehandlung

- Falls das Bild nicht gefunden wird, wird ein Fehlermeldung ins PDF eingefügt
- Falls die Adresse fehlt, wird sie im PDF ausgelassen
- Robuste Behandlung von null-Werten

## Tests

Unit-Tests befinden sich in:

- `tests/Production/PdfServiceTests.cs`

## Konfiguration

Das Fahrradbild kann in `PdfService.cs` geändert werden:

```csharp
// Zeile 81 in PdfService.cs
"IMG_4957.png" // Andere Dateien aus dem images-Ordner verwenden
```

## Deployment

Nach Änderungen:

1. `dotnet build` ausführen
2. Neue iText7-Abhängigkeit wird automatisch installiert
3. PDF-Endpunkt ist über Swagger verfügbar
