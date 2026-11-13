# Eldoria — métricas trazables del recorrido real Bastión I–V

**Alcance confirmado:** 135 indicadores posibles, 17 ámbitos de juego, 67 tipos de evento permitidos por el servidor.

> Importante: se ha revisado el código Unity en main (7/11/2026), las reglas de progresión y la partida I–V. La telemetría todavía no está integrada en la WebGL ni validada en un teléfono real. Ninguna cifra constituye resultados de jugadores.

## Fuera de esta beta

Códice, cartas jugables, duelos/Relicario, equipamiento avanzado, Forja, Bastión VI–X, PvP, Hospital, alianzas y tropas T3–T5 operativas. El Manuscrito de la Fisura en el Arcón sí puede existir como premio único y se mide solo como hito narrativo, no como carta.

## Cómo interpretar

- **P0**: imprescindible para decidir si la prueba fue aprovechable; **P1**: diagnóstico adicional si la mecánica es alcanzable y hay consentimiento.
- `PLANNED_NOT_INSTRUMENTED` significa catálogo planeado: se requieren hooks de Unity, pruebas de recepción HTTPS y asociación al commit jugable. No debe interpretarse como medido.
- Los tiempos deben descontar intervalos en segundo plano; los temporizadores de producción y expediciones se miden con reloj real, no con reloj de QA.
- En modo QA, `is_qa=true`. Ausencia de evento no significa fracaso del jugador hasta demostrar que ese hook funciona en la build exacta.

## Acceso, carga y continuidad

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B001 | Sesiones consentidas iniciadas | `session_start` | P0 | I-V | Sesiones únicas tras consentimiento; separar QA y versiones (`runtime`) |
| B002 | Sesiones con mundo jugable cargado | `game_ready` | P0 | I-V | De inicios consentidos a game_ready (`runtime`) |
| B003 | Carga inicial en milisegundos | `game_ready` | P0 | I-V | Mediana de elapsed_ms hasta primer frame jugable (`runtime`) |
| B004 | Etapa de carga lenta | `load_stage` | P1 | I-V | Duración por stage_code de descarga/arranque (`runtime`) |
| B005 | Fallos de arranque | `client_error` | P0 | I-V | Errores error_code de arranque por dispositivo (`runtime`) |
| B006 | Recargas durante partida | `save_state` | P1 | I-V | Eventos restored tras nuevo arranque, por sesión/invitación (`GameState.cs`) |
| B007 | Fallos de guardar o restaurar | `save_state` | P0 | I-V | state_code failed frente a attempted/restored (`FileStateStore`) |
| B008 | Regreso tras cerrar sesión | `return_visit` | P0 | I-V | Dos sesiones diferentes de una invitación dentro de la ronda; no implica personas distintas (`runtime`) |

## Tutorial, comprensión y narrativa inicial

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B009 | Nombre configurado sin ayuda | `player_name_set` | P0 | I | Ratio completed/attempted, sin recoger el nombre (`PlayerIdentityPolicy.cs`) |
| B010 | Tiempo hasta primera acción con objetivo | `first_objective` | P0 | I | elapsed_ms desde carga; no se interpreta como comprensión subjetiva (`GameState.cs`) |
| B011 | Ayudas de primer uso mostradas | `tutorial_step` | P1 | I-V | step_code y estado visible por Bastión (`UnityContextTutorial.cs`) |
| B012 | Ayudas repetidas | `tutorial_step` | P1 | I-V | reopened por step_code y sesión (`UnityContextTutorial.cs`) |
| B013 | Objetivos completados sin usar Ir al objetivo | `objective_state` | P0 | I-V | completed sin help_opened/ui_action de guía en mismo objetivo (`GameState.cs`) |
| B014 | Objetivos consultados repetidamente | `objective_state` | P1 | I-V | Número de viewed por step_code antes de completar (`GameState.cs`) |
| B015 | Bloqueos por no saber qué hacer | `blocked` | P0 | I-V | reason_code no_se_que_hacer por Bastión (`ui`) |
| B016 | Claridad percibida del comienzo | `survey_answer` | P0 | I | question_id first_minutes_clarity, 1–5 o no_llego (`encuesta`) |

