using CatalogoGalactico.Data;
using CatalogoGalactico.Endpoints;
using CatalogoGalactico.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "Catálogo Galáctico API",
        Version = "v1",
        Description = "API académica de personajes, cartas y eventos construida con ASP.NET Core Minimal API."
    });
});

// Almacenamiento y lógica de negocio viven fuera de los endpoints.
builder.Services.AddSingleton<CatalogoData>();
builder.Services.AddSingleton<CatalogoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

// Todas las rutas públicas de esta versión comienzan con /api/v1.
var api = app.MapGroup("/api/v1");

api.MapPersonajeEndpoints();
api.MapCartaEndpoints();
api.MapEventoEndpoints();

app.Run();
