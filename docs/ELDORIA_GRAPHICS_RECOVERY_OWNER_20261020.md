# Eldoria — Ejecución de la recuperación gráfica 20 octubre 2026

Rama única: main. Workstream único ya reclamado: valoria-master-plan-layered-construction-20261016. Director actual: chatgpt-eldoria-visual-recovery-20261020. Presupuesto: 0 €. No publicación ni Bastión III.

## Auditoría de autoridad y alcance
- Autoridad principal: v0220/index.html, v0220/js/gameplay.js, docs/VALORIA_MASTER_PLAN_V1.md y docs/VALORIA_PROGRESSION_MAP_V1.md. El prologo temprano muestra Bastión I, aserradero, bosque, cantera, Corruptos, Bastión II, cuartel, reclutamiento y Engendro; un nodo de amenaza no obliga a una instalación de tiendas y viviendas residenciales claras se reservan para Bastión III. Material ajeno o adelantado queda fuera de la escena temprana.
- Datos jugables, collider/hotspot IDs, navegación de ciudad/mundo, economía, diálogos, guardados e historia son intocables. Ningún lanzamiento público/WebGL/APK/itch durante esta obra.
- Retirar visuales de la V16: sus dos arcadas/andamios en la pradera son un experimento técnico con arte no aprobado, no componentes del juego. Conservar fuente .blend/GLB y comparación histórica para aprendizaje; revertir únicamente escena y activación. No introducir otros barrios de reemplazo.
- Los campamentos hostiles previos son clones de prefabs del cuartel usados como presentación provisional. Retirar las tiendas y estructuras visuales, conservar POI/selección/reglas y sustituir únicamente con un lenguaje de Brecha/amenaza fundamentado en v0220. No usar tiendas militares como interpretación automática de Corruptos/Engendro.

## Mejor método probado a recuperar
- Biblioteca 8 prefabs: diseño geométrico y espacial antes de modelar, contratos de partes y pivotes, modificación selectiva de malla en Blender, BLEND/GLB/UV/material conservados por SHA, pruebas nativas de contacto, alturas, raycast/selección, formato móvil 16:9/vertical y PlayMode. Evidencia en docs/evidence/valoria-reference-clean/PRODUCTION_STATE.md native61-62, 70-74, 104-111.
- Biblioteca 63 assets: aprovechar únicamente método de biblioteca tipada, clasificación selectiva, familia común de materiales, licencias, pivotes, conectores y descarte si no escala en escena. NO reanudar rama histórica art/valoria-63-individual-quality-uplift-20261015, ni crear otra colección masiva.
- Acantilados y rocas: assets CC0 Poly Haven rock_moss_set_01 / modular_fort_01; recuperar técnica de extraer roca/tierra donante, reautorizar en Blender (escultura/macromalla/material UV, módulos de contacto, silueta medida de cámara), colocar con intersección/enterramiento y contacto real, no repetir piedras sueltas en césped; notas de técnica en docs/VALORIA_ZERO_EURO_ASSET_GATE.md y docs/VALORIA_ROCK_TERRAIN_BASTION_CONTACT_UPLIFT_V1.md.
- Rescate selectivo comprobado: docs/ASSET_LIBRARY_REPROCESSING_PASS_V1_RESULT.md recuperó Hero Bastion y Stone Architecture en lugar de generar 63 variantes. Archivos antiguos son donantes, nunca sustituyen a la escena actual por simple copia.

## Procedimiento obligatorio
1. Congelar SHA de escena actual y última aprobada, referencia visual aprobada, seleccionar y retirar NO-CANONICAL V16 y campamentos sin mecánicas afectadas; native Unity I–II PASS y A/B. Para los campamentos, dejar hotspot/ruta seleccionables aunque los renderers sean desactivados.
2. Hacer survey espacial de macroformas reales (Bastión, puerta, puente, suelo/ribera, aserradero, cuartel, transición) con cotas y bound de pantalla en horizontal/vertical; corregir primero el área dominante. En el mundo trabajar solo bosque, cantera, entorno Brecha/Engendro y rutas reales.
3. Fuente de producción: inventario aprobado/CC0 y comparación de alternativas; fuente .blend editable con modificación de geometría real, no scripted cube-stack. Previsualizar silueta, materiales, contacto antes de Unity. Si el método fracasa cambiar de técnica, no aplicar microtintes ni otro nuevo bloque urbano.
4. Gate Unity en cámara oficial, mismo luz/escala y gameplay; no aceptar ART PASS por CI SUCCESS, polígonos, Qwen o porcentaje de pixels. Metas: ciudad >=7.0/10; world >=5.5/10, usando pesos de docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md (25% silueta, 25% coherencia/contacto, 20% material, 20% estilo, 10% lectura cámara). Revisor independiente inspecciona PNG reales. No falsificar acreditación.

Conservar no autorizados en historia para forense; al llegar a las metas, integrar solo arte certificado sobre main; nunca declarar entrega por plan, nota estimada o un único asset excelente.
