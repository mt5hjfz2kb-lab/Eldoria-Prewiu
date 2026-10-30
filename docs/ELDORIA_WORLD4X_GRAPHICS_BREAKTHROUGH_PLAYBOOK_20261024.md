# Eldoria — cómo desbloqueamos la creación gráfica del mundo 4X

**Memoria operativa reutilizable, 24-10-2026. Estado: guía de técnicas y evidencia, NO aprobación artística ni autorización para ejecutar proyectos nuevos.**

**Propósito:** impedir que futuros Work o chats vuelvan a comenzar el paisaje desde cero, recaigan en prototipos procedimentales débiles, repitan ensayos rechazados o confundan Unity funcionando con calidad visual. Complementa, sin reemplazar, `AGENTS.md`, `docs/ELDORIA_BLENDER_PROFESSIONAL_AUTHORING_PIPELINE_V1.md`, `docs/ELDORIA_ART_KNOWLEDGE_PHASE1_20261021.md` y el contrato visual específico del encargo. **No crea un segundo cerebro artístico.**

## Lectura rápida en un relevo (dos minutos)

1. Recuperar HEAD actuales de `main` y la rama candidata, trabajo `active` en `pipeline/active-workstreams.json`, último commit nativo y dueño de la escena/runner. **Este documento es historia metodológica**, no el estado vivo del PR ni licencia para escribir sobre la reserva de otro Work.
2. Mirar las **imágenes originales**, no regeneraciones: `docs/visual-reference/world-map/{01_vista_cercana,02_vista_regional,03_vista_panoramica}.png`; para Eldoria la panorámica aprobada abarca casi toda Región 1, no solo el sector local.
3. Escoger un **defecto visual observable desde la cámara de juego** y recuperar un precedente concreto (método, fuente editable, captura auténtica de Unity, resultado y rechazo si lo hubo).
4. Reutilizar una **fuente Blender versionada** y su geometría en Unity; reservar los nodos/ciudades/enemigos como objetos de juego independientes. No llenar praderas despejadas para maquillar capturas.
5. Comparar A/B desde idéntica cámara, orientación, resolución, shaders y progresión; conservar solo mejoras visibles. Una prueba PlayMode verde no significa `ART_APPROVED`. Tras dos cambios ineficaces, cambiar de **causa/método**.
6. Registrar SHA del .blend/GLB, scripts de autoría, PNG Unity, run/artefacto, mediciones de zoom, veredicto y siguiente paso. **No afirmar ejecución 24/7 ni resultados de móvil sin pruebas físicas.**

## Qué cambió de verdad: de escena esquemática a proceso reutilizable

