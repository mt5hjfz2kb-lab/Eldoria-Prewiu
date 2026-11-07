# Estado vigente: cinco iconos en una sola fila, acabado general pendiente

Petición explícita del propietario: ningún acceso inferior apilado; retirar letras antes que usar dos filas.
Candidato: `cb8c659ee78c8e40b0ecd8c6dc298db5b9f723f7`.
Unity nativo [42417772857](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42417772857): SUCCESS.
CI de fuentes 42417773032: SUCCESS; Unity hospedado omitido por licencia, validación real en Windows.
Artefacto 13592062245: doce PNG descargados y SHA256 comprobados.

- Cinco botones originales en una fila en 1600×900, 900×1600, 844×390 y 390×844. Sin solapamiento horizontal ni segunda fila.
- Rótulos ocultos reversiblemente; iconos de aproximadamente 44 px, botones de 56 px de altura, funciones y orden conservados.
- Prueba nativa exige cinco iconos contenidos en sus botones, rótulos ocultos y una sola fila.
- Bastión, inventario y misiones mediante puntero, cierre, construcción y guardado aislado PASS. Ejército y Mundo bloqueados no ejercitados.
- Datos originales restaurados después de retirar la skin. Sin prueba física móvil.
- Inspección visual de Bastión 390×844 y pantalla 844×390 confirma una fila sin letras. Ficha y cabecera siguen por debajo de la riqueza visual buscada. NO ART_APPROVED, no merge ni publicación APK/WebGL.

Siguiente trabajo artístico: mejorar composición de ficha y jerarquía de cabecera conservando esta navegación. Las secciones siguientes son evidencia histórica y no describen la navegación vigente.

---

# Estado vigente: recuperación certificada, acabado global pendiente

Candidato nativo: `7f7e7acc5068ff5c66ab5c83dc69e99afaabc54c`.
Unity 6000.3.23f1: [42416965218](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42416965218), SUCCESS.
CI de fuentes del mismo candidato: 42416965283, SUCCESS (la compilación Unity hospedada sin licencia se omite; la certificación real es Windows).
[Doce PNG y reporte originales](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42416965218/artifacts/13592151290). Sus doce SHA256 fueron comprobados tras descargar.

## Corregido y comprobado
- El test buscaba un hijo llamado `Forja / navigation icon` usando Transform.Find, que interpreta la barra como ruta. Ahora compara nombres literales de hijos directos, sin eliminar la exigencia de icono ni separación del texto.
- Navegación: margen de rótulos y encuadre de iconos en las cuatro orientaciones, no solo en móvil vertical. El test exige ausencia de solapamiento también en horizontal.
- Miniatura real: encuadre ortográfico a partir de los límites proyectados del modelo, con 8% de margen. Conserva mallas, luz, ángulo y ciudad; la configuración anterior se restaura al quitar la skin, comprobado en Unity.
- Caché Windows: se actualizó el SHA esperado solo después del inventario real. No se eliminó el guard ni se canceló Unity.
- Ocho capturas A/B, cuatro del Bastión; tres rutas desbloqueadas mediante puntero, cierre, construcción y guardado desechable PASS. Ejército y Mundo bloqueados NO se contabilizan como ejercitados.
- Controles de navegación de al menos 56,55 px en los tamaños probados; controles contextuales de 44 px con tolerancia flotante. Datos de juego idénticos al aplicar y retirar la skin.

## Juicio visual y límites
Se revisaron las cuatro resoluciones y los casos de Bastión móvil. Las miniaturas ganan presencia sin recorte visible y la navegación conserva sus etiquetas dentro de los botones. El resultado todavía no acredita la riqueza compositiva de la referencia original: ficha de edificio y jerarquía de cabecera necesitan revisión artística adicional.
No ART_APPROVED, no merge, no APK/WebGL. Consejo/estados avanzados y plataformas siguen pendientes. La regresión general anterior de cc84c9be se conserva como evidencia anterior, no se atribuye al nuevo SHA.

Siguiente paso: composición de ficha/cabecera sobre estas capturas, después estados avanzados y validación de plataforma. Preservar PR173 y estas pruebas, no repetir generación del kit.

---

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