## Interfaz y navegación real

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B017 | Pantallas descubiertas | `screen_viewed` | P1 | I-V | Número y lista codificada por sesión (`SlicePresenter.cs`) |
| B018 | Última pantalla observada | `screen_viewed` | P1 | I-V | Última pantalla antes de cierre/periodo sin datos; NO es prueba de abandono (`SlicePresenter.cs`) |
| B019 | Botones rechazados | `input_rejected` | P0 | I-V | action_code por pantalla y razón (`LocalGateway.cs`) |
| B020 | Rechazos por revisión obsoleta | `gateway_command` | P0 | I-V | Recuento failed por reason_code estado_desactualizado (`LocalGateway.cs`) |
| B021 | Tasa de acciones aceptadas | `gateway_command` | P0 | I-V | Completed sobre attempted por command_code; no contar solo clics (`LocalGateway.cs`) |
| B022 | Retrocesos y vueltas sin objetivo | `navigation_backtrack` | P1 | I-V | Frecuencia por screen y bastion (`SlicePresenter.cs`) |
| B023 | Dificultades por orientación | `orientation_state` | P0 | I-V | Bloqueos y errores por portrait/landscape (`SlicePresenter.cs`) |
| B024 | Localización de controles | `survey_answer` | P0 | I-V | question_id ui_findability escala 1–5 (`encuesta`) |

## Ascensos, objetivos I–V y final

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B025 | Alcance de Bastión I, II, III, IV, V | `bastion_reached` | P0 | I-V | Sesiones únicas por nivel, denominador población elegible (`GameState.cs`) |
| B026 | Tiempo I→II | `bastion_reached` | P0 | II | Tiempo entre hitos 1 y 2 (`GameState.cs`) |
| B027 | Tiempo II→III | `bastion_reached` | P0 | III | Tiempo entre hitos 2 y 3 (`GameState.cs`) |
| B028 | Tiempo III→IV | `bastion_reached` | P0 | IV | Tiempo entre hitos 3 y 4 (`GameState.cs`) |
| B029 | Tiempo IV→V | `bastion_reached` | P0 | V | Tiempo entre hitos 4 y 5 (`GameState.cs`) |
| B030 | Intentos de ascenso fallidos | `gateway_command` | P0 | II-V | AdvanceBastion failed, con razón y recursos/situación (`LocalGateway.cs`) |
| B031 | Hitos de misión por capítulo | `milestone_reached` | P0 | I-V | milestone_code por Bastión, sin inferir disponibilidad no mostrada (`ChapterProgressState`) |
| B032 | Objetivos de cierre V alcanzados | `end_of_slice` | P0 | V | Punto V 4/4 estado completed, independiente del nivel V (`PLAYER_AUDIT_20261106.md`) |
| B033 | Intentos de continuar a VI | `input_rejected` | P1 | V | reason_code fin_contenido y respuesta del jugador (`PLAYER_EXPERIENCE_FIXES_20261106.md`) |

## Ciudad, edificios y mejoras

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B034 | Primer Aserradero creado | `build_completed` | P0 | I | building_code sawmill / nivel 1 (`GameState.cs`) |
| B035 | Primer Cuartel creado | `build_completed` | P0 | II | building_code barracks / nivel 1 (`GameState.cs`) |
| B036 | Primer Granero creado | `build_completed` | P0 | III | building_code granary / nivel 1 (`GameState.cs`) |
| B037 | Primera Cantera creada | `build_completed` | P0 | V | building_code stoneworks / nivel 1 (`GameState.cs`) |
| B038 | Mejoras por edificio y nivel | `build_completed` | P1 | I-V | building_code, level; máximo el del Bastión (`LocalGateway.cs`) |
| B039 | Obras iniciadas y completadas | `build_state` | P0 | I-V | Emparejar started/completed por building_code y tarea autorizada (`LocalGateway.cs`) |
| B040 | Obras rechazadas por recursos o simultaneidad | `build_state` | P0 | I-V | failed por reason_code y building_code (`LocalGateway.cs`) |
| B041 | Tiempo de espera real de obra | `timer_wait` | P1 | I-V | duration_ms esperado, tiempo transcurrido hasta completado (`LocalGateway.cs`) |
| B042 | Reconocimiento de mejora de producción | `survey_answer` | P1 | I-V | question_id building_clarity, 1–5 (`encuesta`) |

