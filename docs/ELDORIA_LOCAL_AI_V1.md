# Eldoria — motor local de IA v1

## Estado verificable a 8 de octubre de 2026

**OLLAMA INSTALADO / MODELO DESCARGADO Y RESPUESTA LOCAL VERIFICADA / PRUEBA NATIVA FINAL PENDIENTE. La misión no está cerrada.**

La conexión GitHub funciona. El [preflight 37792004269](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37792004269) terminó correctamente en `DESKTOP-R10PE55`: 16 procesadores lógicos, 8,75 GB RAM libre, RTX 3060 de 12288 MiB, 11701 MiB VRAM libre, GPU al 0 %, 488,8 GB libres en el disco consultado. El inventario encontró el comando Python; el segundo intento ha confirmado que es un alias de Microsoft Store sin intérprete disponible. Ollama no se encontró en PATH y no tenía proceso activo. Esto acredita acceso mediante Actions en aquel momento, no instalación ni disponibilidad futura.

Bloqueo de plataforma: el conector conectado tiene lectura de runs/jobs y escritura de archivos, pero no una operación de `workflow_dispatch`. No se ha usado un trigger push para saltarse la regla de AGENTS.md que exige activación manual de instalaciones y diagnósticos. No se ha iniciado otro login ni se ha solicitado un token. El preflight automático existente pertenece a otra sesión y queda intacto.

## Activación concreta

En [Actions: Eldoria local AI install and isolated proof](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/workflows/eldoria-local-ai-install-v1.yml), seleccionar **Run workflow → main → Run workflow**. Puede hacerse desde el navegador móvil con una sesión GitHub autorizada. El ordenador debe estar encendido, el runner conectado y Unity/Blender cerrados. El workflow no cierra aplicaciones del usuario: si están abiertas, se bloquea.

Al terminar, inspeccionar el run y el artifact `eldoria-local-ai-proof-<run_id>`. Un run verde debe contener `proof.json` y `final-check.json` con PASS, respuesta original, código generado, versión, digest del modelo, log local-only y snapshots GPU. Fallo o ausencia de evidencia significa NO COMPLETADO. Registrar aquí el run/artifact reales después de inspeccionarlos; no inferir PASS por haber publicado estos archivos.

## Instalación y límites

Se utiliza la distribución standalone oficial Windows x64 **Ollama v0.40.1**, sin instalador de servicios, sin administrador y sin cambios globales de PATH, firewall, drivers o variables de usuario. ZIP oficial: https://github.com/ollama/ollama/releases/download/v0.40.1/ollama-windows-amd64.zip . SHA-256 de release oficial: `b394d14436d38032f23190e3f14eb2c6dad5ebbe4e192414f74c8fdca01703ab`. La descarga se verifica antes de extraer. Referencia: https://docs.ollama.com/windows .

Directorio persistente del usuario del runner: `%LOCALAPPDATA%\EldoriaLocalAI`, con binarios en `ollama-v0.40.1` y modelos en `models`. No se comparte la Library de Unity ni se modifica el proyecto Unity/Blender. Actions descarga únicamente los dos scripts fijados al SHA del run y verifica sus SHA-256 antes de ejecutarlos; no usa Git ni descarga el repositorio completo. El registry se consulta desde main en el momento de ejecutar.

Solo modelo `qwen2.5-coder:3b`, descargado del registro de Ollama. Servidor en `127.0.0.1:11434`, `OLLAMA_NO_CLOUD=1`, una petición paralela, un modelo cargado, contexto 2048, cuatro hilos en la prueba y descarga inmediata del modelo tras el uso. No hay cuentas cloud ni API de pago; sí descarga por Internet de binarios/modelo y uso de electricidad/recursos locales. Referencias: https://docs.ollama.com/faq y https://docs.ollama.com/api/generate .

La instalación exige Windows 10 22H2 o posterior, 6 GB de RAM libre, 6000 MiB de VRAM libre y 12 GB libres de disco. No sustituye un servidor previo: aborta si hay Ollama activo o puerto 11434 ocupado. Todo proceso que inicia la prueba se termina al acabar; no queda daemon permanente ni consumo recurrente de GPU.

