# Artista híbrido — consolidación en la línea diagnóstica existente

Autorización: solicitud del propietario 15-10-2026, consolidar y ensayar copias
aisladas, coste 0 €. No levanta pausas Bastión I/4X/Unity/publicación. No modifica
reservas ajenas. Main consultado: c8b75a9534fb18a7568d6cde520722973f5c4369.

## Ruta ejecutable

Rama propietaria diagnóstica: `diagnostic/valoria-ai-eyes-brain-20261015`,
continuando d5120f7b9866d5982672a5a3502bc82ae29f3059. El registro histórico aún
nombra art/eldoria-full-blender-artist-studio-20261014; la conciliación vigente
de main identifica esta línea diagnóstica como propietaria de las capacidades.
No se promueve el estudio a main ni se trae su historial experimental completo.

`tools/art-direction/full-studio/hybrid_trial.py` es un adaptador del estudio
existente. `prepare` produce una base normalizada, inspección Blender real,
observaciones VL separadas fuente/referencia, memoria selectiva y un contexto
común con esquema. `compare` consume ese contexto y un plan JSON del director
externo, solicita otro plan original a Qwen14B y ejecuta ambos con el MISMO
`gpt6-tower-eval-20261015/joinery_blender.py`. Ambos tienen 2–12 miembros y
1–16 uniones paramétricas, bisel, UV, textura incrustada y clavijas. No hay una
torre plantilla ni Python arbitrario del modelo. El ensayo acota carpintería
axial: no es una comparación de todas las capacidades creativas de los modelos.

El estudio completo `artist_loop.py` conserva modelado de malla heredada,
materiales, UV, operadores nativos limitados, nodos, importación y reutilización,
y transporte directo/MCP. Se concilian los diez archivos del grounded-beam HEAD
e429daed7d229e333059ead58ad6c7de2364262a, incluyendo ensamblajes de varias vigas,
verdad espacial WORLD y separación VL8B/director14B. No se reemplaza ese estudio.
La carpintería reutiliza la implementación Blender probada de la prueba C.

El workflow existente `eldoria-qwen14b-vl8b-grounded-wood-art-trial.yml` consume
`pipeline/artist-hybrid-trial-request.json`. Solo ese request dispara este
ensayo aislado en la rama propietaria; editar el workflow no dispara Windows.
Concurrencia compartida y precheck de otros jobs Windows, sin cancelar trabajos.
No requiere GPT instalado ni API de pago: el director externo aporta JSON;
Qwen conserva selección de miembros, proporciones, posiciones, juntas y material.

## Recuperación real de las tres pruebas

| Prueba | HEAD recuperado | Run / artefacto original |
|---|---|---|
| Cuartel | 5d55e605ec2d476a67fd0a6b311d3176473eb3d4 | 38779749030 / 11995171792 |
| Qwen14B+VL8B | e429daed7d229e333059ead58ad6c7de2364262a | 38782068708 / 11996617963 |
| Carpintería GPT | 451509815660bce4cb1970946d65e1471ebe0209 | 38776804558 / 11994566724 |
| Crítica V3 | mismo HEAD | 38780125999 / 11994854494 |
| Gate adversarial | mismo HEAD | 38780547435 / sin artefacto |

Se descargaron y abrieron los cuatro ZIP originales. El cuartel conserva BLEND,
GLB y tres PNG; no se recrea. Qwen ejecutó Blender pero el último candidato solo
cambió 1519/691200 píxeles (0,21976 %): rechazo de visibilidad. GPT produjo tres
miembros y dos uniones reales; crítica V3 REVIEW_HOLD, salto declarado 3 puntos
sin fundamentación suficiente. No se acepta esa puntuación.

## Defecto de transporte de materiales descubierto

El GLB original congelado y el resultado Qwen carecen de images/textures glTF.
No hay un shader gris deliberado en inspect_source: ese mensaje del observador
era injustificado y se corrige. El material se llama
EA02_LPNP-Slavika_Colorsheet_v1.png.038. Unity material
EA03_LPNP-Slavika_Colorsheet_v1.mat enlaza _BaseMap/_MainTex al GUID
4f4a523b69ecaf542bcff16bc65d3002; la meta de
`Unity/Assets/EmaceArt/Slavic World Free/Texture/EA03_FREE_Slavica.png`
contiene ese GUID. PNG SHA256
139f70a56e475755347b4fcaf4c8640f209b7b49133047d00c3b1f11de277f5c.

`hybrid_source.py` repara ese enlace solo en una copia, comprobando geometría
y UV idénticos y archivando A/B originales. Ambos directores usan esa misma
copia; el atlas recuperado NO cuenta como intervención creativa ni como ventaja
de un director. Referencia SHA256
8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689.
Torre original SHA256
e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869.

## Límites y continuidad

Estado inicial: preparación técnica implementada; ejecución nueva aún pendiente.
12 tests grounded-beam ejecutados localmente PASS. El crítico VL es una opinión
independiente del autor pero falible; su respuesta nunca eleva aprobación.
La revisión externa debe cotejar A/B, contactos, fuente, materiales y estilo.
No encadenar candidatos con contacto fallido o revisión pendiente. No transferir
a cuartel/tercer asset antes de una mejora real aceptada de la torre.
Memoria: usa `director-control/selective_memory.py`, hasta dos lecciones;
contexto exacto archivado. Es recuperación de experiencias, no entrenamiento.
ART_APPROVED=false; CALIBRATION_VALIDATED=false; Unity sigue pausado.
