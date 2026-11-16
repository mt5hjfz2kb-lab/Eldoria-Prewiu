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


## Resultado visual medido de las primeras dos V9 (no vender CI como calidad)

Se descargaron los **PNG auténticos de Blender** y se inspeccionaron a escala natural.

| Muestra real | CI | Resultado visual | Región de torre principal 770:1025 x 60:337 |
| --- | --- | --- | --- |
| V8 referencia, #43882248142 | PASS | Bloques todavía simples, con arbolado pálido | 36,2 % de píxeles con luminancia < 35 |
| V9 natural #43885990178 | PASS técnico | **RECHAZADA**: fachada central negra y árboles verdes casi grises; no cumple el objetivo | 66,6 % de píxeles con luminancia < 35 |
| V9 estructural #43884858236 | PASS técnico | **REVISIÓN**: mejor continuidad geométrica, pero frontones casi blancos y bosque excesivamente claro | 5,3 % de la zona con luminancia > 185 (V8: 0,6 %) |

Los porcentajes son solo diagnósticos cuantitativos de defectos **visibles** sobre el mismo encuadre, no notas estéticas ni pruebas del Player.

### Hipótesis de causa y cambio sistémico

La arquitectura estructural V9 definía valores de paleta en hexadecimal sRGB y los escribía sin conversión en las entradas `Principled BSDF / Base Color`, que esperan color en espacio lineal. Eso aclara indebidamente piedra, madera y follaje, en especial bajo una configuración de exposición y sol altas. Se ha modificado `hexrgb` para aplicar correctamente la transferencia sRGB→lineal antes de asignar los colores a materiales, junto con una luz menos agresiva. El test de esta hipótesis es el mismo pipeline de 5 niveles y 3 capturas [run #43886721399](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43886721399); **no declarar que mejora hasta inspeccionar los PNG corregidos**.

La segunda causa es arquitectónica: añadir relieves de un solo color sobre masas rectangulares no produce una fachada integrada. Esta hipótesis se comprobará de manera independiente tras resolver la desviación cromática.

