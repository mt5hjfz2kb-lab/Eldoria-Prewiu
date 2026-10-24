# Región 1 — expediente de sustitución del mundo

Estado: candidato nativo verificado, integración bloqueada por el workflow UI reservado; no fusionado ni publicado. Este expediente reúne la evidencia vigente sin convertir el PASS técnico en aprobación artística. Mismo PR #100 y workstream `eldoria-world-region-1-canon-20261022`, relevo revisión 19.

## Identidad de la versión revisada

| Dato | Valor |
|---|---|
| Fuente certificada | `009e285a0dde50570f781d5c4786bcdde1189301` |
| Fuentes editables y binarios materializados | `3caa04a3f2842db9f80aa009ac49727604c0945a` |
| Ejecución nativa | [40069960813](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40069960813) |
| Unity / URP / Blender | 6000.3.23f1 /17.3.0 /5.2.2 LTS |
| Artefacto visual | 12551467631, 165 851 557 bytes |
| SHA256 del ZIP descargado y comprobado | `2693fc3fbc0b31e9710902349e541f764d9f72901008be653894483ddc8da938` |

La documentación posterior no cambia el código o los binarios certificados. El resultado conserva las correcciones verificadas del muestreo marino, pigmento del puente y pintura geológica de la Brecha.

## Qué sustituye y qué conserva

La cámara de mundo usa la geografía Blender continua y sus materiales, vegetación y riberas. El núcleo conserva 50 688 caras, las 30 imágenes originales y las asignaciones de material/UV; la importación comprueba 96 muestras. El atlas continental solo pinta superficies nuevas. Las cámaras cercana, regional y panorámica observan las mismas coordenadas y geometría.

La región jugable ocupa 1820×3600 m. La continuación vecina real, hasta x ±1710/z ±2600, evita una terminación rectangular visible sin ampliar artificialmente los límites jugables ni inventar destinos. Unity comprueba 2416 vértices de contacto; Blender mide diferencia de altura 0 m. El perfil costero exterior actual tiene 26 416 vértices /50 116 triángulos.

LocalGateway y PlayerState mantienen la autoridad. Las representaciones visuales se apoyan en los triángulos importados; no se trasladan colliders para ocultar errores. Los recursos conservan IDs y cantidades, recolección, agotamiento, fechas de reaparición y persistencia. Valoria, UI V3, tropas, marchas, combate y Engendro siguen usando su integración existente. No se modifica el guardado del propietario.

## Certificación funcional

**57/57 PlayMode con GPU, 26/26 EditMode, 44 controles reales I–II; cero fallos y cero omisiones.** La comprobación independiente valida XML, hashes y dimensiones de 126 PNG del mundo más 34 PNG generales nuevos. Los PNG históricos AG están separados y no cuentan como capturas actuales.

| Contrato | Evidencia comprobada |
|---|---|
| Rutas reales, recursos y ciudades | `RealTerrainRoutesReachPopulationAndReserveCities`; 48 recursos y 32 reservas urbanas accesibles |
| Contacto con terreno | Comparación con triángulos importados; apoyo de modelos sin mover colliders |
| Recolección y persistencia | Comandos del Gateway, cantidades independientes, reinicio, agotamiento offline y deadline original |
| Guardados antiguos | JSON schema 1 sin campo de población; igualdad del snapshot completo y posterior reinicio |
| Marchas y combate | Visual de fase autoritativa; entrenamiento, composición, partida, llegada y recompensa mediante controles existentes |
| Bastiones I–II y Engendro | Probe real de 44 controles; capítulo II completado y guardado/recarga, anclaje del enemigo preservado |
| Cámaras y selección | Rayos de suelo válidos en el mismo frame, pan real en ambos aspectos, picking y zoom sin recolocar objetivos |
| Continuidad | Geometría única, todos los vértices jugables dentro de la panorámica, contacto exterior y dos series de zoom |
| Ciudad/UI | Pruebas existentes y 34 capturas nuevas; luz del mundo restaurada al volver a ciudad |

La suite de Editor dura 181,689503 segundos. Es duración de pruebas, **no FPS móvil**.

## Seis vistas auténticas de la versión certificada

| Escala | Horizontal | Vertical |
|---|---|---|
| Cercana | [PNG](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/3caa04a3f2842db9f80aa009ac49727604c0945a/docs/evidence/world4x-canon-20261022/sources/native-40069960813/review/cercana-horizontal-unity.png) | [PNG](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/3caa04a3f2842db9f80aa009ac49727604c0945a/docs/evidence/world4x-canon-20261022/sources/native-40069960813/review/cercana-vertical-unity.png) |
| Regional | [PNG](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/3caa04a3f2842db9f80aa009ac49727604c0945a/docs/evidence/world4x-canon-20261022/sources/native-40069960813/review/regional-horizontal-unity.png) | [PNG](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/3caa04a3f2842db9f80aa009ac49727604c0945a/docs/evidence/world4x-canon-20261022/sources/native-40069960813/review/regional-vertical-unity.png) |
| Panorámica | [PNG](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/3caa04a3f2842db9f80aa009ac49727604c0945a/docs/evidence/world4x-canon-20261022/sources/native-40069960813/review/panoramica-horizontal-unity.png) | [PNG](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/3caa04a3f2842db9f80aa009ac49727604c0945a/docs/evidence/world4x-canon-20261022/sources/native-40069960813/review/panoramica-vertical-unity.png) |

