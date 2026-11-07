# Eldoria — conectar SuperLocalMemory a ChatGPT (solo lectura)

**Situación comprobada:** SuperLocalMemory 4.2.12 está instalado en el perfil Windows del ejecutor y pasó persistencia y MCP local; CI [#42181121389](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42181121389). **NO** hay conexión ChatGPT autenticada ni URL web de SLM validada. Tampoco Codex ha superado todavía conexión en Windows; su script y workflow están preparados, pero dependen de la reserva nativa.

## Para completar ChatGPT, cuando el titular esté en su propio ordenador

1. Abrir la interfaz local de SuperLocalMemory con `slm dashboard` desde el entorno SLM instalado (mismo usuario Windows). En **Connected apps**, elegir **ChatGPT**.
2. Activar acceso web **solo para lectura**; mantener desactivado **Allow saving memories** y **Allow session tools** inicialmente. No activar compartición de claves, información privada ni otros perfiles.
3. Completar **Link this computer with GitHub** en su navegador y aprobar OAuth personalmente. No publicar la URL de autorización, cookies, códigos OAuth, tokens o capturas de credenciales en GitHub.
4. Copiar desde el dashboard el **servidor MCP HTTPS** y, si es necesario, los detalles OAuth proporcionados. No inventar URL ni abrir puertos públicos arbitrarios.
5. Desde un navegador **web** de ChatGPT (la creación de MCP personalizado no se realiza en la aplicación iOS), abrir **Plugins** → **+** → **Add custom MCP server**, si la cuenta lo permite. Introducir nombre, URL y configuración de autenticación; revisar el aviso y autorizar desde el propio navegador. Las políticas de cuenta/plan pueden impedirlo.
6. Seleccionar el plugin conectado en un chat y hacer una prueba de solo lectura: pedir el dato de una decisión pública confirmada, por ejemplo "¿Cuál es el proyecto jugable principal de Eldoria y de qué archivo oficial procede?". **Exigir que sea un resultado real de recall/fetch y verificarlo contra GitHub**, no confiar en respuestas espontáneas del modelo.
7. Si el equipo Windows está apagado o sin enlace web, puede aparecer `connector_asleep`; eso es distinto de "no existe el conocimiento". La memoria local requiere el equipo encendido para servir consultas web.

**IMPORTANTE:** El agente de ChatGPT no puede prestar consentimiento OAuth en nombre del usuario, ni modificar por sí mismo las preferencias de su cuenta. No afirmar que todos los chats tienen memoria compartida hasta que el usuario haya conectado el servidor y se haya comprobado recuperación real.

Referencias actuales:
- https://github.com/qualixar/superlocalmemory/blob/main/docs/remote-access/hosts.md
- https://github.com/qualixar/superlocalmemory/blob/main/docs/web-agents/instructions.md
- https://help.openai.com/en/articles/12584461-connecting-custom-mcp-servers-in-chatgpt

**Seguridad:** Las consultas enviadas por ChatGPT y sus respuestas pasan por el gateway de conexión y el servicio de IA; que la base esté en el equipo no significa que todo el contenido de las consultas permanezca local. Mantener solo recuerdos públicos autorizados y el nivel mínimo de acceso.
