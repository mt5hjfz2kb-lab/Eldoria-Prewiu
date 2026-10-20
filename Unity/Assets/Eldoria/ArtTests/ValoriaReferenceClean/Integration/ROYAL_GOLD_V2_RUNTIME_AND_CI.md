# Royal Gold V2 — integración y pruebas sin duplicar el trabajo artístico

La escena canónica sigue siendo `Valoria_City_World_UI.unity`. El componente
`ValoriaIntegratedWorld` instancia un único `RoyalHUD` desde el prefab de
producción, y añade la presentación V2 en tiempo de ejecución; NO crea otro
`Canvas` principal, `Camera`, `EventSystem`, partida ni `ICommandGateway`.

Los controles V2 son botones uGUI auténticos. El HUD V1 conserva únicamente
los adaptadores de sesión/reglas/comandos; su vista de misión y su ficha
de edificio se ocultan, y las acciones V2 delegan en los comandos
existentes. `ValoriaPlaytestTelemetry` debe medir el rectángulo visible de
V2, nunca el botón V1 oculto, para las pruebas Chromium automatizadas.

Estado probado: Unity nativo 39424591466 SUCCESS, con progresión,
diálogos, encuadre, navegación y persistencia Bastión I-II. La aprobación
visual final y el éxito del WebGL real son independientes; no confundir
compilación técnica con `ART_APPROVED` ni con prueba de iPhone físico.

La captura/render del escenario y los recursos de arte no se modifican
por este módulo. En Unity, las rutas de parcelas pueden conservar hotspots
lógicos sin reintroducir las cuatro parcelas visibles revocadas por el
propietario. No instalar fuentes ni sistemas de interfaz alternativos.

**Cadena de validación apropiada:** la compilación de
`tools/valoria-reference-clean/request.json` con `stage=web_playtest`
usa el workflow nativo exclusivo, en el que el guardado de modelos/escenas
se omite porque ese paso NO ha autorado cambios artísticos. El flujo
`unity-slice` sigue haciendo su análisis de código, pero no debe
compilar una segunda instancia Windows cuando la misma confirmación incluye
el request y el marcador explícito `[valoria-native]`. Los cambios de UI
no acompañados de ese request sí mantienen su comprobación general.

No publicar ni sustituir la versión web estable hasta contar con
reproducción real en navegador y guardados comprobados del artefacto exacto.
