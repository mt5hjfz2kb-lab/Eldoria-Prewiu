# Valoria Corona — ubicación semántica de etiquetas 2.5D

**Fuente probada:** captura auténtica Unity WebGL `70444c`, [run 44802943006](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44802943006) a 390×844. La etiqueta BASTIÓN quedaba encima del pozo y del patio civil; CONSTRUIR ASERRADERO se dibujaba muy por debajo del solar de construcción. No era un problema del espacio vacío sino de anclaje de señalización.

**Corrección de candidato implementada** en `Modular25DInterface.SyncWorldNameplates`: mover el ancla de la etiqueta del Bastión desde (-0.40,-0.55) a (-1.63,3.05), encima de la torre occidental y alejada del objetivo superior derecho; mover la etiqueta del Aserradero desde (0.96,-3.86) a (0.96,-2.90), junto al extremo inferior de la parcela, no en el terreno distante. Las placas siguen ligadas a `WorldToScreenPoint` y obedecen la visibilidad segura en retrato/horizontal y el cierre al abrir paneles. No se mueven edificios, cámara, hitboxes ni elementos ambientales.

**Verificación pendiente:** ver capturas reales Unity/WebGL exact-SHA en 390×844 y 844×390, con zoom/pan, que no haya solape con texto de objetivo, ruinas, tejados ni terreno de trabajo. Una superposición geométrica basada en una imagen antigua es sólo una predicción, no certificado artístico.
