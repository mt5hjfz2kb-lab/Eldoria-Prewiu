# Eldoria — informe de la prueba de memoria compartida entre agentes

**Fecha:** 2026-11-04. **Resultado:** PASS de relevo técnico aislado.  
**Alcance:** pruebas en GitHub Actions hospedado, **NO** en el equipo Windows de Eldoria. No se ha instalado un servicio permanente ni integrado ChatGPT/Codex del propietario.

## Prueba ejecutada y observada

- **Ejecución final:** https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41816975898
- **Commit exacto de código probado:** `344b6ddc992a076f68f285900f2c1f8607254ccc`
- **Versión comprobada:** SuperLocalMemory 4.2.12, Python 3.12, Mode A local, modelo de embeddings activado. Sin llamada a modelos de pago.
- **Agente A:** GitHub Actions job `writer`, id `126530090938`. Guardó cuatro hechos públicos atribuidos a `README.md`, `AGENTS.md`, `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`, `QA_AND_DEPLOY.md`. Detuvo el servicio antes de archivar los ficheros de datos.
- **Filtrado de exportación:** archivo comprimido contuvo únicamente ficheros seleccionados de SQLite/Lance; se omitieron tokens, claves, archivos de autenticación y demás configuración. No contiene recuerdos personales ni credenciales del propietario.
- **Agente B:** job `reader`, id `126530952035`, proceso y VM independientes. Los identificadores de arranque del kernel difirieron: `5a101a8f0257bd7fb67f` (A) frente a `0bd50094e13cec12f5c8` (B). El nombre de host era igual: por eso **no** sirve como prueba de identidad de VM.
- El lector **confirmó memoria vacía antes de la importación**. Comparó la integridad del archivo mediante SHA-256. Restauró el almacén de datos sin copiar credenciales.
- **MCP:** cliente de B enumeró 19 herramientas y recuperó las cuatro fuentes **4/4**, sin errores. Añadió un nuevo dato de prueba mediante MCP y **otro proceso CLI** pudo recuperarlo.
- **Resultado final:** `PASS_SHARED_MEMORY_HANDOFF`; ambas tareas completaron `success`.
- **Artefacto de lector:** https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41816975898/artifacts/13315711452 (metadatos indican caducidad 2026-11-11). Copia `reader.json` contiene los resultados observados.

## Iteración y causas de error

1. https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41816000369 — **fallo seguro**: el filtro inicial detectó posibles ficheros de credenciales en la carpeta y se negó a subir la base completa. Se sustituyó por un allowlist explícito de SQLite/Lance.
2. https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41816341188 — A completó, B falló antes de importar porque se comparó el hostname, que GitHub reutiliza en imágenes de máquinas distintas. Se corrigió el criterio: identidad de arranque del kernel + memoria vacía antes del relevo.
3. https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41816975898 — prueba final **aprobada técnicamente**.

## Qué NO demuestra

- No constituye una conexión real de dos chats de ChatGPT ni de Codex autenticado.
- No prueba sincronización **simultánea y en tiempo real** entre máquinas; demuestra un relevo persistente por transferencia explícita de datos.
- No valida la respuesta ante todas las contradicciones ni la protección frente a errores de recuperación. La prueba anterior de 20 consultas obtuvo 19 coincidencias y encontró respuestas no concluyentes ante preguntas desconocidas.
- No se ha aplicado ninguna decisión al juego, modificado `main`, reclamado el ejecutor Windows ni publicado Unity.
- No garantiza que el plan actual de ChatGPT permita configurar un MCP personalizado.

## Siguiente requisito auténtico

La documentación de Qualixar indica que **ChatGPT web necesita que el usuario conecte la aplicación por OAuth**, con el daemon de SLM ejecutándose en un ordenador propio disponible, y que active explícitamente acceso web (preferiblemente solo lectura inicialmente). No existe un plugin SuperLocalMemory ya conectado y disponible en esta sesión.

Antes de instalar en Windows: confirmar que el ejecutor de Unity se encuentre realmente libre, comprobar recursos/seguridad, hacer backup, instalar en espacio aislado y evaluar estabilidad. El equipo no debe reiniciarse ni un plugin de agente activar escritura automática en `main`.

**Recomendación actual:** conservar GitHub como autoridad de decisiones; usar SLM como índice de recuerdos revisados con enlace y SHA a la fuente, detectar información sustituida, y exigir consulta de la fuente antes de actuar. Integración real en ChatGPT: **pendiente de consentimiento/autenticación del propietario y comprobación de compatibilidad**.
