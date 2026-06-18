# Reporte de Laboratorio - Arquitectura Multi-Nivel (N-Tier) y Patrón MVC

**Curso:** IPC2 
**Fecha:** 17 Junio 2026  
**Estudiante:** Cesar Emmanuel Cipriano López

---

## Parte 1: Fundamentación Teórica y Análisis Crítico

### 1. El Tránsito hacia los Sistemas Distribuidos y Multi-Capa

**Limitación del Monolito Local**  
Cuando la interfaz de usuario, la lógica de negocio y el almacenamiento de datos residen en una única máquina física aislada, se presentan serios problemas de ingeniería:
- **Cuellos de botella:** Todos los usuarios compiten por los mismos recursos (CPU, memoria, E/S), degradando el rendimiento a medida que crece la concurrencia.
- **Inconsistencia de datos:** La sincronización depende de la capacidad de la máquina para manejar transacciones; no hay replicación natural, lo que provoca riesgos de integridad.
- **Punto único de fallo (SPOF):** Cualquier fallo de hardware o software deja inoperativo todo el sistema.
- **Escalabilidad limitada (Vertical Scaling):** La única opción es mejorar el hardware (más RAM, mejor CPU), lo cual tiene un tope físico y es costoso.

**Distinción Crítica (Layers vs. Tiers)**
- **Capas Lógicas (Layers):** Son una organización conceptual del código que agrupa funcionalidades por responsabilidad (presentación, negocio, datos). Pueden residir en el mismo proceso o en diferentes, pero su separación es puramente lógica.
- **Niveles Físicos (Tiers):** Es la distribución real del software en máquinas o servidores independientes. Cada nivel (Tier) se ejecuta en un hardware distinto y se comunica mediante protocolos de red.

**Responsabilidades en la Arquitectura de 3 Niveles (3-Tier)**
- **Nivel 1 (Presentación):** Interactúa con el usuario final. Captura entradas y muestra resultados. Tecnologías: HTML, CSS, JavaScript, frameworks frontend (React, Angular), o vistas Razor en ASP.NET Core.
- **Nivel 2 (Aplicación/Negocio):** Contiene la lógica de negocio, las reglas de procesamiento y la orquestación de operaciones. Tecnologías: .NET Core, Java Spring Boot, Node.js, o Python con Django/Flask, expuestos mediante APIs REST.
- **Nivel 3 (Datos):** Gestiona el almacenamiento persistente, la integridad y la disponibilidad de los datos. Tecnologías: SQL Server, PostgreSQL, MySQL, o bases de datos NoSQL como MongoDB.

**Seguridad Perimetral**  
Exponer el puerto de la base de datos (ej. 1433 para SQL Server) directamente a Internet es un error crítico de seguridad, ya que incrementa la superficie de ataque, permite ataques de fuerza bruta y explotación de vulnerabilidades. La buena práctica es aislar la base de datos en una red privada (VPC) accesible únicamente desde el servidor de aplicaciones mediante firewalls y conexiones cifradas (TLS), utilizando credenciales robustas y autenticación a nivel de aplicación.

### 2. Desacoplamiento Lógico con el Patrón MVC

**La Crisis del Código Espagueti**  
Mezclar en un mismo archivo sentencias SQL, lógica matemática y etiquetas visuales (HTML) genera un código con alto acoplamiento y baja cohesión. Esto dificulta el mantenimiento (cambios en la interfaz rompen la lógica de negocio), impide las pruebas unitarias (no se puede aislar cada componente) y viola el Principio de Responsabilidad Única (SRP) de los principios SOLID.

**Separación de Preocupaciones (SoC) - Patrón MVC (Trygve Reenskaug)**
- **Modelo (Model):** Representa los datos y la lógica de negocio. Es completamente independiente de la interfaz de usuario; no sabe cómo se muestran los datos. Contiene validaciones, cálculos de dominio y reglas de persistencia (a través de repositorios).
- **Vista (View):** Es la representación visual. Se define como una entidad **pasiva** (no contiene lógica de negocio) pero **inteligente** (posee lógica de presentación para iterar listas, mostrar condicionales y formatear datos). Tiene **estrictamente prohibido** contener código de acceso a datos (SQL) o lógica de negocio.
- **Controlador (Controller):** Actúa como el "director de orquesta". Recibe las peticiones HTTP, interactúa con el Modelo para obtener o modificar datos, y selecciona la Vista a renderizar. Su rol es de orquestación, no de implementación; por eso debe ser **delgado (Skinny Controller)** y no superar las 20 líneas de código por método.

