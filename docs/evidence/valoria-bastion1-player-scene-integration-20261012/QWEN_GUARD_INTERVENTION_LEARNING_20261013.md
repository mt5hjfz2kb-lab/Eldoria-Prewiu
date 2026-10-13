# Bastión I — Puesto de guardia occidental: decisión IA, producción y aprendizaje

**Estado: MEJORA ARTÍSTICA LIMITADA CONFIRMADA / NO 6-10 / NO PROMOCIÓN A MAIN**. Fecha de prueba: 2026-10-13 UTC. Presupuesto 0 €. Esta es una intervención focal sobre la escena preservada, no una reconstrucción, ni un modelo reentrenado.

## Identidades verificables
- Línea base ambiental conservada: run nativo Unity [38471224337](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38471224337), calidad estimada previamente 5,2/10. La vegetación seleccionada, la textura de suelo aceptada, camino, escalera y puertas permanecen.
- Inferencia artística **real**: [38475192811](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38475192811), Ollama local **qwen3-vl:4b-instruct**, respuesta terminada, 130,31 s. El agente recibió briefing espacial, antecedentes de fallos y contexto de cámara en texto; **no** se demuestra que observara directamente las imágenes. Artefacto original `bastion1-qwen-architecture-proposal-38475192811`, ID **11853642983**. SHA256 del JSON **96feeed43029b41ecb82db196c4fdd6ec7e937ad9ec7d884d2d7f0f8dbac87bb**.
- **Decisión concreta de Qwen**: pequeño centro funcional y visual de guardia/suministros junto a la fortificación separada occidental, cerca de Blender x≈-20,y≈-12; reutilizar `defensive_props`, `stone_path_platform` y material/ruinas defensivas, dejando libres eje principal y futuras construcciones. No eligió vértices de la cubierta, ni escribió mallas.
- **Ejecución Blender determinista y claramente atribuida**: `tools/valoria-bastion1-assembly/compose_bastion_guard_station.py`, que conserva íntegros los originales y duplica mallas/instancias ya existentes. El criterio arquitectónico inicial proviene de Qwen; la geometría de madera y la corrección de escala y cubierta fueron decisiones técnicas y revisión visual posteriores, **no una autoría geométrica de Qwen**.
- **Candidato final de la rama**: commit [c4c25042a3fc67765fc1653a09a8d8c893af9d8d](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/commit/c4c25042a3fc67765fc1653a09a8d8c893af9d8d); Unity [38477299159](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38477299159) **SUCCESS**. Capturas PNG A/B nativas y encuadre móvil: artefacto `bastion1-unity-review-frames-38477299159` ID **11856320043**. GLB resultado SHA256 `e91bfeb69112cdf300f6a2928d068c15c54673cce2b695c3f578d013c116500c`. El GLB y la escena compilada están en el artefacto del runner, no se han incorporado a `main`.

## Dos pruebas auténticas, cámara bloqueada

| Etapa | Unity run | Píxeles modificados (>5 RGB, 1440×1000) | Juicio |
| --- | --- | --- | --- |
| Primera señal/puesto mínimo con material defensivo reutilizado | [38476307982](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38476307982) | 5.105 / 1.440.000 = **0,3545 %** | **ART REJECTED**. Demasiado pequeño y parece un grupo de props sin un volumen arquitectónico. |
| Corrección: una cubierta a dos aguas de madera con pórtico y apoyos; mallas defensivas preservadas | [38477299159](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38477299159) | 14.233 / 1.440.000 = **0,9884 %** | **Mejora visual localizada**: la cubierta define un puesto reconocible desde la cámara real, sin desplazar los protagonistas existentes. |

La segunda prueba produjo **3.732 / 405.600 = 0,9201 %** píxeles modificados en el encuadre móvil nativo **780×520**. Los ficheros `*.camera.json` de BEFORE/AFTER son **idénticos byte a byte** tanto a 1440×1000 como a 780×520, verificando igualdad de cámara. Un porcentaje de cambio **no certifica calidad artística**.

## Reutilización, reserva de espacio e impacto

- Propiedades Blender: **7 mallas de `LowerGatePracticalSet`** y **7 de pavimento `CentralRoad_-4`**, copiadas con UV/materiales originales; **13 pequeñas mallas nuevas de entramado de madera**, incluidas **dos cubiertas inclinadas**, para reunir los elementos en una unidad. No hay nuevos recursos, colliders, nuevos edificios jugables ni modificación de geometrías originales. El terreno original tiene contacto por raycast (z≈0).
- Posición x=-20,4, y=-12,0 Blender, vecina a `LeftDetachedRemnant` y `LeftDetachedPier`. Margen horizontal calculado al eje axial protegido (7 m de semianchura) **>10,2 m**, ya descontada la anchura del conjunto. Fuera de las parcelas de aserradero y cuartel según centros y volúmenes del editor. Camino y escaleras permanecen visibles.
- Unity Editor/capturas terminados sin fallo en el workflow; **no** equivalen a un ensayo completo de las mecánicas Bastión I→II en este commit, ni a FPS/memoria/temperatura certificados en Android o iOS. No se ejecutó promoción pública ni publicación.
- **Valoración visual conservadora**: punto de partida ≈5,2/10; resultado en torno a **5,4–5,5/10**, sujeto a evaluación humana independiente. La escala y el techo dan legibilidad medieval pero el conjunto aún es modesto; la explanada, contacto del suelo, lateral del acantilado, profundidad e iluminación global siguen limitando la percepción. **No declarar 6/10 ni ART PASS global**.

## Retroalimentación de aprendizaje reutilizable
1. Un prop reutilizado y una señal de 2–3 m pueden compilar sin errores pero solo alterar 0,35 % del encuadre: **desestimar** microintervenciones sin masa, silueta ni agrupación legible.
2. Cuando el agente elige *guardia junto a muralla*, materializar ese concepto con pórtico y cubierta coherentes aumenta el impacto visual a aproximadamente **0,99 %** sin llenar el patio. Conservar como **candidato útil**, no como solución final de calidad.
3. Mantener la autoría separada: **Qwen eligió concepto, familias y emplazamiento**; **Blender y revisión posterior concretaron cubierta y proporciones**; Unity proporciona la evidencia, no una puntuación artística automática.
4. Una propuesta verbal de Qwen **no demuestra entrenamiento ni cambio de pesos**. El aprendizaje del sistema consiste aquí en una memoria externa verificable de decisiones, resultados y rechazos. El próximo agente debe leerla como contexto y volver a comprobar el nuevo A/B antes de atribuir mejora.
5. Próxima corrección artística, si se solicita: unión material y geométrica del pórtico con el muro occidental, variación PBR del techo y sombreado de contacto; no repetir decoraciones minúsculas, no tocar parcelas, no reconstruir la escena ni modificar `main`.

**Resultado entregado de esta misión:** propuesta IA auténtica + autoría Blender separada + dos Unity A/B reales + capturas móviles + conservación del mejor candidato de integración sin tocar el juego publicado. **Limitación:** falta validación del loop jugable y perfilado físico móvil del artefacto exacto; no declarar release readiness.