| Enfoque / decisión | Qué aprendimos | Cómo reutilizarlo |
| --- | --- | --- |
| Terreno y bosque generados principalmente mediante fórmulas y ajustes C# en Unity | Las pruebas funcionales pasaban pero el resultado seguía lejos de las tres ilustraciones; microajustar árboles, ruido y color no corregía la composición continental | Diseñar primero masas de tierra, ríos, Brecha y silueta desde la **vista panorámica canónica**; no confiar la dirección artística a reglas automáticas |
| Capas geográficas autorales / trazados y máscaras semánticas | Mejoran la estructura; una importación GLB técnicamente correcta no prueba la fidelidad visual; evitar convertir Región 1 en isla estrecha o diorama | Definir curvas/cotas/biomas con datos métricos, desde la **misma geografía**, no tres mapas por zoom |
| Blender convertido en **fuente de autoría** (no mero visor) | `.blend` editable, curvas, mallas, UV, imágenes empaquetadas y export GLB reproducibles; el GLB entra en Unity mediante glTFast existente | Esculpir perfiles, riberas, copas, caminos y materiales en Blender, conservar fuente exacta y exportar deterministamente. Consultar [decisión Blender → Unity](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/BLENDER_AUTHORING_DECISION_20261023.md) |
| BlenderMCP interactivo | Se verificó conexión, observación, tres ciclos de edición y guardado en Blender; pequeñas correcciones en cinco masas forestales **no** lograron el salto artístico | Usarlo para editar y revisar, no asumir que «BlenderMCP conectado» = obra profesional; evaluar impacto visible y cambiar de composición si hace falta. [Revisión real](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/INTERACTIVE_BLENDER_REVIEW_20261023.md) |
| **Microsector de ribera híbrido** en vez de reconstruir la región de golpe | Pinos cercanos en volumen, agua/orillas y pradera editables; árboles lejanos como atlas RGBA de **los mismos árboles y posiciones**: primera cadena con identidad 3D/2,5D verificable | Calibrar un sector pequeño con cámaras de Unity antes de reproducirlo; conservar materiales, atlas, geometría y rutas originales. [Piloto](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/RIBERA_HYBRID_PILOT_REVIEW_20261023.md) |
| Esculpir orillas físicas y copas en lugar de colorear bandas | K/O corrigieron localmente la repetición de copas y el aspecto de «canal» mediante perfiles asimétricos, plataformas y márgenes conectados a la malla | Seleccionar vértices, variar sección/aluvión/talud, conectar el pigmento a las coordenadas **reales** de la orilla; no pintar manchas flotantes. [I/K](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/RIBERA_I_K_REVIEW_20261023.md), [K/O](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/RIBERA_K_O_REVIEW_20261023.md) |
| Extender un terreno ya autorado **soldando el borde** | Sectores P/Q/T/AG ampliaron el estudio sin alterar la fuente aprobada O; conservar coordenadas, índices/UV y caminos Bézier evitó rehacerlo | Guardar la malla de origen por hash, compartir vértices de borde (no colocar otra superficie solapada), continuar caminos desde sus extremos, volver a apoyar cada árbol mediante raycast; comprobar uniones a nivel de geometría **y** de textura |
| Cambio de micro-pigmentos a composición visible desde cámara | T→V variaba menos del 0,14 % de píxeles en la vista cercana y aún menos lejos: esfuerzo que no cambiaba la experiencia; W/X/Y/AD/AG trabajaron relieve, masas forestales y lugar realmente visible | Identificar qué parte del sector aparece en **cada cámara nativa** antes de esculpir; grandes decisiones de relieve y distribución antes de texturas locales. [Crítica T/U/V](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/SECTOR_T_U_V_CRITICAL_REVIEW_20261023.md) |
| Pruebas reales de mundo vivo sin contaminar progreso | Extensión AI soldada de 19.200 m²; cuatro parcelas de ciudad; 17 instancias **a escala real**, cinco reservas y cinco reapariciones de ensayo, 18/18 PlayMode | Terreno estable separado de población dinámica: ocupación, terreno, obstáculos y progresión; ensayar sobre malla real, no dibujar enemigos en el GLB. **El fixture no es un servidor de reaparición real.** [Entrega AI](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/LIVING_SECTOR_AI.md) |

## Receta técnica mínima demostrada (no código mágico)

1. **Autoridad visual:** abrir las referencias originales y fijar cobertura, silueta, Biomas/agua, cámara y encuadres; usar la panorámica de Región 1 para organizar la **composición**, y acercamientos para el lenguaje de formas y materiales.
2. **Autoría Blender:** abrir un `.blend` de fuente comprobada por SHA; mallas y curvas editables; esculpir las formas que faltan y preservar las coordenadas protegidas. No reconstruir con cubos/cilindros/óvalos ni crear fuentes duplicadas de terreno. Una transformación «se ve bien en Blender» no es certificación en Unity.
3. **Superficie:** UV correctos, materiales/atlas empaquetados, pigmentar conforme a la topografía/caminos/agua. Separar máscaras de uso semántico. Comprobar textura albedo efectiva y espacio de color tras glTFast; materiales faltantes, 4×4 o texturas blancas son errores de importación, no nueva dirección artística.
4. **Exportación:** `.blend` y `.glb` versionados, manifiesto de SHA-256 e imágenes efectivamente usadas en el recurso, unidad en metros; verificar el cambio de ejes Blender Z-up → Unity/glTF Y-up y las transformaciones/origen. Guardar scripts de reproducción y materiales; no considerar un render de Blender una captura Unity.
5. **Híbrido 3D/2,5D:** cerca instancias 3D de silueta legible; lejos lotes/impostores capturados **desde las mallas reales** (atlas RGBA) y colocados en las mismas coordenadas; LOD y fundido sin salto geográfico. El tramado de fundido puede ser visible: no prometer perfección por pasar tests.
6. **Unity auténtico:** activar un estudio reversible sin sobrescribir los `WorldHotspot`, enemigos, colliders, guardados ni la UI. Usar `Camera.Render` desde cámara real, 3 escalas × horizontal/vertical, mismo encuadre para A/B y zoom intermedio continuo. Analizar calidad artística y presencia de defectos, no solo número de píxeles diferentes.
7. **Juego y móvil:** comprobar selecciones, contacto de mallas/pies (no inferirlo de `Collider.bounds.min.y`), anclas lógicas intactas, navegación y espacio para ciudades, instancias repetidas y reapariciones en otros lugares. Conservar los estados reales. Medir coste de draw calls/triángulos y, **aparte**, FPS/RAM/calor en hardware objetivo; una película de 97 capturas codificada a 16 fps no representa los fps jugables.
8. **Decidir y preservar:** mejora visible → registrar la versión retenida; empeora → revertir sin destruir el experimento; dos intervenciones con impacto insignificante → pivotar a composición/malla/material real. Solo integrar al mundo principal después de QA funcional, visual, permisos de propietario y pruebas móviles.

