# Forja Oscura — calidad del material y nueve cortes en móvil
**Estado:** corrección técnica versionada en PR #173; **NO aprobación visual, NO captura nativa**.

## Hallazgo real
Las superficies iniciales tenían el mismo borde vertical y horizontal. En controles pequeños el borde superior más el inferior llegaban a ocupar toda la altura. Aun con un sprite importable, los materiales y las esquinas podían aplastarse en la ejecución (esto no es un problema de recursos del jugador ni de la ciudad).

Se corrigieron **21 sprites importados** manteniendo sus GUID, sin modificar el PNG, y el importador `EldoriaVisualKitTextureImporter` ahora declara bordes distintos en X e Y. El script `tools/forja-oscura/check-mobile-nine-slice.py` compara los valores originales `.meta` con el importador, exige `spriteMeshType: 0` (FullRect en Unity 6000.3) y deja un centro escalable de al menos el 20% de la altura mínima objetivo.

| Tipo | Bordes L/B/R/T (px fuente) | Altura mínima lógica usada en el control |
|---|---:|---:|
| Botones principales y secundarios | 56/15/56/15 | 44 |
| Navegación inferior | 48/22/48/22 | 65 |
| Paneles oscuros y claros | 56/20/56/20 | 72 |
| Objetivo de capítulo | 56/22/56/22 | 80 |
| Contador de recurso | 36/14/36/14 | 48 |
| Pestañas | 36/10/36/10 | 36 |
| Etiqueta de nivel | 32/8/32/8 | 28 |
| Barra de progreso | 20/3/20/3 | 12 |
| Mensaje breve | 40/12/40/12 | 44 |

**Límite:** este control evita colapsos matemáticos, pero no garantiza que el mismo sprite se vea perfecto a distintas resoluciones. Los adornos ubicados en el centro de un marco deben crearse como hijos independientes y no hornearse dentro de las zonas escalables.

## Segundo hallazgo artístico: fondos ilustrados
La familia de sprites ilustrados con medallones y filigrana tiene más riqueza que las placas procedurales planas, pero en pruebas de Pillow algunas piezas pierden los adornos centrales cuando se redimensionan con nine-slice. Se ha desarrollado una alternativa local con base estirable y ornamentos separados, **todavía no integrada a esta PR**. No convertir un montaje Python en una captura nativa ni afirmar que el arte premium ha sido aprobado.

### Condición de siguiente paso
1. Comprobar que `check-mobile-nine-slice.py` y contratos C# pasan en GitHub.
2. Ejecutar Unity nativo después de un relevo formal del ejecutor Windows `windows-runner-heavy` (sin interferir con PR #163).
3. Capturar **mismo juego, mismo frame lógico, cuatro resoluciones** con ocho PNG reales y comprobar con ojos y tacto que el cambio es sustancialmente mejor.
4. Si el HUD es demasiado clásico, plano, ilegible o deforma adornos, **rechazar la sustitución y trabajar las piezas que fallan**, no reescribir mecánicas.

El estado global se mantiene `ART_APPROVED=false`, `UNITY_NATIVE_PASS=false`, `MERGED=false`.

## Corrección de ornamentos fuera del área fija — 07/11/2026

**Problema verificado en el arte fuente:** el generador original dibujaba remaches y diagonales en coordenadas de esquina de hasta 52 píxeles desde el borde vertical, pero los importadores definen bordes superiores/inferiores de sólo 14–22 píxeles en los controles móviles. Según el comportamiento de 9-slice de Unity, esa decoración invadía franjas escalables; sus remaches se aplastaban y el chaflán de algunos paneles alcanzaba la zona estirable. No se corrige aumentando el número de píxeles de una captura ni ocultándolo con fondos.

**Solución versionada sin alterar la ciudad ni las mecánicas:** `tools/forja-oscura/forja-detail-art.js` limita el chaflán y las marcas grabadas a las cuatro áreas fijas del 9-slice de cada tipo. Las texturas mantienen grano, luz y bisel; se regeneraron en la misma rama los **nueve PNG de producción** (panel, misión, recurso, dos navegaciones y cuatro estados de acción), conservando todos los `.meta` y GUID de Unity. El ZIP histórico de entrada conserva su hash original, pero **no debe restaurarse encima de estos nueve PNG actualizados**.

**Prueba A/B del material de origen (no Unity):** `tools/forja-oscura/check-premium-corner-safety.js` produce cada sprite con y sin decoración, decodifica las filas PNG originales y compara todos los píxeles. Rechaza cualquier diferencia decorativa fuera de los cuatro rectángulos de esquina que la configuración 9-slice deja sin estirar; exige remaches presentes en cada esquina y chaflán contenido en el borde fijo. `premium-art-preview` y los contratos fuente pasaron en [CI de arte #42373789092](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42373789092). La publicación de los nueve activos en la rama pasó en [run #42373789089](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42373789089).

**Límites:** las texturas guardadas no son capturas del HUD, y el test geométrico no acredita color, textura, legibilidad, botones, composición ni fidelidad subjetiva a la imagen elegida. Continúan siendo obligatorios los ocho PNG nativos en una sesión Unity, la inspección artística y las interacciones reales en móvil y WebGL. `UNITY_NATIVE_PASS=false`; `ART_APPROVED=false`; `MERGED=false`.
