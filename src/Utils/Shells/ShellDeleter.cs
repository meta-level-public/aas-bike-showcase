using AasDemoapp.Database.Model;

namespace AasDemoapp.Utils.Shells
{
    public class ShellDeleter
    {
        private static ILogger<ShellDeleter>? _logger;

        static ShellDeleter()
        {
            // Dienstanbieter für Dependency Injection abrufen
            var serviceProvider = new ServiceCollection()
                .AddLogging(configure =>
                {
                    configure.AddConsole();
                    configure.AddDebug();
                })
                .BuildServiceProvider();

            // Logger initialisieren
            _logger = serviceProvider.GetService<ILogger<ShellDeleter>>();
        }

        public static async Task DeleteShell(AasUrls aasUrls, SecuritySetting securitySetting, EditorDescriptor editorDescriptor, CancellationToken cancellationToken)
        {
            using var client = HttpClientCreator.CreateHttpClient(securitySetting);

            var url = editorDescriptor.AasDescriptorEntry.Endpoint;
            var response = await client.DeleteAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogError("Error deleting AAS: {StatusCode}", response.StatusCode);
            }

            editorDescriptor.SubmodelDescriptorEntries.ForEach(async submodelDescriptorEntry =>
            {
                try
                {
                    url = submodelDescriptorEntry.Endpoint;
                    response = await client.DeleteAsync(url, cancellationToken);
                    if (!response.IsSuccessStatusCode)
                    {
                        _logger?.LogError("Error deleting Submodel: {StatusCode}", response.StatusCode);
                    }
                }
                catch (Exception e)
                {
                    _logger?.LogError(e, "Error deleting Submodel: {Message}", e.Message);
                }
            });

            // jetzt noch aus der Discovery entfernen
            try
            {
                await DiscoveryUpdater.RemoveFromDiscovery(aasUrls.DiscoveryUrl, editorDescriptor.AasDescriptorEntry.OldId, cancellationToken, client);
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error removing from Discovery: {Message}", e.Message);
            }

            // und registry aufräumen
            try
            {
                await RegistryUpdater.RemoveFromAasRegistryAsync(aasUrls.AasRegistryUrl, editorDescriptor.AasDescriptorEntry.OldId, cancellationToken, client);
                editorDescriptor.SubmodelDescriptorEntries.ForEach(async submodelDescriptorEntry =>
                {
                    try
                    {
                        await RegistryUpdater.RemoveFromSmRegistryAsync(aasUrls.SubmodelRegistryUrl, submodelDescriptorEntry.OldId, cancellationToken, client);
                    }
                    catch (Exception e)
                    {
                        _logger?.LogError(e, "Error removing from Submodel Registry: {Message}", e.Message);
                    }
                });
            }
            catch (Exception e)
            {
                _logger?.LogError(e, "Error removing from Registry: {Message}", e.Message);
            }
        }
    }
}