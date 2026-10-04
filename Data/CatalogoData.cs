using CatalogoGalactico.Models;

namespace CatalogoGalactico.Data;

public class CatalogoData
{
    public List<Personaje> Personajes { get; } = new()
    {
        new Personaje
        {
            Id = 1,
            Nombre = "Luke Skywalker",
            Especie = "Humano",
            Faccion = "Rebelde",
            Afiliacion = "Alianza Rebelde",
            Estado = "vivo",
            FuerzaSensitivo = true
        },
        new Personaje
        {
            Id = 2,
            Nombre = "Darth Vader",
            Especie = "Humano",
            Faccion = "Imperio",
            Afiliacion = "Imperio Galáctico",
            Estado = "vivo",
            FuerzaSensitivo = true
        },
        new Personaje
        {
            Id = 3,
            Nombre = "Han Solo",
            Especie = "Humano",
            Faccion = "Rebelde",
            Afiliacion = "Alianza Rebelde",
            Estado = "vivo",
            FuerzaSensitivo = false
        },
        new Personaje
        {
            Id = 4,
            Nombre = "Chewbacca",
            Especie = "Wookiee",
            Faccion = "Rebelde",
            Afiliacion = "Alianza Rebelde",
            Estado = "vivo",
            FuerzaSensitivo = false
        },
        new Personaje
        {
            Id = 5,
            Nombre = "Yoda",
            Especie = "Desconocida",
            Faccion = "Neutral",
            Afiliacion = "Orden Jedi",
            Estado = "vivo",
            FuerzaSensitivo = true
        }
    };

    public List<CardPersonaje> Cartas { get; } = new()
    {
        new CardPersonaje { Id = 1, PersonajeId = 1, Poder = 92, HabilidadEspecial = "Fuerza", Arma = "Sable", NivelPeligrosidad = 9, ImagenUrl = "" },
        new CardPersonaje { Id = 2, PersonajeId = 2, Poder = 95, HabilidadEspecial = "Fuerza", Arma = "Sable", NivelPeligrosidad = 10, ImagenUrl = "" },
        new CardPersonaje { Id = 3, PersonajeId = 3, Poder = 76, HabilidadEspecial = "Estrategia", Arma = "Bláster", NivelPeligrosidad = 7, ImagenUrl = "" },
        new CardPersonaje { Id = 4, PersonajeId = 4, Poder = 82, HabilidadEspecial = "Fuerza física", Arma = "Ballesta", NivelPeligrosidad = 8, ImagenUrl = "" },
        new CardPersonaje { Id = 5, PersonajeId = 5, Poder = 98, HabilidadEspecial = "Fuerza", Arma = "Sable", NivelPeligrosidad = 10, ImagenUrl = "" }
    };

    public List<Evento> Eventos { get; } = new()
    {
        new Evento
        {
            Id = 1,
            Nombre = "Batalla de Yavin",
            Fecha = 0,
            Ubicacion = "Yavin",
            Descripcion = "Evento de referencia para la convención temporal del catálogo.",
            Participantes = new List<int> { 1, 2, 3 },
            Muertos = new List<int>()
        },
        new Evento
        {
            Id = 2,
            Nombre = "Encuentro galáctico",
            Fecha = 3,
            Ubicacion = "Sector exterior",
            Descripcion = "Evento de ejemplo para probar relaciones N:N.",
            Participantes = new List<int> { 1, 2, 4 },
            Muertos = new List<int>()
        }
    };

    public int SiguientePersonajeId => Personajes.Count == 0 ? 1 : Personajes.Max(x => x.Id) + 1;
    public int SiguienteCartaId => Cartas.Count == 0 ? 1 : Cartas.Max(x => x.Id) + 1;
    public int SiguienteEventoId => Eventos.Count == 0 ? 1 : Eventos.Max(x => x.Id) + 1;
}
