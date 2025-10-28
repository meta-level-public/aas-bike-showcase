using System.Text.Json.Nodes;
using System.Web;
using AasCore.Aas3_0;
using AasDemoapp.Import;
using AasDemoapp.Proxy;
using AasDemoapp.Settings;
using AasDemoapp.Utils;
using IO.Swagger.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AasDemoapp.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ProxyController : ControllerBase
{
    private readonly ImportService _importService;
    private readonly ProxyService _proxyService;
    private readonly SettingService _settingService;

    public ProxyController(
        ImportService importService,
        ProxyService proxyService,
        SettingService settingService
    )
    {
        _importService = importService;
        _proxyService = proxyService;
        _settingService = settingService;
    }

    [HttpGet]
    public async Task<object> Shells(string registryUrl)
    {
        var decodedUrl = HttpUtility.UrlDecode(registryUrl);
        Console.WriteLine($"[DEBUG] ProxyController.Shells - Original URL: {registryUrl}");
        Console.WriteLine($"[DEBUG] ProxyController.Shells - Decoded URL: {decodedUrl}");
        Console.WriteLine($"[DEBUG] ProxyController.Shells - Target URL: {decodedUrl}/shells");

        var securitySetting = _settingService.GetSecuritySetting(
            SettingTypes.InfrastructureSecurity
        );

        try
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
            Console.WriteLine(
                $"[DEBUG] ProxyController.Shells - Making HTTP request to: {decodedUrl}/shells"
            );
            using HttpResponseMessage response = await client.GetAsync(decodedUrl + "/shells");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(
                $"[DEBUG] ProxyController.Shells - Request successful, response length: {responseBody.Length}"
            );

            return await Task.FromResult(responseBody);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("SSL connection"))
        {
            Console.WriteLine($"[DEBUG] ProxyController.Shells - SSL error occurred: {ex.Message}");

            // Try HTTP fallback if HTTPS fails
            var httpUrl = decodedUrl.Replace("https://", "http://");
            if (httpUrl != decodedUrl)
            {
                Console.WriteLine(
                    $"[DEBUG] ProxyController.Shells - Trying HTTP fallback: {httpUrl}/shells"
                );
                try
                {
                    using var client = HttpClientCreator.CreateHttpClient(securitySetting);
                    using HttpResponseMessage response = await client.GetAsync(httpUrl + "/shells");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[DEBUG] ProxyController.Shells - HTTP fallback successful");
                    return await Task.FromResult(responseBody);
                }
                catch (Exception fallbackEx)
                {
                    Console.WriteLine(
                        $"[DEBUG] ProxyController.Shells - HTTP fallback also failed: {fallbackEx.Message}"
                    );
                }
            }

            // Re-throw original exception if fallback doesn't work
            throw;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DEBUG] ProxyController.Shells - Unexpected error: {ex.Message}");
            throw;
        }
    }

    [HttpGet]
    public async Task<object> Registry(string registryUrl)
    {
        var decodedUrl = HttpUtility.UrlDecode(registryUrl);
        var securitySetting = _settingService.GetSecuritySetting(
            SettingTypes.InfrastructureSecurity
        );

        using var client = HttpClientCreator.CreateHttpClient(securitySetting);
        using HttpResponseMessage response = await client.GetAsync(
            decodedUrl + "/shell-descriptors"
        );
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(responseBody);
    }

    [HttpGet]
    public async Task<object> Shell(string registryUrl, string? aasId)
    {
        var decodedUrl = HttpUtility.UrlDecode(registryUrl);
        var decodedId = HttpUtility.UrlDecode(aasId) ?? string.Empty;
        var securitySetting = _settingService.GetSecuritySetting(
            SettingTypes.InfrastructureSecurity
        );

        using var client = HttpClientCreator.CreateHttpClient(securitySetting);
        using HttpResponseMessage response = await client.GetAsync(
            decodedUrl + $"/shells/{decodedId?.ToBase64()}"
        );
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(responseBody);
    }

    [HttpGet]
    public async Task<string[]> Discovery(string registryUrl, string? assetId)
    {
        if (string.IsNullOrEmpty(assetId))
        {
            return [];
        }

        var decodedUrl = HttpUtility.UrlDecode(registryUrl);
        var securitySetting = _settingService.GetSecuritySetting(
            SettingTypes.InfrastructureSecurity
        );
        var res = await _proxyService.Discover(decodedUrl, securitySetting, assetId);
        return res;
    }

    [HttpGet]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<bool> Import(string localRegistryUrl, string remoteRegistryUrl, string id)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        throw new NotImplementedException();
        // var decodedLocalUrl = HttpUtility.UrlDecode(localRegistryUrl);
        // var decodedRemoteUrl = HttpUtility.UrlDecode(remoteRegistryUrl);

        // // TODO: korrigieren
        // var katalogEintrag

        // var decodedId = HttpUtility.UrlDecode(id);
        // var securitySetting = _settingService.GetSecuritySetting(SettingTypes.InfrastructureSecurity);

        // await _importService.ImportFromRepository(decodedLocalUrl, decodedRemoteUrl, securitySetting, decodedId);

        // return await Task.FromResult(true);
    }
}
