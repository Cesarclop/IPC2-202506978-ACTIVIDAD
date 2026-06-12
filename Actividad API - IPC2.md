# Actividad 11 de Junio IPC2P: Estructuras de Datos Avanzadas y APIs con ASP.NET Core

**Autor:** [Tu Nombre]  
**Fecha:** 11 de junio de 2026  

## Parte 1: Investigación Teórica

### 1. Estructuras de Datos Eficientes

#### Árbol Binario de Búsqueda (ABB)
- **Regla de ordenamiento:** Para cada nodo, todos los valores del subárbol izquierdo son menores y los del subárbol derecho son mayores.
- **Desventaja principal:** Cuando los datos se insertan en orden secuencial, el árbol se degenera en una lista enlazada (altura O(n)), haciendo que las operaciones pasen de O(log n) a O(n).

#### Árbol AVL (Auto‑balanceado)
- **Definición:** Es un ABB que, tras cada inserción o eliminación, se reequilibra mediante rotaciones para mantener la diferencia de alturas entre subárboles como máximo 1.
- **Factor de balanceo:** `Altura_izquierda − Altura_derecha`. Sus valores posibles son -1, 0 o 1. Si se sale de ese rango, se aplican rotaciones.
- **Complejidad:** La altura se mantiene en O(log n), por lo que las operaciones de búsqueda, inserción y eliminación son siempre O(log n).

### 2. Fundamentos de Web APIs

#### API y modelo Cliente‑Servidor
- **API:** Interfaz que permite la comunicación entre aplicaciones. Una API REST expone recursos mediante URLs.
- **Cliente‑Servidor:** El cliente envía una petición HTTP (método, URL, cabeceras, cuerpo opcional) al servidor. El servidor procesa y devuelve una respuesta con código de estado (200, 404, 201, etc.), cabeceras y cuerpo (JSON, etc.).

#### Verbos HTTP: GET y POST
- **GET:** Recupera recursos. Es idempotente (múltiples peticiones idénticas producen el mismo efecto) y no debe modificar el estado del servidor.
- **POST:** Crea nuevos recursos. No es idempotente (varias peticiones pueden crear múltiples recursos). Se usa para enviar datos al servidor.

---

## Parte 2: Implementación Práctica

### Código completo del `Program.cs` (Minimal API)

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Base de datos simulada en memoria
var coleccionNodos = new List<NodoElemento>
{
    new NodoElemento { Id = 10, Valor = "Raíz Inicial (ABB)" },
    new NodoElemento { Id = 5, Valor = "Hijo Izquierdo" }
};

// GET /api/nodos
app.MapGet("/api/nodos", () => Results.Ok(coleccionNodos));

// POST /api/nodos
app.MapPost("/api/nodos", (NodoElemento nuevoNodo) =>
{
    if (nuevoNodo.Id <= 0 || string.IsNullOrEmpty(nuevoNodo.Valor))
        return Results.BadRequest("Datos del nodo inválidos.");

    coleccionNodos.Add(nuevoNodo);
    return Results.Created($"/api/nodos/{nuevoNodo.Id}", nuevoNodo);
});

app.Run();

// Modelo del recurso
public class NodoElemento
{
    public int Id { get; set; }
    public string Valor { get; set; } = string.Empty;
}
```


### Ejecución de la API

La API se ejecuta con el comando `dotnet run` y queda escuchando en `http://localhost:5223`.

![[Pasted image 20260611182746.png]]

	![[Pasted image 20260611183016.png]]
---

## Parte 3: Verificación y Pruebas

### Prueba del GET inicial

**Petición:** `GET http://localhost:5223/api/nodos`

**Resultado esperado:** Código `200 OK` con los dos nodos iniciales.

![[Pasted image 20260611182830.png]]