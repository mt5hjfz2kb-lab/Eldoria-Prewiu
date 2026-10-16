# Eldoria / Valoria — alternativas gráficas duraderas y plan de decisión
**Registro permanente de contingencia · 2026-10-17 · Estado: REFERENCIA, NO ORDEN DE EJECUCIÓN · Presupuesto: 0 €**

> **Motivación del propietario:** evitar que semanas de iteración artística dependan de un chat o de que un agente IA llegue a ser un artista 3D profesional. Conservar alternativas viables, comprobables y recuperables por cualquier futuro equipo.
>
> **Autoridad actual:** leer primero `AGENTS.md`, `pipeline/active-workstreams.json` y `docs/ELDORIA_VALORIA_LAYERED_CONSTRUCTION_EXCLUSIVE_PRIORITY_20261016.md`. El constructor exclusivo `construction/valoria-master-plan-layers-20261016` conserva su misión; **este documento no la pausa, no crea workstreams, no activa runners y NO AUTORIZA ejecutar alternativas ni cambiar el estilo**. Una nueva estrategia requiere decisión expresa del propietario y coordinación de propiedad/recursos. El runtime, `PlayerState`, `LocalGateway`, cámara real, jugabilidad, fuentes y ramas históricas quedan protegidos.

## 1. Problema identificado
Ya se ha demostrado capacidad para construir escenas Unity, exportar y colocar GLB y superar comprobaciones de continuidad; eso **no prueba calidad visual global**. Intentar una y otra vez mejorar un mismo artista IA o ensamblar más assets puede perpetuar el mismo cuello de botella. La prueba que importa es una **captura jugable completa y comparable** de **puerta, puente, ribera/ladera y acceso**, desde la cámara del jugador y ante la referencia `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`, además de rendimiento y conservación de juego.

**Principios:** nunca empezar otra biblioteca masiva ni un proyecto desde cero; copiar/derivar sin destruir originales; trabajar en celda aislada; conservar progreso jugable; preferir fuentes CC0 cuando se pueda; verificar licencias y compatibilidad Unity 6000.3.23f1/URP; coste real 0 €; no confundir TECH PASS con ART PASS. La referencia artística aprobada NO se rebaja sin decisión expresa del propietario.

## 2. Catálogo de alternativas (para conservar; NINGUNA activada)

