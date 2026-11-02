# Eldoria — Preparación de Marchas: reconstrucción visual auténtica

## Decisión / alcance
Mandato del propietario, 2026-11-02: reconstruir la pantalla Marchas a partir de la referencia artística original, desde 5,7/10 hasta aspiración ≥8,3/10. **Sin afirmar puntuación no otorgada**. PR canónica #136 (`fix/player-feedback-context-ui-20261030`). #134 (`feature/eldoria-v41-contracts-20261030`) continúa DRAFT sin fusionar. El titular del workstream `valoria-master-plan-layered-construction-20261016` fue transferido mediante CAS en main (revisión 106) y ampliado para una prueba PlayMode obsoleta (rev107). Sin cambio en las reglas de combate, coste, salvado, tres marchas o héroes/tropas bloqueados.

## Componentes integrados
- Nuevo `RoyalGoldV41MarchArt.cs` con materiales UI nativos independientes: recess de retratos, franjas de nombres reales, filigrana nine-slice, borde de tropa y resumen. No añade interacciones ni pinta datos ficticios en sprites.
- `RoyalGoldV3ManagementScreens.cs` con imágenes reales ajustadas por `contain` y no por recortes `cover`, tres plazas auténticas, tercera bloqueada, T1/T2 reales, controles de cantidad y porcentaje, métricas reales. Panel de órdenes más compacto; fondo de patio auténtico visible. Ningún cambio a `SetQuantity`, `Confirm`, `LocalGateway`, `MarchCapacityRules` ni al guardado.
- Correcciones sucesivas provocadas por fallos reales de V4.1 (desbordamiento de nombre de héroe, texto de tropas, botón de porcentaje), **no solo informes**.
- Prueba `RoyalV3ManagementIntegrationTests.FullScreenRoyalTrainingAndMarchesPreserveCameraAndDoNotWriteOwnerSave`: su viejo contrato buscaba el número `7` en el título decorativo. Ahora comprueba título nativo y valor `7` en `March / live metric 0`, manteniendo la verificación del gateway `PreparedTroops.ArcherT1==7` y héroe Aldric. No rebaja la regla jugable.

## Evidencia Unity auténtica
- Antes: `389580f29d1a1433e9d382d24e96addd0280fe22`, native 41507427853, artifact 13171008209.
- Después artístico: `876846e7a4134315cb70eec6a882bfbdf048a309`, native 41524412737, artifact **13179750990**. `EldoriaV41NativeCapture` terminó **PASS** y produjo cuatro fotografías PNG Marchas (`march-reference-ratio` 941×1672, `march-landscape` 1600×900, `march-small-phone` 390×844, `march-small-phone-landscape` 844×390). `Verify three simultaneous marches in the actual integrated scene` también **PASS** en este run. Incluye comprobación de controles reales +25/50% y guardado. **La regresión completa de ese job es independiente y se documentará al terminar.**
- Versión con actualización del contrato de test (código artístico idéntico): commit `a84f0febbebbbee8aec58713e16160afeb1fdcdc`, nueva certificación 41525299161. Solo se puede atribuir PASS completo a este nuevo SHA cuando el run finalice con conclusión success.
- A/B con las ocho PNG originales y fotografías antes/después listo en la sesión de producción; no es una imagen AI.
- Comparación de referencia: arte aprobado `MARCHA_TRES_HEROES_PORCENTAJES.png` de Biblioteca; sus otras clases y estadísticas no son jugables ahora y por tanto no se han incorporado.

## Evaluación artística honesta
La mejora del A/B real es moderada: retratos más completos y con mejor legibilidad, composición de tropas menos opaca, paneles mejor delimitados. No alcanza de manera demostrable el objetivo ≥8,3/10. Persisten: poca semejanza de la jerarquía global con la referencia artística, tipografía muy pequeña y tarjetas comprimidas en 844×390, y falta de ornamentación a nivel de arte final. Se necesita nueva intervención de macrocomposición (no microretoques) o aceptación provisional explícita del propietario; ninguna de ambas se presupone.

No publicar, fusionar, gastar créditos ni modificar el guardado/combate. `ART_APPROVED=false`.
