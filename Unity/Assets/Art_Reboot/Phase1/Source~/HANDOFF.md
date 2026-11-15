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
2. El propietario autorizó la coordinación. Entrada canónica registrada en main,
   mismo ID `eldoria-art-visual-reboot-phase1-20261115`, revisión 2; descriptor
   de producción artística registrado como metadatos de coordinación, sin arte
   ni código del juego en main. Scope de workflow nativo aislado incluido.
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

## Continuación autónoma — preparación nativa completa

- Workflow `.github/workflows/art-visual-reboot-native.yml`: preparado; Sin solicitud de ejecución nativa. Los pushes ordinarios sólo ejecutan
  preflight Ubuntu, que analiza PowerShell
  y verifica propietario, rama y SHA exacto antes de siquiera encolar Windows.
  El job Windows repite la comprobación viva inmediatamente antes de Unity.
- `Source~/native_capture.ps1`: copia sólo Art_Reboot, escena original intacta
  para comparación, configuración URP y ProjectSettings a RUNNER_TEMP propio.
  No copia Domain/Application/Infrastructure/UI ni ejecuta V4.1. Es un laboratorio
  de importación visual, NO certificación de integración en el juego completo.
  Los objetos históricos desactivados pueden tener referencias no disponibles
  en este laboratorio; no afirmar lo contrario ni usarlo como build jugable.
- El renderer existente carece de postProcessData. PreparePilotPipeline ahora
  copia renderer/URP dentro de Art_Reboot, enlaza el PostProcessData del paquete,
  asigna temporalmente la copia y restaura la selección de pipeline al terminar.
  No se modifica el recurso original. La mejora visual aún NO se ha observado.
- Se conservan logs del run, cuatro PNG, hashes, escena y assets importados.
  El script se niega a tocar un proyecto previo o procesos ajenos; sólo detiene
  por tiempo excedido su propio proceso y conserva diagnóstico si falla.
- Verificados: 8/8 tests de rechazo de reservas indebidas/candidatos obsoletos,
  parser C# sin errores, YAML válido, plan artístico canónico PASS,
  verify_source PASS. PowerShell analizado realmente en Ubuntu/GitHub Actions run 43768175996
  sobre acafc10c6b4d699c31a8d36201fe89dbb3901da3: PASS. Sus 8 tests
  también pasan allí; jobs lease/native omitidos. No es compilación C#/Unity.
- Última lectura: Ashen Citadel todavía mantiene Windows y su run nativo
  43767889061 estaba in_progress. No se solicitó cancelación ni se usurpó reserva.
- Alternativas comprobadas: este Linux tiene Blender 4.3.2, pero no Unity ni
  dotnet/pwsh; el conector disponible no proporciona una segunda máquina Unity.
  Blender sirve para fuente/UV, no para sustituir las imágenes URP solicitadas.

Siguiente acción cuando la reserva quede libre: reclamar sólo Windows mediante
CAS del registro, crear el commit con marcador explícito [visual-reboot-native] en el mensaje, fijar native_source_sha
al SHA real de ese candidato ANTES de actualizar su ref, y dejar que el workflow
verifique esa coincidencia dos veces. No cambia la autorización ya concedida.

Los pushes sin el marcador explícito sólo ejecutan análisis PowerShell y tests
de reserva en Ubuntu; el job Windows se omite, aunque exista una reserva.

Evidencia remota del código exacto: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43768175996
