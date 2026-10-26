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

## Integración completada; Android en curso

UI/progresión `40342562297`: escena I–II y EditMode PASS; PlayMode falla exclusivamente en `ActualPlayableWorldCapturesThreeZoomsOnDisposableProbe` por `DirectoryNotFoundException` del fixture versionado `authoring/matched-canon-camera-controls.json` (blob `688183e3bf761191e25646ad8d5f14b2f397f010`) omitido por el checkout parcial reutilizado. Preparada recuperación exacta del archivo ausente y comprobación de su blob SHA antes de Unity, sin cambiar tests, límites ni cámara. Artefacto del fallo: `12661217070`, SHA256 `075505746816a4074cd4bf091d90b609d2cde2b656466637d5200ef66d90d17d`. Tester `40342562651` se conserva hasta terminar; no se cancela por actualizar la rama. Build Android pendiente de merge y liberación real de Windows. Canal existente observado: 1.13/code13. No se afirma integración en main, APK nueva, distribución ni instalación hasta completar y registrar cada paso.

Se adapta el constructor Android existente para reconocer el workstream de la solicitud, en vez de una identidad Valoria fijada en código. Conserva el veto a procesos Unity/Blender ocupados, firma original, versión/paquete, ARM64, permisos, SHA y bytes; añade rechazo de responsable/revisión obsoletos. Pruebas locales Android: 20/20 PASS. El cambio no altera la política de distribución.

## Resultado del candidato y fusión

PR #100 fusionado en main mediante `1ac74ecec9defeb13f95a8ac7124ba84973ffd9a`, con historial completo. HEAD certificado `beb0ddb8a01de57f1a6341b20b8fecbc95a04e2b`. Certificación nativa `40347811711`: 62/62 PlayMode GPU, 26/26 EditMode, 44 checks I–II y 34 capturas nuevas. Log confirma fixture exacto `688183e3bf761191e25646ad8d5f14b2f397f010`. Artefacto `12663885258`, ZIP SHA256 `80594074f542504a40344ab658e47a1d7ae9b737fb74bdf386b96048633b05c8`.

Tester original `40342562651` PASS. Tester del candidato `40347812175` PASS en intento2, incluidas regresión dirigida, regresión general y partida nueva completa Arco I. Intento1 falló por intercepción del centro del jabalí; no se cambiaron aserciones ni juego para repetir. Checkout limitado omite solo Unity y archivos de evidencia artística; pruebas web completas conservadas. Todos los checks aplicables del HEAD finalizaron correctamente antes de fusionar.

La fusión activa automáticamente `40352938822` (certificación) y `40352938587` (control general Unity); se respeta su ejecución antes de reservar Windows para Android. Aún no se acredita nueva APK, distribución ni dispositivo físico.
