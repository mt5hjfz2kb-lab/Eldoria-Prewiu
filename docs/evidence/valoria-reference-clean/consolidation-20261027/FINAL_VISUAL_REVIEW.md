# Eldoria — consolidación de identidad y entorno

Estado: **RETAIN — integración295 revisada y conservada**. Fuente de juego `3b14ade1b75cfdf7881771da16d36a18f0e088f1`, ejecución nativa `40430426343`. Este veredicto artístico se basa en capturas auténticas y es independiente del resultado de las pruebas.

## Criterio y referencias

El objetivo de cierre es que Valoria se reconozca como la misma ciudad al entrar y al recorrer el mundo, con una ribera menos repetitiva y una panorámica menos encerrada. Se preservan la composición aprobada, una sola geografía, la progresión I–II y los destinos funcionales. No se persigue una puntuación gráfica global.

Referencias originales verificadas: JPEG de Valoria `86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d`; tríptico de Región 1 `51df0c0b8b34619fbe3052e5f453587dcee17c50968cde742d3b3bb5cb384a35`. Los cuatro huecos/parcelas del JPEG están expresamente excluidos por la orden posterior del propietario. AF/AG y los recetarios de Blender/Unity orientan el método, no autorizan edificios nuevos.

## Observación, decisión y corrección

**Valoria.** El mapa usaba otro castillo: torres redondas y cubiertas puntiagudas frente a torres cuadradas, piedra clara y tejados azules de la ciudad real. Se comparó una reducción geométrica de la fuente existente con representaciones estratégicas horneadas en Unity. Se rechazó la primera por degradación de fachada y cubierta. Se retuvieron dos bakes separados de los Bastiones originales I–II, con la escalinata como anclaje. El mundo reutiliza y adapta el puente de piedra de ciudad; mantiene el tablero transitable y los colliders anteriores. No se utiliza el antiguo Bastión generado por IA. La presencia de Valoria en panorámica responde a la orden actual explícita de reconocerla en las tres cámaras, posterior al concepto que ocultaba ciudades pequeñas en esa vista. Cerca se preserva el tamaño revisado; en la transición a regional se recupera un 12 % de escala para cumplir el contrato existente de legibilidad. La selección sigue la silueta visible sin mover el destino.

**Panorámica.** La máscara periférica cerraba el terreno en una figura ovalada y hacía destacar una banda de agua oscura. Se retiene la transición amplia e irregular sobre la continuación ya existente y un tratamiento localizado del borde costero. Cerca y regional siguen despejados. No se añade tierra, océano, región ni emplazamiento jugable. Las imágenes muestran más extensión y continuidad, sin cambiar la cámara ni los límites lógicos.

**Ribera.** Se esculpen las 93 componentes existentes: coronas más bajas, variación de anchura y planos angulosos, conservando el pie sumergido y las piedras portantes de árboles. La malla pasa de 21.608 a 9.722 triángulos. La espuma se ajusta a la intersección real de la nueva piedra con el plano de agua. Se rechazó el primer rebajado excesivo y el contorno de espuma heredado. Las ruinas y los apoyos existentes se inspeccionan sin introducir una nueva distribución de elementos. Se conservan río, puente de ciudad, luminosidad y composición.

**Brecha.** Se prueba una transición de material menos regular. En regional sigue leyéndose como una mancha circular y el beneficio no justifica cambiar la base. Veredicto: REJECT del ensayo; conservar material, relieve, profundidad y centro violeta anteriores. Esta decisión deja un límite visual conocido, no una supuesta corrección terminada.

## Revisión independiente y evidencia

Revisor independiente: `/root/visual_review`, distinto del autor de las modificaciones. Retuvo Boundary286 en seis vistas, Bank288 en ciudad H/V y Combined289 I–II en doce vistas. En la primera integración291 detectó correctamente la pérdida de nitidez introducida por mipmaps; esa integración no se aceptó como cierre. Se restauran los mismos ajustes bilineales sin mipmaps que tenían las texturas revisadas.

Las parejas de mundo usan 1280×720 y 900×1600, tres distancias. Ciudad usa 1448×1086 y 900×1600. Cada PNG conserva sus bytes originales, hash y JSON de cámara de Unity PlayMode. La igualdad de posición, rotación, zoom, objetivo y aspecto se comprueba, no se supone. Las imágenes de Bastión II del ensayo289 están identificadas como candidatas revisadas anteriores al ajuste estratégico; la progresión real I–II se verifica además con el probe funcional existente.

