namespace CatalogoGalactico.Models;

public record PersonajeInput(
    string Nombre,
    string Especie,
    string Faccion,
    string Afiliacion,
    string Estado,
    bool FuerzaSensitivo,
    string? Image = null
);

public record CardPersonajeInput(
    int PersonajeId,
    int Poder,
    string HabilidadEspecial,
    string Arma,
    int NivelPeligrosidad,
    string ImagenUrl
);

public record EventoInput(
    string Nombre,
    int Fecha,
    string Ubicacion,
    string Descripcion,
    List<int> Participantes,
    List<int>? Muertos
);
