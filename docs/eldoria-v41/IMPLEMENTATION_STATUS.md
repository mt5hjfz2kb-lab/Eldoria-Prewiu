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
1. **Fidelidad artística 100% NO CERTIFICADA.** Las cuatro referencias originales se recuperaron y verificaron por SHA256. Se importaron 18 derivados trazables, sin nuevos diseños, con procedencia en `Resources/EldoriaV41/provenance.json`. Las capturas reales aún muestran diferencias de composición: no interpretar la importación como aceptación visual.
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

## Continuación y comprobación nativa — 30/10/2026

- Run `41030096500`: compilación Unity y prueba V4.1 con entrenamiento, preparación de marcha, combate real, lectura/borrado y recarga de guardado PASS. Capturas reales disponibles en `eldoria-v41-native-41030096500`.
- Run `41030966280`: V4.1 PASS; tres marchas globales reales y combinación con investigación PASS; retención PASS. La prueba de Aldric asumía que una recolección automática llevaba al héroe; ahora lo selecciona explícitamente mediante ConfigureMarch antes de verificar su ausencia y regreso.
- Revisada la importación de texturas: se desactiva la conversión a potencias de dos para conservar proporciones originales. La prueba nativa exige las dimensiones originales del arquero. Los huecos serializados como cadenas vacías no deben mostrar compañeros Aldric inexistentes.
- Revisión `bc9c67bcd94c42f1c306a653a99cc7a9383b5830`: certificación completa en `41031738522` PASS: 73/73 PlayMode sin fallos, ignorados ni inconclusos, además de EditMode, pruebas nativas de marchas, retención, Aldric y recorrido I–II. No equivale a una nueva campaña continua I–V. Ninguno de estos estados equivale a ART_APPROVED o prueba física móvil.
- No se ha fusionado ni publicado esta PR. La publicación nocturna Android de main es independiente y no contiene esta interfaz.

Última revisión `32c3109d5791db00fcfc9900f4d5dd3bd5e9dbac`: comprueba selección real de Lyra y reparto 60 T1 + 60 T2 mediante el slider al 50%, acceso al correo por raycast del HUD, proporciones, capas visibles, indicadores no leídos y fechas reales. Certificación `41034159279` terminada SUCCESS: 61/61 EditMode y 73/73 PlayMode, cero fallos. Log del job `123990811759`, resumen real a las 09:47:10 UTC. La prueba de retención separada se omitió en este run; su PASS anterior no debe presentarse como una nueva ejecución.


## Bloqueo de edición concurrente y siguiente cambio concreto

- PR136 sigue editándose y su reserva histórica cubre `Runtime/**`, `LocalGateway.cs`, `GameState.cs`, el árbol de ArtTests y `ui-progression-cert.yml`. Sus ocho archivos cambiados no coinciden con los de PR134, pero el gate de propiedad rechaza el alcance solapado (run `41034337772`).
- Se ha conservado su trabajo y registrado la reconciliación necesaria en la entrada V4.1 de `main/pipeline/active-workstreams.json`. No se ha cambiado su propietario ni estrechado su alcance unilateralmente. El runner V4.1 se liberó tras completar la certificación.
- Diagnóstico visual: entrenamiento y marcha aún usan grandes paneles azul oscuro en lugar de la composición y entorno de la referencia; la selección de Lyra funciona pero muestra una silueta genérica; el buzón necesita afinar proporciones y tipografía. Repetir la suite completa sin cambiar esa composición no resolverá la fidelidad.
- El paquete original `Eldoria_Heroes_Integracion_Preparada_v14.zip` se inspeccionó: contiene una imagen aprobada de Lyra, pero es una ilustración plana con cifras y controles integrados. Puede servir como fuente de retrato, nunca como pantalla de datos reales. No se han modificado los recursos ni el trabajo de PR135.
- Siguiente acción: reconciliar las reservas entre PR134/136 conservando los ocho archivos propios de PR136; reconstruir la composición original con controles dinámicos, incorporar un retrato trazable de Lyra y revisar primero las capturas nativas focalizadas. Solo escalar a regresión completa después de una mejora visual suficiente.
- No integrado, no publicado, sin aprobación artística ni prueba física móvil. Los otros trabajos abiertos se revisarán después de cerrar esta misión, conforme a la orden actual del propietario; no se han asumido cambios concurrentes ajenos.
