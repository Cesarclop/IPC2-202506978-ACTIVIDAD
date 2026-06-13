var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var estadoArbol = new List<NodoAVL>
{
    new NodoAVL
    {
        Id = 30,
        Etiqueta = "Nodo Raiz (Abuelo) - FE: -2"
    },
    new NodoAVL
    {
        Id = 10,
        Etiqueta = "Hijo Izquierdo - FE: +1"
    }
};

app.MapGet("/api/arbol", () =>
{
    return estadoArbol;
});

app.MapPost("/api/arbol/insertar", (NodoAVL nuevoNodo) =>
{
    if (nuevoNodo.Id <= 0)
    {
        return Results.BadRequest("ID invalido");
    }

    if (nuevoNodo.Id == 20)
    {
        estadoArbol.Clear();

        estadoArbol.Add(new NodoAVL
        {
            Id = 20,
            Etiqueta = "Nueva Raiz Balanceada (RID) - FE: 0"
        });

        estadoArbol.Add(new NodoAVL
        {
            Id = 10,
            Etiqueta = "Hijo Izquierdo - FE: 0"
        });

        estadoArbol.Add(new NodoAVL
        {
            Id = 30,
            Etiqueta = "Hijo Derecho - FE: 0"
        });

        return Results.Created("/api/arbol", estadoArbol);
    }

    estadoArbol.Add(nuevoNodo);

    return Results.Created($"/api/arbol/{nuevoNodo.Id}", nuevoNodo);
});

app.Run();
 