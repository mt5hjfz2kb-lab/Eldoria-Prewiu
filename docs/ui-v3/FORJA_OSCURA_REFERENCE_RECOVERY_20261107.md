# Forja Oscura — referencia recuperada y candidato de material

Fecha: 2026-11-07. PR173, misma misión y rama.

## Referencia original verificada

Archivo original: A19C8134-2F98-4AE8-80EE-B75D97F91958.jpeg (1536×864).
SHA-256: c9085f49ade49ff0836e9547f402afe8ad85e87032db8d84659b237f777a65ae.
Referencia conservada: https://chatgpt.com/library?file_id=libfile_52cf2d895f008191a6d34e6a2717b8b8&account_id=personal
Los tres adjuntos JPEG recuperados tienen exactamente ese hash. Imagen inspeccionada visualmente; no sustituir por la variante «Interfaz épica» (hash distinto).

## Diagnóstico y alternativa

El panel procedural actual se inspeccionó a tamaño fuente: líneas uniformes, remaches mínimos y poco volumen, frente al acero negro biselado y bronce gastado de la referencia. Se creó una pieza raster original con generación de imágenes y ambas imágenes como referencia/objetivo. El resultado tiene borde metálico esculpido y centro oscuro libre para texto real. No contiene cifras, héroes, ciudad ni mecánicas ficticias.

Candidato fuente: tools/forja-oscura/art-source/panel-dark-painted-candidate.png. NO importado a Unity todavía. La prueba nativa 42404744384 ejecuta el kit anterior en 4d838a69f7db40232c9c9516e9a0ec109f009e2f. No atribuirle el nuevo marco.

Antes de promover el candidato: adaptar bordes de importación a sus esquinas reales, verificar estiramiento y legibilidad de cabecera/contexto en cuatro resoluciones, comparar capturas reales. No alterar verificadores de paridad para aparentar que procede del generador procedural: si se adopta, declarar origen raster y hash explícitos.

## Continuidad

Rama reconciliada con main eb1cc01725567a09ed49fb542a585885859067ad mediante 34a8b05f505be561a012461d6589b94937c06a68. Conserva las correcciones de temporizadores y entrenamiento de la publicación reciente. Sin merge de PR173 ni publicación de interfaz.

## Continuación 498cad9fd409: candidato importable, sin aprobación

Se conserva el raster exacto `31a073bc88213505795b37445983262b80b132d6b2249144abc26a385bef5447` como `Surfaces/panel_dark_painted.png`, sin resampling ni generación repetida. El original procedural permanece intacto. La skin de prueba selecciona la pieza pintada para cabecera, cartelas y base de navegación; el resto de controles y eventos conservan sus fuentes.

Bordes fijos de 144 px y 600 PPU producen esquinas de 24 unidades de UI con Canvas reference PPU 100: dejan 24 unidades centrales incluso en el panel mínimo de 72. Esto evita estirar el metal de las esquinas. Se valida CRC, igualdad de bytes con el original, SHA-256 fijado, GUID único y acuerdo entre importador/meta. Las nueve superficies procedurales anteriores siguen pasando su comparación exacta sin excepciones. Estas pruebas NO demuestran el acabado en Unity: la revisión visual nativa sigue pendiente.

La captura anterior omitía el enfoque canónico porque su callback `CapturePrepared` sustituye también la preparación de cámara. Se reproduce ahora `FindTarget → Focus → Render` del propio juego para ambas imágenes antes de sincronizar la skin. No se cambia cámara del producto ni se elimina la comprobación de solapamientos. La prueba del kit previo se conserva en run42408083695, source ef61b5082ddcc24e8dff4b3b35a21522ca7d78da.
