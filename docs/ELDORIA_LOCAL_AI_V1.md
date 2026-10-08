# Eldoria — motor local de IA v1

## Resultado: INSTALADO / PRUEBA REAL PASS / MISIÓN CERRADA

Verificado el 8 de octubre de 2026 en el ordenador Windows `DESKTOP-R10PE55`, con Intel i7-10700F, 16 GB de RAM y NVIDIA RTX 3060 de 12 GB.

- Ollama **0.40.1** instalado mediante la distribución standalone oficial Windows x64.
- **qwen2.5-coder:3b**, 3.1B, cuantización Q4_K_M, descargado y ejecutado localmente.
- Prueba real de programación en Windows: **9/9 casos PASS** sobre una función generada por el modelo.
- Endpoint limitado a **127.0.0.1:11434**, nube desactivada y cero llamadas a APIs externas de pago en la prueba.
- Lanzador local persistido. No se instaló un servicio ni arranque automático. El motor funciona bajo demanda; el servidor de prueba quedó cerrado y el modelo descargado de memoria.

**Evidencia final:** [run 37802085508 — SUCCESS](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37802085508), fuente `db15e0e815b22a19857efc3f6bc7d9c37792298f`, [artifact 11560822295](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37802085508/artifacts/11560822295). ZIP SHA-256: `6218fdf5ea2273ded71241cbf33bae4a3c66bf889b32fea341a622d3a46a76a5`.

El archivo de evidencias se descargó, se comprobó su hash y se abrieron directamente `proof.json`, `installation.json`, `final-check.json`, la respuesta original y los logs. Copias durables de los nueve archivos quedan en `docs/evidence/local-ai-v1/final/`; el artifact original de Actions caduca el 22 de octubre.

## Configuración y ubicación

Directorio del usuario del runner: `%LOCALAPPDATA%\EldoriaLocalAI`.

| Elemento | Ubicación o valor |
| --- | --- |
| Binarios | `%LOCALAPPDATA%\EldoriaLocalAI\ollama-v0.40.1` |
| Modelos | `%LOCALAPPDATA%\EldoriaLocalAI\models` |
| Lanzador | `%LOCALAPPDATA%\EldoriaLocalAI\tools\local-ai-install-v1.ps1` |
| Prueba | `%LOCALAPPDATA%\EldoriaLocalAI\tools\local-ai-proof-v1.ps1` |
| Host | `127.0.0.1:11434` |
| Cloud | `OLLAMA_NO_CLOUD=1` |
| Peticiones paralelas / modelos cargados | 1 / 1 |
| Contexto | 2048 tokens |
| Keep alive | 0 por defecto; 30 segundos durante la prueba para medir GPU, luego descarga explícita |

Distribución oficial: https://github.com/ollama/ollama/releases/download/v0.40.1/ollama-windows-amd64.zip . SHA-256 verificado antes de extraer: `b394d14436d38032f23190e3f14eb2c6dad5ebbe4e192414f74c8fdca01703ab`. No se ejecutó un instalador elevado, no se cambiaron PATH, variables globales, firewall, drivers, Unity ni Blender. No se requiere Python o Git en PATH del runner para este workflow.

Digest del modelo local: `f72c60cabf6237b07f6e632b2c48d533cef25eda2efbd34bed21c5e9c01e6225`. La prueba cargó el modelo enteramente en GPU, `size_vram=2081779875` bytes. Generó 65 tokens en 510640000 ns, aproximadamente 127 tokens/s en esta tarea pequeña; no es una evaluación de mantenimiento autónomo de Eldoria.

Fuentes oficiales: https://docs.ollama.com/windows ; https://docs.ollama.com/faq ; https://docs.ollama.com/api/generate . Las descargas necesitan Internet y la ejecución consume recursos/electricidad locales; no tiene tarifa de API por token en este modo local.

## Cómo utilizarlo

Con Unity y Blender cerrados, abrir PowerShell y ejecutar:

```powershell
& "$env:LOCALAPPDATA\EldoriaLocalAI\tools\local-ai-install-v1.ps1" -Mode Serve
```

Mantener esa consola abierta. El lanzador configura las variables solo para ese proceso, exige RAM/VRAM disponibles y aborta si Unity/Blender están abiertos, hay otro Ollama activo o el puerto está ocupado. No cierra aplicaciones existentes.

En otra consola PowerShell:

```powershell
$env:OLLAMA_HOST='127.0.0.1:11434'
& "$env:LOCALAPPDATA\EldoriaLocalAI\ollama-v0.40.1\ollama.exe" run qwen2.5-coder:3b
```

Para salir, finalizar la sesión CLI y pulsar Ctrl+C en la consola del servidor. Cerrarlo antes de abrir Unity/Blender o ejecutar trabajos de producción. No crear un servicio permanente, tarea programada, túnel o exposición LAN sin un alcance posterior autorizado.

