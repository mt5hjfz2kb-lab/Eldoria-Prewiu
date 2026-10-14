# Eldoria — artista IA real en PC: continuidad verificable, 2026-10-14

**Tipo:** evidencia técnica y artística independiente. **No constituye aprobación de arte ni reanudación general de producción.**

## Autoridad y alcance
- Repositorio `mt5hjfz2kb-lab/Eldoria-Prewiu`; rama de trabajo **`validation/eldoria-professional-art-fixed-source-20261014`**, último HEAD revisado `0522be64aee841bb3c641d8fa087367841ab9839`.
- `main` y la fuente original de Valoria **NO** se han alterado por los ciclos creativos. `pipeline/active-workstreams.json` en main mantiene `owner_production_pause.active=true`. Únicamente validaciones artísticas aisladas autorizadas; no publicar WebGL/APK ni iniciar producción Unity.
- Presupuesto 0 €. Ningún servicio o crédito de pago ni modelo descargado.

## Conexiones reales probadas
- Runner Windows privado `[self-hosted, windows, unity-6000-3-23f1]` y **Blender 5.2.2 LTS instalado en PC**. El usuario tenía razón: **Ollama y Qwen están instalados en el equipo**. El daemon fue iniciado localmente en pruebas aisladas donde no estaba ya activo; eso no significa ausencia de instalación.
- `eldoria-artist:latest` y `qwen3-vl:4b-instruct` han producido decisiones artísticas locales. Complemento **BlenderMCP instalado** verificado con SHA-256 **`eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5`**. La cadena Qwen real → BlenderMCP → malla heredada/renders/.blend/.glb funciona. Referencias: [prueba de addon #38612645467](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38612645467), [prueba local artista #38612942968](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38612942968), [modelo personalizado #38613113931](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38613113931).
- Taller técnico con operaciones de malla, PBR y UV y operador BMesh de aristas seleccionadas probado en Blender físico Windows [#38617149810](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38617149810).

## Bloqueos ya corregidos
1. El workflow nuevo buscaba el nombre **equivocado del artefacto** del run `38621127420`. Evidencia [fallo #38623005262](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38623005262). Workflow actual recupera `eldoria-custom-artist-pc-source-grounded-multitool-proof`, comprueba SHA del .blend heredado y evita repetir autorías previas aprobadas técnicamente.
2. El modelo intentaba crear objeto con un nombre heredado; [fallo #38623143225](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38623143225). Ya existe namespace reservado para Qwen, con nombre elegido por la IA, vínculo exacto de acabados y **prefijo distinto por ciclo** (`Eldoria_Added_C01_...`, `Eldoria_Added_C02_...`).
3. Modelos con mallas de 2 triángulos planos, aleros abiertos, múltiples caras sin volumen, aristas no manifold y biseles globales que arruinaban la fuente: ahora se rechazan en preflight con razones y respuestas originales intactas. Hay validación de volumen, original source facts, índices reales de caras/aristas, topología cerrada de la pieza nueva y crecimiento de polígonos.
4. En un intento, Qwen 4B no lograba cerrar la malla manual en 5 revisiones, aunque pudo crear después una primitiva válida; véase [#38625910113](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38625910113). Tras un rechazo no-manifold/área cero se habilita **únicamente la herramienta alternativa de primitiva volumétrica** para esa revisión; la IA elige tipo, coordenadas, escalas y materiales sin que un script invente geometría.

## Resultados y revisión independiente

| Ejecución | Resultado técnico | Juicio artístico independiente |
|---|---|---|
| [#38624160683](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38624160683) | SUCCESS; nueva malla Qwen, PBR y UV; 2 ciclos aceptados por gate técnico; artefacto `11922962418` | NO APTO: plataforma plana de 2 triángulos y cambios poco relevantes |
| [#38625147034](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38625147034) | SUCCESS; nuevo volumen 3D, dos ciclos; artefacto `11922903867` | NO APTO: forma triangular y fragmentos sin soporte convincente |
| [#38625910113](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38625910113) | FAILURE seguro; primer ciclo crea un cubo real; 5 propuestas posteriores rechazadas, artefacto `11924093112` | NO APTO: bloque de 1×4×1 metros invade visualmente puerta/escalera; no fusionar |
| **[#38626949838](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38626949838)** | **SUCCESS; 3 ciclos completos Qwen → BlenderMCP, 3 objetos nuevos con nombres independientes C01/C02/C03, PBR/UV y conservación de fuente; artefacto `11924805847`** | **RECHAZADO:** ampliación cúbica masiva tapa puerta, cilindro bautizado «chimenea» aparece lateral como volumen horizontal, supuesta pieza de tejado es otro bloque. CI verde NO es arte profesional |

Última fuente original intocable recuperada: SHA-256 `ce2590684b9a38e7a473f0831d6df1cdb379099cd3af379458c6c0829a7ae960`. Último resultado de Blender no aprobado (tercer ciclo del run `38626949838`): SHA-256 `.blend` `1e62dec601880f65f5e26f5b120f4c274da6558389c9ec145a052779fd6770e0`, SHA-256 `.glb` `252e1c9bc477f7f4ef24f22f67f1cfd88775aff6d67a4761b63ec3630f754599`. `art_approved=false`, `unity_art_verified=false`, `physical_mobile_validated=false`. Evidencia en artefactos GitHub Actions; **retención temporal**: conservar/copiarlos expresamente si se necesitan pasado su vencimiento.

## Diagnóstico y siguiente etapa correcta

**Conectividad y selección/ejecución de herramientas: VALIDADAS.** Capacidad para generar geometría 3D real y editar modelos: VALIDADA. **Autonomía de criterio espacial/estético profesional: NO DEMOSTRADA.** Qwen `eldoria-artist:latest` continúa eligiendo posiciones, proporciones y relaciones arquitectónicas deficientes incluso con source inspection y varias iteraciones. La autocrítica local de Qwen responde a veces «no defects» a pesar de defectos graves; jamás sustituye revisión visual independiente.

No repetir los ciclos básicos de conexión/instalación ni más «SUCCESS» sin mejora. Próximo ensayo artístico debe priorizar **comprensión espacial de la escena**: vistas ortogonales con coordenadas reales, bounding boxes locales/mundiales, protección de accesos/escaleras y comparación referencia/candidato desde cámara final; una crítica visual independiente que rechace explícitamente nuevos bloques sin coherencia. Se puede estudiar un modelo local gratuito más capaz **solo después de verificar hardware y licencia**. No adjudicar puntuación artística positiva sin ver renders A/B. No introducir formas preseleccionadas por scripts, no reconstruir el proyecto, no invadir Bastión I/4X ni promover el candidato actual.

**Estado de cierre:** `BLENDER_PC_MCP_INTEGRATION_PASS`, `QWEN_MULTICYCLE_AUTHORING_PASS`, `ARTISTIC_PROFESSIONALISM_FAIL`, `NO_PRODUCTION_PROMOTION`.
