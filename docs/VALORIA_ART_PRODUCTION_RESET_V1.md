# VALORIA ART PRODUCTION RESET v1

Fecha: 2026-10-05. Fase: PREPRODUCTION. Resultado y evidencia final: `docs/evidence/valoria-art-production-reset-v1/session-result.json`.

## Autoridad y preservación

Este bloque reinicia exclusivamente el método de producción artística. Eldoria, su core loop, progresión, gameplay, navegación, escenas, fuentes Blender, GLB, pipelines y evidence anteriores se conservan. No se reinicia el producto ni se promueve esta escena al juego.

Inicio real: `main` **6c80311087b77f9ddb19b9c0eb2ba14249f3725a**. Worktree separado: `/workspace/scratch/ff1ba0ab7910/Eldoria`; rama local `work/valoria-art-production-reset-v1`. Escrituras canónicas mediante el conector GitHub, en main. Los cambios sin publicar de otros worktrees no se tocaron.

Nueva autoridad visual: `docs/evidence/valoria-art-production-reset-v1/canonical-target.jpeg`, copia de bytes intactos del adjunto `AF172EB3-ED81-48F6-9C34-F996E9FD6FE1.jpeg`. JPEG **1536×1024**, **702827 bytes**, SHA-256 **8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689**. Se conserva también la referencia anterior en su ubicación histórica.

La directiva expresa del propietario reemplaza, para esta nueva campaña de preproducción, las restricciones artísticas anteriores de Flat Citadel, ciudad densa, referencia no literal y cámara heredada. No reemplaza sus contratos funcionales. Los resultados anteriores se consultan como conocimiento, nunca se reabren automáticamente.

## Orden obligatorio

Concepto/pilares/alcance → core loop → prototipo barato/greybox → playtest → restricciones técnicas/pipeline → target reproducible → cámara/plataforma → blockout completo → asset families → una familia representativa → pipeline completo → vertical slice visual/jugable → escala.

**No fabricar assets para descubrir cómo debe verse el juego.** Esta sesión cubre target, cámara, composición y blockout. Los primeros cinco pasos aprovechan el producto/prototipo y evidencia ya existentes; no se afirma un nuevo playtest de este greybox. Ningún PASS aquí autoriza final art, cambios de gameplay, spend ni rollout.

## Cámara y plataforma

La imagen es 3:2, no 16:9. Se conserva el adjunto completo. La adaptación 16:9 abre horizontalmente el mismo encuadre vertical, sin estirar ni recortar las masas. `UNITY-source-3x2.png` es la comparación directa; `UNITY-16x9.png` es la adaptación. Las zonas laterales añadidas son contexto provisional, no una referencia artística nueva.

| Parámetro | Decisión para el greybox |
|---|---|
| Proyección | Ortográfica |
| Giro | 20° desde el frente −Z en Unity, hacia +X |
| Inclinación | 35° hacia abajo |
| Centro de encuadre Unity | (0,0,0) |
| Posición Unity | (30.8183,63.0934,−84.6726) |
| Distancia | 110 unidades, solo fija profundidad/clipping en ortográfica |
| Tamaño ortográfico | 24; altura visible 48 unidades |
| Clip | 0.1–400 |
| Comparación fuente | 1536×1024; anchura visible 72 unidades |
| Adaptación 16:9 | 1820×1024, aproximación por redondeo; 1280×720 exacto |
| Paisaje móvil | 1280×720; misma orientación y altura visible |
| Retrato móvil | 390×844, HOME central + vistas izquierda/derecha/entrada |
| Retrato HOME | span 48; centro definido exactamente en request JSON |
| Retrato parcelas/entrada | span 36; centros fijos, sin orbit |

La cámara inversa de una sola ilustración no es única. 35°/20° es una decisión de diseño comprobada contra el target, no una recuperación de metadatos inexistentes. Se compararon, sobre las mismas masas, la orientación anterior (~17.22°/30.09°), esta ortográfica y una perspectiva larga a 110 unidades / FOV vertical ~24.62°. La orientación anterior aplana la superficie y altera la relación keep/escalera/puerta; la perspectiva introduce gradientes de escala innecesarios. Se elige ortográfica por alineación estructural y previsibilidad móvil. No se ha cambiado la cámara del juego.

Safe areas provisionales: paisaje 5% arriba, 12% abajo, 5% laterales; retrato 8% arriba, 16% abajo, 5% laterales. Máscaras de evidencia, no HUD integrado. El móvil retrato conserva escala y utiliza navegación entre vistas; no exige meter todo el target dentro de 390 px. Esta prueba comprueba encuadres, no gestos/taps ni rendimiento de dispositivo. Puerta, puente, Bastión, cabaña, campamento y centro se revisan en paisaje y en sus vistas retrato correspondientes.

## Macrocomposición reproducible

Las coordenadas de target son píxeles, origen arriba a la izquierda, sobre 1536×1024. `target-spec.json` contiene cajas, polígonos y alturas. Las anotaciones son manuales, con incertidumbre estimada ±12 px; las medidas de blockout vienen de proyectar vértices 3D. No existe un porcentaje inventado de semejanza artística.

