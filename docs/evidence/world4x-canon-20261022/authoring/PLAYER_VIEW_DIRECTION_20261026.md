# Corrección de la cámara del jugador — 26-10-2026

Dirección: codex-world4x-player-art-22db827a. Encargo vigente del propietario: corregir el mundo, priorizar la imagen cercana, nodos al 85% de la ciudad, integrar después de demostrar calidad y jugabilidad. Presupuesto 0 €.

## Evidencia y diagnóstico

Se han visto los tres PNG originales canónicos y las capturas genuinas del candidato Unity 40253785923, en ambas orientaciones. Evaluación inicial aproximada: cercana 6,5; regional 5,8; panorámica 5,2. No es una certificación. La referencia muestra claros verdes y dorados conectados, sombras legibles, amenazas grandes, puentes cortos y rotura geológica irregular. El candidato presenta dominante amarilla, contraste entre parche central y continuación, recursos pequeños, puente excesivamente largo y paredes de Brecha regulares. Un build verde no resuelve estas diferencias.

Reutilización comprobada con tools/asset-vault/find-reuse.py sobre los índices de main: 14 candidatos. Se conserva la fuente real de Región 1 y sus recursos aprobados; no se añaden módulos arquitectónicos de Valoria ni primitivas decorativas ajenas. Fuente inspeccionada mediante BlenderMCP en 40287703090, SHA-256 fd3063a792eedf77d6908fe9f5d894b47514717720a5d03ee1ac4e08f0690cce. La inspección registra límites, vértices y distribución de materiales reales.

## Decisiones de esta iteración

1. Medir anchura horizontal de los vértices importados: ciudad 24, nodos 20,4. Ajustar el volumen seleccionable existente. Criaturas cercanas: jabalí 10,8 y engendro 13,2, conservando el comportamiento regional.
2. Sustituir la amplificación roja del césped por claros verdes/dorados a escala del mundo, manteniendo grano y caminos del pigmento fuente. Unificar el tratamiento a ambos lados de los límites de material.
3. Dar más lectura a las sombras mediante relleno acotado; reducir las tres luces violetas que se leen como focos.
4. Esculpir escalones y fracturas en los vértices existentes de las paredes, conservando núcleo AF/AG, imágenes originales, topología, reservas y máscaras jugables. Reproyectar las superficies geológicas sobre el terreno y regenerar los datos de altura correspondientes. Eliminar las líneas sinusoidales del acabado.

La ejecución de estas operaciones no constituye aceptación visual. La longitud del puente, el borde de cantera, diversidad de copas y continuidad panorámica requieren revisión y corrección posterior según las capturas. No afirmar que estos defectos estén ya resueltos.

## Criterio de cierre

Revisar capturas reales cercanas, regionales y panorámicas H/V y detalles de Brecha, nodos y criaturas. Mantener pruebas de soporte, selección, rutas, población, persistencia e integración I–II. Comparar con referencia original; valorar por composición y legibilidad. No integrar un candidato defectuoso para alcanzar una fecha, no rebajar tests y no declarar rendimiento físico iPhone sin medición.

## Medición del puente

Inspección de triángulos del GLB real: agua entre z=-17 y -24; terreno a cota 2,98 en -8 y 3,19 en -26. El tablero actual ocupa -35,92 a -1,92. Se corrige a 18 m entre -26 y -8 con apoyo medido en las dos orillas; se conserva el eje x=3,3 y los postes, acortando sus posiciones. La aproximación terrestre deja de ser una rampa de madera. Solo los vértices del propio tablero quedan exentos de la preservación del núcleo; los 96 testigos de pradera y caminos siguen intactos.

El primer intento Unity 40289495400 no llegó al editor: el mensaje de commit no activó la recuperación del paquete base requerida por el workflow. Se corrige el disparo usando el prefijo existente `region-imported(world4x):`; ese intento no cuenta como compilación ni prueba.

La comprobación posterior de los dos bordes (además del eje) detectó agua bajo la esquina x=7,3/z=-26. El gate de apoyo rechazó correctamente ese primer intento. Se conserva el extremo norte -8 y se lleva el sur a -29, donde sí hay terreno en todo el ancho: tablero final previsto de 21 m, no 18. No se debilitó la comprobación de apoyo.

