# Región 1 — contrato gráfico 2.5D modular (22-11-2026)

## Objetivo de aceptación

La ciudad Bastión I aprobada marca el **mínimo de calidad visual** para las vistas cercana, regional y panorámica. La diferencia al viajar a Región 1 debe sentirse como un cambio de escala y distancia, **no** como otro juego. Una build que compila, renderiza o pasa tests no queda aprobada artísticamente sin revisar **capturas reales de Unity y del navegador móvil**. Comparar además Whiteout Survival / Rise of Kingdoms en composición, densidad y legibilidad, **sin copiar arte de terceros**.

No añadir backplates, capas de fotos por encima de terreno antiguo ni puentes visuales falsos. La representación antigua debe estar retirada de renderizado; geometría y datos canónicos se preservan **solo** cuando la física, navegación, casillas, selección o rutas los siguen utilizando.

## Autoría correcta y capa de verdad

- Ciudad: `Modular25DWorld` + `Modular25DGround` + `Modular25DLandscape` y sprites independientes. Materiales y caminos no son una imagen de ciudad completa.
- Mundo, lógica intacta: `RegionOneBlenderLandscape` proporciona altura y consultas geográficas de la Región 1 existente; `RegionOneCanonicalLandscape` mantiene límites, población, rutas, coordenadas y persistencia del gateway. Los donantes importados deben tener `forceRenderingOff=true` en modo modular; ni LOD ni materiales/luces del antiguo render AG se construyen para la imagen final.
- Mundo, **única superficie visible**: `RegionOneOriginalLandscape` construye mallas con la altura canónica; `RegionPaint.shader` muestra **en la misma superficie** las texturas de Valoria:
  - `ValoriaModular25D/BastionMeadow` (misma textura de pradera que en `Modular25DGround`).
  - `ValoriaModular25D/HaulersEarth` (caminos de tierra del kit de ciudad).
  - `ValoriaModular25D/OrganicCobbleRoad` (adoquinado original, condicionado a los caminos reales).
  - Las antiguas máscaras geográficas pueden determinar *dónde* existe camino, agua, relieve o vegetación, nunca funcionar como una imagen pintada completa que domine el color.
  - Se conecta el borde geométrico del relieve a la bruma mediante una falda **sin collider**, no una lámina plana flotante.
- Arquitectura: `RegionOneReferenceNodes` obtiene directamente el sprite de Bastión I (`bastion_i_hd` o `bastion_i`) y los sprites `Progression/bastion_2` … `bastion_5`. No se usan imágenes sustitutas `World4X/Illustrated/Keep1–5`.
- Vegetación: `RegionOneOriginalLandscape.Plant` reutiliza `RecoveredProps/ArbolAntiguo`, `RecoveredProps/ArboledaAntigua` y los sprites ilustrados `World4X/Illustrated/Fir` y `Grove`, sembrados en posiciones geográficas canónicas. **Revisión directa de PNG:** `ValoriaModular25D/pine_0` y `pine_1` eran árboles verdes *low-poly*, no cumplían el acabado ilustrado del Bastión aprobado y **se retiraron del render mundial** (commits `1cb00dc` y `01ceefd`). `Fir`/`Grove` son dibujos ilustrados independientes con alpha y mipmaps; no un regreso a renderizado 3D heredado. La verificación de la imagen final en Unity todavía es obligatoria.
- Recursos, enemigos y fisura: figuras 2.5D independientes `RegionOneModularArt.Place` sobre los hotspots existentes; donantes 3D ocultos pero utilizables para los cálculos y colisiones que requiera la implementación. Los atlas del render antiguo no deben cargarse innecesariamente.

Esta coincidencia de fuentes y lenguaje no implica que la estructura interna sea idéntica (la ciudad es un plano modular en XY; el mundo debe conservar su terreno geográfico en XZ). Se evalúa *el resultado visual integrado*, no la identidad literal de los algoritmos.

