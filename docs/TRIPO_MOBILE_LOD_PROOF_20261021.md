# Eldoria — Tripo | LOD móviles de fuentes existentes (Jabalí y Foco)

**Fecha:** 2026-10-21. **Tipo:** prueba técnica sobre fuentes archivadas, **no aprobación artística ni integración jugable**. No se pagan ni regeneran modelos, ni se utiliza Windows reservado ni se reemplaza el Engendro ya integrado.

## Finalidad

Tras los primeros tres modelos H3.1 de Tripo, Eldoria demostró fuente original y GLB optimizado en Unity, pero el perfil único de unos 49.800 triángulos **no es un presupuesto móvil**, sobre todo para recursos repetidos a distancia. Esta prueba aplica los medios existentes de Blender a los dos modelos todavía fuera de producción: **Jabalí de roca** (Capítulo III, aún no desbloqueado) y **Foco de corrupción** (prop experimental, nunca hotspot).

La hipótesis que se prueba es estrictamente técnica: que podemos obtener variantes geométricas bastante más ligeras del mismo GLB pagado anteriormente, conservar UV0/PBR y presentar evidencia visual real sin gastar nuevos créditos. **No** mide FPS/temperatura Android, calidad percibida dentro de Unity ni equivalencia artística. No promueve automáticamente nada.

## Infraestructura reutilizada y ampliación aditiva

- **Fuente, no generación:** ArtSource/World4X/RockBoar_v1/RockBoar_Optimizado_49800.glb (SHA-256 897c8d02a445c6c760376031c38618698740e9593346099fafb4c3a0b93c1dc3), ArtSource/World4X/RiftFocus_v1/Optimized_Source.glb (SHA-256 00511927ba818a1f5bf4193e2ee1c3533d1b5ac383c8727c9c7c0f5a49f7c53e).
- **Autor existente:** tools/blender_capability_proof.py. Cada ratio lee una copia del .blend temporal original, no decima un LOD sobre otro; nunca toca los GLB de ArtSource/.
- **Perfil explícito:** pipeline/tripo-mobile-lod-profiles.json; ratios experimentales LOD0 = 1, LOD1 = 0,25, LOD2 = 0,08. El ensayo optativo de mapas 1K en LOD2 se implementa en tools/visual-lod-qa/texture_candidate.py; no altera el resto de los LOD. Los umbrales de triángulos son límites de QA propuestos para el experimento, **no** reglas universales de Eldoria.
- **QA nueva, no nuevo pipeline de producción:** tools/visual-lod-qa/render.py crea 6 PNG auténticos por modelo (isométrico y estratégico en los 3 LOD) con cámara original fijada y Cycles CPU en Ubuntu. tools/visual-lod-qa/verify.py reutiliza tools/art-rd/audit_glb.py, verifica SHA de las fuentes, triángulos reales, UV/normales, materiales PBR, imágenes embebidas, orden LOD y PNG + hashes. Solo marca TECH PASS, nunca ART_APPROVED.
- **Workflow ligero e independiente:** .github/workflows/tripo-mobile-lod-proof.yml. Usa un runner GitHub-hosted Ubuntu, sin Tripo API, sin Unity, sin credenciales, sin cambios de escena o guardados. El artefacto temporal contiene GLB, capturas, logs y manifiestos de comparación.

## Comprobaciones y criterios de rechazo

1. El SHA original de cada fuente debe coincidir antes de abrir Blender. **Prohibido regenerar** si falta archivo.
2. El GLB de cada LOD debe conservar geometría no vacía, UV0, normales, materiales y al menos todas las imágenes PBR originales embebidas; no tener dependencias externas, rig inventado ni animaciones.
3. Los LOD deben reducir triángulos de forma descendente. El número REAL de triángulos se comprueba en los bytes GLB, no se deduce del ratio.
4. Cada PNG debe proceder de Blender (cámara/iluminación/resolución constantes), tener 640×640 y pasar hash + formato. Esto prueba representación visual **observable**, no calidad artística.
5. Nada se instala en Unity/Assets; ningún evento de Bastión I–II, recompensa, hotspot o persistencia debe cambiar.
6. Los resultados pasan primero por revisión independiente de silueta y materiales. El candidato elegido solo puede entrar al juego tras un A/B verdadero de Unity 19/12/9/móvil y un perfil físico de dispositivo, gestionado por el dueño de la escena.

## Evidencia y estado: PASS TÉCNICO, no ART PASS