Cantera: el GLB tenía coordenadas UV degeneradas, idénticas para los ocho triángulos del suelo. Se corrige el mapeado de cada copia de contacto y se recorta de forma irregular el borde del material para evitar el aspecto de loseta. No se cambia el emplazamiento guardado ni el donante original.

## Cambio de hipótesis para la Brecha

La previsualización de la talla escalonada conserva el defecto principal: una zanja en Y de profundidad uniforme. La referencia tiene una depresión central rodeada por placas fracturadas y ramas que pierden profundidad. Se prepara una alternativa macroscópica: conservar el eje y las exclusiones existentes, reducir gradualmente la profundidad de las ramas periféricas, tallar cortes poco profundos entre placas irregulares en el entorno central y concentrar el hundimiento en el corazón. No sumar losas flotantes. Todas las superficies de detalle se reproyectan sobre la misma malla; la vegetación existente acompaña verticalmente al suelo.

La panorámica también necesita atmósfera en el borde de la región visible, no solo en el borde lejano de la continuación que queda fuera de cámara. Se prepara una franja estrecha e irregular de bruma únicamente en el zoom panorámico. La vista regional permanece libre de niebla y se restaura el estado de la cámara al terminar cada render.

## Primera revisión real de Unity: 40291321336

Importación y fuente PASS. PlayMode: 60/62, sin omisiones. Dos defectos concretos: el suelo de Brecha se exportó a 2 mm cuando su contrato exige 15 mm (error medido 13,3 mm); se restablece el desplazamiento de 15 mm sin cambiar tolerancias. La nueva prueba de selección consultó `Collider.bounds` mientras la vista de ciudad desactiva su física; se corrige para medir el volumen local real y se fija explícitamente el zoom cercano.

Las capturas reales confirman nodos mayores y el puente corto. Se rechaza el acabado de pradera de esta iteración: atenúa el grano original en cercana y genera camuflaje en panorámica. El control sin acabado conserva mejor la textura. La siguiente versión preserva el grano del núcleo, comprime la variación cromática con la huella de píxel y calibra la continuación al verde/dorado del núcleo, con caminos separados. Se mantiene la crítica de la zanja en Y; la alternativa de depresión fracturada sigue pendiente de observar en Blender y Unity.

## Cauce y transición geológica

Las capturas reales de criaturas y cantera revelan agua tapada por el terreno fuera del núcleo. La inspección del GLB y de sus alturas detecta terreno por encima de 437/464 centros de triángulo en la extensión oriental del río y 795/1292 en la occidental (muestreo de cuadrícula, diagnóstico, no prueba exacta de contacto). Se excava únicamente la huella de las superficies de agua existentes, conservando los tableros y las máscaras jugables. No se inventan cursos nuevos.

La alternativa de depresión central de Blender 40293974247 cambia la masa, pero su material gris uniforme forma un parche y las normales planas acentúan triángulos alargados. Se conserva la geometría de depresión, se suavizan las normales modificadas y se pinta un pigmento editable de placas y grietas con transición a suelo, con UV comunes en terreno y recubrimientos. La revisión de Unity sigue siendo obligatoria.

## Jerarquía regional y continuidad de partidas

El código ocultaba la ciudad a zoom >=152, antes de la escala regional. Se mantiene únicamente la representación de la ciudad realmente desbloqueada, con tamaño regional estable (aprox. 8–10% de altura de pantalla) y su mismo destino seleccionable. Se oculta en panorámica. No se añaden ciudades a las reservas vacías. Las criaturas ajustan su apoyo visual al terreno vigente sin reescribir coordenadas guardadas.

La ejecución Blender 40295461620 agotó el tiempo durante la modificación. Se identificó un acceso a normales de vértices inmediatamente después de cambiar suavizado de cada cara, susceptible de recalcular toda la malla repetidamente. Se agrupan los cambios y se leen las normales una vez; se añade registro de etapa. La recuperación comprueba PID, ejecutable y ruta exacta de la sesión propia antes de detenerla; se reinicia desde la fuente inmutable, no desde un resultado parcial.