| Masa | Función / silueta |
|---|---|
| Plataforma principal | Huella ancha irregular y continua; dos lados abiertos de gameplay; precipicio claramente separado del suelo |
| Terraza superior | Banda más estrecha y elevada, sin otra gran montaña detrás del Bastión |
| Bastión | Cortina horizontal larga con un keep central dominante, alas subordinadas y extremos de distinta altura |
| Puerta inferior | Dos torres y dintel; vacío central real, continuidad visual del acceso |
| Puente | Cruza agua desde foreground hacia la puerta; pretiles bajos, sin tapar el vano |
| Eje | Entrada → puerta → camino diagonal → escalera → puerta superior |
| Escalera | Un ascenso ancho reconocible, sin masas que lo ocluyan |
| Murallas parciales | Dos tramos laterales separados; no cercado continuo que cierre parcelas |
| Cabaña izquierda | Volumen pequeño con tejado a dos aguas y chimenea; huella subordinada |
| Campamento derecho | Dos prismas triangulares ligeros; patio militar claramente distinto de arquitectura pétrea |
| Vegetación | Masas cónicas grandes en bordes y detrás; centro libre |
| Agua/fondo | Plano y terreno/contexto simple para juzgar profundidad, no paisaje final |

Alturas en unidades provisionales: agua 0; banco foreground 3; plataforma 7; suelo 7.03; terraza superior 11; arranque arquitectura superior 11.1; keep máximo 18.7; puerta inferior torres ~15.4. Desnivel principal→superior 4. La escala mundial es de preproducción: plataforma **65.39×37.17**, terraza superior **46.69×12.49**, keep **3.9×4.7×7.6**, cabaña **4.4×2.6×2.1**. El modelo JSON/OBJ define cada transform de forma inequívoca. No trasladar estas dimensiones a colliders del gameplay actual.

Jerarquía: primero Bastión superior; después puerta/acceso y gran plataforma; luego cabaña/campamento; finalmente bordes, agua y bosque. El peso del Bastión se obtiene con altura, skyline y posición, no por llenar el terreno de edificios.

Espacios negativos: conservar las dos áreas centrales laterales del eje, la salida del vano inferior, el frente de escalera y la recepción superior. Se dibujan en el overlay; son reservas visuales de este target, no modificación de las parcelas canónicas. No colocar árboles, viviendas ni props en estos vacíos para disimular una composición pendiente.

Ritmo: grandes horizontales de suelo/muralla contra verticales del keep/torres/árboles; piedra continua contra dos instalaciones ligeras; foreground de acceso, midground vacío útil y fondo elevado. En arte futuro, piedra cálida y puntos de actividad frente a vegetación/agua/fondo más fríos. Aquí los materiales son planos y neutrales: no se evalúa acabado, luz final o textura.

La ocupación se separa en cajas anotadas, polígonos estimados y píxeles realmente visibles del blockout. Las cajas se solapan y **no se suman**. La segmentación del proxy se obtiene con un z-buffer y se guarda por familia; agua/fondo simplificado no pretende una segmentación exacta de la fotografía.

## Construcción y revisión

Escena completa: **85 masas / 1115 triángulos**. Cubos, prismas, planos, tejados triangulares, árboles cónicos, doce peldaños y silueta facetada de acantilados. Cero assets finales, texturas, ornamentos, GLB legacy o authoring final Blender. OBJ/JSON son greybox, no un kit de producción.

Primera iteración: error de colocación de centros elevaba visualmente las bases de torres/cabaña; se corrigió la relación base/altura y se añadió el acceso foreground. Segunda revisión: cabaña excedía la altura anotada; se corrigió volumen/tejado. Última corrección: un árbol atravesaba el extremo del Bastión; se retiró de ese volumen, sin cambiar la jerarquía. No se iteró detalle.

La reconstrucción CPU sirve para iterar rápidamente la cámara; **la evidencia autoritativa final es Unity 6000.3.23f1 en el runner existente**. Se extendió el workflow `valoria-production-art-reset-v1.yml` con un modo de entrada de malla provisional. Su modo legacy permanece disponible. El capturador genérico editor-only `PreproductionSceneCapture` abre una escena nueva vacía, lee exclusivamente el JSON y no guarda ni abre escenas de producción. La reflexión X/Z/Y exige invertir winding; está corregida. No se creó otra cadena Tripo/Blender ni otro workflow de assets.

La comparación usa ocho landmarks con tolerancia macro explícita: centro ≤2.5% de diagonal, proporciones 0.75–1.25. Es una tolerancia de greybox con volúmenes rectos frente a tejados/merlones/rubble, no un criterio para final art. Se revisan además continuidad visual del eje, silueta de plataforma/terraza, vacíos, background y oclusiones. El JSON del resultado conserva cada error, no solo un booleano. La aprobación visual del propietario sigue pendiente.

## Riesgos de producción, sin alterar el target

