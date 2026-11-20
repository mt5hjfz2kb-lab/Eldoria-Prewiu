# Corona de la Brecha — etiquetas condicionadas por el estado de construcción

**Evidencia real:** captura Unity WebGL del [run 44807702613](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44807702613), compilación SHA `70444c18217d3a519f313c840e9fb3b13f2fa033`. La captura tras recargar exhibe un **Aserradero terminado de gran volumen** hasta y≈625 en 390×844, mientras que en el estado disponible sólo existe un solar en y≈490–570. La placa en y≈580 que se propuso para el solar tapa la fachada del edificio construido.

**Corrección implementada sin cambiar gameplay:** `cachedSawmillPhase` se calcula únicamente al recibir un `PlayerState` real en `Refresh`; `SyncWorldNameplates` elige y según la fase actual, sin lanzar `LocalGateway.Snapshot` por frame:
- AVAILABLE y estados iniciales → y=-2.90, próximo al borde del solar.
- UNDER_CONSTRUCTION → y=-3.45, hacia el borde exterior del andamio.
- BUILT → y=-3.86, bajo la fachada del Aserradero ya terminado.
  
La etiqueta del Bastión permanece como señal sobre la torre oeste (x=-1.63, y=3.05), lejos del pozo y del objetivo de arriba a la derecha. Todas las placas se siguen transformando por la cámara Unity real y ocultan si están fuera de zona segura. No se mueve ningún SpriteRenderer, parcela, edificio, ruta de habitante ni se modifica guardado, coste o comando.

**Pruebas:** NUnit exige que la fase de etiqueta pase a `UNDER_CONSTRUCTION` cuando el LocalGateway cambia a obra. Se requiere captura genuina exact-HEAD para verificar visualmente las tres fases en retrato/horizontal; simulaciones o screenshots antiguas no demuestran la corrección.
