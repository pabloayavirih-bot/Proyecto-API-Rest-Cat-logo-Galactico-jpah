using CatalogoGalactico.Models;
using CatalogoGalactico.Services;

namespace CatalogoGalactico.Endpoints;

public static class PersonajeEndpoints
{
    public static void MapPersonajeEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/personajes").WithTags("Personajes");

        group.MapGet("/", (
            CatalogoService service,
            string? faccion = null,
            bool? fuerzaSensitivo = null,
            string? estado = null,
            string? search = null,
            int page = 1,
            int pageSize = 100) =>
        {
            var resultado = service.ListarPersonajes(
                faccion,
                fuerzaSensitivo,
                estado,
                search,
                page,
                pageSize
            );

            if (!resultado.Exitoso)
                return ApiResults.Desde(resultado);

            return Results.Ok(resultado.Datos!.Data);
        })
        .WithName("ListarPersonajes")
        .WithSummary("Lista personajes con filtros.")
        .Produces<IReadOnlyList<Personaje>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/ranking", (CatalogoService service, string por = "poder") =>
        {
            return ApiResults.Desde(service.ObtenerRanking(por));
        })
        .WithName("RankingPersonajes")
        .WithSummary("Ordena personajes según el poder de su carta.")
        .Produces<IReadOnlyList<RankingItem>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}/eventos", (int id, CatalogoService service) =>
        {
            return ApiResults.Desde(service.ObtenerEventosDePersonaje(id));
        })
        .WithName("EventosDePersonaje")
        .WithSummary("Devuelve los eventos relacionados con un personaje.")
        .Produces<IReadOnlyList<Evento>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/con-card", (int id, CatalogoService service) =>
        {
            return ApiResults.Desde(service.ObtenerPersonajeConCard(id));
        })
        .WithName("ObtenerPersonajeConCard")
        .WithSummary("Obtiene un personaje junto con su carta de combate.")
        .Produces<PersonajeConCardResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}", (int id, CatalogoService service) =>
        {
            return ApiResults.Desde(service.ObtenerPersonaje(id));
        })
        .WithName("ObtenerPersonaje")
        .WithSummary("Obtiene un personaje por id.")
        .Produces<Personaje>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", (PersonajeInput input, CatalogoService service) =>
        {
            var resultado = service.CrearPersonaje(input);
            var location = resultado.Datos is null
                ? "/api/v1/personajes"
                : $"/api/v1/personajes/{resultado.Datos.Id}";

            return ApiResults.Creado(resultado, location);
        })
        .WithName("CrearPersonaje")
        .WithSummary("Registra un personaje.")
        .Produces<Personaje>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", (int id, PersonajeInput input, CatalogoService service) =>
        {
            return ApiResults.Desde(service.ActualizarPersonaje(id, input));
        })
        .WithName("ActualizarPersonaje")
        .WithSummary("Actualiza un personaje.")
        .Produces<Personaje>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", (int id, CatalogoService service) =>
        {
            return ApiResults.SinContenido(service.EliminarPersonaje(id));
        })
        .WithName("EliminarPersonaje")
        .WithSummary("Elimina un personaje si no rompe relaciones existentes.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