## Recursos y producción pasiva

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B043 | Primer recurso del mundo recibido | `gather_state` | P0 | I | state_code completed por resource_code (`LocalGateway.cs`) |
| B044 | Cantidad madera/piedra/comida ganada | `resource_state` | P1 | I-V | amount positiva por recurso y fuente codificada (`ResourceWallet`) |
| B045 | Cantidad madera/piedra/comida gastada | `resource_state` | P1 | I-V | amount por recurso, evento spent (`ResourceWallet`) |
| B046 | Intentos con recursos insuficientes | `input_rejected` | P0 | I-V | reason_code recurso_insuficiente por action_code (`LocalGateway.cs`) |
| B047 | Producción pasiva por edificio | `production_state` | P1 | I-V | Delta consolidado del gateway por edificio y ventana, no fingir botón recoger (`EconomyProductionV2.cs`) |
| B048 | Progreso sin conexión | `production_state` | P1 | III-V | state_code accrued_offline y amount según reloj permitido (`EconomyProductionV2.cs`) |
| B049 | Claridad de recursos y costes | `survey_answer` | P0 | I-V | question_id economy_clarity 1–5 (`encuesta`) |

## Mundo, nodos, mapa y recogida

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B050 | Primera visita al mundo 4X | `world_opened` | P0 | I | Tiempo desde game_ready (`WorldRegion1Runtime.cs`) |
| B051 | Tipos de nodos reconocidos/seleccionados | `world_node` | P1 | I-V | type_code wood/stone/food/enemy, state selected (`WorldResourceLocation`) |
| B052 | Intentos sobre nodo agotado o inaccesible | `world_node` | P0 | I-V | state_code failed con reason_code de nodo (`LocalGateway.cs`) |
| B053 | Rutas de recursos iniciadas | `gather_state` | P0 | I-V | started por resource_code y nivel de nodo (`LocalGateway.cs`) |
| B054 | Regresos con recursos al reino | `gather_state` | P0 | I-V | completed y amount; comparación con iniciadas (`MarchState`) |
| B055 | Zoom cerca/regional/panorámico usado | `world_zoom` | P1 | I-V | Conteos de zoom_bucket, no capturar gestos brutos (`WorldRegion1Runtime.cs`) |
| B056 | Problemas de encontrar objetivos | `blocked` | P0 | I-V | reason_code no_encuentro_boton o sin_objetivo en mundo (`PLAYER_AUDIT_20261106.md`) |
| B057 | Curiosidad por explorar más | `survey_answer` | P1 | I-V | question_id explore_interest 1–5 (`encuesta`) |

## Reclutamiento, tropas y marchas

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B058 | Reclutamientos iniciados | `training_state` | P0 | II-V | started con quantity, tier permitido y UTC (`LocalGateway.cs`) |
| B059 | Reclutamientos terminados | `training_state` | P0 | II-V | completed con tropas disponibles (`LocalGateway.cs`) |
| B060 | Primera preparación de marcha válida | `army_prepared` | P0 | II | completed por héroe y número de tropas (`LocalGateway.cs`) |
| B061 | Errores de composición | `march_state` | P0 | II-V | failed con reason_code composicion_invalida o tropas_insuficientes (`LocalGateway.cs`) |
| B062 | Tiempo hasta primera marcha | `march_sent` | P0 | I-V | elapsed_ms desde inicio listo (`LocalGateway.cs`) |
| B063 | Tipos de marcha por destino | `march_sent` | P1 | I-V | target type_code recurso/enemigo, no coordenadas exactas (`MarchState`) |
| B064 | Uso simultáneo de los tres huecos | `march_state` | P1 | II-V | march_slots_used en momentos de envío; sin inventar 4.º (`MarchCapacityRules`) |
| B065 | Tropas comprometidas en otras actividades | `march_state` | P1 | II-V | reserved_troops vs available al intento (`MarchCapacityRules`) |
| B066 | Uso de T1/T2 disponibles | `army_prepared` | P1 | II-V | tier y quantity; no T3–T5 como desbloqueados (`GameState.cs`) |

