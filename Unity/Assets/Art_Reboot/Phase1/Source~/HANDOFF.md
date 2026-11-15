# Visual reboot — Fase 1, relevo 15 noviembre 2026

## Identidad y límites

Continuar únicamente `art/visual-reboot`, fuente recuperada `855af6dd4ea7c37318796847a71778377fd513d8`.
Encargo: piloto medieval crepuscular de realismo estilizado, tres estados
(intacto / ruina / corrupción), atlas 256² y colores por vértice, cámara 45–55°.
No es `experiment/ashen-citadel-i-v-20261115` (otro chat, cinco niveles jugables),
ni PR196. No cambiar main, mecánicas, guardados, escenas originales o V4.1.
La continuación expresa del propietario autoriza estas ediciones aisladas.
No se ha creado una reserva canónica ficticia ni utilizado el PC compartido.

## Recuperado y comprobado

El commit inicial contiene mallas, atlas, Blender, FBX, GLB, Shader Graph y una
copia de la escena con objetos originales desactivados. NO contenía el montaje
Unity anunciado por el menú del guard: sólo existía la protección de Play/build.

En este relevo se añadió `Editor/RebootPilotCapture.cs` y una assembly Editor-only
que sólo referencia Core/URP. Monta el FBX con dos materiales del Shader Graph,
valida importación/atributos/topología, añade cámara a 50°, luz cálida, ambiente
frío, niebla lineal, Bloom contenido y ACES; guarda sólo la escena experimental.
Prepara PNG auténticos URP a 1100×1250, 390×844, 1600×900 y 844×390.
Restaura las escenas abiertas y verifica que la escena original conserve sus bytes.
Rechaza escenas inicialmente sucias, fallos Shader Graph, falta de UV/color,
render plano, píxeles magenta y asignación GPU fallida. No entra en Play.

Comprobaciones ejecutadas en Linux, Blender 4.3.2:
- `python Unity/Assets/Art_Reboot/Phase1/Source~/verify_source.py`: PASS.
- `blender -b -t 2 --python Unity/Assets/Art_Reboot/Phase1/Source~/verify_fbx.py`:
  PASS; cuatro mallas, 21.308 triángulos, colores presentes, cada cara contenida
  en una casilla del atlas. Reimportación real, sin reescribir el arte.
- La escena original coincide byte a byte con la base indicada en verify_source.

El render heredado `Evidence/blender_preview.png` fue inspeccionado en este
relevo. Se distinguen los tres estados y la jerarquía central, pero los pinos
siguen muy facetados, las piedras/caminos repetitivos y la materialidad plana.
No se afirma calidad final ni aprobación artística. Antes de retocar más,
comprobar cuánto de esa lectura se mantiene en el motor real.

## Próximo paso preciso y bloqueos

1. Releer registro vivo en main y cola del runner. En la lectura de este relevo,
   `eldoria-ashen-citadel-original-i-v-20261115` reserva Windows. Su run
   43766002991 figura success, pero eso NO libera su titularidad de escritura
   ni su reserva. No cancelar ni apropiarse de ella.
2. `art/visual-reboot` aún no tiene entrada en el registro de main. El requisito
   de registrarlo entra en conflicto con la prohibición específica de tocar main.
   No escribir allí ni abrir otra PR/workstream para sortear el conflicto.
   Se requiere resolver la excepción de coordinación antes de usar el runner.
3. Con reserva legítima y checkout completo, abrir Unity 6000.3.23f1/URP17.3,
   compilar y ejecutar `Art Reboot/Build and capture phase 1` o el método
   `Eldoria.ArtReboot.Editor.RebootPilotCapture.BuildAndCapture` en Edit Mode,
   sin `-nographics`. Conserva las PNG en `artifacts/visual-reboot/<hora>` fuera
   de Assets; no publicar ni confundirlas con el render Blender.
4. Revisar encuadre, sombras, emisión violeta, compatibilidad ShaderGraph y
   legibilidad móvil. La configuración de luz propuesta NO ha sido vista aún
   en Unity. Registrar SHA fuente real al ejecutar, junto a PNG y logs.
5. ParticleSystem clásico pendiente: el manifest recuperado NO incluye
   `com.unity.modules.particlesystem`. No se añadió referencia que rompa la
   compilación ni se sustituyeron silenciosamente partículas por otro sistema.
6. Play/WebGL/Android y rendimiento pendientes. El guard se conserva: el
   bootstrap global llama a V4.1 aunque se desactiven los objetos de la escena.
   No quitarlo ni prometer una build jugable a partir de una captura Editor.

No ART_APPROVED, no UNITY_COMPILED, no UNITY_RENDERED, no MOBILE_TESTED.
No merge, publicación, gasto, ni cambios sobre el otro experimento.

API de captura consultada: Unity 6 `RenderPipeline.StandardRequest` y
`RenderPipeline.SubmitRenderRequest`:
https://docs.unity.com/en-us/engine/6000.0/manual/cameras/urp/multiple/user-render-requests
