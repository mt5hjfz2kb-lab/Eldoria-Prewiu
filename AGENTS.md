# Eldoria — Agent entry point

This repository is the source of truth. Chat history is disposable.

## ARRANQUE RÁPIDO DE UNA NUEVA SESIÓN
1. Leer `SESSION_HANDOFF.md`.
2. Leer `PROJECT_STATE.md`.
3. Comprobar rama activa y HEAD reales en GitHub.
4. Si coinciden con el handoff, empezar a trabajar directamente.
5. Leer `DESIGN_DECISIONS.md`, `ELDORIA_CONTINUIDAD.md`, `ELDORIA_BASELINE_RULES.md`, `QA_AND_DEPLOY.md`, `CHANGELOG.md` u otros documentos solo cuando la tarea concreta lo requiera.
6. No hacer una auditoría completa del repositorio en cada sesión.
7. No releer documentación extensa que no sea necesaria para la tarea actual.
8. Antes de modificar código, inspeccionar únicamente los archivos y secciones relacionados con la tarea.
9. Ejecutar primero las pruebas mínimas relacionadas con el cambio; hacer QA completo cuando corresponda por alcance o antes de declarar un hito estable.
10. Al terminar un bloque importante, actualizar `SESSION_HANDOFF.md` para que refleje exactamente dónde continuar.

`SESSION_HANDOFF.md` es un índice operativo breve, no otro documento maestro. El repositorio sigue siendo la única fuente de verdad. Los documentos históricos bajo `docs/` y versiones antiguas son referencia bajo demanda, no lectura obligatoria.

## Current state
- Active development line: `development/v0.23-clean`.
- Canonical editable game: `v0220/index.html`.
- Runtime/API version: **v0.23.14**.
- Public build is produced from that file into `playtest/index.html` by `.github/workflows/pages.yml`.
- Protected visual recovery point: commit `f139968ccbfdeb3e1d37f58568187374faf6d1f2` / branch `stable/visual-good-f139968c`. Never modify that branch.
- Current development HEAD immediately before this documentation consolidation: `feee035322acc58108bed0e41a748cbbf87539cf`.
- The real Fissure→Lyra regression test was still being debugged at that HEAD; do not describe the full game as E2E-verified until a fresh complete run actually passes.

## Non-negotiable working rules
- Never reconstruct the project from an older version. Make surgical changes to the canonical file.
- Preserve the visual baseline unless the owner explicitly asks for a visual redesign.
- Never report a fix as verified because syntax/CI is green. For gameplay/UX use Chromium/Playwright with mobile emulation and exercise the real action; for a release also test the deployed Pages URL.
- User workflow: implement → run QA → traverse with Chromium → inspect mobile/interaction → fix → retest → publish → verify published build → give one playable link.
- Prefer real `.click()`/`.tap()` over DOM `evaluate(el=>el.click())`; DOM clicks are acceptable only as diagnostic fallbacks and must be described as such.
- No routine confirmation modal for build/upgrade/gather/attack. Interaction belongs to the object: tap object → compact requirement/cost + action below it → timer above it while active.
- One important mechanic = short contextual explanation when first needed. Do not turn Aldric into a constant tutorial voice.
- Cards/relics go directly to the Codex. Equipment/materials go to Chest/inventory.
- Update `PROJECT_STATE.md` and `CHANGELOG.md` on every important commit. Update design/system docs when behavior or decisions change.
- Be exact about test coverage. “Action panel appears” is not the same as “action completed”, and state-fixture tests are not a fresh-save full playthrough.

## Before editing
Read the current canonical source and the latest workflow result. Check the current branch/commit; do not assume this document is newer than Git history. If documentation and code disagree, inspect the latest commits/tests, reconcile the docs, and do not silently choose an old document.

## Definition of done for a gameplay change
The implementation exists in `v0220/index.html`; appropriate stable test IDs exist; targeted Playwright test performs the real user interaction; regression suite passes; main deploy succeeds; published URL is checked with Chromium when the change affects playability; `PROJECT_STATE.md` + `CHANGELOG.md` reflect the result.
