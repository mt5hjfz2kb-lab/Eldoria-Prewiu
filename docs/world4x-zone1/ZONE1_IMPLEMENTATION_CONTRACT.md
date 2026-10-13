# Eldoria — Zona 1, prototipo 4X aislado

Estado: **especificación ejecutable inicial, no escena Unity validada**. Rama: `prototype/world4x-zone1-zoom-reference-20261013`.

## Contrato visual aprobado (referencias conceptuales)
- Cámara más cenital que Valoria, tres escalas: **global**, **regional** y **local** sobre un único espacio de coordenadas; no son tres mapas independientes.
- **La Brecha** ocupa el centro, como corrupción del suelo, no como edificio. Alrededor, Zona 1 grande y transitable con amplio espacio de asentamiento.
- Predominan praderas abiertas; bosques y montañas dispersos, poco agua; **una sola salida marítima al sur** para expansión futura.
- Nodos iniciales: madera, piedra y comida; campamentos hostiles y ruinas. No introducir recursos tardíos en Bastión I–X.
- Ciudad 4X pequeña y modular: reutilizar `Unity/Assets/Eldoria/Resources/WorldPlayerCity/PlayerCity_v1.glb` donde su licencia, compatibilidad y evaluación visual lo permitan.

## Alcance de la primera prueba
1. Generar distribución lógica determinista de una Zona 1 extensa, con Brecha central y mar solo en extremo sur.
2. Visualizar una porción regional con cámara móvil, seleccionar ciudad/nodo/campamento, hacer pan y zoom; conservar coordenadas entre escalas.
3. Cargar/desactivar únicamente celdas cercanas a cámara; nunca instanciar todo el continente.
4. Reutilizar bibliotecas existentes (WorldRouteKit, WorldResourceKit, Holotna, Quaternius) antes de crear nuevas familias.
5. Validar A/B en cámara Unity real, legibilidad móvil y rendimiento. Sin reclamar aprobado visual ni producción hasta comprobarlo.

## Separación y seguridad
- Región 1 vigente conserva `WorldRegion1Runtime.cs`, sus pruebas, WebGL y Pages. No sustituir su escena ni publicar desde esta rama.
- Bastión I conserva Valoria, artista Qwen/Blender, workflows y runner Windows pesado. Este prototipo no reserva esos recursos.
- La nueva implementación vive únicamente en `tools/world4x-zone1/**` y `Unity/Assets/Eldoria/ArtTests/World4XZone1/**`, y solo podrá integrarse tras conciliación con los propietarios activos.

## Definición de éxito
- Tres zooms funcionales sobre el **mismo terreno**; al acercar la cámara los puntos conservan ubicación.
- Brecha centrada y legible, amplias áreas construibles, costa exclusivamente al sur.
- Variantes reutilizables para ciudades, nodos y POI; evaluación artística real contra las referencias, no solo compilación.
- Sin impacto en Bastión I ni en el World Region 1 publicado.

## Evidencia disponible
- `docs/ELDORIA_ASSET_LIBRARY_ROADMAP_V1.md`: ciudad estratégica v1, kits y carencias.
- `SESSION_HANDOFF.md`: Región 1 tiene WebGL candidato y QA técnicos, pero ART FAIL.
- `pipeline/active-workstreams.json`: propietarios actuales.

Siguiente paso técnico: ejecutar generador lógico local, construir vista aislada en Unity sin modificar el runtime canónico, capturar los tres zooms con cámara idéntica y evaluar visualmente.
