# Traefik Forwarded Headers Setup

## Problem

Wenn Traefik als Reverse Proxy eingesetzt wird, terminiert er SSL/TLS und leitet die Anfragen als HTTP an den Backend-Container weiter. Dadurch geht die Information verloren, ob der ursprüngliche Request mit HTTP oder HTTPS erfolgte.

## Lösung

Traefik setzt automatisch `X-Forwarded-*` Header, die die ursprünglichen Request-Informationen enthalten:

- **X-Forwarded-Proto**: Das ursprüngliche Schema (http/https)
- **X-Forwarded-Host**: Der ursprüngliche Host
- **X-Forwarded-For**: Die Client-IP-Adresse

## Implementierung

### 1. ASP.NET Core Konfiguration

Die `ForwardedHeadersMiddleware` wurde in `Program.cs` konfiguriert:

```csharp
using Microsoft.AspNetCore.HttpOverrides;

// Forwarded Headers konfigurieren
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                               ForwardedHeaders.XForwardedProto |
                               ForwardedHeaders.XForwardedHost;

    // Vertraue allen Proxies (für Docker/Kubernetes)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ...

var app = builder.Build();

// WICHTIG: MUSS vor anderen Middlewares stehen!
app.UseForwardedHeaders();
```

### 2. Traefik Konfiguration

Traefik setzt die Forward-Headers standardmäßig. Optional kann dies explizit konfiguriert werden:

```yaml
# traefik.yml (statische Konfiguration)
entryPoints:
  web:
    address: ':80'
    forwardedHeaders:
      trustedIPs:
        - '0.0.0.0/0' # Oder spezifische IPs/Netze
  websecure:
    address: ':443'
    forwardedHeaders:
      trustedIPs:
        - '0.0.0.0/0'
```

### 3. Docker Compose Labels

Die Labels in `docker-compose.yaml` sind bereits korrekt konfiguriert:

```yaml
proxy:
  labels:
    - 'traefik.enable=true'
    - 'traefik.http.routers.proxy.rule=Host(`dt.aas-bike.showcasehub.de`)'
    - 'traefik.http.routers.proxy.entrypoints=web,websecure'
    - 'traefik.http.routers.proxy.tls.certresolver=letsencrypt'
    - 'traefik.http.routers.proxy.middlewares=redirect-to-https@file'
```

## Schema-Agnostisches URL-Matching

Zusätzlich wurde der `ProxyService` angepasst, um URLs schema-agnostisch zu matchen. Die `NormalizeUrl`-Methode entfernt das Schema (http:// oder https://) vor dem Vergleich:

```csharp
private string NormalizeUrl(string url)
{
    if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        return url.Substring(8);
    }
    if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
    {
        return url.Substring(7);
    }
    return url;
}
```

### Vorteile:

- ✅ URLs mit http:// matchen auch gegen https:// Konfigurationen
- ✅ Flexible Konfiguration unabhängig vom Schema
- ✅ Robustes Matching bei unterschiedlichen Traefik-Konfigurationen

## Logging & Debugging

Im `ProxyController` werden jetzt sowohl das aktuelle Schema als auch der `X-Forwarded-Proto` Header geloggt:

```csharp
_logger.LogInformation(
    "Processing request URL as globalAssetId: {GlobalAssetId} (Scheme: {Scheme}, ForwardedProto: {ForwardedProto})",
    globalAssetId,
    scheme,
    request.Headers["X-Forwarded-Proto"].ToString()
);
```

## Testen

### 1. Request mit HTTPS über Traefik:

```bash
curl -v https://dt.aas-bike.showcasehub.de/ids/asset/4037_1649_7482_6178
```

Im Log sollte erscheinen:

```
Scheme: http
ForwardedProto: https
```

### 2. Direkter Request ohne Traefik:

```bash
curl -v http://localhost:8080/ids/asset/test
```

Im Log sollte erscheinen:

```
Scheme: http
ForwardedProto: (leer)
```

## Sicherheitshinweise

⚠️ **Produktion**: Die aktuelle Konfiguration vertraut allen Proxies (`KnownNetworks.Clear()` und `KnownProxies.Clear()`).

In einer Produktionsumgebung solltest du nur bekannte Proxy-IPs/Netze vertrauen:

```csharp
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;

    // Nur Traefik Container vertrauen
    options.KnownProxies.Add(IPAddress.Parse("172.18.0.5")); // Traefik IP
    // Oder Docker Netzwerk
    options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.18.0.0"), 16));
});
```

## Zusammenfassung

Mit dieser Konfiguration:

1. ✅ Traefik setzt automatisch `X-Forwarded-*` Header
2. ✅ ASP.NET Core wertet diese Header aus und aktualisiert `HttpContext.Request.Scheme`
3. ✅ URL-Matching funktioniert schema-agnostisch
4. ✅ Die ursprüngliche Schema-Information (http/https) bleibt erhalten
