# Valoria · estudio de implantación I–X

Base de estudio: 1104ada09e9d2e71dd440586cb98b85c2dda1b0b. Estado: alternativas para comparación en cámara de Unity. El análisis del material anterior corresponde a la captura stage-5-fixture-390x844 del run 45206467235.

## Lectura espacial

En la captura de Whiteout Survival con el contador 00:01:17 el horno domina por luz y altura. La distancia a la primera línea de cabañas permite distinguir las puertas, aunque el encuadre corta el asentamiento. En la ciudad amurallada de Call of Dragons publicada por MobileMatters, la sede sobresale sobre piezas menores que comparten alineaciones. El espacio entre manzanas forma calles continuas. En la captura del Hall of Order de nivel 23 los indicadores de actividad ya ocupan una parte considerable del espacio sobre los edificios. Para Valoria, preservar el umbral, la silueta y espacio para el temporizador importa más que enmarcar cinco tejados simultáneamente.

Referencias: https://www.topuplive.com/news/whiteout-survival-guide-to-different-kind-of-buildings.html y https://mobilematters.gg/guides/gaming/call-of-dragons-city-hall-resource-upgrade-requirements. Son ejemplos de estados concretos. Las imágenes no permiten medir distancias absolutas ni reconstruir la progresión completa de esos juegos.

En el Bastión V previo, la vía ancha forma una horquilla en torno a un triángulo de césped. Su anchura compite con las fachadas. Cuartel y edificio auxiliar se leen como una sola tira. El molino y el castillo comparten eje vertical y al explorar apenas se descubre ciudad al este u oeste. El patio del Granero y el de Cantera tienen desplazamientos propios. El material DistrictStreets incorpora la distribución antigua. Sustituir ese trazado en su origen, retirar CasasDeOficio del patio militar y vincular cada acceso, patio y actividad a su parcela.

## Dos trazados para el mismo asentamiento

Unidades XY proyectadas en el suelo de la escena. El pie visual es el pivote del sprite. El umbral es la puerta o cargadero, ajustable por recurso mediante un desplazamiento local. La caja seleccionable seguirá la silueta interactiva al cambiar de etapa. Las coordenadas lógicas, los IDs y el progreso pertenecen al estado del juego.

| Uso | A · calle transversal y eje ceremonial | B · camino escalonado por terrazas | Huella máxima inicial (ancho × fondo) |
|---|---:|---:|---:|
| Bastión | (0, 3) | (0, 3) | 5 × 2,4 |
| Aserradero | (5, −1) | (−4, −1) | 3,8 × 1,7 |
| Cuartel | (−6, 0) | (6, 1) | 3,8 × 1,9 |
| Granero | (−3, −6) | (3, −5) | 3,8 × 2 |
| Cantera | (8, −6) | (−7, −7) | 4 × 2,1 |

En A, el camino ceremonial baja de la puerta hasta una bifurcación delante del Bastión. El oeste conduce al patio militar, el este al cargadero de madera. Una calle inferior comunica mercado, acarreo y frente de piedra sin atravesar el acceso ceremonial. El pozo ocupa una esquina del cruce. Las reservas son la prolongación occidental del mercado y el bancal elevado al nordeste. En B la vía asciende en dos quiebros desde el oeste extractivo hasta el Bastión, con un brazo oriental al Cuartel. El Granero marca el cambio entre acarreo y acceso urbano. Las dos reservas continúan los extremos de las terrazas.

Las franjas laterales de crecimiento tienen una parcela útil mínima de 5 × 3 y un corredor de paso mínimo de 1 unidad. En las etapas I–V quedan como prado o lindero rocoso y forestal, con los caminos productivos terminando en un destino activo. Calzadas de 1,05–1,35 unidades, accesos secundarios de 0,7–0,95 y sendas de faena de 0,55–0,75. Las huellas incluyen el contacto arquitectónico, los patios se reservan aparte. Comprobar la anchura real importada y el alero sobre el umbral adyacente.

## Comparación y decisión

Usar la misma escala, iluminación, estado V y relación vertical para ambos trazados. Capturar inicio, cruce, camino de abastecimiento y las cinco puertas. Recorrer en ambos sentidos con el HUD activo. La primera imagen debe mostrar la llegada al Bastión, un cruce y una salida reconocible del encuadre. Las siguientes deben conservar referencias que permitan regresar. Evaluar cuánto tarda en reaparecer una referencia al desplazar en cada dirección, si las cubiertas ocultan el siguiente giro y si los temporizadores tapan accesos. Verificar inicio I con Aserradero sin construir y con la obra en curso; II incorpora Cuartel, III Granero, IV consolida el abastecimiento, V incorpora la Cantera según estado canónico. La selección queda pendiente de observar las dos alternativas en el motor.
