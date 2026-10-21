# Eldoria — comparación real Qwen con/sin memoria (Engendro)

**Fecha del ensayo:** 21-10-2026 · **Evidencia:** [run 39687896808](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39687896808) (SUCCESS) · [artefacto original 12385844815](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39687896808/artifacts/12385844815).

**Veredicto:** DOS INFERENCIAS QWEN REALES Y COMPARABLES COMPLETADAS. **No se ha demostrado una ganancia artística de la memoria en este ensayo.** No hay nuevas geometrías, texturas, cambios Unity, publicación ni aprobación artística.

## Qué se mantuvo igual

- Modelo: `qwen3-vl:2b-instruct`, local, servido por Ollama gratuito en Linux, sin créditos Tripo.
- Engendro: fotografía original de la fuente `ArtSource/World4X/Spawnling_v1/Finished/finished_face.png`; SHA-256 `3fc1d0dc860cfa9c0ff8d4288ea0e590735a349a0d21c3123a097e7b771432c4`.
- Captura Unity ya existente `docs/evidence/spawnling-bastion2/after-local-900x1600.png`; SHA-256 `16e53ce27726787f73e9424851c8cb700467c00ae43b518734c0d3b41e8685fb`.
- Misma reducción auténtica a JPG de 400px, mismo prompt base, misma temperatura, formato JSON restringido. La variable experimental fue agregar antecedentes (`K13, K14, K19, K20, K38`) al segundo prompt.
- Tercera referencia: procedimiento seguro sin Qwen, que no genera cambios sobre el activo hasta disponer de datos de la fuente y una revisión independiente.

## Resultados medidos

| Alternativa | Inferencia | Segundos de ejecución | Tokens generados | Propuesta | Geometría mejorada demostrada |
|---|---|---:|---:|---|---|
| Sin Qwen | No llama al modelo | 0 | 0 | No editar sin prueba; revisar la fuente | No |
| Qwen sin memoria | Real, JSON válido | 118,909 | 110 | `assign_material` | No |
| Qwen con memoria | Real, JSON válido | 14,568 | 77 | `assign_material` | No |

**Advertencia esencial:** el segundo ensayo aprovechó el modelo ya cargado y su caché. Los tiempos **no** miden una supuesta aceleración debida a la memoria. Es un único ensayo, con orden fijo y sin repetición estadística.

Los dos Qwen hicieron propuestas genéricas centradas en materiales. Ninguno demostró un cambio de anatomía y ninguno advirtió explícitamente del desplazamiento global, aunque la memoria incluía esa lección. El Qwen con memoria no fue claramente más preciso: el chequeo de mención al +X de la cabeza fue positivo en el control sin memoria y negativo en la segunda respuesta. Ambos evitaron atribuirse una aprobación visual.

Las respuestas originales, las dos imágenes de entrada reducidas y sus hashes están en el artefacto citado. El resumen estructurado permanente permanece en [`pipeline/art-knowledge/benchmarks/engendro-qwen-memory-ab-20261021.json`](../../../pipeline/art-knowledge/benchmarks/engendro-qwen-memory-ab-20261021.json), incluyendo los SHA-256 exactos de los textos inferidos para preservar su identidad cuando caduque el ZIP de Actions.

## Qué se concluye (y qué no)

**Sí:** el puente de memoria puede reunir conocimiento rastreable, el modelo puede recibir condiciones diferentes y producir resultados técnicos sin tocar el juego. **No:** que la memoria haga por sí sola a Qwen mejor artista, más rápido o más rentable. **No se ha ejecutado Blender ni un A/B Unity** para ninguna propuesta: por tanto, no es un benchmark de calidad visual final.

El resultado refuerza la política de uso selectivo: Blender/Unity y técnicas conocidas ejecutan arte, Qwen solo añade orientación cuando realmente aporta información relevante. Si la primera inferencia no produce una operación plausible, registrar la lección y cambiar de estrategia en lugar de reiterar prompts. Nunca promocionar esta prueba como `ART_APPROVED` ni como `MEMORY_VISUAL_GAIN`.
