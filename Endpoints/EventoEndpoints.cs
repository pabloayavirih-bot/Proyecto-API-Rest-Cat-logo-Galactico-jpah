using CatalogoGalactico.Models;
using CatalogoGalactico.Services;

namespace CatalogoGalactico.Endpoints;

public static class EventoEndpoints
{
    public static void MapEventoEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/eventos").WithTags("Eventos");

        group.MapGet("/", (
            CatalogoService service,
            int? participanteId = null,
            int? desde = null,
            int? hasta = null,
            int page = 1,
            int pageSize = 10) =>
        {
            return ApiResults.Desde(
                service.ListarEventos(participanteId, desde, hasta, page, pageSize));
        })
        .WithName("ListarEventos")
        .WithSummary("Lista eventos con filtros temporales, participante y paginación.")
        .Produces<PagedResponse<Evento>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}/mvp", (int id, CatalogoService service) =>
        {
            return ApiResults.Desde(service.ObtenerMvp(id));
        })
        .WithName("MvpEvento")
        .WithSummary("Devuelve el participante con mayor poder dentro del evento.")
        .Produces<MvpResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/simular", (int id, CatalogoService service) =>
        {
            return ApiResults.Desde(service.SimularEvento(id));
        })
        .WithName("SimularEvento")
        .WithSummary("Simula una batalla usando el poder de las cartas y un factor aleatorio acotado.")
        .Produces<SimulacionResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}", (int id, CatalogoService service) =>
        {
            return ApiResults.Desde(service.ObtenerEvento(id));
        })
        .WithName("ObtenerEvento")
        .WithSummary("Obtiene un evento por id.")
        .Produces<Evento>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", (EventoInput input, CatalogoService service) =>
        {
            var resultado = service.CrearEvento(input);
            var location = resultado.Datos is null
                ? "/api/v1/eventos"
                : $"/api/v1/eventos/{resultado.Datos.Id}";

            return ApiResults.Creado(resultado, location);
        })
        .WithName("CrearEvento")
        .WithSummary("Registra un evento y aplica las reglas temporales de participantes y muertes.")
        .Produces<Evento>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", (int id, EventoInput input, CatalogoService service) =>
        {
            return ApiResults.Desde(service.ActualizarEvento(id, input));
        })
        .WithName("ActualizarEvento")
        .WithSummary("Actualiza un evento conservando las reglas de consistencia temporal.")
        .Produces<Evento>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", (int id, CatalogoService service) =>
        {
            return ApiResults.SinContenido(service.EliminarEvento(id));
        })
        .WithName("EliminarEvento")
        .WithSummary("Elimina un evento que no tenga muertes registradas.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
