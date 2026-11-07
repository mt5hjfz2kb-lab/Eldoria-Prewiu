# Eldoria — PR #173, continuación técnica y límites de aceptación

**Fecha:** 07/11/2026. **Rama:** `feature/forja-oscura-native-proof-20261107`. **Estado:** DRAFT; NO integrada en `main`; NO incluida en APK o WebGL; `ART_APPROVED=false`; `UNITY_NATIVE_PASS=false`.

## Objetivo original (no reducido a un cambio de colores)
Recrear visualmente el HUD de la ciudad a partir del kit de Forja Oscura: metal ennegrecido, bronce envejecido, texto legible, iconos de recursos reconocibles, cinco destinos, objetivo del capítulo y la tarjeta contextual del edificio real. Se conservan los `RoyalButton`, las rutas, el `RoyalHUD`, los datos de juego y las reglas: **ningún panel inventado, ningún mecanismo duplicado**.

## Correcciones en la continuación
- `ForjaOscuraLiveHudSkin`: la resolución nativa de 390×844 usa escala de Canvas `390/800`. Los cinco destinos de V2 podían quedar visualmente muy estrechos (unos **66 px** de ancho) aunque tuvieran suficientes unidades lógicas. Ahora los **cinco botones originales** se reordenan de forma reversible en **3+2** cuando el ancho físico de sus celdas es menor de 100 px; se conserva el orden canónico, sus listeners originales y el restaurado de geometría.
- El mínimo físico diseñado para un objetivo táctil/placa ilustrada es **44,5 px** de altura; se ha separado icono y rótulo en la vista compacta, con más ancho horizontal para palabras como «Construir» y «Crónicas».
- Iconos de **madera, piedra y comida**: el tamaño original de 25 unidades lógicas en vertical / 36 en horizontal podía reducirse a unos 12–16 píxeles físicos en móviles pequeños. La misma ranura real se amplía de forma reversible: la cifra conserva espacio en vertical, y en horizontal se amplía la ilustración hacia el margen libre. No se cambian cantidades.
- El V2 coloca de nuevo sus controles **en cada `LateUpdate`**. Ahora se sincronizan tamaños, colores, textos y raycasts de botones/recursos en **cada fotograma**, reservando el barrido más caro de jerarquías de tarjetas para el refresco limitado.
- `ForjaOscuraNativeEvidence`: las mediciones de texto, objetivos táctiles y raycasts se ejecutarán **después del render nativo**, no antes. Además se probará el click **real** sobre los destinos desbloqueados de una partida desechable. Los destinos bloqueados se marcarán `LOCKED_NOT_EXERCISED` y **nunca** se contabilizarán como probados; la prueba restaurará selección y vista original.
- `forja-detail-art.js`: los remaches se renderizan sobre la zona fija de esquina, también para `resource_chip`, sin atravesar las bandas estirables del 9-slice. El test compara los **píxeles** de nueve materiales generados con/sin adornos; no es una valoración artística.

## Pruebas efectivamente ejecutadas, sin extrapolar a Unity
La CI de la PR ejecuta sintaxis Roslyn, dos compilaciones semánticas **contra stubs limitados**, verificación de contratos con fuentes reales de V1/V2/V3, integridad del kit y texturas, y el modelo de geometría `check-mobile-geometry-model.py`. **No son compilación ni captura del Editor Unity.**

| Resolución | Distribución modelada | Altura táctil estimada | Anchura táctil estimada | Icono de recurso estimado |
|---|---|---:|---:|---:|
| 1600×900 | 5×1 | 106 px | 240 px | 36 px |
| 900×1600 | 5×1 | 118,12 px | 153,27 px | 28,12 px |
| 844×390 | 5×1 | 45,93 px | 104 px | 24,70 px |
| 390×844 | 3+2 | 44,85 px | 112 px | 24,38 px |

Estos valores proceden del **modelo matemático basado en código fuente**, no de un teléfono ni de una renderización. La CI del modelo verifica sus límites, pero no certifica que rótulos, cámaras, máscaras o pulsaciones de Unity respeten lo calculado. En particular, el tamaño físico de texto visible y las colisiones con tarjetas deben probarse en el Editor.

## Bloqueos reales para cerrar PR #173
1. Ejecutar Unity **6000.3.23f1** en el proyecto real con su licencia activada. El host Ubuntu de GitHub no dispone de credenciales de activación y omite el Editor.
2. Relevo formal del recurso `windows-runner-heavy` actualmente reclamado por el trabajo de **PR #163**. **No modificar sus ramas ni su ejecución**.
3. Generar ocho capturas **nativas auténticas**, antes/después para 1600×900, 900×1600, 844×390 y 390×844; revisar artísticamente las ocho y comparar con la referencia aprobada. Capturar no implica `ART_APPROVED`.
4. Validar realmente raycast, texto, cinco destinos según bloqueo/desbloqueo, cerrar/accionar contexto de construcción, cambio de ciudad/mundo, giro, restauración y juego en dispositivo móvil y WebGL. La prueba de rutas actualmente está **implementada, no ejecutada**.
5. Si la calidad visual no alcanza la referencia, **rechazar la integración** y corregir arte/disposición sobre imágenes de Unity; no fusionar por una CI verde.

**Prohibido hasta entonces:** marcar `UNITY_NATIVE_PASS`, `ART_APPROVED`, fusionar la PR o incluirla en una publicación. Esta rama no habilita una captura ni una prueba que no haya sucedido.
