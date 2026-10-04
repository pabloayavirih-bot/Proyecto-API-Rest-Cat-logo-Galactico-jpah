namespace CatalogoGalactico.Models;

public record PageMeta(
    int Page,
    int PageSize,
    int Total,
    int TotalPages
);

public record PagedResponse<T>(
    IReadOnlyList<T> Data,
    PageMeta Meta
);

public record RankingItem(
    int Posicion,
    int PersonajeId,
    string Nombre,
    string Faccion,
    int Poder
);

public record MvpResponse(
    int EventoId,
    int PersonajeId,
    string Nombre,
    int Poder
);

public record FuerzaBando(
    string Faccion,
    int PoderBase,
    double FactorAleatorio,
    double PoderFinal
);

public record SimulacionResponse(
    int EventoId,
    string Evento,
    string Ganador,
    string Resultado,
    string Criterio,
    IReadOnlyList<FuerzaBando> Totales
);

public record ResultadoOperacion<T>(
    int Codigo,
    T? Datos,
    string? Error
)
{
    public bool Exitoso => Codigo >= 200 && Codigo < 300;
}
