using CatalogoGalactico.Models;

namespace CatalogoGalactico.Endpoints;

public static class ApiResults
{
    public static IResult Desde<T>(ResultadoOperacion<T> resultado)
    {
        if (resultado.Exitoso)
            return Results.Ok(resultado.Datos);

        var titulo = resultado.Codigo switch
        {
            404 => "Recurso no encontrado",
            400 => "Solicitud inválida",
            _ => "Error"
        };

        return Results.Problem(
            statusCode: resultado.Codigo,
            title: titulo,
            detail: resultado.Error);
    }

    public static IResult Creado<T>(ResultadoOperacion<T> resultado, string location)
    {
        if (!resultado.Exitoso)
            return Desde(resultado);

        return Results.Created(location, resultado.Datos);
    }

    public static IResult SinContenido(ResultadoOperacion<bool> resultado)
    {
        if (!resultado.Exitoso)
            return Desde(resultado);

        return Results.NoContent();
    }
}