Ejemplo para llamar al motor desde un ejecutor local autorizado, con el servidor abierto:

```powershell
$request=@{
  model='qwen2.5-coder:3b'
  prompt='Explica en tres frases qué hace una función clamp.'
  stream=$false
  keep_alive=0
  options=@{num_ctx=2048;num_predict=256;num_thread=4;temperature=0}
} | ConvertTo-Json -Depth 5
Invoke-RestMethod -Uri 'http://127.0.0.1:11434/api/generate' -Method Post -ContentType 'application/json' -Body $request
```

Las respuestas del modelo son datos no confiables. No ejecutar sus comandos con Invoke-Expression ni darle credenciales o acceso de escritura por el mero hecho de estar instalado.

## Prueba y no interferencia

El modelo generó una función PowerShell `Clamp` con dos comparaciones y retornos de parámetros. Un intérprete de gramática cerrada, dentro del runner Windows, comprobó nueve entradas: valores por debajo/dentro/por encima del rango, ambos límites, rangos negativos y un rango de anchura cero. Admite ambas cabeceras válidas (`function Clamp(...)` y `function Clamp { param(...) }`), pero no llamadas, imports, asignaciones, comandos, acceso a archivos/red o código extra. El texto generado nunca se ejecutó arbitrariamente como script.

`proof.json`: PASS, 9 casos, modelo/digest, respuesta local, programa interpretado, métricas, cero llamadas externas de API. `installation.json`: PROOF_PASS, versión, configuración local, sin servicios o cambios globales. `final-check.json`: PASS, listener cerrado, modelo descargado de memoria, Unity/Blender ausentes antes/después. El código del juego no se modificó y M11/QA no se iniciaron.

VRAM libre antes y después: **11701 MiB**, igual a la base observada. La lectura GPU inmediatamente posterior muestra 32 % de utilización, una muestra puntual tras inferencia; no se presenta como medición de reposo sostenido. La descarga del modelo y el cierre del listener se verificaron por separado. No se certifica rendimiento simultáneo con Unity/Blender; la política actual evita esa concurrencia.

## Repetir la comprobación

El workflow [Eldoria local AI install and isolated proof](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/workflows/eldoria-local-ai-install-v1.yml) es manual y requiere un claim del workstream activo o bloqueado en el registry. La misión cerrada ya no mantiene ese claim: reabrir este mismo bloque y reclamar sus recursos antes de repetirlo; no dejar una autorización permanente de instalación activa.

El workflow descarga únicamente los scripts del SHA del run y comprueba sus hashes. Consulta el registry vivo para evitar conflicto de recursos. Los binarios/modelo se reutilizan. El conector actual no ofrece workflow_dispatch, por lo que se activó manualmente con Run workflow → main. No se eludieron controles usando triggers automáticos.

## Propuesta para el Coordinador Central — NO ACTIVADA

El motor local es una capacidad de inferencia. El Coordinador v1 sigue siendo lector y gate de evidencias; instalar Ollama no activa agentes ni departamentos.

1. Un adaptador local consume una orden canónica aprobada y acotada: ID, commit fuente, scope, criterios y límites. Cola idempotente y un solo worker para esta máquina.
2. El worker usa únicamente el endpoint loopback y el modelo local exacto, sin fallback cloud. Presupuesto de API 0 EUR; máximo tres intentos, 120 s por generación, contexto 2048 y 512 tokens iniciales.
3. Primer piloto sin credenciales de escritura: generar propuestas en workspace temporal, validar esquema, aplicar pruebas independientes y preservar reporte/evidencias. No ejecutar comandos del modelo directamente.
4. El Coordinador consume el reporte del worker y comprueba evidencias; no acepta autoafirmaciones del modelo como PASS. Mantiene gates de M07/M11/QA y los claims actuales.
5. PRs, escritura en repositorio, herramientas Unity/Blender o autonomía persistente requieren un bloque posterior con permisos mínimos, auditoría, recuperación y scope autorizado. El modelo 3B tiene capacidad limitada: elevar autonomía depende de resultados del piloto.

## Historial y cierre

- Preflight 37792004269: hardware y ausencia inicial de Ollama; Python detectado era solo un alias.
- 37794785669: fallo de checkout por Git ausente en PATH y extracción del repositorio completo; no instaló.
- 37797719180: descarga de scripts PASS, alias Python inválido; no instaló.
- 37798837659: instaló Ollama/modelo e hizo inferencia real, pero el harness rechazó una cabecera válida; evidencia preservada y revalidación independiente 9/9.
- **37802085508: SUCCESS**, prueba nativa corregida 9/9, configuración local, lanzador persistido y cierre limpio verificados. **Workstream eldoria-local-ai-install-v1 COMPLETED; recursos liberados.**

Los fallos previos permanecen como historia verificable; no se sustituyeron por supuestos éxitos.
