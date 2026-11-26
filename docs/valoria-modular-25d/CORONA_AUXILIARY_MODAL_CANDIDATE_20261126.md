# Corona — cierre de componentes auxiliares (candidato aislado)
Base 91b8de9874aed87219d733efa6e8e0aa56207f1e. La revisión fijada de ese SHA sigue en curso (Unity 45802472999, auditoría 45802703502, WebKit 45803056673). No mover la rama experimental mientras esas evidencias dependen del SHA.
## Defecto comprobado por lectura de código
RoyalHUD.ShowModal sirve ajustes, confirmación de reinicio de pruebas, ejército y otros cuadros. ModalWindow V1 y RoyalButton siguen heredando la tipografía de origen y su feedback anterior. RoyalGoldV3CinematicFinish sobreescribe la tipografía con reference-serif cada render, aunque Corona ya tiene Cormorant Garamond y Source Sans 3. El contrato compartido no cubría esos consumidores.
## Implementación
CoronaFinish.ApplyModalTypography aplica fuentes por rol y feedback a los tres botones existentes. Se llama una vez desde EnsureView en Corona. Los avisos de Feedback usan Body. En CinematicFinish se usan las fuentes Corona sólo para su build, conservando exactamente la composición de manuscrito, los retratos aprobados, los colores narrativos, la lógica de revelado y la medición de altura existente. No se cambia RoyalHUD.ShowModal, ModalWindow.Show/Hide, el idioma, el reinicio, la ruta ni callbacks. No se altera el arte de las placas sin inspección de resultados.
## Pruebas
Añadida prueba EditMode: título/cuerpo/acciones reciben sus fuentes, se conservan texto ES/EN, tamaño de rectángulo, targetGraphic, estado bloqueado/oculto y listener. No ejecutada en este SHA.
## Criterio de promoción
Primero recibir y auditar capturas de la revisión fijada. Después comparar diálogos, ajustes ES/EN, confirmaciones y avisos en el candidato aislado. Capturar texto completo tras revelado, portrait y landscape, sin desbordamientos ni pérdidas de interacción. No ejecutar reinicio destructivo salvo en un guardado QA desechable. No se aprueba puntuación artística ni se certifica móvil real. No se fusiona Main.
