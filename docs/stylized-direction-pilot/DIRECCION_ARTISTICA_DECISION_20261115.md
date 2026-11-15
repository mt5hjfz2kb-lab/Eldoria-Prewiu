# Eldoria — decisión técnica/artística tras los A/B reales (15/11/2026)

**Dirección elegida:** estrategia medieval oscura con acabado **2.5D pictórico estilizado**, cámara elevada controlada, modelos 3D legibles y materiales compartidos. Adulto y atmosférico, **no realista fotográfico, no caricaturesco, no imágenes enteras pegadas como escena**.

**ESTADO:** propuesta de arte + ensayos nativos Editor, NO calidad comercial validada. `ART_APPROVED=false`. No cambiar main ni beta P0. PR #196 en borrador.

## Ensayos reales y conclusiones independientes

| Experimento | Evidencia | Resultado |
|---|---|---|
| B shader alternativo global | [43723401749](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43723401749) | RECHAZADO: meshes desaparecen aunque proyectan sombra |
| B/C tintes y atlas de color | [43723908844](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43723908844) | RECHAZADO: diferencia pequeña; no arregla silueta/composición |
| D decorado modular aleatorio | [43725103057](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43725103057), [43725656960](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43725656960) | RECHAZADO: piedras desproporcionadas sin contacto visual y árboles inconsistentes |
| F suelo artístico, **sin props rechazados** | [43726672716](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43726672716) | Editor real PASS, A/F mismas cámaras 1600×900 y 390×844, 7 capas del terreno. Mejora cohesión de color **pero insuficiente en calidad global**. No integrado. |
| H versión atmosférica del F | [43727390656](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43727390656) | Editor real PASS A/F/H. La iluminación más cinematográfica no corrige carencias fundamentales. **No promover por sí sola.** |

**Causa dominante:** no basta un cambio del color o postprocesado; faltan macroformas artísticas, transiciones de suelo a arquitectura y vegetación, decoración al servicio del relato, y un kit 3D único con escala, rugosidad y tipología coherentes.

## Nuevo objetivo visual (referencia ilustrada no técnica)

Concepto original de **Bastión temprano en reconstrucción**, ciudad compacta con piedra envejecida, cedros por familias, ruinas **integradas al suelo**, un camino continuo y puente, antorchas ámbar, ribera orgánica, una señal de Brecha violeta lejana. Cámara elevada 3/4; vegetación y arquitectura de siluetas claras; sin gran ciudad artificial ni filas repetitivas de props. La ilustración generada por IA en la conversación del 15/11/2026 **no es escena Unity** ni demuestra rendimiento.

**No copiar literalmente la imagen**: respeta Bastión I–V, edificios desbloqueados por progreso y la excepción del propietario de eliminar cuatro parcelas. El monumento antiguo debe ser una forma 3D con geometría y autoría propias, no un cartel o foto; prohibido el Bastión heredado vetado.

## Siguiente piloto: **UN solo kit coherente**, no más pasadas de filtros

1. **Bloquear una zona real (Bastión y camino hacia puerta):** capturar cámara Editor actual con marcas de las partes vacías que sí deben ocupar geometría. Mantener puntos interactivos, saves y escenario sin cambios.
2. **Auditar recursos reutilizables** (catálogos `tools/asset-vault/find-reuse.py`, licencias y fuentes): Bastión autorizado actual, sillería, 1 arco imperial roto, 2 rocas (pequeña/mediana) que coincidan con ribera, 2 familias de coníferas y 1 kit camino/semisuelo. No gastar créditos ni incorporar licencias desconocidas.
3. **Autoría Blender/Unity en piezas modulares:** variación de silueta y repetición sin patrones; escala relativa respecto al Bastión; piedra envejecida gris/beige **sin rocas blanco puro**; vegetación verde pizarra con un único modelo de iluminación; transiciones naturales entre suelo, muro y orilla. No crear otra zona completa de forma automática.
4. **Preview nativo a la cámara REAL** en una rama aislada con A/B en 1600×900 y 390×844. Escena legible, funcional y estética sin maquillaje. Objetivo evaluación humana ≥7,5/10 respecto a referencia, ninguna pieza infantil/disociada.
5. **QA incremental:** después de la aprobación visual comprobar colliders, botones/hotspots, zoom cercano/regional, memoria y WebGL iPhone; no exportar beta ni fusionar antes de pasar los gates.

## Controles de producción

- **Autoridad:** solo `Unity/Assets/Eldoria/StylizedDirectionPilot/**` y paths del workstream; PR #194 P0 mundo e interfaz no son propiedad de este ensayo.
- **Documentación reproducible:** SHA + Run + 2 orientaciones + A/B + observaciones concretas; diferencias de píxeles no puntúan calidad. Los assets nuevos requieren fuente, procedencia/licencia, GUID, LOD, y proyección Unity con material.
- **Regla de pivote:** tras dos intervenciones menores sin salto apreciable, parar ese enfoque y reconstruir macroformas o reemplazar la familia de materiales; sin nuevas decenas de filtros y sin declarar `ART_APPROVED` automáticamente.
