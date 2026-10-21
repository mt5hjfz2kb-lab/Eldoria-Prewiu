# Eldoria — Tripo | LOD móviles de fuentes existentes (Jabalí y Foco)

**Fecha:** 2026-10-21. **Tipo:** prueba técnica sobre fuentes archivadas, **no aprobación artística ni integración jugable**. No se pagan ni regeneran modelos, ni se utiliza Windows reservado ni se reemplaza el Engendro ya integrado.

## Finalidad

Tras los primeros tres modelos H3.1 de Tripo, Eldoria demostró fuente original y GLB optimizado en Unity, pero el perfil único de unos 49.800 triángulos **no es un presupuesto móvil**, sobre todo para recursos repetidos a distancia. Esta prueba aplica los medios existentes de Blender a los dos modelos todavía fuera de producción: **Jabalí de roca** (Capítulo III, aún no desbloqueado) y **Foco de corrupción** (prop experimental, nunca hotspot).

La hipótesis que se prueba es estrictamente técnica: que podemos obtener variantes geométricas bastante más ligeras del mismo GLB pagado anteriormente, conservar UV0/PBR y presentar evidencia visual real sin gastar nuevos créditos. **No** mide FPS/temperatura Android, calidad percibida dentro de Unity ni equivalencia artística. No promueve automáticamente nada.

## Infraestructura reutilizada y ampliación aditiva

- **Fuente, no generación:** ArtSource/World4X/RockBoar_v1/RockBoar_Optimizado_49800.glb (SHA-256 897c8d02a445c6c760376031c38618698740e9593346099fafb4c3a0b93c1dc3), ArtSource/World4X/RiftFocus_v1/Optimized_Source.glb (SHA-256 00511927ba818a1f5bf4193e2ee1c3533d1b5ac383c8727c9c7c0f5a49f7c53e).
- **Autor existente:** tools/blender_capability_proof.py. Cada ratio lee una copia del .blend temporal original, no decima un LOD sobre otro; nunca toca los GLB de ArtSource/.
- **Perfil explícito:** pipeline/tripo-mobile-lod-profiles.json; ratios experimentales LOD0 = 1, LOD1 = 0,25, LOD2 = 0,08. Los umbrales de triángulos son límites de QA propuestos para el experimento, **no** reglas universales de Eldoria.
- **QA nueva, no nuevo pipeline de producción:** tools/visual-lod-qa/render.py crea 6 PNG auténticos por modelo (isométrico y estratégico en los 3 LOD) con cámara original fijada y Cycles CPU en Ubuntu. tools/visual-lod-qa/verify.py reutiliza tools/art-rd/audit_glb.py, verifica SHA de las fuentes, triángulos reales, UV/normales, materiales PBR, imágenes embebidas, orden LOD y PNG + hashes. Solo marca TECH PASS, nunca ART_APPROVED.
- **Workflow ligero e independiente:** .github/workflows/tripo-mobile-lod-proof.yml. Usa un runner GitHub-hosted Ubuntu, sin Tripo API, sin Unity, sin credenciales, sin cambios de escena o guardados. El artefacto temporal contiene GLB, capturas, logs y manifiestos de comparación.

## Comprobaciones y criterios de rechazo

1. El SHA original de cada fuente debe coincidir antes de abrir Blender. **Prohibido regenerar** si falta archivo.
2. El GLB de cada LOD debe conservar geometría no vacía, UV0, normales, materiales y al menos todas las imágenes PBR originales embebidas; no tener dependencias externas, rig inventado ni animaciones.
3. Los LOD deben reducir triángulos de forma descendente. El número REAL de triángulos se comprueba en los bytes GLB, no se deduce del ratio.
4. Cada PNG debe proceder de Blender (cámara/iluminación/resolución constantes), tener 640×640 y pasar hash + formato. Esto prueba representación visual **observable**, no calidad artística.
5. Nada se instala en Unity/Assets; ningún evento de Bastión I–II, recompensa, hotspot o persistencia debe cambiar.
6. Los resultados pasan primero por revisión independiente de silueta y materiales. El candidato elegido solo puede entrar al juego tras un A/B verdadero de Unity 19/12/9/móvil y un perfil físico de dispositivo, gestionado por el dueño de la escena.

## Evidencia y estado

**CI en ejecución / resultado por incorporar**: la prueba de los dos activos comenzó en GitHub Actions. El cierre debe añadir el run, artefacto y triángulos medidos, o registrar el fallo exacto; nunca declararse terminada mientras el job siga pendiente.

Las pruebas de Tripo API y cerebro artístico previas permanecen cerradas y separadas: [estado acreditado de tres fuentes](TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md) y [puente Qwen/Tripo](ELDORIA_TRIPO_ART_BRAIN_INTEGRATION_20261021.md). La fase de análisis de rig/animación del Engendro pertenece a otro workstream activo y **no se ejecuta ni duplica** aquí.

## Próximo uso permitido

Si los tres LOD funcionan y el revisor confirma desde los PNG que los más ligeros conservan lectura, trasladar solo sus **informes**, no sus archivos ni escenas, al director artístico. Ante pérdida de silueta, no seguir bajando triángulos automáticamente: corregir la malla o el presupuesto, y comparar en Unity. La memoria artística debe registrar el resultado real, incluidos fracasos, sin prometer aprendizaje de pesos ni una calidad 10/10.

**Presupuesto nuevo Tripo:** 0 créditos. **Rendimiento móvil certificado:** NO. **Arte aprobado:** NO. **Juego modificado:** NO.
