# ELDORIA — Sir Aldric | Retrato de busto en diálogos (27/10/2026)

## Alcance y estado
Propietario aprueba la **identidad 3D de Aldric con capa azul** y solicita que en los diálogos NO se muestre el personaje entero, sino **hombros/pecho hacia arriba**, correctamente encuadrado en móviles. Flujo cero crédito y sin modificar progreso.

**Estado de esta rama:** cuatro PNG de busto **reales del modelo azul aprobado**, versionados en esta rama tras verificar la receta de Blender y el GLB exacto. **Aún faltan la compilación y las capturas reales dentro de Unity**, por lo que no procede fusionar a main ni publicar una APK. Bastión III y PR117 continúan bajo su propietario.

## Comportamiento
- Usa **el mismo** `RoyalHUD / ModalWindow / Canonical Aldric portrait` (RawImage), no crea otro diálogo, EventSystem, cámara o controlador narrativo.
- `RoyalGoldV3CinematicFinish` muestra `Resources/UI/AldricDialogue/aldric_neutral` únicamente para los speakers `Sir Aldric` o `Aldric`; si falta se conserva su JPEG anterior. Para Lyra, Orín y narrador se oculta el retrato heredado de Aldric y queda el sigilo. Corrección meramente visual; el controlador RoyalHUD del juego no se modifica.
- Textos, ES/EN, reveal/continue, callbacks, tamaño de papel, zona segura, economía, guardados y progreso no se alteran.
- Cuatro archivos (`aldric_neutral`, `aldric_firme`, `aldric_preocupado`, `aldric_sereno`) son **cuatro iluminaciones/ángulos** desde una sola identidad 3D. **NO** son cuatro expresiones faciales distintas porque el modelo actual carece de blendshapes de rostro probadas.
- La API pública `SetAldricPortraitMood(int)` permite que la narrativa explícitamente authored seleccione 0–3; nueva intervención reinicia a neutro. No inferir emociones del texto.
- Producción móvil: PNG RGBA 512px, UI no precisa una segunda cámara ni renderizado 3D en tiempo real.

## Evidencias y procedencia
- Fuente Tripo exacta, **una única llamada existente** H3.1 / task `f827067d-927e-4172-9bb8-6ee813176d58`, 20 créditos ya gastados. No hacer otra llamada.
- GLB optimizado usado en la automatización: SHA256 `b12265ec907781443520b739a861e39d6860903d1dd273fdc0cd952ce4c71595`, artifact GH 12746169980 run 40527678020.
- **Fuente azul aprobada recuperada:** El workflow reconstruye primero el art-pass exacto desde el GLB optimizado más la receta preservada como `tools/aldric-dialogue/approved-artpass-source.zlib.b64`. Rechaza diferencias del script SHA256 `5715c18b8e559bd752cc291451da2c768b3b72bec19ca3d652b8d1b6ea877315` y el GLB azul aprobado SHA256 `b7b91923f1e8459a8327c98a83afc744cad44813267f92c59640cf755aecb555`. Run [40576565780](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40576565780) certificado. Cuatro PNG RGBA 512×512 y sus importadores Unity persistidos en la rama; **0 créditos nuevos Tripo**.
- El usuario recibió renders auténticos de dicho GLB aprobado en `/mnt/data/aldric_dialogos_busto/` y ZIP `Eldoria_Aldric_Retratos_Dialogos_Busto_20261027.zip`; esos no están automáticamente instalados en Unity por existir en el chat.
- `.github/workflows/aldric-dialogue-portraits.yml` obtiene la fuente original guardada en GitHub Actions, reconstruye el arte aprobado SHA-locked, genera PNG de busto transparente en Ubuntu y versiona en la rama aislada. No utiliza Tripo, Windows Unity ni el PR de Bastión III.

## Puertas de aceptación
1. PNGs cargan en la UI y el busto es de pecho/hombros hacia arriba; ningún cuerpo entero ni fondo negro (alfa real).
2. Comparación de fisonomía y armadura con **Aldric aprobado** (capa azul si visible). No reinterpretar colores/edad/rostro sin autorización.
3. Capturas Unity reales de diálogos con Sir Aldric en 16:9 y retrato móvil; 218 caracteres caben y CONTINUAR funciona.
4. Cambio Sir Aldric → Orín/Lyra/narrador no reutiliza su imagen; el sigilo específico sigue correcto.
5. Texto/revelado/avance ES/EN y estado/guardados sin cambios; QA funcional I–III y regresión al cierre de PR.
6. No se publica APK ni se fusiona rama hasta cumplir pruebas y revisión artística con imágenes **reales Unity**.
