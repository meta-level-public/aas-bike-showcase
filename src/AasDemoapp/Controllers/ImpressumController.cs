using AasDemoapp.Settings;
using Microsoft.AspNetCore.Mvc;

namespace AasDemoapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImpressumController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ImpressumController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public ActionResult<ImpressumSettings> GetImpressum()
    {
        var impressum = new ImpressumSettings
        {
            Name = _configuration["Impressum:Name"] ?? string.Empty,
            Street = _configuration["Impressum:Street"] ?? string.Empty,
            PostalCode = _configuration["Impressum:PostalCode"] ?? string.Empty,
            City = _configuration["Impressum:City"] ?? string.Empty,
            Country = _configuration["Impressum:Country"] ?? "Deutschland",
            Email = _configuration["Impressum:Email"] ?? string.Empty,
            Phone = _configuration["Impressum:Phone"],
        };

        return Ok(impressum);
    }
}
