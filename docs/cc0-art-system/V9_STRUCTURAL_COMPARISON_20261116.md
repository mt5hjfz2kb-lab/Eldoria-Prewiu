# V9 — comparación estructural A/B sin modificar la beta

**Experimento:** Eldoria CC0 Art System — `eldoria-cc0-art-system-20261116`  
**Rama de trabajo:** `experiment/eldoria-cc0-art-system-20261116`  
**Carácter:** hipótesis de dirección de arte y QA, no aprobación ni publicación.  
**Último V8 comprobado:** [run #43882248142](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43882248142), ZIP `eldoria-cc0-director-v8-real-3d` (5 GLB reales, BLEND editable, 3 renders Blender).

## Hechos visuales observados en la captura real de V8

1. Los pinos presentan una silueta excesivamente rígida y gris/mint; aunque las fuentes materiales tengan verdes oscuros, el resultado renderizado **no cumple** la referencia ambiental.
2. La torre alta del Bastión V es un volumen liso y rectangular; la unión de cubierta/ventanales parece una pieza sobrepuesta, insuficiente para la identidad imperial.
3. Las construcciones secundarias se reconocen parcialmente, pero cuartel/granero siguen demasiado próximas a un volumen genérico de almacén.
4. El terreno es plano en zonas amplias, con escasa integración de la arquitectura en la topografía y las huellas de tránsito.
5. El GLB V8 tiene 1.531 mallas a nivel I y 1.885 a nivel V, según su manifiesto. Eso **NO** demuestra coste real en iPhone; obliga a optimizar antes de promover.

## Pruebas separadas, ninguna fusiona ni sustituye main

| Candidato | Escritura | Fuente | Verificación |
| --- | --- | --- | --- |
| V9, estudio natural | `tools/cc0-art-system/director_v9_natural_forest_and_integrated_keep.py` | V8 de run #43882248142 | [run #43884700954](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43884700954); consultar si terminó y revisar las imágenes |
| V9 estructural (esta comparación independiente) | `tools/cc0-art-system/director_v9_architectural_lift.py` | **Mismo** V8, no materiales de proveedores | [run #43884858236](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43884858236); carpeta de salida `DirectorV9Structural` y `previews/director-v9-structural` |

### Modificaciones geométricas reales del candidato estructural

- Copas de coníferas **reconstruidas físicamente** como abanicos y ramajes asimétricos reutilizables, en colores de bosque adulto, no sprites.
- Cubiertas de pizarra con pendientes, borde, alero, hiladas, cumbrera y remates modelados; ampliación del gran salón nivel V y fusión de alas con pilares/contrafuertes.
- Portón y ventanal góticos reconstruidos como geometría, con profundidades, dinteles, transiciones y heráldica restringida.
- Cantera con grúa, extracción y piedra cortada; aserradero con rueda y maderas; cuartel con techo defensivo y entrenamiento; granero con silos de piedra y tejado distintivo.
- Mismo encuadre y emplazamientos, cinco GLB reales y una fuente Blender editable. Ninguna imagen conceptual se utiliza como textura del escenario.

## Cómo decidir

**Primero:** correr el código y validar realmente las cinco geometrías, materiales embebidos, tres capturas y progresión I→V. **Segundo:** inspeccionar las imágenes de ambas variantes V9 y V8 control con la misma escala. **Tercero:** comparar cambios visibles de silueta, materiales, separación, identidad y vegetación. **Cuarto:** pivotar si sigue pareciendo maqueta.

Un CI en verde no equivale a `ART_APPROVED`; no afirmar integración Unity ni iPhone sin ejecución real. No agregar paquetes comerciales sin licencia de redistribución de fuente. No acceder al Windows ajeno; el registro en main debe comprobarse antes de reservarlo.

**Estado al redactar:** V8 real validado; generación V9 estructural y alternativa natural lanzadas, pendientes de inspección de resultados. Actualizar esta línea con el estado exacto al final del lote, sin extrapolar una ejecución pendiente.
