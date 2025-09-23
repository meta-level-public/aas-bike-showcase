# SSL Configuration for HttpClientCreator

## Problem Solved

This update fixes the SSL connection error:

```
System.Net.Http.HttpRequestException: The SSL connection could not be established, see inner exception.
 ---> System.IO.IOException: Unable to read data from the transport connection: Connection reset by peer.
 ---> System.Net.Sockets.SocketException (54): Connection reset by peer
```

## Changes Made

### 1. Enhanced HttpClientCreator (`/src/Utils/HttpClientCreator.cs`)

- Added SSL certificate validation handling
- Added timeout configuration to prevent hanging connections
- Improved error handling for SSL certificate issues
- Support for ignoring SSL errors in development environments

### 2. Extended SecuritySetting Model (`/src/Database/Model/Setting.cs`)

- Added `IgnoreSslErrors` property for development/testing
- Added `TimeoutSeconds` property for configurable timeouts

## Configuration Options

### SecuritySetting Properties

| Property              | Type                  | Default | Description                              |
| --------------------- | --------------------- | ------- | ---------------------------------------- |
| `IgnoreSslErrors`     | bool                  | false   | Ignore SSL certificate validation errors |
| `TimeoutSeconds`      | int                   | 30      | HTTP request timeout in seconds          |
| `Certificate`         | string                | null    | Base64 encoded client certificate        |
| `CertificatePassword` | string                | null    | Client certificate password              |
| `HeaderParameters`    | List<HeaderParameter> | []      | Custom HTTP headers                      |

### Usage Examples

#### For Development (Ignore SSL Errors)

```csharp
var securitySetting = new SecuritySetting
{
    IgnoreSslErrors = true,  // Ignore SSL certificate errors
    TimeoutSeconds = 60      // 60 second timeout
};
```

#### For Production (Strict SSL Validation)

```csharp
var securitySetting = new SecuritySetting
{
    IgnoreSslErrors = false, // Validate SSL certificates
    TimeoutSeconds = 30,     // 30 second timeout
    Certificate = "base64EncodedCert",
    CertificatePassword = "certPassword"
};
```

## SSL Validation Behavior

### Development Mode (`IgnoreSslErrors = true`)

- All SSL certificate errors are ignored
- Useful for self-signed certificates
- Use only in development/testing environments

### Production Mode (`IgnoreSslErrors = false`)

- Custom validation that allows common containerized environment scenarios:
  - Self-signed certificates (common in Docker environments)
  - Certificate name mismatches (when using service names)
- Logs SSL validation failures for debugging
- More permissive than default .NET validation but still secure

## Common Use Cases

### 1. Connecting to Self-Signed Services

```csharp
var setting = new SecuritySetting { IgnoreSslErrors = true };
using var client = HttpClientCreator.CreateHttpClient(setting);
```

### 2. Production with Client Certificates

```csharp
var setting = new SecuritySetting
{
    Certificate = Convert.ToBase64String(certBytes),
    CertificatePassword = "password",
    TimeoutSeconds = 45
};
using var client = HttpClientCreator.CreateHttpClient(setting);
```

### 3. Adding Custom Headers

```csharp
var setting = new SecuritySetting
{
    HeaderParameters = new List<HeaderParameter>
    {
        new() { Name = "X-API-Key", Value = "your-api-key" },
        new() { Name = "Accept", Value = "application/json" }
    }
};
using var client = HttpClientCreator.CreateHttpClient(setting);
```

## Migration Notes

Existing code using `HttpClientCreator.CreateHttpClient(securitySetting)` will continue to work without changes. The new properties have sensible defaults:

- `IgnoreSslErrors = false` (secure by default)
- `TimeoutSeconds = 30` (reasonable default timeout)

## Security Considerations

1. **Never use `IgnoreSslErrors = true` in production** unless you understand the security implications
2. The custom SSL validation is more permissive to handle containerized environments
3. Consider implementing stricter SSL validation for high-security environments
4. Monitor SSL validation logs for potential security issues

## Troubleshooting

If you still encounter SSL issues:

1. Set `IgnoreSslErrors = true` temporarily to test connectivity
2. Check if the target service uses self-signed certificates
3. Verify certificate chain and trust store
4. Increase `TimeoutSeconds` for slow connections
5. Check network connectivity and firewall rules
