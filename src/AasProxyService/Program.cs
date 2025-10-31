using AasProxyService.Configuration;
using AasProxyService.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Forwarded Headers Middleware konfigurieren (für Traefik/Reverse Proxy)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor
        | ForwardedHeaders.XForwardedProto
        | ForwardedHeaders.XForwardedHost;

    // Vertraue allen Proxies (für Docker/Kubernetes)
    // In Produktion sollte dies auf bekannte Proxy-IPs eingeschränkt werden
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Services hinzufügen - Konfiguration kommt aus appsettings.json
builder.Services.Configure<ProxyConfiguration>(
    builder.Configuration.GetSection("ProxyConfiguration")
);

builder.Services.AddScoped<IProxyService, ProxyService>();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS konfigurieren falls nötig
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// WICHTIG: Forwarded Headers MUSS vor anderen Middlewares stehen
app.UseForwardedHeaders();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();

app.MapControllers();

app.Run();
