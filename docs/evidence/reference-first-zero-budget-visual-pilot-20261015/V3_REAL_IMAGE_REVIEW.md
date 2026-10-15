# V3 — evaluación de capturas auténticas de Unity (rechazado como arte final)

Origen original: GitHub Actions ejecución [38806340483](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38806340483) (Windows DESKTOP-R10PE55), commit fuente **9c970cc64177896b0dd92ae8d5750c8b9d534c6f**; trabajo de render **SUCCESS**, trabajo de archivado **FAIL** por nombres de PNG inconsistentes, no por Unity ni por contenido inexistente. ZIP del artifact: **12008060189**, 5 archivos; los dos PNG y el manifiesto fueron extraídos y visualmente inspeccionados.

- 03-fortified-plateau-overview.png: SHA256 **c4684d47e5d1290e79b49a5794fce2caa37da97daf5cc86f2f462941df2ea21f**, 1536x1024.
- 04-fortified-plateau-architecture.png: SHA256 **bdbc7e58096ec31ec59e6ea408dd4252311746eace845e65c8b9f0cde4788d6e**, 1536x1024.
- Manifiesto real: 26 instancias arquitectónicas, 72 árboles, 41 rocas; ART_APPROVED=false, producción sin modificar, licencia no validada.

## Comparación real contra la imagen correcta del usuario

La nueva composición **YA contiene** dos puertas distinguibles, puente inferior, patio central despejado, camino axial, escalera marcada, recinto superior con una muralla transversal, partes laterales y coníferas. Esto supera claramente la disposición equivocada V1/V2 desde el punto de vista ESTRUCTURAL, no equivale a 8/10 artístico.

**RECHAZO ARTÍSTICO PROVISIONAL** por las siguientes divergencias visibles:
- El acantilado tiene fallos evidentes de malla/caras (bordes negros verticales y aristas), no se ve macizo geológico natural y continuo.
- Vegetación de borde excesivamente exterior o flotante en agua, y árboles distribuidos como collar regular artificial.
- Suelo interior liso, monótono y de tono muy apagado, sin hierba, erosión ni mezcla de tierra.
- Camino y escalera planos y repetitivos, sin pavimento de sillares ni contacto natural con el terreno.
- Fragmentos laterales de muralla demasiado separados; piedra de ellos gris-lila, no tiene coherencia con arquitectura principal crema.
- Agua de apariencia plana e iluminación azul homogénea, sin horizonte, montañas o valle con profundidad atmosférica.
- Detalle artístico no cumple la referencia; todavía es un prototipo de composición.

## Decisión y siguiente corrección V3b

Código V3b (en la misma rama, commit a verificar) ha corregido orientación de triángulos del cliff, colocado pinos sobre meseta y terraza superior, añadido mampostería continua detrás de la muralla superior, suelos de tierra variados y reparado nombres de PNG para archivado correcto. **Estos cambios son hipótesis, no logros visuales**, hasta generar y revisar el siguiente render real.

No fusionar en main ni en escena jugable. No confundir TECH_PASS con ART_PASS. Objetivo 8/10 sigue pendiente. 