## Combate automático, caza y amenazas

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B067 | Primera batalla PvE finalizada | `battle_resolved` | P0 | II | Tiempo desde primera marcha hasta resolución (`LocalGateway.cs`) |
| B068 | Combate Engendro resuelto | `battle_resolved` | P0 | II | enemy_code engendro, outcome_code (`GameState.cs`) |
| B069 | Batallas victoria/derrota | `battle_resolved` | P0 | II-V | Por enemy_code y composición; sin inventar heridas PvP (`EldoriaV41Combat.cs`) |
| B070 | Cacerías lobo y jabalí | `hunt_state` | P0 | III | started/completed, enemy_code wolf/boar (`LocalGateway.cs`) |
| B071 | Resultados y recompensas de caza | `hunt_state` | P1 | III-V | state_code completed y resource_code / amount si se acredita (`LocalGateway.cs`) |
| B072 | Acechador de Ceniza derrotado | `ash_stalker_state` | P0 | V | completed después de Bastión V (`GameState.cs`) |
| B073 | Reintentos de encuentros y fallos | `battle_state` | P1 | II-V | failed por reason_code/ enemy_code (`LocalGateway.cs`) |
| B074 | Comprensión del combate automático | `survey_answer` | P0 | II-V | question_id combat_clarity, escala 1–5 (`encuesta`) |

## Fisura de Bastión III y Lyra

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B075 | Inicio de investigación de Fisura | `fissure_state` | P0 | III | investigating, tras alcanzar III (`LocalGateway.cs`) |
| B076 | Investigación concluida | `fissure_state` | P0 | III | investigated y duración 1200s normal (`BastionThreeState`) |
| B077 | Bloqueos por Aldric ocupado o ruta | `fissure_state` | P1 | III | failed con reason_code hero_unavailable/ruta_inaccesible (`LocalGateway.cs`) |
| B078 | Primer contacto narrativo con Lyra | `lyra_state` | P0 | III | contacted, no equivale a reclutada (`NarrativeCoherenceRules.cs`) |
| B079 | Inicio de asalto a la Fisura | `fissure_state` | P0 | III | assault_started, no antes de contacto (`LocalGateway.cs`) |
| B080 | Asalto y cierre de Fisura | `fissure_state` | P0 | III | closed con outcome_code victoria (`BastionThreeState`) |
| B081 | Reclutamiento real de Lyra | `lyra_state` | P0 | III | recruited DESPUÉS del asalto ganado (`GameState.cs`) |
| B082 | Manuscrito único en Arcón | `milestone_reached` | P1 | III | milestone_code fissure_manuscript_recibido; no es carta jugable (`BASTION_III_FISSURE_CANON_20261027.md`) |
| B083 | Interés por Lyra y misterio | `survey_answer` | P1 | III-V | question_id lyra_fissure_interest 1–5 (`encuesta`) |

## Consejo IV, Cantera V y cierre

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B084 | Presentación de prioridades del consejo | `city_priority` | P0 | IV | viewed cuando Bastión IV ofrece decisión (`LocalGateway.cs`) |
| B085 | Elección producción/defensa/civil | `city_priority` | P0 | IV | choice_code production/defense/civil committed (`BastionThreeState`) |
| B086 | Cambio de recursos por elección | `resource_state` | P1 | IV | delta ligada a choose_development (`LocalGateway.cs`) |
| B087 | Dudas antes de elegir prioridad | `city_priority` | P1 | IV | viewed repetido o ayuda solicitada antes de completed (`PLAYER_AUDIT_20261106.md`) |
| B088 | Primer uso económico de Cantera V | `production_state` | P0 | V | stoneworks nivel 1 produce piedra; no basta con crearla (`EconomyProductionV2.cs`) |
| B089 | Subidas independientes a nivel V | `build_completed` | P1 | V | building_code y nivel; Bastión V no sube automáticamente edificios (`docs/bastion-iv-v/MIGRATION.md`) |
| B090 | Desbloqueos reales V y 4/4 | `end_of_slice` | P0 | V | completed tras 4 objetivos de V, distinto de alcanzar V (`PLAYER_AUDIT_20261106.md`) |
| B091 | Comprensión del fin disponible | `survey_answer` | P0 | V | question_id slice_end_clarity o not_seen (`encuesta`) |

