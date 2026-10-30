# Brecha — decisión previa a la corrección del suelo

Observación real: la captura `sources/native-40211155178/review/breach-close-landscape.png` muestra un centro lila liso, parecido a una lámina, y bandas oscuras con facetas largas que se leen como placas superpuestas. La misma ejecución conserva 62/62 PlayMode, 26/26 EditMode y 44 comprobaciones funcionales. Ese PASS no resuelve el defecto artístico.

Decisión: conservar los nodos recién incorporados y dirigir ahora el esfuerzo a la Brecha. Sustituir la superficie lisa por placas de pizarra fracturada con grietas estrechas; reconstruir el revestimiento oscuro sobre las caras reales del terreno inciso, sin mover sus vértices. El resplandor quedará bajo las placas y concentrado hacia el centro, con borde exterior oscuro. Las elevaciones nuevas se limitan a la banda de falla ya excluida del tránsito; fuera de ella solo habrá pigmento pegado al suelo.

Corrección ejecutable: BlenderMCP abrirá el `.blend` nativo conservado por SHA. Copiará las caras reales bajo la huella de la falla y construirá celdas de fractura irregulares, con texturas explícitas de pizarra. Comparará el hash completo de posición/topología/UV/material del terreno y los bytes de todas las imágenes previamente empaquetadas. No habrá nuevas criaturas, objetivos, colliders ni solicitudes Tripo.

Veredicto pendiente: inspeccionar primero los renders genuinos de Blender para descartar láminas o geometría suspendida; después importar en la misma escena Unity, capturar otra vez la Brecha cercana/regional en ambas orientaciones y las seis vistas canónicas. Conservar únicamente una mejora visible. El 7,5/10 permanece sin acreditar.
