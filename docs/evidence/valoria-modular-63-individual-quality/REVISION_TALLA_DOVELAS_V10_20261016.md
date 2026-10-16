# Revisión visual independiente, cantería V10, 16-10-2026

## Resultado observado

**Ejecución V10**: [38907273374](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38907273374), artefacto **12047829601**. Cuatro GLB separados, ocho capturas auténticas Blender y manifiesto de sculpt; PASS técnico, encaje modular verificado. Fuente V9 [38906155308](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38906155308), artefacto **12048097620**.

He inspeccionado directamente las ocho capturas V10, comparadas a la misma cámara, escala e iluminación:

- `arch_keystone_major`: pequeñas hendiduras/pátina más visible en cara frontal, pero no salto de calidad sustancial.
- `arch_voussoir_major`: moteado/discontinuidades en cara frontal, silueta casi idéntica; textura aún de vetas.
- `arch_keystone_minor` y `arch_voussoir_minor`: talla adicional visualmente casi imperceptible a tamaño de asset.
- Las cuatro siguen siendo cuñas modulares útiles, pero sus caras grandes parecen muy planas respecto a la piedra medieval de Valoria.

**Decisión:** V10 queda **ART_REVIEW_REJECTED_AS_FLOOR_UPGRADE** por mejora visual no convincente (`delta < 0,25`). No marcar `ART_APPROVED`. Mantener **V9** (aprox. 6,0 provisional cada una) como mejores candidatos de la biblioteca, con sus GLB y fotos conservadas. Aunque el script V10 es técnicamente válido, no hay justificación artística para promocionarlo.

## Otras dos piezas pendientes

Mortero V10 [38906245333](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38906245333) rechazado anteriormente por junta demasiado oscura. No promover.
Agua y mortero continuados mediante [38907369609](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38907369609). La tarea de estudio en Windows quedó *queued* mientras Bastión I ocupa el runner; respetar la cola y no declarar dos PASS hasta inspeccionar los PNG.

El tablero continúa en **61/63 aproximado**, sujetos a revisión humana de las cinco piezas recién cruzando 6, y **0/63 ART_APPROVED**. No ensamblar la escena, no modificar Main ni escenas Unity.

Referencia A/B creada en la sesión y verificable localmente: `/mnt/data/eldoria_four_sculpt_arches_v10_AB.jpg`, `/mnt/data/ELDORIA_CUATRO_DOVELAS_ESCULPIDAS_V10.zip`.
