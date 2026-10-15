# VALORIA — revisión independiente de la dirección IA fundamentada en geometría

Fecha: 2026-10-15. Branch: `diagnostic/valoria-ai-eyes-brain-20261015`.

## Ejecución real revisada
- GitHub Actions [38701710879](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38701710879) — completado SUCCESS técnico.
- Artefacto: `valoria-spatial-grounded-art-director-4b-8b`, ID `11956448484`.
- Fuente original protegida SHA-256 `e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`.
- Grounding original medido SHA-256 `93f226d8d389f30a745e0b8c8502c79633caa05c19f379214c231889c6ddf317`; malla original única, 11.400 vértices, 5.338 polígonos, 3.029 islas topológicas, un UV set y 893 triángulos inclinados en el estrato superior (por triangulación sin cambiar origen).
- Comparación de ambas Qwen con SIETE casos espaciales sencillos: **4B 7/7, 8B 7/7**. Mejora visible de adhesión a reglas explícitas sobre identidad de GLB, protección de escalera/puerta/UV, cubierta inclinada y contacto real.

## EVALUACIÓN INDEPENDIENTE: FALLA calidad de razonamiento artístico libre

**4B:** en la síntesis libre volvió a afirmar que una cubierta a dos aguas no es compatible con las dimensiones del edificio porque la zona superior tiene 893 triángulos inclinados, y que dicho dato implica un plano único. Esto es un error geométrico: el número de triángulos inclinados y la caja exterior no determinan cuántas vertientes hay. También interpretó como incompatibles distintas descripciones visuales de la misma torre, a pesar de aceptar lo contrario en la batería de 7 pruebas.

**8B:** la respuesta libre corrigió la afirmación más obvia de cubierta forzosamente plana y mantuvo la escalera, pero trató de invalidadar aleros visibles por considerarlos no demostrados sin un análisis semántico real; sugirió cambios de iluminación cálida de antorchas/ventanas tomando detalles de la **imagen de estilo**, no constatados en la torre original de tres renders neutrales. Esa propuesta todavía carece de evidencia en la pieza a corregir. Las 893 caras inclinadas tampoco demuestran un tipo exacto de tejado.

**La contradicción central:** aprobar 7/7 afirmaciones dirigidas con reglas suministradas **no garantiza** que el mismo modelo razone correctamente y de forma autónoma al diseñar. Ninguno supera todavía el criterio profesional de dirección artística. Esta verificación es independiente de las respuestas autoevaluativas de Qwen y del estado SUCCESS del job.

## Estados objetivos
- `SPATIAL_SOURCE_GROUNDING_VERIFIED`: SÍ. Geometría real medida sobre original; procedencia y 3 cámaras verificadas.
- `EXPLICIT_RULE_ADHERENCE_7_OF_7`: SÍ ambos. Es una mejora técnica limitada, NO un diagnóstico artístico.
- `AUTONOMOUS_PROFESSIONAL_ART_DIRECTION`: **NO, REJECTED BOTH**.
- `EXECUTION_SAFETY`: no hay ninguna edición de Blender realizada ni aprobada en este ciclo.
- `ART_APPROVED / CALIBRATION_VALIDATED`: NO.

## Dirección siguiente, sin repetir por repetir

1. Incorporar validación cruzada de cada **afirmación artística libre** contra hechos y límites del GLB, y rechazar propuestas que confundan referencia, perspectiva y realidad del activo.
2. Obligar a adjuntar evidencia por cámara/imagen y citas de geometría a cada hipótesis; falta de prueba = `NO_EDIT`, no una orden al modelador.
3. Mantener revisión independiente de autor; si solo disponemos de Qwen 4B y 8B, no afirmar disponer de un director superior instalado. Para conseguir mejor criterio autónomo habrá que validar un modelo o servicio nuevo, sin asumir gratis ni aprobar descargas/gastos. Mientras tanto, usar el informe independiente de ChatGPT como contraste y el gate de protección.
4. No reanudar producción artística ni pedir tres mejoras A/B hasta comprobar un diagnóstico arquitectónicamente correcto. No reentrenar ni confundir memoria transmitida con pesos modificados.

**Conclusión:** El trabajo de ingeniería de la capa espacial ha funcionado, pero el salto a un artista profesional **sigue sin estar probado**. Se mantiene la pausa del propietario.
