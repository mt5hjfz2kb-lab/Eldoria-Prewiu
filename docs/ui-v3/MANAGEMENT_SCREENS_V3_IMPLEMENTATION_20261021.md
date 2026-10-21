# Eldoria — gestión V3 de Cuartel y Marchas

## Contexto de integración

La interfaz canónica jugable está gobernada por `ValoriaIntegratedWorld` y el único `RoyalHUD`. La PR #89 aporta un prototipo experimental en `SlicePresenter`, pero el host integrado desactiva el Canvas heredado. Por tanto, esta candidata **no fusiona** el popup experimental: implementa destinos reales sobre el HUD V3 existente.

## Destinos funcionales

`RoyalGoldV3ManagementScreens` se incorpora como componente al mismo HUD y se enlaza en `ValoriaIntegratedWorld.BindSession`, sin segunda escena Unity, cámara, gateway, guardado, reloj, HUD, canvas o EventSystem. Las selecciones reales `barracks` y `march-main` abren respectivamente Entrenamiento y Preparación de Marcha. La navegación es extensible mediante el enum `Destination` y su método `Open`; no se generan pantallas falsas para Hospital/Forja.

El fondo es opaco y cubre el Canvas, y las superficies son parte de la gramática Royal Gold V3. La ciudad sigue cargada —para preservar la cámara—, pero no aparece tras el destino. Un único botón devuelve a la misma ciudad y selección; no hay `LoadScene`.

Entrenamiento: solo Arqueros T1 reales, cantidad de 1 a máximo asequible (tope defensivo 500), costes calculados por el dominio y temporizador leído del estado persistido. Perfil OWNER_I_II: 30 segundos por arquero, i.e. 10=300s, 20=600s y 30=900s. Perfil QA_FAST: conserva su cadencia técnica proporcional para las pruebas de progresión ya existentes. Una única tarea de entrenamiento simultánea, sin segundo descuento ni doble entrega; completado offline por `LocalGateway.Advance` y su libro `CompletedTaskIds`. El formato de guardado sigue siendo 1.

Marchas: Sir Aldric, cantidad seleccionable exclusivamente entre arqueros reales disponibles; poder y estadísticas mediante `SliceRules.Expedition`; comando `ConfigureMarch` guarda una copia de la composición elegida y mantiene el combate de Engendro que despliega esa copia. El comando clásico `march-main` conserva la semántica anterior de seleccionar todo.

## Fuente y pruebas

- `RoyalV3ManagementRulesTests`: costes y tiempo 10/20/30, economía insuficiente, reintentos, comandos inválidos o anticuados, entrega offline exactamente una vez, entrenamiento tras el objetivo tutorial, preparación de marcha parcial y carga de guardado legado.
- `RoyalV3ManagementIntegrationTests`: arranque de la escena canónica con un gateway de memoria independiente y desechable; verifica Canvas único, ocultación/entrada, UI táctil, composición, contador, cámara antes/después y comandos reales sin escribir el guardado familiar.
- Los resultados de compilación Unity y capturas visuales deben verificarse sobre el SHA final de la candidata. **No** considerar esta documentación ni la creación de PR como una certificación.

## Concurrencia

La rama `feature/ui-v3-management-completion-20261021` aísla estos cambios mientras `valoria-master-plan-layered-construction-20261016` posea recursos y archivos comunes sobre `main`. Antes de integrar: reconciliar la revisión de `main`, confirmar la liberación o el relevo autorizado del recurso `windows-runner-heavy`, ejecutar Unity con el código final y comprobar la vista en pantalla real. Ninguna publicación automática forma parte del primer commit de esta candidata.

## Native closure and canonical integration

PR #90 merged as `284ef398e078021f4eeef42f852730714c0bd48b`; obsolete prototype PR #89 closed without merging. All existing art and rig candidates retained.

- Native run39653618057 / artifact12370981749: downloaded original ZIP, 26/26 EditMode and 38/38 PlayMode passed with no skips. Native integrated I–II probe passed. Independently inspected actual training landscape/portrait and march portrait captures: opaque destination fully hides city, royal framing and labels readable, quantities/costs/countdown and return controls present. This is native evidence, not physical-phone or hosted-WebGL certification.
- Reconciled source `889ebdc05401a3808f4296db1888fca155bd7bd4`: full native run39654505571 also SUCCESS, artifact12370039251. No original geography or rig source replaced.
- Post-merge main checks running separately. Playable WebGL packaging/QA/delivery remains pending; no publication is claimed by this record.
