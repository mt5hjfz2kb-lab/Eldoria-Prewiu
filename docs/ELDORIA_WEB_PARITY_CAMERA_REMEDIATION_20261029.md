# Eldoria — Paridad web I–V y separación de cámaras · 2026-10-29

## Autoridad y seguridad
Basado en `main` `d518576fc69efb0ba19d60757d374cc15ad3f36e` y `v0220/js/gameplay.js`, `v0220/js/hero-army.js`, `v0220/js/v030-depth.js`.
Este PR es una **corrección aislada no fusionada**. No modifica el editor de la orientación de fachadas de PR #130, no consume Tripo, no publica APK/WebGL y no cambia el modelo antiguo vetado. Reconciliar con el titular del workstream Valoria antes de merge. Nunca interpretar un test estático como aprobación de estética o teléfono.

## Cámaras: contrato de producto
- CIUDAD: cámara `city.PlayerCamera`, `city.Controls`, encuadre/cota de Valoria y gestos de ciudad. No es una vista del mapa 4X.
- MUNDO: cámara `WorldCamera` y controlador `ZoneOneStrategicCamera`. Una única geografía para los tres acercamientos: cercana 36 horizontal / 64 vertical, regional 110, panorámica 280.
- Al cambiar de ciudad a mundo, se deshabilita la cámara inactiva; el zoom del mundo no debe llamar a `city.Controls.ZoomBy`. La ciudad conserva su pose; regresar restablece la pose de ciudad, no la panorámica.
- Fallo original real: `WorldNavigation.regionalZoom=32` mientras `localZoom=36/64`, invirtiendo orden. Corregido con constantes y panorama explícita. Prueba editmode de orden y preseteado real.

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
