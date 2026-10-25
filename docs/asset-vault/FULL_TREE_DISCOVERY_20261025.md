# Eldoria — primer barrido REAL del árbol Git de modelos 3D

**Ejecución verificada:** [40216433076](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40216433076) · **PR #106** · 25-10-2026. 
**Método:** `git ls-tree --name-only -r -z HEAD` sobre checkout de PR; no descarga los cuerpos de modelos y no toca Unity.

## Resultado

- **775 archivos 3D rastreados en el árbol Git de esta revisión** (sin contar .meta, texturas PNG ni imágenes).
- **39 rutas ya conocidas por el catálogo curado**; **736 rutas adicionales descubiertas automáticamente**, todavía sin clasificación artística individual.
- Tipos entre las **736 candidatas**: **540 FBX**, **115 GLB**, **79 Blender (.blend)** y **2 OBJ**.
- De las 736: **597** en `Unity/`, **77** en `docs/`, **60** en `art-source/`, **2** en `pipeline/`.
- Ejemplos de familias: **213** archivos bajo `Unity/Assets/EmaceArt/Slavic World Free/Meshes`, **14** bajo `Unity/Assets/PolyOne/Free Modular Terrain/Model`, **12** bajo `Unity/Assets/Tree_Packs/URP_Tree_Pack/Models` y **8** bajo `Unity/Assets/Bublik/Simple Modular Castle Assets/Meshes`.

**ADVERTENCIA:** 736 no significa 736 modelos nuevos, buenos, únicos ni creados por Eldoria. El barrido incluye librerías de terceros, versiones, experimentos, fuentes y duplicados. La decisión sobre licencias, utilidad, calidad, redundancia y rol se hará por familias, antes de cualquier promoción. No reintroducir Slavic foliage como bosque principal por existir 213 ficheros en ese pack.

## Consecuencia operativa

La biblioteca previa de 26 GLB era **solo la colección canónica de producción**, no el inventario de todos los modelos existentes. El índice de 47 registros sigue siendo el conjunto **curado**, mientras que el escáner detecta los demás como **pendientes**. Esto explica por qué el equipo podía volver a crear recursos que ya existían: no se buscaba sistemáticamente en el árbol Git completo.

El nuevo workflow registra las candidatas como un artefacto de ejecución y, **cuando el PR #106 sea integrado**, mantendrá una incidencia persistente en main. No modifica Work PR #100 ni activa assets automáticamente.

## Qué falta para una cobertura verdaderamente completa

1. Desduplicar por hash real y procedencia de las 736 rutas, separando recursos propios, packs externos, experimentos y modelos aprobados.
2. Revisar licencias y derechos de redistribución/uso de cada familia externa.
3. Inspeccionar geometría, UV, materiales, animaciones, LOD y calidad en cámara de los candidatos prioritarios; generar miniaturas.
4. Ejecutar el barrido completo de los 201 commits archivados y contrastar contra estas 775 rutas actuales.
5. Recuperar selectivamente fuentes históricas valiosas que no estén en Git actual y que todavía sean accesibles, sin copiar miles de variantes inútiles.
6. Revisar depósitos externos/PC: ningún proceso Git puede certificar archivos nunca versionados ni recuperar artefactos caducados.

**Veredicto:** el descubrimiento automático ya funciona y está probado en CI; la curación visual exhaustiva y el rescate histórico completo **NO** están terminados.
