# Eldoria — base principal de desarrollo

Decisión del propietario, 18 octubre 2026 17:23 Europe/Madrid: consolidar la versión integrada actual y conservar los experimentos únicamente como referencia.

## Decisión

La rama canónica sigue siendo main. Se incorpora el árbol Unity exacto de native111, cuya escena integra Valoria, Royal Gold y Zona1, y se selecciona esa escena en EditorBuildSettings. Se conserva main como primer padre y la rama entregada como segundo padre: sin force-push, sin perder cambios/documentación de main y sin borrar ramas. El árbol Unity de main no había cambiado respecto al ancestro común; no existe conflicto de runtime concurrente. Se conservan todas sus rutas previas. Los cambios de árbol Unity durante la consolidación se limitan a elegir la escena de entrada; código, assets, escena y paquetes conservan sus blobs probados.

## Prueba y alcance

Native111 39138751241, fuente48837605705fed198ec485f2c2726af2ffa5eea9, salida663acb07f3a2693a70f46b3e94a40340a38ad84f. Construcción, recursos, ruta corrupta, Bastión II, cuartel, entrenamiento, marcha, Engendro, recarga, narrativa y guardados canónicos PASS mediante botones uGUI. El reloj de la prueba es acelerado: no equivale a evaluar ritmo real. Web artefacto12141221363 publicado privado en https://valoria-city-world.kbkvc5c45n.chatgpt.site.

Base de desarrollo consolidada NO significa juego terminado, aprobación artística 8/10, lanzamiento comercial o iPhone físico certificado. No cambian economía ni partidas por esta consolidación. Android97/1.5 y el antiguo Pages quedan como entregas históricas, no se actualizan silenciosamente.

## Una sola línea

El registro active contiene solo el trabajo integrado sobre main. UI y 4X anteriores pasan a history; sus últimas fuentes236a81aac894ea0845b85c90310b92dddb32d10f y f3ccc8d0b7767472ef09b02c3091f6d676f204ee ya están consumidas. pipeline/historical-branches-20261018.json inventaría 201 ramas preservadas. Se archivan lógicamente, no se borran ni se bloquean mediante ACL. Los otros chats deben consultar main antes de actuar; editar este registro no puede detener una conversación remota.

Las aportaciones futuras parten de main. Recuperar de una rama histórica solo un recurso/cambio con beneficio concreto, comprobar licencia/dependencias y probarlo en el juego integrado. No retomar una alternativa ni fusionar sus cambios completos.

## Construcción y entrega

Escena: Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity. Método WebGL existente: Eldoria.ReferenceClean.Editor.ValoriaIntegratedWebBuild.Build. Workflow nativo existente trasladado a main: ejecutar mediante workflow_dispatch o cambio explícito de request con mensaje [valoria-native], siempre con lease Windows y runner libre. Esto evita duplicar native111 por la consolidación documental. Mantener gates de código/arquitectura y regresión existentes; sus capturas legacy no son la autoridad artística actual. La publicación Pages antigua queda desactivada; el enlace privado actual se conserva.

La herramienta del artista sigue siendo Qwen/Ollama + Blender y BlenderMCP instalados. Presupuesto0€. No Tripo. Mejorar la misma ciudad; ver prioridades y límites de la revisión en docs/ELDORIA_PLAYER_REVIEW_20261018.md.
