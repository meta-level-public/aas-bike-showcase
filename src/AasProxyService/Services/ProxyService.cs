using System.Web;
using AasProxyService.Configuration;
using Microsoft.Extensions.Options;

namespace AasProxyService.Services
{
    public interface IProxyService
    {
        string? GetRedirectUrl(string globalAssetId);
    }

    public class ProxyService : IProxyService
    {
        private readonly ProxyConfiguration _configuration;
        private readonly ILogger<ProxyService> _logger;

        public ProxyService(
            IOptions<ProxyConfiguration> configuration,
            ILogger<ProxyService> logger
        )
        {
            _configuration = configuration.Value;
            _logger = logger;
        }

        public string? GetRedirectUrl(string globalAssetId)
        {
            try
            {
                _logger.LogInformation("Processing globalAssetId: {GlobalAssetId}", globalAssetId);

                // Normalisiere URLs für Schema-agnostisches Matching
                var normalizedGlobalAssetId = NormalizeUrl(globalAssetId);

                // Finde das passende URL-Mapping basierend auf dem Präfix
                var mapping = _configuration.UrlMappings.FirstOrDefault(m =>
                {
                    var normalizedPrefix = NormalizeUrl(m.UrlPrefix);
                    return normalizedGlobalAssetId.StartsWith(
                        normalizedPrefix,
                        StringComparison.OrdinalIgnoreCase
                    );
                });

                if (mapping != null)
                {
                    _logger.LogInformation(
                        "Found matching URL mapping with prefix: {UrlPrefix}",
                        mapping.UrlPrefix
                    );

                    // Baue die Redirect-URL zusammen
                    var redirectUrl = BuildRedirectUrl(
                        globalAssetId,
                        mapping.AasRepositoryUrl,
                        mapping.SmRepositoryUrl,
                        mapping.DiscoveryUrl,
                        mapping.AasRegistryUrl,
                        mapping.CdRepositoryUrl,
                        mapping.SmRegistryUrl
                    );

                    _logger.LogInformation("Generated redirect URL: {RedirectUrl}", redirectUrl);
                    return redirectUrl;
                }

                // Kein Mapping gefunden - prüfe auf Fallback-Konfiguration
                if (_configuration.FallbackMapping != null)
                {
                    _logger.LogInformation(
                        "No matching URL mapping found, using fallback configuration"
                    );

                    var fallbackUrl = BuildRedirectUrl(
                        _configuration.FallbackMapping.GlobalAssetId,
                        _configuration.FallbackMapping.AasRepositoryUrl,
                        _configuration.FallbackMapping.SmRepositoryUrl,
                        _configuration.FallbackMapping.DiscoveryUrl,
                        _configuration.FallbackMapping.AasRegistryUrl,
                        _configuration.FallbackMapping.CdRepositoryUrl,
                        _configuration.FallbackMapping.SmRegistryUrl
                    );

                    _logger.LogInformation(
                        "Generated fallback redirect URL: {RedirectUrl}",
                        fallbackUrl
                    );
                    return fallbackUrl;
                }

                _logger.LogWarning(
                    "No matching URL mapping found and no fallback configured for globalAssetId: {GlobalAssetId}",
                    globalAssetId
                );
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing globalAssetId: {GlobalAssetId}",
                    globalAssetId
                );
                return null;
            }
        }

        private string BuildRedirectUrl(
            string globalAssetId,
            string aasRepositoryUrl,
            string smRepositoryUrl,
            string? discoveryUrl = null,
            string? aasRegistryUrl = null,
            string? cdRepositoryUrl = null,
            string? smRegistryUrl = null
        )
        {
            var queryParams = new Dictionary<string, string>
            {
                ["globalAssetId"] = globalAssetId,
                ["aasRepositoryUrl"] = aasRepositoryUrl,
                ["smRepositoryUrl"] = smRepositoryUrl,
            };

            // Füge optionale Parameter nur hinzu, wenn sie gesetzt sind
            if (!string.IsNullOrEmpty(discoveryUrl))
            {
                queryParams["discoveryUrl"] = discoveryUrl;
            }

            if (!string.IsNullOrEmpty(aasRegistryUrl))
            {
                queryParams["aasRegistryUrl"] = aasRegistryUrl;
            }

            if (!string.IsNullOrEmpty(cdRepositoryUrl))
            {
                queryParams["cdRepositoryUrl"] = cdRepositoryUrl;
            }

            if (!string.IsNullOrEmpty(smRegistryUrl))
            {
                queryParams["smRegistryUrl"] = smRegistryUrl;
            }

            var queryString = string.Join(
                "&",
                queryParams.Select(kvp =>
                    $"{HttpUtility.UrlEncode(kvp.Key)}={HttpUtility.UrlEncode(kvp.Value)}"
                )
            );

            return $"{_configuration.ViewerBaseUrl}?{queryString}";
        }

        /// <summary>
        /// Normalisiert eine URL für Schema-agnostisches Matching
        /// Entfernt das Schema (http:// oder https://) für einheitliches Matching
        /// </summary>
        private string NormalizeUrl(string url)
        {
            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return url.Substring(8); // Entfernt "https://"
            }
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return url.Substring(7); // Entfernt "http://"
            }
            return url;
        }
    }
}
