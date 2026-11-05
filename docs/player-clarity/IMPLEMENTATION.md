# Eldoria — Player Clarity / 05-11-2026

Workstream-ID: eldoria-first-login-map-signage-respawn-20261105  
Issue: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/161

## Alcance real implementado de este primer commit

- Tres pictogramas vectoriales originales con silueta legible: troncos con
  vetas/anillos, piedra facetada y espigas doradas. Varias tintas por icono;
  se dibujan en el tamaño real de los slots del HUD, sin paquetes, coste,
  descargas ni licencias externas.
- Un adaptador runtime de presentación, aislado, que reemplaza **solamente**
  el dibujo de `Madera icon`, `Piedra icon` y `Comida icon` del HUD
  canónico cuando existe. Mantiene los tres RectTransform, textos, wallet,
  lógica de desbloqueo de Comida y acciones originales. No ocupa la escena,
  no crea Canvas, no atrapa raycasts y no modifica archivos de PR141.
- Pruebas EditMode para generar la geometría en 24×24, comprobar triángulos,
  colores y coordenadas válidas.

**Estado:** implementación de origen lista para comprobación en Unity.
Sin ejecución de Unity ni capturas A/B nuevas de esta rama; NO ART_APPROVED,
NO MERGED, NO PUBLICADO. Una compilación no certifica la belleza ni la
legibilidad de estos iconos en móvil. Los bocetos no son capturas de Unity.

## Arquitectura verificada antes de editar

`RoyalIconGraphic` actual es geometría plana monocolor; `RoyalGoldV2Hud`
coloca los iconos auténticos `Madera icon`, `Piedra icon`,
`Comida icon` dentro de ResourceBar. El nuevo componente no cambia
`RoyalGoldV2Hud` ni `RoyalHUD`.

`LocalGateway` ya mantiene UTC y no duplica la población; los intervalos
actuales se configuran desde `RegionOneResourcePlacement` en 30 minutos y
`RegionOneCreaturePopulation` en 15. `WorldCreatureLocation.Level` sí
existe y está guardado; `WorldResourceLocation` no tiene un Level autoritativo.
El `SlicePresenter` anterior genera badges de tres edificios, pero el
Canvas heredado se deshabilita en la escena integrada. El HUD real es
`ValoriaIntegratedWorld.Hud`.

## Siguiente bloque, sujeto al relevo seguro de economía PR141

- Nick en PlayerState schema-1 con comando de gateway validado e idempotente;
  modal del primer arranque antes de narrativa y texto de nick junto al escudo.
- Niveles de recursos que correspondan a una magnitud **real** de producción
  o capacidad, persistidos y coherentes con Economía V2 (no adornos falsos).
- Etiquetas de ciudad/mundo en cámara Unity, pequeñas y visibles sin invadir
  UI, niveles reales de criaturas/caza y recursos, zoom/oclusiones.
- Configuración de 300 segundos en reposición de nodos y fauna renovable,
  migración de cooldowns existentes, dispersión por sectores y QA de
  colisiones/rutas y guardados.
- EditMode/PlayMode, recorrido I–V, A/B reales en 4 tamaños; sin publicar
  ni fusionar por simple éxito sintáctico.

## Advertencias de integración

No reintroducir niveles VI–X, no resucitar enemigos únicos de misión, no
mover la Brecha ni los nodos narrativos, no cambiar las coordenadas canónicas
de Valoria, no reproducir arte no aprobado. La selección del siguiente sitio
no debe hacerse solo en el cliente si un día el mundo es compartido.
