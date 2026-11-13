# Eldoria — Telemetría completa I–V sin alterar la experiencia del jugador

## Realidad comprobada, no una integración afirmada

- `native325` está archivada como release DRAFT exacta. **No** contiene el puente de 67 eventos de Unity.
- El observador externo de PR179, desactivado por defecto, permite **9 familias de sucesos de navegador**: inicio, retorno, orientación, visibilidad, error JS, etapa de carga, juego listo, FPS resumen y heartbeat. No observa la economía, el combate ni los objetivos internos.
- Hay **135 indicadores definidos** en `measurement-catalog.json`, pero registrar los 67 tipos permitidos en Supabase **no equivale** a que los 67 eventos se emitan.
- Para medir realmente el comportamiento I–V se necesita un puente desde la autoridad de Unity. Debe ser **pasivo** y no modificar tutorial, relojes, guardados, recompensas o presentación.

## Puntos reales de integración del código actual

| Fuente real | Estado disponible / transición útil | Evento/valor codificado |
|---|---|---|
| `Unity/Assets/Eldoria/Scripts/Presentation/SliceBoot.cs` | Crea `LocalGateway` y `WebGlPlayerPrefsStore`, selecciona `OWNER_I_II` | `session_start`, `save_state`; preservar perfil y serialización |
| `Unity/Assets/Eldoria/Scripts/Application/LocalGateway.cs` | `Execute(GameCommand)`: `ChoosePlayerName`, `Gather`, `ConfigureMarch`, `Fight`, `AdvanceBastion`, `Build`, `UseSpeedup`, `Recruit` | `player_name_set` (sin enviar el nombre), `gateway_command`, `gather_state`, `march_state`, `battle_state`, `bastion_reached`, `build_state`, `speedup_used`, `training_state` |
| Mismo `LocalGateway` | `ExecuteChapterThree`: `ChooseDevelopment`, `InvestigateFissure`, `ContactLyra`, `UpgradeBuilding`; `Advance()` | `city_priority`, `fissure_state`, `lyra_state`, `production_state`, `build_completed`, `report_state`; distinguir investigación de 20 min vs encuentro vs reclutamiento tras asalto |
| `Unity/Assets/Eldoria/Scripts/Domain/GameState.cs` | `PlayerState`, `ChapterProgressState`, `ChapterThree`, `MarchState`, `BattleReportState`, `Resources`, `Mailbox` | transiciones de estado con `bastion`, `quantity`, `stage_code`, `reason_code` y otros campos de catálogo, nunca JSON completo |
| `Unity/Assets/Eldoria/Scripts/Domain/EarlyRetentionRules.cs` | `VigilantBattle`, `ClaimVigilantXp`, `ScoutDispatch`, `ScoutClaim`; `LastClearedStage` y órdenes de exploradores | `vigilant_attempt`, `vigilant_result`, `vigilant_xp_claim`, `scout_order`, `scout_report`; distinguir intento, victoria y recompensa reclamada |
| Presentación de UI Royal Gold / diálogos | Momento real de visualización/cierre de tutorial y apertura de destino | `tutorial_step`, `dialogue_state`, `screen_viewed`, `help_opened`, `input_rejected` solo cuando exista señal fiable |

## Método preferido para la integración cuando se pueda compilar

Implementar un adaptador opcional `ICommandGateway` **solo en la variante beta instrumentada** que envuelva al `LocalGateway` real sin sustituir su lógica:

1. Antes del comando, tomar solo un snapshot *read-only* reducido; ejecutar la llamada verdadera una única vez y tomar snapshot posterior.
2. Emitir evento **después** de `CommandResult` y después de los cambios reales. Si el comando falla registrar categoría de fallo, no asignar recompensas ni simular victoria.
3. Para tareas diferidas, observar también `Advance()` y diferencias de estado, no fingir que un temporizador iniciado significa completado. Idempotencia por `client_event_id` y `session_id`.
4. En WebGL, puente asincrónico JS desde Unity a `/beta/events` de **mismo origen**, detrás del enlace válido y consentimiento preaceptado; sin `service_role` ni claves privadas en Unity. Carga útil restringida a los códigos del catálogo, nada de nickname, texto de chats, guardado completo, coordenadas precisas o identificadores personales.
5. Emisión de eventos sin bloquear el hilo de UI, máximo 1.5 KB por evento, reintentos con límite y gestión de offline; limitar el impacto medido en FPS.
6. Cambios de fuente y binario generan **una nueva versión beta** independiente. Mantener el DRAFT native325 inalterado; no llamar idénticos a dos WASM distintos.

## Prueba mínima de paridad y calidad antes de invitar

- Mismo recorrido normal I→V, tutorial original, recursos, esperas, 3 marchas, Fisura, Lyra, Vigilantes, Exploradores, consejo IV, cantera y final V 4/4.
- Comparar un recorrido reproducible de base native325 frente a variante instrumentada: comandos aceptados/rechazados, coste, recompensas, temporizadores, persistencia/reinicio y pantallas. No usar `QA_FAST` ni tiempos acelerados como *única* prueba.
- Verificar QA en Chrome escritorio y móviles físicos, además de alojado HTTPS real y cookies/consentimiento.
- Solo después marcar cada uno de los 135 indicadores como `INSTRUMENTADO` / `NO_INSTRUMENTADO` / `NO_ALCANZADO` / `SIN_CONSENTIMIENTO`. **No presentar datos cero cuando el evento no existe.**
- No editar `Unity/**` ni usar runner actualmente reservado a Forja Oscura desde PR179. Este documento es diseño de integración específico y verificable, **no** código compilado ni QA real.

Este método preserva exactamente la experiencia y permite más datos de forma comprobable, pero exige otra compilación si se necesitan los 135 indicadores internos. La versión congelada native325 permanece disponible como respaldo/benchmark.