Cada PNG tiene su JSON de cámara y SHA256 junto a él. Las 100 imágenes de zoom completas permanecen en el artefacto visual; las capturas esenciales y los informes están también versionados con las fuentes.

## Revisión artística auténtica

Se consultaron los tres PNG originales de `docs/visual-reference/world-map/` y las fuentes AF/AG. El detalle fino, los claros, materiales, UV y copas del sector se preservan; las capturas AG coincidentes dan MAE RGB 0,381/255 horizontal y 0,285/255 vertical. Esto mide conservación, no calidad artística.

Se retienen la recuperación del pigmento AF, los perfiles O de ribera, las copas híbridas, el sombreado URP, los caminos conectados, los bosques continentales de contorno desigual y la continuidad física exterior. La costa exterior ya no muestra los grandes dientes del pase anterior. El grano continental derivado se calibró sobre PNG reales y conserva las fuentes originales.

Diferencias todavía visibles frente a los PNG aprobados: el continente es menos rico en variación de suelo y siluetas forestales; la Brecha tiene menor complejidad geológica; los recursos son más pequeños en la panorámica de toda la región. No se agregan objetos o amenazas ficticios para imitar una ilustración. El pase actual elimina la costura rectangular del gradiente marino, recupera el tono del puente y mejora la lectura de la incisión con pizarra acotada; las seis vistas se inspeccionaron directamente. No existe aprobación artística del propietario.

## Fuentes y fabricación reproducible

- [Recetario, parámetros, fuentes y experimentos aceptados/rechazados](AF_AG_PRODUCTION_RECIPE_20261024.md).
- [Verificador independiente del artefacto nativo](review_native_evidence.py).
- [Certificación general y reparación de las carreras de prueba/cámara](GENERAL_CERTIFICATION_REPAIR_20261024.md).
- [Mediciones reproducibles de grano en PNG auténticos](GRAIN_NATIVE_CALIBRATION_20261024.json).
- BLEND, GLB, atlas, máscaras editables, manifiesto de hashes y capturas esenciales en `sources/native-40069960813/`; originales AF/AG/AI e historial conservados.

La reproducción usa el workflow existente `.github/workflows/world4x-canon-native.yml`, reserva viva exclusiva y ejecución secuencial Blender → importación Unity → PlayMode GPU → EditMode/probe → materialización verificada. No se ejecuta otro productor mientras exista una sesión nativa incompatible.

## Integración pendiente

El código certificado se reconcilió con main `9a3865296f3a94c121c635f1e1676d6893e85379`; la actualización final del registro solo libera esta reserva y documenta el bloqueo. GitHub informó el candidato fusionable, pero con estado CI inestable por el gate UI. Antes de la integración definitiva se vuelve a comprobar main y se conservan cambios concurrentes. No se fusiona automáticamente.

El workflow general `.github/workflows/ui-progression-cert.yml` ejecuta PlayMode con `-nographics` y sin carpeta de capturas, por lo que omite una prueba de captura y su gate estricto falla. La ruta nativa propiedad de este workstream ya ejecuta las 57 pruebas con GPU sin omisiones. Está preparado [el parche mínimo](UI_CERT_GPU_CAPTURE_PROPOSED.patch): usar D3D11, carpeta única por run/attempt y adjuntar esas capturas; conserva las aserciones y exige Passed. `git apply --check` pasó sobre el blob vigente.

El gate UI automático del HEAD documental, 40071887223, pasó el recorrido nativo I–II y EditMode, y terminó con `PlayMode tests did not pass: Skipped:Ignored` (log 07:05:26 UTC). Es FAILURE, no PASS. El anterior 40069968005 había terminado en la protección de reserva sin llegar a probar Unity. La ejecución anterior 40061480616 sí demostró el defecto headless: 56 Passed, cero Failed y una captura Ignored; el workflow falló y no se presenta como PASS.

Ese archivo permanece reservado por `eldoria-ui-v3-management-completion-20261021`, revisión 13. No se ha modificado ni apropiado su alcance. La conciliación de ese archivo requiere relevo acotado autorizado o liberación de su responsable, conforme a `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`.

No hay dispositivo Android conectado ni prueba física iOS verificada. No se certifican FPS, memoria térmica o rendimiento físico, no se consumen servicios de pago, no se publica en itch.io y no se concede aprobación de integración en nombre del propietario.


### Identidad canónica disponible tras liberar la reserva

La reclamación del ejecutor, revisión 19, permanece en `history` con el bloqueo exacto. Para que el guard existente pueda atribuir los cambios de arte del PR todavía abierto, `active` conserva el mismo ID como identidad de enrutado **disponible**, revisión 20: `status:active` (identidad abierta), `owner:null`, `ownership_state:available`, `write_scope_released:true` y `resources:[]`. El protocolo permite estas identidades no exclusivas. No se vuelve a reservar Windows, ningún archivo, ni el alcance de UI; no se modifica o debilita el guard.
