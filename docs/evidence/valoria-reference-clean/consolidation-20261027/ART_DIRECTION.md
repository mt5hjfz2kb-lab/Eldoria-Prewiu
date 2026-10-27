# Consolidación visual final — registro de dirección

Base jugable: main 069da49d1403eddf5bf55a211dbcd47710d3956d. Encargo: coherencia Valoria ciudad/mundo, continuidad panorámica, ribera localizada y Brecha solo si hay ganancia proporcionada. Sin APK, Bastión III, cambios de economía, recursos funcionales, guardados o interfaz. Presupuesto 0 €.

## Observación y fuentes

Inspección nativa 40420806811 desde 32b61c89c04bf0f1f8ad450e81cff2b2e1a81709: ciudad I–II horizontal/vertical, geometría y materiales reales. La ciudad usa RoofJunction226/Bastion-0.asset y Bastion-1.asset, con torres cuadradas. El mundo sustituye sus clones históricos por la celda 0 de RegionOneReferenceNodes, otro castillo con torres cilíndricas. Se excluye absolutamente HeroBastionGenerated/Valoria_HeroBastion_v1.glb.

Referencias contrastadas: VALORIA_APPROVED_VISUAL_REFERENCE.jpg, originales de tres cámaras de Región 1, receta AF/AG, playbooks Blender y convergencia visual, estado integrado tras la eliminación de 3251 copas artificiales. Ninguna parcela decorativa, barrio o campamento añadido.

No hay BlenderMCP expuesto en la sesión. Se usa Blender 5.2.2 LTS instalado en el Windows reservado, mediante bpy sobre copias de las mallas exactas; no hay generador de assets ni servicio de pago. Los originales, UV y materiales permanecen intactos. El archivo editable conserva copias congeladas además de las candidatas.

## Decisiones, correcciones y veredictos

- Survey 282: Unity produjo correctamente capturas e inventario; el paso posterior intentó guardar un JSON de 205,58 MB en Git. GitHub lo rechazó; prueba recuperada del artefacto 12696933523. Se excluyeron estas pruebas de la persistencia automática y de los artefactos históricos redundantes.
- Trial 283: error de compilación de instrumentación por colisión de namespace Application; corregido explícitamente a UnityEngine.Application.
- Trial 284 / 40422550638: Blender reduce I de 142830 a 17107 triángulos y II de 162354 a 19445. Las copias no entraban en la capa de la cámara. Imágenes rechazadas como evaluación de arquitectura. Unity completó; el workflow marcó fallo por prefijo de marcador, corregido después.
- Trial 285 / 40423028360: render completo. Se descubre enterramiento por altura heredada anterior a la geografía canónica; se corrige con SampleHeight actual. Ribera excesivamente rebajada, rechazada. Primera transición periférica expone demasiado el agua, rechazada. Brecha: variación de máscara insuficiente.
- Trial 286 / 40423711483: render completo desde 4182eb405988d3427da47314574d02dcb9c7ceab. Puente de piedra reutilizado de ciudad y adaptado al cruce existente: x 1017,3–1025,3; z 1–22; altura del tablero 3,2435–3,2907. Sin cambiar collider ni navegación.
- Revisión independiente /root/visual_review: Boundary286 RETAIN en seis vistas, continuidad y menor banda oscura sin regresiones cercana/regional. Ribera REJECT por espuma de contornos antiguos y necesidad de comprobar apoyos de pinos. Brecha REJECT: sigue leyendo como mancha circular, ganancia insuficiente. Arquitectura reducida REJECT: pérdida de superficies de fachada/tejado. La reducción no se promociona.
- Cambio sustancial de arquitectura: bake estratégico transparente auténtico en Unity desde las mallas originales aprobadas, con I y II separados. Preserva silueta y materiales y evita dibujar cientos de miles de vértices en el mapa. El puente sigue siendo geometría 3D reutilizada. Ninguna imagen generada por IA.
- Corrección de ribera: transformar correctamente el plano de agua al espacio local de la malla, conservar piedras portantes próximas a troncos y reconstruir la espuma en la intersección física agua/piedra candidata. No añadir una colección de props.
- Trial 287: duplicación de campo zoom al ampliar metadatos; error exclusivo de instrumentación, corregido en 288 antes de seguir.

## Estado de aprobación

Panorámica 286 retenida visualmente. Brecha conservada sin cambios por falta de mejora perceptible. Arquitectura y ribera pendientes de revisión de las siguientes capturas. No se ha promovido arte jugable por el mero éxito de compilación o render. No se certifica dispositivo físico ni rendimiento móvil.

Evidencias nativas y editables retenidas en los artefactos de sus ejecuciones; no son imágenes simuladas. La revisión final debe identificar bytes, cámaras, fuente integrada y pruebas funcionales antes del cierre.