## Prueba real aislada

La prueba vigente solicita al modelo una función PowerShell `Clamp` con parámetros `value, low, high`. Se conserva la respuesta original y se comprueban nueve casos incluyendo límites, negativos y rango de anchura cero. Un intérprete de gramática cerrada admite exclusivamente dos comparaciones condicionales y tres retornos de parámetros. El texto generado nunca se ejecuta como script; no se usa Invoke-Expression ni se permiten llamadas, comandos, asignaciones, red, archivos o credenciales. La prueba usa PowerShell/.NET ya existentes y no requiere Python. Esto demuestra una tarea pequeña de programación; no acredita todavía agentes autónomos capaces de mantener Eldoria.

La comprobación de no interferencia es acotada: instalación separada, aplicaciones ausentes antes/durante/después, puerto local verificado, modelo descargado de memoria y cierre de procesos propios. No certifica rendimiento simultáneo con Unity; por defecto no se autoriza esa concurrencia.

## Uso tras un PASS real

Desde PowerShell, después del PASS (el workflow persiste el lanzador seguro):

```powershell
& "$env:LOCALAPPDATA\EldoriaLocalAI\tools\local-ai-install-v1.ps1" -Mode Serve
```

Mantener esa consola abierta. En otra PowerShell:

```powershell
$env:OLLAMA_HOST='127.0.0.1:11434'
& "$env:LOCALAPPDATA\EldoriaLocalAI\ollama-v0.40.1\ollama.exe" run qwen2.5-coder:3b
```

Para una llamada desde el futuro ejecutor, usar POST `http://127.0.0.1:11434/api/generate` con modelo exacto, `stream:false`, `keep_alive:0`, contexto 2048 y un límite `num_predict`. Terminar la sesión CLI y el servidor con Ctrl+C antes de abrir Unity/Blender o despachar producción. No crear un servicio, tarea programada, túnel ni puerto LAN sin un alcance posterior autorizado. Para repetir el test, usar el workflow manual; no es necesario reinstalar si los binarios ya existen.

## Propuesta: conexión al Coordinador Central

La propuesta NO está activada. El coordinador v1 sigue siendo lector y gate de evidencias, no ejecutor de departamentos.

1. Tras PASS verificable, un adaptador local toma una orden canónica aprobada y acotada con ID, commit fuente, scope, criterios y límites. Mantiene cola idempotente y un único worker para esta máquina.
2. El worker consulta el endpoint loopback; presupuesto de API 0 EUR, máximo tres intentos, 120 s por generación, contexto 2048 y 512 tokens iniciales. Solo admite el modelo local exacto, sin fallback cloud.
3. El primer piloto genera propuestas en un workspace temporal sin credenciales de escritura. La respuesta es datos no confiables: validación de esquema, revisión y pruebas independientes antes de ejecutar cualquier acción.
4. El coordinador consume únicamente el reporte y evidencias del worker. No considera la autoafirmación del modelo una prueba. Las restricciones M07/M11/QA siguen gobernadas por el repositorio y sus gates actuales.
5. Escritura en repositorio, PRs, herramientas Unity/Blender y autonomía persistente necesitan un bloque posterior con permisos mínimos, scope claim, auditoría y rollback. El modelo 3B es un piloto de capacidad limitada; aumentar autonomía depende de resultados, no de su mera instalación.

## Continuación y cierre

Workstream `eldoria-local-ai-install-v1`. Recuperar main y el registry antes de continuar. Lanzar el workflow manual cuando la capacidad esté disponible, revisar logs/evidencia, reparar fallos dentro de este mismo scope, registrar run/artifact/digest y cerrar únicamente tras PASS real. No iniciar M11 ni QA. No se ha modificado el código del videojuego.

## Primer intento real y reparación — 2026-10-08

El propietario activó el [run 37794785669](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37794785669). Llegó a DESKTOP-R10PE55, pero falló antes de ejecutar el instalador: Git no está en PATH del runner; actions/checkout recurrió a un archivo completo y Expand-Archive falló al extraerlo. Ollama y el modelo NO se instalaron y no se ejecutó la prueba. No existe artifact de prueba de ese intento.

