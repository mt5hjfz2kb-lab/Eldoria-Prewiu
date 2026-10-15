# ELDORIA — Arquitectura de ojos, geometría y cerebro artístico IA (validación V1)

**Fecha:** 15-10-2026. **Presupuesto:** 0 €. **Estatus:** capa de diagnóstico y transferencia segura **IMPLEMENTADA Y PROBADA**; artista profesional que puede editar **NO VALIDADO**. Nada se publica ni se modifica en la malla/Unity. **Autoridad:** último `main/AGENTS.md` mantiene pausa de producción.

## Arquitectura resultante y proveedores realmente operativos

1. **Ojos:** `qwen3-vl:8b-instruct-q4_K_M` en Ollama/Windows, vista por vista, como generador de observaciones **hipotéticas**. No se trata su texto como geometría verificada. Tres PNG auténticos de la misma torre SHA original.
2. **Medición:** Blender del PC produce geometría WORLD XYZ y vectores reales, independiente de Qwen. `tools/art-direction/vision-diagnostic/spatial_grounding.py` y `tools/art-direction/director-control/roof_direction_probe.py`; fuente original inmutable SHA `e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`.
3. **Cerebro:** **nuevo modelo local Qwen3 14B** (texto/razonamiento, NO tiene ojos), descargado gratis de Ollama modelo Apache-2 Q4 para el PC con RTX 3060 de 12 GB. `strong_local_director.py` recibe mediciones verificadas y observaciones con su incertidumbre; `grounded_director_benchmark.py` conserva baseline 4B/8B. No tiene ninguna herramienta de modelado directamente.
4. **Revisor/gate independiente:** `tools/art-direction/director-control/evidence_gate.py` y doce pruebas automáticas: verifican SHA, cámaras, fuente, zonas protegidas, limitaciones de contacto y prohíben órdenes de Blender. La evaluación estética independiente sigue separada. `build_diagnostic_handoff.py` lleva la respuesta validada de 14B a `NO_EDIT` seguro y deja hashes para auditoría.
5. **Ejecución artística:** sigue PAUSADA por instrucción del propietario. `ART_APPROVED = false`; ninguna prueba nueva alteró la torre ni los assets Unity.

## Pruebas exactas ejecutadas

| Evidencia | Ejecutado | Resultado |
|---|---|---|
| [38699407600](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38699407600) | 3 vistas originales + referencia; percepción 4B/8B | Capturas auténticas y descripciones de un asset; juicio profesional insuficiente |
| [38701710879](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38701710879) | Blender original WORLD y prueba adversarial 4B/8B | 7/7 reglas explícitas en ambos; fallos en síntesis artística libre |
| [38703106394](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38703106394) | Gate independiente | 12/12 tests PASS (fail closed); no hay autorización artística |
| [38703423820](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38703423820) | Inventario real de Ollama y GPU | Solo 4B, 8B y variantes pequeñas; RTX 3060 12 GB |
| [38704030602](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38704030602) | Descarga GRATUITA Qwen3 14B + prueba de dirección espacial | Modelo local instalado; 8/8 casos explícitos; en respuesta libre cometió clasificación de tejado no demostrada y dijo NO_EDIT |
| [38705235631](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38705235631) | Nuevos normales geométricos Blender + autocrítica de 14B | Áreas inclinadas en X+, X-, Y+, Y-, corrección explícita del error de certeza sobre «dos aguas»; NO_EDIT |
| [38705666442](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38705666442) | Transferencia completa director 14B → propuesta → gate independiente | Gate PASS; salida `NO_EDIT_INSUFFICIENT_EVIDENCE`, cero órdenes ejecutables, `art_approved=false` |

## Resultados métricos relevantes

- Caja WORLD m: `[2.44774, 2.65, 3.27077]`; 1 malla importada, 11.400 vértices, 5.338 polígonos, 3.029 islas topológicas, UV original intacta.
- Inclinaciones superiores: 893 triángulos en la franja alta, estimadas por geometría real del GLB.
- Superficies inclinadas upper por direcciones WORLD: `X+ 2.68314 m²`, `X− 2.66940 m²`, `Y+ 2.30833 m²`, `Y− 2.30971 m²`. **Estas áreas no certifican por sí solas el número de faldones del tejado**, porque pueden incluir tablas y otras superficies.
- Qwen3 14B reconoció en la última comprobación que **no estaba justificada** su afirmación anterior de tejado a dos aguas, que perspectivas diferentes no cambian el GLB y que no puede autorizar una corrección sin comprobar geometría de contacto.
- Fuente de corrección final `qwen3-14b-roof-independent-correction.json` SHA-256 de respuesta: `32e96e5004d8d56038139c4e735fa6aeae94df14296f666d6fc5ed44734bca12`.
- Handoff final `director-no-edit-handoff.json`: `mode DIAGNOSTIC_ONLY`, `editor_actions=[]`, `decision NO_EDIT_INSUFFICIENT_EVIDENCE`.
- Verificación final `independent-evidence-gate-result.json`: `EVIDENCE_REFERENCES_VALID_DIAGNOSTIC_ONLY`, `can_edit_blender=false`, `art_approved=false`.

## Aprendizaje obtenido y reglas para reutilización

**Lección específica verificada:** NO dar por correcto lo que describen los ojos VL ni la primera síntesis del director. La información de Blender sobre coordenadas/normales de la fuente y la procedencia de cámara tiene prioridad. Un director que asegura haber identificado un tipo de cubierta a partir de dimensiones y conteos de triángulos no es fiable. Pedir crítica contra mediciones, conservar limitaciones y **negar ejecución** ante incertidumbre sí ha producido una autocorrección significativa en el 14B.

**No hay reentrenamiento de pesos:** se ha instalado un modelo más capaz *candidato* y se han conectado fuentes/evaluaciones; no se puede concluir aprendizaje permanente del modelo ni que ya produzca obra nivel 8/10. La memoria reutilizable está en estas evidencias y reglas.

**Fase de ingeniería cerrada:** ojos separados, medición espacial, director mayor, controles y transferencia segura probados a coste 0. **Fase artística pendiente:** autorización del propietario para levantar parcialmente la pausa y validar UNA corrección sobre copia + A/B de Blender + Unity + revisión independiente; después tres mejoras sucesivas según contrato. No recomendar edición concreta ni integrar 14B en el ejecutor productivo sin esa validación.

**Prohibido sustituir por una afirmación de éxito técnico la falta de aprobación artística.**
