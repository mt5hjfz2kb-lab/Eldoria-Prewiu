# ELDORIA — Auditoría visual causal, clasificación por familias y piloto verificable

**Fecha de corte:** 03-11-2026, Europe/Madrid. **Tipo:** diagnóstico y plan de contraste; NO aprobación artística, NO permiso para invadir la PR/runner de otro titular, NO orden de merge/publicación. **Presupuesto adicional:** 0 euros.

## 0. Autoridad, alcance y advertencias

Esta memoria conserva la conversación de diagnóstico de Eldoria, la clasificación integral por sistemas visuales y una única intervención que se puede aceptar o refutar. No sustituye el estado vivo. Antes de editar, leer [AGENTS.md](../AGENTS.md), [registro activo](../pipeline/active-workstreams.json), la [PR #136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/136), últimos commits y runs. En el corte de esta auditoría, la PR #136 está abierta y el titular de Valoria/Windows trabaja en **Entrenamiento**; NO editar esa rama o arrancar Unity concurrentemente.

**Categorías:** R = reconstrucción estructural (forma/composición incorrecta); I = corregir integración de una fuente válida; A = acabado visual local; C = conservar baseline aprobado/provisional; V = verificar con pruebas actuales antes de decidir. La categoría es por **defecto**, no sentencia de rehacer un sistema entero. Confianza **alta/media/baja/no evaluable** expresa solidez de la relación causa→intervención, NO probabilidad numérica, aumento de nota, promesa de resultado ni pase artístico.

**Fuente de verdad:** referencias aprobadas exactas (ciudad: [Valoria canon 20/10](VALORIA_VISUAL_REFERENCE_CANON_20261020.md), sin cuatro parcelas visibles; mundo: las tres vistas originales de Región 1; héroes: [maestro de la PR #136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/fix/player-feedback-context-ui-20261030/docs/heroes-ui/REFERENCIA_MAESTRA_APROBADA.png); UI: [Crónica Real](ui-v3/OWNER_APPROVED_DIRECTION.md)), más PNG Unity A/B del SHA examinado y decisión expresa del propietario. No usar maqueta conceptual como juego real, ni un PASS de CI como ART_APPROVED. Calibrar con [reglas vigentes](ELDORIA_VISUAL_PRODUCTION_RULES.md), [escalera de convergencia](ELDORIA_VISUAL_CONVERGENCE_PIPELINE.md) y [contrato de aprobación](ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md); en este último hay referencias históricas a Qwen que han sido **revocadas** por [la orden de 22/10](ELDORIA_QWEN_ART_DECOUPLING_20261022.md).

## 1. Diagnóstico transversal causal

La debilidad recurrente NO equivale a que todo arte sea malo ni a que falte una herramienta: se confunde la existencia de una fuente, la corrección de su código/importación y el PASS de Unity con fidelidad artística. La cadena **referencia 2D → interpretación de geometría/UI → integración → cámara real** pierde composición, identidad, profundidad, escala y materiales si no se verifica cada una por separado. El proyecto ya tenía reglas de verificación independientes, pero los dos ensayos de la Brecha probaron que se puede aprobar técnicamente una mala representación. La solución global es **diagnosticar capa causal antes de editar**, validar una fuente candidata antes de certificar toda la escena y exigir A/B auténtico con revisión independiente.

**Evidencia positiva que prohíbe reconstrucción masiva:** [reautoría del barrio inferior](VALORIA_LOWER_CITY_URBAN_MASSING_REAUTHORING_V1_RESULT.md) suprimió 27 renderizadores redundantes, reunió 3 grupos y obtuvo TECH+VISUAL PASS con geometría reutilizada; [navegación móvil](VALORIA_MOBILE_NAVIGABLE_CITY_V1_RESULT.md) validó zoom y home pose sin miniaturizar toda la ciudad. [Revisión de Aserradero y Cuartel](VALORIA_VISUAL_BASELINE_DIAGNOSIS.md) identificó problemas de materiales/importación, no necesidad de regenerar su silueta.

**Evidencia negativa:** [recoloreado general de ciudad](VALORIA_IN_CITY_SURFACE_PASS_V1_RESULT.md) pasó técnicamente, pero recibió VISUAL FAIL/PARTIAL y no fue promovido; [ensayos de Brecha](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/95) PASARON CI y fallaron la similitud. Descartar microtintes, overlays de 11 tiras sobre terreno y clipping de una única capa como soluciones probadas.

## 2. Inventario de áreas visuales y solución causal

**CIUDAD VALORIA.** La confianza se refiere a **corregir el defecto de la fila**, no a alcanzar calidad global 8/10.

| Elemento / defecto | Tipo | Intervención exacta, conservando lo protegido | Confianza |
| --- | --- | --- | --- |
| Composición urbana fragmentada o grupos repetidos | R/I | Reconciliar sistemas de presentación superpuestos; mantener tres grupos urbanos aceptados, calles y jerarquía Bastión→secundarios; no poblar por intuición | Alta si la superposición aún existe; comprobar HEAD |
| Bastión I protagonista | C/V | Aplicar veto al modelo antiguo, verificar renderer/GUID efectivo antes de builds; mantener fuente autorizada, no generar otro | Alta para identidad/protección |
| Aserradero | I/A | Mantener GLB; comparar fuente PBR versus materiales realmente importados; restaurar albedo, normales, roughness/metálico y luz que separen madera/piedra | Alta si persiste el lavado documentado |
| Cuartel | I/A | Mantener entrada/silueta militar; corregir sobreexposición, sombreado, materiales y asiento del edificio antes de regenerar | Alta para lavado, media para riqueza final |
| Granero | I/A | Retener fuente dedicada; comprobar lectura agrícola, acceso, techo y contacto a escala oficial; no sustituir por genérico | Media-alta |
| Puente, escalera, descanso y terrazas | I/R | Corregir el apoyo y transición física entre piezas aprobadas y terreno, sin decorar para tapar grietas; candidata acotada en §5 | Alta para unión visible, media para impacto global |
| Material/iluminación/atmósfera general | A/I | Unificar perfiles URP con normales/PBR, sombras de contacto y jerarquía clara; no aplicar MaterialPropertyBlock global que blanquee todo | Media-alta |
| Cámara de ciudad/HUD sobre paisaje | C/I | Conservar encuadres y pan móvil certificados, corregir solo recortes/clicks comprobados | Alta para lectura |

**MUNDO 4X — REGIÓN 1.**

| Elemento / defecto | Tipo | Intervención exacta | Confianza |
| --- | --- | --- | --- |
| Terreno/praderas/biomas repetitivos | R/A | Preservar única malla física, coordenadas/nodos; reautorizar masas y transiciones en fuente Blender, con lecturas cercana/regional/panorámica, no nuevos mapas | Media-alta |
| Brecha circular | R | Reautoría conjunta del suelo, placas, estratos, roca y pigmento radial en maestro Blender; **no** otras tiras superpuestas ni clip de placas aislado | Alta en causa; media en alcanzar calidad deseada |
| Bosque plano/repetitivo | R/A | Controlar grupos, claros, troncos/copas y LOD lejano sin perder silueta; no llenar pastos abiertos para ocultar defectos | Media-alta |
| Río/ribera | I/A | Corregir UV de flujo, forma de orilla, contacto agua–piedra y respuesta de material; sin cambiar cauce/navegación | Alta en fallo UV documentado |
| Cantera/madera | I/A | Mantener IDs, proporción de nodos y source; caracterización visible de cantera/aserrado, bases integradas, no piedras aleatorias | Media-alta |
| Engendro, jabalí, lobos | I/A | Conservar GLB/animaciones correctas; medir tamaño en píxeles bajo cámara de jugador, nivel de detalle y apoyo al suelo, actualizar solo si la lectura falla hoy | Alta para legibilidad; no evalúa belleza |
| Cámara estratégica y continuidad de territorio | I/C | Verificar primera entrada alejada, zoom a cercana, visibilidad y selección por distancias; no modificar la cámara solo para ocultar arte | Alta para orientación |
| Marchas en mapa y objetivos | I/A | Representaciones coherentes sin líneas amarillas/cian; mantener selección, límites, VFX y guardado | Media-alta |
| Costa/panorámica y horizonte | A/R | Corregir masas y transición a gran escala solo si sigue visible borde repetitivo; no volver a crear continente | Media |

**UI / GESTIÓN Y NARRATIVA.**

| Elemento / defecto | Tipo | Intervención exacta | Confianza |
| --- | --- | --- | --- |
| HUD global Crónica Real | C/A | Preservar propuesta 1 azul tinta/oro y navegación real, reducir o desplazar elementos que tapen escena | Alta |
| Construir/Mejorar | I/A | Cartela inferior en la propia ciudad con miniatura del edificio real, costes, requisitos, temporizador y acción; nunca escena extra | Alta |
| Entrenamiento | R/I | Arquero alfa y patio independientes; paneles ilustrados nine-slice; jerarquía selector→estadística→coste/tiempo→cantidad→Entrenar, texto runtime | Alta para panel plano; resultado final por revisar. **Área en ejecución ajena** |
| Marchas/preparación | R/I | Reconstruir macro jerarquía de héroes/tropas/porcentajes/orden, retratos con máscaras y fondos modulados; no inventar clases ni datos | Media-alta |
| Diálogos | I/A | Cartela pictórica 'Diálogo 1', retrato/continuar y safe areas, texto real; Orín y Fisura con tratamiento contextual específico | Alta |
| Camino de Vigilantes | I/A | Proyectar 25 hitos sobre perspectiva del sendero y separar niveles y recompensas de fondo; conservar progreso | Media-alta |
| Mesa de Exploradores | I/A | Pergamino separado con alfa, 3 rutas/controles y textos reales; no foto escénica con texto horneado | Media-alta |
| Buzón e informes | A/V | Mantener categorías/ledger/estadísticas reales; comprobar capturas y lectura antes de intervenir | Media |

**HÉROES Y OTROS.**

| Elemento / defecto | Tipo | Intervención exacta | Confianza |
| --- | --- | --- | --- |
| Salón de Héroes | C/A | **Aceptado provisionalmente**; seis plazas, dos reales, cuatro reservas, modelos y fuentes intactos. Mejorar ornamento/retratos después, sin reiniciar | Alta en preservar |
| Fichas Aldric/Lyra | I/A | Conservar GLBs; cámara, RenderTexture, pose natural, sombras de contacto, profundidad del escenario y cuatro atributos reales | Media-alta |
| Habilidades | C/A | **Aceptación provisional**; Baluarte y Disparo de Ruptura reales, resto bloqueadas; mejorar marcos/lectura solo en pase futuro | Media |
| Talentos | I/A | Tres ramas y nueve nodos bloqueados; mejorar composición/iconografía/844×390 sin inventar poderes | Media-alta |
| Equipamiento | I/A | Modelo auténtico, cuatro slots visuales bloqueados, iconos independientes, prioridad de lectura a 844×390 | Media-alta |
| Animaciones héroes/criaturas | C/I | Mantener clips/rig aprobados, reparar únicamente contacto, deformación o escala perceptiva demostrados | Alta para proteger |
| Motor visual de combate, Relicario, prólogo | V | Recuperar sus fuentes y capturas ACTUALES; no determinar reconstrucción hasta ver evidencia | No evaluable |
| Tutorial y overlays | I/A | Superposición, safe area, legibilidad y textos realistas; sin paneles grandes que oculten el juego | Media-alta |
| Calidad WebGL/iOS/Android | V | A/B del build *distribuido* en hardware físico, texturas, memoria, antialias y framerate; no tratar CI Windows como móvil | No evaluable |

**No es un inventario individual de todos los 3D y sprites**: esos requieren biblioteca curada, originales, hashes e imagen visible por asset. Este documento clasifica familias y pantallas, señalando cuándo una decisión necesita evidencia actual. Leer [issue #95](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/95), [PR #136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/136), [Habilidades provisional #145](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/145), [inventario de edificios](VALORIA_BUILDING_PRODUCTION_INVENTORY_V1.md) y [dirección UI](ui-v3/OWNER_APPROVED_DIRECTION.md). Veto Bastión antiguo prevalece sobre PASS históricos.

## 3. Brecha: post mortem exacto — no volver a invertir en lo rechazado

En el GLB genuino histórico de Región 1 (ejecución Blender 40304157232) la Brecha consta de varias piezas superpuestas: suelo 29.527 tris, placas 13.514, estratos 4.218, roca desprendida 1.687, fracturas periféricas 1.283, núcleo 27. El generador de placas define 110 semillas Voronoi en torno a centro elíptico; usa fallas para relieve pero NO para determinar la huella global. El shader suma anillos/pigmentos radiales. Primera candidata con cintas 3D: franjas negras, peor. Segunda candidata cortó las placas, no las demás capas: solo cambió **0,011 %** de píxeles cercana y **0,062 %** regional vs baseline en comparativa Unity (umbral por canal >15). **Ambas ART_REJECTED aunque CI PASS**. Restauración completa mediante commit [59905af](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/commit/59905af6d57fbf82fce06a1a9e63f1bc88254276) y verificación de tres archivos idénticos al punto previo. Mantener resultados históricos para evitar iteración circular. Detalles [issue #95](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/95).

## 4. Orden y criterios universales de éxito

1. **No interrumpir** Entrenamiento: workstream de Valoria/PR136 y Windows reservado en el corte. Releer titular del registro antes de cualquier acción.
2. **P0**: finalizar A/B de Entrenamiento y Marchas ya en marcha, sin rehacer lo aceptado; usuario exige referencias exactas.
3. **P1**: una corrección pequeña y causal en contacto ciudad/terreno, propuesta en §5; esta auditoría no autoriza ejecutarla concurrentemente.
4. **P1**: escala del Engendro y primer encuadre del mundo si las capturas recientes repiten defecto de [issue #95](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/95).
5. **P2**: suelo, bosque/ribera y reautoría Brecha solo cuando se pueda abordar TODA su fuente, no una capa radial.
6. **P2**: acabado del Salón, Fichas, Habilidades, Talentos y Equipamiento; proteger aceptaciones provisionales.
7. **V**: combate visual, cartas, prólogo y QA de móvil requieren material actual antes de diagnosticar.

**Puertas de aceptación (aplican a cada familia):**
- G0: causa visible, versión y fuente SHA, diseño aprobado exacto, 2–3 defectos priorizados; si la causa no aparece en la captura actual, NO intervenir.
- G1: fuente editable candidata y previsualización en cámara de juego, identidad/silueta, sin detalles ficticios; no incorporar un asset FAIL en Unity por insistencia.
- G2: A/B genuino de Unity **mismo SHA/cámara/resolución/estado**, horizontal + vertical, macro y detalle. La medición de píxeles detecta irrelevancia pero NO decide belleza. Revisor independiente ART SOURCE + VISUAL PASS; si empeora, descartar.
- G3: gameplay y guardado idénticos, hotspots/raycast/colisiones y coste móvil; no atribuir FPS físico no medido.
- **Stop:** como máximo una candidata y una corrección causal, después declarar ACCEPT/REJECT y cambiar causa, no tercer microtinte del mismo problema. Sin aprobación expresa del propietario, no fusionar/publicar.

## 5. ÚNICO PILOTO ELEGIDO — Unión escalera central → descanso inferior → terreno de Valoria

**Hipótesis refutable:** una parte de la artificialidad de la ciudad no está en el color o el número de edificios, sino en una transición físico-visual insuficiente entre escalera, rellano y terreno. Ya existe precedente de corrección de contacto por geometría; el [ensayo general de superficie 02/10](VALORIA_IN_CITY_SURFACE_PASS_V1_RESULT.md) rechazó recolorear muros/rocas porque prácticamente no mejoró y propuso expresamente el uso focal de piezas **TerraceStairRock** y **StreetLandingTransition**. Esta es una *hipótesis histórica*: primero confirmar con el último HEAD, ya que trabajos posteriores pueden haber corregido el mismo contacto. La selección de recursos debe respetar el catálogo limpio: las dos piezas históricamente propuestas figuran como **uso CONDICIONAL**, no como assets de producción automáticamente aptos.

**Predicción concreta observable:** en la misma cámara, el descanso no debe parecer una pieza colocada sobre tierra plana ni tener una línea oscura artificial continua. La roca, el borde de escalera y el suelo deberán unirse de forma creíble, manteniendo claramente legible el camino al Bastión. Se espera mejora local del apoyo y profundidad, **no** una transformación de toda Valoria ni una puntuación ≥8,5. **Confianza causal: alta si la junta sigue visible; confianza en elevación sensible de la ciudad completa: media.**

**Plan de ejecución EXACTO, sin compras ni asset nuevo:**
1. En una copia aislada de la escena vigente, registrar HEAD, fuente actual de escalera y rellano, transform, cámara, shaders, luz y 3 PNG Unity base (HOME, enfoque de escalera, móvil vertical); inspeccionar que siga existiendo un corte/soporte débil. Si ya no existe, abortar hipótesis y **no** aplicar parche.
2. Examinar primero las fuentes YA INSTANCIADAS de escalera, rellano y terreno. Consultar **StreetLandingTransition** y **TerraceStairRock** como DONANTES CONDICIONALES: la biblioteca limpia marca ambos con aproximadamente **49.800 triángulos por archivo**, sin pase artístico/cámara/móvil ni licencia certificada. NO instanciarlos enteros ni considerarlos aprobados. Exigir licencia/procedencia, integridad y ajuste estético; si no pasan, adaptar solo geometría/materiales de las piezas ya aprobadas e integradas en escena. Si ni eso está verificado, detener el piloto.
3. En una copia editable de las fuentes **autorizadas**, construir **un único fragmento de transición** ajustado a la junta real del rellano y los primeros peldaños, con objetivo de **no más de 1.000 triángulos nuevos** (presupuesto del ensayo, no garantía automática de rendimiento); incrustar bases y encajar labio de piedra y último escalón, preservando UV/material de origen y evitando costuras y z-fighting. No añadir 10 piedras dispersas, cambiar color global, duplicar escaleras ni rehacer caminos.
4. Integrar como presentación **sin nuevos colliders ni hotspots**, conservando coordenadas, camino, Bastión/puente, cámara, luces, guardado, navegación y selección. Si necesita una malla puente específica, conservar fuente .blend/UV/material en commit y reutilizar el material de la zona, no generar PBR aleatorio.
5. Capturar Unity B en idéntica escena/estado/cámaras/resoluciones, más zoom 9/12/19 y horizontal/vertical. Crear tríptico referencia→A→B y recorte del borde. Un evaluador independiente debe confirmar **seam/contacto y profundidad mejorados**, no desplazamiento de defecto a otro punto.
6. Rechazar si (a) la junta no se ve más natural en HOME y acercamiento, (b) aparece nueva costura/clip, (c) invade ruta de cámara o tapa puente/edificio, (d) toca raycast/collider/guardado o (e) se percibe igual. Si supera filtro artístico, ejecutar controles y EditMode/PlayMode/QA móvil donde proceda. ART PASS no equivale a entrega publicada.

**Qué demostraría tener razón:** mejora A/B claramente visible *en esa junta* sin defecto nuevo y pruebas de jugabilidad intactas. **Qué falsaría el diagnóstico:** contacto ya correcto en el HEAD reciente, cambio invisible desde la cámara, o nueva pieza creando aspecto artificial. En ese caso se descarta y se prioriza el siguiente defecto de mayor impacto: **no se abre una tercera iteración**.

## 6. Relevo a otro chat — instrucciones compactas y seguras

«Continúa Eldoria sin empezar de cero. Lee docs/ELDORIA_AUDITORIA_VISUAL_CAUSAL_Y_PILOTO_20261103.md y docs/DOCUMENTATION_INDEX.md. Comprueba HEAD main y PR136, pipeline/active-workstreams.json y runs activos. Protege los trabajos artísticos aceptados provisionalmente y el veto del Bastión antiguo. No tomes Windows/PR136 si Entrenamiento sigue reservado. La auditoría separa reconstrucción, integración, acabado, conservar y verificar; los porcentajes expresan diferencias visuales, no notas. Único piloto elegido: unión física de escalera/descanso/terreno en Valoria (§5), condicionado a que la captura actual aún muestre el defecto. No crear nuevas herramientas, assets ni PR para hacerlo. Entrega captura Unity auténtica A/B con juicio de aceptación o rechazo, sin prometer ART_APPROVED.»
