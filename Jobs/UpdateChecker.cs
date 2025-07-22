
using System.Text.Json.Nodes;
using AasCore.Aas3_0;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Proxy;
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
        private Timer? _timer = null;

        public UpdateChecker(ILogger<UpdateChecker> logger, IServiceProvider provider, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _provider = provider;
            _scopeFactory = scopeFactory;
            _context = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AasDemoappContext>();
            _proxyService = scopeFactory.CreateScope().ServiceProvider.GetService<ProxyService>();

        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("StatisticCalculator running.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void DoWork(object? state)
        {
            try
            {
                var count = Interlocked.Increment(ref executionCount);

                var now = DateTime.Today;
                var allShellIds = _proxyService.Discover("http://localhost:9421", string.Empty);

                _context.KatalogEintraege.Where(k => k.RemoteRepositoryUrl != null).ToList().ForEach(async eintrag =>
                {
                    try
                    {

                        var client = new HttpClient();
                        using HttpResponseMessage remoteResponse = await client.GetAsync(eintrag.RemoteRepositoryUrl + $"/shells/{eintrag.AasId.ToBase64()}");
                        remoteResponse.EnsureSuccessStatusCode();
                        string remoteResponseBody = await remoteResponse.Content.ReadAsStringAsync();
                        var remoteShell = Jsonization.Deserialize.AssetAdministrationShellFrom(JsonNode.Parse(remoteResponseBody));

                        using HttpResponseMessage localResponse = await client.GetAsync($"http://localhost:9421/shells/{eintrag.LocalAasId.ToBase64()}");
                        localResponse.EnsureSuccessStatusCode();
                        string localResposeBody = await localResponse.Content.ReadAsStringAsync();
                        var localShell = Jsonization.Deserialize.AssetAdministrationShellFrom(JsonNode.Parse(localResposeBody));

                        Console.WriteLine(localShell.Administration?.Version);
                        Console.WriteLine(remoteShell.Administration?.Version);
                        Console.WriteLine(localShell.Administration?.Revision);
                        Console.WriteLine(remoteShell.Administration?.Revision);

                        if (localShell.Administration != null && remoteShell.Administration != null && (localShell.Administration.Version != remoteShell.Administration.Version
                                                                                                        || remoteShell.Administration.Revision != localShell.Administration.Revision))
                        {
                            // es giibt was zu aktualisieren
                            if (!_context.UpdateableShells.Any(s => s.KatalogEintrag.Id == eintrag.Id))
                            {
                                var upddateEintrag = new UpdateableShell()
                                {
                                    KatalogEintrag = eintrag,
                                    UpdateFoundTimestamp = DateTime.Now
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
                _logger.LogInformation(
                    "UpdateChecker is working. Count: {Count}", count);
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