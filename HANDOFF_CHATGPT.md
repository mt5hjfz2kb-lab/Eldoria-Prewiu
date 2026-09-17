# Eldoria — Handoff ChatGPT

## Estado actual
Candidata pública: **v0.19.5 r7 — Vertical Slice Candidate · Polished**.
Fecha: 2026-09-17.
Repositorio de publicación: `mt5hjfz2kb-lab/Eldoria-Prewiu`.
Ruta pública candidata: `/v0195-r7/`.
La raíz apunta a r7 durante el playtest de aprobación.

## Flujo de trabajo recuperado
La publicación histórica usa GitHub Pages y, cuando el tamaño del artefacto supera lo cómodo para el conector, loaders/parches de texto sobre una base estable. r7 usa r6 como base web y aplica `v0195-r7/r7patch.js` antes de renderizar. No reconstruir desde cero ni migrar a otro hosting sin necesidad.

## Fuente de desarrollo
La baseline de desarrollo r7 es la build consolidada/separada `index.html + styles.css + config.js + app.js`. El loader web es distribución, no la nueva arquitectura fuente.

## Gate inmediato
Cristian debe jugar desde móvil sin instrucciones externas, desde partida nueva hasta el cierre/Mar de Cristal. Registrar cualquier bloqueo, overlay, botón fuera de pantalla, tutorial confuso o tramo muerto. No añadir nuevos sistemas antes de esa validación.

## Regla de veracidad
No afirmar que un E2E o playtest completo pasó si no fue ejecutado. El QA estructural r7 pasó `node --check`; el E2E local quedó bloqueado por restricciones de navegación del sandbox.