## Antipatrón y decisión: NO REPETIR SIN NUEVA CAUSA DEMOSTRABLE

| Intento visto | Resultado verificado | Lección operativa |
| --- | --- | --- |
| Bosque como manchas ovaladas / árboles y caminos genéricos / mapa estrecho con demasiada agua | Capturas alejadas del concepto, aunque importación y tests fueran correctos | Corregir composición general **antes** de adornar |
| Microediciones BlenderMCP en cinco agrupaciones | Control real de Blender, pero impacto artístico insuficiente | Herramienta conectada ≠ mejora; intervenir sobre masa/silueta si domina el cuadro |
| J, pintura de agua y márgenes; L, pigmento de caminos; N, agua transparente y fondo 3D | J charcas planas; L bultos artificiales; N agua turbia y lisa (18/18 tests podían seguir verdes) | Prefiera perfiles de ribera, corriente y relaciones de materiales. No reciclar soluciones rechazadas como variantes aprobadas |
| U/V, añadir pigmentos casi invisibles | Variación cercana 0,1351 %, regional 0,0495 %, panorámica local 0,0182 % respecto a T; no puntuar belleza por esos números | Modificar la causa macroscópica y lo que **ve** la cámara |
| AE, hierbas triangulares dispersas | «Confeti» oscuro sobre la pradera; descartado | Claros necesarios para nodos/ciudades; calidad del suelo por volumen/material/iluminación, no por densidad decorativa |
| AF, adelantar impostores 2,5D; AH, placas de roca elevadas | AF perdió aspecto cercano/regional del bosque; AH añadió placas angulosas blanquecinas; se conservó AG | Diseñar la transición LOD por imagen real, no solo reducir triángulos; no poner «rocas» para tapar una superficie |
| Resolver diferencias entre altura y objetivos moviendo colliders | Rechazado. En AI las pruebas mantuvieron seis triggers lógicos, anclas visuales y registraron +0,08 m de separación deliberada | Preservar lógica/estado; medir contacto de la geometría visual y separar diagnóstico del centro del collider del pie real |
| 120 marcadores al 30 % como demostración de capacidad final | Útil como primer fixture, insuficiente; a tamaño real la prueba final certificó **17 simultáneos**, no cientos | Probar huellas originales, reservas de recolocación, accesos, progresión y no duplicación |
| Llamar «panorámica» a zoom local 52/74 | Solo abarca sector; no es la panorámica canónica de la región (históricamente adaptador con 500–620) | Etiquetar **panorámica local del sector** por separado y probar **casi toda Región 1** cuando exista |

## Contrato 4X para futuros assets y biomas

- **Praderas abiertas son funcionales**, no defecto por falta de árboles: albergarán decenas de nodos por recurso, bestias/enemigos recurrentes y emplazamientos de ciudades. Mejorar color, material, ondulación y luz sin bloquear su ocupación.
- **Geografía permanente ≠ entidades dinámicas**: fauna, nodos, enemigos, ciudades y temporizadores no se incrustan en Blender. Mantener progresión real I–X y encuentros únicos con su derrota persistida; evitar duplicar gateway o inventar multijugador.
- Modelos ya existentes: Engendro Bastión II near/far y Jabalí de roca de capítulo III pendiente de rig/integración. Consultar `docs/ELDORIA_WORLD4X_REUSE_EXISTING_CREATURES_20261023.md` y `pipeline/tripo-art-brain/catalog.json`. Para nodos comunes, preferir biblioteca modular controlable Blender + variantes/LOD, no nuevas generaciones de Tripo por defecto.
- Los modelos antiguos de `main` **no son canon artístico** por estar en producción. Una sustitución visual podrá ocultarlos, pero jamás borrar por accidente las funciones y guardados que representan.
- **Separar alcance:** producción del terreno, integración de estado/respawn, marchas y UI pueden tener propietarios distintos; investigar y pedir/reconciliar relevo real antes de escribir sus archivos. El fixture AI no autoriza modificar `LocalGateway` o `GameState`.

