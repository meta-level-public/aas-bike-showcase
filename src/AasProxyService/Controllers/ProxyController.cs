using System.Web;
using AasProxyService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AasProxyService.Controllers
{
    [ApiController]
    [Route("")]
    public class ProxyController : ControllerBase
    {
        private readonly IProxyService _proxyService;
        private readonly ILogger<ProxyController> _logger;

        public ProxyController(IProxyService proxyService, ILogger<ProxyController> logger)
        {
            _proxyService = proxyService;
            _logger = logger;
        }

        /// <summary>
        /// Leitet Anfragen basierend auf der Request-URL weiter
        /// Die vollständige Request-URL wird als globalAssetId behandelt
        /// </summary>
        /// <returns>Redirect zur entsprechenden Viewer-URL</returns>
        [HttpGet("{**catchall}")]
        public IActionResult RedirectToViewer(string? catchall)
        {
            // Konstruiere die vollständige Request-URL
            var request = HttpContext.Request;
            var scheme = request.Scheme;
            var host = request.Host.Value;
            var path = request.Path.Value;
            var queryString = request.QueryString.Value;

            var globalAssetId = $"{scheme}://{host}{path}{queryString}";

            _logger.LogInformation(
                "Processing request URL as globalAssetId: {GlobalAssetId} (Scheme: {Scheme}, ForwardedProto: {ForwardedProto})",
                globalAssetId,
                scheme,
                request.Headers["X-Forwarded-Proto"].ToString()
            );

            var redirectUrl = _proxyService.GetRedirectUrl(globalAssetId);

            if (string.IsNullOrEmpty(redirectUrl))
            {
                _logger.LogWarning(
                    "No matching configuration found for globalAssetId: {GlobalAssetId}",
                    globalAssetId
                );
                return NotFound(
                    $"No configuration found for the provided globalAssetId: {globalAssetId}"
                );
            }

            _logger.LogInformation("Redirecting to: {RedirectUrl}", redirectUrl);
            return Redirect(redirectUrl);
        }

        /// <summary>
        /// Gesundheitscheck-Endpoint
        /// </summary>
        /// <returns>OK wenn der Service läuft</returns>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Zeigt die aktuelle Konfiguration (für Debugging)
        /// </summary>
        /// <returns>Aktuelle Proxy-Konfiguration</returns>
        [HttpGet("config")]
        public IActionResult GetConfiguration()
        {
            // In Produktionsumgebung sollte dieser Endpoint entfernt oder geschützt werden
            return Ok(_proxyService);
        }
    }
}
