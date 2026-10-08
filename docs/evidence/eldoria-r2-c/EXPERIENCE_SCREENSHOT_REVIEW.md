# R2-C · Revisión visual REAL del WebGL estable — cámara y edificios

Fecha: 2026-10-09. Fuente técnica autentificada: GitHub Actions [#37841358217](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37841358217), artifact **#11577389259**, `eldoria-webgl-startup-evidence`. Inspeccionadas imágenes originales `home-landscape.png`, `pan-vertical-landscape.png`, `landscape-selected-sawmill.png`, `landscape-selected-bastion.png`, `portrait-selected-sawmill.png`, `portrait-selected-barracks.png`. Chromium con emulación móvil, no iPhone físico. Se conserva la observación directa del propietario como evidencia superior de defectos de experiencia.

## EXP-CAM-01 — desplazamiento vertical insuficiente
Las capturas `home-landscape.png` y `pan-vertical-landscape.png` demuestran que ocurre un desplazamiento visible pero NO prueban cobertura suficiente del área Valoria: cambian el encuadre de la puerta y el terreno en vertical mientras continúan recortadas zonas de la ciudad. La regla canónica pide ciudad navegable, cámara con límites y primer encuadre cerrado, no panorámica completa. El código `ValoriaParcelPresentation.Pan` aplica factor vertical .006 y límite ±1.10, frente al factor horizontal .012 y límite ±2.80 (WebGL horizontal además depende de la capa de splats). **Estado P1: OPEN / NOT FIXED.** Responsable R2-C/D09; no atribuir a una sola constante sin medir ambas orientaciones, gestos repetidos y logs `ELDORIA_PLAYABLE_TOUCH_PAN`.

## EXP-BLD-02 — interacción con edificios
Las capturas reales muestran paneles contextuales visibles y al menos una CTA funcionalmente ubicable, pero distribución inconsistente: `landscape-selected-sawmill.png` ubica el panel ASERRADERO en el tercio inferior izquierdo, `landscape-selected-bastion.png` ubica BASTIÓN en el tercio superior derecho; ambos ocupan superficies de escena importantes. En `home-landscape.png` aparece además una CTA global grande `RECONSTRUIR ASERRADERO` sobre el espacio jugable. Las capturas portrait demuestran paneles renderizados, pero no garantizan hitboxes táctiles cómodos. **Estado P1: OPEN / NOT FIXED** conforme al test personal del propietario. Responsable R2-C/D10+D09; requiere selección táctil real, hitbox, CTA visible/pulsable, reacción tras acción, cierre, reload y comprobación de superposiciones, sin invadir el scope R2-B `SlicePresenter.cs` durante su candidato ni M07 SHARP editor.

## Aceptación específica, no sintética
- Para cámara: documentar inicio, arrastre largo en vertical arriba/abajo, imagen final y límites en 844×390 y 390×844; verificar recorrido de las áreas previstas y preservar zoom/encuadre inicial de autoridad.
- Para interacción: Aserradero, Cuartel y Bastión, estados disponible/construcción/built, toque sobre collider, respuesta de panel, CTA, cierre, persistencia; capturas y logs de build real.
- Comparar con `docs/VALORIA_BASTION_I_II_VERTICAL_SLICE_MIGRATION_V1.md`, `docs/VALORIA_MASTER_PLAN_AND_PROGRESSION_REQUIREMENTS.md` y el vertical slice; no añadir controles no implementados.
- La prueba automatizada publicada previo `PASS` valida el macroloop pero NO revoca EXP-CAM-01 ni EXP-BLD-02.
- No se ha modificado Unity ni emitido un PASS visual/experiencia por crear este informe.

## Entrega al Coordinador
Usar este artefacto y las capturas del QA estable como baseline para D09/D10. Exigir recepción del owner R2-C y claim de archivos preciso ANTES de parchear código (R2-B todavía reclama `SlicePresenter.cs`). M07 mantiene sus archivos y aceptación pendiente. Presupuesto externo 0 €.