La fuente `.blend` conserva originales congelados y derivados del ensayo. Las mallas reducidas de Bastión y puerta dentro de ese archivo NO son assets aprobados. `SOURCE_ROLES.json` distingue expresamente esas piezas de los bakes originales, puente y ribera utilizados. La entrada real de la autoría bpy fue la exportación exacta de mallas integradas en Unity; `BastionRoofJunctions.blend` es la fuente editable anterior, no una supuesta entrada directa de ese script.

Revisión independiente de código `3b14ade`: sin bloqueos identificados. El puente no duplica ya el terreno. El revisor comprobó el orden LookDevelopment → ReferenceNodes → StoneBridge, la conservación de las instancias de material gestionadas por niebla/zoom, el descarte equivalente en ForwardLit y ShadowCaster, la exclusión de los otros cruces y la restauración al destruir el componente. La comprobación visual nativa sigue siendo independiente de esta inspección estática.

## Límites y reversión

Permanece el solapamiento parcial de cantera y acceso en regional vertical; no se desplazan recursos para disimularlo. Persisten simplificaciones del río distante, cierta repetición vegetal y un corte costero en panorámica vertical. Se conserva la Brecha anterior. No se certifica un móvil físico ni un nivel artístico global 8/10.

La integración mantiene las fuentes y materiales originales. Para revertir selectivamente: restaurar las dos referencias de malla de ribera/espuma de la escena; desactivar la instalación de `RegionOneStoneBridge`; restaurar la representación anterior en `RegionOneReferenceNodes`; restaurar únicamente el bloque de atmósfera periférica del shader. Los commits iniciales y el registro `ART_DIRECTION.md` documentan el antes. No se requiere borrar archivos, resetear guardados ni reescribir Git.

Sin APK nueva, publicación web, gasto, edificios inventados, barrios, campamentos, cuatro parcelas ni trabajo en Bastión III.


## Veredicto final independiente295

`/root/visual_review` examinó los seis PNG de mundo y los dos de ciudad. RETAIN: nitidez recuperada frente a291; arquitectura cuadrada reconocible como la de ciudad; conexión escalinata–puente y composición cercana H/V equivalentes a289. El aumento estratégico mejora el reconocimiento sin una invasión nueva importante de fachada por la cantera. El puente reproduce289, sin tablero antiguo ni sombra residual visibles, y los demás cruces siguen presentes. La panorámica conserva la mejora frente al óvalo original, aunque el borde oriental sigue siendo geométrico. La ribera reproduce288, con espuma en contacto y apoyos vegetales conservados. No se detecta una regresión visual que impida conservar la integración. La Brecha original permanece por rechazo razonado de su ensayo.

Pruebas nativas separadas del juicio artístico: 26/26 EditMode, 65/65 PlayMode y 44 comprobaciones del recorrido real I–II, interfaz V3, marchas, recursos, combate, retorno y guardados; cero tests omitidos. `integration-result.json` identifica el mismo commit y confirma los hashes de guardados canónicos preservados. La captura usa la escena y el runtime de producción, sin sustituciones visuales manuales. No se ha probado un teléfono físico.

Artefacto nativo final `12702388496`, ZIP202.757.576bytes, SHA256 `20b07ffba007182c8107fd20f0e2d5f219d9b7b15e3f4e8f70155c1b62b76ce7`. Las capturas y fuentes se conservan con sus hashes en el paquete de evidencia, no se retocan para esta entrega.

El control general recuperado40432752930 también supera las pruebas EditMode/PlayMode y la compilaciónWindows sobre0004a5a85aafd4cf9641050f0cb146ecab37977d, cuyo runtime es idéntico a3b14ade. Se reparó la selección de la copia de trabajo sin rebajar pruebas ni borrar la copia anterior. La comprobación de proceso gráfico también pasa; se mantiene separada del dictamen artístico.

[Paquete verificable: comparativas originales, galería index.html, fuentes Blender y pruebas](https://chatgpt.com/library?file_id=libfile_30faaa5b476c8191b5f41b8d1bcdf551&account_id=personal), 216.665.528 bytes. Identidad del archivo y materialización en DELIVERY.json; inventario completo en EVIDENCE_MANIFEST.json.
