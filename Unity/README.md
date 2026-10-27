> **19-10-2026 — Validación en recuperación:** usar la escena integrada de `pipeline/development-baseline.json`. El run 39274718168 restauró 10.212 archivos, pero `-projectPath=Unity` abrió la última copia del editor (`valoria-layered-source`) y sus XML no certifican el checkout solicitado. Se corrige a argumento separado con ruta absoluta entre comillas y normaliza los separadores Windows antes de exigir que los XML provengan de `Unity/Library/ScriptAssemblies` del checkout exacto. Mantener los resultados anteriores como diagnóstico, no como cierre.

# Eldoria en Unity — primera slice de Valoria

**Estado: primera slice compilada y jugada correctamente en Unity 6.3 LTS sobre Windows.** Es una línea Unity separada del producto web v0.32.0; esta carpeta no cambia la versión ni la URL del web. La versión del Editor está fijada en `ProjectSettings/ProjectVersion.txt`: **6000.3.23f1 (Unity 6.3 LTS)**.

## Abrir y jugar

1. Abre con Unity Hub y Unity Editor **6000.3.23f1** y una licencia activada. Para el player en tu PC se utiliza el módulo Windows Build Support (IL2CPP). Linux Build Support solo hace falta si vas a producir el player Linux localmente.
2. Desde Hub abre la carpeta `Unity/` del repositorio. Espera a que Package Manager importe URP, Input System, uGUI y Test Framework; revisa Console y confirma que no hay errores de compilación. Conserva el `Packages/packages-lock.json` que produzca una importación correcta en un commit posterior.
3. Ejecuta **Eldoria → Regenerate slice scenes and build settings**. Este comando configura URP y escribe las tres escenas versionadas. Abre `Assets/Eldoria/Scenes/Bootstrap.unity` y pulsa **Play**. En caso de URP sin renderer válido, crea un `Universal Renderer` y un `URP Pipeline Asset` desde el menú Assets y asígnalos a Graphics y Quality; registra el cambio porque el editor no ha validado la automatización de la configuración URP.
4. En Valoria, Aldric explica que faltan 80 de madera y tienes 30. Usa la puerta o **IR AL MUNDO**, toca el bosque o **BOSQUE · RECOLECTAR**, espera el regreso de la marcha (9 s), vuelve a Valoria y mejora el Aserradero (6 s). El explorador corrupto ofrece un encuentro PvE opcional; su resultado no adelanta Bastión II. Verifica que la estructura reparada y el Poder pasan de **2452 → 2622**. El texto de fin declara el cierre de esta slice; no es el final de Arco I.
5. Para generar el ejecutable de Windows, usa **Eldoria → Build Windows slice**. La salida es `Unity/Builds/Windows/Eldoria.exe` y su carpeta de datos; ambos son necesarios para jugar. El guardado local está en `Application.persistentDataPath/eldoria-unity-slice-v1.json`. Para iniciar una partida completamente nueva **cierra el player/Play y elimina solo ese archivo de save de esta slice**. Guarda una copia si quieres preservarlo. Un save web no se importa automáticamente.

## Nodo Unity automático en Windows

La instalación del nodo remoto se automatiza con `scripts/setup-unity-runner-windows.ps1`. Debe ejecutarse **una sola vez desde Windows PowerShell como administrador**. El script reutiliza el ZIP del runner ya descargado, crea una instalación limpia en `C:\\actions-runner-eldoria`, detecta Unity 6000.3.23f1, configura `UNITY_EDITOR_PATH` a nivel de máquina y registra el runner como servicio de Windows con la etiqueta `unity-6000-3-23f1`. El único dato manual que solicita es un token de registro nuevo generado por GitHub; no lo guarda.

Después de que el runner aparezca Online, define la variable de Actions `UNITY_RUNNER_READY=true`. A partir de ahí los cambios relevantes en `Unity/**` pueden activar automáticamente tests EditMode/PlayMode, build Windows y subida de logs/player como artifact.

El mismo workflow audita ahora, de forma no bloqueante, si el nodo tiene instalado **Unity Android Build Support**, si existe el `adb` incluido con el SDK y si hay algún dispositivo Android visible/autorizado. Este audit no certifica rendimiento móvil: solo determina si el runner está preparado para iniciar el perfilado físico definido en `../pipeline/mobile-visual-performance-gate.json`.

