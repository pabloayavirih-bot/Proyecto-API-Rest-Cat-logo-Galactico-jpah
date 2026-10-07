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

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Almacenamiento y lógica de negocio viven fuera de los endpoints.
builder.Services.AddSingleton<CatalogoData>();
builder.Services.AddSingleton<CatalogoService>();

var app = builder.Build();

app.Urls.Clear();
app.Urls.Add("http://localhost:5000");

app.UseCors("Frontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

// Todas las rutas públicas de esta versión comienzan con /api/v1.
var api = app.MapGroup("");

api.MapPersonajeEndpoints();
api.MapCartaEndpoints();
api.MapEventoEndpoints();

app.Run();
