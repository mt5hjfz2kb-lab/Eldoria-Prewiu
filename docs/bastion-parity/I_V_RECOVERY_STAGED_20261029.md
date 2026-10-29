# Eldoria · recuperación de paridad web I–V y separación de cámaras
Fecha: 2026-10-29  
Estado: **CAMBIOS PREPARADOS EN RAMA AISLADA · NO FUSIONADOS NI PUBLICADOS**.  
Referencia: `v0220/js/gameplay.js`, `v0220/js/chapters.js`, `v0220/js/hero-army.js`, `v0220/js/v030-depth.js` y `DESIGN_DECISIONS.md`.

## Qué se ha corregido en la rama

1. **Cámaras**: la ciudad sigue usando `ValoriaReferenceCleanInteraction.playerCamera` (zoom urbano propio). El mundo sigue usando `ZoneOneStrategicCamera` en `ValoriaIntegratedWorld.WorldCamera`. El orden cercano/regional/panorámico del mundo se calcula según tamaño de pantalla, respetando el parámetro panorámico existente y forzando `near < regional < panorama`. Anteriormente el zoom regional 32 era menor que el cercano vertical 64. Se añadieron pruebas EditMode de jerarquía y aislamiento de objetos Camera.
2. **Recompensas web III–IV**: `c3-bastion`, recompensa final III, misiones IV y recompensa final IV, con el libro persistente de pagos únicos ya existente; en V se incluyen los hitos efectivamente alcanzables de Cantera, 1.200 piedra y 60 arqueros. No se pagan `c5-rare`, ascenso VI ni capítulo V sin contenido real.
3. **Tropas T2**: se desbloquea su reclutamiento en Cuartel IV, con coste por unidad 20 madera y 12 piedra (como web), retención de T1, tarea persistente por tier y selección de marcha T2 mediante el comando actual de cinco partes. La pantalla militar existente permite alternar T1/T2 desde entrenamiento; la presentación combinada de varios tiers y porcentajes de tropas pertenece al diseño de marchas posterior que NO se sustituye aquí.
4. **Aceleradores**: restaurados los de 1 min (primera tarea elegible IV+) y 5 min (recompensa de capítulo IV). Se conservan en el guardado y se consumen por comando idempotente solo sobre obras, Bastión o reclutamiento reales, nunca recolección ni marchas. Las pantallas de construcción y entrenamiento reutilizan su botón de acción existente para aplicarlos mientras una tarea está activa.
5. **Informes**: se registran estadísticas de la fuerza desplegada y daños por intercambio del resolutor actual, sin variar su ecuación o resultados. Se muestran en el presentador heredado donde esté activo; la pantalla dedicada de informe del RoyalHUD y la integración visual V4.1 no forman parte de este parche.
6. **HUD de objetivos**: Bastión IV presenta sus cinco hitos reales; Bastión V muestra Cantera/piedra/ejército y explicita que Acechador/Bastión VI aún faltan. Las mejoras 1→5 continúan disponibles pero no sustituyen esas misiones.

## Pruebas y límites

- Prueba portátil de dominio en la misma rama mediante `tools/bastion-iv-v/DomainProbe.csproj` y `tools/bastion-iii/DomainProbe.csproj`; incluye progresión, economía, cola, elecciones, Cantera, guardado y regresión I–III. La ejecución **40931351346 concluyó SUCCESS** e incluye nuevas comprobaciones reales de T2, acelerador 1m, guardado, composición T2 y reporte por rondas.
- Nuevas pruebas `CameraGestureTests`, `RoyalV3ManagementRulesTests` y `SliceCoreTests` añadidas pero **todavía no ejecutadas en Unity**. No se certifica compilación de UI Unity, capturas de zoom, tacto móvil ni APK.
- **Pendiente de paridad completa:** enemigo y combate específico Acechador de Ceniza (sin generar ni reutilizar arte no aprobado), capítulo V hasta Bastión VI, presentación combinada de tiers/porcentajes en la UI aprobada recientemente, aceleradores futuros de 15m y pantalla de informe de batalla RoyalHUD. No declarar el proyecto 100% migrado mientras falten.
- **Advertencia sobre guardados anteriores:** el libro de pagos evita dobles cobros; el objetivo web de comida III es de saldo disponible. Las partidas históricas que gastaron comida sin cobrar `c3-food` pueden necesitar recuperación explícita de evidencia; no se inventa un objetivo retroactivamente cumplido.
- **Reglas de titularidad:** `main/pipeline/active-workstreams.json` mantiene al propietario del workstream `valoria-master-plan-layered-construction-20261016` con escritura sobre estos archivos, y la [PR #130](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/130) de orientación V sigue abierta. Por ello los cambios están exclusivamente en esta rama y no se abre una segunda PR conflictiva ni se toma el runner Windows. Al reconciliar, trasladar los commits con revisión de archivos y ejecutar el gate Unity completo antes de cualquier fusión.

## Recomendación de integración

Preservar el trabajo de orientación y PR #130; comparar `main` y su HEAD vivo, reconciliar campos del guardado y UI con las pantallas de marchas V4.1, correr EditMode+PlayMode y capturas auténticas de ciudad (zoom propio) y mundo (cercana, regional, panorámica) en ambas orientaciones. No publicar ni aprobar arte antes de comprobar los encuadres y la separación de cámaras.
