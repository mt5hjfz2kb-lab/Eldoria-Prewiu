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

### Preparación de pruebas PlayMode — 27/10/2026

Se añadió `Unity/Assets/Eldoria/Tests/PlayMode/AldricDialogueBustPresentationTests.cs` y `.meta` al ámbito exclusivo de PR118, registrados en `pipeline/active-workstreams.json` de main. El test sobre el **RoyalHUD y la escena integrados** (bajo guardia `VALORIA_GAMEPLAY_PROBE=1` de guardado descartable) cubre:

- Carga real de las 4 texturas 512×512 desde Unity Resources y comparación del PNG neutral con el `RawImage` que dibuja el diálogo.
- Cinco pruebas de speaker solo de interfaz: Sir Aldric, Lyra, Orín, NARRADOR y Aldric. Solo Aldric recibe imagen; el resto usan sigilo, sin retocar diálogos canónicos.
- Comprueba tamaño mínimo de busto, región UV, disponibilidad del botón real Continuar y finalización narrativa por el mismo botón.
- `RoyalGoldV3CinematicFinish.cs` usa el sigilo como respaldo solo si tampoco existe el retrato antiguo y falta el PNG nuevo.

**Es un test escrito, NO una prueba ejecutada ni evidencia de captura.** El equipo Windows sigue reclamado por `valoria-master-plan-layered-construction-20261016` para Bastión III. El workflow [UI Progression Certification](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40577952133) falla antes de Unity por esta reserva; no falsear test PASS. El nuevo test no altera guardianías ni escenas. Después de la liberación oficial ejecutar pruebas y obtener capturas 16:9/vertical en el juego antes de solicitar el merge.

### Corrección artística del propietario: solo hombros y cabeza

El propietario revisó el primer retrato y observó correctamente que mostraba demasiado cuerpo. **Se elimina visualmente todo el torso restante del diálogo:** rostro con presencia dominante, cuello azul y solo remate de ambas hombreras. El render proviene del **mismo GLB de capa azul aprobado**, sin reesculpir, regenerar ni gastar Tripo. La modificación se aplica al generador canónico `tools/aldric-dialogue/render_existing_model_busts.py` como **recorte físico de los píxeles RGBA originales** desde `(138,16)` hasta `(374,252)` sobre el render 512×512, redimensionado a salida 512×512 con Lanczos, conservando alfa. No manipular escala de todo el personaje en Unity ni sustituirlo por un retrato inventado. El modelo 3D completo no cambia.

El flujo aislado [GitHub Actions run 40582415092](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40582415092) terminó `SUCCESS` y **versionó los cuatro PNG corregidos** directamente en `Unity/Assets/Eldoria/Resources/UI/AldricDialogue/`. El manifiesto `ALDRIC_BUST_PROVENANCE.json` registra `crop_box_512:[138,16,374,252]`, `body_framing: close head and shoulders, NO torso` y origen SHA256 aprobado `b7b91923f1e8459a8327c98a83afc744cad44813267f92c59640cf755aecb555`. El RawImage de la PR118 sigue apuntando a las mismas rutas Unity Resources, de modo que cargará la versión nueva sin cambiar diálogos, eventos ni guardados.

**Pendiente:** validación nativa Unity horizontal/vertical y prueba real de avanzar los diálogos bajo la autoridad del workstream Bastión III. El éxito de render y guardado de imágenes no constituye prueba de ejecución en el juego. PR118 permanece DRAFT.

## Certificación final Unity nativa — 28/10/2026

**Prueba real sobre el merge exacto de PR #118 + main:** [GitHub Actions native run 40600968661](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40600968661), **SUCCESS**, una sola reserva Windows `windows-runner-heavy` del trabajo canónico. Se prepararon primero los recursos originales de Bastión III en el checkout aislado sin modificar main ni guardados.

- **Unity 6000.3.23f1 PlayMode:** **2/2 PASS, 0 FAIL, 0 SKIP**, con tests `AldricDialogueBustPresentationTests`. Se resolvió un falso fallo anterior por reflexión: `RoyalHUD.Modal` es un campo público y no una propiedad.
- **Capturas reales Unity:** **12 PNG distintos**, 1600×900 y 900×1600, cinco speakers en español (`Sir Aldric`, `Aldric`, `Lyra`, `Orín`, `NARRADOR`) y Aldric inglés en ambas orientaciones. `ALDRIC_NATIVE_PASS.txt` validó ruta del sprite de referencia, nombre del speaker, ventana de pergamino, texto revelado, tamaño, original `Continuar` y su cierre sin cambiar el progreso.
- La comprobación cromática se corrigió para **distinguir el pergamino violeta auténtico de Orín** del pergamino cálido de los demás. En la captura de Orín no aparece erróneamente la cara de Aldric; Lyra/narrador conservan sus respectivos sigilos. Se inspeccionaron visualmente capturas de Aldric horizontal/vertical, Lyra, Orín y narrador: busto solo cabeza/hombros, sin cuerpo entero y con texto visible.
- Arte de origen: GLB Tripo/Blender aprobado con capa azul SHA256 `b7b91923f1e8459a8327c98a83afc744cad44813267f92c59640cf755aecb555`. **0 créditos adicionales Tripo**. No se han añadido otros retratos 3D ni una cámara extra en el HUD.
- Evidencia descargable exacta del run: [artifact aldric-pr118-unity-actual-40600968661](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40600968661/artifacts/12778721828), que conserva XML, logs, 12 PNG y recibo de aceptación. SHA de la rama fuente ensayada: `ce8447e10827b8c88775ddced8af820fef228ce6`. Al consolidar **solo este documento** después de la prueba no cambia el código Unity ni los cuatro PNG ensayados.

**Alcance de la aceptación:** integración de retrato de Aldric en diálogos y escenas UI ES/EN con captura nativa. No certifica prueba física de Android. Las comprobaciones genéricas del tester web (`qa/e2e-v0301-ux-clarity.js`) tienen un timeout anterior ajeno al alcance de esta PR y el workflow de UI Progression está condicionado a una reserva distinta; la evidencia de aceptación pertinente para este trabajo es la prueba nativa dedicada. No confundir `PR MERGED` con `APK PUBLICADA`: no se ha solicitado ni realizado otra publicación familiar para este cambio.
