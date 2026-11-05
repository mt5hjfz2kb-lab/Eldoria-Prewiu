# ELDORIA — Plan de corrección narrativa y tutorial en UNITY
**Reorientación del alcance — 5 de noviembre de 2026**

## Fuente y destino
- **Destino exclusivo de implementación: Unity** (escena jugable canónica, guardado real, HUD y sistemas reales).
- **Prototipo web: referencia de consulta**, exclusivamente para comparar lore, orden de acontecimientos y diseño de desbloqueos. No modificar sus archivos, UI, textos, lógica, efectos ni pruebas de la versión web.
- Canon: `docs/BASTION_I_X_MASTER_TABLE.md` y `docs/BASTION_III_FISSURE_CANON_20261027.md`. Las decisiones recientes del propietario prevalecen sobre textos históricos.
- Sin nuevas ilustraciones o modificaciones artísticas en esta fase. Ni gastos ni publicación.
- Respetar los trabajos existentes: PR #134 (combate), PR #152 (construcción), titulares del área Valoria/UI. La PR #160 permanece DRAFT hasta reconciliar titularidades.

## Diagnóstico e implementación en Unity
1. **Bloque I — Historia I–V:** ordenar las escenas por estado autorizado del jugador y no por simple consulta de pantalla; evitar que se reintroduzcan escenas completadas. Revisar la persistencia tras volver a entrar.
2. **Bloque II — Conocimiento del jugador:** revelar Lyra y los detalles de su hermano solo después de la investigación y el encuentro. Tras victoria, reclutarla y revelar el manuscrito. La fisura local es distinta de la Brecha principal.
3. **Bloque III — Redacción y nomenclatura:** corregir encabezados del Bastión, diálogos del consejo IV, términos Fisura/Brecha y textos de misiones que podrían adelantar descubrimientos.
4. **Bloque IV — Tutorial Unity:** introducir cada destino después de su desbloqueo: producción, expediciones, cuartel, marchas, Vigilantes, Exploradores, buzón, héroes, mejoras. La primera ayuda debe invitar a una acción real y quedar completada solo al realizarla.
5. **Bloque V — Progresión hasta X:** el canon web describe el arco I–X, pero la implementación Unity actual solo llega a V. Diseñar y trasladar el tutorial VI–X a Unity cuando se implemente la progresión real. No fingir que existe en la build Unity.
6. **Bloque VI — Combate:** reconciliar con el trabajo canónico de PR #134 antes de reemplazar cualquier regla o resultado. La resolución del combate y el tutorial deben reflejar victorias, heridos y recompensas reales, no inventadas.

## Qué se ha implementado en esta PR (solo Unity)
- `NarrativeCoherenceRules` en `GameState.cs`: selección y orden de eventos, etapas de conocimiento de la fisura.
- `ValoriaJourneyGuidance.cs`: elección de escena por progreso guardado y mejora textual del consejo IV.
- `ExistingSliceBinding.cs`: descripción contextual de la fisura, nombres de acciones sin spoilers y etiquetas de Bastión, no capítulos falsos.
- `ValoriaMarchPresentation.cs`: terminología de investigación coherente.
- `RoyalGoldV3EarlyRetentionScreens.cs`: no mostrar la maestría de Lyra antes de desbloquearla.
- `BastionIIIProgressionTests.cs`: pruebas EditMode de información conocida, orden de eventos y persistencia.

## Pruebas y criterios de cierre
- Ejecutar EditMode/PlayMode de Unity sobre el **HEAD exacto** tras aprobar la titularidad de archivos y liberar el runner, sin desplazar otros trabajos.
- Partida nueva real de Unity I–V, incluidos primer inicio, reconstrucción, salida al mundo, fisura de 20 minutos, contacto con Lyra, victoria, reclutamiento, recarga y Bastión IV–V.
- Pantallas reales de Unity en orientación vertical/horizontal: cero bloqueos, recortes y escenas superpuestas. Compararlas con las referencias gráficas aprobadas de Unity, no con capturas experimentales del web.
- Prueba de regreso offline y guardados históricos: cero revelaciones prematuras o recompensas repetidas.
- La calidad de ritmo y redacción requiere revisión jugable; un test funcional PASS no equivale a aprobación artística.
- **Pendiente de verificar**: CI completo de Unity, pruebas físicas móvil y compatibilidad final con PR #134. No fusionar ni publicar antes de resolver los requisitos de integración.

## Registro de corrección de alcance
Una pasada anterior había añadido pruebas y cambios a `v0220/**` y `qa/**`. Se han revertido de esta rama sin alterar `main` ni el historial del proyecto. Las evidencias de navegador pertenecen a ese intento descartado y **no certifican Unity**.
