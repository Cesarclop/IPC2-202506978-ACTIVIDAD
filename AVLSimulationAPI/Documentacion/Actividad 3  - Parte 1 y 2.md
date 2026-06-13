# Actividad: Rotaciones Dobles en Árboles AVL y su Simulación vía API Web

## Introducción

En el estudio de las estructuras de datos balanceadas, los árboles AVL representan uno de los primeros y más elegantes mecanismos para garantizar que las operaciones de búsqueda, inserción y eliminación se mantengan en tiempo logarítmico. Sin embargo, no basta con aplicar rotaciones simples cuando el desbalance adquiere una forma de “zig‑zag”; es aquí donde entran en juego las rotaciones dobles. En esta actividad se explora por qué las rotaciones simples son insuficientes, cómo se define matemáticamente una rotación doble izquierda‑derecha (RID) y qué ventajas de ingeniería de software aporta reutilizar rotaciones simples. Además, se repasan los fundamentos del protocolo HTTP y el modelo cliente‑servidor para, finalmente, implementar una pequeña API en C# que simula el rebalanceo de un árbol AVL en el caso concreto de los valores 30, 10 y 20.

---

## Parte 1 – Investigación teórica

### El límite de las rotaciones simples y el desbalance en zig‑zag

Cuando se insertan valores en un árbol AVL, en ocasiones el desbalance no sigue una línea recta. Por ejemplo, si añadimos los números 30, luego 10 y después 20, el árbol se inclina primero a la izquierda (30 → 10) y luego el 20 se convierte en hijo derecho del 10. El resultado es una estructura en forma de “zig‑zag”: el abuelo (30) está desbalanceado hacia la izquierda, pero su hijo izquierdo (10) está cargado hacia la derecha.

Si en esta situación aplicamos únicamente una rotación simple derecha (sobre el abuelo) o una rotación simple izquierda (sobre el hijo), el desbalance no se corrige; simplemente se traslada de un lado a otro. La razón es que las rotaciones simples asumen que el desbalance se produce en una línea continua (izquierda‑izquierda o derecha‑derecha), pero en el caso zig‑zag el problema reside en la rama interior, que queda mal ubicada después de una rotación simple.

Para resolverlo de forma definitiva se necesita una **rotación doble**. En concreto, cuando hablamos de una secuencia izquierda‑derecha (el abuelo está desbalanceado hacia la izquierda y su hijo izquierdo está desbalanceado hacia la derecha), la solución es primero una rotación simple izquierda en el hijo y luego una rotación simple derecha en el abuelo. Esta combinación recibe el nombre de **Rotación Doble Izquierda‑Derecha (RID)**. Matemáticamente, se identifica porque el factor de equilibrio del abuelo es -2 y el factor de equilibrio del hijo izquierdo es +1.

Desde el punto de vista de la calidad del software, reutilizar las funciones de rotación simple para construir la rotación doble es una aplicación clara del principio **DRY (Don’t Repeat Yourself)**. En lugar de escribir desde cero la compleja reasignación de punteros que implica una rotación doble, se componen dos llamadas a rotaciones simples ya probadas. Esto hace el código más legible, más fácil de mantener y reduce drásticamente la posibilidad de errores.

### Fundamentos de arquitectura web y protocolo HTTP

Para exponer esta lógica de balanceo a través de la web es necesario comprender cómo se comunican un cliente y un servidor mediante HTTP. En el modelo cliente‑servidor, el cliente (por ejemplo, un navegador o una herramienta como `curl`) construye un mensaje de **petición** que incluye un método (GET, POST, etc.), una dirección (URL), cabeceras con metadatos y, opcionalmente, un cuerpo con datos. Ese mensaje viaja por la red hasta el servidor. El servidor interpreta la petición, ejecuta la lógica correspondiente y devuelve un mensaje de **respuesta** con un código de estado (200 para éxito, 404 para no encontrado, etc.), cabeceras y un cuerpo que contiene el resultado (por ejemplo, un JSON con la estructura del árbol).

Entre los métodos HTTP más importantes, **GET** está diseñado para la recuperación de información sin efectos secundarios; es decir, se usa para leer un recurso sin modificarlo. Por otra parte, **POST** se utiliza cuando se desea enviar datos al servidor para que provoquen un cambio de estado, como la inserción de un nuevo elemento en una estructura de datos. En nuestra API, el GET permitirá consultar el estado actual del árbol AVL, mientras que el POST enviará un nuevo nodo que puede desencadenar una rotación doble si se dan las condiciones adecuadas.

---

## Parte 2 – Implementación práctica (visión general)

Para consolidar estos conceptos se ha construido una pequeña API web utilizando [ASP.NET](https://asp.net/) Core en modo de “Minimal API”. El código se ha organizado en un único archivo `Program.cs`, donde se definen:

- Un modelo `NodoAVL` con propiedades `Id`, `Etiqueta` y `Altura`.
    
- Una lista en memoria llamada `estadoArbol` que simula el árbol AVL en su estado inicial desbalanceado (nodos 30 y 10).
    
- Un endpoint `GET /api/arbol` que devuelve la lista actual.
    
- Un endpoint `POST /api/arbol/insertar` que, cuando recibe un nodo con `Id = 20`, aplica la rotación RID sustituyendo la lista por una nueva con el nodo 20 como raíz, 10 como hijo izquierdo y 30 como hijo derecho. Para cualquier otro valor, simplemente añade el nodo a la lista sin aplicar balanceo.
    

Esta implementación es puramente didáctica: simula exactamente el caso estudiado en las diapositivas (secuencia 30, 10, 20) y demuestra cómo una rotación doble corrige el desbalance. Las pruebas de verificación consisten en realizar un GET para observar el estado inicial desbalanceado, luego un POST con el nodo 20, y finalmente comprobar que la respuesta contiene el nuevo árbol balanceado y un código de estado 201 (Created).

---

## Conclusión

Las rotaciones dobles son imprescindibles para mantener la propiedad de equilibrio en un árbol AVL cuando se presentan inserciones en zig‑zag. Reutilizar rotaciones simples es una buena práctica de ingeniería que mejora la mantenibilidad del código. Al exponer esta lógica mediante una API REST, se refuerza la comprensión del modelo cliente‑servidor y de la semántica de los métodos HTTP. La implementación en C# es sencilla pero ilustra perfectamente el concepto, permitiendo probar con herramientas como `curl` o Postman cómo se transforma la estructura del árbol tras una inserción crítica.