## Mesa de Exploradores

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B092 | Acceso real a Exploradores | `screen_viewed` | P0 | II-V | screen scouts tras II, Cuartel y primera marcha (`EarlyRetentionRules.cs`) |
| B093 | Contratos disponibles descubiertos | `scout_order` | P1 | II-V | viewed con order_code familia, no ID original (`EarlyRetentionRules.cs`) |
| B094 | Contratos enviados | `scout_order` | P0 | II-V | sent por tipo de encargo (`EarlyRetentionRules.cs`) |
| B095 | Elección de estrategia | `scout_order` | P1 | II-V | choice_code/strategy_code y tropas comprometidas (`EarlyRetentionRules.cs`) |
| B096 | Contratos finalizados | `scout_order` | P0 | II-V | completed por orden y duración real (`ScoutOrder`) |
| B097 | Recompensas reclamadas una sola vez | `scout_report` | P0 | II-V | claimed, evitar doble premio (`EarlyRetentionRules.cs`) |
| B098 | Errores de plazas o tropas reservadas | `scout_order` | P0 | II-V | failed por reason_code y slots usados (`EarlyRetentionRules.cs`) |

## Camino de los Vigilantes

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B099 | Primera entrada al Camino | `screen_viewed` | P0 | I-V | screen vigilants tras Aserradero I (`EarlyRetentionRules.cs`) |
| B100 | Etapas intentadas | `vigilant_attempt` | P0 | I-V | level real y tropas/héroe disponibles (`EarlyRetentionRules.cs`) |
| B101 | Etapas superadas | `vigilant_result` | P0 | I-V | win por level; no duplicar una victoria antigua (`EarlyRetentionRules.cs`) |
| B102 | Dificultad por tramo de 5 etapas | `vigilant_result` | P1 | II-V | Derrotas y vida restante en guardianes 5/10/15/20/25 (`EarlyRetentionRules.cs`) |
| B103 | Reintentos por derrota | `vigilant_attempt` | P1 | I-V | Intentos por level y mismos recursos disponibles (`EarlyRetentionRules.cs`) |
| B104 | Errores por tropas ocupadas | `vigilant_attempt` | P0 | II-V | failed por falta de marchas/tropas (`EarlyRetentionRules.cs`) |
| B105 | XP pasiva disponible/reclamada | `vigilant_xp_claim` | P1 | II-V | amount y tiempo; acumulación límite 8h (`EarlyRetentionRules.cs`) |
| B106 | Progresión máxima por Bastión | `vigilant_result` | P0 | II-V | Máxima etapa completada por Bastión; 25 primer capítulo, más solo si alcanzables (`EarlyRetentionRules.cs`) |

## Héroes, maestría y Poder

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B107 | Ficha de Aldric consultada | `hero_opened` | P1 | I-V | hero_code aldric (`GameState.cs`) |
| B108 | Ficha de Lyra consultada tras reclutar | `hero_opened` | P0 | III-V | hero_code lyra, elegible solo reclutada (`GameState.cs`) |
| B109 | Uso de uno/dos héroes en marcha | `army_prepared` | P1 | III-V | hero_code aldric/lyra/aldric_lyra y disponibilidad (`LocalGateway.cs`) |
| B110 | Niveles de maestría alcanzados | `hero_state` | P1 | I-V | level por héroe, escalado específico PvE temprano (`EarlyRetentionRules.cs`) |
| B111 | Incrementos reales de Poder | `power_changed` | P0 | I-V | delta y source milestone_code, no indicadores ficticios (`GameState.cs`) |
| B112 | Consulta del desglose de Poder | `ui_action` | P1 | I-V | action_code power_breakdown cuando exista control real (`UI`) |
| B113 | Interés en evolucionar héroes | `survey_answer` | P0 | I-V | question_id hero_interest 1–5 (`encuesta`) |

