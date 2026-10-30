# Eldoria V4.1 — implementación real en curso (PR #134)

**Fuente principal:** Issue #131 y referencias originales verificadas que están en la biblioteca ChatGPT
`/Eldoria/Produccion-pendiente/`. **No generar nuevos diseños ni sustituirlos por aproximaciones.**

## Qué hay realmente en la rama
- Contratos de dominio `EldoriaV41Contracts.cs`: tres huecos de héroe, cálculo y edición por cantidad/porcentaje, reparto sin crear tropas, buzón persistente y copias independientes de informes.
- Combate `EldoriaV41Combat.cs`: motor **experimental aislado** basado en V4.1, sin sustituir la campaña; T2/T3 aportan progresión, familias futuras continúan bloqueadas por narrativa.
- `GameState.cs`: campos aditivos de héroes y buzón; `LocalGateway.cs`: comandos validados de marcha, correo de combate/exploración y recompensas, lectura/borrado y no duplicación. Mantiene `SliceRules` como motor actual de I–V.
- `RoyalGoldV3ManagementScreens.cs`: nuevos controles provisionales de tres héroes y porcentajes y su envío real, sobre el único HUD.
- `RoyalGoldV41MailboxScreen.cs`: buzón UI funcional **provisional**, con cuatro categorías, informes con datos de combate histórico, acciones reales y no leídos. **No cumple aún la referencia artística final.**
- Pruebas `.NET 8` sobre reglas y gateway reales, más nuevos EditMode de Unity; suites históricas preservadas.
- Los nuevos scripts incluyen archivos `.meta` con GUID estables.

## Qué no se debe declarar completado
1. **Fidelidad artística 100% NO CERTIFICADA.** Falta importar las imágenes exactas y separar fondos, marcos, pestañas y retratos en capas editables. Son composiciones PNG con controles y texto horneados; los 15 recortes anteriores son referencias, no sprites limpios.
2. La UI funcional actual es una etapa provisional. Comparar pantalla de entrenamiento, marcha y buzón en **capturas reales Unity** con las imágenes inmutables de Issue #131. No fusionar o publicar si el resultado es una reinterpretación.
3. Los tres huecos son visibles pero solo Aldric/Lyra están desbloqueados; el tercer héroe queda bloqueado. Paladines/Brujos no son jugables aún; su aparición seguirá Bastión XI o contratos narrativos posteriores.
4. El combate V4.1 no está activado en la campaña. Cualquier sustitución debe pasar simulación + PlayMode I–V + pérdida/heridos/guardados + revisión de estadísticas.
5. No hay APK o WebGL publicados con estos cambios. PR #134 debe permanecer **DRAFT** hasta QA.
6. La certificación UI nativa de esta rama se consulta en Actions: no interpretar un smoke .NET verde como ART PASS.

## Identidad de las referencias aprobadas
- Marcha de tres héroes y porcentajes `9afd178a57783f83bd0ccb5bbfb78c50643eb75ae74d01cbed63375d7dcd091c`.
- Lámina de estética ligera `554329927bfd09cf6a42ce8eae930a03a3f0d3193a1a933c1db6d4a9c58cab93`.
- Entrenamiento recortado `a110ba0ffd2a08fa4976c488f87e624fa5601fe3f36b223ee82d19f306adbadc`.
- Buzón final con pestañas llamativas `ef01ed38d83d0be012561320727098e59eaa1e809d08f2febf1b1dd44886b0dd`.

**Aceptación final:** controles y partidas reales; un HUD/cámara, tres marchas globales, máximo tres héroes por marcha según desbloqueo, ratios válidos, tier > tier con composición igual, inbox idempotente y con informes históricos, pruebas físicas/responsive, y reproducción fidedigna validada por el propietario. Al detectar divergencias artísticas no inventar nuevas imágenes.
