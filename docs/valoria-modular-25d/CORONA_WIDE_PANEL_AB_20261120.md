# Corona de la Brecha — panel de obsidiana A/B con corona intacta

**Fecha:** 20/11/2026. Rama exclusiva de investigación artística `experiment/corona-wide-panel-art-ab-20261120`, derivada de `935612805effdf15cb50933aa4e005d943305a19`. La rama principal del experimento continúa sus pruebas nativas; no se la modifica.

## Causa visual

En la aplicación original, `panel_obsidian.png` (500×540 RGBA) se muestra como `Image.Type.Simple` en 355×244 puntos de UI. La diferencia de relación de aspecto, **0,926 → 1,455**, deforma la gema del centro y comprime las esquinas verticalmente. `Image.Type.Sliced` tampoco servía con estas ilustraciones porque la corona cae justo en la zona de estiramiento.

## Método implementado

Un algoritmo de remapeo por cinco regiones en el horneador original `tools/valoria-corona-brecha/bake.py`, sólo para `panel_obsidian`. El original se normaliza a 463×500 y se expande a 720×500 preservando a escala original la gema central y las piezas laterales, y expandiendo de forma suave los dos tramos intermedios del borde ornamental. No se copian fotografías, no se reconstruye toda la ciudad ni se añade UI ficticia.

- Fuente conservada: `tools/valoria-corona-brecha/premium/panel_obsidian.png`.
- Salida: `Resources/ValoriaModular25D/UI/CoronaBrecha/panel_obsidian.png`, **mismo GUID SpriteImporter**, 720×500 RGBA. Los otros catorce recursos deben permanecer idénticos.
- Muestras A/B a resolución 355×244 con idénticos parámetros de reescalado, sin atribuir a Unity lo que son imágenes generadas por PIL.

## Evidencia y aceptación

El workflow `corona-wide-panel-ab.yml` hornea en Ubuntu, compara hashes de catorce fuentes intactas, prueba alpha/dimensiones y adjunta dos capturas del material a tamaño de UI. El primer método de bandas repetidas fue **rechazado** por generar costuras verticales evidentes; este remapeo continuo carece de esos empalmes.

El resultado no se integra a la rama principal de Corona hasta **comparación en Unity real** del panel con widgets de construcción, texto y hitboxes. No se modifica `LocalGateway`, edificios ni cámaras. Aprobación artística comercial e iPhone todavía pendientes.
