# ELDORIA — Diagnóstico de causas de fallo del artista (15-10-2026)

**Tipo:** análisis reproducible de solo lectura. **No altera los activos del juego, el artista, los workflows ni las reservas del runner.** La pausa y los contratos de `main/AGENTS.md` y `main/docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md` siguen vigentes. No significa `ART_APPROVED`.

## 1. Resultados y denominador exacto

12 ejecuciones de GitHub Actions denominadas **Eldoria one frozen-tower copy visual art calibration** en la rama `diagnostic/valoria-ai-eyes-brain-20261015` del 15-10-2026: **2 SUCCESS técnico y 10 FAILURE**. No hay ningún encadenamiento de tres mejoras artísticas aceptadas ni aprobación visual Unity. Un SUCCESS de workflow no evalúa estética.

Clasificación conservadora de diez fallos tras leer sus logs:
- 38743202194: JSONDecodeError / formato de datos del pipeline (1).
- 38747114932: ImportError de `check_triangle_growth`, incompatibilidad entre módulos del estudio (1).
- 38750612414: Qwen genera operación `geometry` inexistente para el ejecutor, tres rechazos (1).
- 38747182257: propuesta enterrada en geometría original y después interfiere con acceso/escalera (1).
- 38757106669 y 38758741311: el esculpido cambió datos de malla pero fue invisible en cámara (2).
- 38743534741, 38744314100, 38744349120, 38745685946: el workflow registra rechazo del estudio; no adjudicar causa más concreta sin inspeccionar los artefactos originales (4).

**Conclusión:** no son diez fallos del transporte MCP. Hay errores de serialización, de contrato entre módulos, de decisiones espaciales y de juicio visual.

## 2. Evidencia visual que explica los TECH PASS

Run [38754996414](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38754996414), artefacto 11982678775: `execution-proof.json` declara `transport=direct`, `blender_execution=true`, cuatro acciones genuinas (`primitive` x2 y `material` x2); `inherited_mesh_data_changed=false`. El plan bruto dice crear **dos vigas de madera**. Los PNG exactos muestran un bloque beige grande de aspecto similar a chimenea, superpuesto a la pared, sin mejora convincente. `camera-screen-impact.json` cuenta 17.754/691.200 píxeles distintos (2,56858 %); eso demuestra visibilidad, **no calidad**. El revisor Qwen-VL marca `clearly_failed=true` y describe el bloque como chimenea. `ART_APPROVED=false`.

Run [38758741311](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38758741311), artefacto 11984912039: ambos `sculpt` modificaron realmente vértices heredados con Blender directo, pero cambiaron 878/691.200 (0,12703 %) y 491/691.200 (0,07104 %) píxeles, frente al mínimo operativo 0,6 %. Falló por `SOURCE_SCULPT_NOT_VISIBLE_FROM_REVIEW_CAMERA`. El espacio de acciones de ese flujo se restringe a **un único sculpt** por intento; un esculpido invisible no prueba que Blender o el MCP estén averiados.

## 3. Qué enseñan los siete assets aceptados anteriormente

Fuentes: commit [0abf138](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/commit/0abf13870d21725c30acf3f3e3a8c86385470073), `tools/art-rd/valoria_family_v2_1_qwen_art_director.py`, `tools/art-rd/valoria_family_v2_1_mcp_author.py`, `art-source/valoria/production/asset-family-v2/quality-v3/visual-review.json`.

Qwen3-VL 4B recibía un **tablero de imágenes real** y elegía de un vocabulario de operaciones **semánticas de arquitectura** (`bonded_masonry`, `tapered_buttresses`, `v3_complete_tower`, etc.), con proporciones en listas acotadas. El autor especializado traducía esos deseos a estructuras 3D en Blender. Se reaprovechaban fuentes existentes: el `watchtower/evidence.json` declara `v3_complete_tower`, **374 piezas reutilizadas** y traslado afín, no 374 piezas diseñadas libremente por Qwen. Siete fuentes llegaron al mínimo **Puerta V3**, sin certificación Unity.

Esto demuestra un producto útil de **dirección generativa + biblioteca de habilidades + reutilización de arte**, no demuestra que aquel Qwen4B pudiera elegir correctamente vértices arbitrarios de una malla heredada y esculpirlos desde cero. No es un A/B equivalente a las pruebas nuevas.

## 4. BlenderMCP frente a direct: lo que sí se verificó

La prueba [38612645467](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38612645467) prueba `transport=blendermcp`, addon instalado con SHA fijado, edición de la malla heredada real, sin aprobación artística. Run 38754996414 prueba `transport=direct`, Blender real y edición de objetos. **Ambas vías funcionan**, pero se usaron planes diferentes y NO constituyen una comparación causal rigurosa.