| Riesgo | Resolución prevista después de aprobación |
|---|---|
| Mampostería individual y merlones | Módulos + trim/normal; geometría solo para ritmo de silueta |
| Acantilado único enorme | 4–6 segmentos orgánicos con interfaces comunes; 1–2 piezas de silueta únicas |
| Vegetación muy numerosa | 3–4 variantes instanciadas; LOD e impostor en fondo |
| Agua/reflejos caros | Plano con normales, orilla simple; evitar SSR como dependencia |
| Muchos fuegos/luces | Emisión y partículas; como hipótesis ≤2 luces locales con coste medido |
| Ruido de tejados/props | Textura/decals; ninguna geometría invisible a cámara móvil |
| Escena retrato demasiado ancha | HOME + pan acotado; mantener el framing paisaje, no comprimir ciudad |
| Mesh fused legacy | B solo tras separar y comprobar interfaces; C si impone otra silueta |
| Presupuesto móvil incierto | Presupuestos por familia son hipótesis, no certificación; perfil físico en vertical slice |

No se simplifica automáticamente ningún elemento del adjunto. Lo imprescindible para silueta, escala o función debe sobrevivir LOD. Los detalles interiores fuera de la cámara no deben condicionar el target.

## Asset families y reutilización

Inventario estructurado: `asset-families.json`; inventario de archivos legacy reales: `legacy-inventory.json`. Los presupuestos son de planificación LOD0 para esta escena, no resultados de rendimiento. A = as-is comprobado; B = candidato a modificar y volver a probar; C = solo referencia; D = no usar en este target. Ningún candidato B se ha reutilizado efectivamente ni se considera aprobado. No se asigna A a geometría por su certificación histórica.

| Familia | Piezas / modularidad / método futuro | Presupuesto LOD0 de escena | Legacy |
|---|---|---:|---|
| Cliff / Rock | 4–6 tramos repetibles + 1–2 siluetas; Blender orgánico con sockets de altura | 45k tris | B BroadRockPlatform / SteppedRockTerrace como fragmentos; C masas fused |
| Ground / Terrain | 2 superficies propias + bordes repetibles; malla/terreno, material ground | 15k | B Ground Kit compatible; C topología vieja |
| Bridge | vano/deck propio + pretiles/pilares repetibles; Blender modular | 12k | C GateStreetRiseRock; D copiar su diorama completo |
| Lower Gate | vano abierto + torres + cortina; Blender de piedra | 25k | B StoneDefensiveV2/MainGate/Tower, tras medir silueta; C gate fused |
| Wall | recto, esquina, terminal, torre; módulos stone | 12k | B StoneArchitecture y StoneDefensiveV2; no A sin comparación |
| Bastion | keep/alas/puertas/extremos editables; source artist-led, stone | 40k | C HeroBastionGenerated completo; B fragmentos si conservan nueva silueta |
| Road | 2 tiras + recepción; tile/decals, sin ornamentación | 3k | B materiales de Ground; C geometría de rutas históricas |
| Stair | peldaño/cheeks/landing modular; conexión propia | 5k | B fragmentos TerraceStairRock; C módulo entero |
| Cabin / Timber | 1 cuerpo/roof + puerta/chimenea modular; wood/slate | 10k | B Workshop/MidTier Piece03 solo tras reducir huella; C Granero_RCF |
| Camp | 2 tiendas + valla/banderín repetibles; timber/fabric | 7k | C Cuartel_CFSv1; D barracón pétreo como campamento |
| Vegetation | 3–4 árboles + arbustos, instancing | 20k | B forest source con LOD; sin importación en sesión |
| Props | 6–10 tipos compartidos; crates/logs/banner/torch | 6k | B props separados, nunca diorama completo |
| Water / Shore | plano y 2–3 orillas | 3k | B shader/soluciones de agua existentes, sin nueva superficie final |
| Background | relieve simple + impostors/forest | 8k | C World Frame previo; D montaña protagonista |

Total de planificación: **211k tris LOD0 visibles**, antes de medir calidad y coste real. Hipótesis de ≤128 MiB de texturas y ≤120 renderers después de instancing/consolidación. LOD1 40–60% y LOD2 15–25% de LOD0 según error de silueta; fondo impostor; no bajar calidad por un número especulativo. Stone/timber/slate/ground/rock/fabric/vegetation/water permanecen familias de material separadas pero coherentes. Prioridad: terreno/roca y accesos; después Bastión/puerta/muralla; luego cabin/camp/vegetación; props al final.

## Límite del PASS y próximo paso

El gate de esta sesión certifica una definición macro comprobada y reproducible, no la calidad artística final, la integración jugable de la nueva ciudad ni el rendimiento de móvil. Gameplay intacto se demuestra por cero cambios de runtime/escenas/assets anteriores y tests enfocados reales en Unity.

Después de la revisión/aprobación personal del propietario: seleccionar **Lower Gate Family** como única familia representativa. Probar vano abierto, torres, proporciones de piedra, suelo/puente y lectura móvil con el blockout ya fijado; source review → integración aislada → captura matched → coste/LOD. No rehacer Bastión, no producir otras familias y no gastar Tripo sin una autorización nueva sobre input/coste exactos. Este siguiente bloque **no se inicia en esta sesión**.
