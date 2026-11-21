# Valoria · ciudad modular y Región 1

La referencia de Bastión I es `09fac3a`, escena `Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity`. La base regional recuperada es `1461d40`. La validación nativa 44927295453 conservó capturas, rutas y cuadrícula antes de fallar en el empaquetado.

## Ciudad

Los sprites de cuartel, granero y cantera conservan sus GUID, escala de 288 píxeles por unidad y contacto inferior. El archivo CORONA_PROGRESSION_ASSET_PROVENANCE_20261121.json registra dimensiones, pivotes y huellas. Las tres láminas se derivaron de un atlas transparente original. Se separaron por componentes, se redimensionaron uniformemente y se registraron a la altura de los pies.

Los accesos de distrito aparecen al completar el edificio. Cada uno filtra las órdenes del consejo por su destino. El cuartel reúne entrenamiento, marcha y mejora. La vista Entregas muestra correo reclamable y paquetes obtenidos. Las ubicaciones de los tres controles originales se mantienen.

## Región y partida

El jugador combinado arranca en la escena modular y carga la escena geográfica con la regla CanEnterWorld. Cada escena recupera el mismo almacén experimental. Los controladores y las cámaras se descargan al cambiar de escena. La vista regional conserva los sistemas de selección, marcha y navegación del proyecto.

El canal alfa de NearFeatures codifica distancia firmada a la orilla. El shader usa ese canal para la transición de tierra húmeda a agua somera y conserva las máscaras de colisión y rutas. La superficie de pradera combina dos escalas de su textura original.

## Validación reproducible

El flujo valoria-region-modular-native.yml compara las rutas y la cuadrícula, ejecuta las capturas PlayMode, construye las dos escenas y entrega el jugador compilado. El trabajo mobile-journey ejecuta la progresión con entradas táctiles de Chromium y reloj de pruebas acelerado. Comprueba las idas al mundo y el regreso al mismo nivel y edificios.

La aceptación visual requiere revisar las imágenes del commit ensayado. La prueba de navegador simula un teléfono. El rendimiento y la estabilidad en Safari de un iPhone físico requieren una prueba en el dispositivo.

## Comprobaciones de procedencia

A 21 de noviembre, las carpetas Application, Domain, Infrastructure y Presentation de Eldoria/Scripts tienen los mismos identificadores de árbol Git que main (afa602aa850b2e9cb92c96e6a43a9435e9584160). También coinciden production-region.json, RegionOneBlenderWorld.glb, RegionOneResources.glb y el manifiesto geográfico en el repositorio. La reducción de modelos para WebGL opera sobre la copia temporal de compilación.

En el artefacto regional 14716859844 de la base recuperada, routes.txt mide 236178 bytes y grid.bin mide 1595160 bytes. Los SHA-256 coinciden antes y después de activar el arte modular: b9f2e5ffed15deabbc3d3f96632686591cd6eef73b1df3f05f7f55da7be98321 y ce18c0d100b909915f30854fe34b820ec00b75a4bb52908b4d1c2cb151f1ca2a respectivamente. La prueba regional del jugador modificado se ejecuta de nuevo sobre su propio commit.
