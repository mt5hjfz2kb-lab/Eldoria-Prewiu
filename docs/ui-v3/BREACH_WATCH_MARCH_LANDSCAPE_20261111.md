# Vigilia de la Brecha · Marcha horizontal: dos héroes reales

**11/11/2026 — PR canónica #173, experimental antes de certificar.** Nada integrado en main, nada publicado y ninguna aceptación artística global inferida.

## Fallo demostrado en render real

La captura nativa Unity `real-unity-march-844x390.png` del artefacto `breachwatch-first-native-0edc9d7d0a1073b7f407cde9f4abad848bb20176-43085172334` (Actions [43085172334](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43085172334)) mostraba TRES grandes tarjetas de héroe en horizontal. La tercera decía «No disponible en esta versión» y ocupaba aproximadamente un tercio de la sección, a pesar de que el botón de selección estaba realmente deshabilitado. Esto incumple la decisión del propietario: no ofrecer funciones inexistentes ni marcos huecos.

**Causa localizada:** `BreachWatchMarchComposition.RefreshScorecard` salía inmediatamente de las orientaciones landscape, evitando por completo la composición de dos tarjetas que ya estaba implementada y validada en portrait. El gestor canónico continuaba dibujando las tres tarjetas antiguas. Ajustar solo color, márgenes o tipo no solucionaría este problema estructural.

## Corrección implementada en la misma PR

La composición en landscape llama ahora también a `RefreshActualHeroCards`, pero **sin tocar** la disposición canónica de estadísticas y tropas del paisaje. Reclama la tercera tarjeta únicamente si su Button no es interactuable. Las otras dos tarjetas son los botones y retratos originales del gestor de marchas; se expanden dentro del contenedor real, con retrato a la izquierda y texto en un espacio propio para 844×390. En dispositivos de retrato se conserva la disposición anterior. `Restore()` devuelve el estado original al cerrar.

No se crea un tercer héroe, no se abre un nuevo controlador, no se escriben guardados, no se modifica capacidad de marcha ni se altera el combate. El antiguo Canvas sigue desactivado mientras Vigilia es visible.

## Evidencia y criterio de decisión

La verificación de código independiente `validate_march_composition.py` certifica la presencia del nuevo camino horizontal, el ocultamiento condicional del slot inactivo y la restauración; el workflow de source guard queda vinculado a dicho archivo. **Esto no valida aspecto.** El ensayo nativo de esta HEAD debe probar **844×390, 1600×900, 390×844 y 900×1600**, y rechazar si aparece la tercera tarjeta, si algún retrato/texto invade sus bordes, si falla el botón existente o si la captura pierde material o jerarquía. Comparar directamente con la captura real anterior y con la referencia del propietario.

**Estado:** diseño y código integrados en la rama; la revisión de imágenes nativas y la aceptación visual siguen pendientes de resultado; no elevar a `ART_APPROVED` sin inspección. La prioridad posterior es completar Gobierno/Inventario y las demás superficies con datos y acciones auténticos, no tarjetas de demostración.
