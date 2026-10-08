# R2-B · Decisión estratégica en Región 1 — especificación de producción v1

**Autorización:** DG Issue #23 comentario #6068111056, Ronda 2 línea B, presupuesto externo 0 €. **Estado:** especificación de producción, NO código de gameplay implementado, NO QA final. Owner coordinador: `eldoria-dg-r2-approved-20261008`.

## Restricciones del juego real
- `Unity/Assets/Eldoria/Scripts/Domain/GameState.cs` usa `PlayerState.SchemaVersion=1`, `Revision`, `CompletedCommandIds`, `ChapterProgressState`, `MarchState` y `ResourceWallet`.
- `Unity/Assets/Eldoria/Scripts/Application/LocalGateway.cs` proporciona `GameCommand(Id,ActorId,Kind,TargetId,ExpectedRevision)`, `Execute`, `Snapshot` y persistencia por `IStateStore`.
- Mantener el loop de Bastión I→II, recompensas existentes, recompensas únicas y la fuente de verdad del gateway. No Bastión III, no tocar SHARP ni M07.

## Encargo jugable
Tras llegar a un punto de interés ya existente de Región 1, ofrecer dos opciones significativas usando una misma recompensa total máxima: **A Explorar cuidadosamente:** se obtiene información/seguridad visible y una recompensa menor; **B Recoger inmediatamente:** recompensa superior con un coste/riesgo pequeño, determinista y conocido por el jugador. El departamento D07 debe validar los números con `SliceContentProfiles` y los tests de balance actuales ANTES de codificar. Ninguna rama inventará recursos ni autorizará duplicados por refresh.

## Contrato de implementación
1. Registrar opción en una única orden de dominio idempotente con id único y `ExpectedRevision`. No editar `Resources` directamente desde UI.
2. No aceptar una segunda elección para el mismo encuentro si está cerrado; preservar el historial entre guardar, recargar y volver a Región 1.
3. Mantener esquema compatible: al modificar `PlayerState` revisar Snapshot/store/normalización, default para saves preexistentes y migración solo si realmente necesaria. No incrementar schema por conveniencia.
4. Separar ownership: D07 recibe contrato de comandos/decisión y D06 persistencia/compatibilidad; editar `GameState.cs` y `LocalGateway.cs` solo con un claim exclusivo coordinado que no compita con R1.
5. UI presenta ambas opciones, costes y resultados antes de ejecutar; el mundo no pierde acceso a la ruta, navegación o recursos existentes.
6. Sin RNG no controlado ni tiempo real en la decisión; documentar si se utiliza `IClock`/`IRandomSource` ya disponibles.

## QA exigido antes de aceptación
- Dos opciones en estados frescos separados, resultados distintos y límites de recompensa comprobados.
- Segunda aplicación de `GameCommand.Id` rechazada o tratada como idempotente sin doble beneficio.
- Revisión obsoleta rechazada sin alterar recursos.
- Guardar/recargar conserva opción, estado del encuentro, inventario y ruta.
- Reset restaura el encuentro de manera canónica.
- No rompe el loop original I→II, reclutamiento, marcha ni combate.
- PlayMode/EditMode y dos recorridos reales WebGL en portrait/landscape; no errores fatales.
- M03 verifica SHA/run/artifact independiente; M04 recibe fallo con owner intacto y como máximo dos reintentos autorizados.

## Dependencias de ejecución
La especificación es independiente de M07. La implementación en Unity no puede invadir su scope ni ocupar recursos reclamados por otro owner. Preparar test/contracts puede ejecutarse sin el runner de Windows. Publicación final e integración con R2-A/R2-C se escalonan por recurso exclusivo.

**Siguiente operación de D07/D06:** confirmar el encuentro canónico concreto y tabla de balance existente; reclamar scopes precisos antes de implementar. Este documento no es un simulacro de la mecánica ni un cierre del encargo.