## Evidencia, sin falsear resultados

- Último recorrido WebGL **anterior** a la última sustitución del mundo: run `44996612315` (SHA `cb7663c`). Se comprobó en Chromium emulando móvil desde la elección del nick hasta Bastión V; tres viajes de ida y vuelta, y salas reales. **No valida el nuevo shader ni la nueva vegetación**. Se registraron hasta ~606 MB de memoria asignada y errores de renderizado: optimización e iPhone físico siguen pendientes.
- Las modificaciones recientes incluyen exactamente recursos de ciudad, ocultación y retiro de render del GLB, falda de perímetro, y aserciones en `RegionOneCanonIntegratedAcceptanceTests.CoronaRegionRendersAndRetainsGeographicRoutes`. No proclamar calidad aprobada antes de recibir imágenes nuevas.
- Flujo nativo: `.github/workflows/valoria-region-modular-native.yml`. El estado de commit `eldoria/valoria-2-5d-visual-proof` incluye en `target_url` la ejecución auténtica de Actions. Leer su `sourceSha` en `evidence.json` y contrastarlo con la revisión de código. Los trabajos `verify-packaging`, `preflight`, `unity` y `mobile-journey` deben aprobarse sin omitir escenas. Los artefactos `ciudad-cinco-bastiones-unity`, `region1-cameras-and-routes`, `ciudad-region-recorrido` permiten inspección pixel a pixel.
- Antes/después de cualquier cambio, las rutas y `grid.bin` deben coincidir byte a byte con el control base. Si una optimización del render toca geometría de gameplay, se revierte.
- Comparar en la misma cámara y formato: Bastión I–V con detalle visible, zona de ciudad del mundo, pradera y orilla próxima al puente, forestación, La Brecha, Región 1 completa y bordes. Observar en movimiento (pan/zoom) que no aparezcan sprites flotantes, duplicados ni costuras.
- Validar siempre iOS real por separado: **no** inventar estabilidad física a partir de Chromium.

## Bloqueos que justificaron los cambios

1. El mundo usaba un enorme color geográfico heredado y pigmentos PBR del GLB, con otra escala de detalle, terreno/camino uniformes y una silueta de borde separado.
2. Tenía un castillo sustituto `Keep1–5` distinto al aprobado y con anclas fijas; se sustituyó por sprites del Bastión real y sus pivotes.
3. Los árboles eran de una familia distinta a la de la ciudad; se cambiaron por árboles y agrupaciones de la familia Valoria con siembra espacial.
4. Se seguían recreando iluminación, materiales, LODs y atlas del antiguo 3D aunque el nuevo render los ocultara; se retiró ese trabajo, **no** las consultas geográficas.
5. La transición ciudad/mundo tuvo una prueba de regreso con un falso positivo por telemetría de escena antigua; `journey-browser.py` ahora exige escena activa correcta y cortina oculta.

## Pendiente de aprobar

1. Compilación, PlayMode, WebGL y capturas auténticas para el **último** commit de esta rama, no de candidatos anteriores.
2. Revisión crítica frente a Bastión I y referencias comerciales, con correcciones enfocadas en densidad de bosque, carreteras, escala de castillo, borde oriental, calidad de pradera y texturas integradas, sin agregar capas superpuestas.
3. Confirmar memoria, tiempo de entrada al mundo, ausencia de errores y estabilidad de pan/zoom/juego completo B1–B5 después del cambio.
4. Salas Héroes/Entrenamiento/Marcas/UI fieles al kit aprobado (mismo lenguaje visual, navegación canónica de main).
5. Publicación privada **solo** cuando pase calidad y funcionalidad, nunca merger a `main` sin permiso.

Trabajar de forma acumulativa: distinguir siempre **implementado, compilado, renderizado, validado visualmente y probado en móvil físico**. Si la captura no alcanza calidad, corregir el origen concreto antes de anunciar victoria.
