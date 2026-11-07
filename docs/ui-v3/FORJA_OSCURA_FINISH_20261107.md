# Forja Oscura: acabado sobre evidencia nativa

La certificación general 42412604560 terminó correctamente en cc84c9be: progresión I–II, tres marchas, Aldric, EditMode y PlayMode. El paso de compilación Windows estaba omitido: no es prueba de un nuevo ejecutable. Las doce capturas anteriores pertenecen al runtime 10f63181, run 42412459100.

## Diagnóstico y cambio de enfoque

La superficie naranja procedural no reproduce el volumen/material de la referencia. La separación de icono y texto en el dock móvil tampoco estaba garantizada: el layout original reescribe geometría cada frame y la corrección abandonaba los botones que ya cumplían el tamaño táctil.

- Nueva superficie amber pintada generada con imagegen incorporado, referencia visual original A19C8134 y botón anterior como entradas; PNG RGBA sin retoque posterior. SHA256 b49c15d7ad54747f8f1fd9edd5542b0b0f08b572c73991f508b800f39a07dbe5. La fuente exacta está en Resources/ForjaOscura/Surfaces/button_amber_painted.png. Las piezas previas se conservan.
- Dirección de autoría: una sola placa frontal de hierro envejecido y bronce, interior cuero ámbar oscuro, biseles iluminados, sin texto/iconos, transparencia real, detalles laterales aptos para nueve cortes. Prompt: replicate the refined blackened iron, aged bronze and luminous amber primary action button of the approved reference; front orthographic; no text or icon; quiet dark amber center; clipped corners; restrained gold rim, no neon; transparent exterior.
- Acción principal mantiene la nueva pieza en hover/pulsación/deshabilitado mediante tintes; sigue siendo el mismo botón real. Ajustes mantiene medallón circular en todos sus estados.
- Navegación vertical reserva 56 px por fila, icono 24 px y línea de texto independiente. La prueba nativa ahora exige que el icono no invada la etiqueta y que la etiqueta quede dentro de su botón.
- Oculta solamente la placa decorativa azul heredada que tapaba el material nuevo de identidad; preserva texto e interacción originales.

Implementado y pruebas de archivos/9-slice aprobadas. Pendiente captura Unity del nuevo candidato; no atribuirle el PASS ni las capturas del SHA anterior. No ART_APPROVED, publicación ni prueba física móvil.
