# Explicación del proyecto para poder defenderlo

## 1. Qué hace la aplicación

La aplicación es una API REST construida con **ASP.NET Core Minimal API**. No tiene interfaz gráfica ni base de datos: recibe solicitudes HTTP y trabaja con tres listas en memoria.

Las tres entidades son:

1. **Personaje**: información general del personaje.
2. **CardPersonaje**: estadísticas de juego de ese personaje.
3. **Evento**: hecho que relaciona a varios personajes.

Las relaciones son:

- **Personaje 1:1 CardPersonaje**: una carta principal por personaje.
- **Personaje N:N Evento**: un personaje puede estar en muchos eventos y un evento puede tener muchos personajes. El evento guarda la lista de IDs de participantes.

## 2. Por qué el código está separado en carpetas

### `Models`

Define la forma de los datos.

- `Personaje.cs`, `CardPersonaje.cs`, `Evento.cs`: entidades almacenadas en memoria.
- `Requests.cs`: DTOs recibidos en POST y PUT.
- `Responses.cs`: respuestas especiales como paginación, ranking, MVP y simulación.

La idea es que el endpoint no tenga que inventar estructuras JSON manualmente.

### `Data`

`CatalogoData.cs` contiene las tres listas en memoria y datos iniciales.

También calcula el siguiente ID con el máximo existente + 1. Al reiniciar la aplicación, se crea de nuevo esta instancia y regresan los datos iniciales.

### `Services`

`CatalogoService.cs` contiene la lógica de negocio. Aquí se decide si una operación es válida.

Ejemplos:

- verificar facciones y estados permitidos;
- garantizar una sola carta por personaje;
- comprobar que los participantes existan;
- impedir eventos posteriores a una muerte;
- actualizar el estado a `muerto`;
- calcular ranking y MVP;
- simular batallas.

Esto evita colocar toda la lógica dentro de los handlers HTTP.

### `Endpoints`

Cada archivo agrupa las rutas de un recurso:

- `PersonajeEndpoints.cs`
- `CartaEndpoints.cs`
- `EventoEndpoints.cs`

Los endpoints reciben datos, llaman al servicio y transforman el resultado en una respuesta HTTP.

`ApiResults.cs` centraliza la forma de responder errores mediante Problem Details.

### `Program.cs`

Es el punto de inicio. Hace cuatro cosas principales:

1. configura Swagger;
2. registra `CatalogoData` y `CatalogoService` en inyección de dependencias;
3. crea el grupo `/api/v1`;
4. conecta los tres grupos de endpoints.

Por eso `Program.cs` queda pequeño.

## 3. Flujo de una solicitud

Ejemplo:

```text
POST /api/v1/eventos
```

El recorrido es:

```text
Cliente
  ↓
EventoEndpoints
  ↓
CatalogoService
  ↓
CatalogoData
  ↓
respuesta HTTP
```

El endpoint no modifica listas directamente. La decisión de si el evento es válido está en el servicio.

## 4. Versionado

Todas las rutas comienzan con:

```text
/api/v1
```

La idea es que en el futuro pueda existir `/api/v2` sin romper a los clientes que todavía usan la versión 1.

## 5. Relación 1:1 entre personaje y carta

Cuando se crea o actualiza una carta, `ValidarCarta` verifica:

1. que `personajeId` exista;
2. que `poder` sea mayor que cero;
3. que `nivelPeligrosidad` no sea negativo;
4. que no exista otra carta vinculada al mismo personaje.

Si ya existe otra, responde 400.

## 6. Relación N:N entre personajes y eventos

`Evento.Participantes` es una lista de IDs:

```json
"participantes": [1, 2, 3]
```

No se duplican objetos Personaje dentro de Evento. Solo se guardan identificadores.

Para consultar la relación en sentido contrario se usa:

```text
GET /api/v1/personajes/{id}/eventos
```

El servicio busca todos los eventos cuya lista de participantes contenga ese ID.

## 7. Convención BBY/ABY

Se utiliza un entero para poder comparar fechas sin hacer lógica de texto:

```text
10 BBY -> -10
5 BBY  -> -5
Yavin  -> 0
3 ABY  -> 3
10 ABY -> 10
```

Así una comparación cronológica es una comparación normal de enteros.

Ejemplo:

```csharp
if (eventoMuerte.Fecha < eventoNuevo.Fecha)
```

significa que la muerte ocurrió antes del nuevo evento.

## 8. Regla temporal de muerte

Para poder representar explícitamente quién muere, `Evento` tiene:

```csharp
List<int> Muertos
```

La validación tiene dos direcciones.

### Caso A: intentar participar después de morir

