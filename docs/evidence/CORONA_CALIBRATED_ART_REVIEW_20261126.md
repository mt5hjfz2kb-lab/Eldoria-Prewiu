# Calibrated visual review: original 24 November weights

Real independent agent scorecard, run [45844200241](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45844200241), artifact 15130965763. The failed validation run 45843367417 produced a paid HUD receipt reused without another call. Seven actual completed calls; estimated cost 0.10939600 USD, not invoice-verified.

Same seven weighted dimensions as 24 Nov. Before/after/reference totals computed from seven numeric subscores, not the evaluator's prose arithmetic. Animation excluded (still frames); world A/B compares the palette toggle with fixed camera at ff387633, not complete revisions. UI and city compare 5ef2aee to 3486754. The final corrected 41c124f revision is being measured separately. Do not infer approval or mix scores across source revisions.

Mean 6.82 → 6.86; 16 improved, 16 unchanged, 1 regressed. No row meets its minimum target.

| Role | Before | After | Delta | Reference | Kind |
|---|---:|---:|---:|---:|---|
| hud-b1 | 6.98 | 6.98 | 0.00 | 8.23 | direct |
| hud-b5 | 7.09 | 7.09 | 0.00 | 8.23 | direct |
| tutorial | 7.24 | 7.24 | 0.00 | — | none |
| construction | 6.88 | 6.95 | 0.07 | — | none |
| march | 6.83 | 6.75 | -0.08 | 8.22 | direct |
| bag | 6.71 | 6.91 | 0.20 | 8.05 | direct |
| mail | 6.92 | 7.02 | 0.10 | 8.08 | direct |
| heroes | 7.18 | 7.28 | 0.10 | 8.18 | direct |
| training | 7.18 | 7.21 | 0.03 | 8.24 | direct |
| name | 7.14 | 7.38 | 0.24 | — | none |
| daily | 6.87 | 7.17 | 0.30 | 7.98 | analog |
| scouts | 7.15 | 7.15 | 0.00 | 7.60 | analog |
| vigilants | 7.17 | 7.17 | 0.00 | — | none |
| council-b1 | 6.34 | 6.34 | 0.00 | — | none |
| council-b2 | 6.08 | 6.08 | 0.00 | — | none |
| sawmill-built | 6.75 | 6.75 | 0.00 | — | none |
| bastion-upgrade | 5.92 | 5.92 | 0.00 | — | none |
| barracks-plot | 5.92 | 5.92 | 0.00 | — | none |
| bastion-1 | 6.86 | 6.86 | 0.00 | 8.01 | analog |
| bastion-2 | 7.01 | 7.01 | 0.00 | 8.01 | analog |
| bastion-3 | 7.09 | 7.09 | 0.00 | 8.01 | analog |
| bastion-4 | 7.10 | 7.10 | 0.00 | 8.01 | analog |
| bastion-5 | 7.29 | 7.29 | 0.00 | 8.01 | analog |
| bastion-5-panorama | 7.29 | 7.29 | 0.00 | 8.01 | analog |
| home | 7.17 | 7.20 | 0.03 | — | none |
| wood | 7.17 | 7.20 | 0.03 | — | none |
| stone | 7.14 | 7.17 | 0.03 | — | none |
| woodland | 7.00 | 7.01 | 0.01 | — | none |
| river | 7.09 | 7.11 | 0.02 | — | none |
| region | 6.25 | 6.30 | 0.05 | — | none |
| breach | 6.25 | 6.30 | 0.05 | — | none |
| panorama | 6.08 | 6.12 | 0.04 | — | none |
| breach-panorama | 6.08 | 6.12 | 0.04 | — | none |

