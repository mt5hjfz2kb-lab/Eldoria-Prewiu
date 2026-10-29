# Eldoria — Paridad web I–V y separación de cámaras · 2026-10-29

## Autoridad y seguridad
Basado en `main` `d518576fc69efb0ba19d60757d374cc15ad3f36e` y `v0220/js/gameplay.js`, `v0220/js/hero-army.js`, `v0220/js/v030-depth.js`.
Este PR es una **corrección aislada no fusionada**. No modifica el editor de la orientación de fachadas de PR #130, no consume Tripo, no publica APK/WebGL y no cambia el modelo antiguo vetado. Reconciliar con el titular del workstream Valoria antes de merge. Nunca interpretar un test estático como aprobación de estética o teléfono.

## Cámaras: contrato de producto
- CIUDAD: cámara `city.PlayerCamera`, `city.Controls`, encuadre/cota de Valoria y gestos de ciudad. No es una vista del mapa 4X.
- MUNDO: cámara `WorldCamera` y controlador `ZoneOneStrategicCamera`. Una única geografía: cercana 36 horizontal / 64 vertical; regional max(280, 400/aspecto); panorámica calculada con los límites reales del terreno. RegionOneCanonicalLandscape conserva la autoridad sobre escala y LOD.
- Al cambiar de ciudad a mundo, se deshabilita la cámara inactiva; el zoom del mundo no debe llamar a `city.Controls.ZoomBy`. La ciudad conserva su pose; regresar restablece la pose de ciudad, no la panorámica.
- El valor inicial regional 32 era incoherente, pero el adaptador del paisaje ya lo corregía en runtime. La inicialización ahora reutiliza los cálculos canónicos: no imponer 110/280 sobre una región mayor. La prueba nativa verifica orden, zoom continuo y aislamiento de la ciudad en ambas orientaciones.

## Paridad de misiones y economía
- Se completan los registros idempotentes de `c3-bastion`, `chapter-3`, `c4-*`, `chapter-4`, `c5-stoneworks`, `c5-stone`, `c5-army` y `c5-rare`, conservando pagos por una sola vez y perfil del propietario. No forzar `chapter-5` sin Bastión VI.
- V vuelve a enseñar Cantera → 1200 piedra → 60 arqueros → Acechador. Las mejoras individuales de edificios al V permanecen funcionales, pero no reemplazan el arco de misión.
- El Acechador tiene combate PvE y victoria persistente. **La representación provisional reutiliza un AshWolf existente, no es aprobación artística ni sustituye un modelo dedicado.**
- T2 se recluta en Cuartel IV en el mismo gateway/guardado, con coste del contrato web (20 madera + 12 piedra por arquero), sin convertir T1; la pantalla militar permite escoger T1/T2. La composición libre de varios tiers/porcentajes sigue siendo trabajo UX pendiente de validar contra los diseños recientes.
- Los aceleradores de 1 y 5 minutos son valores persistentes, se otorgan con libro único, consumen una unidad por comando idempotente y solo acortan obra, Bastión o entrenamiento. Nunca aceleran recolección.

## Comprobaciones pendientes antes de fusionar
1. Ejecutar EditMode, PlayMode, recorrido I–V, saves antiguos, ritmo offline y botones/raycast de gestión/entrenamiento. Comparar capítulos 3/4/5 con el web con fixture real; ajustar si los tests fallan.
2. QA de cámara real por horizontal/vertical y tres vistas (paisaje, LOD, Brecha, foco, hotspot, entrada/salida); cámara ciudad debe mantener su zoom separado.
3. Examinar la ruta y combate del Acechador en escena real, y aprobar/encargar su modelo definitivo sin generar ni gastar por defecto.
4. Revisar informes de combate explicativos/por ronda y composición mixta porcentual en las pantallas aprobadas, sin duplicar la UI ni sustituir diseños del usuario.
5. Reconciliar `main` y PR #130. **No hacer merge ni publicar hasta que estas comprobaciones y el dueño del workstream estén resueltos.**

## Continuación autorizada de PR132 / issue133
Titular: codex-parity132-continuation-ab04f62d0b26, workstream canónico, revisión66. Main y PR130 reconciliados sin cambiar la publicación vigente.

- DomainProbe III y IV–V ejecutados con .NET8: PASS. Nuevos casos: backfill exacto +1540 madera/+900 piedra/+1120 alimento/+780 poder; una recompensa de5min; recarga byte-idéntica; T2 no convierte T1, espera150s para5 unidades; acelerador idempotente y exclusivo de tarea.
- Combate de emboscada contrastado con tres resultados ejecutados en v0220 gameplay.js/hero-army.js: (poder2000 → victoria,193HP,6 rondas), (4200 → victoria,812HP,3), (200 → derrota,0HP,0).
- Selector visible de duración1/5min en entrenamiento y obras. El Acechador aplica daño de emboscada y reduce18% su defensa; el informe guardado lo explica.
- Prueba Unity ampliada: botones reales, T2 y guardado, acelerador elegido, cámara ciudad/mundo, tres escalas por orientación, raycast del Acechador, combate y persistencia. Pendiente ejecución/inspección antes de merge.
- Informe por ronda y editor mixto por porcentajes pertenecen a #131, que sigue preparado/no iniciado; no son condición para rehacer la UI en este parche. Sin aprobación artística del modelo provisional ni certificación de teléfono físico.
