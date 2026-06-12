var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Base de datos simulada en memoria
var coleccionNodos = new List<NodoElemento>
{
    new NodoElemento { Id = 10, Valor = "Raíz Inicial (ABB)" },
    new NodoElemento { Id = 5, Valor = "Hijo Izquierdo" }
};

// GET
app.MapGet("/api/nodos", () => Results.Ok(coleccionNodos));

// POST
app.MapPost("/api/nodos", (NodoElemento nuevoNodo) =>
{
    if (nuevoNodo.Id <= 0 || string.IsNullOrEmpty(nuevoNodo.Valor))
        return Results.BadRequest("Datos del nodo inválidos.");

    coleccionNodos.Add(nuevoNodo);
    return Results.Created($"/api/nodos/{nuevoNodo.Id}", nuevoNodo);
});

app.Run();

// Clase simple
public class NodoElemento
{
    public int Id { get; set; }
    public string Valor { get; set; } = string.Empty;
}