## Buzón, informes y recompensas

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B114 | Primer aviso de Buzón recibido | `mailbox_state` | P1 | II-V | state_code acquired y report_code (`EldoriaV41Mailbox`) |
| B115 | Buzón abierto tras aviso | `mailbox_state` | P0 | II-V | opened después de acquired (`UnityContextTutorial.cs`) |
| B116 | Informe de batalla abierto | `battle_report_opened` | P0 | II-V | Por sesión y combate resuelto (`EldoriaV41Mailbox`) |
| B117 | Informes consultados en horizontal/vertical | `report_state` | P0 | II-V | viewed con orientation sin grabar texto (`PLAYER_AUDIT_20261106.md`) |
| B118 | Recompensa visible y entendible | `report_state` | P1 | II-V | state_code viewed por report_code + pregunta opcional (`EldoriaV41Mailbox`) |
| B119 | Premios repetidos rechazados correctamente | `gateway_command` | P1 | II-V | Intentos duplicados vs idempotencia, sin asumir que se pagó otra vez (`LocalGateway.cs`) |

## Rendimiento y tiempos de espera

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B120 | FPS mediano por plataforma | `frame_summary` | P0 | I-V | fps_p50 resumido cada ~60s, no frame a frame (`Unity WebGL`) |
| B121 | Percentil bajo de FPS | `frame_summary` | P0 | I-V | fps_p05 por dispositivo/pantalla (`Unity WebGL`) |
| B122 | Tirones por minuto | `frame_summary` | P1 | I-V | stutter_count por ventana, solo con definición exacta (`Unity WebGL`) |
| B123 | Errores runtime por sesión | `client_error` | P0 | I-V | Conteo por error_code sin mensajes crudos (`Unity WebGL`) |
| B124 | Cambios a segundo plano | `visibility_state` | P1 | I-V | background y foreground; descontar de tiempo activo (`Unity WebGL`) |
| B125 | Espera de entrenamiento inicial | `timer_wait` | P0 | II | Tiempo activo durante 10min, retornos tras el plazo (`PLAYER_AUDIT_20261106.md`) |
| B126 | Espera de investigación de Fisura | `timer_wait` | P0 | III | 20min normales y actividades paralelas realizadas (`LocalGateway.cs`) |
| B127 | Aceleradores utilizados | `speedup_used` | P1 | III-V | type_code one_min/five_min y estado permitido (`LocalGateway.cs`) |

## Opinión, fricción y continuidad

| ID | Indicador | Evento principal | Prioridad | Elegibilidad | Cómo se calcula y evidencia |
|---|---|---|---|---|---|
| B128 | Incidentes reportados voluntariamente | `blocked` | P0 | I-V | Por reason_code, Bastión y pantalla (`feedback`) |
| B129 | Diversión general | `survey_answer` | P0 | I-V | question_id enjoyment escala 1–5 (`encuesta`) |
| B130 | Impresión visual | `survey_answer` | P0 | I-V | question_id visual_satisfaction 1–5 (`encuesta`) |
| B131 | Comprensión de marchas | `survey_answer` | P0 | II-V | question_id marches_clarity 1–5 o not_seen (`encuesta`) |
| B132 | Interés por la historia y la Brecha | `survey_answer` | P0 | III-V | question_id story_continue 1–5 (`encuesta`) |
| B133 | Motivación por seguir progresando | `survey_answer` | P0 | I-V | question_id growth_satisfaction 1–5 (`encuesta`) |
| B134 | Ganas declaradas de seguir jugando | `survey_answer` | P0 | I-V | question_id keep_playing 1–5 (`encuesta`) |
| B135 | Retorno real a Eldoria | `return_visit` | P0 | I-V | Nueva sesión días después bajo misma invitación; no equivale a lealtad futura (`runtime`) |

