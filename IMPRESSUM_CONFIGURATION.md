# Impressum Configuration

Die Impressumsdaten können über Umgebungsvariablen konfiguriert werden. Dies gilt für beide Anwendungen: **AasDemoapp** und **LandingPage**.

## Umgebungsvariablen

- `IMPRESSUM_NAME`: Name des Betreibers/der verantwortlichen Person
- `IMPRESSUM_STREET`: Straße und Hausnummer
- `IMPRESSUM_POSTAL_CODE`: Postleitzahl
- `IMPRESSUM_CITY`: Stadt
- `IMPRESSUM_COUNTRY`: Land (Standard: "Deutschland")
- `IMPRESSUM_EMAIL`: Kontakt-E-Mail-Adresse
- `IMPRESSUM_PHONE`: Telefonnummer (optional)

## AasDemoapp (Backend mit API)

### Konfiguration in Docker Compose

Die Umgebungsvariablen sind in `docker/prod/docker-compose.yaml` bereits vorkonfiguriert:

```yaml
environment:
  - Impressum__Name=${IMPRESSUM_NAME:-Christina Bornträger}
  - Impressum__Street=${IMPRESSUM_STREET:-Hintereckstr. 10}
  - Impressum__PostalCode=${IMPRESSUM_POSTAL_CODE:-66606}
  - Impressum__City=${IMPRESSUM_CITY:-St. Wendel}
  - Impressum__Country=${IMPRESSUM_COUNTRY:-Deutschland}
  - Impressum__Email=${IMPRESSUM_EMAIL:-borntraegerchristina@gmail.com}
  - Impressum__Phone=${IMPRESSUM_PHONE:-}
```

### API-Endpoint

Die Impressumsdaten werden über den API-Endpoint `/api/Impressum` bereitgestellt und von der Frontend-Anwendung dynamisch geladen.

### Implementation

- **Backend**: `src/AasDemoapp/Controllers/ImpressumController.cs`
- **Frontend Service**: `src/AasDemoapp/ClientApp/src/app/impressum/impressum.service.ts`
- **Frontend Component**: `src/AasDemoapp/ClientApp/src/app/impressum/impressum.component.ts`

## LandingPage (Statische Angular-App)

### Konfiguration in Docker

Die LandingPage lädt die Impressumsdaten aus einer JSON-Datei, die beim Container-Start aus Umgebungsvariablen generiert wird.

Fügen Sie die Umgebungsvariablen in Ihrer Docker-Compose-Konfiguration hinzu:

```yaml
landingpage:
  image: your-landingpage-image
  environment:
    - IMPRESSUM_NAME=${IMPRESSUM_NAME:-Christina Bornträger}
    - IMPRESSUM_STREET=${IMPRESSUM_STREET:-Hintereckstr. 10}
    - IMPRESSUM_POSTAL_CODE=${IMPRESSUM_POSTAL_CODE:-66606}
    - IMPRESSUM_CITY=${IMPRESSUM_CITY:-St. Wendel}
    - IMPRESSUM_COUNTRY=${IMPRESSUM_COUNTRY:-Deutschland}
    - IMPRESSUM_EMAIL=${IMPRESSUM_EMAIL:-borntraegerchristina@gmail.com}
    - IMPRESSUM_PHONE=${IMPRESSUM_PHONE:-}
```

### Implementation

- **Config Generation Script**: `src/LandingPage/generate-impressum-config.sh`
- **Config File**: `/usr/share/caddy/assets/config/impressum.json` (wird zur Laufzeit generiert)
- **Frontend Service**: `src/LandingPage/src/app/pages/impressum/impressum.service.ts`
- **Frontend Component**: `src/LandingPage/src/app/pages/impressum/impressum.ts`

### Lokale Entwicklung

Für die lokale Entwicklung können Sie die Datei `src/LandingPage/public/assets/config/impressum.json` direkt bearbeiten.

## Verwendung mit .env-Datei

Erstellen Sie eine `.env`-Datei im `docker/prod`-Verzeichnis basierend auf `.env.example`:

```bash
cd docker/prod
cp .env.example .env
```

Passen Sie die Werte in der `.env`-Datei an Ihre Bedürfnisse an:

```env
IMPRESSUM_NAME=Ihr Name
IMPRESSUM_STREET=Ihre Straße 123
IMPRESSUM_POSTAL_CODE=12345
IMPRESSUM_CITY=Ihre Stadt
IMPRESSUM_COUNTRY=Deutschland
IMPRESSUM_EMAIL=ihre.email@example.com
IMPRESSUM_PHONE=+49 123 456789
```

## Vorteile dieser Lösung

1. **Keine Code-Änderungen erforderlich**: Impressumsdaten können ohne Rebuild der Container geändert werden
2. **Zentrale Konfiguration**: Dieselben Umgebungsvariablen für beide Anwendungen
3. **Sichere Defaults**: Fallback-Werte sind in der Konfiguration hinterlegt
4. **Einfache Wartung**: Änderungen nur in der `.env`-Datei oder Docker-Compose-Konfiguration
