# AAS Bike Showcase - Digital Product Passport Demo

Eine Demo-Anwendung, die zeigt, wie mit Asset Administration Shells (AAS) ein Digital Product Passport (DPP) für Fahrräder implementiert werden kann.

## 🎯 Projektbeschreibung

Diese Showcase-Anwendung demonstriert die praktische Umsetzung von Digital Product Passports mittels Asset Administration Shells (AAS) in der Fahrradindustrie. Das System ermöglicht die vollständige Rückverfolgbarkeit von Fahrradkomponenten und deren Produktionsprozessen.

## 🏗️ Architektur

### Backend (.NET 9.0)

- **ASP.NET Core Web API** mit Angular SPA Proxy
- **Entity Framework Core** mit SQLite Datenbank
- **AAS Core 3.0** für Asset Administration Shell Funktionalität
- **Swagger/OpenAPI** Dokumentation

### Frontend (Angular 20)

- **Angular 20** Single Page Application
- **PrimeNG** UI-Komponenten
- **FontAwesome** Icons
- **AAS Core TypeScript** für AAS-Datenstrukturen

### AAS Infrastructure

- **AASX Server** für Asset Administration Shell Management
- **AAS Registry** für Discovery Services
- **Docker Compose** Setup für Infrastructure Services

## 🚀 Features

### Dashboard

- Übersicht über verfügbare AAS Updates
- Anzahl der verwalteten Asset Administration Shells
- Systemstatus und Monitoring

### Katalog-Management

- Verwaltung von Fahrradkomponenten
- Teile-Lookup und Verfügbarkeit
- Lieferanten-Integration

### Konfigurator

- Fahrradkonfiguration mit verfügbaren Komponenten
- Produktvarianten-Management
- Bestandsverwaltung

### Produktion

- Produktionsplanung und -ausführung
- Asset Administration Shell Generierung für produzierte Fahrräder
- Nameplate-Submodell Erstellung

### Digital Product Passport (DPP)

- Vollständige Produkthistorie
- Komponenten-Rückverfolgbarkeit
- AAS-basierte Datenstruktur

### Lieferanten-Integration

- Remote Repository Anbindung
- Asset Discovery Services
- Automatische AAS Synchronisation

## 🛠️ Installation & Setup

### Voraussetzungen

- .NET 9.0 SDK
- Node.js (v18+)
- Docker & Docker Compose

### Backend Setup

```bash
# Repository klonen
git clone <repository-url>
cd aas-bike-showcase/src

# Dependencies wiederherstellen
dotnet restore

# Datenbank migrieren
dotnet ef database update

# Backend starten
dotnet run
```

### Frontend Setup

```bash
# In ClientApp Verzeichnis wechseln
cd ClientApp

# Dependencies installieren
npm install

# Frontend starten (Development)
npm start
```

### Docker Infrastructure

```bash
# AAS Infrastructure starten
cd docker
docker-compose up -d
```

## 🧱 Build

Die Anwendung wird über ein Multi-Stage-Dockerfile gebaut. Du kannst dafür entweder das PowerShell-Skript verwenden oder den `docker build` Befehl direkt ausführen.

### Option A: PowerShell-Skript (empfohlen)

Voraussetzung: PowerShell 7 (pwsh) und Docker sind installiert.

```powershell
# Standard-Build (Release, finale Runtime-Stage, Image-Tag: aas-bike-showcase:local)
./build/build.ps1

# Beispiele
./build/build.ps1 -Configuration Debug
./build/build.ps1 -Target publish
./build/build.ps1 -Tag myrepo/aas-bike-showcase:dev
```

Parameter:

- `-Configuration`: `Debug` oder `Release` (Default: `Release`)
- `-Target`: `build` | `publish` | `final` (Default: `final`)
  - `build`: kompiliert (Zwischen-Stage)
  - `publish`: veröffentlicht Dateien (Publish-Stage)
  - `final`: erstellt das lauffähige Runtime-Image (Default)
