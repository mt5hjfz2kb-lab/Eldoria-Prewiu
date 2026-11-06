# SuperLocalMemory para Eldoria — estado verificado

**6/11/2026 · Rama de piloto (no integrada en `main`).**

## Instalación en Windows: CERTIFICADA

SuperLocalMemory **4.2.12 está instalado en el perfil local privado del ejecutor Windows** (`%LOCALAPPDATA%\\EldoriaSLM`). Es un entorno Python 3.12 separado del juego; no modifica Unity ni requiere pagar llamadas a modelos externos.

Evidencia exacta: [GitHub Actions #42181121389](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42181121389), job `127770736821`, todas las etapas **SUCCESS**. Los logs confirman:

- `SLM_LOCAL_INSTALL_PASS memories_saved=10`: diez conocimientos revisados y con referencias Git verificadas.
- `slm doctor --quick`: 33 comprobaciones superadas, 3 advertencias, 0 fallos.
- Tras detener y reabrir el daemon, las consultas CLI y el cliente MCP stdio independiente recuperaron los conocimientos: `SLM_MCP_STDIO_PASS` y `SLM_WIN_VERIFIED`.
- Sin servicio autoarrancado, sin exposición a internet, sin OAuth, sin reiniciar Windows, sin tocar el juego, sin añadir costes de API. La memoria **sí es persistente** aunque el daemon se detuviera al terminar.

La instalación usa `Install-SlmWindows.ps1` y fuentes `verified-facts.json`. [Informe y pruebas detalladas](FINAL_HANDOFF_20261106.md).

## Conexión Codex: PREPARADA / TODAVÍA NO VALIDADA EN WINDOWS

Se ha desarrollado una conexión segura con un `mcp_servers.superlocalmemory` privado, que apunta a la instalación local por ruta absoluta y preserva la configuración de otros servidores Codex. Se respalda la configuración previa y no se sustituyen archivos ajenos.

Archivos:

- `Connect-SlmCodex.py`: edición aditiva, idempotente y con respaldo de `~/.codex/config.toml`.
- `Connect-SlmCodex.ps1`: comprobaciones de usuario, reserva Windows, proceso Unity, instalación SLM, integraciones y consulta MCP.
- `test_codex_config.py`: 7 ensayos reproducibles; [preflight CI #42189603865](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42189603865) **SUCCESS**.
- [Workflow de conexión](../../.github/workflows/eldoria-slm-codex-connect.yml): preparado y dormido hasta crear `codex-connect-trigger.json` cuando el recurso Windows esté libre.

No iniciar esta conexión si otro trabajo es titular válido de `windows-runner-heavy`, ni si Unity sigue ejecutándose. Tampoco intentar iniciar sesión en Codex ni filtrar claves OAuth por logs de un repositorio público. Conexión MCP registrable y uso auténtico de un modelo autenticado son dos pruebas distintas.

## Conexión ChatGPT: REQUIERE ACCIÓN PRIVADA DEL PROPIETARIO

No está activada. SuperLocalMemory permite un servidor MCP web con OAuth, pero **la vinculación privada a ChatGPT requiere que el usuario configure y autorice el plugin en su cuenta**. La IA no puede aceptar la autorización por él ni publicar códigos de acceso. Inicialmente se recomiendan permisos de **solo lectura**, con consentimiento explícito para escritura más adelante.

Guía oficial: https://github.com/qualixar/superlocalmemory/blob/main/docs/remote-access/hosts.md

## Coordinación

El workstream es `eldoria-slm-permanent-bootstrap-20261104`, con titular `chatgpt-eldoria-slm-bootstrap-20261104` en `main/pipeline/active-workstreams.json`. La memoria es un **índice auxiliar** de conocimientos; las decisiones vigentes, código, permisos y evidencias siguen siendo los archivos y SHA de GitHub.

**No hay automatizaciones periódicas de seguimiento:** el usuario prohibió supervisiones; la automatización anteriormente creada se desactivó. Para conectar Codex se hará solo un workflow de ejecución puntual, con guarda de reserva y proceso.

**No se debe afirmar que “todos los chats recuerdan Eldoria” hasta que una conexión real y autenticada de ChatGPT supere una prueba de recuperación.**
