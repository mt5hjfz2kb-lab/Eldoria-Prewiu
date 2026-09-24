# Eldoria en Unity — primera slice de Valoria

**Estado: proyecto fuente preparado, aún no compilado ni jugado en Unity.** Es una línea Unity separada del producto web v0.32.0; esta carpeta no cambia la versión ni la URL del web. La versión del Editor está fijada en `ProjectSettings/ProjectVersion.txt`: **6000.3.23f1 (Unity 6.3 LTS)**.

## Abrir y jugar

1. Instala Unity Hub, Unity Editor **6000.3.23f1** y el módulo Linux Build Support si vas a generar el player Linux. Activa una licencia que permita ejecutarlo. No se necesitan assets de pago.
2. Desde Hub abre la carpeta `Unity/` del repositorio. Espera a que Package Manager importe URP, Input System, uGUI y Test Framework; revisa Console y confirma que no hay errores de compilación. Conserva el `Packages/packages-lock.json` que produzca una importación correcta en un commit posterior.
3. Ejecuta **Eldoria → Regenerate slice scenes and build settings**. Este comando configura URP y escribe las tres escenas versionadas. Abre `Assets/Eldoria/Scenes/Bootstrap.unity` y pulsa **Play**. En caso de URP sin renderer válido, crea un `Universal Renderer` y un `URP Pipeline Asset` desde el menú Assets y asígnalos a Graphics y Quality; registra el cambio porque el editor no ha validado la automatización de la configuración URP.
4. En Valoria, Aldric explica que faltan 80 de madera y tienes 30. Usa la puerta o **IR AL MUNDO**, toca el bosque o **BOSQUE · RECOLECTAR**, espera el regreso de la marcha (9 s), vuelve a Valoria y mejora el Aserradero (6 s). El explorador corrupto ofrece un encuentro PvE opcional; su resultado no adelanta Bastión II. Verifica que la estructura reparada y el Poder pasan de **2452 → 2622**. El texto de fin declara el cierre de esta slice; no es el final de Arco I.
5. El guardado local está en `Application.persistentDataPath/eldoria-unity-slice-v1.json`. Para iniciar una partida completamente nueva **cierra el player/Play y elimina solo ese archivo de save de esta slice**. Guarda una copia si quieres preservarlo. Un save web no se importa automáticamente.

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
| Comandos versionados/idempotentes; estado de recursos, construcción y marcha; Poder derivado; pronóstico/encuentro PvE; regreso y premio; guardado versionado; escenas/controles fuente. | Geometría modular por primitivas, Aldric/arquero silueta, bosque, Brecha pulsante, UI uGUI española, estadísticas de amenaza y economía del corte. | Importación/compilación real de paquetes, tests EditMode/PlayMode, pipeline URP efectivo, build Linux, screenshots, sensibilidad y safe area móvil, arte/audio/animación y playtest humano. |

El cliente local es autoridad **solo de esta slice**; `ICommandGateway` centraliza órdenes, el estado tiene IDs y revisiones, y economía y combate se calculan fuera de MonoBehaviour. Ninguna garantía de antifraude u online es implícita. El contrato para Poder y Marcha está en [`../UNITY_CORE_CONTRACT.md`](../UNITY_CORE_CONTRACT.md). La ruta funcional/visual objetivo está en [`../UNITY_VERTICAL_SLICE.md`](../UNITY_VERTICAL_SLICE.md). Los assets actuales son originales de código y **bloqueo**, no candidatos de producción; consultar [`../UNITY_ASSET_AUDIT.md`](../UNITY_ASSET_AUDIT.md).