- **Run de fuente/LOD inicial:** [39628155679 — SUCCESS](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39628155679). Blender 4.0.2 gratuito en GitHub-hosted Ubuntu; originales SHA verificados; dos fuentes, tres LOD por fuente, 12 renders Cycles CPU reales, UV0, normales y tres mapas PBR embebidos: PASS. Artefacto: 12357962256.
- **Run final con texturas de prueba 1K:** [39629479262 — SUCCESS](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39629479262). Amplía lo anterior con un cuarto candidato por fuente que **conserva los mismos triángulos del LOD2**, pero reduce albedo y normal de 2048 a 1024; ORM permanece en 1024. Artefacto final `tripo-boar-focus-mobile-lod-qa-verified` / ID **12358279342**, 16 PNG genuinos (8 por fuente), GLB, métricas, hashes y reportes reproducibles. Los artefactos Actions son temporales (14 días); las fuentes y recetas están en main.
- **Coste adicional Tripo:** 0 créditos, sin envío API, sin gasto nuevo y sin usar el equipo Windows reservado.

| Recurso | Triángulos LOD0 → LOD1 → LOD2 | Tamaño GLB LOD0 | LOD2 + mapas 2K | LOD2 + mapas 1K | Reducción de bytes final vs. LOD0 |
| --- | --- | ---: | ---: | ---: | ---: |
| Jabalí de roca | 49.800 → 12.450 → 3.984 | 6.037.236 B | 4.820.248 B | 3.741.384 B | 38,03 % |
| Foco de corrupción | 49.799 → 12.448 → 3.983 | 6.291.500 B | 5.203.456 B | 3.728.512 B | 40,74 % |

**Integridad verificable de los GLB candidatos (SHA-256):**

- Jabalí LOD1 `ab6e5d5732be71c34f0b734d5ccc0fd458d17723f276ad9b31c89b297a4e0a12`, LOD2 `1c66763fa74419303ee875a700c216cbc603ece674500b8c5ca91f66e961edc2`, LOD2 con mapas 1K `03de21c88a66d3006f7b493b80d880e006d170e77809abc15a6ba02c72222c5e`.
- Foco LOD1 `4a84fd07997e1de2bc58601eb88d70beeb3a9fa78a078fc826363f091e29117b`, LOD2 `efd2b44221eb928c76454c4e5a67e867b794ec62acc1c9700b6d1006c3ed58d6`, LOD2 con mapas 1K `e2eb482c74bd25e7f78a4db53daf70db8dfc49d9e6faeafa63ce4769e75e8a4e`.

### Lectura artística limitada de las capturas reales

Las mismas cámaras, luz y escala permiten comparar sin regenerar modelos. **Jabalí:** LOD1 (12.450) mantiene la lectura general y rasgos destacados; LOD2 (3.984) todavía se reconoce desde vista estratégica, pero la nariz, las patas y el borde del lomo presentan mayor facetado y deterioro. LOD2 sería como máximo candidato para **distancias lejanas**, no reemplazo universal del detalle cercano. El cambio adicional de mapas a 1K resulta poco visible en estas dos vistas de Blender.

**Foco:** los LODs conservan la masa general y grietas violetas, pero **no corrigen el defecto artístico principal**: parece un grupo de rocas brillantes más que una manifestación integrada de corrupción. LOD2 tiene aristas más rotas. El texturizado 1K conserva la lectura general de la captura, pero no acredita un diseño aprobado. Para mejorar su valor artístico haría falta reautoría geométrica y de materiales dirigida por Qwen en una obra aparte.

Estas son observaciones sobre vistas aisladas de Blender, **no** certificación de ART_APPROVED, consumo real GPU, calidad en Unity ni FPS/batería Android. Los ahorros en megabytes son del archivo GLB, no medición de memoria comprimida en dispositivo.

Las pruebas de Tripo API y cerebro artístico previas permanecen cerradas y separadas: [estado acreditado de tres fuentes](TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md) y [puente Qwen/Tripo](ELDORIA_TRIPO_ART_BRAIN_INTEGRATION_20261021.md). La fase de análisis de rig/animación del Engendro pertenece a otro workstream activo y **no se ejecuta ni duplica** aquí.

## Próximo uso permitido

Si los tres LOD funcionan y el revisor confirma desde los PNG que los más ligeros conservan lectura, trasladar solo sus **informes**, no sus archivos ni escenas, al director artístico. Ante pérdida de silueta, no seguir bajando triángulos automáticamente: corregir la malla o el presupuesto, y comparar en Unity. La memoria artística debe registrar el resultado real, incluidos fracasos, sin prometer aprendizaje de pesos ni una calidad 10/10.

**Presupuesto nuevo Tripo:** 0 créditos. **Rendimiento móvil certificado:** NO. **Arte aprobado:** NO. **Juego modificado:** NO.
