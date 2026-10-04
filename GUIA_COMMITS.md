# Guía de commits

Este proyecto se entrega como un repositorio Git con **varios commits reales**, no como un único commit gigante.

## Historial incluido

El orden de construcción es:

1. `chore: crea base Minimal API con Swagger`
2. `feat: define modelos DTOs y respuestas de la API`
3. `feat: agrega almacenamiento en memoria y datos iniciales`
4. `feat: implementa reglas de negocio relaciones y estadísticas`
5. `feat: agrega endpoints REST versionados para los tres recursos`
6. `test: agrega ejemplos para probar la API y reglas de negocio`
7. `docs: documenta arquitectura uso y estrategia de commits`

Puedes comprobarlo con:

```bash
git log --oneline
```

## Si vas a subir ESTE historial a un repositorio vacío

Dentro de la carpeta del proyecto:

```bash
git remote add origin URL_DE_TU_REPOSITORIO
git push -u origin main
```

Si ya tenías un `origin` configurado:

```bash
git remote set-url origin URL_DE_TU_REPOSITORIO
git push -u origin main
```

De esta forma GitHub recibirá todos los commits por separado.

## Por qué los commits están separados así

### Commit 1: base

Solo configura el proyecto, .NET 10, Swagger y archivos iniciales. Es el esqueleto.

### Commit 2: modelos

Agrega las entidades y DTOs. Primero se define qué datos maneja el sistema.

### Commit 3: datos

Agrega las listas en memoria y datos iniciales. Los modelos ya existen antes de crear el almacenamiento.

### Commit 4: servicios

Agrega la lógica de negocio sin mezclarla todavía con HTTP: relaciones, validaciones, muerte, ranking, MVP y simulación.

### Commit 5: endpoints

Conecta la lógica con las rutas REST y actualiza `Program.cs` para mapear `/api/v1`.

### Commit 6: pruebas manuales

Agrega solicitudes concretas para demostrar cada funcionalidad.

### Commit 7: documentación

Explica el proyecto una vez que la implementación ya está completa.

## Si tu repositorio remoto YA tiene trabajo que no quieres perder

No hagas `push --force`. Primero guarda una copia de tu repositorio y compara el historial.

La forma más segura es conservar este proyecto como referencia y trasladar los cambios a tu repositorio en el mismo orden lógico: base, modelos, datos, servicios, endpoints, pruebas y documentación, haciendo un commit después de cada grupo.