| Ruta | Qué cambia de verdad | Prueba mínima sin dependencia de artista IA | Riesgo / decisión |
| --- | --- | --- | --- |
| **A — Kit 3D completo y coherente** | No crear cada edificio: adaptar a la topología existente una familia diseñada por artista humano. | Instanciar una sola familia gratuita en puerta+calle+edificio, render A/B misma cámara. | Puede no contener fortificación monumental; estilo distinto. |
| **B — Kit medieval CC0 de un solo autor** | Familia de Quaternius o Kenney en vez de ensamblar 63 fuentes dispares. | Prototipo con un único pack, pivotes/escala/URP y materiales unificados. | Calidad estilizada quizá alejada de la referencia; pivote artístico requiere consentimiento. |
| **C — Arte 3D híbrido con assets de terceros + pocos héroes** | Adquirir gratis naturaleza/materiales/rocas ya resueltos; autoría personalizada solo en puerta y Bastión. | Celda con rocas/árboles/materiales CC0 coherentes y un hero asset existente. | Homogeneidad, licencia, tamaño y rendimiento móvil. |
| **D — Terreno real mediante herramientas de modelado de paisajes** | Reemplazar losas cúbicas con malla esculpida/heightfield y transiciones roca-camino-ribera. | Malla continua propia en Blender o Unity Terrain con colisiones y accesos existentes; captura silueta. | Sin buenas texturas/iluminación puede seguir viéndose pobre; adaptar cotas. |
| **E — Autoría Blender manual/técnica sin LLM** | Usar herramientas nativas, modificadores, escultura y técnicas modulares a partir de un diseño definido; agente IA NO es requisito. | Una torre o puente nuevo con fuente .blend editable, UV, piezas y render comparativo. | Requiere experiencia técnica y tiempo; no automatizar falsas «mejoras». |
| **F — Cámara 3D restringida y set dressing dirigido** | Diseñar para 1–3 cámaras oficiales, ocultando geometría no visible, conservando selección/zoom permitidos. | Demostrar composición, encuadre horizontal/vertical y construcción progresiva con cámara acotada. | Reduce libertad de cámara; necesita decisión de diseño. |
| **G — 2.5D híbrido en Unity** | Fondo y arquitectura ilustrados por capas; geometría/colliders/hotspots 3D reales; profundidad/parallax. | Zona de acceso con puente caminable y silueta ilustrada ligada a estados de edificios. | Oclusión, paneo, futuros cambios de estado y consistencia visual. |
| **H — Edificios prerenderizados desde Blender** | Renderizar variantes de edificaciones al ángulo oficial y usarlas como sprites/planos, dejando elementos interactivos 3D. | Un edificio con 3 estados de mejora, selección y zoom real, visible en móvil. | Animaciones y cualquier cámara libre disparan costo de variantes. |
| **I — Piezas «hero» humanas gratuitas + entorno procedural** | Arquitectura principal reutilizada bajo licencias verificadas, detalles/terreno vegetación procedurales. | Composición puerta-puente con 1–2 héroes cuya calidad ya se compruebe en fuente. | Dificultad para encontrar modelos de la silueta exacta a coste cero. |
| **J — Dirección de arte estilizada uniforme** | Cambiar meta desde semirrealismo ilustrado complejo a 3D estilizado de buena factura, mediante un solo kit. | Render del mismo encuadre y test del bucle I→II con estética coherente. | No equivale a referencia original: solo el propietario puede aprobar el cambio. |
| **K — Gameplay-first con arte provisional aceptado** | Desacoplar diseño de cartas, gobierno, exploración, puerto y PvE del bloqueo gráfico. | Vertical slice Unity existente + un sistema jugable acotado, sin tocar producción artística pausada. | No resuelve arte; es una estrategia de producto, requiere levantar pausa correspondiente. |
| **L — Colaboración artística humana voluntaria** | Solicitar una revisión focalizada o contribución CC0 a comunidades/artistas en lugar de depender del agente. | Brief de puerta+puente, criterios de entrega, licencia explícita, fuente editable, aprobación humana. | Disponibilidad no garantizada; coordinación y derechos deben acordarse. |
| **M — Producción por fondos pintados + capas de progreso** | Diseñar un escenario principal ilustrado de Valoria y cambiar capas con expansión del Bastión, en vez de render 3D libre. | Bastión I/II en paisaje/retrato con selección, apariciones y estados persistentes reales. | La escalabilidad a Bastión 25–35 y cambios dinámicos exige plan de variantes. |
| **N — Ajuste de alcance artístico por distritos** | Mantener hero gate/bridge a alta calidad, demás distrito a media distancia y LOD/billboards; no cargar toda la ciudad a calidad hero. | Captura final con solo un «hero frame», calidad, FPS y memoria móviles documentados. | No soluciona una puerta/puente flojos; sí puede hacer viable el mundo. |

