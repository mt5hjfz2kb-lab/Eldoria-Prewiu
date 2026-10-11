# Contrato del núcleo Unity · corte inicial

Fuente observada: `v0220/js/hero-army.js`, `gameplay.js`, `military-medical.js` y `v0220/index.html` en `main` `1da613d`. Este contrato es una **decisión de implementación para la slice**, no un rebalanceo retroactivo del web.

## Poder

Web: `fresh().power=1800`, `totalPower()=s.power+gearPower()`; distintos eventos añaden deltas a `s.power` y el panel reconstruye categorías mediante restas. Resultado: un mismo inventario puede producir Poder distinto según historial. **Unity calcula un valor derivado**, jamás guarda el total como recurso editable: `Reino=600×Bastión +170×nivel Aserradero`; `Ejército=18×(disponibles+heridos)`; `Héroes=valor estadístico de Aldric (78×4+112×3+420+34×4=1204)`; `Colección=0` en la slice sin Reliquias; `Equipo=0` mientras no haya objetos. Inicial Unity: 600+648+1204=**2452**. Tras mejorar Aserradero: **2622**. Los pesos son configuración inicial de la slice y **no prometen paridad numérica v0.32.0**. En la fase de I–X extender por datos de edificios, héroes, cartas descubiertas/poseídas y equipo sin duplicación y con tests de desglose. Heridos cuentan como propiedad, nunca como fuerza expedicionaria.

## Marcha y combate

Web: `marchSetup.heroIds` + `troopRoster` por tier producen Poder de Expedición, pero `combatMarch.hero` + total de arqueros modelados T1/1 héroe resuelven combate; heridas toman número de ese segundo camino. **Unity tiene una sola composición autoritativa**: `MarchState` con `marchId`, `ownerId`, `heroId`, roster tierado reservado y objetivo. Las estadísticas se derivan de esa composición con reglas versionadas; forecast y resolución leen exactamente el mismo roster, no el Poder Total. Al salir, reservar las unidades disponibles y devolverlas al terminar. Para la slice: Aldric + 36 Arqueros T1; referencia de `heroArmy.buildMarch` web: ATQ 198, DEF 184, VIDA 642, RUP 100 y Poder de Expedición **2176**. El encuentro opcional actualmente no genera heridas: el contrato de hospital/heridas por tier se implementará al portar ese sistema en I–X, nunca debe inferirse de una composición ficticia. PvE no tiene muertes permanentes en el web.

## Economía y alcance

Web inicia con madera 230; la slice inicia con **30** para demostrar necesidad/recolección y conserva coste Aserradero **80** y carga máxima bosque **360**. Es una elección explícita del corte visual, no una copia de save web ni balance definitivo. `WOOD` se gasta una sola vez al iniciar obra y se acredita al regresar la marcha. Construcción y viaje son tareas UTC terminadas una sola vez tras recarga. No importar saves `localStorage` ni sumar `s.power` por victoria. El enemigo de demostración usa un perfil estadístico de referencia del Engendro, como **explorador corrupto opcional** de la slice; no adelanta oficialmente la misión Bastión II.

## IDs / invariantes

`player-local`, `valoria-local`, `world-local`, `forest-valoria`, `corrupt-scout`, `sawmill`, `aldric`, `archer:t1`. Comandos con `commandId`, `actorId`, `expectedRevision`. La autoridad local valida y responde con error sin mutar, registra IDs ejecutados y revisiones; sustitución futura por gateway remoto. Lo que ve UI solo proyecta estado. Disponibles+heridos=poseídos, reserva no supera disponibles, `totalPower()==sum(parts)`, forecast/result parten de un snapshot, el mismo `commandId` no cobra/abona dos veces. Los relojes del cliente **no son autoridad futura de servidor**: fase 2 revalida UTC y resultados en backend.