## Pruebas y build

Desde raíz del repositorio, `python3 scripts/check_unity_slice.py` solo comprueba la estructura de archivos. Para validación **real**, ejecuta en un equipo con Editor y licencia:

```bash
UNITY_EDITOR_PATH=/ruta/absoluta/al/Editor/Unity
"$UNITY_EDITOR_PATH" -batchmode -nographics -quit -projectPath "$PWD/Unity" -runTests -testPlatform EditMode -testResults "$PWD/editmode.xml" -logFile "$PWD/editmode.log"
"$UNITY_EDITOR_PATH" -batchmode -nographics -quit -projectPath "$PWD/Unity" -runTests -testPlatform PlayMode -testResults "$PWD/playmode.xml" -logFile "$PWD/playmode.log"
"$UNITY_EDITOR_PATH" -batchmode -nographics -quit -projectPath "$PWD/Unity" -executeMethod Eldoria.EditorTools.SceneSetup.BuildLinux -logFile "$PWD/build.log"
```

La última orden escribe `Unity/Builds/Linux/Eldoria.x86_64` y su carpeta de datos, fuera de git. Inspecciona XML, logs y el ejecutable, y realiza el recorrido manual de principio a fin. Para activar el job remoto `editor-tests-and-player`, el PC Windows del propietario debe registrarse una sola vez como **self-hosted GitHub Actions runner** con la etiqueta `unity-6000-3-23f1`, tener `UNITY_EDITOR_PATH` apuntando al Unity 6000.3.23f1 instalado y una licencia activada, y el repositorio debe definir `UNITY_RUNNER_READY=true`. Desde ese momento, cada push relevante a `main` puede ejecutar automáticamente EditMode, PlayMode y una build Windows y subir logs/build como artefacto. Hasta completar ese registro único, el workflow solo ejecuta el preflight y no genera build.

## Qué contiene el corte

| Funcional | Provisional | Pendiente de validar en Editor/dispositivo |
| --- | --- | --- |
| Comandos versionados/idempotentes; estado de recursos, construcción y marcha; Poder derivado; pronóstico/encuentro PvE; regreso y premio; guardado versionado; escenas/controles fuente. | Geometría modular por primitivas, Aldric/arquero silueta, bosque, Brecha pulsante, UI uGUI española, estadísticas de amenaza y economía del corte. | Tests EditMode/PlayMode automatizados, build Linux, sensibilidad/safe area móvil, arte/audio/animación y playtest humano ampliado. |

El cliente local es autoridad **solo de esta slice**; `ICommandGateway` centraliza órdenes, el estado tiene IDs y revisiones, y economía y combate se calculan fuera de MonoBehaviour. Ninguna garantía de antifraude u online es implícita. El contrato para Poder y Marcha está en [`../UNITY_CORE_CONTRACT.md`](../UNITY_CORE_CONTRACT.md). La ruta funcional/visual objetivo está en [`../UNITY_VERTICAL_SLICE.md`](../UNITY_VERTICAL_SLICE.md). Los assets actuales son originales de código y **bloqueo**, no candidatos de producción; consultar [`../UNITY_ASSET_AUDIT.md`](../UNITY_ASSET_AUDIT.md).

## Validación Windows real

El 2026-09-25 el propietario abrió el proyecto con Unity 6.3.23f1 en Windows, salió de Safe Mode tras corregir los cuatro errores iniciales, regeneró las escenas y completó la slice Valoria → Frontera → recolección → regreso → Aserradero. El Poder pasó de 2452 a 2622 y el guardado persistió entre Editor y player.

La primera build Windows en `Unity/Builds/Windows/` compiló y arrancó. Un fallo magenta del player se corrigió asegurando un material URP base incluido en Resources; la build posterior renderizó correctamente y la interacción de mundo/recolección funcionó fuera del Editor.

El PC Windows del propietario será el nodo Unity de Eldoria: GitHub → PC → Unity → compilación/tests/build → logs/resultados. La automatización se implantará progresivamente cuando compilación, tests y builds sean reproducibles, con el objetivo de dirigir el desarrollo principalmente desde móvil y minimizar intervención física.


