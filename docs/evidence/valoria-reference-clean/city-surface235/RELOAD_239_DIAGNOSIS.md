# Native239: diferencia localizada de serialización

La integración se rechazó sin persistir main. Run39502200815; artifact compacto12302258198. Las seis comparaciones previas a guardar pasan (media máxima0.002305/255). Tras reabrir, los detalles del Bastión se mantienen: media I0.001381, II0.000778/255. La diferencia aparece exclusivamente en las franjas de mezcla material del terreno junto a los caminos: media paisaje0.07489, vertical0.12237/255, fracción por encima de5/255 hasta0.014043. La imagen de diferencia fue inspeccionada; no reaparece el fallo de terrazas/antorchas.

Corrección: crear y guardar TerrainData antes de la captura de ensayo, descargarlo de memoria y recargar el recurso de disco. La integración reutilizará ese TerrainData exacto con SHA, en vez de regenerar un mapa transitorio. Se mantienen las seis comparaciones antes de guardar y las seis después de reabrir, con sus tolerancias intactas. API oficial de Unity6000.3: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Resources.UnloadAsset.html . La causa de detalle de textura se considera inferida por localización de la diferencia; el ensayo240 comprobará la solución.

El directorio no versionado CitySurface235 de la ejecución fallida se conserva en RUNNER_TEMP/valoria-rejected-239-terrain antes de crear CitySurface240, sin tocar recursos versionados ni la escena canónica.
