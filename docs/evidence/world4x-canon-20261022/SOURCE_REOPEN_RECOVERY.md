# Ribera I — conservación de la fuente al reabrir

La versión I visible en Unity se conserva como autoridad de partida (commit `14d134cf4c719824a6db6a28761556bebdbbf8c8`, ejecución `39953231775`). No se reconstruye la escena.

En la primera derivación J, las pruebas funcionales pasaron pero el terreno apareció negro. La textura `I retained surface new contact` estaba exportada correctamente dentro del GLB de I, pero el bucle histórico que guardaba únicamente `image.type == IMAGE` no había guardado y empaquetado esa imagen generada. Al reabrir el `.blend`, los píxeles generados no estaban conservados.

Se recuperó la imagen exacta del buffer PNG del GLB de I, comprobando previamente su SHA256 `76f5129b09f788e0e3c9f57d6a255ba3ad9f8317ea8e80e174e7231874e75f2a`. Se sustituyó exclusivamente la referencia a esa imagen en el material y se guardó/empaquetó. La comparación local de los píxeles originales y los reexportados resultó idéntica (2048 × 2048 RGB). La reparación no repinta el terreno ni utiliza capturas como textura.

La fuente nueva se vuelve a abrir en Blender antes de renderizar. El inventario de objetos debe reconstruirse después de abrir un archivo: los punteros RNA de la sesión anterior quedan invalidados. La ejecución intermedia `39956549482` descubrió ese segundo problema en la generación del manifiesto; no llegó a Unity y no constituye evidencia de PlayMode.

Lección aplicable: conservar imágenes generadas por su `source`, guardar y empaquetar explícitamente, volver a abrir la fuente, y comprobar las capturas reales. Dieciocho pruebas verdes no detectaron el terreno negro del primer J.