## Recuperación del lanzador EditMode/PlayMode (2026-10-19)

En Windows self-hosted, el gate de `.github/workflows/unity-slice.yml` ejecuta Unity a través de PowerShell `Start-Process`. La ejecución 39251384434 falló antes de las pruebas porque `-projectPath` se interpretó como una ruta absoluta duplicada (error `Couldn't set project path`). El workflow se corrigió en 5548bd93c110f8fa223892335d876f0de4a7e6c3 para iniciar Unity con `-WorkingDirectory $env:GITHUB_WORKSPACE` y `-projectPath Unity`, sin modificar ni recompilar el juego.

Este cambio documental solicita una ejecución real de los gates existentes al afectar `Unity/**`; el resultado del nuevo workflow, y no este texto, determina si EditMode, PlayMode y Windows Player funcionan. La certificación funcional específica Bastión I–II/Chrome con reloj normal de 39253871138 se reutiliza mientras los bytes del WebGL no cambien. No publicar el artefacto de prueba automáticamente, ni tratar el éxito técnico como aprobación artística.


### Diagnóstico del rechazo de ruta en Windows (segunda comprobación)

La ejecución `39263683526` confirmó que el proceso Unity.exe arranca con el directorio relativo `Unity`, pero la API del Editor todavía devuelve `Couldn't set project path to` **antes de entrar en EditMode**. El siguiente gate no repetirá sin observación la misma hipótesis: verificará que `Unity/Assets`, `Unity/Packages/manifest.json` y `Unity/ProjectSettings/ProjectVersion.txt` están presentes en el checkout Windows, que el proyecto admite una escritura temporal y que no existe otro proceso Unity ocupando el PC. También comprobará la forma atómica `-projectPath=Unity` recomendada en incidencias históricas de invocación automatizada. La continuación de CI solo es válida cuando esas comprobaciones y las pruebas EditMode/PlayMode aporten evidencia real; ninguna recompilación WebGL está solicitada por este cambio.


### Recuperación exacta de Unity/Assets en Windows CI (19-10-2026)

El diagnóstico `39266054859` demostró que el checkout del workflow genérico no contenía `Unity/Assets` en `GITHUB_WORKSPACE`, aunque GitHub conserva los recursos del mismo commit. El workflow ahora verifica que el directorio de Git y el workspace coinciden y, únicamente cuando faltan carpetas rastreadas, ejecuta una restauración explícita de `Unity/Assets`, `Unity/Packages` y `Unity/ProjectSettings` **desde el SHA exacto**. Si faltan después, el gate falla con causa concreta antes de Unity. No modifica `Unity/Library`, `valoria-layered-source`, assets del juego ni guardados. La recuperación no equivale a pruebas aprobadas; el siguiente resultado Windows decidirá.


### Native gate checkout recovery — 2026-10-27

The Windows source/editor gate uses the same verified `valoria-layered-source` checkout as the native integrated-scene workflow. Run40430426425 failed before Unity because the old workspace-root sparse checkout lacked Git objects and could not restore tracked files. That old directory is preserved, not cleaned or reset. The gate still verifies the exact commit, tracked source, unexpected compilation inputs, test assembly paths, all EditMode/PlayMode results and the Windows build. Project, script and artifact paths now resolve consistently under the canonical checkout; the single labelled Windows runner serializes native use. No game assets or runtime behavior are changed by this recovery.

### Bastion III full editor suite resource parity — 2026-10-27

The shared Windows source-only EditMode/PlayMode gate must **author the same zero-cost
PrivateFissure** and **bake the real Granary + StockCrate/StockSack/StockBarrel**
using `BastionIIIArtBake.Build` *before* entering the 66-case PlayMode suite.
Unlike the isolated Bastion III native run and signed Android build, the legacy
generic gate previously skipped that preparation; 27 tests then failed from
`BASTION_III_MISSING_ART` log errors at scene initialization rather than
gameplay assertions. The repaired gate is fail-closed on missing PBR assets,
not a suppression of log errors or relaxation of tests. Full Unity validation
is requested explicitly by commit marker `[unity-editor-cert]`; do not
self-trigger the heavy Windows gate from edits to its own workflow file.
