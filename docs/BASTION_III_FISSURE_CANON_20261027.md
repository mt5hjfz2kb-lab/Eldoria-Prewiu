# Bastión III — Fisura y Lyra · canon del vertical slice (27-10-2026)

**Autoridad:** decisión explícita del propietario el 27 de octubre de 2026. Se integra en la referencia jugable web `v0220/index.html` antes de migrar Bastión III a Unity. Complementa `DESIGN_DECISIONS.md` y `docs/BASTION_I_X_MASTER_TABLE.md`. No ordena migrar Unity inmediatamente, publicar builds, generar assets ni cambiar el mundo compartido.

## Intención y límites

Eldoria conserva su identidad de reinos medievales, exploración, reconstrucción y 4X. La Brecha es un misterio mayor; **esta fisura es una manifestación local, excepcional y temporal**, no un nodo de recolección renovable ni un campamento PvE para farmear. No decidir ahora si el conflicto sobrenatural dominará el futuro del juego. Lyra no es experta en fisuras: **busca a su hermano desaparecido**, encuentra pistas y lucha con Aldric por interés compartido.

## Secuencia canónica de Bastión III

1. Al alcanzar Bastión III, preservar Granero, comida, dos cacerías y frontera ampliada. Una **única fisura** aparece cerca de Valoria; su representación respeta el canon geológico de la Brecha, no un portal futurista o una fortaleza.
2. Tocar la fisura inicia la **expedición de investigación de Aldric de 20 minutos** (valor concreto dentro de los 15–30 min acordados). El reloj es persistente y funciona tras salir/volver; el estado no debe duplicar tareas o recompensas. Cuenta atrás vinculada a la propia fisura, además del registro de tareas.
3. Tras completar la expedición, el jugador descubre las huellas y **conoce a Lyra** mediante diálogo contextual. Explica la desaparición de su hermano y por qué necesita a Valoria. Nadie conoce todavía qué origina la Brecha ni de dónde vienen las criaturas. **Aún no se desbloquea Lyra como heroína del roster.**
4. Lyra **acompaña a Aldric en el asalto a la fisura** (alianza narrativa previa al reclutamiento definitivo). El enfrentamiento conserva el progreso, coste y recompensas aprobados por el prototipo; no añade combate PvP, multijugador ficticio ni otro campamento permanente.
5. **Solo al superar el asalto** se cierra la fisura; desaparece del mapa y no se puede atacar ni recompensar otra vez. Mostrar durante unos segundos una cicatriz de tierra que se atenúa y desaparece, sin alterar otros nodos del mundo ni a otros jugadores.
6. En los instantes del cierre, **Lyra reconoce un símbolo vinculado a su hermano**. Se celebra su ingreso como heroína tras luchar junto a Aldric. Se concede una sola vez el **Manuscrito de la Fisura** en el **Arcón**, como misterio no utilizable aún. Mantener las recompensas de piedra y XP del flujo original; no revelar el papel futuro del manuscrito ni tropas Paladín/Brujo.
7. Regresar a Valoria y continuar progresión ordinaria de Bastión III→IV, sin un icono de fisura persistentemente visible.

## Contratos de persistencia y paridad

- Estados distinguibles: `not_started` → `investigating` → `investigated` → `lyra_contacted` → `assault_completed/closed`. El prototipo usa `explore-fissure` en `tasks`, `fissureExplored`, `lyraContacted`, `boss`, `lyra` y `fissureClosingUntil`. Unity debe migrar la **semántica**, no necesariamente los nombres o la arquitectura JavaScript.
- Respetar guardados antiguos: una partida con `boss=true` es completada, jamás reabre el nodo; una partida histórica con `lyra=true` y `boss=false` debe poder continuar el asalto sin reclutamiento duplicado.
- Completar la espera fuera de línea es idempotente. La recompensa y el manuscrito se emiten una vez; después la fisura no reaparece por respawn ni por cambio de escena/cámara.
- En futuro 4X, el evento de Bastión III es **progreso privado del jugador**. La desaparición de su fisura no puede borrar un elemento compartido a otros usuarios. Posibles fisuras futuras temporales y cooperativas son **hipótesis posteriores**, no sistemas autorizados ahora.
- La Brecha principal y sus amenazas posteriores (Devorador, Heraldo, final del prólogo) **no desaparecen** porque se cierre la fisura de Bastión III; distinguir expresamente entidad local de conflicto global.

## Comprobaciones de aceptación antes de migrar

- Partida nueva: fisura visible en III; tocarla crea exactamente una tarea de 20 min y no entrega Lyra ni manuscrito por anticipado.
- Guardar/cerrar/abrir: reloj correcto; al pasar el plazo solo se habilita encuentro con Lyra; la misma exploración no puede duplicarse.
- Contacto: diálogo de hermano; estado de Lyra aún no reclutado; a continuación asalto desbloqueado.
- Victoria: Lyra pasa al roster una vez, manuscrito queda una vez en Arcón, misión `c3-fissure` progresa, cicatriz transitoria, nodo ausente al terminar.
- Reabrir partida ganada: sin segunda fisura, segundo manuscrito, doble piedra o doble Poder. Partidas históricas conservan el progreso.
- Verificar flujo táctil real en móvil y persistencia al portar a Unity; inspección estática de código no sustituye una prueba jugable.

**Decisión pospuesta:** peso final del misterio sobrenatural frente a conquista medieval 4X. No convertir esta decisión pendiente en un bloqueo de Bastión III.
