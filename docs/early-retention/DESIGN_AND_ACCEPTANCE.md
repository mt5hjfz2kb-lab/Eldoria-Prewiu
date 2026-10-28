# ELDORIA · Retención temprana · dos destinos jugables independientes

Autoridad: aprobación del propietario 28/10/2026. No se inserta dentro de Códice/Crónicas ni se agregan edificios ficticios. UI en la familia visual **Crónica Real**, siempre una única sesión RoyalHUD, Canvas, EventSystem y LocalGateway. No reinterpretar como una supervisión o automatización fuera de juego.

## Camino de los Vigilantes

- Primera prueba accesible después de construir Aserradero I. El botón no aparece antes de ese momento.
- Desde Bastión II, progreso individual permanente en capítulos de **25 etapas**. Niveles 5/10/15/20/25 tienen guardián destacado. Bastión III habilita segunda tanda; futuros bastiones abren capítulos posteriores.
- Combate inmediato **automático**, usando `SliceRules.Expedition` y `SliceRules.FightCreature`: nunca requiere pulsar habilidades o estar presente durante la pelea. No consume la marcha de mundo ni crea ejército ficticio; utiliza tropas realmente disponibles y héroes desbloqueados.
- Primera victoria de cada etapa: madera, piedra y XP registrada; ninguna repetición duplica recompensas. Los enemigos/escalados son una actividad PERSONAL, no los Élites públicos de la Brecha.
- Experiencia pasiva: tasa de XP depende de hitos alcanzados, calcula el intervalo transcurrido por tiempo UTC y limita acumulación a **ocho horas**. La experiencia de Aldric/Lyra queda registrada en `PlayerState.Retention`; su maestría (80 XP por nivel, máximo 20) mejora de forma limitada ATQ/DEF/VIDA/RUPTURA/PODER **solo en los dos modos PvE tempranos**, sin alterar el combate canónico del mundo ni afirmar que existe una progresión general de héroes. Este límite es visible al jugador.

## Mesa de Exploradores

- El acceso aparece únicamente cuando Bastión II tiene Cuartel construido **y** el jugador ha confirmado su primera marcha real. No es el Faro ni una sección del Códice.
- **Tres encargos por bloque de 12 h UTC**, con máximo **seis pendientes** y sin fabricar meses de actividades tras desconexiones prolongadas.
- Cuatro familias reutilizando texto, modelos y reglas: patrulla corrupta, rescate, suministros, rastros de la Brecha. El jugador puede enviar y cerrar el juego; finalización/claim por UTC.
- Cada encargo dispone de estado disponible, enviado, reclamable, reclamado. Solo el propietario de la partida lo puede reclamar y solamente una vez; los comandos conservan el chequeo de `revision` e `idempotency-id` existente.
- Recompensa en recursos reales y XP guardada, sin sistema monetario nuevo ni barras de energía inventadas.

## Interfaz y desbloqueos

- Son **dos pantallas completas distintas**, una de progresión/combate y otra de órdenes. Cada una tiene cabecera, arte medieval original dibujado por uGUI, información real, botones propios y regreso a Valoria.
- **Dos accesos discretos en el HUD de ciudad** cerca del borde seguro inferior, *por encima* de la navegación del reino. Ninguno se dibuja antes de su desbloqueo, si hay diálogo abierto, menú modal o selección de un edificio.
- No se crea un segundo `Canvas`, cámara, `EventSystem` o sesión, no se altera la navegación canónica ni se reutiliza Códice o Relicario.
- Escenario Vigilantes: sendero y atalaya con cambios cromáticos por capítulos. Escenario Exploradores: carta/mapa, rutas, blasones. Todo geometry editable real, no una imagen maquetada ni recurso externo con coste.

## Evidencia obligatoria antes de integrar en main

1. .NET smoke con el **código real** Domain + Application incluyendo `EarlyRetentionRules.cs` sin duplicar sistemas.
2. EditMode de Unity: desbloqueos, 25 niveles, premios idempotentes, experiencia offline, acumulación acotada y persistencia.
3. PlayMode/capturas reales horizontal 1600×900 y vertical 900×1600 de ambas pantallas; comparación de jerarquía Crónica Real y solapamientos.
4. Click real en combate/encargo, retorno al reino, sin botones prematuros, único HUD y sin errores; la prueba usa `VALORIA_GAMEPLAY_PROBE=1` con archivo desechable, **no** la partida del propietario.
5. Verificación de compatibilidad con PR #119 sobre interfaz y mobile antes de merge/release. Los textos de tests no equivalen a **ART_APPROVED**; tras prueba gráfica se aplica revisión artística en cámara.

## Límites

- El modo multijugador de la Brecha es otro sistema, posterior; estos dos son personales.
- La experiencia almacenada no se representa falsamente como aumento de nivel de héroe antes de existir la mejora de héroes.
- No publicar APK ni tocar `main` de jugabilidad hasta completar las comprobaciones en la rama propia.
