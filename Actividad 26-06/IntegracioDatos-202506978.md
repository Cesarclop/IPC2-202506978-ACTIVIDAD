# Laboratorio: Integración y Transferencia de Datos

**Estudiante:** Cesar Emmanuel Cipriano López  
**Carnet:** 202506978  
**Curso:** Introducción a la Programación y Computación 2  
**Universidad de San Carlos de Guatemala — Facultad de Ingeniería**

---

## Sección 1 — Fundamentos teóricos

### 1.1 ¿Cuándo usar CSV y cuándo XML?

Elegir entre CSV y XML depende principalmente del tipo de datos que se quiere mover y la complejidad de la estructura que se necesita representar.

El formato **CSV** es una buena opción cuando los datos son planos, es decir, cuando cada fila representa un registro con campos simples sin relaciones anidadas. Su ventaja principal es que los archivos son pequeños y rápidos de leer o escribir. La desventaja es que si un valor contiene una coma, el archivo puede corromperse o malinterpretarse; además, no hay manera de indicar el tipo de cada columna (texto, número, fecha) directamente en el archivo.

El formato **XML** permite representar estructuras más complejas: un registro puede contener subelementos, atributos adicionales o listas internas. Es útil cuando se trabaja con sistemas que requieren validación estricta usando esquemas XSD. El inconveniente es que el mismo contenido ocupa bastante más espacio que en CSV y el tiempo de procesamiento aumenta en archivos grandes.

| Criterio | CSV | XML |
|---|---|---|
| Tamaño del archivo | Ligero | Más pesado |
| Estructuras anidadas | No soporta | Sí soporta |
| Validación de tipos | No | Sí (con XSD) |
| Velocidad de lectura | Rápida | Más lenta en archivos grandes |
| Uso recomendado | Cargas masivas simples | Datos jerárquicos o con validación |

---

### 1.2 Conversión de objetos a JSON y viceversa

Cuando una aplicación necesita enviar datos a un servicio externo o guardarlos en un archivo, no puede mandar directamente el objeto de C# porque ese objeto solo existe en la memoria del proceso. Es necesario convertirlo a texto primero — eso es la **serialización**.

Al recibir una respuesta de una API o leer ese archivo guardado, el proceso contrario convierte el texto de vuelta a un objeto que el programa puede manipular — eso es la **deserialización**.

En .NET, la librería `System.Text.Json` provee las herramientas para hacer ambas operaciones:

- Para serializar: `JsonSerializer.Serialize(miObjeto)` → devuelve un `string` con el JSON
- Para deserializar: `JsonSerializer.Deserialize<MiClase>(textoJson)` → devuelve una instancia de `MiClase`

Una opción útil al deserializar es `PropertyNameCaseInsensitive = true`, que permite que el mapeo funcione aunque la API devuelva los campos en `camelCase` y la clase tenga propiedades en `PascalCase`.

---

### 1.3 Carga masiva eficiente: evitar el cuello de botella

Cuando se importa un archivo CSV con cientos o miles de filas, una implementación ingenua podría guardar cada registro uno a uno en la base de datos. Esto genera un problema: si el archivo tiene 800 filas, el sistema lanza 800 operaciones `INSERT` independientes, cada una con su propia conexión y overhead de red.

Este patrón se conoce como el **problema N+1** y puede hacer que una importación que debería tomar segundos se extienda a minutos.

La solución es simple pero importante: leer **todas** las filas del archivo en memoria primero, construir la lista completa de objetos, y luego insertar todo de golpe usando `AddRange()` más una única llamada a `SaveChangesAsync()`. Así la base de datos recibe todos los registros en una sola transacción.

---

## Sección 2 — Implementación práctica

### 2.1 Consumo de endpoint REST

El siguiente servicio consulta la API de estudiantes de USAC, deserializa la respuesta y maneja los posibles errores de red o de formato.

```csharp
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

// Modelo que representa los datos que devuelve la API
public class DatosEstudiante
{
    public int Id { get; set; }
    public string Carnet { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
}

public class ServicioConsultaEstudiantes
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions _opcionesJson = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    };

    public ServicioConsultaEstudiantes(HttpClient http)
    {
        _http = http;
    }

    public async Task<DatosEstudiante?> ObtenerPorCarnetAsync(string carnet)
    {
        string url = $"https://api.usac.edu/v1/estudiantes/{carnet}";

        try
        {
            HttpResponseMessage respuesta = await _http.GetAsync(url);

            // Lanza excepción si el servidor devolvió 4xx o 5xx
            respuesta.EnsureSuccessStatusCode();

            string cuerpo = await respuesta.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<DatosEstudiante>(cuerpo, _opcionesJson);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Fallo la conexion con la API: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"No se pudo interpretar la respuesta JSON: {ex.Message}");
            return null;
        }
    }
}
```

---

### 2.2 Importación masiva de estudiantes desde CSV

El controlador recibe el archivo, lee cada línea, construye los registros en memoria y los guarda todos juntos al final para evitar el problema N+1.

```csharp
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class Estudiante
{
    public int Id { get; set; }
    public string Carnet { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Carrera { get; set; } = string.Empty;
}

public class BdAcademica : DbContext
{
    public DbSet<Estudiante> Estudiantes { get; set; }

    public BdAcademica(DbContextOptions<BdAcademica> opciones) : base(opciones) { }
}

[ApiController]
[Route("api/importacion")]
public class ImportacionController : ControllerBase
{
    private readonly BdAcademica _bd;

    public ImportacionController(BdAcademica bd)
    {
        _bd = bd;
    }

    [HttpPost("estudiantes")]
    public async Task<IActionResult> SubirArchivoEstudiantes(IFormFile csv)
    {
        if (csv is null || csv.Length == 0)
            return BadRequest("El archivo no puede estar vacío.");

        var registros = new List<Estudiante>();

        using var lector = new StreamReader(csv.OpenReadStream());

        // Primera línea es el encabezado — se descarta
        await lector.ReadLineAsync();

        string? linea;
        int numeroLinea = 1;

        while ((linea = await lector.ReadLineAsync()) is not null)
        {
            numeroLinea++;

            if (string.IsNullOrWhiteSpace(linea))
                continue;

            try
            {
                registros.Add(MapearLinea(linea));
            }
            catch (Exception)
            {
                // Si una línea está mal formada, se omite y sigue el proceso
                Console.WriteLine($"Línea {numeroLinea} omitida por formato inválido.");
            }
        }

        // Un solo INSERT masivo en lugar de uno por registro
        await _bd.Estudiantes.AddRangeAsync(registros);
        await _bd.SaveChangesAsync();

        return Ok(new
        {
            estado = "completado",
            totalImportados = registros.Count
        });
    }

    // Convierte una línea CSV en un objeto Estudiante
    // Formato esperado: Carnet,NombreCompleto,Correo,Carrera
    private static Estudiante MapearLinea(string linea)
    {
        string[] campos = linea.Split(',');

        return new Estudiante
        {
            Carnet          = campos[0].Trim(),
            NombreCompleto  = campos[1].Trim(),
            Correo          = campos[2].Trim(),
            Carrera         = campos.Length > 3 ? campos[3].Trim() : string.Empty
        };
    }
}
```

---

## Sección 3 — Referencia bibliográfica

Facultad de Ingeniería, Universidad de San Carlos de Guatemala (2026). *Sesión 20: Integración de Datos, Consumo de APIs Externas y Carga Masiva CSV/XML*. Material de laboratorio del curso Introducción a la Programación y Computación 2. Guatemala.
