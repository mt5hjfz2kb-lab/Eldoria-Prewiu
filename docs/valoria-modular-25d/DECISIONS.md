# Decisiones de producción · Valoria modular Bastión I
Fecha de inicio: 2026-11-18

## Lecciones constatadas en las ramas anteriores
- Ciudad2D ilustrada: los sprites se podían importar y el editor capturó muestras, pero el fondo era una sola lámina. Las bases artificiales y el desacuerdo de contraste de los edificios no se resolvían con enfoque (README de rama art/ciudad-2d-hitos-1-5-20261116).
- Profundidad de Premium V6: aumentar sombras sin cambiar las capas, puntos de apoyo ni perspectiva no elimina la percepción de collage.
- Reboot/CC0/Stylized: herramientas distintas no garantizan composición; se evita modelado de cajas/miniaturas para todo.
- La técnica real de Whiteout, Top Heroes o Kingshot no se infiere de capturas: se toman principios de legibilidad, no pipelines ni modelos.

## Técnica seleccionada (candidato; sin aprobación final)
**Sprites isométricos independientes con mosaico de diamantes 2:1** en lugar de malla 3D o una ilustración completa. Se escoge SpriteRenderer por contener el paquete ya disponible en el proyecto y evitar introducir paquetes o escenas canónicas durante un piloto; Tilemap isométrico queda como opción de implementación de terreno a validar en Unity si los 441 renderers consumen demasiadas llamadas/tiempo. Malla auxiliar 3D no añade valor en una cámara fija y reintroduce el coste de geometría.
La escena debe medir en dispositivo si Tilemap batchea mejor, antes de congelar técnica.

## Biblia reproducible
- Cámara: ortográfica de frente, eje visual XY 2:1, sin rotación libre.
- Mosaico: `x=(col-row)*1`, `y=(col+row)*.5`; tamaño sprite 132×68 con PPU64, 2 px solape; 21×21 células iniciales.
- Mundo: 1 unidad = 64 px en suelo, las construcciones emplean PPU96 por tamaño de origen; regla vinculante = pies reales alineados con mundo, no escalas arbitrarias de pantalla.
- Pivotes: edificios `(0.5,0.095)` Bastión, `(0.5,0.085)` Aserradero; personajes `(0.5,0.07)`; árboles `(0.5,0.06)`.
- Orden: `Mathf.RoundToInt(1000 - footY*100)`; sombra `order-1/-2`. El suelo va a órdenes negativos. Los cambios de z no simulan profundidad.
- Paleta máster: tinta #242a30, pizarra #3a4e5a, piedra iluminada #b8b6a0, piedra base #828b85, madera #774d36, madera clara #aa784a, hierba #5d7b57, ocre #977f61, ventana #f1ad58, místico #735d91.
- Luz: superior izquierda, sombras frías al sudeste; paleta y sombreado horneados por el mismo generador raster, sin luces físicas costosas.
- Silueta: Bastión I compacto con torre reparada irregular, portón, resto imperial derrumbado y andamiaje; Aserradero mucho más bajo con taller abierto, banco de sierra y pila de troncos.
- Autoría: algoritmo C# trazable con semilla fija y funciones `Poly,Line,Ellipse`, doble resolución y filtrado bilineal. No copia sprites ajenos, no usa el GLB vetado.
- Animación: dos frames de marcha por personaje, ruta en coordenadas de mundo, sorting cada frame; árbol con sway de baja amplitud.
- UI: uGUI por datos de `PlayerState`; construcción via `ICommandGateway.Execute`, lectura del reloj UTC de la partida, persistencia via `FileStateStore`.
- Memoria inicial: texturas de suelo 132×68×4 variantes×4 tipos (ARGB32), 480×416 Bastión, 362×299 Aserradero; generadas una vez, sin mipmaps. Estas son **dimensiones**, no memoria GPU verificada. Medir CPU de creación de texturas, GC, batch/draw calls y memoria WebGL/iPhone antes de escalar.
- Export: conservar `.meta` para que Unity preserve referencias; arte se materializa desde C# en PlayMode. Futuros niveles deben pasar a atlas/fuente master tras aprobación humana, no depender del frame runtime indefinidamente.

## Riesgos activos y siguientes correcciones
1. **Arte candidato no aprobado**: el generador de polígonos puede parecer demasiado rígido/esquemático. Se requiere captura auténtica nativa para evaluar siluetas en 390×844, luego redibujar volumen/curvatura/control de color; no declarar 8/10 por compilar.
2. Los estados iniciales del Aserradero cambian tinte, **no tienen un sprite de obra distinto**. Reautorizar estados con la misma biblioteca raster; nunca otra UI que simule gasto.
3. Sin navegación ni colisión real. Los recorridos prefijados deben cruzar visualmente árbol sin atravesar su tronco; comprobar en secuencia.
4. El HUD está en ScreenSpaceOverlay y las capturas Camera.Render solo muestran mundo; capturar GameView realmente para aceptación UI.
5. Falta PlayMode, import/compilación, WebGL, prueba táctil, persistencia recargada, rendimiento iPhone y aprobación propietario. No rellenar vacíos con pruebas simuladas.

## Compromisos de concurrencia
Nuevo trabajo exclusivamente en `Assets/Experimentos/ValoriaModular25D` y `docs/valoria-modular-25d`. Se creó rama aislada desde `60d8ce11`. No se cambia `main` ni registro vivo en main porque la orden del propietario prohíbe modificarlo. La reserva Windows observada corresponde a `eldoria-valoria-25d-depth-motion-pilot-20261118`; no se usa ni se desplaza. Para pruebas Unity pesadas habrá que registrar la titularidad de la rama/reserva conforme al protocolo, sin sobrescribir la actual.
