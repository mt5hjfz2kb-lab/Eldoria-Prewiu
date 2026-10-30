# Eldoria — trabajo de interfaz V3 y visual Valoria APARCADO para después de Región 1
Fecha: 2026-10-24. Instrucción expresa del propietario: conservar el trabajo de interfaz y las correcciones visuales relacionadas, dejarlo ordenado y **no reanudar su producción hasta después del trabajo prioritario del mundo 4X**.

## No es cancelación, borrado ni aprobación
Se congela **solo el trabajo de continuación** de los dos workstreams de la misma sesión de interfaz/Issue #95:
- `eldoria-ui-v3-management-completion-20261021` (último responsable `chatgpt-issue95-visual-experience-20261022`, revisión 13 antes de pausar).
- `valoria-master-plan-layered-construction-20261016` (mismo responsable, revisión 20 antes de pausar): las tareas visuales de cámara/Valoria/Spawnling vinculadas a UI quedan igualmente reservadas para una reanudación posterior. **La escena Valoria existente y el trabajo ya integrado en main NO se suspenden ni se revierten.**

El mundo 4X `eldoria-world-region-1-canon-20261022`, PR #100, continúa como prioridad exclusiva de producción, con su propio responsable y su reserva de Windows. **No tocar su registro, rama, licencia de ejecutor, código, pruebas ni sus assets.**

## Lo que ya está hecho y debe preservarse
- **Interfaz V3 de gestión funcional ya fusionada** en `main` mediante PR #90, merge `284ef398e078021f4eeef42f852730714c0bd48b`: pantalla opaca de Entrenamiento, preparación de Marchas, comando real y persistencia compatibles, entrada/salida hacia la misma ciudad/HUD. Native `39653618057` (26/26 EditMode; 38/38 PlayMode) y `39654505571` SUCCESS. Documento: `docs/ui-v3/MANAGEMENT_SCREENS_V3_IMPLEMENTATION_20261021.md`.
- El ajuste visual/estratégico de Issue #95 sigue en **PR #99 DRAFT** `fix/issue95-strategic-world-spawnling-ui-20261022` (último commit `d00d5dc669967e07da2693c023e70fc5609c769a`), no fusionado ni artísticamente aprobado; mantener rama y capturas intactas.
- Se conservan los sistemas UI de `main`, PRs y artefactos existentes; no se vuelven a implementar sistemas ya fusionados.

## Lo que queda para el futuro, no para competir hoy
1. Reconstruir el estado desde el **HEAD vivo de main y el registro de workstreams**; recuperar las referencias del HUD aprobadas (Interfaz 1, Diálogo 1, Diálogo 3 reservado a Orín/cartas), sin ensanchar el HUD sobre el mundo. Asegurar botón Construir y temporizador como en el vertical slice y pantallas dedicadas de entrenamiento/héroes/marchas.
2. Retomar en aislamiento las correcciones pendientes de composición de cámara estratégica, escala del Engendro, HUD V3 y las pruebas de UI/Issue #95. Validar contra la nueva Región 1 tras su aceptación.
3. **Corregir con dueño/relevo autorizado** el workflow `.github/workflows/ui-progression-cert.yml`, que pertenece al workstream UI; es actualmente headless y deja una prueba de captura Ignored, provocando FAIL del gate incluso cuando las 57 pruebas pasan en GPU dentro del workflow del mundo. Parche propuesto para revisión, aún NO aplicado: `docs/evidence/world4x-canon-20261022/UI_CERT_GPU_CAPTURE_PROPOSED.patch`.
4. Comparar capturas reales de Unity en horizontal/vertical con UI canónica, probar regresiones I–II, guardados, cámara y retorno a ciudad; no sustituir la decisión del propietario sobre el acabado artístico.
5. Antes de cada reanudación: comprobar que el mundo ha liberado `windows-runner-heavy`, que no quedan ejecuciones nativas incompatibles y que ya ha terminado o está listo para aceptación el PR #100.

## Regla de pausa
**No reclamar ni editar los archivos de la UI / Valoria aparcados, no generar experimentos, no despachar nuevas ejecuciones dedicadas de UI y no fusionar PR #99 hasta orden expresa del propietario de reanudar tras el mundo.** El código y documentación ya integrados siguen disponibles en `main` para que el juego y sus pruebas funcionen. Este documento NO cancela acciones de GitHub disparadas automáticamente por el PR #100; sus gates pueden seguir ejecutándose o fallando mientras otro workstream tiene el runner, por diseño. **No desactivar ni debilitar las pruebas para silenciar fallos** y no invadir el workflow UI reservado mediante un trabajo del mundo.

El registro `pipeline/active-workstreams.json` deberá conservar estas dos entradas canónicas disponibles (sin dueño exclusivo ni recursos), más una pausa explícita con requisitos de reanudación. Retener íntegramente el historial de responsables, commits, capturas, pruebas y artefactos.
