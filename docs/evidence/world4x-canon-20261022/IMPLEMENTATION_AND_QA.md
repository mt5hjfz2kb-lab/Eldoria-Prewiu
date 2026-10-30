# Región 1 4X — implementación canónica candidata (22-10-2026)

**Estado: SOURCE CANDIDATE — NO ART_APPROVED, NO UNITY PASS, NO PLAYABLE RELEASE.**
Este documento no sustituye a los PNG del propietario ni a capturas reales de Unity.

## Autoridad
Canon inmutable:
- `docs/visual-reference/ELDORIA_WORLD_MAP_CANON_20261022.md`;
- `docs/visual-reference/world-map/{01_vista_cercana,02_vista_regional,03_vista_panoramica,00_triptych_aprobado_2026-10-22}.png`.
- Escena jugable activa: `Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity`.
- Entrada real: `ValoriaIntegratedWorld.TrySetWorld`, `ZoneOneStrategicCamera` y `ZoneOneGatewayHost`.
- Progresión y órdenes: `LocalGateway`, guardados e interfaz no modificados.

## Cambio introducido
`RegionOneCanonicalLandscape` se instala **solo** si la escena integrada real contiene `ValoriaIntegratedWorld`, espera a `Ready`, obtiene `WorldRoot` y `WorldHome` y añade **presentación** por debajo de la escena existente, sin WorldHotspots, colliders, eventos, unidades, gateways ni persistencia nuevos.

- Una sola malla continua de Región 1, suelo de textura procedimental determinista con continuación natural más allá del encuadre (sin vacío artificial).
- Geografía compartida para todas las vistas: agrupaciones forestales y vegetación de geometría existente, caminos y río suaves, mínimo borde marítimo oriental, relieve bajo.
- Brecha central de ramas fracturadas horizontales en el suelo, sin torre ni portal vertical ni enemigo falso.
- Distancias de zoom local 17, regional 84, panorámica 244; cámara cenital a 66°, pan acotado a geografía existente.
- Representación 3D de árboles donantes en escala cercana/regional (máximo 90 MeshRenderer reutilizados), simplificación por terreno coloreado a zoom distante.
- Cuatro bandas de niebla gráfica simple, sin volumen, activas **solo** en panorámica (zoom desde 190), desactivadas en cercana y regional.
- Sin gastos, sin cambios en escenas Unity serializadas, HUD, modelos del Engendro, reglas o comandos.

**No reemplaza** `WorldRegion1Runtime` histórico ni la rama experimental `ZoneOneWorld` aislada. Está destinado exclusivamente al `WorldRoot` del juego integrado.

## Precauciones conocidas
1. Las coordenadas y POI lógicos preexistentes siguen agrupados en el sector original: no se han inventado ciudades, nodos ni enemigos para llenar la región. La expansión geográfica es visible, **pero las interacciones distantes no aumentan**. Las imágenes ilustrativas pueden representar amenazas y ciudades futuras NO implementadas; estas se omiten por diseño y progresión.
2. Este primer render depende de un pequeño número de modelos reutilizables presentes en la escena. Sin árbol compatible, la masa forestal lejana sigue siendo textura, sin crear modelo ajeno.
3. La textura cartográfica y la geometría son una aproximación inicial. **No se puede afirmar fidelidad 10/10** a un arte de referencia volumétrico con esta fuente sola.
4. Las vistas vertical y horizontal no muestran la misma fracción del territorio, aunque el espacio world XYZ es único. Exigir revisión visual real en ambas orientaciones.
5. El costo extra GPU/CPU/ram y la interacción con el P0 de iPhone #96 NO han sido medidos. **No publicar** hasta perfilar en plataforma y resolver el reinicio independientemente.
6. `RegionOneCanonicalCapture` produce render real del WorldCamera integrado **solo en Editor PlayMode con `InWorld=true` y autorización real de acceso al mundo**; no genera pantallas conceptualizadas ni bypass de progresión. Sus imágenes no suponen hardware iPhone.

## Verificación fuente prevista (no ejecutada)
Abrir proyecto `Unity/` en Unity **6000.3.23f1**, URP **17.3.0**, desde el runner legítimo cuando no esté ocupado. Ejecutar `Eldoria.World4XCanon.Editor.RegionOneCanonicalSourceGate.Validate`. Debe verificar:
- geografía y posiciones deterministas,
- Valoria integrada y enlazada al mismo `WorldRoot`,
- script importado como MonoBehaviour.

Esto es únicamente **SOURCE PASS**, no `TECH PASS` ni `ART_APPROVED`.

## Capturas reales (pendientes hasta ejecutar Unity)
En Editor PlayMode entrar legítimamente en Mundo desde la ciudad (el aserradero/progresión deben permitirlo). Ejecutar menú:
`Eldoria → Region 1 → Capture 3 actual integrated game views`.

La utilidad requiere la escena activa real y guarda seis PNG con metadatos hash/zoom/aspecto:
- `cercana-landscape-unity.png`, `regional-landscape-unity.png`, `panoramica-landscape-unity.png`;
- `cercana-portrait-unity.png`, `regional-portrait-unity.png`, `panoramica-portrait-unity.png`.

Ruta limpia opcional mediante `ELDORIA_REGION1_CAPTURE_OUT`. Nunca sobrescribir evidencia; identificar SHA de la compilación junto a los archivos. Comparar A/B con los cuatro PNG aprobados; capturas de Editor nunca son prueba de iPhone físico.

