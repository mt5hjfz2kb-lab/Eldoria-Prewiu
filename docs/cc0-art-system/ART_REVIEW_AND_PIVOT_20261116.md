# Registro crítico A/B — fuente gratuita frente a unidad artística

**Fecha:** 2026-11-16. **Hito:** [run #43846555487](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43846555487), generación Blender y validación de tres PNG correctas. **No fue éxito completo de workflow**: commit de capturas falló por cambios locales de archivos propios generados al hacer rebase.

## Conclusión del examen VISUAL (no aprobatorio)

- Se descargaron a disco efímero **18 instancias de 11 archivos diferentes** de Quaternius: cuartel, almacén, mina, troncos, ruinas FBX, pinos y rocas.
- La prueba demuestra importación real y composición básica. Pero el conjunto **no alcanza nivel de calidad suficiente**: Quaternius aparece con material blanquecino/verdoso y detalle más simplificado frente a la fortaleza propia. Algunos restos de ruinas quedan cerca/detrás de la silueta sin enriquecerla. El terreno pierde actividad porque se retiraron demasiados elementos propios.
- **Lo que funcionó:** la geometría del Bastión mantiene silueta y las posiciones geográficas son estables; existe material gratis de construcción que puede inspirar y donar partes secundarias.
- **Lo que NO funcionó:** asumir que mezclar mallas originales directamente con una simple desaturación automáticamente genera un estilo coherente. La heterogeneidad de escala, detalle, silueta y material sobrevive a esa técnica. No resolver por más y más micro-recoloreados.

## Pivote de dirección aprobado para prueba técnica, no para producción

**Mantener el Bastión y una familia de arquitectura secundaria derivada de sus mismas reglas geométricas y materiales.** Separar físicamente aserradero, cuartel, granero y cantera, conectándolos con caminos naturales, sin plataformas. Añadir identidad propia a cada función (almacenamiento, entrenamiento, grano, extracción). Construir 5 niveles físicamente exportables desde una sola fuente editable.

Quaternius queda **como fuente de referencia y piezas subordinadas** solo cuando una importación particular resuelva un problema con una transformación demostrable y legalmente documentada. El conjunto mayoritario de Quaternius RTS sin cambios **no se declara base visual definitiva**. La alternativa consistente de un único pack RTS queda disponible para evaluación posterior, pero exigiría cambiar también la fortaleza y personajes, no insertar kits diversos sin criterio.

## Criterios de siguiente corte

1. Bastión V visiblemente superior I, mismas coordenadas/cámara.
2. Las cuatro industrias separadas del principal, cada una identificable, apoyadas en terreno continuo.
3. Dos caminos secundarios que unan sin formar calles artificiales ni parcelas.
4. Luz cálida y fría balanceadas, materiales compartidos, ventanas sin sobreexposición.
5. Screenshot Blender real con fuentes 3D editables y GLB físicos para 5 niveles. **Unity/iPhone pendientes**.
6. Publicar comparativa A/B en la rama sin tocar main y no calificarla como arte aprobado automáticamente.

**Nota legal:** hasta acreditar la licencia original concreta, ningún FBX/glTF Quaternius debe publicarse como fuente en GitHub; los renders aplanados se consideran pruebas ilustrativas. Cinder UI sigue sin importar en repositorio público.
