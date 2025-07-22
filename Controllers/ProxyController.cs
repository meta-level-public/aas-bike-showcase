using System.Web;
using Microsoft.AspNetCore.Mvc;
using AasCore.Aas3_0;
using AasDemoapp.Utils;
using IO.Swagger.Model;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using AasDemoapp.Import;
using System.Text.Json.Nodes;
using AasDemoapp.Proxy;

namespace AasDemoapp.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ProxyController : ControllerBase
{
    private readonly ImportService _importService;
    private readonly ProxyService _proxyService;

    public ProxyController(ImportService importService, ProxyService proxyService)
    {
        _importService = importService;
        _proxyService = proxyService;
    }

    [HttpGet]
    public async Task<object> Shells(string registryUrl)
    {
        var decodedUrl = HttpUtility.UrlDecode(registryUrl);

        var client = new HttpClient();
        using HttpResponseMessage response = await client.GetAsync(decodedUrl + "/shells");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(responseBody);
    }

    [HttpGet]
    public async Task<object> Registry(string registryUrl)
    {
        var decodedUrl = HttpUtility.UrlDecode(registryUrl);

        var client = new HttpClient();
        using HttpResponseMessage response = await client.GetAsync(decodedUrl + "/shell-descriptors");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(responseBody);
    }

    [HttpGet]
    public async Task<object> Shell(string registryUrl, string? aasId)
    {
        var decodedUrl = HttpUtility.UrlDecode(registryUrl);
        var decodedId = HttpUtility.UrlDecode(aasId) ?? string.Empty;

        var client = new HttpClient();
        using HttpResponseMessage response = await client.GetAsync(decodedUrl  + $"/shells/{decodedId?.ToBase64()}");
        response.EnsureSuccessStatusCode();
        string responseBody = await response.Content.ReadAsStringAsync();

        return await Task.FromResult(responseBody);
    }

    [HttpGet]
    public async Task<object> Discovery(string registryUrl, string? assetId)
    {
        var decodedUrl = HttpUtility.UrlDecode(registryUrl);
        var res = await _proxyService.Discover(decodedUrl, assetId);
        return res;
    }


    [HttpGet]
    public async Task<bool> Import(string localRegistryUrl, string remoteRegistryUrl, string id)
    {
        var decodedLocalUrl = HttpUtility.UrlDecode(localRegistryUrl);
        var decodedRemoteUrl = HttpUtility.UrlDecode(remoteRegistryUrl);
        var decodedId = HttpUtility.UrlDecode(id);

        await _importService.ImportFromRepository(decodedLocalUrl, decodedRemoteUrl, decodedId);


        return await Task.FromResult(true);
    }



}