**Métricas de Ingeniería de Software**
- **Alta Cohesión:** Cada componente tiene una responsabilidad única y bien definida (Modelo = datos, Vista = UI, Controlador = flujo), lo que facilita el mantenimiento y la evolución paralela.
- **Bajo Acoplamiento:** Los componentes se comunican a través de interfaces claras, reduciendo las dependencias internas. Esto permite modificar una capa sin afectar a las otras y facilita las pruebas unitarias (se pueden simular los componentes con mocks).

---

## Parte 2: Modelado del Ciclo de Vida y Enrutamiento Semántico

### 1. Mapeo Analítico de URLs

El motor de enrutamiento de ASP.NET Core utiliza la plantilla por defecto:  
`{controller=Home}/{action=Index}/{id?}`  

| URL Entrante | Clase Controladora | Método (Acción) | Parámetro `id` | Análisis Semántico |
| :--- | :--- | :--- | :--- | :--- |
| `https://ingenieria.usac.edu.gt/ControlAcademico/Login` | `ControlAcademicoController` | `Login` | `null` | Acción de inicio de sesión. No requiere identificador. |
| `https://ingenieria.usac.edu.gt/Estudiante/Historial/20260123` | `EstudianteController` | `Historial` | `20260123` | El ID corresponde al carné del estudiante cuyo historial se consulta. |
| `https://ingenieria.usac.edu.gt/Asignacion/Detalle/10` | `AsignacionController` | `Detalle` | `10` | El ID corresponde al identificador de la asignación a detallar. |
| `https://ingenieria.usac.edu.gt/Home` | `HomeController` | `Index` (por defecto) | `null` | Página de inicio; se usa el valor por defecto de la acción. |

### 2. Diagramación del Flujo Interactivo (Ciclo de Vida de una Petición HTTP)

**Paso 1: Interacción del Usuario**  
El usuario hace clic en un botón o enlace del navegador. El navegador construye una solicitud HTTP (GET o POST) con la URL correspondiente y la envía al servidor.

**Paso 2: Enrutamiento (Routing)**  
El servidor (Kestrel/IIS) recibe la petición y el middleware de enrutamiento de ASP.NET Core la procesa. Compara la URL con la plantilla registrada (`{controller}/{action}/{id?}`) y extrae los valores de controlador, acción e ID.

**Paso 3: Activación del Controlador**  
El framework instancia la clase controladora (por ejemplo, `EstudianteController`) utilizando un Controller Factory. Luego invoca el método de acción correspondiente (ej. `Listar`), inyectando los parámetros necesarios (como `id`).

**Paso 4: Orquestación del Controlador**  
El controlador se comunica con el Modelo (o con la capa de servicios) para obtener o modificar los datos. En el caso práctico, obtiene la lista de estudiantes de la memoria centralizada (simulando el Tier 3). No realiza cálculos complejos; solo delega. Finalmente, retorna una Vista con el modelo de datos.

**Paso 5: Renderizado de la Vista y Respuesta**  
El motor de vistas (Razor) combina la plantilla `.cshtml` con el modelo de datos para generar el HTML final. Este HTML se envía como respuesta HTTP al navegador, que lo interpreta y muestra la página al usuario, completando así el ciclo.

---

## Parte 5: Referencias Bibliográficas (Requisito Formal)

- Facultad de Ingeniería, USAC. (2026). Sesión 11: Modelado Base y Arquitecturas de Despliegue. Evolución de Sistemas Distribuidos, Fundamentos del Modelo Cliente-Servidor y Diseño Físico Multi-Capas (N-Tier). Laboratorio del curso Introducción a la Programación y Computación 2. Guatemala.

- Facultad de Ingeniería, USAC. (2026). Sesión 12: Arquitectura y Componentes del Patrón MVC. Desacoplamiento Lógico de Software, Ciclo de Vida de las Peticiones y Enrutamiento en Aplicaciones Interactivas Modernas. Laboratorio del curso Introducción a la Programación y Computación 2. Guatemala.

---

**Nota final:** La implementación práctica se realizó siguiendo los lineamientos de arquitectura N-Tier y MVC, garantizando un bajo acoplamiento y una alta cohesión, con controladores delgados (métodos < 20 líneas) y separación estricta de responsabilidades, conforme a lo exigido en la actividad.