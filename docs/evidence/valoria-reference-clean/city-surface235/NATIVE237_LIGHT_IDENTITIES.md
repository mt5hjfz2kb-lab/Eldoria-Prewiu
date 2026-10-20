# Corrección de las luces tras native237

Run39495268604 corrigió los renderizadores pero las cuatro luces conservan identidad horneada. No declarar restaurado un resplandor que las imágenes no muestran. URP existente ya permite cuatro luces adicionales; no cambiar presupuesto URP ni materiales.

Reutilizar las cuatro posiciones, colores y radios, sustituir solo los componentes de luz por identidades nuevas Realtime con salida no horneada, deshabilitando originales y manteniendo referencias. Intensidad directa 4,4 (original2,2) para recuperar lectura cálida a luz diurna; sin sombras puntuales ni luces activas extra. Revisar seis imágenes y después guardar/reabrir con comparación estricta. Las fuentes de geometría y colliders no cambian.
