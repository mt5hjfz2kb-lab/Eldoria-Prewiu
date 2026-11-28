# Sustitución artística de Main

## Revisiones de referencia

| Fuente | Identidad |
| --- | --- |
| Main antes del registro de trabajo | d74f1a29d5472e9037d8534142768d4c13667ddd |
| Experimental donante | 6057c2604de84ae27f47ede44eafa294c96bcd24 |
| Árbol Unity de Main | db625919f50f2489d2a7cf3c107789b1392f8e86 |
| Referencia de recuperación | safety/main-before-valoria-art-d74f1a29 |

El archivo branch-diff.tsv registra las 664 diferencias de ruta, sin detección de renombrados. donor-assets.json identifica 174 candidatos en Assets por tipo de archivo, con las identidades Git de su contenido y de sus metadatos. Son candidatos para investigar. La presencia de una imagen en el donante no prueba que se muestre, que esté aprobada o que pueda sustituir al recurso activo de Main.

El aserradero aprobado es sawmill_i_hd.png, variante C, SHA-256 c3376fe92c5fc253093a80fade654a34f369d0ce7c5e6f3ed63e8da7155b4293. El instalador de Mundo experimental desactiva el dibujo de los renderizadores originales mediante forceRenderingOff. Mantener esos recursos residentes incumpliría el criterio de sustitución. La migración exige identificar el propietario y los consumidores de cada representación antes de retirarla. Los datos de posición de bosques y riberas del donante, su cámara y los cambios de distribución en Marchas, Entrenamiento y Mochila no cuentan con autorización de traslado.

## Estado de aceptación

Inventario en curso. No se ha modificado el contenido jugable. Pendientes el inventario observado por estado, la sustitución, las tres comparativas, el recorrido integrado I–V, el perfilado comparado, la revisión artística y las pruebas de plataforma. Ninguna calificación se ha extrapolado de ejecuciones históricas.

## Recuperación

Antes de fusionar, basta mantener esta PR en borrador para conservar el producto de Main. Tras una eventual integración, revertir mediante una PR el cambio artístico integrado, conservando los cambios posteriores de otros trabajos. Validar la compilación recuperada con una copia del guardado anterior y con una copia guardada por la candidata. La referencia de seguridad permite reconstruir el contenido original o comparar archivos por GUID. Los canales publicados requieren una autorización propia.

## Evidencias de la línea base

- [Contrato funcional por superficie](main-controls.md), [escena de ciudad y mapa](city-world.md), [correspondencias espaciales](city-world-assets.tsv) y [correspondencias de UI](ui-art.md).
- [Diez capturas nativas de referencia](main-baseline.jpg) y [procedencia, resolución y huella de cada original](main-baseline-images.json). Se obtuvieron del artefacto 14042200967 del run 43433023729. Los estados de prueba incluyen un informe sintético. El árbol Unity coincide con el de Main.
- [Fuentes y plataformas de la línea base](native-evidence.md) e [índice de informes](baseline-report-index.json). Entre sus límites están los estados preparados, los relojes acelerados y las pruebas de WebKit en escritorio.
- [Inventario espacial serializado](scene-contract-original.json). Desde la raíz del repositorio se reproduce con `python3 docs/valoria-main-art-migration/qa/scene_contract.py d74f1a29`. Registra nueve hotspots, dos cámaras y veintisiete colliders, con sus cadenas de transformaciones. La huella SHA-256 del JSON es bf584756cbac99a7e39223859df642bb82cd53c2e69aa9f8152bf6f0fd2081ff. El resultado es idéntico en la base de integración. Los objetos dinámicos y el coste de sus recursos se comprueban en ejecución.
- [Sonda externa de Play Mode y comparadores](qa/README.md). Preparados para observar controles y geografía en una copia de pruebas. La ejecución de la sonda sobre las tres revisiones está pendiente.

### Inventario de Main ejecutado el 28 de noviembre

Unity 6000.3.23f1 ejecutó el proyecto de `d74f1a29d5472e9037d8534142768d4c13667ddd` en el PC Windows. El [run 46189973255](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/46189973255) terminó con los cuatro grupos completos y limpió su checkout. El [artefacto 15282156169](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/46189973255/artifacts/15282156169) contiene los PNG y los inventarios de los mismos fotogramas. Su SHA-256 es `064983e8c01f5340b2519c8cb0ba0031a265a7b6c25e435ccaec59d01de5c672`.

| Grupo | Pares de imagen e inventario |
|---|---:|
| HUD, nombre, diálogos y ciudad | 15 |
| Destinos de navegación | 45 |
| Exploradores y Vigilantes | 9 |
| Mochila y correo | 20 |

En las 89 observaciones había una cámara, un Canvas y un raycaster habilitados en objetos activos. El [desglose por estado](main-native/states.tsv) conserva recuentos y hashes. Los anexos espaciales de esta ejecución corresponden a Bastión I, II y III. Los grupos de estados preparados y las distintas resoluciones generan muestras repetidas de algunas pantallas.

El [contrato observado de controles](main-ui-contract.json.gz) contiene las 19.324 observaciones de controles, con 3.298 registros distintos. Por cada estado registra geometría, navegación, eventos observables, Canvas, cámaras y raycasters. Los eventos con listeners creados en ejecución informan el número de suscripciones. Sus destinos se contrastan con el código y con las acciones reproducidas en el jugador. Para regenerar el contrato se usa [capture_ui_contract.py](qa/capture_ui_contract.py) con el ZIP original y la ruta del fichero de salida. El tamaño del Editor y la resolución de la cámara están registrados por separado.

En el [jugador WebGL de referencia](main-webgl/README.md) se conservaron el aserradero 1, la revisión 11 y 210 de madera al cerrar el juego y Chromium y abrir ambos de nuevo. La bolsa conservó el paquete de 120 tras consumir el de 60. Las 19 capturas se obtuvieron a 390 × 844 con reloj normal. El jugador archivado comparte el árbol de Unity del Main original. Las pruebas de dispositivo y las de la futura candidata se contabilizan por separado.

Las zonas táctiles del jugador se comprueban con eventos de entrada al tamaño real de la ventana. Las esquinas obtenidas en el Editor dependen del Canvas y de su cámara.
