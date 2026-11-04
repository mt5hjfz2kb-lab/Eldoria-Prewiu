# Eldoria — Talentos: reconstrucción artística real (02-11-2026)

## Origen y objetivo
PR canónica [#136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/136). Objetivo subjetivo del propietario ≥8,0/10 y aprobación después de A/B de capturas **Unity auténticas**, no imagen generada ni promesa de puntuación.

Captura A autorizada: artifact de V4.1 Unity del run `41441316111` en la PR #136 (`heroes-aldric-Talentos-{landscape,phone,small-phone,small-phone-landscape}.png` y equivalentes de Lyra); la capturadora `EldoriaV41NativeCapture` toma las mismas 8 vistas en el HEAD candidato. Referencia general aprobada exacta: `docs/heroes-ui/REFERENCIA_MAESTRA_APROBADA.png` (ilustraciones del área Talentos, no mecánicas del mockup).

## Cambios implementados en código
- `Unity/Assets/Eldoria/HeroesUI/HeroesTalentArt.cs`: crea **assets de sprite independientes** procedurales de formato real (texturas, Sprite.Create) para esmalte/metal, pergamino, cabeceras, conectores, medallones y candados. Marcos con borde nine-slice real (no imagen de la pantalla completa). Sin servicios externos, créditos de generación, dependencias de edición ni imágenes con cifras.
- `HeroesApprovedDestination.BuildReservedTalents` delega el render en `HeroesTalentArt.Build`. Conserva Canvas canónico, estructura de navegación y selector, identidad Aldric/Lyra, archivos de progreso y modelos aprobados.
- Ramas **OFENSIVA** cobriza, **DEFENSA** azul acero y **SOPORTE** dorada. Todas llevan **tres medallones** ornamentados unidos por un conector, distintivos de rama, marcos metálicos y cartel de indisponibilidad.
- Se conservan los nombres inspeccionados por QA `Heroes / reserved talent paths`, `Reserved talent path <nombre>` y `Reserved talent node <rama>-<rango>`, con nueve `Button` expresamente `interactable=false`.
- Ningún nivel, bonus, mejora, precio, recurso, habilidad, tooltip o mecánica nueva se añade. Todos los nodos son reservas visuales, sin eventos ni poderes.
- Layout adaptativo en tres columnas tanto vertical como horizontal, iconos/marcos con medida escalada; leyenda dividida en dos `Text` nativos, nunca incrustada en bitmap.

## Gate requerido antes de aprobación artística
1. Compilar Unity `6000.3.23f1` sobre el HEAD exacto de PR136; ejecutar `EldoriaV41NativeCapture.Capture`, que recoge las 4 resoluciones de la pestaña Talentos para **Aldric y Lyra**.
2. Comprobar en el árbol real de Unity tres ramas, nueve nodos bloqueados, ninguna habilitación de talentos/acciones, guardado anterior y posterior **byte por byte** idéntico, navegación lateral e identidad.
3. Revisar de forma visual las ocho capturas y comparar con A y referencia: legibilidad móvil 390x844 y 844x390, marcos no recortados, colisiones, presencia de los nueve nodos, ornamentación y color por rama.
4. Guardar A/B real y reportar defectos si los hay; no declarar ≥8/10, `ART_APPROVED` o cierre si la imagen nueva no está inspeccionada.
5. No tocar compilaciones Android/Web, `main`, modelos Tripo o la reserva Windows de otro workstream. La PR sigue DRAFT hasta el gate global y aprobación del propietario.

**Precisión:** las imágenes generadas previamente son objetivos visuales, **no capturas del juego**. Este cambio es código implementado, pendiente de certificación Unity y revisión A/B exactas.
