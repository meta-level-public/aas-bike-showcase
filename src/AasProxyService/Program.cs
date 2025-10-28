using AasProxyService.Configuration;
using AasProxyService.Services;

var builder = WebApplication.CreateBuilder(args);

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
