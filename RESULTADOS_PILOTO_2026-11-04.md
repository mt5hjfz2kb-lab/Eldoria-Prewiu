# Eldoria — piloto aislado de SuperLocalMemory y viabilidad IA ↔ Unity

**Fecha de validación:** 2026-11-04  
**Estado:** piloto técnico terminado; NO integrado en Eldoria ni ChatGPT/Codex del propietario.  
**Ámbito:** rama aislada `pilot/slm-disposable-validation-20261104`; árbol Git de prueba con solo el workflow, el script y este informe; sin tocar `main`, Unity ni Windows.  
**Software probado:** SuperLocalMemory 4.2.12, Python 3.12, Linux x64 de GitHub hospedado, modo A local, embedder `nomic-embed-text-v1.5` cargado. Sin API de modelo de pago.

## Evidencia de ejecución reproducible

- Ejecución inicial: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41813676117
- Repetición con evidencia ZIP: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41814175139
- Ejecución final con conexión MCP y consultas refinadas: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41814627320
- Artefacto final ID `13314505988`, SHA-256 `acd41ab92f40e57f8a08809be5806ab45204251978ccf7d76830cb8efc69ed45`, retenido hasta 2026-11-18 (según metadatos de GitHub).
- Commit exacto del script probado: `9c69879f7be041db1cb8e5c6811be305dbc5c409`.

## Resultados medidos

1. Instalación, inicialización no interactiva, modo A, daemon y modelo semántico: **PASS**.
2. Persistencia de diez hechos públicos de Eldoria: **10/10 guardados**; los siguientes procesos de CLI los recuperaron sin compartir memoria de proceso.
3. Búsqueda inicial: **19/20** respuestas incluyeron el dato esperado (prueba de subcadena, **no** evaluación semántica completa).
4. Pregunta fallida: «¿Qué resolución horizontal móvil se utiliza en QA?» no recuperó `844x390` en la primera formulación. **3/3** reformulaciones posteriores sí lo recuperaron.
5. Tres preguntas sin respuesta conocida: **2/3** indicaron explícitamente falta de respuesta exacta. La pregunta sobre una supuesta contraseña devolvió recuerdos no relacionados sin aviso suficientemente claro. **No fiarse de la abstención automática en temas sensibles**.
6. Cliente MCP Python independiente conectado mediante stdio al mismo almacén: **PASS**. Se enumeraron **19 herramientas** y el MCP `recall` devolvió correctamente `6000.3.23f1`, previamente guardado mediante CLI. Sin error de herramienta.
7. Mediana medida en la segunda ejecución para 20 consultas CLI: aproximadamente **0,465 segundos**; depende del entorno, no extrapolar al PC del propietario.

## Límite de la prueba

Esto valida memoria, búsquedas y acceso MCP en entorno temporal, **no** memoria compartida efectiva entre todos los chats de ChatGPT, ni conexión a un PC personal, ni seguridad completa, ni uso de herramientas por un agente autenticado. El almacén de prueba era efímero, solo contenía datos públicos y no se transfirió a Eldoria. GitHub sigue siendo la autoridad de decisiones.

## Integración Unity — evidencias anteriores cotejadas

- Editor Unity real, proyecto temporal: Unity CLI + Pipeline crearon cubo, comprobaron MeshRenderer y obtuvieron dos PNG nativos. https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41478984732 — PASS técnico.
- Codex 0.166.0 y plugin Unity 0.1.8-beta: instalados en sesión efímera, https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41486031681 — PASS de instalación.
- **No** demostrado: una sesión autenticada de Codex modificando una escena artística real de Eldoria. Ver https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/147 y restricciones del ejecutor Windows.

## Decisión prudente

**SuperLocalMemory: candidato prometedor para un piloto de integración controlado, no aprobado para producción.** Hay que exigir referencias a los documentos canónicos, cuestionar respuestas de baja confianza, probar contradicciones/correcciones e identificar permisos de conexión privada a ChatGPT. La integración Codex–Unity necesita autenticación privada y una reserva nativa realmente libre. No ejecutar OAuth mediante logs de repositorio público, no modificar PR #136 y no publicar cambios del juego a partir de este ensayo.
