# Organización de Eldoria: dónde está cada cosa

Revisión estructural: 04/11/2026. Esta guía organiza archivos; no concede aprobación
artística, permisos de edición ni autorización de publicación.

La entrada es el [README principal](../../README.md). Las reglas vigentes están en
[AGENTS](../../AGENTS.md), los responsables en el [registro vivo](../../pipeline/active-workstreams.json)
y la documentación temática en el [índice central](../DOCUMENTATION_INDEX.md).

## Mapa completo de las carpetas

| Ubicación | Función y criterio para guardar archivos |
| --- | --- |
| `Unity/` | Juego principal. Código, escenas, recursos importados y ajustes. Conservar `.meta`, GUID y referencias; mover mediante un cambio específico probado en Unity. |
| `ArtSource/` | Fuentes World4X y archivo del Bastión rechazado. Reutilizar la carpeta de la pieza y su procedencia; un original conservado no equivale a aprobación. |
| `art-source/` | Fuentes de producción y ensayos históricos de Valoria. Mantener el nombre existente y sus referencias; no crear una tercera variante de carpeta artística. |
| `references/` | Referencias del propietario, incluida una imagen explícitamente sustituida. El contrato vigente identifica cuál es canónica. |
| `artifacts/` | Evidencia histórica conservada, no carpeta temporal general ni versión distribuida. |
| `assets/` | Recurso del prototipo web heredado. No usarlo como nuevo almacén de recursos Unity. |
| `docs/` | Contratos y guías. Documentación de un área en su subcarpeta; un nuevo Markdown directamente en `docs/` necesita entrada en el catálogo. |
| `docs/evidence/`, `docs/archive/` | Evidencias e historia. Mantener run, SHA, procedencia y originales; los estados antiguos no son órdenes actuales. También hay evidencias dentro de subcarpetas temáticas. |
| `pipeline/` | Configuración, solicitudes y registros consumidos por herramientas. Diferenciar campos actuales de snapshots y evidencias. |
| `tools/` | Herramientas agrupadas por propósito, con instrucciones locales. Reutilizar una herramienta existente antes de añadir otra. |
| `scripts/` | Scripts generales ya referenciados por el proyecto. Evitar moverlos solo para uniformar nombres. |
| `.github/` | Automatización y configuración de GitHub. Consultar el [índice de workflows](../WORKFLOW_INDEX.md) y sus guardas. |
| `qa/`, `tests/` | Comprobaciones existentes del prototipo y contratos. Los tests del juego Unity viven dentro de `Unity/Assets/`. |
| `updates/` | Manifiestos del canal de actualización; contienen estado vivo, no un historial manual duplicado. |
| `v0220/` | Fuente editable del prototipo web y referencia de paridad. No es la aplicación Unity actual. |
| `tester-v0265/` | Instantánea de pruebas congelada; conservar su identidad. |
| `v0192/`, `v0193/`, `v0194/`, `v0195/`, `v0195-clean/`, `v0195-r4/`, `v0195-r5/`, `v0195-r6/`, `v0195-r7/`, `v0195-stable/`, `v0200/`, `v0201/`, `v0202/`, `v0210/` | Versiones históricas del prototipo. Archivo lógico: conservar las rutas para enlaces y comparación, sin reactivar producción allí. |
| Archivos de la raíz | Entradas, contratos generales y lanzadores heredados (`index.html`, `world4x.html`, `package.json`). Las guías de migración y resultados fechados necesitan contexto; no son por su ubicación el estado actual. |

`playtest/` es una salida generada prevista por herramientas, ausente del árbol
auditado: no es una carpeta fuente perdida. Los nombres `ArtTests` o `Generated`
no prueban que un recurso esté fuera del juego; la escena integrada usa `ArtTests`.

## Hallazgos y decisiones

El [inventario reproducible](INVENTORY_20261104.json) corresponde exactamente al
commit `95c00907071b79fc8069e671117f8073aa1ccc86`, anterior a estas mejoras documentales.
No es un contador vivo de `main`.

