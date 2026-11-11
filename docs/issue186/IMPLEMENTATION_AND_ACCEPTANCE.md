# Issue #186 — Primer avance de implementación, aislado

**Estado:** módulo de reglas de dominio escrito en una rama separada; la integración jugable y las capturas de Unity están PENDIENTES. No constituye una modificación de la beta ni del juego publicado.

## Contrato aprobado por el propietario
- Nodos, caza y edificios suman recursos al contador por sus vías actuales.
- Cualquier otra recompensa en madera, piedra o comida se entrega como paquete a mochila: **no suma al contador al recibirlo**.
- Buzón informativo con reclamación explícita: mensaje → Reclamar → paquete en mochila → Usar → recursos en contador. Leer no es reclamar.
- Iconos con aviso discreto para correo nuevo / premio diario listo. La novedad de mochila no se trata como una obligación eterna de consumir.
- Tutorial contextual con mano/puntero anclado al control REAL, que enseña el siguiente paso y solo lo considera completado tras la acción de juego confirmada.

## Implementado en este avance (no activado aún)
\`RewardExperienceRules.cs\` contiene objetos de paquete serializables, \`TryIssue\` para fuentes indirectas, \`ClaimMail\`, \`TryUse\` que abona una sola vez, comprobación de desbordamiento y copias profundas. Conserva recibos usados como lápidas para impedir duplicaciones en reintentos. También contiene la clasificación semántica de indicadores y la persistencia de los pasos del tutorial; NO dibuja puntos ni manos sin un control real.

## Por qué está aislado
La PR #181 ya modifica \`GameState.cs\`, \`LocalGateway.cs\` y el flujo de recompensas. La PR #173 es dueña del HUD nuevo; la #179 está resolviendo los reinicios del iPhone. Integrar en ellas a ciegas crearía conflictos o comportamientos no probados. Este avance no modifica esos archivos, ni usa Windows.

## Pendiente obligatorio para una función jugable
1. Después de coordinar los tres titulares, agregar \`ResourceRewardInventory\` y \`GuidedTutorialProgress\` al guardado real \`PlayerState\`, su clon \`Snapshot()\` y la migración segura de guardados antiguos.
2. Unificar TODAS las rutas de abono que hoy incrementan directamente contadores (misiones en \`GrantWebMissionRewards\`, Vigilantes, exploradores y Tesoros; distinguir de caza/nodos/producción). Concesión, reclamación, consumo y guardado deben ser **una transacción autoritativa** de \`LocalGateway\`, no operaciones UI.
3. El buzón debe mostrar premios pendientes y reclamar con comando real sin entregar al contador. Mensajes solo informativos deben permanecer iguales.
4. PR #173 debe integrar indicadores visuales en accesos reales y el puntero ligado a \`RectTransform\`, respetando el safe-area, sin duplicar HUD. Validar que el aviso de premio no se quita por abrir la ventana.
5. Definir apilamiento, límites/espacio de mochila y compatibilidad con recompensas antiguas. Nunca dar recursos directos como fallback ante mochila llena.
6. Unity EditMode/PlayMode, guardados reales, 4 resoluciones, controles táctiles, WebGL/iPhone y recorrido I–V. Un test verde de Ubuntu NO acredita Unity ni experiencia del jugador.

**No fusionar/publicar** hasta tener integración de gameplay y pruebas sobre la misma fuente, y comprobación artística con el diseño aprobado.
