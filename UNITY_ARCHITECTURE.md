# Eldoria — arquitectura de la etapa 1 preparada para autoridad futura

Consultar primero `UNITY_MIGRATION_PLAN.md` y `UNITY_READINESS_REPORT.md`. Esta es la **arquitectura propuesta**, no un sistema implementado. No escoger servicio de red/backend en esta etapa.

## Regla de separación

El cliente Unity presenta el mundo e inicia **intenciones**; el núcleo de reglas valida comandos y produce cambios; un repositorio guarda snapshots y un reloj entrega tiempo. En etapa 1 una implementación local ejerce autoridad. En etapa 2 podrá existir un procesador autoritativo de servidor que ejecute la misma semántica de comandos/versiones: no tratar `MonoBehaviour`, `Transform`, UI o `ScriptableObject` como fuente de verdad de recursos, recompensas, temporizadores, inventario, propietarios o progreso.

| Límite | Etapa 1 | Contrato que preserva etapa 2 |
| --- | --- | --- |
| Identidad y ámbito | ID estable de jugador, reino, mundo local, región; único propietario real | `playerId`, `realmId`, `worldId/serverId`, `entityId` distintos; mundo y propietario no implícitos en un singleton. |
| Acción | `ICommandGateway.Execute(command, expectedRevision)` local | Comandos con `commandId` para idempotencia, actor/target, validación, versión y resultado/errores; nunca mutar oro/wood directamente en UI. |
| Economía | `Wallet`, costes/recompensas con ledger local y reserva atómica | Autoridad futura valida fondos, gasto y premio una vez; no confiar en cantidades enviadas por el cliente. |
| Tiempo | `IClock.UtcNow` y tareas con `startedAt`, `endsAt`, `taskId`, estado | El servidor fijará/revalidará UTC. Offline/resume deriva de timestamps y resolución idempotente; no confiar en reloj/dispositivo del jugador. |
| Mundo/marchas | ID estable de nodo, región, coordenada lógica, dueño, visibilidad y ciclo de vida; marcha con roster snapshot y `ownerId` | Servidor decide ocupación/respawn, viaje y resultado; una operación puede agrupar varias contribuciones conservando dueño, tropas/heridos/recompensa individuales. |
| Combate | Resolver determinista con reglas versionadas, `seed` de prueba, composición y snapshot enemigo | Resultado/loot/heridos calculado por autoridad; cliente anima un `CombatReport` verificable, no transmite un resultado declarado. |
| Estado y save | Snapshot versionado + migradores + persistencia atómica local | Snapshots/eventos auditables y cursor de revisiones; modelo independiente de plataforma aunque las partidas web no se importen automáticamente. |

## Modelos puros propuestos

`GameRules` (costes, producción, límites, afinidades, stats, botín, heridas, cartas); `PlayerState` (wallet, edificio/nivel, capítulo/misiones, héroes/XP/equipo, roster por familia/tier, heridos, reliquias/instancias, inventario, flags narrativos); `WorldState` (región/nodos/amenazas/respawns, etapa local de Brecha); `MarchState` (ID, propietario, héroes, tropas comprometidas, target, estado/viaje); `TaskState` (construcción, reclutamiento, Hospital, recolección); `CombatRequest/Report`; `ChapterDefinition/MissionProgress`; `RelicDefinition/RelicInstance` (calidad Indestructible separada de rareza; descubierto ≠ poseído ≠ consumido; buff y cooldown con expiry). Emplear IDs estables y `contentVersion/rulesVersion`, nunca texto localizado como clave. `ScriptableObject`/JSON pueden actuar como catálogo de autoría exportado a datos puros, no contener balances que solo funcionen en el editor.

**Invariantes de aceptación:** gastar y conceder recompensas exactamente una vez; completar misión/capítulo una vez; reclutar/curar sin duplicación tras recarga; disponibles + heridos = unidades poseídas en PvE (muertos permanentes = 0), salvo eventos de adquisición/pérdida válidos; marcha no supera disponibles; cambiar equipo devuelve el anterior; `Poder Total` se deriva de propiedad/progresión sin duplicar equipo/heridos, `Poder de Expedición` solo de marcha disponible y equipo desplegado; forecast y resolución usan el mismo snapshot/reglas. Los números actuales del web se capturan como *golden cases*, no se dan por correctos sin reconciliación.

## Estructura inicial, acotada

