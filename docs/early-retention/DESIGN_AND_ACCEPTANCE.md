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

- Son **dos pantallas completas distintas**, una de progresión/combate y otra de órdenes. Cada una tiene cabecera, ilustración ambiental original y un recorrido/tarjetas superpuestos por uGUI, información real, botones propios y regreso a Valoria.
- **Dos accesos discretos en el HUD de ciudad** cerca del borde seguro inferior, *por encima* de la navegación del reino. Ninguno se dibuja antes de su desbloqueo, si hay diálogo abierto, menú modal o selección de un edificio.
- No se crea un segundo `Canvas`, cámara, `EventSystem` o sesión, no se altera la navegación canónica ni se reutiliza Códice o Relicario.
- Escenario Vigilantes: `Resources/EldoriaRetention/vigilants-illustration.png`, ilustración original medieval con camino, atalaya, luz cálida y entorno de la Brecha. El camino real de 25 etapas y los cinco guardianes siguen siendo componentes independientes de uGUI, no parte de una imagen con números falsos.
- Escenario Exploradores: `Resources/EldoriaRetention/scouts-illustration.png`, mapa ilustrado original con rutas y rosa de los vientos. Los tres pergaminos y sus títulos, tiempos, sellos y acciones siguen siendo componentes uGUI con estado real.
- Ambas ilustraciones son texturas 640×640 indexadas (256 colores) sin textos, libres de elementos interactivos y con importación normal de Unity. `RoyalGoldV3EarlyRetentionScreens` las carga como `RawImage` en el **Canvas existente**, recorta proporcionalmente sin estirar y mantiene el arte vectorial original como respaldo si faltan recursos. No hay servicios, compras, nuevas escenas 3D ni cámaras adicionales.
- Al enviar o recoger una expedición se muestra un reconocimiento breve y no bloqueante; la única autoridad para el premio continúa siendo `LocalGateway`, nunca la presentación.

## Evidencia obligatoria antes de integrar en main

1. .NET smoke con el **código real** Domain + Application incluyendo `EarlyRetentionRules.cs` sin duplicar sistemas.
2. EditMode de Unity: desbloqueos, 25 niveles, premios idempotentes, experiencia offline, acumulación acotada y persistencia.
3. PlayMode/capturas reales horizontal 1600×900 y vertical 900×1600 de ambas pantallas; comparación de jerarquía Crónica Real y solapamientos.
4. Click real en combate/encargo, retorno al reino, sin botones prematuros, único HUD y sin errores; la prueba usa `VALORIA_GAMEPLAY_PROBE=1` con archivo desechable, **no** la partida del propietario.
5. Verificación de compatibilidad con PR #119 sobre interfaz y mobile antes de merge/release. Los textos de tests no equivalen a **ART_APPROVED**; tras prueba gráfica se aplica revisión artística en cámara.
6. Verificar con capturas Unity que **ambas texturas independientes** se cargan efectivamente (no basta con la compilación o la imagen de referencia), sin bloquear raycasts, con pergaminos legibles y camino de 25 etapas distinguible.
7. El run Ubuntu de `early-retention-domain.yml` comprueba formato, CRC, paleta, resolución, diferencias entre ambas imágenes y publica los originales como artefacto para revisión humana. Es una prueba de integridad, no aprobación artística.

## Límites

- El modo multijugador de la Brecha es otro sistema, posterior; estos dos son personales.
- La experiencia almacenada no se representa falsamente como aumento de nivel de héroe antes de existir la mejora de héroes.
- No publicar APK ni tocar `main` de jugabilidad hasta completar las comprobaciones en la rama propia.

## Relevo y revisión independiente del 28/10/2026

El propietario reasigna PR #119 y PR #120 al mismo responsable, conservando las dos PR canónicas y presupuesto 0 €. Unity nativo 40657717839 certificó técnicamente el SHA 01685c187f8df1a23c5743a76a6d27c3f32ab23e; artefacto 12802867199, diez EditMode y ocho PNG originales. Las ocho imágenes se abrieron y compararon con la revisión anterior (artefacto 12801360443). La referencia Crónica Real original se recuperó y su SHA-256 27565c7b85004dd7b84efdfe7587b9bfd976e376b07242c51ae5241851e60582 se verificó.

Resultado: los títulos de cartas y la jerarquía mejoran; tres encargos visibles y botones completos. **Revisión todavía rechazada para cierre:** el rasgo del enemigo no cabe en el bloque vertical y el texto de victoria se corta en horizontal. Los árboles decorativos desbordan el marco. Corregir esos defectos conservando arte, lógica, recompensas y navegación.

La siguiente certificación exige doce capturas auténticas: los cuatro estados en 1600×900, 900×1600 y 390×844. Comprobar altura tipográfica durante el render real, las tres acciones de envío y recogida con raycast, botón de regreso y persistencia tras recarga. No confundir una prueba de código con aceptación visual. Se reserva Windows a PR #119 tras terminar la prueba 120, sin interrumpir procesos; reconciliar la combinación antes de certificar 120 de nuevo.

- Review found a real eight-hour cap defect: a nine-hour absence allowed eight hours then an immediate extra hour. Clamp the accrual start before consuming paid time; parameterized 8/9/16/30-hour tests also reject a fresh second claim. Native button proof now claims offline hero XP and verifies its save reload.
