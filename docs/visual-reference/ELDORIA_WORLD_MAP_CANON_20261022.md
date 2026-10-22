# ELDORIA — MUNDO 4X · REFERENCIA VISUAL CANÓNICA DEL PROPIETARIO

**Aprobación conceptual del propietario:** 22-10-2026. **Estado:** CANON DE DIRECCIÓN VISUAL, NO escena implementada ni ART_APPROVED en Unity. **Presupuesto:** 0 €. **Ámbito:** Región 1, tres distancias de cámara. **Prevalece sobre** propuestas conceptuales anteriores incompatibles; no sustituye contratos funcionales, de progreso, guardados, UI ni la referencia visual de Valoria.

## Imágenes originales aprobadas e identidad

El **conjunto binario original de cuatro PNG** está conservado en la Biblioteca del proyecto de ChatGPT, carpeta `/Eldoria/Referencias_Canonicas/Mundo/`. Los tres archivos de vistas son recortes exactos del tríptico, no reinterpretaciones ni imágenes nuevas. Verificación SHA-256 realizada sobre los PNG archivados:

| Archivo en Biblioteca | Tamaño | SHA-256 |
| --- | --- | --- |
| `00_triptych_aprobado_2026-10-22.png` | 1536×1024 | `51df0c0b8b34619fbe3052e5f453587dcee17c50968cde742d3b3bb5cb384a35` |
| `01_vista_cercana.png` | 512×1024 | `bb053cf3099a202eff472a53207169dfba522b93c0c5b7bff0fed05b96438fd9` |
| `02_vista_regional.png` | 512×1024 | `82a38cf89e9a8ad249739496de4c972a8008df29ec4efc946c3a22273f88f316` |
| `03_vista_panoramica.png` | 512×1024 | `68cd5b898cf3675db537b7ddceb8082befe955895864079a0ac26bdf4fb37144` |

**Alcance del archivado actual:** los PNG están materialmente en la Biblioteca de ChatGPT. **Los binarios PNG no están todavía copiados al árbol Git de este repositorio**; no inventar rutas GitHub a PNG inexistentes ni declarar ese paso cerrado. Para obtenerlos desde otra sesión de trabajo, recuperar los cuatro archivos de la carpeta exacta y validar sus hashes antes de usarlos. Este contrato textual sí reside en el repositorio.

## Canon visual por escala

**Cercana:** terreno transitable, baja densidad de árboles, predominio de nodos de recolección verdaderos y amenazas/fauna claramente identificables. No mostrar familias de recursos, enemigos o premios antes de su desbloqueo real de Bastión I–X. Engendros solo según progresión canónica (Bastión II). **Sin campamentos inventados**, ni edificios decorativos que hagan de enemigos, ni nodos falsos. La vista no necesita calidad gráfica de la ciudad.

**Regional:** perspectiva elevada ligeramente cenital; región extensa, varias zonas reconocibles, caminos, bosques moderados, pocas montañas y agua secundaria. **Sin niebla**. La Brecha, central, es una **fractura/anomalía geológica situada en el suelo**, nunca un monumento, torre, portal elevado u obelisco. POI/ciudades solo si son reales o, en el concepto, inequívocamente futuros; no fingir otros jugadores conectados.

**Panorámica:** casi **toda la Región 1**, mucho más alejada que regional pero algo más cercana que los primeros bocetos globales. Terreno claramente predominante, **mar limitado a un borde** con posibilidad geográfica de futuro muelle no implementado, agua interior discreta, pocas montañas y bosques como masas. Brecha geológica central identificable. **Niebla ligera y estrecha solo en los bordes exteriores** para insinuar regiones aún no disponibles; nada de niebla regional. Sin nodos, criaturas o ciudades pequeñas superpuestas; mapa legible y amplio, no una isla flotante/diorama.

**Identidad del espacio:** una sola geografía consistente entre escalas; la cámara cambia de detalle, no teletransporta a mapas incompatibles. Mundo estratégico 4X con perspectiva algo más cenital. Las imágenes son referencias de composición y lectura, no especificación pixel a pixel ni aprobación de nuevos sistemas.

## Construcción simplificada aprobada como vía de trabajo

1. **Una única base geográfica** con diferentes niveles de detalle/visibilidad según zoom; nunca tres mapas hechos desde cero.
2. Terreno continuo y de relieve suave; limitar montañas, costas y ríos complejos. Rutas y cursos de agua preferentemente mediante material/textura o geometría superficial barata donde la cámara lo permita.
3. Pocas familias reutilizables de árboles, piedras y nodos, con distribución y siluetas legibles. Cerca: 3D moderado. Lejos: grupos, representación simple o masas de bioma, sin miles de instancias simultáneas.
4. Brecha integrada en terreno mediante material y fractura geométrica mínima, efectos limitados. Niebla panorámica barata de borde, no simulación volumétrica.
5. **Reutilizar antes de generar:** Unity 6000.3.23f1 / URP 17.3.0, Blender y fuentes/kit existentes; Tripo/Qwen solo donde existe una carencia real demostrada y sin crédito/coste no autorizado. Probar primero en cámara de juego y en iPhone/WebGL, no fabricar arte caro que desaparezca con zoom.

**Estimación preliminar orientativa:** complejidad de **creación gráfica 4/10**, tras simplificar desde 5–6/10; **no** es una medición de esfuerzo, promesa de plazo ni valoración de la dificultad de la integración. Estabilidad, transición de cámaras, continuidad de escena, interacciones, persistencia y móviles tienen dificultad propia y requieren pruebas.

## Pruebas obligatorias antes de promoción al juego

- Capturas reales de Unity a las tres distancias, comparadas con **estos cuatro PNG originales** y revisadas en móvil horizontal/vertical; transiciones fluidas sin salto espacial ni detalles falsos.
- POIs/hotspots existentes siguen seleccionables; marchas, recursos, combate, Engendro, recargas/guardados y retorno a Valoria sin regresión.
- Perfil real de FPS, memoria y reinicios en dispositivos objetivo; la incidencia **#96** de reinicios de iPhone debe resolverse de forma independiente.
- V3 de interfaz debe recuperar fielmente referencia **propuesta 1** y ser aceptada por el propietario en build real. **No migrar Bastiones III–V** hasta que mundo, UI y estabilidad tengan su aprobación jugable.
- No aceptar cambios solo por CI verde, concepto bonito o captura simulada. Coordinar con el workstream vivo para no pisar escena/runner ni duplicar publicación.

**Incidencias:** [#95 — experiencia visual y mundo](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/95) · [#96 — reinicios iPhone](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/96).
