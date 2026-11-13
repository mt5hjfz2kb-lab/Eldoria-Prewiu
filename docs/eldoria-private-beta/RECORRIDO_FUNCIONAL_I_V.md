# Eldoria — recorrido de pruebas I–V (contraste contra Unity)

**Fuente de verdad:** `GameState.cs`, `LocalGateway.cs`, `EarlyRetentionRules.cs`, `EconomyProductionV2.cs`, `NarrativeCoherenceRules.cs`, `SliceContentProfiles.cs`, más `PLAYER_AUDIT_20261106.md`. Las pruebas documentadas son sobre versiones concretas y NO avalan automáticamente la próxima WebGL.

| Etapa alcanzada | Acciones reales y dependencias | Métricas primarias |
|---|---|---|
| Inicio / I | Elegir nombre; reconstrucción de Valoria, aserradero, nodos de madera/piedra, ida/regreso, recompensas, subida I→II; Vigilantes desbloqueables tras Aserradero I | inicio/tiempo, ayuda, obra, recurso, mundo, marcha, primer Guardián |
| II | Cuartel, reclutamiento arqueros, preparación real de marcha, Engendro, informe, subida II→III. Exploradores se desbloquean con II+Cuartel+primera marcha confirmada | 10 min espera de entrenamiento, composición, marchas disponibles, victoria, informe, contratos |
| III | Granero/comida, lobo/jabalí/carne, Fisura privada investigada 20 min, contacto Lyra, asalto, cierre/reclutamiento Lyra, Arcón con manuscrito único, subida III→IV | caza, ruta, espera paralela, fase narrativa, diferencia contacto/reclutamiento, premio único |
| IV | Aserradero/Cuartel/Granero a nivel 2, decisión irrepetible producción/defensa/civil, crecimiento y subida IV→V | claridad de opción, elección, coste, efectos y mejoras individuales |
| V | Cantera/stoneworks produce piedra, Acechador de Ceniza, progreso Bastión V 4/4, fin de contenido explicable. Vigilantes/Exploradores siguen disponibles, y edificios se mejoran de forma individual | cantera útil, enemigo, cierre verdadero, expectativa de VI, interés en más contenido |

**Actividades transversales realmente disponibles:** tres huecos de marcha (sin ejército infinito), Vigilantes: etapas de 25 por capítulo y recompensas únicas, Exploradores: 3 encargos / 12h con 6 pendientes como máximo, buzon/informes, producción pasiva con límite offline, aceleradores de 1/5min existentes, héroes Aldric y Lyra tras III, Poder real, diálogos contextuales, dos orientaciones, persistencia de partida.

**Matices importantes:** las etapas de Vigilantes más allá de 25 dependen de Bastión y del estado efectivo de contenido; no son requisito para llegar a V. No asumir que PvP/hospital/equipamiento/T3–T5 están activos solo por aparecer como tipos de datos internos. El Códice y las cartas están fuera de la ventana de pruebas, prevista hasta V.

**Observación de calidad:** un fallo de acción que luego se resuelve no es igual a bloqueo absoluto; eventos de fallos deben registrar resultado del gateway, motivo y pantalla, no solo que se pulsó un botón. En combate no se inventan acciones de habilidades manuales.
