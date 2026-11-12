# Salón de héroes — colisión real de subtítulo, 12/11/2026

## Observación verificable

La captura de Unity `real-unity-heroes-1600x900.png`, ejecución
[43142184165](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43142184165),
fuente exacta `af658a6cc1a02d45d7efc9c57dafef24e8512564`, presenta la segunda
línea del texto introductorio parcialmente detrás de los cuatro filtros.
El control anterior `DesktopHallIntroductionUnobstructed` informaba
éxito porque medía posición/alto del RectTransform, **no los glifos
visibles**. Es una discrepancia perceptible, no un fallo de compilar.

## Técnica nueva (sólo PR #173)

Se abandona el ajuste repetitivo de tamaño de letra en una caja con
alineación vertical centrada. El mismo objeto Text del controlador
canónico se coloca dentro de una franja física delimitada por el borde
inferior del título y el borde superior de las pestañas, con alineación
superior y medida de alto tras rasterizado; se restauran tamaño,
alineación, interlineado y posición al cerrar el destino. No se modifica
la narrativa, el layout de las tarjetas, las banderas azules originales,
datos de héroes, sus botones o el Canvas independiente.

La prueba nativa ahora realiza una **comparación GPU con el texto
real activado y desactivado** en 1600x900, mide píxeles visibles de la
introducción y rechaza cualquier aparición significativa en una banda
de 14 px junto a las pestañas. Un `PASS` de geometría o CI fuente
sin esa prueba no demuestra el arreglo.

## Estado y aceptación

- **Fuente implementada:** sí, en la misma rama PR173.
- **Captura Unity de esta revisión:** pendiente hasta turno legítimo.
- **A/B contra referencia original:** pendiente con nueva captura y
  misma orientación/estado.
- **ART_APPROVED / aprobación para main / pruebas iPhone:** no.

Se preservan el sobre ilustrado sin recorte, Lyra original y el conector
optativo a la bolsa real PR187 sin reclamar integración funcional combinada.
