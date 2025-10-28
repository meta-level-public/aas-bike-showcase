# AAS Proxy Service

Ein ASP.NET Core Web API Service, der URLs entgegennimmt und basierend auf konfigurierten URL-Präfixen Benutzer auf entsprechende AAS-Viewer-Seiten weiterleitet.

## Funktionalität

Der Service empfängt globalAssetIds und leitet diese basierend auf dem URL-Präfix an den entsprechenden AAS-Viewer weiter. Die Konfiguration erfolgt über einfache URL-Präfix-Mappings in den appsettings.

### Beispiel-Anfrage

```
GET https://dt.suppl.mydomain.de/test123
```

### Beispiel-Antwort

```
HTTP 302 Redirect zu:
https://viewer.aas-bike.showcasehub.de/?globalAssetId=https%3A//dt.suppl.mydomain.de/test123&aasRepositoryUrl=https%3A//basyx-suppl.lan&smRepositoryUrl=https%3A//basyx-suppl.lan
```

## Konfiguration

Die Konfiguration erfolgt über die Datei `appsettings.json`:

```json
{
  "ProxyConfiguration": {
    "ViewerBaseUrl": "https://viewer.aas-bike.showcasehub.de/",
    "UrlMappings": [
      {
        "UrlPrefix": "https://dt.suppl.mydomain.de",
        "AasRepositoryUrl": "https://basyx-suppl.lan",
        "SmRepositoryUrl": "https://basyx-suppl.lan",
        "DiscoveryUrl": "https://discovery-suppl.lan",
        "AasRegistryUrl": "https://aas-registry-suppl.lan",
        "CdRepositoryUrl": "https://cd-repo-suppl.lan",
        "SmRegistryUrl": "https://sm-registry-suppl.lan"
      },
      {
        "UrlPrefix": "https://dt.factory.mydomain.de",
        "AasRepositoryUrl": "https://basyx-factory.lan",
        "SmRepositoryUrl": "https://basyx-factory.lan"
      }
    ],
    "FallbackMapping": {
      "GlobalAssetId": "https://dt.default.mydomain.de/default",
      "AasRepositoryUrl": "https://basyx-default.lan",
      "SmRepositoryUrl": "https://basyx-default.lan",
      "DiscoveryUrl": "https://discovery-default.lan",
      "AasRegistryUrl": "https://aas-registry-default.lan",
      "CdRepositoryUrl": "https://cd-repo-default.lan",
      "SmRegistryUrl": "https://sm-registry-default.lan"
    }
  }
}
```

### Konfigurationsparameter

- **ViewerBaseUrl**: Die Basis-URL des AAS-Viewers
- **UrlMappings**: Array von URL-Präfix-Mapping-Regeln
  - **UrlPrefix**: URL-Präfix, mit dem die globalAssetId beginnen muss
  - **AasRepositoryUrl**: Repository-URL für AAS-Daten
  - **SmRepositoryUrl**: Repository-URL für Submodel-Daten
  - **DiscoveryUrl** (optional): AAS Discovery Service URL
  - **AasRegistryUrl** (optional): AAS Registry URL
  - **CdRepositoryUrl** (optional): Conceptdescription Repository URL
  - **SmRegistryUrl** (optional): Submodel Registry URL
- **FallbackMapping** (optional): Standard-Konfiguration für den Fall, dass kein Mapping gefunden wird
  - **GlobalAssetId**: Standard-GlobalAssetId, die verwendet wird
  - **AasRepositoryUrl**: Standard-Repository-URL für AAS-Daten
  - **SmRepositoryUrl**: Standard-Repository-URL für Submodel-Daten
  - **DiscoveryUrl** (optional): Standard-AAS Discovery Service URL
  - **AasRegistryUrl** (optional): Standard-AAS Registry URL
  - **CdRepositoryUrl** (optional): Standard-Conceptdescription Repository URL
  - **SmRegistryUrl** (optional): Standard-Submodel Registry URL

### Funktionsweise

