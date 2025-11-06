# NextBike Landing Page

Eine moderne Angular-Landingpage für das NextBike-Projekt - eine Demonstrationsplattform für die virtuelle Fabrik der Zukunft.

## Übersicht

Diese Anwendung präsentiert das NextBike-Projekt, das unter der Führung der Open Industry 4.0 Alliance (OI4) entwickelt wurde. Sie zeigt die vollständige digitale Wertschöpfungskette mittels der Asset Administration Shell (AAS).

## Technologie-Stack

- **Angular 20.3.9** - Aktuellstes Angular Framework
- **PrimeNG** - UI-Komponentenbibliothek
- **SCSS** - Styling
- **TypeScript** - Programmiersprache

## Projekt-Struktur

```
src/
├── app/
│   ├── components/
│   │   └── navigation/          # Haupt-Navigation
│   ├── pages/
│   │   ├── home/                # Startseite mit Überblick
│   │   ├── dpp/                 # Digital Product Passport Erklärung
│   │   ├── projekt/             # Projektbeschreibung
│   │   ├── projekt-architektur/ # Technische Architektur
│   │   └── members/             # Projekt-Member
│   ├── app-module.ts
│   └── app-routing-module.ts
├── styles.scss                   # Globale Styles
└── index.html
```

## Seiten

### Home

Startseite mit Überblick und Navigationskarten zu den Hauptbereichen.

### DPP (Digital Product Passport)

Erklärt das Konzept des Digital Product Passports und seine Bedeutung für Industrie 4.0.

### Projekt

Beschreibt das NextBike-Projekt, die virtuelle Fabrik, alle Komponenten der Wertschöpfungskette und die Vorgängerprojekte "Transfer of Ownership" und "Product Change Notification".

### Projekt/Architektur

Detaillierte technische Dokumentation der Systemarchitektur, Datenflüsse, Sicherheitskonzepte und Deployment-Strategien.

### Members

Präsentation der Mitgliedsunternehmen:

- Meta-Level Software AG
- Murr Elektronik
- MMC Müller Manufacturing Consulting
- Xitaso

## Installation & Start

### Voraussetzungen

- Node.js (Version 18 oder höher)
- npm

### Installation

```bash
cd src/LandingPage
npm install
```

### Entwicklungsserver starten

```bash
npm start
```

Die Anwendung ist dann unter `http://localhost:4200` erreichbar.

### Build für Produktion

```bash
npm run build
```

Die Build-Artefakte werden im `dist/` Verzeichnis erstellt.

## Features

✅ Moderne, responsive UI mit PrimeNG  
✅ Vollständige Navigation zwischen allen Seiten  
✅ Detaillierte Projekt- und Architekturdokumentation  
✅ Informationen zu allen Projekt-Membern  
✅ Vorbereitet für Logo-Integration  
✅ Clean Architecture mit Component-based Design

## Zukünftige Erweiterungen

- [ ] Firmenlogos zu den Members hinzufügen
- [ ] Weitere Member ergänzen
- [ ] Interaktive Architektur-Diagramme
- [ ] Mehrsprachigkeit (i18n)
- [ ] Dark Mode

## Entwickelt für

Open Industry 4.0 Alliance (OI4) und die NextBike-Projekt-Community

## Lizenz

Siehe Haupt-Repository