Reparación: eliminado actions/checkout; se obtienen solamente los dos scripts desde raw.githubusercontent.com con el SHA inmutable del run y sus hashes SHA-256 esperados. El instalador standalone también utiliza extracción .NET en vez de Expand-Archive. No se cambia Git, PATH ni las herramientas existentes. YAML parseado y hashes comprobados localmente; ejecución Windows de la reparación aún pendiente.

**Siguiente paso necesario:** Run workflow → main → Run workflow nuevamente sobre la definición corregida. No usar Re-run jobs del intento antiguo: reutilizaría la definición del commit fallido. El conector continúa sin operación workflow_dispatch. Recuperar el nuevo run, inspeccionar prueba real y artefactos; no declarar cierre hasta PASS verificable.

## Segundo intento real y eliminación de dependencia Python

[Run 37797719180](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37797719180), job 113381437420: descarga de scripts y verificación de hashes funcionan, pero `python --version` falla porque el comando es un alias Microsoft Store. Se detuvo ANTES del instalador; no se descargó Ollama ni el modelo, sin prueba real ni artifact.

Se sustituye la prueba Python por `tools/local-ai-proof-v1.ps1`, intérprete cerrado de una función PowerShell generada, con los mismos nueve casos y preservación de respuesta/digest/métricas. No se instala Python ni se modifican aliases/PATH o Blender. El harness anterior Python se conserva como historia, no lo utiliza el workflow vigente. La gramática se comprobó contra un ejemplo puro y cuatro intentos de inyección; esto es validación estática, no ejecución nativa Windows ni prueba real del modelo.

El nuevo workflow todavía debe activarse con **Run workflow → main → Run workflow**. No usar Re-run del run anterior. La ausencia de dispatch en el conector sigue siendo el bloqueo para ejecutar la definición nueva.

## Tercer intento — instalación y respuesta local reales, defecto del verificador

[Run 37798837659](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37798837659), artifact **11559797875**, ZIP SHA-256 `14629db8819ad8ac72dc7e5c1a28415892a05166ba029db77d57c2b6e7f5a689`. Artifact descargado, hash comprobado y contenido abierto directamente.

**Verificado:** Ollama **0.40.1** instalado y sirviendo en 127.0.0.1:11434; log `Ollama cloud disabled: true`; NVIDIA RTX 3060, CUDA, 37/37 capas descargadas a GPU. Modelo qwen2.5-coder:3b descargado con comprobación SHA-256 por Ollama; blob GGUF `4a188102020e9c9530b687fd6400f775c45e90a0d7baafe65bd0a36963fbb7ba`. Respuesta local completa de 65 tokens, 515051000 ns de generación, 3132765100 ns total. Antes y después: 11701 MiB VRAM libre, 0 % GPU; el servidor de prueba se terminó mediante el bloque finally.

El run figura **FAIL** porque el verificador admitía únicamente `function Clamp { param(...) ... }`, mientras que el modelo generó la variante válida `function Clamp(...) { ... }`. No es un fallo de instalación ni de inferencia. La fuente real se revalidó independientemente en un intérprete de gramática cerrada: **9/9 casos PASS**, cuatro intentos de inyección rechazados. Fuente/respuesta original y reporte quedan en `docs/evidence/local-ai-v1/attempt3-*`; esa revalidación es local a la sesión y no se presenta como ejecución nativa Windows posterior al arreglo.

Corregido el verificador para admitir ambas cabeceras con los mismos parámetros y el mismo cuerpo restringido, sin permitir comandos nuevos. No se relajan permisos ni se ejecuta código generado. Hace falta una nueva activación **Run workflow → main** para ejecutar el harness corregido en Windows, persistir el lanzador local y comprobar `proof.json`, `installation.json` y `final-check.json`. Reutilizará binarios y modelo existentes. No se necesita repetir las descargas pesadas. Hasta entonces NO CERRADO.
