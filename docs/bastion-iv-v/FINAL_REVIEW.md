# Bastión IV–V · revisión de migración

> **Corrección posterior obligatoria:** la orientación inicial fue rechazada, incluso en la captura normal. PR130, integrada en5878fca, corrige los cinco giros y el acceso. Native40929047929 SUCCESS:54Edit/73Play,386 IV–V,I–II,76 PNG y raycast32/280. Captura actual SHA256 `c4613c33e11061bf9c34e36b31157c1023913cc171afefd71b27ce0813058e8b`. Web40929055448 intento2 SUCCESS. Ver `ORIENTATION_REWORK.md`; las valoraciones previas de orientación quedan supersedidas.


Fuente de ejecución: `1469708a79dfb8c0ef0b0faa4bcb971ce88a4b32`. Escena canónica `Valoria_City_World_UI.unity`, Unity 6000.3.23f1, render D3D11 real. Esta revisión acepta el alcance visual de IV–V. Los checks generales y web también pasaron y PR129 está integrada en main.

## Resultado visual

| Edificio V | Revisión en la cámara del jugador | Veredicto |
|---|---|---|
| Bastión A | Fortaleza de torres cuadradas, piedra clara y cubiertas azules; acceso visible, escala del emplazamiento conservada | Aceptado |
| Aserradero B | Nave compacta, madera/piedra, patio de troncos y lectura productiva | Aceptado |
| Cuartel A | Patio militar legible, perímetro contenido y materiales coherentes | Aceptado |
| Granero simplificado | Volumen de almacenamiento, portón y carga; conserva la identidad funcional | Aceptado |
| Cantera simplificada | Nave, mecanismo y pequeña carga de piedra; despejada de los árboles que la ocultaban | Aceptado |

Los GLB V conservan los bytes de las entregas aprobadas, contrastados con `tools/bastion-iv-v/assets.json`. La integración cambia posición, escala y orientación de las instancias, sin reinterpretar sus mallas. Cada arquitectura aparece al finalizar el nivel V de su propio edificio y permanece como base para niveles futuros. Alcanzar Bastión V no mejora automáticamente los demás edificios.

Los niveles intermedios conservan sus arquitecturas previas y suman stock de trabajo reutilizado por nivel. La cantera I–IV usa el taller medieval original con sus materiales; el GLB provisional girado y sin atlas fue rechazado y su copia de integración retirada. Se conserva su fuente histórica. Las capturas de cada nivel muestran cambios pequeños; la sustitución V es el salto principal.

En el mapa, el Bastión V utiliza la misma arquitectura, apoya sobre la altura regional y conecta su entrada con el anclaje aprobado del puente. El acceso permanece estable durante el zoom. El collider canónico sigue la silueta en todas las distancias.

## Interfaz y sistemas

- Gestión independiente de los cinco edificios, sobre el único Canvas y la sesión existentes; referencia V identificada, producción, costes reales, requisitos y progreso pagado.
- Consejo IV con tres elecciones permanentes y recompensas exactas; ascenso V, cantera productiva y extracción regional fieles al vertical slice web.
- El cuartel enlaza con el entrenamiento existente. Se ocultan las tarjetas y etiquetas contextuales durante la gestión. La clasificación declara expresamente que usa perfiles simulados.
- Un edificio terminado en V muestra su estado completado, sin ofrecer visualmente un coste de VI. No existe una mejora a VI en este alcance.
- El presentador persistente heredado cede ante una escena que ya tiene un `SceneHost` propio. Esto elimina el segundo HUD que interceptaba toques pese a que el botón nuevo tenía una profundidad gráfica correcta. Las escenas heredadas recuperan su presentador al regresar.

## Evidencia nativa

| Comprobación | Evidencia |
|---|---|
| Toque real sobre Volver; HUD antiguo sin interceptar | PASS, 1/1, artefacto `12911069818` |
| Dominio IV–V y regresión III | PASS, 162 + 50 comprobaciones, ejecución `40906346835` |
| Recorrido IV–V, gasto único, guardado/recarga y sustituciones individuales | PASS, 386 comprobaciones, ejecución `40906347082` |
| Toque físico sobre la ciudad V y apoyo en terreno | PASS en zoom 32 y 280 |
| Capturas originales de ciudad, niveles, gestión y mundo | 70 PNG, artefacto `12913106042`, con cámaras y fuente compilada |
| EditMode completo | PASS, 54/54, diagnóstico `12913038382` |
| PlayMode completo | PASS, 73/73, diagnóstico `12913038382` |
| Recorrido I–II completo | PASS, ejecución `40906347082` |

La selección inicial de 270° uniforme resultó insuficiente y fue rechazada por el propietario. PR130 la sustituye por las orientaciones individuales y el anclaje real descritos en ORIENTATION_REWORK.md. También contrasta las capturas III/IV, los niveles de cada edificio, la gestión en horizontal/vertical y el mundo cercano/panorámico. El cambio de píxeles solo comprueba que hay un cambio visible; no constituye una aprobación artística por sí mismo.

Los fallos y métodos descartados están documentados en `MIGRATION.md`: etiquetas vacías, JPEG importadas como Cubemap, capa de mundo incorrecta, iluminación antigua, orientación y duplicación del HUD heredado. La prueba táctil se ejecuta primero para evitar repetir una suite completa cuando esa interacción ya falla.

No se han publicado APK/WebGL ni gastado créditos. Esta validación no mide FPS, memoria o temperatura en un móvil físico, ni certifica una nota artística global del mundo. Los originales y editables de las entregas anteriores siguen conservados.

Artefacto nativo completo: `12913871649`, SHA-256 `0d2ceb5f8a24f51ebfc791be5d66a6f698e312645d099a618c028e729a0b7433`. La certificación general adicional se consulta en los checks de PR129 y debe pasar antes de integrar. El snapshot de ejecución ya no mantiene una reserva pesada al concluir; el mismo propietario conserva la escritura hasta el cierre.

La certificación general distingue los destinos opacos de la ciudad expuesta. Comprueba geometría real de pantalla completa, opacidad, bloqueo y controles activos para construcción/ascenso/entrenamiento/marchas; conserva las exigencias originales de encuadre y área de ciudad libre para las vistas de mundo pertinentes. Ver el diagnóstico del control40911481759 en `MIGRATION.md`.

## Cierre de integración

PR129 integrada en `b7aefa07f9897fd8fe7c43f9bc3b79e4df351d5e`. Su árbol Git `33c632bb2399ff1a98a25d4dd67f8ad4ae7dd7f0` coincide exactamente con el certificado en `9639d946aafbb724f75cc50f6978007106b1c719`, no solo con una rama parecida.

Control general `40914641659` SUCCESS:54/54 EditMode,73/73 PlayMode,44 comprobaciones I–II, tres marchas simultáneas, Aldric y12 vistas, siete destinos de gestión certificados y las ganancias originales de área libre de ciudad. Artefacto general `12916382095` (SHA-256 `769370db9ef64f55b7cbb49d19ed893a40c53200826cb69a86f8b1226bd2274d`); Aldric compartido `12917245613`; tres marchas `12914019994`. Tester web `40914641605` SUCCESS, incluido Arco I completo desde partida nueva.

Se libera la asignación canónica al estado disponible y se conserva el mismo ID, historial y fuentes. El cierre solo actualiza documentación/registro; no modifica los archivos ejecutables ya certificados ni publica una versión distribuida.
