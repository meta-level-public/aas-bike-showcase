using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AasDemoapp.Database.Model;

namespace AasDemoapp.Utils;

public class HttpClientCreator
{
    public static HttpClient CreateHttpClient(SecuritySetting setting)
    {
        var client = new HttpClient();
        if (
            setting.Certificate != null
            && setting.Certificate.Length > 0
            && !string.IsNullOrWhiteSpace(setting.CertificatePassword)
        )
        {
            var certificate = X509CertificateLoader.LoadPkcs12(
                Convert.FromBase64String(setting.Certificate),
                setting.CertificatePassword
            );

            // Create an HttpClientHandler and set the client certificate
            var handler = new HttpClientHandler();
            handler.ClientCertificates.Add(certificate);

            // Create an HttpClient using the handler
            client = new HttpClient(handler);
        }
        if (setting.HeaderParameters.Any())
        {
            setting
                .HeaderParameters.ToList()
                .ForEach(header =>
                {
                    client.DefaultRequestHeaders.Add(header.Name, header.Value);
                });
        }
        // TODO: falls man bearer token weiterleiten will, muss dieses hierher übergeben werden.
        // aktuell einfach auskommentiert
        // if (jwToken != null && !setting.HeaderParameters.Any(k => k.Name == "Authorization"))
        // {
        //     client.DefaultRequestHeaders.Add("Authorization", "Bearer " + jwToken);
        // }

        return client;
    }
}
