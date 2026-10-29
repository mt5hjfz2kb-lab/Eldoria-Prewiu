# Revisión rechazada: orientación individual de edificios V

El propietario rechazó también city-V-complete-horizontal: cambiar la imagen experimental por la normal no resolvía las fachadas oblicuas. Se retira la conclusión de que la orientación estaba validada. No publicar Android/WebGL hasta nueva revisión visual nativa.

La rotación uniforme270 no compensa los ejes diagonales diferentes de los GLB. Los planos de fachada medidos sobre triángulos verticales de los modelos inmutables, contrastados con vistas texturizadas de sus entradas, dan los candidatos Unity: Bastión235, aserradero313, cuartel231, granero306 y cantera318 grados. La entrada del Bastión se centra sobre el eje de su acceso; el modelo regional conserva su anclaje estratégico y el apoyo al terreno.

Los renders locales con geometría original sirven para descartar giros incorrectos antes de ocupar Unity. No son evidencia de integración Unity ni aprobación final. El nuevo control nativo elimina de la entrega las variantes con rotaciones acumuladas y genera cinco primeros planos, una planta de alineación, vistas normales horizontal/vertical y un registro de transformaciones reales. Guardados, progresión por nivel y modelos aprobados permanecen sujetos a regresión completa.

Estado: candidato de corrección, pendiente de capturas genuinas Unity y revisión de accesos. No ART PASS por código o ángulos solamente.

Native40925435457 confirma las cinco fachadas alineadas, pero rechaza todavía el cierre: la puerta queda separada del camino y el raycast panorámico falla. Renderer.bounds es una caja de malla girada, no el borde real de fachada; se sustituye el anclaje estimado por el umbral medido sobre el GLB original. La caja táctil se obtiene de las ocho esquinas transformadas, evitando que InverseTransformVector sobre un tamaño cancele ejes cuando hay rotación. Se conservan imágenes y fallo originales.

Causa del fallo panorámico identificada: RegionOneReferenceNodes.Refresh restablecía la caja I–IV al cambiar el zoom, antes del LateUpdate del presentador V. La representación antigua deja de escribir esa caja mientras existe la malla V; al recargar niveles anteriores recupera el tamaño original. Se mantiene la prueba de raycast exacta, sin relajar su condición.
