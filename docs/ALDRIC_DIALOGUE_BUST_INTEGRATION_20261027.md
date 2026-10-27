# ELDORIA — Sir Aldric | Retrato de busto en diálogos (27/10/2026)

## Alcance y estado
Propietario aprueba la **identidad 3D de Aldric con capa azul** y solicita que en los diálogos NO se muestre el personaje entero, sino **hombros/pecho hacia arriba**, correctamente encuadrado en móviles. Flujo cero crédito y sin modificar progreso.

**Estado de esta rama:** código de presentación preparado; capturas/GLB locales auténticos revisables por el propietario. Integración Unity nativa y verificación visual en ejecución CI aislada **no se consideran realizadas** hasta capturas y test del run exacto. No fusionar a main ni publicar APK solo por un PASS de render en Ubuntu. Bastión III y PR117 siguen con su propietario.

## Comportamiento
- Usa **el mismo** `RoyalHUD / ModalWindow / Canonical Aldric portrait` (RawImage), no crea otro diálogo, EventSystem, cámara o controlador narrativo.
- `RoyalGoldV3CinematicFinish` carga `Resources/UI/AldricDialogue/aldric_neutral` solo cuando `NarrativeSpeaker == 'Sir Aldric'`. Si el recurso no está o falla, conserva el JPEG original. Los otros personajes siguen con su sigilo actual.
- Textos, ES/EN, reveal/continue, callbacks, tamaño de papel, zona segura, economía, guardados y progreso no se alteran.
- Cuatro archivos (`aldric_neutral`, `aldric_firme`, `aldric_preocupado`, `aldric_sereno`) son **cuatro iluminaciones/ángulos** desde una sola identidad 3D. **NO** son cuatro expresiones faciales distintas porque el modelo actual carece de blendshapes de rostro probadas.
- La API pública `SetAldricPortraitMood(int)` permite que la narrativa explícitamente authored seleccione 0–3; nueva intervención reinicia a neutro. No inferir emociones del texto.
- Producción móvil: PNG RGBA 512px, UI no precisa una segunda cámara ni renderizado 3D en tiempo real.

## Evidencias y procedencia
- Fuente Tripo exacta, **una única llamada existente** H3.1 / task `f827067d-927e-4172-9bb8-6ee813176d58`, 20 créditos ya gastados. No hacer otra llamada.
- GLB optimizado usado en la automatización: SHA256 `b12265ec907781443520b739a861e39d6860903d1dd273fdc0cd952ce4c71595`, artifact GH 12746169980 run 40527678020.
- **Limitación artística pendiente:** la automatización CI usa la **malla Tripo optimizada anterior al art pass de capa azul** porque la entrega de capa aprobada sigue fuera del repositorio (GLB local `SirAldric_ArtPass_CapaAzul_v1.glb`, SHA256 `b7b91923f1e8459a8327c98a83afc744cad44813267f92c59640cf755aecb555`). Esto conserva el mismo rostro y armadura, pero **NO certifica equivalencia exacta de capa/vestuario**. Para aprobación final comparar el PNG CI con renders del GLB aprobado y, si es necesario, utilizar el ZIP de preproducción del chat original para producir los PNG finales.
- El usuario recibió renders auténticos de dicho GLB aprobado en `/mnt/data/aldric_dialogos_busto/` y ZIP `Eldoria_Aldric_Retratos_Dialogos_Busto_20261027.zip`; esos no están automáticamente instalados en Unity por existir en el chat.
- `.github/workflows/aldric-dialogue-portraits.yml` reconstruye cuatro imágenes únicamente desde el GLB certificado en un runner GitHub Ubuntu; **nunca** usa Tripo, Unity Windows ni toca Bastión III.

## Puertas de aceptación
1. PNGs cargan en la UI y el busto es de pecho/hombros hacia arriba; ningún cuerpo entero ni fondo negro (alfa real).
2. Comparación de fisonomía y armadura con **Aldric aprobado** (capa azul si visible). No reinterpretar colores/edad/rostro sin autorización.
3. Capturas Unity reales de diálogos con Sir Aldric en 16:9 y retrato móvil; 218 caracteres caben y CONTINUAR funciona.
4. Cambio Sir Aldric → Orín/Lyra/narrador no reutiliza su imagen; el sigilo específico sigue correcto.
5. Texto/revelado/avance ES/EN y estado/guardados sin cambios; QA funcional I–III y regresión al cierre de PR.
6. No se publica APK ni se fusiona rama hasta cumplir pruebas y revisión artística con imágenes **reales Unity**.
