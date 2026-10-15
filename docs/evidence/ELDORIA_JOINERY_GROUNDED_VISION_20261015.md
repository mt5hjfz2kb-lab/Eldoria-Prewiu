# Eldoria: taller de uniones reales y crítico visual fundamentado (15-10-2026)

**Ámbito:** rama aislada `diagnostic/gpt6-vs-qwen-valoria-tower-20261015`, sin merge a `main`, sin Unity y sin reanudar la producción de Bastión/4X. Presupuesto 0€. Torre original congelada SHA256 `e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`. Verificar en cada nuevo chat `main/AGENTS.md` y los estados activos.

## 1. Contrato de taller con autoría del modelo

- `joinery_contract.py`: verifica cada miembro y mortaja/espiga (dimensiones artísticas explícitas, orden de extremos, holgura, protección de tejado, finite floats). No decide geometría arbitrariamente ni aprueba arte.
- `joinery_blender.py`: operaciones Blender **reales** de `Boolean UNION/DIFFERENCE`, bisel, clavija, textura propia incrustada e UV en cada pieza nueva. Reutiliza cámara y motor de `full-studio/blender_toolbox.py` y BVH de `blender_surface_contact.py`. Nunca toca la malla heredada ni su UV.
- `gpt6-joinery-plan-r3.json`: hipótesis de prueba, dos postes y dintel con **dos uniones de mortaja/espiga**; es una propuesta GPT-6 acotada, NO trabajo de modelado autónomo general validado. El modelo elige los parámetros en metros WORLD; no existen macros de castillos prefabricados.

### Evidencia real de Blender
Run [38776804558](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38776804558), artefacto [11994566724](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38776804558/artifacts/11994566724):
- Blender 5.2, dos uniones BOOLEAN reales. Cada mortaja aumenta caras de su malla receptora (6→12 y 40→46), holgura total 0.008 m en cada eje.
- Tres mallas nuevas con UV, textura de madera incrustada, GLB/B LEND originales, fuente original intacta y cámara idéntica.
- SHA256 del GLB: `b8866c6c64f3f017ded1a85990d3dd85ab172ffe06468d5aedfd347331a65219`.
- Prueba técnica exitosa **no es una mejora artística certificada**. La composición es principalmente un marco frontal oscuro sencillo sobre una torre original blanca; no basta para profesionalidad ni para el objetivo 8/10.

## 2. Crítico de dos fases y diagnóstico del error

1. `grounded_review.py` coteja las piezas reales exportadas y sus métricas BVH; hay veto a referencias a diagonales/fascia inexistentes y a inventarios falsos. Afirmaciones sobre contacto geométrico no pueden ser sustituidas por sensaciones visuales.
2. `review_live.py` ejecutó dos fases y bloqueó la valoración cuando VL8B afirmó que **ninguno de los tres objetos añadidos** era visible, además de inventar `NOT_NEAR_SOURCE` pese a contacto medido: prueba [38776804558](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38776804558), `grounded-critic-verdict.json`. El rechazo del crítico no se transfirió injustamente a la obra.
3. `review_live_v2.py` recibe **BEFORE + AFTER + acercamiento de AFTER** en fase 1, separa del todo la referencia de estilo, y mide evidencia de visibilidad real por ROIs limitados a cada objeto y vinculados con SHA originales.
4. En los archivos exactos, la fracción de píxeles cambiados (>12 por canal) en las ventanas de cada miembro es 45,5033% dintel, 68,071% poste izquierdo y 49,6981% poste derecho. Esto **no mide arte**.
5. En la ejecución [38778491176](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38778491176), VL8B identificó expresamente en el nuevo encuadre los tres miembros como **visibles** y describió su tono marrón/estructura. Sin embargo, devolvió claves/valores JSON sin comillas; el primer parser lo retuvo como `INVALID_CRITIC_JSON`. Se ha añadido un parser con reparación **exclusivamente de claves y enumeraciones de una lista blanca**, nunca altera hechos ni inventa identificación o evaluación. La respuesta en bruto permanece archivada.
6. La continuación de fase 2 usa los MISMOS bytes originales, no vuelve a generar un asset; ver workflow `.github/workflows/eldoria-gpt6-critic-v2-phase2.yml`. Esperar el resultado real del run, no prometer una nota.

### Pruebas y gobierno
- Run [38776966017](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38776966017): **17/17** pruebas Python de contratos y defensas adversariales. No sustituye Blender/Qwen.
- Cualquier fase 1 no fundamentada → `REVIEW_HOLD`, **nunca** `ART_REJECTED` ni `ART_APPROVED`.
- Una fase 2 coherente solo concede `PROVISIONAL_REVIEW_GROUNDED`. La aprobación artística sigue prohibida sin criterio independiente, Unity y contrato de `main/docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md`.
- Uniones con contacto geométrico cercano no demuestran estabilidad mecánica, estética profesional ni transferibilidad a dos assets adicionales.
- No fusionar este diagnóstico en la rama propietaria del artista ni sobrescribir trabajo concurrente. Siguiente integración requiere relectura de claims de `main`.

**Manual relevante:** Blender Boolean Exact `bpy.types.BooleanModifier` y exportador glTF (Image Texture enlazado a Principled BSDF) documentados en https://docs.blender.org/api/5.4/bpy.types.BooleanModifier.html y https://docs.blender.org/manual/en/5.4/addons/scene_gltf2.html.

## Cierre del diagnóstico del revisor (rondas V2 y V3)
- [Run 38779291224](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38779291224), artefacto 11994254492: VL8B reconoce correctamente los tres nuevos miembros y da `IMPROVED`, pero copia literalmente las tres descripciones de fase 1, presenta solo un argumento genérico y concede 8–9/10 sin baseline. **Rechazar la supuesta nota alta como certificación; no hay evaluación artística independiente suficiente.**
- [Run 38780125999](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38780125999), artefacto 11994854494: fase 1 reutilizada exactamente y válida, fase 2 ya proporciona scores ANTES/DESPUÉS y dos razones; además incluye la incertidumbre de Unity. Pero la respuesta no respeta campos categóricos (`visible in zoomed-in view`, `mortise and tenon joint visible...`) y repite descripciones genéricas de las tres piezas. Calcula un incremento ponderado declarado de 3.0/10, no apoyado en revisión independiente; el `status` acertadamente permanece **`REVIEW_HOLD`**. Las mortajas ocultas no pueden certificarse a simple vista aunque los Boolean sean reales.
- Guardias reforzadas: `validate_independent_delta()` exige rúbrica antes/después, referencias verificadas, motivos diferentes por pieza, incertidumbre razonable sin Unity, veto a que se atribuya visibilidad a un ensamblaje interno oculto y segunda opinión si el modelo reclama un salto >1,5 puntos en un único render. Estos umbrales son controles prudentes de evidencia, **no** normas de estética objetiva.
- Regla de autoría: los hashes/física proceden del equipo, no se pide a Qwen inventarlos; la reparación JSON se limita a claves/enum whitelisted y archiva las respuestas originales. Un rechazo del crítico por mala identificación/formato es **`REVIEW_HOLD`**, no una condena del asset.
- **Cierre técnico:** el sistema de ensamblaje nuevo se ha ejecutado y preservado; el circuito de visión tiene ahora barreras auditables y detecta falsos positivos/negativos específicos. **Cierre artístico: PENDIENTE**, no elevar a `ART_APPROVED`, no afirmar que el artista está calibrado ni prometer arte de 8/10. La capacidad debe pasar posteriormente ensayos de generalización y revisión Unity con su propietario.
