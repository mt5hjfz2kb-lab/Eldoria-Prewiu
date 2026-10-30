# Decisión forestal tras el primer A/B real

Observación: run40382253603 (fuente993b3f53f68cae58ef0febb9c58a65d2b3892624), artefacto12684943701 SHA25660cc38fff2a824a2f55bfa94769a22e9587fccd4746ac48ad90b084cc477189e. Seis cámaras coinciden con el control40372404380 y sus PNG verifican los hashes nativos. Pruebas técnicas correctas. Veredicto: reducción por hash RECHAZADA por conservar copas artificiales y dispersión.

Decisión: retirar la familia compartida redondeada y las cohortes añadidas para rellenar huecos, en vez de ocultar individuos al azar. Fuentes inspeccionadas: authoring/build_region_from_ai.py líneas800–1012 y authoring/interactive/command.py líneas200–262. Las cohortes Context/Belt/Regional/Castle son decorativas; ningún nodo de madera/piedra pertenece a ellas.

Corrección: ocultar ambas representaciones de 3251 de los5047 árboles continentales: 2482 copas redondeadas más769 coníferas de relleno no solapadas. Conservar1796 árboles continentales, incluidos31 ecotonos de abeto, y los462 Pilot_Pine originales. Se eliminan grupos de relleno completos para recuperar claros; permanecen masas numeradas y abetos de ribera. El GLB, los BLEND, anclas, navegación y recursos quedan intactos. No se crea nueva geografía ni malla, no se utiliza ruido procedural.

Preflight BlenderMCP: la lista real de herramientas de esta sesión no ofrece BlenderMCP; Unity/Blender locales ausentes. La corrección es exclusivamente de visibilidad en Unity sobre geometría Blender existente, sin alegar nueva autoría bpy ni sesión MCP. La validación siguiente exige capturas Unity nativas y revisión; la versión revisada NO se considera aceptada por esta decisión.

Cierre posterior: el ciclo se completó con la revisión independiente de los seis
pares40387268720/40394742870 y la certificación final40399630175 (65/65 PlayMode,
26/26 EditMode). La mejora forestal fue RETENIDA y PR113 quedó fusionado como
c448cb783c512377702b452bd3bb6a7459c1c135. Ver FOREST_VISUAL_REVIEW.md para el juicio,
los límites y la reversión exacta. No equivale a ART_APPROVED global. Sin publicación
APK, coste0€.

