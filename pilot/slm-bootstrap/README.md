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


## Ejecución comprobada de la preparación (4/11/2026)

- Script de instalación segura y preflight: [`Install-SlmWindows.ps1`](Install-SlmWindows.ps1). Solo instala con `-Mode install`, y únicamente con una **reserva propia explícita en main** para el ID `eldoria-slm-permanent-bootstrap-20261104`, owner `chatgpt-eldoria-slm-bootstrap-20261104`.
- Validación aislada hospedada (sin Windows): [GitHub Actions #41820422456](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41820422456) **SUCCESS**. El parser PowerShell informó `POWERSHELL_PARSE_PASS` y el verificador online confirmó `VERIFIED_SOURCES_PASS 8`.
- El script y sus dependencias **NO se han ejecutado en un Windows real**. Comprobar un parser y una fuente no equivale a probar una instalación.
- **No utilizar `-Mode install`** mientras el registro mantenga la reserva ajena. Tampoco activar automáticamente Web access o servicios en segundo plano.
- No se ha modificado `main`, el código del juego, las PR abiertas, las reservas ni el instalador de Unity.

### Orden de continuación

Releer el registro de reservas. Una vez liberada oficialmente la reserva, asignar en main el ID del piloto SLM bajo CAS, verificar usuario y RAM, ejecutar el script en modo `check`; si pasa, ejecutar `install` en ese mismo usuario sin reiniciar. Comprobar después que los recuerdos se mantienen tras cerrar y reabrir el proceso, que Codex puede consultar MCP, y por último solicitar al propietario la autorización OAuth específica de ChatGPT.


## 6/11/2026 — relevo seguro para completar instalación

- La operación fue retomada tras comprobar que GitHub vuelve a aceptar escrituras. Actualizada la huella SHA de `README.md` en `verified-facts.json` y la comprobación Ubuntu volvió a pasar: [run 42129498935](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42129498935).
- Registrado en `main/pipeline/active-workstreams.json` el ID canónico `eldoria-slm-permanent-bootstrap-20261104`, titular `chatgpt-eldoria-slm-bootstrap-20261104`, en alcance aislado, **sin** reserva Windows inicialmente: commit `c8d7ab1d389b96c3d1b6442804500a122a6ee422`. Verificar siempre la versión viva; esta es evidencia histórica de la reclamación.
- Windows **no** estaba realmente disponible para ejecutar nuevas tareas: había una certificación Unity nativa en curso, run [42129353689](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42129353689) en el paso PlayMode. No cancelar ni interferir. La ausencia de reserva formal en el registro no significa que no haya procesos Unity trabajando.
- Creado preflight de hardware y procesos en **solo lectura**, [workflow](../../.github/workflows/eldoria-slm-windows-preflight.yml), ejecución [42129818778](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42129818778). Puede quedar en cola detrás de Unity. Comprobar resultado antes de decidir instalación.
- Preparado el [workflow de instalación protegido](../../.github/workflows/eldoria-slm-windows-install.yml) en esta rama. No se ejecuta por su creación; únicamente se activa con un `install-trigger.json` nuevo/actualizado **después** de obtener la reserva exclusiva Windows registrada en main (CAS) y de verificar que no haya ningún proceso ajeno. Al ejecutarse vuelve a verificar ambas condiciones y el instalador repite el preflight.
- El instalador no arranca servicios permanentes, no activa acceso web, no toca Unity ni modifica `main`. Cuando funcione hay que probar persistencia/MCP y decidir un servicio seguro sin contención de recursos; la conexión ChatGPT necesita autorización OAuth personal, que no puede hacerse a través de logs públicos ni por un agente sin el propietario.

**Orden segura:** comprobar fin de run nativo + resultado del preflight → consultar `main` en vivo → reclamar exclusivamente `windows-runner-heavy` en el ID ya creado mediante su blob SHA → volver a comprobar procesos/claims → crear `pilot/slm-bootstrap/install-trigger.json` en la misma rama (el workflow se inicia por push) → seguir ejecución y reparar fallos repetibles → verificar memoria persistente en el usuario Windows adecuado → liberar Windows y documentar resultado. Si cualquier comprobación falla, no iniciar instalación ni apropiarse de trabajo ajeno.
