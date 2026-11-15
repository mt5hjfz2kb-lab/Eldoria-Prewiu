# Eldoria — La Senda de la Brecha: evento arcade independiente

**Fecha:** 15 noviembre 2026 · **Estado:** prototipo funcional HTML5; integración Unity pendiente · **Workstream-ID:** `eldoria-senda-brecha-lane-event-20261115`

## 1. Objetivo de jugador

Añadir una actividad **opcional, breve y repetible**, distinta del combate táctico principal de Eldoria, en la que el jugador conduzca a un destacamento por un camino corrompido. En cada bifurcación elige una ventaja visible. Las tropas luchan automáticamente; las unidades propias suben o bajan, el jugador entiende el coste de sus decisiones y puede reintentar cuando fracasa. El objetivo del piloto es comprobar *diversión y claridad*, no imitar visualmente otro producto, monetizar ni sustituir el motor estratégico de Valoria.

## 2. Loop V0 entregado

`Introducción → elección 1 → acechadores → elección 2 → guardianes → elección 3 → jefe → victoria o derrota → repetir`.

- **Entrada:** 16 infantes y 6 arqueros. Control izquierdo/derecho mediante tacto, deslizamiento, botón o teclado. 6 s por elección; avance y daño automáticos.
- **Tramo I — bosque silencioso:** guardia de Valoria (+12 infantes) o arqueros errantes (+9 arqueros); encuentro de acechadores.
- **Tramo II — ruinas del juramento:** forja de campaña (+40 % ataque) o escudos consagrados (−40 % daño recibido); encuentro de guardianes corruptos.
- **Tramo III — umbral violeta:** último destacamento (+16 infantes) o flechas rúnicas (+16 arqueros); jefe guardián de la Brecha.
- **Salida:** resultado, supervivientes, tiempo, opción de reintentar. 45 fragmentos y 1 sello **solo son una simulación visible** tras la victoria y jamás pasan a la economía de Eldoria.
- **Duración observada en simulación:** 46–59 s según las rutas. Objetivo de diseño para una versión definitiva: 45–90 s, sujeto a playtest real.

La simulación arcade tiene su propio balance y no pretende utilizar ni modificar los 15 perfiles T1–T5 ni el resolver `LastBattleReport` existente. No concede tropas reales, altera marchas, heridos, hospital, guardados, nick, cuentas o inventario.

## 3. Dirección artística propuesta para Unity, **no implementada**

1. Escena propia `SendaBrecha_Event.unity`, descargable antes de volver a ciudad, sin cargar simultáneamente la Región 1 completa. El mundo de Bastión I–V y la beta Safari se mantienen intactos.
2. Cámara fija ligeramente elevada: calzada de piedra, bosque de Valoria, ruinas oscuras, antorchas y corrupción violeta. Reutilizar recursos cuya licencia, canon y presupuesto se hayan auditado. La geometría y shaders deben seguir el estilo 2.5D adulto aprobado, no la estética brillante del juego comercial.
3. Personajes 3D simples y reconocibles. Hasta una cantidad acotada de representantes visibles para cada contingente; el conteo matemático de tropas puede ser mayor. Utilizar `ObjectPool` para proyectiles/impactos, evitar instanciar/destrozar unidades continuamente.
4. Puertas claramente diferenciadas, con beneficio legible antes de cruzarlas, reacción visual al aplicar el efecto y números en pantalla. Combate resuelto con reglas sin físicas individuales por soldado, con feedback limitado a impactos y bajas.
5. UI aislada dentro de `Unity/Assets/Eldoria/SendaBrecha/`, con arte modular de Vigilia **solo cuando el titular de la interfaz autorice y entregue componentes reutilizables**. Sin mezclar el HUD anterior, sin usar el Bastión prohibido y sin acceder a `RoyalHUD` de producción.

**Criterio visual:** obtener capturas reales de Unity de puerta, combate, victoria y derrota en 390×844 / 430×932 / 844×390 / 1600×900 y compararlas con la dirección artística aprobada; no bastan maquetas o capturas HTML.

## 4. Arquitectura de adaptación sugerida

- `SendaRunDefinition` (ScriptableObject): fases, opciones, tiempos, enemigos, efectos y límites. Sin valores mágicos en la presentación.
- `SendaRunState` (clase serializable o modelo puro): fase, selección, tropas temporales, heridas arcade, tiempo, resultado, versión. Debe ser determinista y reproducible en EditMode.
- `SendaRunController` (MonoBehaviour): recibe cambios de carril, ejecuta el avance y traduce eventos a VFX/UI, sin escribir la partida real.
- `SendaRunPresenter` (cámara + efectos + agrupación de soldados): estados visuales a partir de una única autoridad matemática; nunca usa la cantidad de GameObjects para decidir el resultado.
- `SendaEventGateway` **futuro**: validar autorización, límite de repetición y recibo idempotente antes de conceder recompensas reales. Hasta que exista, debe permanecer desconectado.

**Prohibiciones:** no enganchar la UI a navegación principal en la beta I–V, no habilitar compra, no crear recursos premium, no integrar una nueva cámara en mundo, no sincronizar el saldo local de una demo con el guardado principal.

## 5. Pruebas observadas y pendientes

| Área | Estado | Evidencia o límite |
|---|---|---|
| Motor arcade | PROBADO localmente | 8/8 rutas completadas, 5 victorias y 3 derrotas; no se duplican elecciones; premios solo en victorias |
| Interacción web | PROBADO localmente | Chromium 390×844, 430×932, 844×390: táctil, botones, avance, victoria, derrota, reinicio y sin errores JS registrados |
| Identidad visual | PROTOTIPO | Fondos y personajes dibujados procedimentalmente; no equivalen al acabado premium requerido |
| Fuente Unity | NO IMPLEMENTADA | Solo reglas y propuesta de adaptación, sin escena ni paquetes 3D creados |
| Safari iPhone físico | NO PROBADO | No extrapolar de Chromium; la entrada al mundo principal sigue siendo otro P0 independiente |
| Economía, guardados y buzón real | AISLADOS | Ninguna escritura; recompensa de simulación deliberadamente no persistente |

Para decidir si desarrollarlo después del test inicial, medir: porcentaje que termina el primer intento, porcentaje que repite, duración, distribución de opciones elegidas, comprensión de puertas y valoración de diversión. Registrar únicamente métricas agregadas/protegidas si el sistema de telemetría del juego se ha definido y autorizado; el prototipo no recoge datos de usuarios.

## 6. Próxima decisión de producción

Primero probar el prototipo con el propietario y algunos testers. Solo si el loop funciona, **traducirlo a Unity** en una PR propia y sin tocar la beta viva; comparar dos métodos visuales (primitivas estilizadas con art kit existente frente a instancias reducidas de soldados originales) en una cámara fija con medición real de memoria, tiempos de carga y frame timing. Rechazar sin más iteraciones cosméticas el método que no aporte un salto visible. La activación/recompensa final requerirá reglas de economía, UI y validación dentro del juego, pero no bloquea la prueba del evento aislado.
