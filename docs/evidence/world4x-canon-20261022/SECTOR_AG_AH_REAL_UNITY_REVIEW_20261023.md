# Eldoria sector conectado — revisión nativa AG/AH, 23 octubre 2026

## Candidato retenido y exclusiones
- **AG retenida** como última composición artística técnicamente comprobada; rama `feature/world4x-region1-canon-20261022`, PR #100 en borrador. Fuente del acabado idéntica al commit `84ce3ae48b1818b6bee7b4420d32ce5fa3df4169`, con su historia preservada.
- AG: [Unity+Blender run 40012204581](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40012204581), artefacto **12527505668**. Unity PlayMode XML **18/18 PASS**; 6 PNG sectoriales, 49 fotogramas horizontales y 49 verticales procedentes de `Camera.Render`, .blend editable, GLB, texturas y máscara empaquetados.
- AH: [run 40013431035](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40013431035), artefacto **12527830825**, 18/18 PlayMode, **DESCARTADA ARTÍSTICAMENTE**. Los 5 nuevos afloramientos y el aumento de altura crearon placas pétreas de aspecto geométrico/blanquecino en las cámaras reales. Las fracciones de píxeles modificados (>5/255 RGB, seis vistas AG→AH): horizontal cerca 1,08 %, regional 1,60 %, panorama 0,85 %; vertical cerca 1,67 %, regional 1,36 %, panorama 1,00 %. NINGUNA de estas cifras es una nota artística.
- La AE (run 40008174998, artifact 12524838329) tampoco fue retenida: 285 hierbas triangulares parecían confeti oscuro. AF adelantó excesivamente el impostor regional y perdió la forma del bosque; AG restableció la transición tridimensional.

## Lo comprobado en AG
- 429 árboles totales. Los **16.405 vértices de O y sus 156 árboles** permanecen intactos. K, I, O, Z, AE, AF y AH son recuperables mediante commits/runs; no expandir Región 1.
- Objetivos: **24/24** selecciones del piloto correctas, **6 colisionadores lógicos sin desplazamiento**. Cinco marcadores visuales ajustados a la malla Blender con error de contacto <0,03 m; engendro animado con contacto medido y pose restaurada, sin alterar combate, guardados ni progresión.
- El diagnóstico bruto de base de colisionadores sigue con **maxHeightDifference=8,2472496 m** y `heightDeltaIsGroundingVerdict=false`. No lo transformar en PASS ni mover colisionadores para falsear un resultado. Los visuales sí están apoyados, pero **certificación de contacto físico/semántico de cada objetivo todavía pendiente**.
- LOD nativo, cuenta estimada de triángulos de renderers activos (NO medida de GPU/FPS móvil): cerca horizontal **753.632**, regional horizontal **754.490**, panorama horizontal **101.552**; panorama vertical **94.322**. Sin perfilado físico de iPhone. La transición de representación en 37–47 de tamaño ortográfico se conservó después de rechazar el cambio demasiado precoz de AF.
- El mismo relieve/geografía está presente en seis cámaras y 49+49 cuadros continuos. No se estiran coordenadas al pasar de horizontal a vertical.

## Juicio visual y siguiente técnica
La masa forestal y el terreno mejoraron de W a AD/AG, y las rutas continúan; aun así, la mitad sur del encuadre sigue con pradera plana y caminos excesivamente limpios. AH demuestra que más piedras locales tampoco solventa esa escala compositiva. **No repetir AH ni AE.** Si se persigue el objetivo 7,5–8, reconsiderar la división visual de la pradera a nivel macroscópico, su volumen y las transiciones de materiales, con referencias y cámaras idénticas. No se concede `ART_APPROVED`.

NO MERGE · NO RELEASE · NO GASTO · NO CERTIFICACIÓN DE FPS.
