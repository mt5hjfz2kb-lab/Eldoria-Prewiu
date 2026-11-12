# Tesoros de Valoria — ubicación real y recorrido del jugador

Documento de implementación para la PR #181. **Las capturas de concepto no son capturas de Unity.** No describe mecánicas que todavía no existan.

## 1. Acceso en la ciudad

El jugador **no** recibe una ventana obligatoria al iniciar sesión. Hay un pequeño medallón con el cofre ilustrado y una señal de tributo disponible. Al pulsarlo se abre el calendario.

- Si **Vigilia de la Brecha (PR173)** está activa, el panel utiliza el **Canvas independiente ya existente** y se ancla a su `Safe composition`; no añade otro Canvas, otro EventSystem ni inventa otro guardado.
  - Vertical: cofre de acceso en la zona izquierda, aproximadamente **103 unidades de interfaz sobre el borde inferior**, encima de la navegación.
  - Horizontal: acceso junto a las utilidades, **x=256**, aproximadamente **168 unidades sobre el borde inferior**, fuera del objetivo principal y de la fila inferior.
- Si Vigilia está ausente/deshabilitada, el sistema retorna a la zona segura del HUD RoyalGold, conservando la sesión.
- El cofre de acceso mantiene su arte original, con una pequeña luz pulsante solo cuando hay un tributo legítimamente disponible. Puede abrirse también para consultar premios ya cobrados; no entrega nada hasta que el usuario reclama.
- Tutorial, narrativa, edificio seleccionado y una sala del juego tienen prioridad. La ceremonia de un hito puede aplazarse; no debe interrumpir la primera explicación de una mecánica.

**Fuentes de verdad:** `DailyTreasurePanel.SynchronizePresentationCanvas`, `DailyTreasurePanel.Layout`, `RoyalGoldV3EarlyRetentionScreens.Update`.

## 2. Calendario de fidelidad, sin castigos excesivos

El calendario tiene **siete premios**, con el séptimo destacado en una tarjeta más ancha. La entrada consecutiva concede un extra moderado. Si se falta un día, se reinicia **solo** el bonus consecutivo: el progreso de premios recogidos no desaparece.

- Días 1, 2, 3, 5 y 6: presentación breve con icono de recurso, cantidad exacta y confirmación.
- **Día 4:** ceremonia especial, cofre de madera/metal original, brillo cálido, brasas y **dos premios reales**: madera y piedra.
- **Día 7:** ceremonia legendaria; **cuatro premios auténticos** si hay racha que añada comida, o tres sin ella. La luz ligeramente más intensa anticipa el acontecimiento; se conserva la legibilidad de los iconos y textos.
- En todos los casos el comando `ClaimDailyTreasure` concede y guarda los recursos **antes** de mostrar la animación. Si se pulsa «Saltar» o se cierra la aplicación, no se pierde el premio ni se concede dos veces.
- El buzón conserva el justificante informativo del tributo. Abrir o borrar el mensaje no vuelve a abonar recursos.

**Cantidades:** las de `DailyTreasureRules.ForDay` son candidatas de balance, no económicas finales. Los porcentajes de algunos conceptos gráficos anteriores **no forman parte del juego**.

## 3. Otras celebraciones que forman parte del trabajo

| Acontecimiento real | Disparador | Presentación | De dónde sale el premio |
|---|---|---|---|
| Primera familia de hallazgos de exploradores | `ScoutClaim` realmente completado; especie nunca descubierta antes | Ceremonia especial, con tono verde/ámbar | Informes/recompensas que ya concedió `LocalGateway` |
| Cierre de un capítulo de Vigilantes | Victoria del verdadero guardián de la etapa final | Ceremonia legendaria, tono violeta/ámbar | Informe de combate ya resuelto |
| Lyra se incorpora por primera vez | Cambio legítimo de `LyraRecruited` y carta narrativa existente | Presentación legendaria de su incorporación, sin carta/recompensa inventada | Progresión de historia y su carta de buzón |
| Bastión II y Bastión V completados | Obra realmente finalizada y carta narrativa recibida | Ceremonia de reconstrucción y bronce cálido | Mejoras que ya registró el juego; **carta no pagadora** |

Los acontecimientos no pagan premios nuevos al reproducir su ceremonia. **No** se muestran continuamente en bucle ni se presentan como conseguidos al iniciar una partida que ya los había completado: los sucesos offline se consultan en el buzón.

**Fuente:** `DailyTreasureMilestoneRules`, `DailyTreasureMilestoneQueue`, integración RoyalGold y el recibo existente del gateway.

## 4. Arte y calidad mínima

Todo lo anterior utiliza la misma colección de **nueve sprites ilustrados originales** versionados en la rama: cofre cerrado/abierto, tres marcos/texturas nine-slice y cuatro iconos. Es el idioma común (hierro forjado, bronce, madera negra y luz ámbar), pero **cada gran acontecimiento tiene una puesta en escena distinta**, no una copia del cofre con otro color:

- Lyra: el hall pictórico ya existente en Eldoria. Se prioriza su presentación nativa aprobada si la pantalla canónica de héroes está abierta; la ceremonia de cofre se difiere hasta cerrarla y no inventa un retrato nuevo.
- Exploradores: el mapa pintado existente, visible como escena protagonista junto al cofre en horizontal.
- Vigilantes: el panorama auténtico de Eldoria, con composición de capítulo y luz violeta contenida.
- Bastión: **la ciudad real**, visible tras el marco semitransparente; no se pinta un Bastión ajeno a su nivel real.

`DailyTreasureStageDirection` coloca paisaje, cofre, encabezado y texto de logro en formato vertical/apaisado. `DailyTreasureCeremonyTheme` y `DailyTreasureRevealTimeline` aportan luces y movimiento; `SafePainterlyDrift` mueve suavemente la pintura con recorte UV seguro. Las recompensas individuales salen hacia tarjetas ilustradas **después del pago**. La composición nativa sigue **pendiente** de revisión artística en Unity.

El **acabado mínimo** es el de la previsualización artística que aprobó el propietario, no una sustitución por rectángulos ni un conjunto de efectos luminosos que dificulten ver los premios.

## 5. Certificación pendiente — NO-GO hasta superar Unity

- Compilar la PR exacta en Unity 6000.3.23f1 y ver la **ciudad real** con el acceso en ambas orientaciones.
- Ejecutar la prueba de componente de recompensas realmente cobradas y las muestras estilísticas, revisar imagen por imagen: tipografía, materiales, perspectiva, calidad del cofre, animación completa, luz, ritmo, sonido, solapes, safe area y controles táctiles.
- Comprobar tras guardado/cierre/reapertura, más la partida real Bastión I–V, la convivencia con Vigilia, el buzón y móviles físicos.
- Medir retención y economía con testers sin deducir mejora de fidelidad simplemente por tener recompensas bonitas.

**Estado:** código y arte base versionados / pruebas de dominio y origen de arte / Unity y calidad jugable todavía sin aprobar / PR181 no fusionada ni publicada.
