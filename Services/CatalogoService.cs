using CatalogoGalactico.Data;
using CatalogoGalactico.Models;

namespace CatalogoGalactico.Services;

public class CatalogoService
{
    private readonly CatalogoData _data;

    private static readonly string[] FaccionesValidas = { "Rebelde", "Imperio", "Neutral" };
    private static readonly string[] EstadosValidos = { "vivo", "muerto", "desconocido" };

    public CatalogoService(CatalogoData data)
    {
        _data = data;
    }

    // ---------------- PERSONAJES ----------------

    public ResultadoOperacion<PagedResponse<Personaje>> ListarPersonajes(
        string? faccion,
        bool? fuerzaSensitivo,
        string? estado,
        string? search,
        int page,
        int pageSize)
    {
        var errorPaginacion = ValidarPaginacion(page, pageSize);
        if (errorPaginacion is not null)
            return Error<PagedResponse<Personaje>>(400, errorPaginacion);

        IEnumerable<Personaje> consulta = _data.Personajes;

        if (!string.IsNullOrWhiteSpace(faccion))
        {
            var faccionNormalizada = Normalizar(FaccionesValidas, faccion);
            if (faccionNormalizada is null)
                return Error<PagedResponse<Personaje>>(400, "La facción debe ser Rebelde, Imperio o Neutral.");

            consulta = consulta.Where(p => p.Faccion.Equals(faccionNormalizada, StringComparison.OrdinalIgnoreCase));
        }

        if (fuerzaSensitivo.HasValue)
            consulta = consulta.Where(p => p.FuerzaSensitivo == fuerzaSensitivo.Value);

        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoNormalizado = Normalizar(EstadosValidos, estado);
            if (estadoNormalizado is null)
                return Error<PagedResponse<Personaje>>(400, "El estado debe ser vivo, muerto o desconocido.");

            consulta = consulta.Where(p => p.Estado.Equals(estadoNormalizado, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            consulta = consulta.Where(p =>
                p.Nombre.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Especie.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                p.Afiliacion.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        consulta = consulta.OrderBy(p => p.Id);
        return Ok(Paginar(consulta, page, pageSize));
    }

    public ResultadoOperacion<Personaje> ObtenerPersonaje(int id)
    {
        var personaje = _data.Personajes.FirstOrDefault(p => p.Id == id);
        return personaje is null
            ? Error<Personaje>(404, $"No existe el personaje con id {id}.")
            : Ok(personaje);
    }

    public ResultadoOperacion<Personaje> CrearPersonaje(PersonajeInput input)
    {
        var error = ValidarPersonaje(input, null);
        if (error is not null)
            return Error<Personaje>(400, error);

        var personaje = new Personaje
        {
            Id = _data.SiguientePersonajeId,
            Nombre = input.Nombre.Trim(),
            Especie = input.Especie.Trim(),
            Faccion = Normalizar(FaccionesValidas, input.Faccion)!,
            Afiliacion = input.Afiliacion.Trim(),
            Estado = Normalizar(EstadosValidos, input.Estado)!,
            FuerzaSensitivo = input.FuerzaSensitivo
        };

        _data.Personajes.Add(personaje);
        return Creado(personaje);
    }

    public ResultadoOperacion<Personaje> ActualizarPersonaje(int id, PersonajeInput input)
    {
        var personaje = _data.Personajes.FirstOrDefault(p => p.Id == id);
        if (personaje is null)
            return Error<Personaje>(404, $"No existe el personaje con id {id}.");

        var error = ValidarPersonaje(input, id);
        if (error is not null)
            return Error<Personaje>(400, error);

        var nuevoEstado = Normalizar(EstadosValidos, input.Estado)!;
        var tieneMuerteRegistrada = _data.Eventos.Any(e => e.Muertos.Contains(id));
        if (tieneMuerteRegistrada && !nuevoEstado.Equals("muerto", StringComparison.OrdinalIgnoreCase))
        {
            return Error<Personaje>(400,
                "El personaje tiene una muerte registrada en un evento y no puede cambiarse a un estado distinto de muerto.");
        }

        personaje.Nombre = input.Nombre.Trim();
        personaje.Especie = input.Especie.Trim();
        personaje.Faccion = Normalizar(FaccionesValidas, input.Faccion)!;
        personaje.Afiliacion = input.Afiliacion.Trim();
        personaje.Estado = nuevoEstado;
        personaje.FuerzaSensitivo = input.FuerzaSensitivo;

        return Ok(personaje);
    }

    public ResultadoOperacion<bool> EliminarPersonaje(int id)
    {
        var personaje = _data.Personajes.FirstOrDefault(p => p.Id == id);
        if (personaje is null)
            return Error<bool>(404, $"No existe el personaje con id {id}.");

        if (_data.Eventos.Any(e => e.Participantes.Contains(id)))
        {
            return Error<bool>(400,
                "No se puede eliminar el personaje porque está relacionado con uno o más eventos.");
        }

        var carta = _data.Cartas.FirstOrDefault(c => c.PersonajeId == id);
        if (carta is not null)
            _data.Cartas.Remove(carta);

        _data.Personajes.Remove(personaje);
        return Ok(true);
    }

    public ResultadoOperacion<IReadOnlyList<Evento>> ObtenerEventosDePersonaje(int id)
    {
        if (!_data.Personajes.Any(p => p.Id == id))
            return Error<IReadOnlyList<Evento>>(404, $"No existe el personaje con id {id}.");

        var eventos = _data.Eventos
            .Where(e => e.Participantes.Contains(id))
            .OrderBy(e => e.Fecha)
            .ToList();

        return Ok<IReadOnlyList<Evento>>(eventos);
    }

    public ResultadoOperacion<IReadOnlyList<RankingItem>> ObtenerRanking(string por)
    {
        if (!por.Equals("poder", StringComparison.OrdinalIgnoreCase))
            return Error<IReadOnlyList<RankingItem>>(400, "Por ahora el ranking disponible es por=poder.");

        var rankingBase = _data.Personajes
            .Select(p => new
            {
                Personaje = p,
                Poder = _data.Cartas.FirstOrDefault(c => c.PersonajeId == p.Id)?.Poder ?? 0
            })
            .OrderByDescending(x => x.Poder)
            .ThenBy(x => x.Personaje.Nombre)
            .ToList();

        var ranking = rankingBase
            .Select((x, index) => new RankingItem(
                index + 1,
                x.Personaje.Id,
                x.Personaje.Nombre,
                x.Personaje.Faccion,
                x.Poder))
            .ToList();

        return Ok<IReadOnlyList<RankingItem>>(ranking);
    }

    // ---------------- CARTAS ----------------

    public ResultadoOperacion<PagedResponse<CardPersonaje>> ListarCartas(
        int? personajeId,
        int? minPoder,
        int page,
        int pageSize)
    {
        var errorPaginacion = ValidarPaginacion(page, pageSize);
        if (errorPaginacion is not null)
            return Error<PagedResponse<CardPersonaje>>(400, errorPaginacion);

        IEnumerable<CardPersonaje> consulta = _data.Cartas;

        if (personajeId.HasValue)
            consulta = consulta.Where(c => c.PersonajeId == personajeId.Value);

        if (minPoder.HasValue)
        {
            if (minPoder.Value < 0)
                return Error<PagedResponse<CardPersonaje>>(400, "minPoder no puede ser negativo.");

            consulta = consulta.Where(c => c.Poder >= minPoder.Value);
        }

        consulta = consulta.OrderBy(c => c.Id);
        return Ok(Paginar(consulta, page, pageSize));
    }

    public ResultadoOperacion<CardPersonaje> ObtenerCarta(int id)
    {
        var carta = _data.Cartas.FirstOrDefault(c => c.Id == id);
        return carta is null
            ? Error<CardPersonaje>(404, $"No existe la carta con id {id}.")
            : Ok(carta);
    }

    public ResultadoOperacion<CardPersonaje> CrearCarta(CardPersonajeInput input)
    {
        var error = ValidarCarta(input, null);
        if (error is not null)
            return Error<CardPersonaje>(400, error);

        var carta = new CardPersonaje
        {
            Id = _data.SiguienteCartaId,
            PersonajeId = input.PersonajeId,
            Poder = input.Poder,
            HabilidadEspecial = input.HabilidadEspecial?.Trim() ?? string.Empty,
            Arma = input.Arma?.Trim() ?? string.Empty,
            NivelPeligrosidad = input.NivelPeligrosidad,
            ImagenUrl = input.ImagenUrl?.Trim() ?? string.Empty
        };

        _data.Cartas.Add(carta);
        return Creado(carta);
    }

    public ResultadoOperacion<CardPersonaje> ActualizarCarta(int id, CardPersonajeInput input)
    {
        var carta = _data.Cartas.FirstOrDefault(c => c.Id == id);
        if (carta is null)
            return Error<CardPersonaje>(404, $"No existe la carta con id {id}.");

        var error = ValidarCarta(input, id);
        if (error is not null)
            return Error<CardPersonaje>(400, error);

        carta.PersonajeId = input.PersonajeId;
        carta.Poder = input.Poder;
        carta.HabilidadEspecial = input.HabilidadEspecial?.Trim() ?? string.Empty;
        carta.Arma = input.Arma?.Trim() ?? string.Empty;
        carta.NivelPeligrosidad = input.NivelPeligrosidad;
        carta.ImagenUrl = input.ImagenUrl?.Trim() ?? string.Empty;

        return Ok(carta);
    }

    public ResultadoOperacion<bool> EliminarCarta(int id)
    {
        var carta = _data.Cartas.FirstOrDefault(c => c.Id == id);
        if (carta is null)
            return Error<bool>(404, $"No existe la carta con id {id}.");

        _data.Cartas.Remove(carta);
        return Ok(true);
    }

    // ---------------- EVENTOS ----------------

    public ResultadoOperacion<PagedResponse<Evento>> ListarEventos(
        int? participanteId,
        int? desde,
        int? hasta,
        int page,
        int pageSize)
    {
        var errorPaginacion = ValidarPaginacion(page, pageSize);
        if (errorPaginacion is not null)
            return Error<PagedResponse<Evento>>(400, errorPaginacion);

        if (desde.HasValue && hasta.HasValue && desde.Value > hasta.Value)
            return Error<PagedResponse<Evento>>(400, "El filtro desde no puede ser mayor que hasta.");

        IEnumerable<Evento> consulta = _data.Eventos;

        if (participanteId.HasValue)
            consulta = consulta.Where(e => e.Participantes.Contains(participanteId.Value));

        if (desde.HasValue)
            consulta = consulta.Where(e => e.Fecha >= desde.Value);

        if (hasta.HasValue)
            consulta = consulta.Where(e => e.Fecha <= hasta.Value);

        consulta = consulta.OrderBy(e => e.Fecha).ThenBy(e => e.Id);
        return Ok(Paginar(consulta, page, pageSize));
    }

    public ResultadoOperacion<Evento> ObtenerEvento(int id)
    {
        var evento = _data.Eventos.FirstOrDefault(e => e.Id == id);
        return evento is null
            ? Error<Evento>(404, $"No existe el evento con id {id}.")
            : Ok(evento);
    }

    public ResultadoOperacion<Evento> CrearEvento(EventoInput input)
    {
        var error = ValidarEvento(input, null);
        if (error is not null)
            return Error<Evento>(400, error);

        var evento = new Evento
        {
            Id = _data.SiguienteEventoId,
            Nombre = input.Nombre.Trim(),
            Fecha = input.Fecha,
            Ubicacion = input.Ubicacion.Trim(),
            Descripcion = input.Descripcion?.Trim() ?? string.Empty,
            Participantes = input.Participantes.Distinct().ToList(),
            Muertos = (input.Muertos ?? new List<int>()).Distinct().ToList()
        };

        _data.Eventos.Add(evento);
        AplicarMuertes(evento.Muertos);
        return Creado(evento);
    }

    public ResultadoOperacion<Evento> ActualizarEvento(int id, EventoInput input)
    {
        var evento = _data.Eventos.FirstOrDefault(e => e.Id == id);
        if (evento is null)
            return Error<Evento>(404, $"No existe el evento con id {id}.");

        var nuevosMuertos = (input.Muertos ?? new List<int>()).Distinct().ToList();
        if (evento.Muertos.Any(idMuerto => !nuevosMuertos.Contains(idMuerto)))
        {
            return Error<Evento>(400,
                "No se puede quitar una muerte que ya fue registrada en el evento.");
        }

        var error = ValidarEvento(input, id);
        if (error is not null)
            return Error<Evento>(400, error);

        evento.Nombre = input.Nombre.Trim();
        evento.Fecha = input.Fecha;
        evento.Ubicacion = input.Ubicacion.Trim();
        evento.Descripcion = input.Descripcion?.Trim() ?? string.Empty;
        evento.Participantes = input.Participantes.Distinct().ToList();
        evento.Muertos = nuevosMuertos;

        // Si cambian participantes o fecha, la simulación anterior deja de ser válida.
        evento.Resultado = null;
        evento.Ganador = null;

        AplicarMuertes(evento.Muertos);
        return Ok(evento);
    }

    public ResultadoOperacion<bool> EliminarEvento(int id)
    {
        var evento = _data.Eventos.FirstOrDefault(e => e.Id == id);
        if (evento is null)
            return Error<bool>(404, $"No existe el evento con id {id}.");

        if (evento.Muertos.Count > 0)
        {
            return Error<bool>(400,
                "No se puede eliminar un evento que registra muertes, porque rompería la consistencia del estado de los personajes.");
        }

        _data.Eventos.Remove(evento);
        return Ok(true);
    }

    public ResultadoOperacion<MvpResponse> ObtenerMvp(int eventoId)
    {
        var evento = _data.Eventos.FirstOrDefault(e => e.Id == eventoId);
        if (evento is null)
            return Error<MvpResponse>(404, $"No existe el evento con id {eventoId}.");

        if (evento.Participantes.Count == 0)
            return Error<MvpResponse>(400, "El evento no tiene participantes.");

        var cartas = _data.Cartas
            .Where(c => evento.Participantes.Contains(c.PersonajeId))
            .ToList();

        if (cartas.Count != evento.Participantes.Distinct().Count())
            return Error<MvpResponse>(400, "Todos los participantes deben tener una carta para calcular el MVP.");

        var mejorCarta = cartas.OrderByDescending(c => c.Poder).First();
        var personaje = _data.Personajes.First(p => p.Id == mejorCarta.PersonajeId);

        return Ok(new MvpResponse(evento.Id, personaje.Id, personaje.Nombre, mejorCarta.Poder));
    }

    public ResultadoOperacion<SimulacionResponse> SimularEvento(int eventoId)
    {
        var evento = _data.Eventos.FirstOrDefault(e => e.Id == eventoId);
        if (evento is null)
            return Error<SimulacionResponse>(404, $"No existe el evento con id {eventoId}.");

        var participantes = _data.Personajes
            .Where(p => evento.Participantes.Contains(p.Id))
            .ToList();

        if (participantes.Count < 2)
            return Error<SimulacionResponse>(400, "Se necesitan al menos dos participantes para simular una batalla.");

        var cartas = _data.Cartas
            .Where(c => evento.Participantes.Contains(c.PersonajeId))
            .ToList();

        if (cartas.Count != evento.Participantes.Distinct().Count())
            return Error<SimulacionResponse>(400, "Todos los participantes deben tener una carta antes de simular.");

        var grupos = participantes.GroupBy(p => p.Faccion).ToList();
        if (grupos.Count < 2)
            return Error<SimulacionResponse>(400, "La simulación necesita participantes de al menos dos facciones distintas.");

        var totales = new List<FuerzaBando>();

        foreach (var grupo in grupos)
        {
            var ids = grupo.Select(p => p.Id).ToHashSet();
            var poderBase = cartas.Where(c => ids.Contains(c.PersonajeId)).Sum(c => c.Poder);

            // Factor aleatorio acotado entre 0.90 y 1.10.
            var factor = Math.Round(0.90 + Random.Shared.NextDouble() * 0.20, 3);
            var poderFinal = Math.Round(poderBase * factor, 2);

            totales.Add(new FuerzaBando(grupo.Key, poderBase, factor, poderFinal));
        }

        var ordenados = totales.OrderByDescending(t => t.PoderFinal).ToList();
        var ganador = ordenados.Count > 1 && Math.Abs(ordenados[0].PoderFinal - ordenados[1].PoderFinal) < 0.001
            ? "Empate"
            : ordenados[0].Faccion;

        var resultado = ganador == "Empate"
            ? "La simulación terminó en empate."
            : $"La facción ganadora es {ganador}.";

        const string criterio =
            "Se suma el poder de las cartas por facción y se multiplica por un factor aleatorio controlado entre 0.90 y 1.10.";

        // Solo se guarda el resultado después de que todas las validaciones terminaron correctamente.
        evento.Ganador = ganador;
        evento.Resultado = resultado;

        return Ok(new SimulacionResponse(
            evento.Id,
            evento.Nombre,
            ganador,
            resultado,
            criterio,
            totales));
    }

    // ---------------- VALIDACIONES Y AYUDANTES ----------------

    private string? ValidarPersonaje(PersonajeInput input, int? personajeId)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre))
            return "El nombre es obligatorio.";

        if (string.IsNullOrWhiteSpace(input.Especie))
            return "La especie es obligatoria.";

        if (Normalizar(FaccionesValidas, input.Faccion) is null)
            return "La facción debe ser Rebelde, Imperio o Neutral.";

        if (Normalizar(EstadosValidos, input.Estado) is null)
            return "El estado debe ser vivo, muerto o desconocido.";

        if (personajeId.HasValue)
        {
            var muerteRegistrada = _data.Eventos.Any(e => e.Muertos.Contains(personajeId.Value));
            var estado = Normalizar(EstadosValidos, input.Estado);
            if (muerteRegistrada && estado != "muerto")
                return "El personaje tiene una muerte registrada y debe conservar el estado muerto.";
        }

        return null;
    }

