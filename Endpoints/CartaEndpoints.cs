using CatalogoGalactico.Models;
using CatalogoGalactico.Services;

namespace CatalogoGalactico.Endpoints;

public static class CartaEndpoints
{
    public static void MapCartaEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/cartas").WithTags("Cartas");

        group.MapGet("/", (
            CatalogoService service,
            int? personajeId = null,
            int? minPoder = null,
            int page = 1,
            int pageSize = 10) =>
        {
            return ApiResults.Desde(service.ListarCartas(personajeId, minPoder, page, pageSize));
        })
        .WithName("ListarCartas")
        .WithSummary("Lista cartas con filtros y paginación.")
        .Produces<PagedResponse<CardPersonaje>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", (int id, CatalogoService service) =>
        {
            return ApiResults.Desde(service.ObtenerCarta(id));
        })
        .WithName("ObtenerCarta")
        .WithSummary("Obtiene una carta por id.")
        .Produces<CardPersonaje>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", (CardPersonajeInput input, CatalogoService service) =>
        {
            var resultado = service.CrearCarta(input);
            var location = resultado.Datos is null
                ? "/api/v1/cartas"
                : $"/api/v1/cartas/{resultado.Datos.Id}";

            return ApiResults.Creado(resultado, location);
        })
        .WithName("CrearCarta")
        .WithSummary("Crea la carta principal de un personaje.")
        .Produces<CardPersonaje>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", (int id, CardPersonajeInput input, CatalogoService service) =>
        {
            return ApiResults.Desde(service.ActualizarCarta(id, input));
        })
        .WithName("ActualizarCarta")
        .WithSummary("Actualiza una carta.")
        .Produces<CardPersonaje>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", (int id, CatalogoService service) =>
        {
            return ApiResults.SinContenido(service.EliminarCarta(id));
        })
        .WithName("EliminarCarta")
        .WithSummary("Elimina una carta.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound);
    }
}
