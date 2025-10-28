using System.Text.Json.Nodes;
using AasCore.Aas3_0;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Proxy;
using AasDemoapp.Settings;
using AasDemoapp.Utils;

namespace AasDemoapp.Jobs
{
    public class UpdateChecker : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<UpdateChecker> _logger;
        private readonly IServiceProvider _provider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AasDemoappContext? _context;
        private readonly ProxyService? _proxyService;
        private readonly SettingService? _settingsService;
        private Timer? _timer = null;

        public UpdateChecker(
            ILogger<UpdateChecker> logger,
            IServiceProvider provider,
            IServiceScopeFactory scopeFactory
        )
        {
            _logger = logger;
            _provider = provider;
            _scopeFactory = scopeFactory;
            var scope = scopeFactory.CreateScope();
            _context = scope.ServiceProvider.GetRequiredService<AasDemoappContext>();
            _proxyService = scope.ServiceProvider.GetService<ProxyService>();
            _settingsService = scope.ServiceProvider.GetService<SettingService>();
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StatisticCalculator running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            try
            {
                var count = Interlocked.Increment(ref executionCount);

                var now = DateTime.Today;
                var discovery = _settingsService?.GetSetting(SettingTypes.DiscoveryUrl);
                var aasRepo = _settingsService?.GetSetting(SettingTypes.AasRepositoryUrl);
                var securitySetting =
                    _settingsService?.GetSecuritySetting(SettingTypes.InfrastructureSecurity)
                    ?? new SecuritySetting();

                var allShellIds = _proxyService?.Discover(
                    discovery?.Value ?? "",
                    securitySetting,
                    string.Empty
                );

                _context
                    ?.KatalogEintraege.Where(k => k.RemoteRepositoryUrl != null)
                    .ToList()
                    .ForEach(async eintrag =>
                    {
                        try
                        {
                            using var client = HttpClientCreator.CreateHttpClient(securitySetting);
                            using HttpResponseMessage remoteResponse = await client.GetAsync(
                                eintrag.RemoteRepositoryUrl + $"/shells/{eintrag.AasId.ToBase64()}"
                            );
                            remoteResponse.EnsureSuccessStatusCode();
                            string remoteResponseBody =
                                await remoteResponse.Content.ReadAsStringAsync();
                            var remoteJsonNode = JsonNode.Parse(remoteResponseBody);
                            if (remoteJsonNode == null)
                            {
                                _logger.LogWarning(
                                    "Failed to parse remote response JSON for shell {ShellId}",
                                    eintrag.AasId
                                );
                                return;
                            }
                            var remoteShell = Jsonization.Deserialize.AssetAdministrationShellFrom(
                                remoteJsonNode
                            );

                            using HttpResponseMessage localResponse = await client.GetAsync(
                                $"{aasRepo?.Value ?? ""}/shells/{eintrag.LocalAasId.ToBase64()}"
                            );
                            localResponse.EnsureSuccessStatusCode();
                            string localResponseBody =
                                await localResponse.Content.ReadAsStringAsync();
                            var localJsonNode = JsonNode.Parse(localResponseBody);
                            if (localJsonNode == null)
                            {
                                _logger.LogWarning(
                                    "Failed to parse local response JSON for shell {ShellId}",
                                    eintrag.LocalAasId
                                );
                                return;
                            }
                            var localShell = Jsonization.Deserialize.AssetAdministrationShellFrom(
                                localJsonNode
                            );

                            // Console.WriteLine(localShell.Administration?.Version);
                            // Console.WriteLine(remoteShell.Administration?.Version);
                            // Console.WriteLine(localShell.Administration?.Revision);
                            // Console.WriteLine(remoteShell.Administration?.Revision);

                            if (
                                localShell.Administration != null
                                && remoteShell.Administration != null
                                && (
                                    localShell.Administration.Version
                                        != remoteShell.Administration.Version
                                    || remoteShell.Administration.Revision
                                        != localShell.Administration.Revision
                                )
                            )
                            {
                                // es giibt was zu aktualisieren
                                if (
                                    !_context.UpdateableShells.Any(s =>
                                        s.KatalogEintrag.Id == eintrag.Id
                                    )
                                )
                                {
                                    var upddateEintrag = new UpdateableShell()
                                    {
                                        KatalogEintrag = eintrag,
                                        UpdateFoundTimestamp = DateTime.Now,
                                    };
                                    _context.Add(upddateEintrag);
                                    _context.SaveChanges();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Fehler bei Updatecheck");
                            Console.WriteLine(ex);
                        }
                    });
                _logger.LogInformation("UpdateChecker is working. Count: {Count}", count);
            }
            catch
            {
                /**/
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StatisticCalculator is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
