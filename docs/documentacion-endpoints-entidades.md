# Documentación resumida

## Endpoints

### POST `/api/processing/process-folder`
- **Propósito:** analiza un proyecto (carpeta) y sube los grafos de flujo resultantes a almacenamiento.
- **Body:** JSON con `FolderPath` (string). Es obligatorio y debe apuntar a un directorio existente que contenga un `.sln` (el `ImportType` usado internamente es `Solution`).
- **Flujo:** se recorre la carpeta, se generan grafos de dependencia y se guarda cada archivo como `{nombreDeCarpeta}.json` en el bucket `plagiarism-detector`.
- **Respuestas:** `200 OK` si se procesó; `400 Bad Request` si falta `FolderPath`; `500 Internal Server Error` si ocurre una excepción.

### GET `/api/processing/list-files/{bucketName}`
- **Propósito:** lista los grafos que ya están guardados en el bucket indicado.
- **Ruta:** `bucketName` es obligatorio y se pasa como parte del path.
- **Respuesta:** `200 OK` con el siguiente cuerpo:
  ```json
  {
    "Bucket": "{bucketName}",
    "TotalFiles": <n>,
    "Files": [<FlowGraphs>, ...]
  }
  ```
  donde `Files` son los objetos `FlowGraphs` devueltos por el repositorio y `TotalFiles` es el conteo.
- **Errores:** `400 Bad Request` cuando el bucket está vacío o es sólo espacios en blanco; `500 Internal Server Error` si la lista falla.

### GET `/scalar`
- **Propósito:** proveer la interfaz de documentación automática (Scalar) que consume el archivo OpenAPI generado en `/openapi/v1/openapi.json`.
- **Disponibilidad:** sólo activa cuando la aplicación se ejecuta en `Development`, ya que `Program.cs` sólo invoca `UseOpenApiServices()` dentro de ese entorno.
- **Respuesta:** HTML con la UI de Scalar; en la vista principal se pueden elegir documentos por nombre (si hay más de uno) y explorar métodos, modelos y ejemplos.
- **Notas:** Scalar mapea esta interfaz al endpoint `/scalar` por defecto; el mismo paquete permite cambiar la ruta si fuera necesario (`MapScalarApiReference("/docs")`, etc.). citeturn0search0

### GET `/openapi/v1/openapi.json`
- **Propósito:** entrega el documento OpenAPI que alimenta la UI de Scalar y cualquier cliente externo que quiera conocer la API programáticamente.
- **Ruta generada automáticamente:** se expone cuando `app.MapOpenApi()` está registrado y corresponde a la versión `v1` usada en `AddOpenApiServices()`.
- **Consumo:** los navegadores y herramientas de prueba (Postman, Insomnia, etc.) pueden descargar este JSON para inspeccionar esquemas, parámetros y cuerpos.

## Entidades principales

### `NodeEssentials`
Representa un nodo del árbol de control. Campos:
- `Id` (int): identificador incremental único.
- `Name` (string): etiqueta del nodo.
- `ParentId` (int): identificador del padre (−1 para la raíz).

### `GraphEssentials`
Describe un nodo dentro del grafo de datos y sus dependencias.
- `Id` (int): identificador del nodo.
- `Name` (string): nombre del símbolo.
- `Children` (List<int>): lista de IDs de nodos dependientes.

### `FlowGraphs`
Agrega los dos conjuntos de grafos generados para un archivo:
- `Trees` (List<NodeEssentials>): árbol de control.
- `Graphs` (List<GraphEssentials>): grafo de datos.

### `FlowGraphResponse`
Estructura usada al generar contenido antes de subirlo:
- `filename` (string): nombre de carpeta/archivo asociado al flujo.
- `flowGraphs` (`FlowGraphs`): los grafos resultantes.

### `ImportType` (enum)
Usado por el extractor de proyectos para determinar qué archivos buscar:
- `String`
- `File`
- `CSproj`
- `Solution` (el que se utiliza durante el endpoint `process-folder`).
