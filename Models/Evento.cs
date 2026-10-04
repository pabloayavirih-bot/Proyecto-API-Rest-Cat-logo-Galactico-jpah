namespace CatalogoGalactico.Models;

public class Evento
{
    public int Id { get; init; }
    public string Nombre { get; set; } = string.Empty;

    // Convención usada por el proyecto:
    // BBY = valor negativo, Batalla de Yavin = 0, ABY = valor positivo.
    public int Fecha { get; set; }

    public string Ubicacion { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public List<int> Participantes { get; set; } = new();

    // Esta lista permite aplicar la regla de negocio de las muertes.
    public List<int> Muertos { get; set; } = new();

    public string? Resultado { get; set; }
    public string? Ganador { get; set; }
}
