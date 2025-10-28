using AasDemoapp.Production;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class QrCodeController : ControllerBase
    {
        /// <summary>
        /// Generiert einen QR-Code mit Rahmen und Dreieck als PNG-Bild
        /// </summary>
        /// <param name="id">Die ID/Text für den QR-Code</param>
        /// <param name="pixelsPerModule">Optionale Größe (Pixel pro Modul, Standard: 10)</param>
        /// <returns>PNG-Bild des QR-Codes</returns>
        [HttpGet]
        public IActionResult GenerateWithFrame(
            [FromQuery] string id,
            [FromQuery] int pixelsPerModule = 10
        )
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new { error = "ID parameter is required" });
            }

            if (pixelsPerModule < 1 || pixelsPerModule > 50)
            {
                return BadRequest(new { error = "pixelsPerModule must be between 1 and 50" });
            }

            var qrCodeBytes = QrCodeService.GenerateQrCodeBytes(id, pixelsPerModule);

            if (qrCodeBytes == null)
            {
                return StatusCode(500, new { error = "Failed to generate QR code" });
            }

            return File(qrCodeBytes, "image/png", $"qrcode-{id}.png");
        }
    }
}
