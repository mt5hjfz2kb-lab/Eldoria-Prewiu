# Corona — familia visual Bastión I–V

Base aprobada por el usuario: 09fac3a366d59a7525d3fa057dc9acd2c3bac793. Encargo del 21-11-2026: preparar 6–8 accesos, última pasada I y continuar la misma técnica hasta V. Rama experimental conservada; main y beta no se modifican.

## Autoría e integración
Cuatro variantes arquitectónicas independientes derivadas del Bastión I mediante generación de imagen con referencia. II repara el ala izquierda; III eleva la sala central y cubre la torre circular; IV refuerza galerías y puerta; V corona la torre central con una linterna amatista y dos torretas. Nuevos Cuartel, Granero y Cantería: misma perspectiva, luz noroeste, piedra cálida, pizarra azul, madera envejecida y enseña del árbol. No son recolores ni una ilustración plana de la ciudad.

Los PNG finales y sus metas viven en Resources/ValoriaModular25D/Progression. La procedencia y briefs están en CORONA_PROGRESSION_ASSET_PROVENANCE_20261121.json; prepare_progression_assets.py reproduce el ajuste uniforme de tamaño y anclaje a partir de las fuentes indicadas. No retoca artísticamente las imágenes. La extracción del fondo del Cuartel se hizo con la herramienta de generación integrada. El RGB oculto de los PNG no es el fondo visible: se conserva alfa real. También se restaura el recurso disponible del Aserradero a triple resolución lineal, conservando el original.

## Presentación y reglas
Modular25DWorld.Apply selecciona el Bastión por PlayerState.BastionLevel. Cuartel exige Bastión II y BarracksLevel>0; Granero exige III y ChapterThree.GranaryLevel>0; Cantería exige V y StoneworksLevel>0. Las texturas de edificios futuros se cargan al mostrarse, no al entrar en I. Accesos y contactos de suelo aparecen solo con el edificio terminado. El huerto inicial se oculta cuando ocupa su espacio el Cuartel real. La cámara de inicio I se conserva; centrar desde II abre el encuadre para la expansión sur.

No se modifican LocalGateway, la economía, los requisitos, guardados, temporizadores o combate. Esta ampliación es visual: no implementa las pantallas del mundo, entrenamiento, expediciones ni el recorrido completo de capítulos que permite ascender. Las vistas I–V de QA usan estados de presentación expresamente identificados como fixtures; jamás se guardan en la partida. No equivalen a una campaña jugada de I a V.

## Navegación
Ocho posiciones de 44–45 puntos de ancho táctil, separación mínima 3; iconos originales preservados. Solo Estado/Ciudad/Obras existen y ocupan las últimas tres posiciones. Ningún botón ficticio. A menos de 380 puntos lógicos, las ocho posiciones caben en dos filas. Se actualizan coordenadas reales del test de Chromium. Las comprobaciones locales de geometría pasan en 320/390/430/750 puntos; falta el resultado Unity del nuevo commit al escribir este documento.

## Verificación prevista
Prueba PlayMode del candidato exacto: arte HD, estados I–V, ausencia de edificios sin construir, ocho zonas táctiles, comportamiento B1 de construcción y guardado. Native: 15 vistas adicionales I–V de la escena real, con etiquetas de fixture; regresión B1, vídeo ocho segundos a 1x, WebGL y Chromium vertical/DPR3/horizontal. Revisar visualmente contactos, sombras, escala y oclusión. Nunca promover por un CI verde.

## Límites
Aprobación artística final y Safari físico pendientes. Texturas nuevas aumentan descarga y memoria, por medir en dispositivo. No se afirma que las nuevas pantallas de sistemas estén creadas ni que todas las mejoras de nivel de los edificios secundarios tengan su propio arte. Las etapas de construcción secundarias y su interfaz contextual se desarrollarán sobre los comandos existentes, sin inventar desbloqueos.