```text
Unity/                         # carpeta propuesta; no crear aún
  Assets/Eldoria/
    Scenes/Bootstrap.unity     # composition root y carga de save
    Scenes/Valoria.unity       # escena jugable de la primera slice
    Scenes/Frontier.unity      # frontera/ruta/encuentro; combate en capa o subescena si perfilado lo justifica
    Content/                   # definiciones ID/versionadas, localización ES/EN
    Art/{Characters,Buildings,World,VFX,UI,Audio}/
    Prefabs/{City,World,Combat,UI}/
    Scripts/Domain/            # C# puro, sin UnityEngine
    Scripts/Application/       # comandos, guardas, transacciones, puertos
    Scripts/Infrastructure/    # save, reloj, RNG, content loaders
    Scripts/Presentation/      # cámara, animación, UIs, navegación
    Tests/{EditMode,PlayMode}/
  Packages/manifest.json; Packages/packages-lock.json
  ProjectSettings/ProjectVersion.txt
```

Assemblies propuestas `Eldoria.Domain` → ninguna dependencia Unity; `Eldoria.Application` → Domain; `Eldoria.Infrastructure` → Application/Domain; `Eldoria.Presentation` → Application y Unity; `Eldoria.Tests.EditMode`/`.PlayMode` referencian solo lo necesario. Bootstrap inyecta `IClock`, `IRandomSource`, `IContentCatalog`, `ISaveStore`, `ICommandGateway`; no `GameManager` global con estado público. Separar definiciones autorales de snapshots mutable y la proyección de UI del estado canónico.

## Migración de saves y paridad

El save web usa una clave `localStorage` y objetos/flags históricos. **No prometer importación automática web → iPhone Unity.** Primero documentar esquema v1 Unity, datos que representan propiedad frente a UI/telemetría, defaults, invariantes y migradores v1→v2. Extraer estados representativos web a fixtures anonimizados (inicial, antes/después de Bastión III, VII, X; equipo; Indestructible/cooldown; heridos/curación) y compararlos con casos esperados de reglas antes de decidir si se construirá importador explícito. La primera slice guarda estado Unity local versionado y confirma recarga/offline sin duplicar premios. Mantener exportación manual de diagnóstico para QA.

## Herramientas, motor y dependencias (verificado 2026-09-24)

- **Unity 6.3 LTS**, fijar el *patch* exacto al crear proyecto (`ProjectVersion.txt` y CI). Soporte publicado hasta diciembre 2027; 6.0 LTS hasta octubre 2026. [Unity Releases & Support](https://unity.com/releases/unity-6/support). La rama de actualizaciones puede tener funciones más nuevas; se prefiere una base LTS para comparar builds visuales durante la etapa 1.
- **URP + 3D móvil estilizado**: presupuesto controlable de luces/sombras, shader de corrupción y VFX; no elegir HDRP. La documentación oficial describe URP para móvil y plataformas amplias: [Unity URP](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.render-pipelines.universal.html). Validar iPhone real antes de fijar resolución/calidad. No fijar texturas/polígonos/FPS sin dispositivo objetivo.
- **Input System** para toque, arrastre de cámara y ratón de editor; **UI Toolkit** para paneles de gestión y navegación, con prueba temprana de accesibilidad, safe areas y overlays; permitir uGUI localizado si medición móvil demuestra necesidad. [Input](https://docs.unity3d.com/6000.5/Documentation/Manual/Input.html), [UI Toolkit runtime](https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-get-started-with-runtime-ui.html). Fijar versiones compatibles desde Package Manager al crear el proyecto y conservar `packages-lock.json`; no inventar aquí números de paquetes.
- **Unity Test Framework** para EditMode (reglas) y PlayMode (interacción/escena); **Addressables** cuando el volumen de contenidos/memoria lo justifique, no infraestructura remota en la primera slice. [Addressables](https://docs.unity3d.com/6000.5/Documentation/Manual/com.unity.addressables.html).
- **Localización:** claves por ID en ES y EN; nombres de estado estables; traducciones en catálogo separado de lógica. Audio como clips licenciados/capturados y mezclador por bus, nunca depender de WebAudio del navegador.

No incorporar networking SDK, cuenta, Unity Gaming Services o backend concreto para cumplir la regla de arquitectura: diseñar puertos/contratos y probar reemplazo del gateway local por un *mock* remoto, sin red aparente al jugador. El proveedor se decide con requisitos de escala, seguridad, coste, operatividad, regiones y reconciliación definidos más adelante.
