# Eldoria — Contrato permanente de mejora artística verificable (V1)

**Decisión del propietario: 2026-10-15. Fuente de autoridad: `main/AGENTS.md`.**
**Alcance:** todo chat normal/Work, agente IA, rama, obra 3D, familia de assets o evaluación gráfica futura de Eldoria. No levanta la pausa de Bastión I, mundo 4X, Unity, publicación ni el uso concurrente de Windows. Presupuesto 0 €.

## 1. Objetivo y principio innegociable

No confundir `CI SUCCESS`, ejecución de Qwen, geometría modificada, exportación GLB, diferencias de píxeles, autocrítica favorable del modelo o disponibilidad de Blender con progreso artístico. **Una obra solo mejora cuando un revisor independiente confirma, mediante evidencia A/B real, que supera visualmente a su versión anterior sin degradar elementos protegidos.** No inventar puntuaciones ni certificaciones.

El primer experimento obligatorio para la línea de artista unificado es **la torre de Valoria ya utilizada**, sobre una **fuente original congelada (SHA-256)**. No sustituirla por un diseño diferente ni reiniciar el estudio. Trabajar únicamente sobre copias, conservar cada resultado verificado y usar el último candidato aceptado como entrada para el siguiente ciclo.

## 2. Bucle obligatorio para cada intervención artística

1. Inspeccionar fuente, imágenes y geometría real con coordenadas y dimensiones en Blender WORLD XYZ (Z vertical), junto con `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` y la última evaluación independiente.
2. Seleccionar hasta tres defectos visuales **concretos** y priorizados, explicando qué mejora debería resultar visible desde la cámara de revisión y, cuando corresponda, desde la cámara del jugador.
3. Preparar y **entregar efectivamente a Qwen** la memoria de errores/lecciones verificadas; guardar el hash del contexto realmente utilizado. Qwen decide herramienta, valores y edición original. No sustituir su autoría por geometría preprogramada.
4. Ejecutar edición real en Blender/BlenderMCP sobre copia; archivar plan bruto y hash, SHA de entrada y salida, BLEND/GLB, versiones, nombre de modelo y run id. Rechazar falsos cambios donde solo se ha movido el objeto o destruido atlas, UV o una parte protegida.
5. Producir PNG **before/after auténticos** del mismo objetivo, cámara, resolución, iluminación, render y referencia. Medir visibilidad solo para descartar cambios irrelevantes, **no para aprobar calidad**.
6. Realizar evaluación **independiente de Qwen autor**, revisando geometría, composición, silueta, material, estilo, contactos/funcionalidad, partes protegidas y el resultado perceptible. Exigir diagnóstico, mejora visible y rúbrica 0–10 fundamentados en imágenes genuinas.
7. Si el resultado es peor, idéntico o introduce un defecto grave: `ART_REJECTED`; conservar lección verificable, no promover ni utilizarlo de base. Si la mejora es visualmente creíble en Blender: `BLENDER_VISUAL_ACCEPTED` **provisional**, no `ART_APPROVED`. Reservar Unity/QA móvil para candidatos que justifiquen el coste y estén autorizados.
8. Para `ART_APPROVED`, verificar A/B **real en Unity** (cámara, render, escala, materiales y resolución equiparables), revisión independiente y ausencia de regresiones funcionales. Un arte provisional no se publica.
9. Registrar fuentes, hashes y criterio antes/después y alimentar el siguiente ciclo. Conservar siempre la última versión realmente aprobada.

## 3. Rúbrica permanente y criterio de aceptación

Puntuación 0–10 por apartado con la referencia visual de Eldoria como objetivo:

| Apartado | Peso |
| --- | ---: |
| Geometría y silueta | 25 % |
| Coherencia arquitectónica, proporciones y contacto | 25 % |
| Materiales y detalle | 20 % |
| Estilo y coherencia con Valoria | 20 % |
| Legibilidad desde cámara de juego | 10 % |

Una intervención marcada como mejora aceptada exige **aumento ponderado >= 0,25 puntos**, ninguna caída de más de 0,5 en un apartado, ningún defecto grave nuevo ni invasión de zonas protegidas. El umbral es un filtro de consistencia, NO una prueba matemática de belleza: la revisión independiente de imágenes sigue siendo indispensable.

