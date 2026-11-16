# Auditoría visual A/B — Director V3/V4 frente a objetivo real de Valoria

**Fecha:** 16-11-2026 · **Rama:** `experiment/eldoria-cc0-art-system-20261116`.  
**Naturaleza:** inspección independiente de PNG auténticos de Blender descargados del artefacto, NO inferencia de su calidad a partir de CI.

## Evidencia directamente examinada
- Director V3: [run 43866662062](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43866662062), 1600×900 Bastión I y V, 720×1200 Bastión V, 5 GLB y una fuente .blend.
- Primer Director V4: [run 43870376065](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43870376065). Los 5 GLB y 3 PNG se produjeron de verdad, pero el gate de diferenciación I/V **falló** con diferencia media global de 3,3019; no presentarlo como SUCCESS completo.
- No confundir ninguna de estas capturas reales con las **ilustraciones conceptuales** creadas durante las conversaciones. Las ilustraciones no existen como geometría del juego.

## Defectos y causas plausibles verificables
1. **Director V3: cuatro cubiertas industriales parecen excesivamente claras y genéricas.** Pese al uso de materiales comunes, las superficies originales eran geométricamente simples; conservar su forma y cambiar solo el color no basta.
2. **V3: coníferas excesivamente repetitivas, copa casi cónica, fondo escaso en volúmenes naturales.** Se requieren siluetas asimétricas y una relación legible entre claros, bosque y arquitectura.
3. **V3/V4: suelo plano por lectura del jugador a distancia y senderos apenas visibles.** Rehacer mezclas cromáticas y trazados pegados al relieve; no reintroducir parcelas, losas ni vías como escalerillas.
4. **V4: iluminación excesivamente oscura en la cámara real.** Medición sobre PNG 1600×900: RGB medios ~31/43/41 y **74,8 % de los píxeles con luminancia inferior a 45/255**; V3 fue ~84/93/81, con 8,1 % bajo 45. La mejora de atmósfera V4 empeoró legibilidad. Se impone restablecer una exposición equilibrada antes de microdetalles.
5. **V4: el Bastión V conserva demasiado la forma inicial en pantalla completa.** La diferencia media de imagen 3,3019 (fallo CI); el salto necesita masa arquitectónica, una torre alta real, coronación y entrada distinta, no solo insignias superficiales.
6. **Rendimiento:** el V3 sin agrupar tiene 1.828 mallas en estadio V; no deducir FPS de su tamaño de fichero. La agrupación deberá regenerarse para cada variante y contrastarse con geometría/texturas y cámara real.

## Respuesta implementada para inspección
- [Director V5 script](../../tools/cc0-art-system/director_v5_scene_refine.py), código **Blender**, añade físicamente una torre de observación del V, sendas continuas, arbolado irregular, útiles de industria y una iluminación revisada, con 5 GLB y nuevas capturas.
- [Workflow Director V5](../../.github/workflows/cc0-director-v5.yml) usa como referencia exacta los ficheros V4 del run 43870376065; **ninguna fuente externa vendida o no certificada**.
- El éxito de V5 requiere en este orden: ejecución técnica, imágenes contrastadas, revisión artística, agrupación móvil, integración Unity y prueba en teléfono. No sustituir las etapas por una puntuación numérica.

## Condición de parada y pivote
Si las nuevas capturas siguen pareciendo un conjunto de primitivas, **la solución no es añadir más torres/ornamentos repetidos**. Cambiar el método de modelado a masas arquitectónicas editadas a mano en Blender, materiales comunes con normal maps y trazos de terreno, usando el código actual solo como distribución inicial. La coherencia requiere geometría, no solo paleta.