    private string? ValidarCarta(CardPersonajeInput input, int? cartaId)
    {
        if (!_data.Personajes.Any(p => p.Id == input.PersonajeId))
            return $"El personaje {input.PersonajeId} no existe.";

        if (input.Poder <= 0)
            return "El poder debe ser mayor que cero.";

        if (input.NivelPeligrosidad < 0)
            return "El nivel de peligrosidad no puede ser negativo.";

        var otraCarta = _data.Cartas.Any(c =>
            c.PersonajeId == input.PersonajeId &&
            (!cartaId.HasValue || c.Id != cartaId.Value));

        if (otraCarta)
            return "Cada personaje puede tener una sola carta principal.";

        return null;
    }

    private string? ValidarEvento(EventoInput input, int? eventoId)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre))
            return "El nombre del evento es obligatorio.";

        if (string.IsNullOrWhiteSpace(input.Ubicacion))
            return "La ubicación del evento es obligatoria.";

        if (input.Participantes is null)
            return "La lista de participantes es obligatoria.";

        var participantes = input.Participantes.Distinct().ToList();
        var muertos = (input.Muertos ?? new List<int>()).Distinct().ToList();

        foreach (var participanteId in participantes)
        {
            if (!_data.Personajes.Any(p => p.Id == participanteId))
                return $"El participante {participanteId} no existe.";

            var muerteAnterior = _data.Eventos
                .Where(e => !eventoId.HasValue || e.Id != eventoId.Value)
                .Where(e => e.Muertos.Contains(participanteId))
                .OrderBy(e => e.Fecha)
                .FirstOrDefault(e => e.Fecha < input.Fecha);

            if (muerteAnterior is not null)
            {
                return $"El personaje {participanteId} murió en el evento '{muerteAnterior.Nombre}' " +
                       $"(fecha {muerteAnterior.Fecha}) y no puede participar en un evento posterior.";
            }
        }

        foreach (var muertoId in muertos)
        {
            if (!participantes.Contains(muertoId))
                return $"El personaje {muertoId} figura como muerto pero no está entre los participantes.";

            var participacionPosterior = _data.Eventos
                .Where(e => !eventoId.HasValue || e.Id != eventoId.Value)
                .FirstOrDefault(e => e.Fecha > input.Fecha && e.Participantes.Contains(muertoId));

            if (participacionPosterior is not null)
            {
                return $"No se puede registrar la muerte del personaje {muertoId} en la fecha {input.Fecha} " +
                       $"porque ya participa en el evento posterior '{participacionPosterior.Nombre}' " +
                       $"(fecha {participacionPosterior.Fecha}).";
            }
        }

        return null;
    }

    private void AplicarMuertes(IEnumerable<int> idsMuertos)
    {
        foreach (var id in idsMuertos)
        {
            var personaje = _data.Personajes.FirstOrDefault(p => p.Id == id);
            if (personaje is not null)
                personaje.Estado = "muerto";
        }
    }

    private static string? ValidarPaginacion(int page, int pageSize)
    {
        if (page < 1)
            return "page debe ser mayor o igual a 1.";

        if (pageSize < 1 || pageSize > 100)
            return "pageSize debe estar entre 1 y 100.";

        return null;
    }

    private static string? Normalizar(IEnumerable<string> permitidos, string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        return permitidos.FirstOrDefault(x => x.Equals(valor.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static PagedResponse<T> Paginar<T>(IEnumerable<T> fuente, int page, int pageSize)
    {
        var lista = fuente.ToList();
        var total = lista.Count;
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        var data = lista.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return new PagedResponse<T>(
            data,
            new PageMeta(page, pageSize, total, totalPages));
    }

    private static ResultadoOperacion<T> Ok<T>(T datos) => new(200, datos, null);
    private static ResultadoOperacion<T> Creado<T>(T datos) => new(201, datos, null);
    private static ResultadoOperacion<T> Error<T>(int codigo, string mensaje) => new(codigo, default, mensaje);
}
