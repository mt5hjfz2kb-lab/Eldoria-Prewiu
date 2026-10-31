# Eldoria — BLOQUE 01 · QA real de la última candidata antes de rehacer
**Fecha:** 2026-10-31  
**Estado del bloque:** VERIFICACIÓN COMPLETADA CON DEFECTOS ARTÍSTICOS ABIERTOS. No es ART_APPROVED ni decisión de publicación.  
**Repositorio/PR:** [#136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/136), `fix/player-feedback-context-ui-20261030`  
**Código Unity bajo prueba:** `43525a5f2efbd55efe656a969195dad9c14a4583`  
**Relevo posterior:** commits exclusivamente de `pipeline/active-workstreams.json`. El árbol `Unity/` permanece idéntico a 43525a5. Revalidar si cambia cualquier fichero Unity.  
**Run completo:** [UI Progression Certification 41232151180](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41232151180), 31/10/2026, completed/success, `head_sha=43525a5f2efbd55efe656a969195dad9c14a4583` (el artefacto completo identifica además el merge tree `43c358301c1aa0e498c5684fa170536f4646b76a`).  
**Capturas Unity reales:** artifact **13054401754** (`eldoria-v41-native-41232151180`), PNG vertical/horizontal 900×1600, 1600×900 y móvil 390×844 / 844×390. Artefacto SHA-256 `4fe552c3011e8a14404d93c1f5255b9f6460bd6d9b148f02aed5ee8d841534fa`. `result.json`: `functional=PASS`, `visual_review=REQUIRED`, `art_approved=false`.  
**Prueba integral:** workflow compartido `shared_scope/certify` terminó success: compilación, V4.1 UI/controles, buzón, tres marchas, Retención, Aldric, recorrido integrado I–V, EditMode, PlayMode y capturas. No equivale a pruebas físicas iOS/Android ni a fidelidad artística. Guard del Bastión rechazado: 1 escena, 551 dependencias, 0 assets/loaders vetados.  
**Referencia aprobada recuperada (NO regenerada):** `Panel de Héroes de Eldoria.png`, archivo original de Biblioteca ChatGPT, **SHA-256 `48cbc4e73d4144c45af12731773f0fd60b45e81d2a871256b33fe2a00f7d6800`**. Este hash coincide con el maestro artístico ya registrado en #135 y `docs/HEROES_UI_APPROVED_ART_LOCK_20261030.md`. La referencia comprende seis pantallas, no únicamente el fondo.

## Matriz de verificación visual A/B auténtica
Comparación de referencia original (seis paneles) contra PNG extraídos del artefacto **13054401754**. No confundir fotografía/render plano con componente jugable:

| Área | Observación en Unity | Resultado |
|---|---|---|
| Salón 1600×900 | Conserva geometría 3 columnas × 2 filas, Aldric y Lyra reales y 4 reservas. Falta la jerarquía aprobada de cabecera Eldoria, filtros Todos/Combate/Apoyo/Estrategia, fichas decoradas con nivel/rareza y barra inferior Volver/Héroes/Marchas/Relicario. Fondo en una sola ilustración domina la escena; tarjetas mayormente rectángulos semitransparentes. | **ART FAIL → Bloque 02** |
| Salón 390×844 | Seis cartas presentes y texto general legible, pero hay grandes tarjetas inferiores vacías y mezcla de retrato ilustrado de Aldric con modelo Lyra. Faltan filtros y navegación de la referencia. | **ART FAIL → Bloque 02** |
| Salón 844×390 | **Regresión visual concreta respecto al run 41219845900:** Aldric/Lyra quedan prácticamente ocultos detrás de las franjas opacas con los nombres. Se aprecian solo cabeza/parte del modelo; la fila inferior es oscura. La prueba de seis tarjetas/tamaño de botones pasó pero NO detectó pérdida de superficie visible del retrato. | **P0 VISUAL FAIL → Bloque 02** |
| Aldric Resumen | Personaje 3D real sobre escena con perspectiva; estadísticas Vida/Ataque/Defensa/Ruptura/Power de dominio. Pero sin composición original de protagonista dominante, sin barra inferior de miniaturas y sin panel lateral heráldico con roles. El modelo/rostro no equivale visualmente al concepto; los guanteletes aún parecen exagerados en parte de las vistas (revisar desde cámara real). | **ART FAIL → Bloque 03** |
| Aldric Habilidades | La vista existe y muestra la habilidad real Baluarte. La composición approved de habilidad por tarjeta ilustrada, iconografía y jerarquía no está; no inventar ni desbloquear habilidades no autorizadas por el dominio. | **ART FAIL → Bloque 03** |
| Talentos/Equipamiento | Bloqueos narrativos reales, correctamente sin habilitar mecánicas futuras. Pero la distribución visual no se parece a la referencia y el texto de ciertos estados es muy tenue. La referencia incluye elementos ficticios aún no jugables: reconstruir su gramática visual con reservas bloqueadas, no activar mecánicas. | **ART FAIL → Bloque 03** |
| Lyra Resumen | Render del modelo Tripo aprobado presente en Unity, controles reales y estadísticas correctas. Sigue habiendo gran distancia con la composición y el tratamiento de personaje de la lámina original. **Conservar modelo aprobado.** | **ART FAIL → Bloque 03** |
| Controles/guardados | Unity native `PASS`: 2 acciones reales, 6 slots, 4 bordes UI por slot, objetivo de cierre ≥44 px en móvil, tabs despejados de navegación inferior, guardado de héroes idéntico byte por byte. No sustituye examen visual. | **TECH PASS** |

## A/B también contra el candidato anterior
Artefacto de comparación: run [41219845900](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41219845900), SHA `89474863d7e1a85778774cf05240b411b391e115`, artifact `13049507231`; confrontado con **13054401754** en los nombres de PNG idénticos. La composición horizontal 844×390 cambia visiblemente y **pierde los retratos**; el perfil horizontal aumenta la talla de encabezados y controles, pero reduce notablemente la superficie del héroe. La mejora técnica de legibilidad de menús no autoriza esa regresión de presentación.

## Criterio para el siguiente bloque (02)
1. Reconstruir **solo Salón** siguiendo la referencia exacta arriba, sin sustituir retratos/modelos ni inventar héroes/filtros funcionales ficticios. Sí construir filtros reales únicamente si existen criterios/estado aprobados; en otro caso, mostrar reservas deshabilitadas explicadas.
2. Corregir el Salón 844×390 donde las franjas de nombre tapan retratos, **con pruebas automáticas de área visible del retrato**; no bastan botones de 44 px ni seis tarjetas.
3. Preferir componentes separados/capas estructuradas; descartar repetir retoques pequeños de bordes en una imagen fotográfica. Máximo dos intentos de la misma técnica sin progreso perceptible.
4. Capturar PNG Unity iguales en 1600×900, 900×1600, 844×390 y 390×844; verificar controles, gestos, selección Aldric/Lyra, guardado y cero Canvas/EventSystem extra.
5. Mantener PR136 DRAFT, sin merge/publicación ni costes. Bloque 03 seguirá con fichas individuales, habilidades, talentos y equipamiento; NO abarcarlo en Bloque 02.

**Cierre del BLOQUE 01:** su objetivo era determinar con evidencia qué NO debe rehacerse y qué defecto estructural concreto corresponde arreglar. Se recuperaron referencia exacta, dos generaciones de capturas nativas, se certificó funcionalidad del código de partida y se identificó una regresión artística de móvil. **No se ha corregido arte todavía ni se ha aprobado al 100%.**
