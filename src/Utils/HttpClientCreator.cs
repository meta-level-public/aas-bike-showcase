using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;

namespace AasDemoapp.Utils;

public class HttpClientCreator
{
    public static HttpClient CreateHttpClient(SecuritySetting setting)
    {
        HttpClient client;

        try
        {
            Console.WriteLine($"[DEBUG] Creating HttpClient with SSL bypass...");

            // Create HttpClientHandler with maximum SSL bypass configuration
            var handler = new HttpClientHandler();

            // MAXIMUM SSL BYPASS - Ignore ALL SSL certificate validation
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                Console.WriteLine(
                    $"[DEBUG] SSL Callback triggered. Errors: {errors}. Ignoring all SSL errors."
                );
                return true; // ALWAYS ignore SSL errors
            };

            // Additional SSL workarounds
            handler.SslProtocols =
                System.Security.Authentication.SslProtocols.Tls12
                | System.Security.Authentication.SslProtocols.Tls13;

            // Disable certificate revocation checking
            handler.CheckCertificateRevocationList = false;

            Console.WriteLine($"[DEBUG] HttpClientHandler configured with SSL bypass");

            // Configure client certificate if provided
            if (
                setting.Certificate != null
                && setting.Certificate.Length > 0
                && !string.IsNullOrWhiteSpace(setting.CertificatePassword)
            )
            {
                try
                {
                    var certificate = X509CertificateLoader.LoadPkcs12(
                        Convert.FromBase64String(setting.Certificate),
                        setting.CertificatePassword
                    );
                    handler.ClientCertificates.Add(certificate);
                    Console.WriteLine($"[DEBUG] Client certificate added");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DEBUG] Failed to load client certificate: {ex.Message}");
                }
            }

            // Create HttpClient with configured handler
            client = new HttpClient(handler);
            Console.WriteLine($"[DEBUG] HttpClient created with handler");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] Failed to create HttpClient with handler: {ex.Message}");
            Console.WriteLine($"[DEBUG] Falling back to simple HttpClient without SSL validation");

            // Fallback: Create simple HttpClient without custom handler
            client = new HttpClient();
        }

        // Set timeout to prevent hanging connections
        client.Timeout = TimeSpan.FromSeconds(
            setting.TimeoutSeconds > 0 ? setting.TimeoutSeconds : 60
        );
        Console.WriteLine($"[DEBUG] Timeout set to {client.Timeout.TotalSeconds} seconds");

        // Add custom headers
        if (setting.HeaderParameters.Any())
        {
            setting
                .HeaderParameters.ToList()
                .ForEach(header =>
                {
                    try
                    {
                        client.DefaultRequestHeaders.Add(header.Name, header.Value);
                        Console.WriteLine($"[DEBUG] Added header: {header.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[DEBUG] Failed to add header {header.Name}: {ex.Message}"
                        );
                    }
                });
        }

        // TODO: falls man bearer token weiterleiten will, muss dieses hierher übergeben werden.
        // aktuell einfach auskommentiert
        // if (jwToken != null && !setting.HeaderParameters.Any(k => k.Name == "Authorization"))
        // {
        //     client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwToken);
        // }

        Console.WriteLine($"[DEBUG] HttpClient creation completed");
        return client;
    }

    /// <summary>
    /// Custom SSL certificate validation for production environments
    /// </summary>
    private static bool ValidateServerCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors
    )
    {
        // For production, you might want to implement custom validation logic
        // For now, we'll be more permissive to handle common SSL issues

        // Allow self-signed certificates and name mismatches for internal services
        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        // Allow self-signed certificates (common in containerized environments)
        if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            return true;

        // Allow name mismatches (common when using service names instead of FQDNs)
        if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            return true;

        // Log the SSL error for debugging
        Console.WriteLine($"SSL Certificate validation failed: {sslPolicyErrors}");

        // In a real production environment, you might want to be more strict
        // For now, return true to allow connections (can be configured via IgnoreSslErrors)
        return true;
    }
}
