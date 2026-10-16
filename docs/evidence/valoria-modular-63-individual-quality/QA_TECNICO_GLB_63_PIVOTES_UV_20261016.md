# Valoria — QA técnico independiente de los 63 modelos GLB seleccionados (16/10/2026)

**Ámbito:** exclusivamente los 63 módulos individuales de la biblioteca artística aislada. No se importó a Unity, no se editó Bastión I, no se publicó arte ni se certificó ART_APPROVED.

## Procedimiento con datos auténticos

La prueba programática abrió los **63 archivos GLB consolidados**, comparó sus bytes SHA256 con el manifiesto de ganadores individuales y leyó escenas reales (geometría, posición, bounds, primitivas, UV y extensiones glTF). Para inspección geométrica en el contenedor de análisis se usó `trimesh` sobre los GLB originales, sin sintetizar renders o modelos nuevos.

- **63/63 GLB analizados; 0 fallos de lectura o geometría con dimensiones degeneradas**.
- **9 módulos** contienen primitivas sin `TEXCOORD_0`: `dirt_ground_patch`, `grass_ground_patch`, `grass_tuft_1`, `grass_tuft_2`, `mortar_joint_recess`, `portcullis_large`, `portcullis_small`, `timber_beam_1`, `timber_beam_2`. Esto **NO demuestra por sí mismo un fallo** si aquellas primitivas utilizan materiales sin textura.
- **4 dovelas/claves de arco** presentan geometría alejada del origen: `arch_keystone_major`, `arch_keystone_minor`, `arch_voussoir_major`, `arch_voussoir_minor`. Puede ser el anclaje intencional de una geometría radial alrededor del centro del arco; **no recentrar sin comprobar coordinación modular**.
- **1 asset**, `river_water_tile`, declara `KHR_materials_transmission` e `KHR_materials_ior`, los cuales requieren comprobación de compatibilidad material y transparencia en **Unity URP** (no certificada).
- Los modelos con mayor número de triángulos del conjunto inspeccionado son `cliff_stone_chip` (66.122), `boulder_2` (66.122), `tower_square_tall` (38.124), `cliff_rock_column` (29.566), `tower_square_short` (25.164) y `dirt_ground_patch` (22.866). Estos valores no son un FPS medido ni un presupuesto final de Android/iOS.

## Reglas de interpretación

Ninguna alerta técnica equivale automáticamente a una nueva puntuación estética baja. Ningún archivo pasó Unity aquí. Las orientaciones reportadas corresponden al sistema espacial del GLB; al validar pivotes en Blender/Unity hay que respetar las convenciones de ejes y orígenes. Tampoco demuestra licencias CC0 individuales: esos comprobantes siguen pendientes en el inventario.

**Estado artístico consolidado al realizar QA:** 31/63 candidatos de estudio >=6/10 y 32 por debajo, con 0 ART_APPROVED. `arrow_slit_frame` V10 demuestra espesor de 20mm y un hueco libre en Blender, pero su incremento visual no se acepta automáticamente. `cobble_pebbles` V11 depende de una prueba real de Blender; **no asignar nota ni decir completado basándose en workflow lanzado**.

**Pruebas de referencia:** artifact de `merlon` #38938952286, `wall_course` #38940413584 y `arrow_slit` #38943106847. Inventario con SHA256 por asset: `RECUPERACION_GPT6_20261016_63_GLBS_CONTINUIDAD.md`.

**Pendiente:** decidir mejoras 6/10 individualmente con A/B, optimizar cuando el mesh sea desproporcionado y validar URP cuando se autorice el trabajo individual de integración; sin ensamblar escena.