| Comprobación | Resultado y decisión |
| --- | --- |
| Cobertura de archivos | 21.177 entradas versionadas, todas clasificadas por ubicación; 11.431 pertenecen a Unity. |
| Documentación | 502 Markdown inspeccionados para enlaces inline; 483 rutas locales comprobadas, ninguna inexistente. No se han validado anclas, HTML ni enlaces por referencia. |
| Portabilidad de nombres | Ninguna colisión al ignorar mayúsculas/minúsculas; ninguna ruta relativa supera 240 caracteres. Esto no certifica la longitud absoluta en cada ordenador. |
| Copias idénticas | 1.025 grupos por identidad exacta del blob Git. Se conservan: igualdad de bytes no demuestra que una ruta sobre. |
| Distribución de copias | 697 grupos solo de evidencias históricas; 263 solo dentro de Unity; 24 entre fuentes artísticas y Unity; 17 entre fuentes y evidencias; 15 solo de fuentes/referencias; 5 del prototipo; 3 de evidencias en carpetas documentales temáticas; 1 entre herramienta y Unity. |
| Documentos catalogados | 227: 26 declarados `current`, 25 `reference`, 175 `review_pending` y 1 `historical`. Son etiquetas del catálogo, no una certificación semántica independiente. |
| Instrucciones históricas presentadas como actuales | Corregidas cuatro cabeceras: pausa global del 14/10, exclusividad del 16/10, parcelas visibles de la biblia y rama antigua de dirección artística. Texto y pruebas originales conservados debajo. |
| Navegación | README, índice central y bootstrap conducen a esta guía; se distingue la antigua ruta Unity del Bastión rechazado de su archivo actual. |
| Metadatos | Reparación anterior en PR153; consultar [su informe](../evidence/unity-metadata-20261104/RESULT.md). No se repite ni se atribuye a esta pasada. |

### Revisión semántica que no se debe dar por hecha

Los **175 documentos `review_pending`** están listados individualmente en el JSON.
Este trabajo completa el inventario estructural, no una revisión frase por frase
de todo el diseño histórico. No se cambian esas etiquetas a `current` por pasar un test.

El catálogo todavía etiqueta `current` los cuatro documentos con avisos corregidos.
Su conciliación requiere separar método reutilizable de órdenes de ejecución
sustituidas. `docs/documentation-catalog.json` pertenece al alcance activo del trabajo
de Valoria; esta auditoría no modifica su reserva ni sobrescribe el catálogo.
Hasta conciliarlo, las cabeceras corregidas y AGENTS determinan la precedencia.

Los duplicados quedan inventariados, **no declarados basura**. En particular, los
tres grupos documentales son pruebas repetidas de Bastión III y manifiestos de
marchas: conservar cada ruta mantiene la procedencia de cada ejecución.
No se han declarado recursos Unity «sin uso»: cargas por nombre, `Resources` y
otras dependencias dinámicas impiden deducirlo de una simple búsqueda de texto.

## Mantener el orden al trabajar

1. Consultar el registro vivo, reutilizar el área existente y comprobar las PR.
2. Guardar fuentes editables con su procedencia; guardar su versión importada en
   Unity con su `.meta`. No sustituir el original por una captura o una exportación.
3. Guardar resultados en la carpeta de evidencias del área con SHA y run, sin copiar
   el estado vivo a todos los índices ni convertir una prueba histórica en permiso.
4. Para archivar una guía, añadir aviso de vigencia y sustituto. Moverla solo si se
   conocen sus referencias y se dispone de su alcance; preservar la historia Git.
5. Antes de eliminar una copia, verificar consumidores, referencias, importación,
   procedencia y necesidad de conservar una instantánea. Registrar la decisión.

Inventario de solo lectura, sin Unity ni Windows:

```sh
python3 tools/repository-organization/audit.py --output /tmp/eldoria-organization.json
node tools/check-documentation-governance.mjs --tracked-tree
```

El script informa; no mueve ni elimina archivos, no determina licencias o calidad,
ni inspecciona archivos no versionados o todas las ramas remotas. Para revisar otra
revisión exacta, añadir `--ref SHA`. Los cambios de esta pasada son documentales y
de diagnóstico; no alteran escenas, código jugable, recursos ni publicaciones.
