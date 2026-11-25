# Componentes y materiales de Valoria

## Interfaz

| Pieza que se necesita | Componente o fuente de la rama | Regla de uso |
|---|---|---|
| Título y nombre de héroe | `CoronaFinish.HeadingFont` | Cormorant Garamond Semibold. Reservar un área independiente para nombres largos. |
| Cifra, botón, plazo o campo editable | `CoronaFinish.BodyFont` | Source Sans 3 Semibold. Situar las cifras en la parte limpia del material y comprobar el texto real más largo. |
| Texto principal, secundario y fecha | `CoronaFinish.Ivory`, `Muted` y `Brass` | El contraste debe sobrevivir a la textura. El estado debe poder reconocerse también por la etiqueta, la forma o el icono. |
| Retrato y recurso de mando | `RoyalGoldV41SourceArt.HeroPortrait` | Reutilizar la identidad aprobada. El área enmascarada del comandante puede recortar hombros, siempre que se conserve el rostro. |
| Iconos de materiales | `CoronaBrecha/icon_wood`, `icon_stone`, `icon_food` | Conservar el color local de madera, piedra y comida. La cifra o el estado expresan escasez. |
| Acción y navegación | `action_arcane` y medallones aprobados de Corona de la Brecha | El medallón ya tiene su cerco. Mantener el área táctil y la leyenda al dimensionar el símbolo. |
| Ilustración de una sala | `RoyalGoldV41SourceArt.Surface` y su ventana de escena | Aplicar el ajuste de aspecto dentro del recorte de la ilustración. A 390 × 844 verificar que se ven y se pueden tocar las acciones situadas arriba y abajo. |

## Terreno, arquitectura y movimiento

La ciudad comparte el material `DistrictGround` entre sectores. El eje de calle, los umbrales y los patios se localizan con `ValoriaUrbanPlan`. La tierra pisada y el serrín se integran en ese material. Los edificios y los personajes conservan su propio pie y su ordenación de profundidad. En el mundo, `RegionPaint` utiliza la cobertura forestal, la pendiente y la distancia a la orilla para mezclar los materiales en el terreno existente.

Antes de aprobar una sustitución, comparar con el mismo guardado, cámara y nivel. Revisar la silueta desde lejos, el umbral de cerca y un personaje que cruce delante y detrás. Para un control animado, revisar juntos el primer fotograma, la transición y el estado terminado. Un control que sigue respondiendo al toque puede estar oculto por el dibujo de otro elemento.

Las capturas del Editor, Play Mode, Chromium, WebKit y dispositivo físico se identifican por separado. Las medidas del proceso GPU de Chromium y la memoria estimada de texturas en el Editor corresponden a entornos diferentes.
