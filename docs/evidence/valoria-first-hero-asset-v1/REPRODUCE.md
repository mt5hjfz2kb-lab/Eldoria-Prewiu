# Valoria — continuar esta pieza, sin empezar otra puerta

Repositorio: mt5hjfz2kb-lab/Eldoria-Prewiu. Rama: art/valoria-first-hero-asset-v1-20261011. Fuente de producción seleccionada: 9ca81653880395f13acd2ed1fc806fcd5f3a7155.

## Punto de partida

Se abrió la referencia canónica y se inspeccionaron V5 y V8. Se eligió reconstruir una sola puerta compacta de torres cuadradas, con bóveda real, rastrillo, banderas azules y piedra de una familia tonal coherente. No se modificó main ni la rama de calibración V5. El cuerpo conserva su arquitectura durante las correcciones posteriores.

## Reproducción de fuente

1. Obtener el HEAD vivo de esta rama; leer AGENTS.md y solo active del registro. No recuperar un SHA antiguo sobre el HEAD actual.
2. Descargar Blender 4.2.5 LTS oficial. SHA256 del tar Linux: cd28ab8164d88237c0c45a188ef7e4974bcb8c651de15fcbc3635166e584e4b3. La distribución Ubuntu 4.0.2 carece de OpenImageDenoiser: no usarla con este render.
3. Ejecutar `python3 tools/art-rd/first-hero-support/make_surfaces.py` con Pillow y NumPy.
4. Recuperar el puente existente: `python3 tools/valoria-art-brain/bridge.py --check`; stage fuera del repositorio. El origen fijado es art/studio-reuse-handoff-20261011, SHA e137aaf713c682509cacc62cd1b3279c140a811c.
5. Obtener addon.py de BlenderMCP en commit 7a0373ec9199183cb460068c4f96aed9c579fb4f, verificar SHA256 eb0facf69781a30e69792532087d8d41c6a14fcd323353250abe7988ee297fa5.
6. Definir `ELDORIA_HERO_PHASE=refine` y `ELDORIA_BLENDER_MCP_ADDON=/ruta/absoluta/addon.py`. Ejecutar `blender -b -t 8 --python tools/art-rd/valoria_first_hero_asset.py`.
7. Ejecutar `blender -b -t 8 --python tools/art-rd/first-hero-support/verify_export.py`. Reimporta el GLB y produce comparación real con V5 y LOD. No aceptar LOD solo porque exporta: inspeccionar tela, almenas, bóveda y normales.
8. Para reunir texturas embebidas, materiales, hashes, comparación y ZIP sin reconstruir, ejecutar `python3 tools/art-rd/valoria_first_hero_delivery.py`.
9. Abrir los renders reales. El director decide: conservar o rechazar, sin usar un SUCCESS como aceptación artística.

El workflow .github/workflows/valoria-first-hero-asset.yml reproduce estos pasos, guarda archivos editables en la misma rama y genera un artefacto. Se activa por push de las fuentes o workflow_dispatch. Un chat con GitHub puede editar estas fuentes mediante contenidos/blob/tree/commit/ref y consultar /actions/runs, /jobs y /artifacts. No utilizar issues como bus de CI. Un commit documental no necesita reconstruir el recurso.

## Reanudar Unity cuando el recurso esté libre

No hay Unity Editor ni Ollama en el entorno local utilizado. El registro de main reserva el Windows runner para valoria-mesh-only-worldspace-proof-20261010; ausencia de jobs en ejecución no equivale a liberación. No se tomó ese runner.

Reusar la integración histórica en integration/valoria-gatehouse-unity-v1-20261011: .github/workflows/valoria-gatehouse-real-unity-v1.yml y ValoriaGatehouseRealUnityV1.cs. En una rama aislada y con reserva válida, adaptar SOLO el import de la puerta a este GLB exacto, mantener la escena de prueba mínima, verificar SHA256, materiales y escala. No reconstruir Valoria ni modificar gameplay/main. Capturar cámara estratégica, móvil y detalle; persistir meshes/materiales/texturas, cerrar Unity y reabrir la escena en otro proceso. El método VerifySavedScene existente prueba regeneración de recursos. Se necesita evidencia nueva para esta puerta: los PASS históricos no sirven.

## Qué aportó el cerebro

La observación de imágenes cambió la elección de base. La revisión del primer render cambió spandrels, relieve de esquinas, llamas y encuadre. BlenderMCP ejecutó modificaciones acotadas y verificables sobre la misma pieza. El puente evitó copiar otra arquitectura del estudio. Los mapas de PBR son bitmaps portables, no solo ruido de shader. La política recuperada verifica decisiones contradictorias; no decide arte por sí sola. Qwen no se ejecutó: conexión local rechazada y runtime ausente. CC0 no se utilizó en la pieza: todas las superficies entregadas son de autoría original para Eldoria, evitando un patrón de ladrillos encima de mallas de ladrillos. No se afirma haber ejecutado todo el cerebro.

## Límites

La fuente y su GLB no certifican rendimiento físico Android/iOS. 14 materiales y aproximadamente 114 mil triángulos en LOD0 exigen selección de LOD y medición antes de producción móvil. El artefacto es una candidata hero de fuente; la certificación final dentro de Unity requiere esa ejecución real.
