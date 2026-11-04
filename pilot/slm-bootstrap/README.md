# Eldoria — SuperLocalMemory: instalación permanente y conexión

**Estado verificado el 4/11/2026:** SuperLocalMemory 4.2.12 superó los pilotos técnicos en Linux y la recuperación entre dos máquinas virtuales distintas, pero **todavía NO está instalado permanentemente en el ordenador Windows ni conectado a los chats de ChatGPT o a una sesión autenticada de Codex**.

## Impedimento real
La reserva `windows-runner-heavy` está actualmente asignada a `eldoria-unity-metadata-integrity-20261104` (owner `codex-eldoria-metadata-recovery-20261104`). La última ejecución puede finalizar sin liberar el derecho de uso. No instalar en Windows, no ejecutar otro editor, no finalizar tareas ni forzar la liberación. Comprobar siempre el registro **vivo** `main/pipeline/active-workstreams.json`, las ejecuciones y los procesos.

## Implantación preparada
- `verified-facts.json`: 10 recuerdos públicos, con ruta y SHA de cada archivo origen. No contiene claves, credenciales ni información privada. Se verifican de nuevo contra GitHub al importarlos.
- Bundle local de instalación creado en el chat propietario: `eldoria_superlocalmemory_instalacion_preparada.zip`, SHA-256 `684c96956445c58cdc48e032547d51f5f4792e6ac52d33296487cbac63d5812b`; contiene un instalador PowerShell, preflight de reserva, importador y pruebas. **No se ha ejecutado en Windows**.
- Piloto de SLM previo: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41814627320 ; piloto de dos entornos: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41816975898 .
- La memoria será un índice de decisiones con fuente, nunca una autoridad sobre cambios, permisos, arte o reservas.

## Comprobaciones exigidas antes de instalar
1. Relevo oficial de `windows-runner-heavy` al trabajo de instalación SLM, con propiedad, scope y recurso registrados mediante CAS en main; no bastan ausencia de jobs ni un mensaje de chat.
2. Preflight lectura sola: propietario de reserva, cero procesos Unity/Blender/Ollama, suficiente RAM y disco, Python 3.12 y usuario de Windows estable. Ante error, **FAIL CLOSED**.
3. Instalación en virtualenv separado y directorio privado del usuario, paquete fijado a `superlocalmemory==4.2.12`, Mode A, **sin servicios autoiniciados**, sin Web access, sin reinicios y sin gasto.
4. Importar recuerdos revisados en memoria local y contrastar varios con fuentes de GitHub exactas; registrar evidencia y reversión. Si alguna fuente cambió, reexaminar antes de importar.
5. Conexión MCP a Codex solo tras comprobar sesión/usuario adecuados. La conexión web de ChatGPT requiere aprobación OAuth personal y la app habilitada en la cuenta; empezar con permisos de solo lectura. No publicar códigos de dispositivo en GitHub.

## Límites
No es posible activar una memoria permanente en el servidor Windows ocupado con las garantías de no interferencia vigentes. El paquete y los datos están preparados; el resto sigue **pendiente de implementar y validar**. Las pruebas temporales NO demuestran conexión con chats reales.
