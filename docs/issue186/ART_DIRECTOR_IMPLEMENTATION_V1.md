# Dirección artística de la incidencia #186 — primera producción

**Fuente visual:** imagen aprobada de Vigilia de la Brecha (marcos de pizarra, medallones dorados, interior burdeos) y referencia artística de Tesoros de Valoria (cofre dorado y recompensas). No adoptar la anterior variante violeta rechazada. **Estado: ART CANDIDATE / Unity visual QA pendiente.**

## Entregables creados en este chat (11/11/2026)

- `Eldoria_Issue186_Arte_Recursos_v1.zip`: 12 retratos separados RGBA de 256×256 y cuatro sprites de soporte, un manifiesto por SHA256 y una guía.
- `Eldoria_Issue186_Unity_Import_v1.zip`: estructura importable `Unity/Assets/Eldoria/Issue186Art/Resources/Issue186ResourceArt/` con esos dieciséis PNG, `.meta` estables, parámetros `TextureImporter` cotejados con #181 y #173 y documentación auxiliar.
- Previsualización de 12 recompensas con texto colocado **fuera del PNG de producción** y revisión de legibilidad a 48/64 px.
- Distintivos `badge_new.png` (correo/información sin leer) y `badge_claimable.png` (hay una acción real pendiente).
- Guía formada por `tutorial_pointing_hand.png` y `tutorial_target_halo.png`, separables para anclar y animar sin interacción táctil falsa.

**Actualización comprobada del repositorio:** los 16 sprites PNG de la variante Git candidata, sus 16 `.meta` y el manifiesto están ahora versionados en la **PR #187** bajo `Unity/Assets/Eldoria/Issue186Art/`. Esto NO los integra en `main` ni significa que Unity o iPhone los hayan probado. Los ZIP originales de las versiones de arte v5–v7 también siguen disponibles como entregables de esta conversación; no confundir sus bytes con la variante exacta Git sin comparar hashes.

## Nombres y cantidades canónicos

Para cada `resourceKind` = `wood`, `stone`, `food`:
- `pack_<resourceKind>_100.png` = 100.
- `pack_<resourceKind>_1k.png` = 1.000.
- `pack_<resourceKind>_10k.png` = 10.000.
- `pack_<resourceKind>_100k.png` = 100.000.

La función `ResourceRewardRules.SpriteKey(resourceKind,amount)` devuelve la ruta lógica de `Resources.Load<Sprite>`. Los importadores PNG **no contienen la cantidad pintada**: el valor lo dibuja la interfaz desde el estado del recurso; así no existe doble verdad entre arte y contadores. El manifiesto local valida procedencia, dimensiones y SHA256. No atribuir equivalencia de hashes entre iteraciones diferentes.

## Aprendizaje del proceso artístico

Dos composiciones ilustradas daban el nivel material y coherencia buscado, pero **incrustaban cantidad y símbolos de recurso incorrectos en la placa**. En lugar de gastar nuevas iteraciones de generación casi idénticas, se reconstruyó una cartela limpia reutilizable para cada una de las 12 ilustraciones y se separó la tipografía de la pintura. Se extrajo la mano con alfa real en vez de dejar un rectángulo oscuro sobre la pantalla. Los distintivos también se separaron del medallón original.

La revisión a 48px muestra que el objeto sigue reconocible, pero **el texto debe ir fuera de la tarjeta** a ese tamaño: usar como referencia 60–96px para tarjetas normales y revisar cada resolución real antes de producción. Los cofres de 10K y 100K se distinguen menos a tamaño diminuto; el nombre/importe accesible y el tratamiento de nivel de recompensa deben resolverlo.

## Integración pendiente — no declarar éxito de juego

1. Tras la validación artística y archivo duradero de los PNG, importar en copia aislada de Unity 6000.3.23f1; comprobar transparencias, textura, GUID, safe area, legibilidad y escala.
2. No tocar el HUD de PR #173 ni las recompensas de PR #181 sin relevo expreso y revisión del estado de ambas PR. Referenciar los sprites mediante adaptador sin superposición de UI antigua.
3. El icono `claimable` debe responder al **estado autoritativo de reclamar** y no quedar permanentemente iluminado por haber abierto el buzón.
4. El puntero debe apuntar al `RectTransform` realmente clicable, `raycastTarget=false`, pantalla visible y acción confirmada; respetar tutorial previo y persistencia.
5. Capturas Unity auténticas de mochila, buzón y tutorial en 1600×900, 900×1600, 844×390, 390×844, A/B con referencia aprobada, QA de interacción; después WebGL/iPhone físico.
6. Aceptación final del propietario tras valorar la calidad sobre esas capturas; **CI verde no equivale a ART_APPROVED**.

No se usó el PC Unity, no se publicó nada ni se modificaron partidas.

## Evidencia nueva — 11/11/2026, etapa de importación verificable

- GitHub Actions [run 43060768023](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43060768023) demostró un fallo de importación **real de datos**: 15/16 PNG de la rama tenían anchura, altura o tamaño diferentes de lo que aseguraba `issue186_manifest.json`. Era un riesgo directo de `ISSUE186_IMPORT_FAIL` en Unity, no una opinión artística.
- Se corrigieron los **15 metadatos** para representar los 16 binarios ya incorporados sin reescribir ni regenerar originales ni cambiar GUID. La verificación de fuente Python `tools/reward-experience/check_art_import.py` comprueba CRC PNG, SHA-256, dimensiones, bytes, 16 GUID únicos, `TextureImporter`, clave de carga, denominaciones y ausencia de archivos duplicados.
- [Run 43062799673](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43062799673): `ISSUE186_SOURCE_IMPORT_PASS checked=16 errors=0`, 69 pruebas de dominio y `ISSUE186_ROSLYN_SYNTAX_PASS` en **4 fuentes reales**; esto **no** es compilación ni representación gráfica de Unity.
- Se implementó `Issue186NativeEvidence.Capture()` con cuatro capturas GPU auténticas desde Canvas Unity independiente, doce sprites y números dinámicos, badge de reclamación, puntero guiado y raycast al botón real. El código existe; **ejecución nativa pendiente**. No se fabrica una imagen o un aprobado nativo al revisar sintaxis.
- Workflow nativo `.github/workflows/issue186-native.yml` preparado con preflight CAS dual, checkout exacto, artefacto PNG+informe y **bloqueo expreso si Windows sigue reservado por PR #173**. No solicita ni ocupa Windows mientras esa cesión no sea real.
- Diferencia artística pendiente: algunas piezas de la rama corresponden al arte recortado para tarjeta 256×256, mientras la lámina de objetos independientes v7 de la conversación utiliza 320×320 y badges refinados. No promover una variante por nombre: seleccionar con **capturas de Unity**, no solo por un registro de origen.

**Siguiente paso ejecutable sin intervención del jugador:** cuando el responsable de PR #173 libere formalmente Windows, reclamar recurso con CAS, actualizar el marcador `tools/reward-experience/native-request.txt` de la misma rama para disparar el workflow nativo de #187, inspeccionar cuatro PNG+reporte y corregir fallos sustanciales antes de considerar integración. El código de recompensas y la navegación de #181/#173 siguen siendo propiedad de sus titulares. La beta #179 no se modifica.