## Fuentes exactas de recuperación

**Proyecto:** PR [#100](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/100), rama `feature/world4x-region1-canon-20261022`, workstream `eldoria-world-region-1-canon-20261022`. **El PR es vivo; sus datos actuales pueden superar estos ensayos.** Los enlaces a documentos de `e230ac8...` fijan la **evidencia histórica** sin sobreescribir su historia.

| Versión, ámbito | Evidencia Unity y fuente | Qué demuestra / límite |
| --- | --- | --- |
| I (ribera) | [Run 39953231775](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39953231775), commit `14d134cf4c719824a6db6a28761556bebdbbf8c8` | Comparador conservado |
| K | [Run 39957867110](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39957867110), commit `a911bd153713e3aadf7eb7422faa07d91bbf28d8` | Copas y margen físico, frente a I |
| O | [Run 39965511083](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39965511083), commit `feabc4d514b9ce4eed66f777d70d46b33cf167c8` | Bosque estilizado y orillas asimétricas, no 8/10 |
| T → V | [Run 39979131350](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39979131350), [run 39983210334](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39983210334) | Sector conectado; micro-pigmentos casi invisibles |
| AG / AH | [Run AG 40012204581](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40012204581); [AG/AH revisión](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/SECTOR_AG_AH_REAL_UNITY_REVIEW_20261023.md) | AG retenida, AH roca rechazada |
| AI: ampliación y ensayo de población | [Run 40024930128](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40024930128), fuente `cb0ef556dcaef508f140f431e450e4b279687bfc`, [informe y reproducción](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/LIVING_SECTOR_AI.md) | +19.200 m², 4 reservas de ciudad, **17 instancias reales**, cinco recolocaciones QA; NO backend ni mundo completo |
| Scripts editables (históricos) | [Autor del sector](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/cb0ef556dcaef508f140f431e450e4b279687bfc/docs/evidence/world4x-canon-20261022/authoring/extend_living_sector.py), [piloto híbrido](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/cb0ef556dcaef508f140f431e450e4b279687bfc/Unity/Assets/Eldoria/World4XCanon/RegionOneRiberaPilot.cs), [capacidad QA](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/cb0ef556dcaef508f140f431e450e4b279687bfc/Unity/Assets/Eldoria/World4XCanon/RegionOneSectorCapacity.cs) | Reutilizar técnicas, no copiar código sin cotejar con `main` |
| Métodos de origen | [Método profesional](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/PROFESSIONAL_METHOD_20261023.md), [decisión Blender](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/e230ac8b4478c79b9b067e513c2ec34d5e9b0019/docs/evidence/world4x-canon-20261022/BLENDER_AUTHORING_DECISION_20261023.md) | Por qué pivotamos, límites de fuentes y cámaras |

**Conservación:** .blend/GLB/PNG en runs/artefactos concretos y paquete persistente descrito en `LIVING_SECTOR_AI.md`. **Los artefactos de GitHub Actions pueden caducar;** conservar fuente y evidencia versionadas en Git o en el paquete persistente, sin depender solo del ZIP del runner.

## Plantilla breve obligada para la evidencia de futuros ciclos artísticos

```text
Objetivo visual y referencia SHA:
Escena/área, orientación, cámara y zoom exactos:
Base recuperada (.blend/GLB SHA, commit, workstream titular):
Defecto observado desde Unity:
Hipótesis y operación autoral Blender efectuada:
Material/UV/LOD y transformación de importación:
Run nativo exacto + XML Unity + PNG A/B misma cámara:
Juicio independiente: MEJORA / SIN CAMBIO / EMPEORA (por qué):
Gameplay, nodos/alturas, guardados y FPS físico: PASS/FAIL/NO PROBADO:
Decisión: RETENER / REVERTIR / CAMBIAR MÉTODO; siguiente trabajo:
```

**Límite honesto:** hemos desbloqueado una **cadena de producción visual editable, comprobable y expansible** y varias mejoras visibles. **No** se ha demostrado todavía la fidelidad final de toda Región 1, un 8/10 aprobado por el propietario, un multijugador vivo implementado ni FPS móviles certificados. Ese es trabajo futuro, no una propiedad ya obtenida por documentarlo.
