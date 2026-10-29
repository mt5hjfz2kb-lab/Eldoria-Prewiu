# Eldoria — Paridad web I–V y separación de cámaras · 2026-10-29

## Autoridad y seguridad
Basado en `main` `d518576fc69efb0ba19d60757d374cc15ad3f36e` y `v0220/js/gameplay.js`, `v0220/js/hero-army.js`, `v0220/js/v030-depth.js`.
Corrección autorizada en la PR132 existente, bajo el relevo del workstream canónico. Main y PR130 reconciliados. Sin gasto de Tripo ni sustitución de arte aprobado; APK1.19/WebGLnative298 siguen siendo las publicaciones previas. Nunca interpretar un test estático como aprobación de estética o teléfono.

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

## Matriz web → Unity I–V
Fuente: CHAPTERS de v0220/js/gameplay.js. Recursos con nombres originales para comparación exacta. Perfil normal OwnerIii:600 madera/500 piedra,20 reclutas,2250 poder. Los hitos permanecen en ChapterProgress.GrantedRewards; las partidas antiguas reconcilian premios alcanzados sin borrar el libro.

| ID | Objetivo | Recompensa web/Unity | Estado |
|---|---|---|---|
| c1-sawmill | Reconstruye el Aserradero | wood 120 | GrantWebMissionRewards / GrantedRewards |
| c1-wood | Recupera 600 madera | stone 80 | GrantWebMissionRewards / GrantedRewards |
| c1-stone | Recupera 500 piedra | wood 120 | GrantWebMissionRewards / GrantedRewards |
| c1-route | Despeja la ruta de Corruptos | wood 90, stone 55 | GrantWebMissionRewards / GrantedRewards |
| c1-bastion | Eleva el Bastión a nivel 2 | power 80 | GrantWebMissionRewards / GrantedRewards |
| chapter-1 | Completar capítulo 1 | wood 180, stone 140, power 120 | Libro persistente, pago único |
| c2-barracks | Construye el Cuartel | wood 140, stone 90 | GrantWebMissionRewards / GrantedRewards |
| c2-train | Entrena 20 arqueros | wood 220, stone 150 | GrantWebMissionRewards / GrantedRewards |
| c2-power | Alcanza 2.250 de Poder de expedición | power 100 | GrantWebMissionRewards / GrantedRewards |
| c2-spawnling | Derrota un Engendro de la Fisura | wood 120, stone 120 | GrantWebMissionRewards / GrantedRewards |
| c2-bastion | Eleva el Bastión a nivel 3 | food 180 | GrantWebMissionRewards / GrantedRewards |
| chapter-2 | Completar capítulo 2 | wood 240, stone 200, food 220, power 180 | Libro persistente, pago único |
| c3-granary | Construye el Granero | wood 160, stone 110 | GrantWebMissionRewards / GrantedRewards |
| c3-food | Reúne 500 comida | wood 120 | GrantWebMissionRewards / GrantedRewards |
| c3-hunt | Completa 2 cacerías | food 220 | GrantWebMissionRewards / GrantedRewards |
| c3-fissure | Somete la Fisura | stone 260, power 100 | GrantWebMissionRewards / GrantedRewards |
| c3-infra | Prepara la infraestructura de Valoria | wood 220, stone 180 | GrantWebMissionRewards / GrantedRewards |
| c3-bastion | Eleva el Bastión a nivel 4 | food 180 | GrantWebMissionRewards / GrantedRewards |
| chapter-3 | Completar capítulo 3 | wood 280, stone 240, food 260, power 220 | Libro persistente, pago único |
| c4-sawmill2 | Aserradero a nivel 2 | wood 180 | GrantWebMissionRewards / GrantedRewards |
| c4-granary2 | Granero a nivel 2 | food 220 | GrantWebMissionRewards / GrantedRewards |
| c4-barracks2 | Cuartel a nivel 2 | stone 180 | GrantWebMissionRewards / GrantedRewards |
| c4-priority | Decide qué necesita Valoria primero | power 80 | GrantWebMissionRewards / GrantedRewards |
| c4-bastion | Eleva el Bastión a nivel 5 | power 220 | GrantWebMissionRewards / GrantedRewards |
| chapter-4 | Completar capítulo 4 | wood 360, stone 300, food 280, speedup5 1, power 260 | Libro persistente, pago único |
| c5-stoneworks | Construye la Cantera de Valoria | wood 220, food 180 | GrantWebMissionRewards / GrantedRewards |
| c5-stone | Mantén 1.200 piedra disponible | wood 240 | GrantWebMissionRewards / GrantedRewards |
| c5-army | Alcanza 60 arqueros | wood 260, stone 180 | GrantWebMissionRewards / GrantedRewards |
| c5-rare | Derrota al Acechador de Ceniza | food 260, stone 180 | GrantWebMissionRewards / GrantedRewards |
| c5-bastion | Eleva el Bastión a nivel 6 | power 260 | Bloqueado: requiere VI |
| chapter-5 | Completar capítulo 5 | wood 420, stone 360, food 320, speedup5 1, power 300 | Bloqueado: requiere VI |

Decisiones y desbloqueos: prioridad IV (producción260 madera/180 piedra; defensa220 piedra/100 poder; civil180 madera/280 alimento), además del premio c4-priority de80 poder. DomainProbe comprueba las tres, repetición y recarga. T2 requiere CuartelIV,20 madera/12 piedra y30s por unidad, conserva T1. Aceleradores1/5min persisten y actúan solo en obra/Bastión/entrenamiento. El primer premio de15min es de capítuloVII: queda fuera deI–V, sin concesión artificial. Acechador exigeV, persiste victoria e informe; tres fixtures web y prueba de ruta/raycast nativa. Mejoras individuales aV siguen limitadas por Bastión y conservan sus pruebas visuales; no sustituyen los objetivos del capítuloV.

## Evidencia nativa de la corrección
Fuente Unity747763dce7c5041f061a1109690cd85a707f1398, ejecución40961914744. RecorridoIII–V:456 comprobaciones correctas. Revisión12937615601, ZIP176433052 bytes, SHA256c3a443030054388aae1bc882c9276b08464db93071e8f6ec790851d6b1de2c9a. Capturas examinadas: entrenamientoT2 horizontal, aceleración1min vertical, Acechador accesible (antes de completar el botín/reaparición), seis vistas reales de escala horizontal/vertical. Controles legibles, selección T2 preservada y geografía consistente. No es ART_APPROVED ni prueba física móvil. Suites EditMode/PlayMode, I–II y certificación general se registrarán al finalizar.

### Revisión final del combate
El web `resolveLayeredCombat0266` también entrega120 comida/110 piedra por victoria y repone el Acechador a60s. Corregido: botín en la marcha y el informe, cobro único al regresar; victoria persistente separada del temporizador. La misión c5-rare paga260 comida/180 piedra una sola vez. DomainProbe comprueba primera y segunda victoria, recarga y frontera exacta del cooldown. La primera prueba nativa747763 cubría el encuentro único inicial; debe repetirse sobre esta corrección antes de certificar el código final.