Der Service prüft für jede eingehende globalAssetId, ob diese mit einem der konfigurierten `UrlPrefix`-Werte beginnt. Wenn ein Match gefunden wird, werden die zugehörigen `AasRepositoryUrl` und `SmRepositoryUrl` verwendet, um die Redirect-URL zum Viewer zu erstellen.

Wenn kein passendes Mapping gefunden wird und ein `FallbackMapping` konfiguriert ist, wird stattdessen die Fallback-Konfiguration verwendet. In diesem Fall wird die in `FallbackMapping.GlobalAssetId` konfigurierte ID anstelle der ursprünglich angefragten ID verwendet.

### Docker-Compose Konfiguration

Die Konfiguration kann über Umgebungsvariablen gesetzt werden:

```yaml
environment:
  - ProxyConfiguration__ViewerBaseUrl=https://viewer.aas-bike.showcasehub.de/
  - ProxyConfiguration__UrlMappings__0__UrlPrefix=https://dt.suppl.mydomain.de
  - ProxyConfiguration__UrlMappings__0__AasRepositoryUrl=https://basyx-suppl.lan
  - ProxyConfiguration__UrlMappings__0__SmRepositoryUrl=https://basyx-suppl.lan
  - ProxyConfiguration__UrlMappings__0__DiscoveryUrl=https://discovery-suppl.lan
  - ProxyConfiguration__UrlMappings__0__AasRegistryUrl=https://aas-registry-suppl.lan
  - ProxyConfiguration__UrlMappings__0__CdRepositoryUrl=https://cd-repo-suppl.lan
  - ProxyConfiguration__UrlMappings__0__SmRegistryUrl=https://sm-registry-suppl.lan
  - ProxyConfiguration__FallbackMapping__GlobalAssetId=https://dt.default.mydomain.de/default
  - ProxyConfiguration__FallbackMapping__AasRepositoryUrl=https://basyx-default.lan
  - ProxyConfiguration__FallbackMapping__SmRepositoryUrl=https://basyx-default.lan
  - ProxyConfiguration__FallbackMapping__DiscoveryUrl=https://discovery-default.lan
  - ProxyConfiguration__FallbackMapping__AasRegistryUrl=https://aas-registry-default.lan
  - ProxyConfiguration__FallbackMapping__CdRepositoryUrl=https://cd-repo-default.lan
  - ProxyConfiguration__FallbackMapping__SmRegistryUrl=https://sm-registry-default.lan
```

- ProxyConfiguration**FallbackMapping**SmRepositoryUrl=https://basyx-default.lan

````

## API-Endpunkte

### `GET /{**catchall}`

Hauptendpoint für die Proxy-Funktionalität.

**URL-Format:** `/globalAssetId`

Der Pfad nach dem Host wird als globalAssetId interpretiert.

**Antworten:**

- `302 Redirect`: Erfolgreiche Weiterleitung
- `400 Bad Request`: Fehlender Pfad in der Request
- `404 Not Found`: Keine passende Konfiguration gefunden

### `GET /health`

Gesundheitscheck-Endpoint.

**Antwort:**

```json
{
  "Status": "Healthy",
  "Timestamp": "2025-10-20T10:30:00Z"
}
````

### `GET /config`

Zeigt die aktuelle Konfiguration (nur für Debugging).

## Entwicklung

### Voraussetzungen

- .NET 9.0 oder höher

### Projekt starten

```bash
cd src/AasProxyService
dotnet run
```

Der Service ist dann unter `https://localhost:5001` oder `http://localhost:5000` verfügbar.

### Swagger UI

In der Entwicklungsumgebung ist Swagger UI verfügbar unter:
`https://localhost:5001/swagger`

## Beispiele

### Erfolgreiche Weiterleitung

```bash
curl -i "http://localhost:5168" -H "Host: dt.suppl.mydomain.de"
```

### Mit Query-String

```bash
curl -i "http://localhost:5168/test123?id=0001" -H "Host: dt.factory.mydomain.de"
```

### Gesundheitscheck

```bash
curl http://localhost:5168/health
```

### Konfiguration anzeigen

```bash
curl http://localhost:5168/config
```
