# Valoria V6 — hallazgo geométrico real en sprites maestros (18/11/2026)

## Evidencia y clasificación

**Investigado a partir de fuentes reales.** Se recuperaron los PNG RGBA exactos de la rama artística `10dbc197d48ea8517c6a3dd6c22f1a2abf75d508` mediante [GitHub Actions #44280257279](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44280257279), sin ocupar Windows ni regenerar arte. El artefacto efímero `valoria-depth-v6-original-alpha-audit` contiene fuente original y `audit.json` con hashes. También se inspeccionó la captura auténtica de Unity del Bastión V del run `44133747143` (recuperada del archivo `44135322287`). La cuantificación de alfa que sigue **no es una nueva captura Unity ni una puntuación de calidad**.

## Defecto causal encontrado: confundir el píxel más bajo con la cimentación

Las imágenes premium incluyen piezas de primer plano (troncos, tocón, grano, sacos, vallas, decoración de patio) que descienden por debajo del cuerpo principal del edificio. El constructor V6 original ajusta el apoyo utilizando `AlphaFoot().minimumY`, es decir, la silueta opaca más baja. Esta propiedad no puede identificar si la muestra procede de un muro que soporta peso o de un objeto accesorio; se introduce riesgo de **alinear el decorado y no la construcción**.

La primera versión de nuestro piloto repetía parte del error: centraba la nueva sombra de contacto sobre una estrecha franja junto al píxel opaco inferior. La medición de la fuente demuestra que en Aserradero y Granero el extremo visible queda dominado por utilería adelantada. En lugar de mejorar, la sombra nueva podía subrayar el efecto de collage. **Esta hipótesis se ha descartado antes de aprobar arte.**

### Datos de alfa reales de la fuente (coordenadas en píxeles, origen arriba)

| Sprite premium | Tamaño RGBA | Último alfa material (>80/255) | Centro horizontal de la franja inferior original | Fila inferior con ocupación amplia ≥92 % del máximo de la imagen | Centro horizontal en esa fila |
|---|---:|---:|---:|---:|---:|
| Bastión V | 764 × 854 | 847 | ≈445 | 722 | ≈352 |
| Cuartel V2 | 1479 × 1479 | 1392 | ≈775 | 1262 | ≈705 |
| Aserradero V2 | 1506 × 1506 | 1424 | ≈609 | 1245 | ≈731 |
| Granero V1 | 1574 × 1574 | 1492 | ≈633 | 1415 | ≈743 |

**Interpretación:** Aserradero desplaza el centro de apoyo aparente horizontal ≈122 píxeles entre los dos métodos, y la fila de contacto ≈179 píxeles de la imagen fuente. Granero desplaza el centro ≈110 píxeles y la fila ≈77 píxeles. Son diferencias **del RGBA fuente**, no de pantalla ni de mundo Unity: deben multiplicarse por escala y PPU reales antes de concluir un desplazamiento jugable.

El umbral de ocupación del 92 % es un **criterio experimental explícito**, no una detección semántica infalible. En edificios con patios extensos, dos cuerpos separados o vegetación densa podría fallar; por ello solo se usa en B localizada y requiere evaluación directa de Unity.

## Corrección implementada, aún sin prueba nativa

`City2DDepthPilot/Editor/DepthPilotNativeCapture.cs → InstallContact` ahora:

1. Inspecciona el alfa real del sprite maestro, sin borrar ni modificar sus píxeles.
2. Cuenta píxeles opacos por fila y encuentra la última fila (desde abajo hacia arriba) con al menos 92 % de la ocupación horizontal máxima.
3. Calcula un centro horizontal ponderado por alfa usando una franja estrecha sobre ese apoyo amplio, no junto a un tocón o un puñado de grano.
4. Dibuja una sombra interior **muy tenue** detrás del sprite, solo si la construcción existe, sin cambiar edificio, collider, terreno, gameplay ni posición lógica.
5. Conserva A original y sus sombras separadas inalteradas.

**Pendiente:** medir en Unity si el nuevo apoyo es realmente más creíble o provoca falsa sombra intermedia. Si se percibe como otro parche no se extiende. Una futura solución definitiva puede requerir máscaras de cimentación dibujadas con geometría real y aprobación visual; el alfa por sí solo no permite reconstruir un tejado, pared, camino o muro posterior.

## Procedencia comprobable

- Aserradero PNG original SHA-256: `d692c25b525a50b5e9f227df6d4b721a5269173a7acffcf7b044dd7a2da6c518`.
- Granero PNG original SHA-256: `699c8c15f5147cc634ea39d1197e13058831786f6da60cab81d711f118118229`.
- Cuartel PNG original SHA-256: `0dcb8edb8237621f13893893edce20fec51deb1248df8bcbc3a89ea3990b4062`.
- Bastión V PNG original SHA-256: `8ffc9f1c04c262893c454f784afb3404c0e390cac811a3c1f1d894201ba48a89`.
- Control A escena Unity SHA-256: `aa566bf6a075868ad852ac865aa8a8cb867d1ade601ac3dd74e8e091f44f0b06`.

No se ha ejecutado Unity de nuevo tras cambiar la técnica; **el resultado visual B sigue sin demostrar**.