En `tools/art-direction/full-studio/blender_toolbox.py`, direct ejecuta `apply_known_action(act)` y la rama MCP invoca `execute_via_blendermcp(plan['actions'], apply_known_action, ...)`. La creatividad está en el **plan anterior al transporte**; ambas vías recurren a la misma implementación de acciones. Activar MCP no ampliará por arte de magia el vocabulario que Qwen recibe en su esquema. Sí conviene conservar el transporte y probar su latencia/robustez con **el MISMO plan, SHA fuente, versión Blender y cámara** cuando el runner esté libre y autorizado.

## 5. Hipótesis prioritarias y remedios concretos

**P0 — Espacio de acción empobrecido y contratos desalineados.** El artista antiguo disponía de habilidades semánticas elaboradas; el diagnóstico nuevo en `tower_source_refinement.py` solo permite `sculpt` sobre vértice/fuente. Reunir un catálogo fiel a lo que ejecuta Blender: operaciones atómicas + habilidades arquitectónicas paramétricas, cada una ligada a un esquema formal, prueba real, límites y resultado esperado. Qwen conserva selección de intervención, proporciones y lugar. No codificar una torre final como plantilla; no sustituir autoría por geometría falsa.

**P1 — Ojos desconectados del último resultado.** Qwen14B de texto opera sobre descripciones de Qwen-VL, hashes y coordenadas; no ve directamente las imágenes. El Qwen3-VL 4B antiguo veía tableros compuestos. Entregar al modelo visual imágenes auténticas **actuales** (objetivo, antes, después, detalle, máscara del cambio y vista lateral/superior), con revisión anclada en el plan ejecutado. El director 14B usa diagnóstico con incertidumbre y verdad geométrica de Blender. Conservar modelo y plano congelados para probar comparabilidad.

**P2 — Falta verificación semántica entre intención y forma.** En 38754996414 se pidieron `dos vigas`, pero el resultado aparenta un bloque vertical. Rechazar si el ensamblaje no responde a la intención, aunque cambie 2,56 % de píxeles. Revisor independiente debe cotejar etiquetas de objeto y plan con imágenes/geometry, no solo decir “algo cambió”.

**P3 — Falta de triaje previo al gasto en Qwen.** CI ha consumido inferencia para descubrir ImportError y JSON malformed. Antes de invocar el modelo: imports de ambos checkouts fijados por SHA, prueba de esquema `format` idéntica al validador, operador permitido por contrato, disponibilidad de herramientas, fuente exacta y zonas de contacto. Las pruebas técnicas baratas preceden al ciclo pesado.

**P4 — Umbral fotométrico no es juicio estético.** El 0,6 % descarta intervenciones muy pequeñas, pero no demuestra mejora; el 2,57 % del bloque beige es contraejemplo. Primero revisión de silueta/cámara y composición de la zona afectada, luego evaluación visual independiente y Unity solo para candidatos aceptables.

## 6. Secuencia experimental mínima falsable y de coste 0

1. **Sin Windows:** congelar fuentes y artefactos anteriores; auditoría de schema, de librería de herramientas semánticas y de planes antes de llamadas Qwen. Etiquetar cada fallo según fase: infraestructura → serialización → plan → ejecución → geometría visible → criterio visual.
2. **A/B de transporte (solo al estar libre Windows y con la misma autorización):** misma torre SHA y **mismo plan ya válido**, ejecutar direct/MCP en salidas temporales separadas, 3 repeticiones ligeras; medir errores de transporte, tiempo, geometría post-edición, hashes de operación, daño UV, imagen A/B de cámara fija. No tratar diferencias de bytes de renders estocásticos como arte.
3. **A/B de autoría:** dos modelos/rutas con **mismas imágenes, fuente y vocabulario de operaciones**: Qwen-VL4B imagen directa versus director 14B con observación VL y geometría verificada. No comparar “con habilidades semánticas” frente a “solo sculpt” como si el modelo fuese la única diferencia.
4. Una única mejora de silueta/arquitectura priorizada sobre copia; revisar contra el objetivo, rechazar daño, puntuar independientemente y usar cadena de SHA solo si fue realmente aceptada. No producir Unity ni publicar en esta fase sin autorización vigente.
5. Tras dos fallos de la misma familia, **cambiar hipótesis**; después de cinco, detener *esa estrategia* y revisar modelo/inputs/herramientas según contrato.

### Documentación externa contrastada
- Blender 5.2 Python API: https://docs.blender.org/api/5.2/info_quickstart.html
- BlenderMCP: https://github.com/ahujasid/blender-mcp
- Ollama formatos estructurados con JSON Schema: https://github.com/ollama/ollama/blob/main/docs/capabilities/structured-outputs.mdx

**Veredicto:** no existe prueba de que el transporte MCP sea la causa predominante de los rechazos. Sí existe evidencia específica de errores anteriores al transporte y de ediciones visualmente inútiles. La acción de más valor es rediseñar la comunicación **visión → intención → herramienta validada → geometría → crítica visual** sin tirar el estudio, la memoria o los siete assets.
