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


## Preparación real completada

Run 38786948113, commit 74ca48138f79934962a23a08a0344b1fce8ce236,
Windows DESKTOP-R10PE55, SUCCESS. Artifact 11997163937 (ZIP SHA256
37bd4fb16bb688b42e5e7db225504b933c8b048b59c46393ad0203a853183f9e).
Restauración real: normalized-source.glb SHA256
8a77d244b0434faeec1d4430b6194ffddd17234bfda20bedbe83e382c0a01632,
556112 bytes, un PNG incrustado y TEXCOORD_0 en todas las primitivas.
Proof Blender: geometría y UV idénticos; original intacto.

El A/B real muestra recuperación marrón del tejado, troncos, puerta y escalera.
Esto elimina un sesgo de evaluación, no acredita nueva autoría artística.
Contexto común exacto SHA256
60a7081bf870e74326035a1e716b23447fb42c74108b71464c983098e029cdc0.
Dos lecciones recuperadas realmente incluidas: AABB no demuestra contacto;
referencia de estilo/luz no es una medida geométrica. No entrenamiento.

Limitación observada: VL8B describe la torre pero afirma sin justificación
que no hay defectos, incertidumbres ni caras ausentes. Una imagen no certifica
inventario topológico ni ausencia de errores; no adoptar esas frases como
verdad geométrica. Mantener crítica independiente de autor y de observador.

Comparación en curso: run 38787448197, commit
89d4823c005199bb3c80ea5bda5208d32c63a7c4. Descarga exactamente el artefacto
anterior en vez de regenerar observaciones entre directores. GPT externo
reconoce el contexto exacto por SHA; Qwen recibe ese mismo contexto textual,
memoria, geometría y esquema. El director externo además inspecciona los PNG
originales: la mediación visual VL8B para el director 14B se declara como límite,
no como igualdad de percepción. Aún no existe resultado artístico aceptado.


## Primera comparación recibida y cambio de hipótesis obligatorio

Run 38787448197 SUCCESS técnico; artifact 11997969936, ZIP SHA256
7a23941fd26ab41c7b69c71a7fc61daacb051803c05de729b8fe42560db4b14f.
GPT externo: tres miembros, dos uniones reales, UV y textura incrustada,
contacto BVH de todos los miembros; Blender 15,938 s; GLB SHA256
1b2185c09ecc63a5bf72b9d69978586e58af4901a52faab9153e05ca61c0f059.
No aprobación: estructura sencilla, la estética global apenas cambia; bases
por encima del plano de revisión. El crítico repite el encargo y llama
material recovery a una adición geométrica. Esa crítica NO es una aceptación.

Qwen14B: dos planes idénticos, TENON_SOCKET_SIZE_MISMATCH. Además de las
holguras imposibles, coloca postes verticales dentro del volumen de la torre y
llama uniones a conexiones entre postes separados. No ejecutó Blender para
ese brazo. La comparación produjo un resultado asimétrico y no demuestra
superioridad causal general de ningún modelo. Tiempo total del ensayo
239,735 s; tiempo manual GPT no medido, no inferir ahorro.

Tras los dos rechazos se cambia hipótesis: **no pedir microaritmética de
fabricación al director artístico**. `timber_frame_skill.py` ofrece una habilidad
semántica: el autor escoge una cara fuente real, dimensiones, altura de base,
separación y número de postes (2–5), sección, offset y acabado. La herramienta
calcula las mortajas con holgura fija documentada de 6 mm. No desplaza ni
repara silenciosamente el diseño: composiciones incompatibles se rechazan.
No genera una torre ni una pieza fija; sigue disponible el estudio completo.
`frame_skill_blender.py` resuelve ancla y normal REALES de Blender y llama al
mismo taller probado. Ambos directores reciben el mismo esquema nuevo.

El crítico de esta prueba posterior se vuelve **ciego a la intención**: observa
BEFORE/AFTER/referencia sin recibir el discurso del autor. La nueva prueba
reutiliza la misma base normalizada, NO encadena el marco sin aprobar.
Cuatro pruebas focalizadas verifican variación de composición sin reescritura,
ensamblajes reales compatibles, rechazo de piezas fuera de dintel y veto a
convertir una cara de tejado en pared. Falta todavía su ejecución real en PC.

Ruta local sin GPT preparada: `hybrid_trial.py local --workshop frame --pack ...`
usa visión, memoria selectiva y Qwen locales; `compare` añade el brazo externo.
Ninguna aprobación o encadenamiento sucede automáticamente. Esta ruta local
queda implementada, no se presenta como autonomía artística ya validada.
