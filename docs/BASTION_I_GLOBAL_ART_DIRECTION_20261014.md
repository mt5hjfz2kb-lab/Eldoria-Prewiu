# BASTIÓN I — DIRECCIÓN ARTÍSTICA GLOBAL, CAMBIO DE MÉTODO
Fecha: 2026-10-14 · Estado: instrucción de producción, NO aprobación de arte.
Repositorio: Eldoria-Prewiu · Rama viva: integration/valoria-bastion1-player-scene-20261012.
Motor: Unity 6000.3.23f1 · Presupuesto: 0 € · No tocar Región 1 ni mapa 4X.

## Problema observado por el jugador y revisión independiente
Las capturas auténticas muestran una explanada de césped vacía, murallas desconectadas y poco relieve creíble. El recinto no se lee como una fortaleza habitada. Un A/B auténtico de guardia en el run 38522636764 alteró solo 20.216/1.440.000 píxeles (1,404 %) en la cámara principal; esto demuestra **diferencia detectable**, NO mejora de calidad ni salto global. A2 y B fueron pruebas; B, fragmentada, fue rechazada. La A2 es material de exploración todavía SIN ART PASS. Las ejecuciones posteriores 38523902008 y 38523942615 fallaron en el ensamblador Blender: corregir el circuito sin promocionar resultados.

## Visión espacial antes de autoría
Objetivo: **composición medieval legible en tres segundos** desde la cámara de jugador, no colección de props. Primer plano: puerta y ruta con suelo desgastado y enmarcado creíble. Plano medio: patio con contraste intencional de zonas transitadas/suelo irregular y perímetros arquitectónicos continuos, NO desierto verde ni cintas superpuestas. Fondo: ciudadela superior y laderas escalonadas integradas con roca y muralla. Una sola gramática de escala, piedra envejecida, madera y hierba, sin saturación ni tintes aislados.

Primero definir 2 alternativas **genuinamente incompatibles de grandes masas** (no 2 colores sobre el mismo montaje): (A) recinto defensivo habitado, muros y patio escalonado; (B) casco antiguo en bancales, cortes de terreno y contención de piedra. Elegir por mayor calidad de lectura de cámara/continuidad del suelo, no por rapidez de exportación. El generador Blender puede montar prototipos, pero si decisiones concretas de espacios/medidas son fijas de un script, etiquetar su autoría como determinista; Qwen no recibe atribución de geometría que no hizo.

## Restricciones innegociables
- Fuente jugable y estado previo intactos como base recuperable, sin reconstrucción ni rama nueva.
- Mantener puerta hero, escalera y ruta axial; corredor protegido x entre -7 y 7.
- Preservar dos parcelas 10m x 10m centradas en (-16,3) y (16,3), con sus márgenes y accesos. No pintar nuevos edificios encima.
- No alterar economía, guardado, cámara, nodos de recursos, colisiones funcionales, scripts de Región 1 ni activos 4X.
- Debe funcionar en las capturas Unity reales, no únicamente en Blender. No usar pago ni créditos nuevos.

## Encargo operativo al director artístico IA
1. Analizar una misma captura nativa de referencia, sus defectos y el inventario de mallas auténticas. Formular una **hipótesis espacial concreta**: grandes volúmenes, transición suelo/muralla, lectura primer/medio/fondo, proporciones/posiciones protegidas.
2. Producir decisiones de autoría registradas **antes** de tocar Blender: elección, causas, límites, lista de operaciones editables y alternativa distinta si falla. Aceptar herramientas BlenderMCP y activos libres ya disponibles.
3. Ejecutar un cambio macroscópico controlado; conservar source/candidate hashes y .blend/GLB. No confundir scripts rígidos con creación espacial auténtica IA.
4. Capturar ANTES/DESPUÉS con cámara Unity idéntica y calidad equivalente. Evaluar por separado silueta, profundidad, arquitectura-suelo, materialidad, patio con intención, identidad medieval, legibilidad del jugador.
5. Ante rechazo visual cambiar de concepto, NO iterar tintes, arbustos o un cobertizo; nunca declarar ART PASS a partir de los píxeles o SUCCESS técnico.

## Umbral cualitativo / anti-falso éxito
**Pasa a revisión artística solo si** las diferencias son reales, visibles desde cámara y respetan gameplay. ART PASS exige inspección visual independiente del conjunto y mejora convincente respecto al baseline; un porcentaje de píxeles es un detector de invisibilidad, no una calificación. Si el patio sigue siendo plano y dominante, si la fortaleza no forma una sola composición, o si las piezas parecen pegadas, resultado **ART REJECT**, incluso con workflow SUCCESS. Conservar ambas alternativas y explicitar razones de descarte. No avanzar a microdetalles antes de resolver el paisaje y la arquitectura.

## Fuentes verificables
- [Bastión I corrida nativa anterior](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38522636764).
- [Primera A2 posterior fallida](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38523902008).
- [Última A2 posterior fallida](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38523942615).
- docs/VALORIA_VISUAL_BIBLE.md y docs/UNITY_ART_FINISHING_PROFESSIONAL_PLAYBOOK.md.