Para marcar **`CALIBRATION_VALIDATED`**: mínimo **tres intervenciones consecutivas aceptadas sobre el mismo asset**, enlazadas por SHA (salida del ciclo N = entrada del N+1), fuente original fija, mejora visible acumulativa, última nota ponderada >= 8/10, evidencia A/B de Unity del candidato final, juicio independiente explícito y procedencia verificable. Intentos rechazados quedan en registro, pero **no cuentan** entre los tres. Si el resultado no alcanza 8/10 se mantiene `CALIBRATION_IN_PROGRESS` aunque haya tres progresos reales.

La calibración de una sola torre **NO** declara producción masiva. Antes de `ART_PRODUCTION_SCALABLE`: demostrar transferencia a **dos assets adicionales distintos**, ambos >=8/10 y medir menos tiempo/intervenciones contra una línea base comparable. No inventar ganancias de velocidad ni aprendizaje de pesos (la memoria actual es recuperada del historial).

## 4. Criterios para cambiar estrategia, no solo parámetros

- Dos rechazos visuales consecutivos de la misma hipótesis/operación => cambiar de hipótesis geométrica o artística. No presentar nuevas tintas o variaciones mínimas como progreso.
- Cinco intentos completos sin mejora perceptible => suspender **esa estrategia** para diagnóstico de fuente/contexto espacial/modelo/herramientas; no abandonar automáticamente la misión ni seguir repitiendo el mismo intento.
- Comparar 4B/8B por mejora de imagen real y coste local, no por longitud de JSON o rapidez aislada; usar el modelo menor si logra calidad equiparable y otro mayor si mejora efectivamente el criterio.
- Un revisor no debe autoaprobar el trabajo de Qwen; una evaluación Qwen positiva no sustituye inspección independiente.

## 5. Evidencia y barrera automatizable

Las declaraciones formales `ART_APPROVED` y `CALIBRATION_VALIDATED` se registran exclusivamente en `pipeline/art-visual-approvals/*.json`, con contrato `tools/art-visual-acceptance/verify.mjs`. Ese verificador exige registros encadenados, hashes de planes, cámara fija, PNG existentes con SHA real, puntuaciones fundamentadas y revisión independiente; el último ciclo aprobado aporta también A/B Unity. Un `TECH_PASS` sin manifiesto no crea una aprobación. Un manifiesto con evidencia ausente o inconsistente falla en modo cerrado. Los manifiestos de evaluación pueden prepararse **fuera** de esa ruta hasta ser verificables; nunca subir simulaciones como aprobado.

`docs/evidence/**` y GitHub Actions contienen evidencias históricas, pero **la existencia de un artefacto o una URL por sí sola no verifica sus bytes**. Para pasar la comprobación de un manifiesto aprobado, materializar sus PNG originales en rutas relativas verificables en el checkout de validación. La rúbrica es una declaración revisada por un agente/persona independiente: el script prueba coherencia de evidencias, **no puede juzgar estética subjetiva ni certificar por sí mismo el juego**.

## 6. Aplicación a chats ya activos y futuros

Antes de la **siguiente** decisión/inferencia o proclamación artística, los agentes deben volver a leer `main/AGENTS.md` y este documento, aunque su rama sea antigua. El propietario unificado de `art/eldoria-full-blender-artist-studio-20261014` debe adaptar su ciclo existente a esta barrera en su **propia rama**, sin reiniciar, duplicar trabajos, pisar otras ramas ni usar Windows sin exclusividad. El guard de GitHub en `main` opera sobre los eventos/rutas de esa rama canónica; las ramas activas deben actualizar/consumir el contrato para aplicarlo localmente. Esta modificación de archivos **no puede inyectar mensajes a un chat que ya está ejecutándose**: el chat necesita releer `main` antes del siguiente ciclo o recibir el enlace del propietario.

**Estados distintos:** `TECH_PASS`, `ART_REJECTED`, `BLENDER_VISUAL_ACCEPTED`, `ART_REVIEW_REQUIRED`, `ART_APPROVED`, `CALIBRATION_VALIDATED`, `ART_PRODUCTION_SCALABLE`. Un gate técnico verde nunca asciende un estado artístico automáticamente.
