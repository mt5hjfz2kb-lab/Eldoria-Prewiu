# Valoria: prueba de nueva base artística (0 €)

Fecha: 2026-09-26. Punto de partida: `docs/VALORIA_ART_ASSET_GATE.md`; criterio: `docs/ELDORIA_VISUAL_BENCHMARK.md`. El núcleo y la escena jugable actuales permanecen intactos durante esta prueba.

## Cobertura necesaria, por familias y no por objetos sueltos

| Familia | Mínimo para el benchmark | Situación de la nueva selección |
| --- | --- | --- |
| Fortaleza | Cuerpo alto habitable, torres de silueta desigual, murallas, portal, arcos, contrafuertes, pasos, cubiertas y piedra dañada compatible | **Sin resolver**. La fortaleza CC0 elegida para la prueba tiene paramentos, puerta y almenas texturados, pero es baja y no aporta palacio antiguo, torreones altos ni tejados monumentales. |
| Barrio | Viviendas, talleres y cubiertas de distintas alturas, fachadas y paleta compatibles con la fortaleza | **Sin resolver**. Las casas Slavic World Free permiten medir la mezcla, pero pertenecen a una familia aldeana diferente. |
| Ruina | Arcos y masas rotas del antiguo palacio, piezas que compartan piedra y escala con el Bastión | **Sin resolver**; roca natural no reemplaza arquitectura rota. |
| Terreno | Terreno esculpible, cliffs y estratos, suelo/camino, transiciones piedra/tierra/musgo, decals | Terreno Unity y roca CC0 para la prueba. Faltan cliffs conectables, camino materializado y decals de unión. |
| Paisaje | Árboles de distintas especies y edades, arbustos, bosque lejano, cordillera | Tres prefabs URP existentes permiten un ensayo. Faltan familias de sotobosque y fondo compatibles en estilo y LOD. |
| Brecha | Roca y suelo alterado, vegetación muerta, fisuras y bruma territorial | **Sin resolver**. No se simula con geometría violeta provisional en esta prueba. |

## Shortlist muy reducida

| Candidato | Fuente / licencia | Resuelve y combina | Límite / URP / decisión |
| --- | --- | --- | --- |
| [Modular Fort 01](https://polyhaven.com/a/modular_fort_01) | Poly Haven, [CC0](https://polyhaven.com/license) | Piedra erosionada, muros, almenas, portón y arcos; combina con sus propias texturas PBR y roca natural. | Fuerte histórico bajo, sin cuerpo principal vertical ni tejados. FBX se importa en Unity; materiales requieren conversión manual a URP Lit. **Probar como candidato, nunca aprobar como Bastión completo por ficha web**. |
| [Rock Moss Set 01](https://polyhaven.com/a/rock_moss_set_01) y [Mountainside](https://polyhaven.com/a/mountainside) | Poly Haven, CC0 | Roca cercana y futuro relieve distante con base de luz/textura realista. | Roca detallada (~63k tri el set de seis): separar, LOD/batching y comprobar escala. FBX/material URP manual. Mountainside aún no importado ni validado. |
| [Medieval Village MegaKit](https://quaternius.com/packs/medievalvillagemegakit.html) | Quaternius, CC0 (edición gratuita: FBX/OBJ/glTF) | Familia grande de casas, tejados, piezas de muro y escalera de estilo uniforme. | Siluetas simples/estilo estilizado; mezclarla con Poly Haven realista crearía collage. Proyecto Unity preconfigurado en edición aparte; conversión URP de modelos gratuitos manual. **Alternativa estilizada solo si la captura confirma nivel objetivo; no mezclar con la prueba realista**. |
| [URP Tree Models](https://assetstore.unity.com/packages/3d/vegetation/trees/urp-tree-models-253340) ya importado | Unity Asset Store, licencia de paquete existente | Tres árboles importables URP; no requiere pago adicional y ayuda a evaluar coherencia con roca. | Solo tres variantes, insuficientes para un valle. Los materiales deben juzgarse en captura. |

Kenney Castle Kit y Bublik quedan fuera del Bastión por su escala/estilo simple. RenderCrate Medieval Castle Kit queda **excluido** por restricciones “Editorial Use Only” en sus términos; los anuncios de “free” no equivalen a permiso para un videojuego comercial. Los kits de Sketchfab aislados tampoco se aprueban sin comprobar descarga, licencia individual, autores y calidad del conjunto. El pack `DETAILED - Medieval Castle` y su Village coherente serían una solución arquitectónica, pero son de pago y no cumplen 0 €.

## Prueba aislada elegida

`scripts/fetch-valoria-art-gate.ps1` descarga y comprueba MD5 de los diez ficheros FBX/JPG oficiales recogidos en `scripts/valoria-art-gate-assets.json` (también en el runner). Los binarios descargados quedan fuera de Git. Después `Eldoria.EditorTools.ValoriaAssetGateCapture.Capture` convierte material difuso y normal a URP Lit, monta un terreno con altura, una fortaleza amurallada, roca, dos viviendas existentes, tres tipos de árboles e iluminación cálida. Genera `AssetGateCaptures/asset-gate-establishing.png` y `asset-gate-oblique.png` a 1280×720, sin modificar `VisualWorld` ni gameplay. Su objeto es **medir** compatibilidad visual; el hecho de que los modelos estén importados no aprueba la mezcla. La referencia se compara con `valoria-establishing.png` del arte certificado `0442f880`.

### Puerta de decisión

1. Abrir ambas capturas auténticas de Unity; revisar mallas, UV, color, shaders, tejados y discrepancia de escala.
2. Si el fuerte continúa leyendo como bloque bajo o las casas/árboles parecen de otro juego, **rechazar la combinación** y no tocar Valoria. Obtener una familia de arquitectura medieval vertical con palacio, torres, ruina y cubiertas compatibles, más cliffs/terreno y bosque; con 0 € quizá requiera modelado original dirigido y texturas CC0 o la cesión explícita de un kit adecuado.
3. Solo si hay salto de conjunto claro, comenzar integración controlada Bastión→barrios→terreno→vegetación→fondo→Brecha y validar EditMode, PlayMode y Windows con tres vistas.

Fuentes importadas: Poly Haven `modular_fort_01` FBX/texturas JPG 1K y `rock_moss_set_01` FBX/texturas JPG 1K, originales CC0, descargados del API público oficial el 2026-09-26. No se distribuyen previsualizaciones de la web como assets. Esta evaluación no certifica todavía materiales definitivos, LOD, URP en el runner ni calidad estética de las capturas.
