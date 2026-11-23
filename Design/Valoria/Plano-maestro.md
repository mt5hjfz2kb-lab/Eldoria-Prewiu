# Plano maestro de Valoria

La planta activa es B, el camino escalonado por terrazas. El cruce del pozo enlaza el recinto del Bastión, el cargadero del Aserradero y el patio militar. La calle de abastos desciende hasta el cruce de mercancías, donde se separan el acceso al Granero y el acarreo de la Cantera. Los dos cruces sirven para orientarse al desplazar la cámara.

## Parcelas

Unidades de la escena en el plano XY proyectado. Cada puerta figura en coordenadas absolutas para permitir comprobaciones de obra. El código la obtiene por desplazamiento desde el pie de su parcela.

| Distrito e identificador | Pie de la ilustración | Puerta o cargadero | Huella arquitectónica | Ocupación |
|---|---|---|---|---|
| Monumental · bastion | (0; 3) | (0; 2,75) | 5 × 2,4 | I–V, con el mismo umbral |
| Productivo · sawmill | (−4; −1) | (−4,45; −1,20) | 3,8 × 1,7 | I, parcela disponible, obra o edificio terminado |
| Militar · barracks | (6; 1) | (5,65; 0,80) | 3,8 × 1,9 | II y Cuartel construido |
| Abastecimiento · granary | (3; −5) | (3,35; −5,20) | 3,8 × 2 | III y Granero construido. Puesto de venta desde IV |
| Extractivo · stoneworks | (−7; −7) | (−6,45; −7,20) | 4 × 2,1 | V y Cantera construida |

El pie registra la imagen en el suelo y determina el orden de profundidad. La huella reserva el espacio físico por debajo de aleros y cubiertas. La selección comprueba los límites del dibujo activo, salvo en el Aserradero, donde la parcela conserva el mismo blanco táctil durante sus tres estados. El identificador enlaza con la construcción, los plazos y la partida guardada.

## Calles, ocupación y cámara

La puerta del Bastión tiene un antepatio de piedra centrado 0,46 unidades por debajo de su umbral. El patio usa el mismo suelo continuo que la calle. El cruce del pozo está en (2,2; −1,7) y el de carga en (−1; −6,5). Los accesos del Bastión y el Cuartel miden 1,32 unidades de ancho. La vía de abastos mide 1,15 y los accesos de faena 0,92. Los caminos llegan de frente a la puerta en su último tramo. El acopio, el patio de armas, el cultivo y el frente de roca tienen su propia ocupación lateral.

La cámara abre en (0; 1,9). En 390 × 844 su semialtura es 6,60 unidades. El centro puede recorrer X de −13 a 13 e Y de −10 a 9. Desde el pozo se puede buscar el Aserradero al oeste o subir hacia el Cuartel al nordeste. La Cantera se descubre siguiendo la calle inferior hacia el oeste.

## Reservas VI–X

El bancal occidental ocupa X −14 a −9 e Y −8 a −5. La reserva nordeste ocupa X 8 a 13 e Y 3 a 6. Cada una admite una parcela de 5 × 3. El límite oriental conecta con la prolongación militar y el occidental con el distrito extractivo. Son puntos de crecimiento utilizables en etapas futuras, con la asignación de edificios pendiente del diseño de VI–X. Se conservan libres de huellas funcionales I–V.

Las posiciones se mantienen en ValoriaUrbanPlan.City. Los accesorios se registran con su pie, escala y distrito en Environment. Las vías y los itinerarios de los habitantes usan las mismas puertas y cruces. Los recursos gráficos conservan su registro de apoyo en los parámetros de importación de Unity.
