using Microsoft.AspNetCore.Mvc;
using ControlAcademicMvc.Models;

namespace ControlAcademicMvc.Controllers;

public class EstudianteController : Controller
{
    // Almacenamiento simulado en memoria
    private static readonly List<Estudiante> _baseDatosMemoria = new()
    {
        new Estudiante { Carne = 2026012, Nombre = "Fernando Velasquez", Promedio = 91.5 },
        new Estudiante { Carne = 2026045, Nombre = "Maria Mercedes", Promedio = 84.0 }
    };

    // GET: /Estudiante/Listar
    public IActionResult Listar()
    {
        return View(_baseDatosMemoria);
    }

    // GET: /Estudiante/Registrar
    public IActionResult Registrar()
    {
        return View();
    }

    // POST: /Estudiante/Registrar (VERSIÓN DE DIAGNÓSTICO)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrar(IFormCollection form)
    {
        // ----------------------------------------------
        // ¡MIRA LA TERMINAL! Aquí imprimimos lo que llega
        // ----------------------------------------------
        Console.WriteLine("=== DATOS RECIBIDOS ===");
        foreach (var key in form.Keys)
        {
            Console.WriteLine($"Clave: {key} -> Valor: {form[key]}");
        }
        Console.WriteLine("========================");

        // Leer manualmente los valores
        string carneStr = form["Carne"].ToString();
        string nombreStr = form["Nombre"].ToString();
        string promedioStr = form["Promedio"].ToString();

        // Verificar si llegaron vacíos
        if (string.IsNullOrEmpty(carneStr) || string.IsNullOrEmpty(nombreStr))
        {
            ModelState.AddModelError(string.Empty, "Error: El formulario no envió los datos correctamente. Verifica los nombres de los campos.");
            return View();
        }

        // Convertir manualmente
        if (!int.TryParse(carneStr, out int carne))
        {
            ModelState.AddModelError("Carne", "El Carné debe ser un número válido.");
            return View();
        }

        if (carne <= 0)
        {
            ModelState.AddModelError("Carne", "El Carné debe ser mayor a 0.");
            return View();
        }

        // Si todo está bien, creamos el estudiante manualmente
        var nuevoEstudiante = new Estudiante
        {
            Carne = carne,
            Nombre = nombreStr,
            Promedio = double.TryParse(promedioStr, out double prom) ? prom : 0.0
        };

        _baseDatosMemoria.Add(nuevoEstudiante);
        return RedirectToAction(nameof(Listar));
    }
}