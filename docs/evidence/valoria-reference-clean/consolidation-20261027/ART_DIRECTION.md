# Consolidación visual final — registro de dirección

**CIERRE VERIFICADO:** tres intervenciones retenidas tras revisión independiente295; Brecha original conservada. Native40430426343 y gate general recuperado40432752930 SUCCESS. Evidencias y editables preservados en DELIVERY.json. Las etapas siguientes son el historial de decisiones y descartes.

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

## Estado al terminar286 (histórico)

Panorámica 286 retenida visualmente. Brecha conservada sin cambios por falta de mejora perceptible. Arquitectura y ribera pendientes de revisión de las siguientes capturas. No se ha promovido arte jugable por el mero éxito de compilación o render. No se certifica dispositivo físico ni rendimiento móvil.

Evidencias nativas y editables retenidas en los artefactos de sus ejecuciones; no son imágenes simuladas. La revisión final debe identificar bytes, cámaras, fuente integrada y pruebas funcionales antes del cierre.

## Revisión independiente final de candidatas

Ensayo288 / 40425009034: ribera RETAIN en H/V; contacto corregido y rocas portantes preservadas. Arquitectura original horneada: identidad y silueta RETAIN provisional en doce vistas, contacto con puente pendiente.

Ensayo289 / 40425753661, fuente b7ef0667dc407b4e9b1a6d93ba46d90d8d4b2076: /root/visual_review retiene combined-I y combined-II tras revisar las doce vistas. La escalinata conecta con el tablero sin la separación de césped anterior. La arquitectura I-II es reconocible y las panorámicas mantienen continuidad. La superposición parcial preexistente de cantera/acceso en regional vertical se documenta sin mover recursos funcionales.

La integración usa exclusivamente bytes del artefacto12699089321, ZIP SHA25648ebe5ce113ffdaaebdb592a29876731845a6bd6fc76384892ad36cbd3cd12b3. Mesh JSON582ded9a1c1b38fd148da669399dbc92cf73c099b51b99f6d799c14c45bde4ce. Fuente editable8121a7aacf1f23e6b3a3a0922c20405b031140b7ece5b579aa67d240fc27ba24. No se promueve la reducción de los Bastiones ni la variante de Brecha.

La revisión corresponde a candidatas auténticas; queda pendiente comprobar la integración y sus funciones antes de cerrar.

## Correcciones de la integración real

Integración290 / 40427017963: se guardaron los bakes exactos de I–II, los anclajes de escalinata, seis mallas reutilizadas del puente y las mallas de ribera/espuma. La escena cambia dos referencias de MeshFilter; Unity serializó también valores implícitos existentes de navegación/Engendro. No se cambió código de economía, comandos, distribución de recursos, guardados ni UI. El puente conserva el collider y la autoridad de navegación originales; solo reemplaza la superficie visible de madera del cruce central.

Validación291 / 40427512254: capturas de la escena de producción sin reemplazos manuales, 44 comprobaciones reales I–II/UI/guardados y 26 EditMode pasan. PlayMode: 64/65; falla la altura estratégica del nuevo Bastión compacto (0,06346 del viewport frente a mínimo 0,07). No se rebaja el contrato ni se modifica la prueba. Decisión: conservar tamaño cercano y anclaje; recuperar un 12 % de escala estratégica mediante transición continua entre zoom cercano y regional. La caja de selección sigue la misma escala visible, sin nuevos destinos ni alteración de sus coordenadas.

Revisión independiente291: /root/visual_review observa que las seis vistas conservan puente, posición y atmósfera, y que ambas vistas de ciudad retienen la ribera. Detecta pérdida de nitidez del Bastión respecto al candidato289. La diferencia técnica es concreta: el ensayo cargó el bake sin mipmaps, mientras la importación habilitó mipmaps. Corrección: restaurar bilinear sin mipmaps para los dos bakes, conservando los mismos PNG, UV, color y anclajes. No se disimula el fallo con una nueva imagen ni se lo aprueba por las pruebas funcionales.

Estas dos correcciones necesitan nueva captura y revisión antes del veredicto definitivo. Los resultados291 se conservan como evidencia rechazada de integración, no como certificación final.

Revisión de coste: el primer servicio de puente duplicaba la malla de terreno para retirar triángulos visibles del tablero. Se elimina esa copia mediante descarte localizado en las dos familias de material de madera ya instanciadas por LookDevelopment (cuatro instancias runtime verificadas). El shader conserva el resto del material y sus actualizaciones de niebla por zoom; el servicio restaura el valor al destruirse. La malla original, su collider y los demás cruces permanecen intactos. Se requiere confirmar idéntica apariencia en la última captura, sin afirmar una medición física de memoria móvil.

Incidencia técnica ajena al arte: gate40430426425 falló antes de Unity al restaurar el checkout antiguo de la raíz de Windows, con objetos Git ausentes y conflicto de directorio. CorrecciónCI0004a5a85aafd4cf9641050f0cb146ecab37977d: usar el checkout canónico ya verificado por las ejecuciones nativas, conservar intacta la copia antigua y resolver de forma coherente todas las rutas de proyecto, scripts, ensamblados y artefactos. Se mantienen los controles completos de fuente exacta, EditMode, PlayMode y buildWindows. Los controles de gobierno del workflow pasan; la ejecución recuperada40432752930 supera restauración exacta, EditMode, PlayMode y compilaciónWindows.

Validación final295 / 40430426343 desde3b14ade1b75cfdf7881771da16d36a18f0e088f1: SUCCESS, 26/26 EditMode, 65/65 PlayMode, 44 comprobaciones de progresión/UI/guardados, cero casos omitidos. Capturas reales sin overrides de producción: seis de mundo y dos de ciudad. Log del puente: terrain_mesh_copies=0, retained_collision=1, clipped_materials=4. Revisión de director: nitidez restaurada, escala regional legible, piedra conectada, atmósfera y ribera conservadas. Veredicto independiente final de /root/visual_review: RETAIN tras inspeccionar los ocho PNG; nitidez y puente recuperados, mejora estratégica y de entorno conservadas. El gate Windows recuperado se registra separadamente.

Cierre artístico y técnico: el verificador WORLD4X_ARTISTIC_PROCESS_EVIDENCE_PASS confirma hashes de fuentes editables y seis pares auténticos de cámara idéntica; también se verifican las dos parejas de ciudad. Este gate no aprueba arte automáticamente: el veredicto independiente295 es RETAIN. Tres intervenciones retenidas; Brecha original conservada. Sin publicación, APK, BastiónIII, cambios de economía/recursos o prueba física móvil.
