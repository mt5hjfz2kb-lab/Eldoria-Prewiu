# Eldoria — Buzón V4.1 · Aceptación provisional del propietario

Fecha: **2026-11-03**. **Decisión del propietario:** «Está bien, lo dejamos como aprobado provisional».

## Identidad de la candidata

- **Versión B preservada**: commit de código exacto `178d12ca8e6467c00b78ac96a1a86bd7511e4df8`.
- **Rama/PR canónica**: `fix/player-feedback-context-ui-20261030`, [PR #136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/136), permanece **DRAFT**, sin merge ni publicación. Esta aceptación se refiere al **Buzón**, no a todas las pantallas incluidas en la PR.
- **Versión A**: `408f0a1f25e3e170bff7a4fa34affe7a0585387c`. Referencia de fidelidad visual: `buzon_final_pestanas_APROBADO.png`, del kit artístico V4.1 aprobado.
- **Comparación presentada**: capturas nativas de Unity, A/B vertical/horizontal; candidata B inspeccionada en 900×1600, 1600×900, 390×844, 844×390, más pantalla de detalle, buzón vacío y listas con 12 mensajes QA aislados (sin crear correos ficticios en la partida del usuario).
- **Certificación nativa del SHA exacto**: [run 41621181708](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41621181708) — **completed SUCCESS**. Artefacto `eldoria-v41-native-41621181708` (ID `13223123153`); pruebas del Buzón V4.1, tres marchas, EditMode y PlayMode completadas sin errores.

## Qué se conserva

1. Cuatro categorías auténticas: informes, recompensas, sistema y narrativa; indicadores de no leído, tarjetas ilustradas y pestañas, marcos nine-slice, recuentos y fechas reales.
2. Adaptación a cero, uno y múltiples mensajes; paginación con 5 tarjetas habituales y 2 en móvil horizontal compacto, sin inventar correos para rellenar el espacio.
3. Lectura/apertura de informes con detalle desplazable, borrado, recarga y persistencia reales. Los mensajes simulados para estresar la lista se construyen sobre un snapshot de QA de solo lectura y no se guardan en producción.
4. Informes de combate limitados a resultados observables (marcha, unidades, rondas, vida, botín) y sin revelar fórmulas ocultas o estrategias óptimas.
5. **Separación del informe y el pago**: los premios se liquidan donde corresponde en el gateway; abrir, marcar leído o borrar no vuelve a abonarlos. No inventar un botón de «reclamar» que implique un segundo pago ni activar el motor V4.1 en campaña.
6. También se preservan las decisiones de Entrenamiento aceptadas provisionalmente en la misma PR; no reconstruir pantallas ajenas a esta área por defecto.

## Defectos artísticos pendientes (no bloquearon la aceptación provisional)

- **Fidelidad insuficiente a la lámina aprobada**: el Buzón B sigue siendo más esquemático y menos ornamentado que el pergamino/fondo/arquitectura de referencia.
- **Superficie desocupada**: especialmente con uno o ningún mensaje persisten zonas excesivas de fondo oscuro. Mejorar la composición escénica y su relación con la lista sin añadir contenido ficticio.
- **Jerarquía visual y materiales**: revisar consistencia de iconos, tipos, sombras, contraste, papel y metal nine-slice en 390×844 y 844×390, además del detalle de informes, antes de eventual aprobación definitiva.
- **Puntuación objetivo ≥8,0/10**: no acreditada ni declarada; `ART_APPROVED=false`. El propietario ha elegido conservar la propuesta **provisionalmente**, no una aprobación estética final.
- Cualquier futura reconstrucción debe presentar nuevo A/B genuino del nuevo SHA, cuatro resoluciones y QA vacío/1/12, leer/borrar/recargar, informes y no duplicación; no utilizar pruebas de otro commit.

## Relevo seguro

El trabajo de este bloque queda **entregado provisionalmente con pruebas completas**. La PR permanece en borrador y no se publica ni fusiona. Conservar este documento en `main` **sin modificar la rama de la PR** para que el SHA de código y la certificación nativa sigan coincidiendo.

La reserva Windows se liberará solo mediante el protocolo de `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md` tras confirmar todas las condiciones y la cola de workflows.
