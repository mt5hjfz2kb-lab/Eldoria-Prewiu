# Eldoria en Unity — primera slice de Valoria

**Estado: primera slice compilada y jugada correctamente en Unity 6.3 LTS sobre Windows.** Es una línea Unity separada del producto web v0.32.0; esta carpeta no cambia la versión ni la URL del web. La versión del Editor está fijada en `ProjectSettings/ProjectVersion.txt`: **6000.3.23f1 (Unity 6.3 LTS)**.

## Abrir y jugar

1. Abre con Unity Hub y Unity Editor **6000.3.23f1** y una licencia activada. Para el player en tu PC se utiliza el módulo Windows Build Support (IL2CPP). Linux Build Support solo hace falta si vas a producir el player Linux localmente.
2. Desde Hub abre la carpeta `Unity/` del repositorio. Espera a que Package Manager importe URP, Input System, uGUI y Test Framework; revisa Console y confirma que no hay errores de compilación. Conserva el `Packages/packages-lock.json` que produzca una importación correcta en un commit posterior.
3. Ejecuta **Eldoria → Regenerate slice scenes and build settings**. Este comando configura URP y escribe las tres escenas versionadas. Abre `Assets/Eldoria/Scenes/Bootstrap.unity` y pulsa **Play**. En caso de URP sin renderer válido, crea un `Universal Renderer` y un `URP Pipeline Asset` desde el menú Assets y asígnalos a Graphics y Quality; registra el cambio porque el editor no ha validado la automatización de la configuración URP.
4. En Valoria, Aldric explica que faltan 80 de madera y tienes 30. Usa la puerta o **IR AL MUNDO**, toca el bosque o **BOSQUE · RECOLECTAR**, espera el regreso de la marcha (9 s), vuelve a Valoria y mejora el Aserradero (6 s). El explorador corrupto ofrece un encuentro PvE opcional; su resultado no adelanta Bastión II. Verifica que la estructura reparada y el Poder pasan de **2452 → 2622**. El texto de fin declara el cierre de esta slice; no es el final de Arco I.
5. Para generar el ejecutable de Windows, usa **Eldoria → Build Windows slice**. La salida es `Unity/Builds/Windows/Eldoria.exe` y su carpeta de datos; ambos son necesarios para jugar. El guardado local está en `Application.persistentDataPath/eldoria-unity-slice-v1.json`. Para iniciar una partida completamente nueva **cierra el player/Play y elimina solo ese archivo de save de esta slice**. Guarda una copia si quieres preservarlo. Un save web no se importa automáticamente.

## Pruebas y build

Desde raíz del repositorio, `python3 scripts/check_unity_slice.py` solo comprueba la estructura de archivos. Para validación **real**, ejecuta en un equipo con Editor y licencia:

```bash
UNITY_EDITOR_PATH=/ruta/absoluta/al/Editor/Unity
"$UNITY_EDITOR_PATH" -batchmode -nographics -quit -projectPath "$PWD/Unity" -runTests -testPlatform EditMode -testResults "$PWD/editmode.xml" -logFile "$PWD/editmode.log"
"$UNITY_EDITOR_PATH" -batchmode -nographics -quit -projectPath "$PWD/Unity" -runTests -testPlatform PlayMode -testResults "$PWD/playmode.xml" -logFile "$PWD/playmode.log"
"$UNITY_EDITOR_PATH" -batchmode -nographics -quit -projectPath "$PWD/Unity" -executeMethod Eldoria.EditorTools.SceneSetup.BuildLinux -logFile "$PWD/build.log"
```

La última orden escribe `Unity/Builds/Linux/Eldoria.x86_64` y su carpeta de datos, fuera de git. Inspecciona XML, logs y el ejecutable, y realiza el recorrido manual de principio a fin. Para activar el job remoto `editor-tests-and-player`, configura un runner Linux con etiqueta `unity-6000-3-23f1`, variable `UNITY_EDITOR_PATH` y Editor/licencia activados, y establece la variable del repositorio `UNITY_RUNNER_READY=true`. Sin ello el workflow solo ejecuta el preflight y **no genera build**. El runner no debe publicar una build como estable sin revisión.

## Qué contiene el corte

| Funcional | Provisional | Pendiente de validar en Editor/dispositivo |
| --- | --- | --- |
| Comandos versionados/idempotentes; estado de recursos, construcción y marcha; Poder derivado; pronóstico/encuentro PvE; regreso y premio; guardado versionado; escenas/controles fuente. | Geometría modular por primitivas, Aldric/arquero silueta, bosque, Brecha pulsante, UI uGUI española, estadísticas de amenaza y economía del corte. | Tests EditMode/PlayMode automatizados, build Linux, sensibilidad/safe area móvil, arte/audio/animación y playtest humano ampliado. |

El cliente local es autoridad **solo de esta slice**; `ICommandGateway` centraliza órdenes, el estado tiene IDs y revisiones, y economía y combate se calculan fuera de MonoBehaviour. Ninguna garantía de antifraude u online es implícita. El contrato para Poder y Marcha está en [`../UNITY_CORE_CONTRACT.md`](../UNITY_CORE_CONTRACT.md). La ruta funcional/visual objetivo está en [`../UNITY_VERTICAL_SLICE.md`](../UNITY_VERTICAL_SLICE.md). Los assets actuales son originales de código y **bloqueo**, no candidatos de producción; consultar [`../UNITY_ASSET_AUDIT.md`](../UNITY_ASSET_AUDIT.md).

## Validación Windows real

El 2026-09-25 el propietario abrió el proyecto con Unity 6.3.23f1 en Windows, salió de Safe Mode tras corregir los cuatro errores iniciales, regeneró las escenas y completó la slice Valoria → Frontera → recolección → regreso → Aserradero. El Poder pasó de 2452 a 2622 y el guardado persistió entre Editor y player.

La primera build Windows en `Unity/Builds/Windows/` compiló y arrancó. Un fallo magenta del player se corrigió asegurando un material URP base incluido en Resources; la build posterior renderizó correctamente y la interacción de mundo/recolección funcionó fuera del Editor.

El PC Windows del propietario será el nodo Unity de Eldoria: GitHub → PC → Unity → compilación/tests/build → logs/resultados. La automatización se implantará progresivamente cuando compilación, tests y builds sean reproducibles, con el objetivo de dirigir el desarrollo principalmente desde móvil y minimizar intervención física.