## QA funcional obligatoria (SIN CERTIFICAR)
- Compilación y entrada Valoria → Mundo → Valoria en sesión real.
- Pan/zoom/selección de `forest-valoria`, `quarry-valoria`, `corrupt-scout`, `engendro-valoria` solo si existe/progresión, `old-watch-ruin` y ciudad.
- Marchas y retorno real, combate I–II, premios una sola vez, guardado/recarga exacta con command IDs idempotentes.
- Calidad de Brecha a todas las escalas; sin niebla regional ni unidades a tamaño panorámico; sin límites o agujeros en orientaciones 390×844 / 1280×720.
- Perfil de FPS, RAM, asignaciones, cambios de orientación y estabilidad WebGL Chromium/iPhone; reinicios P0 #96 por separado.
- UI V3 y Engendro revisados por los responsables correspondientes. Ninguna prueba de esta rama equivale a su aprobación.
- No modificar main con código no compilado o arte sin comparación. No publicar itch.io hasta QA y autorización.

## Evidencia ejecutada en este trabajo
- **INSPECCIÓN REAL** de los PNG binarios originales en `main`, además del contrato Markdown.
- Revisión de código auténtico en `main`: `ValoriaIntegratedWorld`, `ZoneOneStrategicCamera`, `WorldRegion1Runtime`, `VisualWorld`, `ZoneOneWorld`, editor de Sector Zone One y protocolos.
- Consulta del registro de workstreams y jobs GitHub; registro de una responsabilidad nueva **no solapada** bajo `eldoria-world-region-1-canon-20261022`.
- Código fuente creado en rama aislada, sin gasto, sin subida jugable ni mutaciones del guardado.
- **NO** Unity instalado en el entorno de ejecución del chat, ni runner remoto reservado/autorizado en esta sesión; ninguna captura real ni FPS inventados.

## Puertas de aceptación pendientes
`UNITY_COMPILE`, `PLAYMODE_INTEGRATED`, `CAMERA_THREE_VIEWS`, `ART_AB_REFERENCE`, `I_II_FUNCTIONAL_REGRESSION`, `WEBGL_PLAYTEST`, `IPHONE_96_30MIN`, `OWNER_VISUAL_ACCEPTANCE`, `AUTHORIZED_RELEASE`: todas **PENDING**. PR de trabajo debe permanecer **draft** hasta su prueba.


## CI nativa automática incorporada (sin aprobación implícita)

El **PR #100** posee un workflow acotado `.github/workflows/world4x-canon-native.yml`.
La primera puerta independiente `canon-source` verifica los **SHA-256 verdaderos**
de las tres imágenes aprobadas, y ya ha pasado en varios commits, incluido
`d7d7a054`. Los controles de gobernanza artística, arquitectura y workflows
han vuelto a **PASS** tras registrar la petición artística obligatoria de
coste cero en `pipeline/art-production-requests/eldoria-world-region-1-canon-20261022.json`.

La segunda puerta solo se ejecuta en el Windows self-hosted existente, previa
reserva exclusiva del runner; hace checkout restringido a `Unity/**` más el
contrato del lease, valida la importación real del Unity Editor y ejecuta la
clase `RegionOneCanonIntegratedAcceptanceTests` en la escena
`Valoria_City_World_UI`.

**Captura automatizada (nueva):** el cuarto test, en el almacén desechable
`VALORIA_GAMEPLAY_PROBE=1`, llama a la entrada real al mundo. Si necesita
aserradero, usa solo la oferta `Build` que devuelve el gateway auténtico:
no establece `InWorld` por reflexión ni altera guardados del propietario.
Entonces renderiza seis PNG de `WorldCamera` en las tres escalas,
1280×720 y 390×844, con hash SHA-256 e identidad de cámara. Si no puede abrir
Mundo por la progresión permitida, el test falla sin fingir apertura.

**Estado de las capturas:** aún no están verificadas hasta que la ejecución
nativa complete e incluya seis archivos con metadatos y tests aprobados.
Ni una captura de Unity Editor sustituye rendimiento o estabilidad iPhone.

### Puntos visuales todavía sin medir

- Desde el panorama en **paisaje horizontal**, el territorio esencial mide
  aproximadamente 850×520 unidades y el objetivo ortográfico 244 puede
  encuadrarlo; la malla continúa mucho más allá para ocultar límites.
- En **portrait iPhone**, la proporción 390×844 impide de entrada el mismo
  encuadre horizontal. Es un asunto de cámara que debe corregirse o diseñarse
  tras inspeccionar los seis PNG; no afirmar a priori que la Región 1 completa
  esté visible en ambas orientaciones.
- Los bosques interiores y la costa pequeña en el borde oriental se han
  desplazado al área encuadrada; composición, legibilidad y rendimiento son
  únicamente hipótesis hasta visualizar las capturas reales.
- La Brecha es una fractura geológica sin collider: se han corregido las
  normales de la malla para que su cara apunte a la cámara cenital.
- El adaptador escucha `SceneManager.sceneLoaded` además del primer arranque:
  se ha corregido una omisión que lo habría dejado fuera de escenas cargadas
  después del bootstrap. Falta la prueba de ejecución para certificarlo.

