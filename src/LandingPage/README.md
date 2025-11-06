# NextBike Landing Page

Angular Landing Page für das NextBike-Projekt mit Informationen zu Digital Product Passport, Projektarchitektur und teilnehmenden Unternehmen.

## 🚀 Technologie-Stack

- **Angular 20.3.9** - Moderne standalone component architecture
- **PrimeNG** - UI Component Library
- **@primeuix/themes** - Aura Theme mit Custom Branding (#76b82a)
- **Caddy** - Production Web Server für SPA

## 📦 Entwicklung

### Lokale Entwicklung starten

```bash
npm install
npm start
```

Die Anwendung läuft auf `http://localhost:4200`

### Build für Production

```bash
npm run build
```

Build-Output: `dist/landing-page/browser/`

## 🐳 Docker

### Docker Image bauen

```bash
docker build -t landing-page:latest .
```

### Docker Container starten

```bash
docker run -p 8080:80 landing-page:latest
```

Die Anwendung ist dann unter `http://localhost:8080` erreichbar.

### Docker Compose (optional)

```yaml
services:
  landing-page:
    image: ghcr.io/meta-level-public/landing-page:latest
    ports:
      - '8080:80'
    restart: unless-stopped
    healthcheck:
      test: ['CMD', 'wget', '--no-verbose', '--tries=1', '--spider', 'http://localhost:80/']
      interval: 30s
      timeout: 3s
      retries: 3
```

## 🎨 Design System

### Primärfarbe

- **Basis**: `#76b82a` (Grün)
- **Gradient**: `#76b82a` → `#5fa022`

### Komponenten

- PrimeNG Cards mit grünen Header-Gradienten
- Responsive Grid-Layouts
- System Fonts mit optimiertem Rendering
- Konsistente Farbgebung über alle Seiten

## 📄 Seiten

- **Home** (`/`) - Willkommensseite mit Übersicht
- **DPP** (`/dpp`) - Digital Product Passport Erklärung
- **Projekt** (`/projekt`) - NextBike Projektbeschreibung
- **Architektur** (`/projekt/architektur`) - Technische Architektur
- **Members** (`/members`) - Teilnehmende Unternehmen

## 🏗️ Architektur

### Standalone Components

Moderne Angular-Architektur ohne NgModules:

- `app.config.ts` - Application Configuration mit PrimeNG Theme
- `app.routes.ts` - Route Definitions
- `bootstrapApplication()` statt `platformBrowser().bootstrapModule()`

### Struktur

```
src/
├── app/
│   ├── components/
│   │   └── navigation/
│   ├── pages/
│   │   ├── home/
│   │   ├── dpp/
│   │   ├── projekt/
│   │   ├── projekt-architektur/
│   │   └── members/
│   ├── app.ts
│   ├── app.config.ts
│   └── app.routes.ts
├── public/
│   └── [logos, favicon]
└── styles.scss
```

## 🔄 CI/CD

Der Build wird automatisch in der GitHub Actions Pipeline ausgeführt:

- **Branch `main`**: `ghcr.io/meta-level-public/landing-page:latest`
- **Andere Branches**: `ghcr.io/meta-level-public/landing-page:preview`

## Angular CLI

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 20.3.9.

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