- `-Tag`: Docker Image Tag (Default: `aas-bike-showcase:local`)

### Option B: Direkt per Docker

In zsh/Bash aus dem Repo-Root:

```bash
# Finale Runtime-Stage (empfohlen)
docker build -f build/Dockerfile \
	--build-arg BUILD_CONFIGURATION=Release \
	-t aas-bike-showcase:local .

# Nur bis zur Publish-Stage bauen
docker build -f build/Dockerfile \
	--build-arg BUILD_CONFIGURATION=Debug \
	--target publish \
	-t aas-bike-showcase:publish .
```

Nach erfolgreichem Build kannst du das Image starten:

```bash
docker run --rm -p 8080:8080 -p 8081:8081 aas-bike-showcase:local
```

Hinweise:

- Das Dockerfile befindet sich unter `build/Dockerfile` und kopiert den Code aus dem Repo-Root (`COPY . .`). Führe den Build daher aus dem Repo-Root aus.
- Für den lokalen Entwicklungsmodus des Frontends (Angular) ist kein Docker-Image erforderlich; siehe Abschnitt „Frontend Setup“.

## 🔧 Konfiguration

### AAS Services

- **AASX Server**: `http://localhost:9421`
- **AAS Registry**: konfigurierbar über Umgebungsvariablen
- **Backend API**: `https://localhost:7043`
- **Frontend SPA**: `https://localhost:42400`

### Datenbank

- SQLite Datenbank wird automatisch erstellt
- Migrations werden bei Startup ausgeführt
- Entwicklungsdaten können über Seeding geladen werden

## 📁 Projektstruktur

```text
├── Controllers/          # Web API Controller
├── ClientApp/            # Angular Frontend
├── Database/             # Entity Framework Modelle
├── Dashboard/            # Dashboard Services
├── Dpp/                  # Digital Product Passport
├── Import/               # AAS Import Services
├── Katalog/              # Produktkatalog
├── Konfigurator/         # Produktkonfigurator
├── Production/           # Produktionsmanagement
├── Proxy/                # AAS Proxy Services
├── Suppliers/            # Lieferantenintegration
├── docker/               # Docker Compose Setup
├── dockerfiles/          # Container Definitions
├── images/               # Produktbilder
└── Migrations/           # EF Core Migrations
```

## 🔗 API Endpoints

### Dashboard API

- `GET /api/dashboard/GetCountAvailableUpdateCount`
- `GET /api/dashboard/GetCountContainedShells`

### DPP API

- `GET /api/dpp/GetAll`

### Katalog API

- `GET /api/katalog/GetAll`
- `POST /api/katalog/RohteilLookup`

### Konfigurator API

- `GET /api/konfigurator/GetAll`
- `POST /api/konfigurator/Create`

### Produktion API

- `POST /api/production/Create`

## 🛡️ Technologien

### Backend

- .NET 9.0
- ASP.NET Core
- Entity Framework Core
- AasCore.Aas3_0
- NSwag (OpenAPI)
- SQLite

### Frontend

- Angular 20
- TypeScript
- PrimeNG
- FontAwesome
- AAS Core TypeScript

### Infrastructure

- Docker
- Docker Compose
- AASX Server
- AAS Registry

## 🤝 Contributing

1. Fork das Repository
2. Erstelle einen Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit deine Änderungen (`git commit -m 'Add some AmazingFeature'`)
4. Push zum Branch (`git push origin feature/AmazingFeature`)
5. Öffne einen Pull Request

## 📄 Lizenz

Dieses Projekt steht unter der [MIT Lizenz](LICENSE).

## 👥 Team

Entwickelt von Meta Level Software AG & Open Industry Alliance

## 📞 Support

Bei Fragen oder Problemen erstellen Sie bitte ein Issue im Repository.

## 🔮 Roadmap

- [ ] Erweiterte DPP Funktionalitäten
- [ ] Integration "echter" Viewer für die AAS
- [ ] Aufrufbarkeit über QR-Code
