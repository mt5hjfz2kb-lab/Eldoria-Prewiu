# Eldoria · SuperLocalMemory — instalación real de Windows certificada

**Corte:** 2026-11-06 (Europe/Madrid). **Autorización:** propietario pidió terminar el encargo sin supervisiones programadas.

## INSTALADO / PROBADO en la máquina Windows

- Instalación en directorio de usuario privado aislado (`%LOCALAPPDATA%\EldoriaSLM`), virtualenv Python 3.12, paquete **superlocalmemory 4.2.12** (fijado), sin reinstalar Unity ni reiniciar Windows.
- Fuente: [GitHub Actions #42181121389](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42181121389), job **127770736821**, ejecución **SUCCESS** (todas las fases correctas), commit de instalación `18dbab75a9ce2f3501777f647b5adbcf41a0403d`.
- Mensaje literal observado: `SLM_LOCAL_INSTALL_PASS memories_saved=10; no autostart or remote integration`.
- `slm doctor --quick`: **33 PASS, 3 WARN, 0 FAIL**. Las advertencias incluían plugins de agentes todavía no configurados.
- Estado de las memorias: 10 hechos públicos verificados contra 8 fuentes Git exactas, guardados y consultables.
- Persistencia tras detener/iniciar el daemon: CLI consultó dos hechos; cliente MCP **stdio** independiente reportó `SLM_MCP_STDIO_PASS` y `SLM_WIN_VERIFIED: true; CLI persistence and MCP tools/call both passed`.
- El instalador detuvo su daemon al terminar. **No** se activó servidor web externo, OAuth, servicio Windows permanente ni telemetría de aplicaciones.
- Artefacto técnico de ejecución: ID `13485799759` (sanitizado, sin bases de datos ni claves).

## Actualización tras la instalación

- El workstream canónico `eldoria-slm-permanent-bootstrap-20261104` sigue **reclamado** para completar configuración. Registro: `main/pipeline/active-workstreams.json`, fase `WINDOWS_MODE_A_INSTALLED_10_MEMORY_MCP_VERIFIED_CODEX_WIRING_PENDING`.
- No tiene reserva Windows mientras otro workstream la use. No tomar una reserva válida por la fuerza ni cancelar los trabajos del juego.
- Preparación de Codex en la **misma rama existente**, evitando otra PR o workstream:
  - `Connect-SlmCodex.py`: añade la entrada MCP en `~/.codex/config.toml` preservando todos los servidores existentes, usa ruta absoluta al ejecutable privado de SLM y `SLM_HOME` explícito, crea copia local de seguridad antes de modificar y rechaza sobrescrituras de configuración ajena.
  - `test_codex_config.py`: siete pruebas de sintaxis TOML, idempotencia, copia previa, preservación de otros servidores y rechazo de conflictos. CI Ubuntu `eldoria-slm-bootstrap-check.yml`.
  - `Connect-SlmCodex.ps1`: valida previamente usuario, instalación propia, claim exclusivo Windows, ausencia de Unity/Blender, configura Codex y comprueba MCP, sin lanzar modelos, reiniciar Windows ni iniciar OAuth.
  - `.github/workflows/eldoria-slm-codex-connect.yml`: ejecución **manual de un solo intento**, activada por `pilot/slm-bootstrap/codex-connect-trigger.json` solo tras obtener la reserva legal y comprobar procesos de nuevo.
- **No afirmar conexión real a Codex hasta que este workflow pase**, ni afirmar sesión de modelo autenticada si Codex todavía no está identificado/iniciado.

## Condición externa no automatizable en este chat

La conexión de un chat de ChatGPT con SLM **exige que el propietario autorice personalmente** OAuth y el servidor MCP en Ajustes/Plugins de ChatGPT. Documentación oficial: https://github.com/qualixar/superlocalmemory/blob/main/docs/remote-access/hosts.md

Por defecto comenzar **solo lectura**; no publicar tokens de OAuth, device codes ni URLs de autenticación privada en el repositorio público. La autorización de ChatGPT no se realiza en GitHub Actions, y no se debe anunciar como hecha.

## Siguiente acción segura

1. Revisar `main` y cola Actions, esperar a que el titular de `windows-runner-heavy` libere la reserva. El propietario **no** pidió supervisiones recurrentes; se deshabilitó la automatización `Automation_5e7d00c54a0c8191a04e324a7daa380e`.
2. Obtener la reserva mediante CAS en el **mismo** workstream `eldoria-slm-permanent-bootstrap-20261104`.
3. Crear `pilot/slm-bootstrap/codex-connect-trigger.json` en la rama existente (o actualizarlo solo tras validar ejecución previa); consultar run hasta resultado final. Fallar seguro si Unity sigue trabajando.
4. Verificar Codex config, skills, consultas MCP y seguridad; **no activar Web access sin OAuth del propietario**. Liberar Windows tras cerrar pruebas.
5. Registrar estado final sin confundir instalación local con conexión de chats reales.
