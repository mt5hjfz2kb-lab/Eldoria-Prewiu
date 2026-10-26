# Región 1 — integración y entrega Android, 26-10-2026

## Encargo y titularidad

Mismo PR #100 y workstream `eldoria-world-region-1-canon-20261022`. Relevo autorizado por el propietario, registrado en main `02ee6c7fa5ebab7dd0098432a1364ad9dccff73c`, revisión 41, responsable `codex-world4x-integration-97fedb88`. Presupuesto 0 €. Ciudad/UI y Bastiones III–V fuera del rediseño.

## Auditoría de sustitución

- `RegionOneCanonicalLandscape.Bootstrap` y `SceneManager.sceneLoaded` instalan el adaptador en la escena integrada; espera `host.Ready` y rechaza una segunda geografía. Volver desde ciudad activa el mismo `WorldRoot` y cámara, no crea otra sesión/mapa.
- `Build` instala exclusivamente `RegionOneBlenderLandscape`: el prefab `World4X/Blender/RegionOneBlenderWorld` es la autoridad física/visual. No llama a los generadores históricos `RegionOneDirectedGeography`, `RegionOneHybridForestImpostors` ni `RegionOneGeographicDressing`. Las APIs de comparación de terreno anterior fallan explícitamente en vez de reconstruirlo.
- Los renderizadores serializados del mundo antiguo se conservan como fuentes/donantes inertes; `UpdateLevelOfDetail` los desactiva en todos los zooms, salvo el engendro Tripo existente autorizado. Los LOD de vegetación donante quedan desactivados. No se afirma borrado de archivos ni ausencia de componentes históricos en memoria: lo acreditado es una sola presentación activa, sin terreno/ecología procedurales antiguos construidos bajo ella.
- `RegionOneReferenceNodes` mantiene la ciudad aprobada y desactiva cantera/madera/rocas/ruina antiguas; `RegionOneResourceModels` usa la biblioteca aprobada. `RegionOneCreaturePopulation` comparte gateway y familias Tripo existentes. Panorámica retira también los renderizadores/sombras y selección de elementos ocultos.
- El host conserva selección, navegación, interfaz, `LocalGateway`, progreso y persistencia. Las pruebas incluyen JSON schema 1 sin campos de población, reinicio de recursos/criaturas, depleción/reaparición, rutas y reserva de ciudades. No se modifica ni borra el guardado real del propietario.

## Evidencia reutilizada y revisión

Código `75fcc8b9faaa1272d66f9b1a134c0c422db3ddb1`; materialización `80d50a4aefec2368d04d43acab9f8626f5080ee0`; candidato `731e03d741783d1d351c4098166c87bb80bac5f1`. Comparación GitHub código→candidato y materialización→candidato: cero archivos Unity modificados. Árbol Unity candidato: `fe4aca3dacc303d953eb58089b32c15720fd3a94`.

Native `40337702256`: 62/62 PlayMode GPU, 26/26 EditMode y 44 checks I–II; cero fallos/omisiones. Manifiestos PNG verificados: 160; capturas generales: 34; seis vistas comparables. Informe: `NATIVE_REVIEW_40337702256.json`.

Pruebas de sustitución relevantes: `OneGeographyAndNoInventedGameplayHotspots`, `PanoramaHidesOldSquareWithoutRemovingWorldGameObjects`, `EnemyCampPlaceholdersAreVisuallyAbsentButRealTargetsRemain`, `ThreeScalesUseSameWorldHomeWithoutCameraResetOrFakeBounds`, `ActualPlayableWorldCapturesThreeZoomsOnDisposableProbe`. Comprueban geografía única, donantes/LOD antiguos ocultos en cercana→regional→panorámica→cercana, cámara real y cobertura completa en ambas orientaciones. Los tests cargan realmente la escena integrada, no un render externo.

Revisión directa de las seis capturas nativas materializadas: geografía coherente, río continuo, ciudad/recursos legibles en cercana; Brecha geológica y territorio completo presentes en regional/panorámica. No se ha abierto una nueva campaña artística. Se conserva la valoración previa aproximada del autor, sin convertirla en aprobación externa. `ART_APPROVED=false`; dispositivo físico no acreditado.

## Integración y controles completados

PR #100 fusionado en main mediante `1ac74ecec9defeb13f95a8ac7124ba84973ffd9a`, con historial completo. Candidato certificado `beb0ddb8a01de57f1a6341b20b8fecbc95a04e2b`. Árbol Unity inalterado `fe4aca3dacc303d953eb58089b32c15720fd3a94`.

