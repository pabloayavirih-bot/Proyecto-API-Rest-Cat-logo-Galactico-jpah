namespace CatalogoGalactico.Models;

public class Personaje
{
    public int Id { get; init; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string Faccion { get; set; } = "Neutral";
    public string Afiliacion { get; set; } = string.Empty;
    public string Estado { get; set; } = "desconocido";
    public bool FuerzaSensitivo { get; set; }
}
