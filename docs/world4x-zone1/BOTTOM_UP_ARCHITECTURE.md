# Zona 1 — decisión de reconstrucción desde fundamentos (2026-10-14)

## Diagnóstico sobre el producto existente
La `WorldRegion1Runtime.cs` vigente crea una **escena acotada** con coordenadas fijas para Valoria, bosque, cantera, ruina y explorador, y genera terreno/decoración con funciones de presentación. Es útil como demostración de los bucles Bastión I–II y no debe borrarse; **no es la arquitectura de un mundo 4X persistente** con cientos de asentamientos, desplazamiento territorial y zoom continuo. Su ART FAIL registrado es independiente de esta carencia de escala.

`docs/WORLD_MAP_4X_FUNCTIONAL_LIBRARY_V1.md` ya especifica correctamente que el terreno es el fondo y las ciudades, ejércitos, recursos y alianzas son capas de gameplay. `docs/WORLD_MAP_VISUAL_BENCHMARK_V1.md` define cámara fija y zoom ortográfico 18/14/10/7, más prueba móvil. El error sería reconstruir primero una ilustración panorámica y después intentar encajar el juego dentro.

## Orden de construcción desde abajo
**0. Contrato de coordenadas.** Mundo lógico en celdas enteras (x,z), identidades estables para nodos, ciudades y POI; el mapa no se divide en escenas por zona. La zona visible es un subconjunto consultado por cámara.

**1. Topología.** La Brecha central es una región prohibida/peligrosa según reglas futuras; su geometría es un campo territorial, no un castillo. Zona 1 forma un anillo grande y accesible; únicamente el borde sur contiene litoral. Las barreras de montaña no pueden encerrar jugadores. La distancia y la velocidad de marcha son reglas de servidor, no unidades de píxeles.

**2. Simulación lógica.** Generador determinista, con datos de celdas y ocupación separados. Prohibido acoplar `PlayerCity_v1.glb` a la posición real de la ciudad. Entidades con IDs persistentes; servidor será autoridad futura.

**3. Streaming.** Dividir en chunks de 16x16 celdas. Cargar chunks alrededor de la cámara, reciclar al salir; prohibido instanciar las 16.384 celdas del prototipo en Unity. Render a distancia con iconos/LOD y a cercanía con prefabs. Tres zooms de la **misma cámara** y las **mismas coordenadas**, no imágenes independientes.

**4. Gramática visual barata.** Pradera como fondo principal; árboles en grupos escasos; montañas en masas bajas y periféricas; nodos madera/piedra/comida reconocibles por silueta; ciudades pequeñas con bandera como overlay; corrupción del suelo con materiales y pocos picos. Un solo litoral sur, sin construir barcos ahora.

**5. Interacción antes del embellecimiento.** Selección de ciudad/nodo/campamento, movimiento de cámara, zoom, transición a Valoria y marcador de marcha. Luego calidad visual con cámara Unity, reutilizando librería existente si supera la comparación A/B.

**6. Integración.** Una vez que el prototipo aislado funciona, coordinar con el propietario de Region 1 la migración por interfaces; no sustituir `WorldRegion1Runtime` ni el gameplay certificado mientras su trabajo siga activo. No reclamar runner Windows del Bastión.

## Gates de aceptación de la primera entrega
- Un mapa lógico continuo y reproducible de 128x128, con 16x16 chunks.
- Brecha central, costa exclusivamente al sur y predominio de terreno construible.
- La misma ciudad mantiene ID y coordenadas a todos los zooms.
- El número de objetos renderizados depende de chunks visibles, no del tamaño total del mundo.
- Pan y zoom preservan el punto bajo el foco; pruebas de móvil y captura Unity pendientes de validación real.
- Cero modificación de la escena de Valoria y de Region 1 en producción.

## Límites y estado
Esta fase define arquitectura y construye generador **sin sustituir el producto actual**. La simulación multijugador y la persistencia autoritativa no se pueden declarar terminadas con un generador local. Las imágenes aprobadas son dirección visual, no certificación de assets ni distancias reales.
