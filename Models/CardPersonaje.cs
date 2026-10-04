namespace CatalogoGalactico.Models;

public class CardPersonaje
{
    public int Id { get; init; }
    public int PersonajeId { get; set; }
    public int Poder { get; set; }
    public string HabilidadEspecial { get; set; } = string.Empty;
    public string Arma { get; set; } = string.Empty;
    public int NivelPeligrosidad { get; set; }
    public string ImagenUrl { get; set; } = string.Empty;
}
