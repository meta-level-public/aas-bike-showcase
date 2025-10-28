namespace AasProxyService.Configuration
{
    public class ProxyConfiguration
    {
        public string ViewerBaseUrl { get; set; } = string.Empty;
        public List<UrlMapping> UrlMappings { get; set; } = new();

        /// <summary>
        /// Fallback-Konfiguration, die verwendet wird, wenn keine passende UrlMapping gefunden wird
        /// </summary>
        public FallbackMapping? FallbackMapping { get; set; }
    }

    public class UrlMapping
    {
        /// <summary>
        /// URL-Präfix, mit dem die globalAssetId beginnen muss
        /// </summary>
        public string UrlPrefix { get; set; } = string.Empty;

        /// <summary>
        /// AAS Repository URL für dieses Präfix
        /// </summary>
        public string AasRepositoryUrl { get; set; } = string.Empty;

        /// <summary>
        /// Submodel Repository URL für dieses Präfix
        /// </summary>
        public string SmRepositoryUrl { get; set; } = string.Empty;

        /// <summary>
        /// AAS Discovery URL für dieses Präfix
        /// </summary>
        public string? DiscoveryUrl { get; set; }

        /// <summary>
        /// AAS Registry URL für dieses Präfix
        /// </summary>
        public string? AasRegistryUrl { get; set; }

        /// <summary>
        /// Conceptdescription Repository URL für dieses Präfix
        /// </summary>
        public string? CdRepositoryUrl { get; set; }

        /// <summary>
        /// Submodel Registry URL für dieses Präfix
        /// </summary>
        public string? SmRegistryUrl { get; set; }
    }

    public class FallbackMapping
    {
        /// <summary>
        /// Standard-GlobalAssetId, die verwendet wird, wenn kein Mapping gefunden wird
        /// </summary>
        public string GlobalAssetId { get; set; } = string.Empty;

        /// <summary>
        /// Standard-AAS Repository URL
        /// </summary>
        public string AasRepositoryUrl { get; set; } = string.Empty;

        /// <summary>
        /// Standard-Submodel Repository URL
        /// </summary>
        public string SmRepositoryUrl { get; set; } = string.Empty;

        /// <summary>
        /// Standard-AAS Discovery URL
        /// </summary>
        public string? DiscoveryUrl { get; set; }

        /// <summary>
        /// Standard-AAS Registry URL
        /// </summary>
        public string? AasRegistryUrl { get; set; }

        /// <summary>
        /// Standard-Conceptdescription Repository URL
        /// </summary>
        public string? CdRepositoryUrl { get; set; }

        /// <summary>
        /// Standard-Submodel Registry URL
        /// </summary>
        public string? SmRegistryUrl { get; set; }
    }
}
