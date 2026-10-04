# Catálogo Galáctico API

Proyecto académico en **.NET 10 + ASP.NET Core Minimal API** para administrar personajes, cartas y eventos relacionados. Los datos se guardan en memoria, por lo que al reiniciar la aplicación vuelven a su estado inicial.

La solución sigue la estructura pedida para el proyecto:

```text
CatalogoGalactico/
├── Data/
│   └── CatalogoData.cs
├── Endpoints/
│   ├── ApiResults.cs
│   ├── CartaEndpoints.cs
│   ├── EventoEndpoints.cs
│   └── PersonajeEndpoints.cs
├── Models/
│   ├── CardPersonaje.cs
│   ├── Evento.cs
│   ├── Personaje.cs
│   ├── Requests.cs
│   └── Responses.cs
├── Services/
│   └── CatalogoService.cs
├── Properties/
│   └── launchSettings.json
├── CatalogoGalactico.http
├── CatalogoGalactico.csproj
├── EXPLICACION_PROYECTO.md
├── GUIA_COMMITS.md
└── Program.cs
```

## Ejecutar

Necesitas **.NET SDK 10.0 o superior**.

```bash
dotnet restore
dotnet run
```

Con el perfil incluido, la API usa normalmente:

```text
http://localhost:5187
```

Swagger:

```text
http://localhost:5187/swagger
```

Si tu terminal muestra otro puerto al ejecutar `dotnet run`, usa ese puerto.

## Convención temporal

La fecha de los eventos se guarda como entero para poder compararla cronológicamente:

| Fecha del universo | Valor guardado |
|---|---:|
| 10 BBY | -10 |
| 5 BBY | -5 |
| Batalla de Yavin | 0 |
| 3 ABY | 3 |
| 10 ABY | 10 |

## Modelo

### Personaje

- `id`
- `nombre`
- `especie`
- `faccion`: `Rebelde`, `Imperio` o `Neutral`
- `afiliacion`
- `estado`: `vivo`, `muerto` o `desconocido`
- `fuerzaSensitivo`

### CardPersonaje

- `id`
- `personajeId`
- `poder`
- `habilidadEspecial`
- `arma`
- `nivelPeligrosidad`
- `imagenUrl`

Cada personaje puede tener **una sola carta principal**, representando la relación 1:1.

### Evento

- `id`
- `nombre`
- `fecha`
- `ubicacion`
- `descripcion`
- `participantes`: lista de IDs de personajes
- `muertos`: IDs de participantes que mueren en el evento
- `resultado`
- `ganador`

`muertos` se incorpora porque la consigna exige registrar una muerte, actualizar automáticamente el estado del personaje e impedir participaciones posteriores. Sin una referencia explícita a qué personaje murió no sería posible aplicar esa regla.

## Endpoints principales

Todas las rutas están versionadas con `/api/v1`.

### Personajes

| Método | Ruta | Función |
|---|---|---|
| GET | `/api/v1/personajes` | Lista con filtros y paginación |
| GET | `/api/v1/personajes/{id}` | Consulta por id |
| POST | `/api/v1/personajes` | Crea un personaje |
| PUT | `/api/v1/personajes/{id}` | Actualiza un personaje |
| DELETE | `/api/v1/personajes/{id}` | Elimina un personaje si no rompe relaciones |
| GET | `/api/v1/personajes/{id}/eventos` | Eventos de un personaje |
| GET | `/api/v1/personajes/ranking?por=poder` | Ranking por poder de carta |

Filtros combinables de ejemplo:

```text
GET /api/v1/personajes?faccion=Imperio&fuerzaSensitivo=true&page=1&pageSize=10
```

También se aceptan `estado` y `search`.

### Cartas

| Método | Ruta | Función |
|---|---|---|
| GET | `/api/v1/cartas` | Lista cartas |
| GET | `/api/v1/cartas/{id}` | Consulta una carta |
| POST | `/api/v1/cartas` | Crea una carta |
| PUT | `/api/v1/cartas/{id}` | Actualiza una carta |
| DELETE | `/api/v1/cartas/{id}` | Elimina una carta |

Filtros disponibles: `personajeId`, `minPoder`, `page`, `pageSize`.

### Eventos

| Método | Ruta | Función |
|---|---|---|
| GET | `/api/v1/eventos` | Lista eventos |
| GET | `/api/v1/eventos/{id}` | Consulta un evento |
| POST | `/api/v1/eventos` | Crea un evento |
| PUT | `/api/v1/eventos/{id}` | Actualiza un evento |
| DELETE | `/api/v1/eventos/{id}` | Elimina un evento que no registre muertes |
| GET | `/api/v1/eventos/{id}/mvp` | Participante de mayor poder |
| POST | `/api/v1/eventos/{id}/simular` | Simula la batalla |

Filtros disponibles: `participanteId`, `desde`, `hasta`, `page`, `pageSize`.

## Reglas de negocio importantes

1. Una carta solo puede apuntar a un personaje existente.
2. Un personaje solo puede tener una carta principal.
3. Los participantes de un evento deben existir.
4. Un personaje marcado como muerto debe estar dentro de los participantes del evento.
5. Al registrar una muerte, el estado del personaje cambia automáticamente a `muerto`.
6. No se puede crear un evento posterior que incluya a un personaje cuya muerte ya está registrada.
7. Tampoco se puede registrar una muerte en una fecha anterior si ya existe una participación posterior, porque eso dejaría datos temporalmente contradictorios.
8. La simulación exige al menos dos participantes, cartas para todos ellos y al menos dos facciones.
9. La simulación suma el poder de las cartas por facción y aplica un factor aleatorio limitado entre `0.90` y `1.10`.
10. El resultado de una simulación se guarda únicamente después de superar todas las validaciones.

## Paginación y errores

Las listas usan:

```text
?page=1&pageSize=10
```

La respuesta tiene una forma consistente:

```json
{
  "data": [],
  "meta": {
    "page": 1,
    "pageSize": 10,
    "total": 0,
    "totalPages": 0
  }
}
```

Los errores se devuelven como **Problem Details**, usando principalmente:

- `400 Bad Request` para datos o reglas inválidas.
- `404 Not Found` cuando el recurso no existe.
- `201 Created` al crear.
- `204 No Content` al eliminar.

## Archivos para estudiar el proyecto

- **`EXPLICACION_PROYECTO.md`**: explicación de arquitectura, flujo y reglas para poder exponerlo.
- **`CatalogoGalactico.http`**: solicitudes listas para probar desde VS Code/Visual Studio o adaptar a Postman.
- **`GUIA_COMMITS.md`**: historial y forma de subir el proyecto con varios commits en lugar de uno solo.
