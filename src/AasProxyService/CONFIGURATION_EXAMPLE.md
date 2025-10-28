# Beispielkonfiguration für AAS Proxy Service

Diese Datei zeigt eine vollständige Beispielkonfiguration für den AAS Proxy Service.

## Vollständige appsettings.json Beispiel

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ProxyConfiguration": {
    "ViewerBaseUrl": "https://viewer.aas-bike.showcasehub.de/",
    "UrlMappings": [
      {
        "UrlPrefix": "https://dt.suppl.aas-bike.showcasehub.de",
        "AasRepositoryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/shells",
        "SmRepositoryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/submodels",
        "DiscoveryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/lookup/shells",
        "AasRegistryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/shell-descriptors",
        "CdRepositoryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/concept-descriptions",
        "SmRegistryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/submodel-descriptors"
      },
      {
        "UrlPrefix": "https://dt.factory.aas-bike.showcasehub.de",
        "AasRepositoryUrl": "https://basyx-factory.aas-bike.showcasehub.de/shells",
        "SmRepositoryUrl": "https://basyx-factory.aas-bike.showcasehub.de/submodels",
        "DiscoveryUrl": "https://basyx-factory.aas-bike.showcasehub.de/lookup/shells",
        "AasRegistryUrl": "https://basyx-factory.aas-bike.showcasehub.de/shell-descriptors",
        "CdRepositoryUrl": "https://basyx-factory.aas-bike.showcasehub.de/concept-descriptions",
        "SmRegistryUrl": "https://basyx-factory.aas-bike.showcasehub.de/submodel-descriptors"
      }
    ],
    "FallbackMapping": {
      "GlobalAssetId": "https://dt.default.aas-bike.showcasehub.de/default-asset",
      "AasRepositoryUrl": "https://basyx-default.aas-bike.showcasehub.de/shells",
      "SmRepositoryUrl": "https://basyx-default.aas-bike.showcasehub.de/submodels",
      "DiscoveryUrl": "https://basyx-default.aas-bike.showcasehub.de/lookup/shells",
      "AasRegistryUrl": "https://basyx-default.aas-bike.showcasehub.de/shell-descriptors",
      "CdRepositoryUrl": "https://basyx-default.aas-bike.showcasehub.de/concept-descriptions",
      "SmRegistryUrl": "https://basyx-default.aas-bike.showcasehub.de/submodel-descriptors"
    }
  }
}
```

## Minimale Konfiguration (nur Pflichtfelder)

Wenn Sie die optionalen Felder nicht benötigen, können Sie auch eine minimale Konfiguration verwenden:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ProxyConfiguration": {
    "ViewerBaseUrl": "https://viewer.aas-bike.showcasehub.de/",
    "UrlMappings": [
      {
        "UrlPrefix": "https://dt.suppl.aas-bike.showcasehub.de",
        "AasRepositoryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/shells",
        "SmRepositoryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/submodels"
      }
    ]
  }
}
```

## Konfiguration ohne Fallback

Wenn kein Fallback benötigt wird, kann `FallbackMapping` weggelassen werden:

```json
{
  "ProxyConfiguration": {
    "ViewerBaseUrl": "https://viewer.aas-bike.showcasehub.de/",
    "UrlMappings": [
      {
        "UrlPrefix": "https://dt.suppl.aas-bike.showcasehub.de",
        "AasRepositoryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/shells",
        "SmRepositoryUrl": "https://basyx-suppl.aas-bike.showcasehub.de/submodels"
      }
    ]
  }
}
```

## Verwendung

1. Kopieren Sie eine der obigen Konfigurationen in Ihre `appsettings.json` oder `appsettings.Development.json`
2. Passen Sie die URLs an Ihre Umgebung an
3. Starten Sie den Service

## Beispiel-Anfragen

Mit der obigen Konfiguration würde eine Anfrage wie:

```
GET https://proxy.example.com/https://dt.suppl.aas-bike.showcasehub.de/asset-123
```

Weitergeleitet zu:

```
https://viewer.aas-bike.showcasehub.de/?globalAssetId=https%3A%2F%2Fdt.suppl.aas-bike.showcasehub.de%2Fasset-123&aasRepositoryUrl=https%3A%2F%2Fbasyx-suppl.aas-bike.showcasehub.de%2Fshells&smRepositoryUrl=https%3A%2F%2Fbasyx-suppl.aas-bike.showcasehub.de%2Fsubmodels&discoveryUrl=https%3A%2F%2Fbasyx-suppl.aas-bike.showcasehub.de%2Flookup%2Fshells&aasRegistryUrl=https%3A%2F%2Fbasyx-suppl.aas-bike.showcasehub.de%2Fshell-descriptors&cdRepositoryUrl=https%3A%2F%2Fbasyx-suppl.aas-bike.showcasehub.de%2Fconcept-descriptions&smRegistryUrl=https%3A%2F%2Fbasyx-suppl.aas-bike.showcasehub.de%2Fsubmodel-descriptors
```
