# Valoria Bastión V — prueba de lenguaje visual móvil V9 (aislada)
Fecha: 2026-11-16. Rama: `art/valoria-compact-mobile-v9-20261116`.

## Encargo vigente
Construir una alternativa **original de geometría tridimensional real** para Valoria inspirada en la legibilidad y accesibilidad de los juegos de estrategia móviles estilizados, especialmente Whiteout Survival, Kingshot y Top Heroes, **sin copiar modelos** ni alterar `main`. La referencia es de lenguaje visual, no de propiedad intelectual.

## Diagnóstico empírico (V7, render Blender verificado)
V7 es una composición técnicamente reproducible (archivo .blend, GLB y capturas), pero no resuelve la dirección artística: el Bastión se parece a un gran edificio con múltiples tejados superpuestos; su escala roba atención a los edificios secundarios; todos comparten un azul dominante; el terreno es apagado; falta una silueta de fortaleza legible a resolución de móvil. La calidad artística NO está aprobada. El éxito técnico de CI no prueba calidad comercial ni rendimiento real Unity.

## V9 — decisiones implementadas en código, pendientes de aprobar visualmente
1. **Replanteamiento volumétrico**: eliminación espacial de la construcción principal experimental anterior, conservando edificios de recursos originales; bastión nuevo más bajo, con solo una torre de mando dominante, dos torreones octogonales, puerta monumental y gran sala con un único gran tejado.
2. **Identidad de los cinco edificios**: madera y cedro para Aserradero; ocre de cosecha en Granero; terracota y equipo militar en Cuartel; pizarra gris mineral en Cantera; arquitectura de piedra crema y pizarra azul en Bastión.
3. **Paleta / terreno**: verde claro menos oscuro, variación moderada en follaje, caminos beige, menos piedras sin función; escena sin plataforma elevada independiente.
4. **Composición móvil**: cámara ortográfica más cenital, encuadre horizontal y vertical distinto de V7, sin módulos que tapen toda la pantalla.
5. **Implementación reproducible**: `tools/stylized-direction-pilot/valoria_mobile_citadel_v9.py` se ejecuta dentro de la generación original Blender; materiales con atlas almacenados en el archivo fuente y GLB; render PNG auténtico, editable .blend y GLB exportado.

### Verificaciones obligatorias
- [ ] Render Blender real, horizontal 1600×1000 y vertical 900×1600, sin grietas negras, caras perdidas, apoyos flotantes ni saturación quemada.
- [ ] Composición valorada por inspección humana real (no script): a tamaño teléfono debe reconocerse **qué hace cada edificio**, Bastión no debe dominar toda la escena, y el resultado debe parecer un juego móvil comercial más que una maqueta low-poly.
- [ ] Colorimetría, volumen y sombras coherentes entre ciudad y escenario.
- [ ] GLB optimizado por zonas de interacción, preservando bounding boxes, normales y materialidad; presupuesto de renderers documentado.
- [ ] Prueba capturada en **la cámara real de Unity URP**, con HUD minimalista; sin tocar sistemas de juego.
- [ ] Prueba física de estabilidad, nitidez y legibilidad en iPhone antes de decidir migrar.
- [ ] Si no se alcanzan los criterios, no maquillar con presentaciones: revisar técnica y considerar kit modular 3D CC0 solo tras inspección y licencia, preservando autoría visual.

### Estado actual
- **Investigado**: referencias de lenguaje visual móvil; materiales y Blender reproducibles. Material alternativo modular Quaternius/Kenney verificado CC0, pero **no descargado ni implantado**.
- **Implementado**: rama aislada, geometría central totalmente alternativa, nuevos materiales y cámara, workflow CI.
- **Probado**: todavía no consta el render V9 final en esta revisión. Los renders V7 sí se generaron realmente, pero no constituyen aprobación.
- **Pendiente**: inspección de V9, iteración artística, optimización y Unity/iPhone.

## Invariantes
`main` no recibe cambios. Ni mecánicas, ni datos, ni progresión, ni combate se modifican; el experimento altera solo presentación. No atribuir a este trabajo validaciones de Unity que no hayan sucedido realmente.
