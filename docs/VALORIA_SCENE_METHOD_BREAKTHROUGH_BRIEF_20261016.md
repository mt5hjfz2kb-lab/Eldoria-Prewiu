# ELDORIA / VALORIA — EXPERIMENTO DE ESCENA LIMPIA (16-10-2026)
Autoridad: propietario, después de la auditoría artística. Coste 0 euros. Unity 6000.3.23f1 + URP 17.3, Android/iOS.
ESTADO: autorizado para un futuro chat ejecutor en rama NUEVA y directorio NUEVO.

## Trabajos expresamente PAUSADOS
- integration/valoria-bastion1-player-scene-20261012 (Bastión I modular);
- art/valoria-63-individual-quality-uplift-20261015 (63 módulos).
No reactivar, borrar, revertir, fusionar ni modificar sus fuentes; ver docs/VALORIA_TWO_WORKSTREAMS_OWNER_PAUSE_20261016.md y pipeline/active-workstreams.json. Verificar jobs vivos y ownership del runner Windows antes de actuar. Las nuevas pruebas se llevan en el MISMO proyecto de Unity pero en otra escena y otra rama propias; NO iniciar otro videojuego, otra biblioteca ni otro agente IA.

## Hipótesis visual y evidencia
El kit modular es técnicamente utilizable, pero las imágenes Unity reales de Bastión I no convergen aún con la referencia: arquitectura rígida, torres oscuras, llanura demasiado vacía, acantilados y horizonte artificiales, baja cohesión de iluminación y composición. No resolver a base de iterar micro-ítems. Este diagnóstico concuerda con los ART_FAIL documentados de varias ejecuciones técnicamente SUCCESS.

Referencia exacta: references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg, SHA256 8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689, 1536x1024. Macroformas: puerta inferior y puente delante; bastión elevado arriba; dos plataformas; pradera central libre con ruta y escaleras; casa/cabaña a la izquierda; tienda de campaña a la derecha; acantilados, mar, pinos y luces cálidas. No estirar ni recortar el objetivo para falsear coincidencias.
Baseline técnico/visual reciente: rama Bastión I congelada, run Unity 38940426182 y su artifact, plus revisar ultimo run 38943004088 (FAIL) sin inferir valor estético de resultados. Conserva imágenes originales y SHA de escena/cámara. 
Vía SHARP/2.5D ya demostrada solo para HOME, pan/zoom acotados y geometría de soporte en docs/VALORIA_VISUAL_AUTHORITY_RESET_V1.md, run 37474695980. No confundir esto con 3D totalmente navegable.
Investigación IA: docs/evidence/ELDORIA_ARTIST_FAILURE_FORENSICS_20261015.md documenta cambios geométricos genuinos pero no criterio artístico autónomo; no volver a invertir el sprint en calibración del cerebro.

## Misión
**NO CONSULTORÍA.** Crear escena Unity limpia y funcional en directorio nuevo y comparar dos soluciones sobre la misma composición/cámara:
A) 3D híbrido auténtico: arquitectura protagonista como conjuntos integrados (hero meshes), mejores módulos existentes solo cuando beneficien, suelo, roca, agua, árboles, caminos, luz y atmósfera coherente. Utilizar GLBs ya verificables o CC0 con procedencia.
B) 3D interactivo con fondo/atmósfera 2.5D desde proyección/SHARP/parallax, solo donde no rompa navegación, oclusión, selección, zoom, construcción ni expansión. No convertir la imagen de referencia en una captura a pantalla completa inerte.
Ninguna opción requiere un proyecto nuevo ni 63 assets nuevos. Un solo workstream nuevo y dos variantes comparables, no múltiples pilotos compitiendo.