**Fuentes candidatas comprobadas en sitios originales al documentar (no importadas ni aprobadas):**
- Quaternius [Medieval Village MegaKit](https://quaternius.com/packs/medievalvillagemegakit.html): CC0, 300+ módulos; **solo parte de los contenidos es gratuita** (aprox. 60–70 % según sitio); la versión completa e implementaciones de motor pueden tener acceso diferente. Verificar paquete gratuito exacto antes de usar.
- Quaternius [Modular Medieval Building Pack](https://quaternius.com/packs/modularmedievalbuildings.html): CC0, Blender/FBX/OBJ, uso comercial gratuito.
- Quaternius [Medieval Village Pack](https://quaternius.com/packs/medievalvillage.html): 44 modelos CC0.
- Kenney [Castle Kit](https://kenney.nl/assets/castle-kit): 75 archivos, CC0.
- Poly Haven [licencia](https://polyhaven.com/license): modelos, texturas y HDRI CC0 para uso comercial, sujeto a inspección individual, optimización y estilo.
- Cualquier paquete de Unity Asset Store: verificar que es realmente gratuito, su licencia específica, derechos de modificación/redistribución y compatibilidad con Unity6 URP; **no equipararlo automáticamente a CC0**.

## 3. Experimento único, comparable y con límite: «celda hero de Valoria»

**No abrir simultáneamente alternativas.** Sólo si el propietario decide activar una:
1. **Congelar** SHA, escena, fuente y PNG de la última versión válida; mantener plano maestro, navegación, carrera Bastión I→II, guardado y topología.
2. Crear **una celda aislada** de aproximadamente puerta+puente+riberas+inicio de escaleras, sin cambiar la escena canónica; elegir UNA alternativa y UN pack/dirección artística.
3. Verificar fuente, escala, licencia, UV/PBR e importación; render **mismo encuadre y ajustes oficiales** antes/después en horizontal y vertical, incluido movimiento/zoom permitido del jugador, no solo cámara ortográfica histórica.
4. Evaluar separadamente: (a) calidad de silueta/escala/estilo/contactos; (b) composición frente a referencia; (c) recorrido, colisiones, hotspots y persistencia; (d) rendimiento en objetivo móvil, cuando esté disponible.
5. **Descartar la estrategia si dos ciclos completos de corrección no muestran mejoras claras** o sacrifican jugabilidad. No seguir reportando recuentos de meshes/commits como éxito visual. Registrar evidencia y pasar a otra opción solo con consentimiento.
6. Promover únicamente después de revisión A/B real, presupuesto 0 €, revisión legal de assets y autorización de integración.

## 4. Árbol de decisiones de contingencia
- **Si falta calidad de modelos concretos pero encaja topología/cámara:** A/B/C/E/I antes que rediseñar Eldoria.
- **Si los mejores modelos no forman un escenario natural convincente:** D + A o C; tratar primero terreno/riberas/relieve.
- **Si la referencia depende sobre todo de perspectiva pictórica y cámara fija:** comparar F/G/H/M, con consentimiento por cambios de presentación.
- **Si el 3D libre no es viable con calidad/rendimiento/presupuesto:** proponer J/N o híbrido; decisión del propietario.
- **Si el bloqueo gráfico vuelve a congelar semanas de juego:** considerar K, preservar progreso del proyecto y reactivar gameplay solo por orden.
- **Si los recursos automáticos no resuelven la autoría artística:** L, sin prometer colaboraciones gratuitas.

## 5. Punto de decisión y plantilla para futuros chats
**Disparador:** tras el siguiente gate visual real de la misión activa si puerta+puente+terreno siguen sin mejora convincente; o si dos ciclos artísticos consecutivos no producen mejora comparativa.
**Estado al crear el documento:** la línea Valoria por capas sigue siendo la única misión autorizada; arte global NO aprobado; no declarar esta contingencia activa sin nueva orden.

Futuro chat o agente: «Recupera `docs/VALORIA_VISUAL_FALLBACK_DECISION_PLAYBOOK_20261017.md`, el último HEAD y el registro activo; distingue la ruta autorizada de las alternativas reservadas. Presenta al propietario 1–2 pruebas acotadas según los defectos observados, con costes/licencias/impacto y criterios A/B. No ejecutes ni reclames otra misión sin permiso explícito».

**Criterio de éxito:** una celda visiblemente mejor en Unity, reproducible y jugable. **Criterio de no-éxito:** solo nuevos modelos, pruebas técnicas, documentos o cambios de método sin una mejora perceptible.
