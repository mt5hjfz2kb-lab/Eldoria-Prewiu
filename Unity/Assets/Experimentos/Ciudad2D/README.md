# Eldoria · Ciudad 2.5D ilustrada, hitos I/V (sandbox)

**Estado:** obra gráfica y código preparados; composición auditada con Pillow; **sin captura de Unity ni compilación PlayMode certificada**. La escena presente en Git está vacía hasta que el Editor importe los ocho sprites y la regenere. No afirmar que la ciudad es jugable hasta probar PlayMode y punteros.

**Rama única de este pase:** `art/ciudad-2d-hitos-1-5-20261116`, derivada de `experimento-ciudad-2d` en `1e3d2215451aa2db78c1db3e703608b556e17240`. `main`, escenas reales, sistemas y guardados no se han editado. No añadir `Experimento_Ciudad2D.unity` a Build Settings hasta aprobar arte, QA Unity y perfiles móviles.

## Decisión de producción

- Técnica: **2.5D ilustrado** (sprites isométricos, edificios independientes, escena ortográfica; no una imagen plana única). Dirección: fantasía medieval adulta y legible, piedra cálida/pizarra azul, madera envejecida, iluminación de fuego, heráldica azul/oro y amenaza distante violeta. Se evita el look de castillo infantil o fotografía pegada.
- Comparación: el mismo Bastión muestra un hito inicial **I** compacto, irregular y reconstruido, y un hito **V** monumental con dos torres. Ambos proceden de un mismo pase artístico de dos variantes, perspectiva y emblema compartidos.
- Los niveles de juego no se alteran: visual **I para niveles 1–4**, **V para 5–9**. En 10–14/15–19 se conserva provisionalmente el arte de V, NO se inventa arte de 10/15. Hitos futuros: 10 y 15, tras aprobación de los dos primeros. Otros edificios mantienen un único sprite hasta que se apruebe su progresión.
- El terreno y cinco edificios secundarios proceden de los siete PNG adjuntos en la conversación del experimento anterior, armonizados en tono y heráldica; el Bastión I/V son nuevos. No mezclar arte externo sin licencia ni reutilizar el modelo 3D de Bastión vetado.
- **Arte no aprobado todavía**: puede apreciarse que el Bastión tiene iluminación/contraste más oscuros que los edificios secundarios. Hay exceso de césped vacío y simetría deliberadamente conservada en la primera prueba. No expandirlo a 50 edificios sin corregir ese aspecto.

## En Unity

Proyecto real: `Unity/Assets/Experimentos/Ciudad2D`. La carpeta contiene 8 PNG autocontenidos: `Terreno`, cinco edificios secundarios y `Bastion_Nivel01`/`Bastion_Nivel05`. Cuando Unity los importe, `City2DSceneBuilder` guardará la escena aislada `Experimento_Ciudad2D.unity`. Se puede reconstruir desde **Eldoria → Experimentos → Ciudad 2D → Reconstruir escena hitos I-V**.

No se tocan `ProjectSettings`, `Valoria_City_World_UI.unity`, código existente del juego ni el repositorio `main`. Cámara ortográfica acotada; retrato muestra la ciudad completa y apaisado recorta verticalmente para evitar que todo sea diminuto. Zoom de ratón/pinza y arrastre con botón medio/derecho; controlar en dispositivo físico. La UI ligera de estudio ofrece botones **I–IV / V–IX** y selección de edificios con `Physics2DRaycaster`. Los botones de estudio cambian SOLO la ilustración del Bastión, no recursos ni nivel de la partida.

### Progresión auténtica: lectura, no simulación

`City2DStateBridge.BindGateway(ICommandGateway)` permite una fuente existente sin cambiar las reglas. `ApplySnapshot(PlayerState)` consulta los niveles **BastionLevel, SawmillLevel, BarracksLevel, ChapterThree.GranaryLevel, ChapterThree.StoneworksLevel**. Los edificios con nivel `0` se ocultan, para dejar la parcela vacía. Salón de Héroes sigue siendo un marcador artístico porque no existe un nivel propio almacenado; no simularlo como progreso persistente. Las mejoras reales solo ocurren a través de los comandos ya existentes fuera del sandbox. El adaptador NO ejecuta `GameCommand`, NO muta el snapshot y NO escribe guardados.

### Comprobaciones exigidas antes de cerrar el encargo

1. Importar la rama en Unity 6000.3.23f1 y comprobar compilación sin warnings nuevos. Asegurar que se reconstruye la escena REAL en disco, no solo el marcador.
2. PlayMode: punteros en los seis edificios, HUD, cambios I↔V y adaptación portrait 390×844 / landscape 844×390 / 1170×2532. En ambos modos, clicks en edificio correcto, cámara y HUD sin colisiones/recortes.
3. Conectar un snapshot real de los cinco edificios a través de `ICommandGateway` y verificar que cambiar `BastionLevel` de 4 a 5 cambia únicamente la ilustración. Probar guardado/recarga sin modificar datos.
4. Verificar memoria de texturas, picos de render y estabilidad WebGL/iPhone/Android físico. No publicar beta hasta superar este gate.
5. Revisión del propietario comparando **captures auténticas Unity**, no solo el montaje offline. El éxito de código no equivale a aprobación artística.

## Evidencia

`QA/asset_manifest.json` contiene dimensiones, tamaño y SHA-256 de los ocho PNG y estados de pruebas. `QA/preview-I-V.png` es exclusivamente composición Pillow, no captura del motor. `QA/validate_pilot.py` valida los sprites, los límites de hitos y la ausencia de mutaciones en el adaptador. Cada defecto detectado debe documentarse antes de abrir el siguiente bloque de producción.
