using System.Text.Json;
using System.Text.Json.Serialization;
using AasDemoapp.Dashboard;
using AasDemoapp.Database;
using AasDemoapp.Import;
using AasDemoapp.Jobs;
using AasDemoapp.Katalog;
using AasDemoapp.Konfigurator;
using AasDemoapp.Production;
using AasDemoapp.Services.ProductionOrder;
using AasDemoapp.Proxy;
using AasDemoapp.Settings;
using AasDemoapp.Suppliers;
using AasDemoapp.ToolRepos;
using AasDemoapp.Dpp;
using AasDemoapp.Mapping;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerDocument(config =>
{
    config.DocumentName = "Internal API";
    config.Title = "Internal API";
    config.Description = "Internal API for interaction with frontend";
    config.PostProcess = document =>
    {
        document.Info.Version = "v1.0";
        document.Info.Contact = new NSwag.OpenApiContact
        {
            Name = "Meta Level Software AG"
        };
    };
    config.SchemaSettings.DefaultReferenceTypeNullHandling = NJsonSchema.Generation.ReferenceTypeNullHandling.NotNull;
});


// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });


builder.Services.AddDbContext<AasDemoappContext>(options =>
{
    options.UseSqlite();
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

//  builder.Services.AddScoped<AasDemoappContext>(provider => provider.GetService<AasDemoappContext>());

// AutoMapper registrieren
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<ImportService>();
builder.Services.AddScoped<KatalogService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<ToolRepoService>();
builder.Services.AddScoped<SettingService>();
builder.Services.AddScoped<ProxyService>();
builder.Services.AddScoped<KonfiguratorService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ProductionService>();
builder.Services.AddScoped<ProductionOrderService>();
builder.Services.AddScoped<DppService>();


builder.Services.AddHostedService<UpdateChecker>();


var app = builder.Build();

app.UseOpenApi();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();


// app.MapControllerRoute(
//     name: "default",
//     pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");



using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetService<AasDemoappContext>();

    dbContext?.Database.Migrate();
}

app.Run();