## Secuencia profesional obligatoria
1. Leer los documentos vigentes, verificar herramientas, runner, active claims y último main. Reclamar un scope aislado. Preservar código gameplay/estado/escenas ajenas.
2. Congelar baseline con same-camera imágenes nativas y hashes. Establecer cámara original HOME, FOV, resolución 3:2 y adaptación 16:9/móvil sin distorsión.
3. Composición macro COMPLETA antes de detalle: formas, dos accesos, alturas, terreno, puente, escalera, grupos arbóreos, casa, campamento, mar, fondo. Rechazar composición que ya falla en greybox.
4. Construir A completa y revisar en Unity desde cámara real; corregir principal factor de área/peso visual, no micro-asset. Conservar capturas y tiempos de ejecución.
5. Construir B acotada y someterla al mismo zoom, pan, control táctil/hit tests. Descartar B si solo imita una captura estática o distorsiona al desplazarse.
6. Producir comparativa 4 columnas: referencia, vieja escena, A, B, con mismo encuadre/condiciones y filas landscape 3:2, 16:9, portrait y estados pan/zoom. Aportar capturas Unity nativas, fuente y artefactos.
7. Tests: compilación Unity, origen y hashes de GLB/materiales, separación arte/proxies, contacto y costuras, colliders/hotspots, carretera-escalera, selección y navegación, zonas vacías y expansión; coste GPU/CPU móvil si medido en dispositivo, sin inventar FPS.
8. Revisión visual independiente: puntuaciones justificadas 0–10 de composición, silueta arquitectura, paisaje, materiales, iluminación/atmósfera, lectura móvil, interacción, viabilidad Bastión II. No aprobar por CI o % píxeles cambiados. Buscar mejora mayor perceptible (~1 punto) y suelo de calidad global >=6 si es realista, sin caída funcional. 8/10 sigue objetivo futuro.
9. Promover solo concepto ganador dentro de la propia rama experimental; NO fusionar a main sin ART PASS real y ausencia de regresiones. Si A y B no superan baseline, reportar NO WIN, causa demostrable y mejor siguiente método; no ampliar indefinidamente.

## No repetir estos errores
- No fabricar otra biblioteca, ni otro agente IA, ni largas auditorías sin escena.
- No suponer que una pared o torre 7/10 convierte la escena en 7/10.
- No tapar paredes antiguas dejando overlays; ensamblar físicamente y desactivar visuales históricos donde proceda SOLO en escena experimental.
- No usar referencia como fondo fullscreen simulando jugabilidad.
- No ignorar valor de recursos previamente aceptados o experimentos SHARP limitados.
- No usar el runner si hay trabajo ajeno/pausado todavía activo.
- No gastar ni usar servicios de pago ni créditos; no publicar builds del juego sin autorización.

## Referencias
- docs/VALORIA_ART_PRODUCTION_RESET_V1.md (target, cámara y blockout antes de asset families).
- docs/ELDORIA_PRODUCTION_ART_SOURCE_PIPELINE.md (greybox/support/production).
- docs/UNITY_ART_FINISHING_PROFESSIONAL_PLAYBOOK.md.
- docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md.
- https://dev.epicgames.com/documentation/en-us/unreal-engine/designer-01-project-setup-and-level-blockout-in-unreal-engine
- https://docs.unity.com/en-us/engine/6000.3/manual/working-with-scenes/scenes-working-with
- https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/in-urp
- https://docs.unity.com/en-us/engine/6000.0/manual/visual-effects/decals/renderer-feature-decal/decal
- https://polyhaven.com/license
- https://slime_contactme.artstation.com/projects/6LWqaN

## Entrega
Al final, escena .unity NUEVA jugable aislada en GitHub, scripts/recursos propios, source SHA, última prueba nativa exacta y artifacts, capturas comparables reales, lista explícita de fallos, conclusión A/B/no-win y decisión argumentada. Informe no equivale a entrega sin la escena. Ejecución continua dentro de la sesión; no detenerse en un informe intermedio. Únicos finales: entregable real con gates demostrados o bloqueo genuino documentado. Tiempo orientativo para la primera comparación: 2–3 jornadas efectivas, NO promesa ni autorización de estudios paralelos sin fin.