- Candidato, native `40347811711`: 62/62 PlayMode GPU, 26/26 EditMode, 44 checks I–II, 34 capturas. Artefacto `12663885258`; ZIP SHA256 `80594074f542504a40344ab658e47a1d7ae9b737fb74bdf386b96048633b05c8`.
- Main fusionado, native `40352938822`: los mismos 62/26/44 y 34 capturas PASS. Artefacto `12665337093`; ZIP SHA256 `cebb4413f15cb0952e9185ed0e9e9f24cbd4f6a49b004747e4ae459c6194c0c6`.
- Tester original `40342562651`: PASS. Tester del candidato `40347812175`: PASS intento2, incluidas regresión dirigida, general y partida nueva completa Arco I. Intento1 falló por intercepción del centro del jabalí; tests y juego sin modificar en la repetición. Checkout web omite únicamente Unity y archivos de evidencia artística. Todos los checks aplicables al PR finalizaron correctamente antes de fusionar.
- Control general Unity reparado `40357678545`: EditMode, PlayMode GPU completo sin omisiones, compilación Windows y render benchmark PASS. Artefacto `12667453051`; ZIP SHA256 `2f3ef73c062643a371c91b585c1a7aec44d13a9ce2d5f31f97c6c85433e9c44b`.

## Incidencias de preparación resueltas

`40342562297` falló exclusivamente en captura por fixture ausente del checkout parcial; se restaura y comprueba el blob exacto `688183e3bf761191e25646ad8d5f14b2f397f010` antes de Unity. No se cambian tests ni cámaras.

El control general automático de main `40352938587` usaba configuración antigua headless/sin directorio de captura y falló `Skipped:Ignored`. Corrección `b575f3d2450af1aaa888113599288795c3dd867d`: GPU, fixture exacto, directorio nuevo por intento y todos los casos obligatoriamente aprobados. Verificado en `40357678545`. La adición temporal del autotrigger por editar el propio workflow fue rechazada por gobernanza; se retiró inmediatamente en `87a23ae4ab4f514326f3614d912deccba13212a0`. Gobernanza vuelve a PASS; no se cambió su regla. El cuerpo ejecutado del control corregido y Unity son idénticos al main posterior; solo se retiró aquel disparador. No se cancelaron ejecuciones previas.

## Android — compilado y distribuido

Versión **1.14/code14**. Fuente exacta `993137ce42e416a2ff36d7acf6ce2a5a56e640cf`; construcción firmada `40360846540` PASS; artefacto `12668218748`. ZIP SHA256 `71c442c71d45a9eed16549a0624d7b6027bc1d5caea3ad8c146ec350c3f505f8`.

APK: **395.817.304 bytes**, SHA256 `453f1302e904fe281ebacf90c77271d0d984e1429b1dc3d1222a55a12a9487d0`. Paquete `com.eldoria.valoria.v5test`; certificado original `f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8`; ARM64, permisos de instalación/red, firma nativa, CRC ZIP, identidad exacta y ausencia de reset verificados. Se conserva la escena integrada ciudad/mundo y el árbol Unity certificado. Pruebas locales del canal20/20 PASS.

Promoción `40364103100` PASS: descarga del artefacto exacto, comparación pública byte a byte, validación del manifiesto y avance del canal en `10d52fb2b6ced3639af44244dbc0ada643b9f2ac`. Segunda descarga independiente del enlace público confirmó tamaño y SHA256. No se cambió la política del canal existente.

**Descarga:** https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code14/Eldoria-Valoria-code14.apk

Instalar como actualización de la aplicación existente, sin desinstalar. Android requiere confirmación del usuario. El canal familiar apunta a1.14/code14; la solicitud de construcción quedó desactivada antes de publicar y la de publicación queda desactivada al cerrar. No hay nuevas generaciones ni costes.

## Cierre y límites

Integrado en main: sí. Compilado: sí. Distribuido por el canal familiar: sí, con descarga verificada. **Instalado/probado físicamente en el móvil del propietario: no acreditado. ART_APPROVED: no acreditado.** No se presentan pruebas de editor como rendimiento o aprobación física móvil.

Workstream canónico cerrado tras entrega, revisión44, Windows y alcance liberados; otros trabajos conservados. La instalación física no deja una reserva abierta ni se presenta como realizada. La siguiente intervención solo corresponde a un defecto concreto comunicado o un nuevo encargo.