Si el personaje 5 murió en fecha 4 y se intenta crear un evento en fecha 5 que lo incluye, la API responde 400.

### Caso B: intentar registrar una muerte demasiado atrás

Si ya existe un evento de fecha 5 donde aparece el personaje y después se intenta registrar su muerte en fecha 4, también se rechaza. Esto evita que las listas queden temporalmente contradictorias.

### Actualización automática

Cuando el evento se guarda correctamente:

```csharp
personaje.Estado = "muerto";
```

No se hace antes de terminar las validaciones. De esa manera un error no deja un cambio parcial.

## 9. Ranking

Ruta:

```text
GET /api/v1/personajes/ranking?por=poder
```

El servicio:

1. recorre los personajes;
2. busca la carta de cada uno;
3. toma su poder;
4. ordena de mayor a menor;
5. asigna posición 1, 2, 3, etc.

Si un personaje todavía no tiene carta, queda con poder 0.

## 10. MVP

Ruta:

```text
GET /api/v1/eventos/{id}/mvp
```

El MVP es el participante cuya carta tiene el mayor `poder`.

Antes de calcularlo se comprueba que:

- el evento exista;
- tenga participantes;
- todos los participantes tengan carta.

## 11. Simulación de batalla

Ruta:

```text
POST /api/v1/eventos/{id}/simular
```

Pasos:

1. encontrar el evento;
2. reunir sus personajes;
3. verificar que haya al menos dos participantes;
4. reunir las cartas;
5. verificar que todos tengan carta;
6. agrupar participantes por `Faccion`;
7. sumar el poder base de cada facción;
8. generar para cada bando un factor entre `0.90` y `1.10`;
9. calcular `poderBase * factor`;
10. elegir el total más alto;
11. guardar `Ganador` y `Resultado` únicamente al final.

Ejemplo conceptual:

```text
Rebelde: 168 × 1.04 = 174.72
Imperio: 95 × 0.96 = 91.20
Ganador: Rebelde
```

Cada simulación puede variar porque el componente aleatorio está controlado dentro de un intervalo pequeño.

## 12. Filtros combinados

Ejemplo solicitado por el proyecto:

```text
GET /api/v1/personajes?faccion=Imperio&fuerzaSensitivo=true
```

Cada filtro es opcional. El servicio parte de todos los personajes y añade condiciones solo cuando el parámetro fue enviado.

También existen:

- `estado`
- `search`
- `page`
- `pageSize`

## 13. Paginación

`page` empieza en 1 y `pageSize` debe estar entre 1 y 100.

Proceso:

```text
Skip((page - 1) * pageSize)
Take(pageSize)
```

La respuesta agrega `meta` con página, tamaño, total y total de páginas.

## 14. Códigos HTTP

Los más importantes son:

- **200 OK**: lectura o actualización correcta.
- **201 Created**: recurso creado.
- **204 No Content**: eliminación correcta.
- **400 Bad Request**: datos inválidos o regla de negocio violada.
- **404 Not Found**: ID inexistente.

## 15. Qué demostrar en la exposición

Una demostración corta puede seguir este orden:

1. Abrir Swagger y mostrar `/api/v1`.
2. Ejecutar `GET /personajes`.
3. Ejecutar el filtro `faccion=Imperio&fuerzaSensitivo=true`.
4. Mostrar `GET /personajes/1/eventos` para la relación N:N.
5. Mostrar `/personajes/ranking?por=poder`.
6. Mostrar `/eventos/1/mvp`.
7. Ejecutar `POST /eventos/1/simular` y explicar poder base + factor.
8. Crear un evento donde muera un personaje.
9. Consultar al personaje y mostrar que pasó a `muerto`.
10. Intentar incluirlo en un evento posterior y mostrar el 400.

El archivo `CatalogoGalactico.http` ya contiene estos ejemplos en un orden preparado para la demostración.

## 16. Correspondencia con la rúbrica

### Modelo de datos y relaciones

- existen las tres entidades;
- personaje-carta 1:1;
- personaje-evento N:N;
- IDs claros y listas relacionadas.

### API y operaciones por recurso

- rutas REST separadas por recurso;
- filtros combinados;
- paginación;
- 200, 201, 204, 400 y 404 según la operación.

### Reglas de negocio y relaciones

- eventos por personaje;
- validación temporal de muerte;
- actualización automática del estado.

### Simulación y estadísticas

- poder de cartas;
- factor aleatorio acotado;
- explicación del cálculo;
- ranking;
- MVP.

### Estructura, calidad y documentación

- `Models`, `Data`, `Services`, `Endpoints`;
- DTOs separados;
- Swagger;
- README, explicación y archivo de pruebas.