## Required corrections
- **hud-b1:** Mejorar legibilidad de etiquetas, jerarquía e integración de indicadores con el fondo. La presentación visible no permite evaluar animación.
- **hud-b5:** Reforzar contraste y tamaño de rótulos y separar mejor sus contornos del fondo. La puntuación del HUD no premia la cantidad de edificios; animación no evaluada.
- **tutorial:** Aumentar legibilidad del coste y subtítulo y ajustar la separación de la mano respecto al panel. No hay referente directo de este panel entre los aportados; animación no evaluada.
- **construction:** Mejorar el equilibrio entre tamaño del temporizador y espacio de la escena, y comprobar su lectura a 390×844. No hay referente directo de este estado entre los aportados; animación no evaluada.
- **march:** Ampliar las etiquetas del resumen, equilibrar el espacio vacío del bloque de tropas y mejorar la separación entre controles. Animación no evaluada; finish mide sólo la presentación visible.
- **bag:** Mejorar la diferenciación entre tipos de recurso, reducir la repetición de avisos rojos y comprobar contraste y tamaño de los textos. Animación no evaluada; finish mide sólo la presentación visible.
- **mail:** Aumentar el contraste de fechas y textos secundarios, y homogeneizar la escala y el tratamiento de las miniaturas. Animación no evaluada; finish mide sólo la presentación visible.
- **heroes:** Dar más protagonismo a la información de cada héroe, distinguir mejor los espacios bloqueados y reducir la acumulación de marcos. Animación no evaluada; finish mide sólo la presentación visible.
- **training:** Aumentar la legibilidad de costes y estado, dar más separación visual a los controles y resolver la transición entre ilustración y panel oscuro. Animación no evaluada; finish mide sólo la presentación visible.
- **name:** El fondo arquitectónico compite con el panel y la barra superior queda muy oscurecida; el campo sigue siendo una superficie visualmente pesada.
- **daily:** Las celdas oscuras separan débilmente algunos premios y los textos secundarios son pequeños; el botón final es estrecho. En el referente, la superposición también congestiona parte de la lista.
- **scouts:** El mapa concentra mucha textura y las miniaturas, recompensas y tiempos son pequeños para móvil; las tarjetas inferiores quedan apretadas. La captura horizontal del referente limita también la comparación de legibilidad a 390×844.
- **vigilants:** Los marcadores se pierden parcialmente sobre el paisaje detallado; estadísticas y recompensas son pequeñas y el personaje queda poco integrado en el panel.
- **council-b1:** Despejar el encabezado, reforzar la jerarquía entre acciones y mejorar la integración del panel con la escena. Sin referente equivalente de consejo entre los aportados.
- **council-b2:** Dar margen a la última fila, ordenar las acciones por prioridad y despejar el encabezado. No hay referente equivalente de consejo entre los aportados.
- **sawmill-built:** Mejorar el tamaño del texto secundario y la separación del cierre respecto del marco. No se aporta un referente equivalente de panel de edificio construido.
- **bastion-upgrade:** Separar costes y acción, ajustar la altura del panel a su contenido y despejar el título. No se aporta un referente equivalente de mejora de edificio.
- **barracks-plot:** Separar la acción de sus costes, reducir el espacio vacío y dar al encabezado una zona limpia. COD training muestra entrenamiento de unidades, no un panel equivalente de parcela por construir.
- **bastion-1:** El terreno repetitivo ocupa gran parte del encuadre; los caminos y contactos de los edificios necesitan transiciones más naturales. Las figuras pequeñas pierden definición.
- **bastion-2:** Persiste mucho césped uniforme, una unión blanda entre suelo y caminos y detalle diminuto en los personajes.
- **bastion-3:** La plaza y los caminos podrían definir mejor sus bordes y contactos; el espacio vacío y las figuras muy pequeñas debilitan la lectura.
- **bastion-4:** La red viaria se diluye en el césped y varios elementos quedan cortados en el borde derecho; las figuras pequeñas son difíciles de distinguir.
- **bastion-5:** La agrupación de edificios ofrece mayor riqueza visual, pero algunos contactos y bordes de camino son blandos; el detalle fino se pierde a escala móvil y hay recortes laterales.
- **bastion-5-panorama:** Mejorar el encuentro entre terreno, pavimento y construcciones, así como la legibilidad de personajes y detalles pequeños; revisar el recorte de edificios en los bordes.
- **home:** La pradera es repetitiva y algunos elementos pequeños pierden definición a escala móvil. Mejorar la transición del río y el contacto de los elementos con el terreno.
- **wood:** Los caminos y las bases de vegetación necesitan transiciones más naturales; el detalle fino del aserradero puede perderse en móvil.
- **stone:** La mitad inferior queda muy vacía y repetitiva. La cantera y los árboles podrían asentarse mejor sobre el terreno, y el detalle de la maquinaria es pequeño para móvil.
- **woodland:** La zona central es extensa y uniforme, con foco débil. El borde del grupo boscoso y las transiciones con el terreno resultan abruptos; varios detalles son pequeños a escala móvil.
- **river:** La orilla se funde de forma irregular con el césped y la composición corta elementos principales en los bordes. Persiste una amplia zona de terreno repetitivo.
- **region:** Persisten vegetación repetitiva, caminos finos y contactos poco definidos. A tamaño móvil, los objetos menores compiten entre sí. No hay referente de mundo directamente aplicable.
- **breach:** La grieta destaca, pero su contacto con el terreno es brusco; el castillo queda cerca del borde y los elementos pequeños pierden lectura. No hay referente de mundo directamente aplicable.
- **panorama:** A esta escala los recursos individuales y los caminos son difíciles de distinguir en móvil; la textura del terreno y las agrupaciones de árboles resultan repetitivas. No hay referente de mundo directamente aplicable.
- **breach-panorama:** La grieta es pequeña en el encuadre general y los detalles del mapa tienen baja legibilidad móvil. Persisten patrones de vegetación y transiciones de terreno poco refinadas. No hay referente de mundo directamente aplicable.

## Final source-specific measurement
The separately launched run 45844755669 uses the same rubric and remaining $0.42 reservation, after real captures from native run 45842935818. No promotion until actual evidence.