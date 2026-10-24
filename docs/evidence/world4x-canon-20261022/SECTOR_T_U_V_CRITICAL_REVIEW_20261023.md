# Eldoria — auditoría crítica T/U/V, 23 octubre 2026

Estado **ART_NOT_APPROVED / NO MERGE / NO RELEASE**. El sector conectado sigue siendo un piloto opt-in sobre O; no se ha ampliado Región 1. O, K, I y T se conservan recuperables.

## Evidencia nativa inspeccionada

- T: run [39979131350](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39979131350), artifact 12513275626.
- U: run [39981878592](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39981878592), artifact 12513314053.
- V: commit `7f63607e4ccf9a68373a58637813df30118c5b9a`, run [39983210334](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39983210334), artifact 12515295163; **18/18 PlayMode PASS**. 16.405 vértices O sin alterar, 62 árboles de extensión modificados, 156 árboles O intactos.

Comparación de PNG auténticos Unity desde las mismas cámaras, T frente a V: píxeles con diferencia máxima de canal RGB >5/255: cercana horizontal **0,1351 %**, regional **0,0495 %**, panorámica **0,0182 %**. Estas cifras no son nota artística: indican que las dos últimas intervenciones de pigmento son prácticamente invisibles a escala regional y panorámica.

### Juicio independiente

Las capturas T/V siguen mostrando pradera homogénea, grupos de coníferas relativamente aislados, caminos de borde uniforme y loma de lectura débil. No se ha demostrado un salto hacia 7,5–8/10. Tras dos intervenciones sin avance visual sustancial, **no continuar con más pigmentos locales**: cambiar de método hacia edición macroscópica de geometría/relieve, masa forestal y materiales físicos en Blender, sin reconstruir O. El sector V no se promociona como aprobado ni sustituye la referencia O.

### Hallazgo funcional P0

`region1-canon-captures-39983210334/ribera-pilot/sector/integration-and-cost.json` documenta `maxHeightDifference = 13.283976554870606` metros entre objetivos lógicos y superficie visual. Casos: `corrupt-scout` -13,284 m; `forest-valoria` +5,747 m; `old-watch-ruin` -5,188 m; `engendro-valoria` -4,430 m. Las selecciones de objetivos funcionan y las posiciones lógicas permanecen sin cambios, pero **el desfase visual NO está corregido**. No mover nodos, colisionadores ni guardados para esconderlo. Primero revisar datum y transformación del GLB respecto al origen local de la escena, y distinguir centro del objetivo frente a contacto real del mesh/terreno; luego corregir exclusivamente representación y probar alturas con límites verificables.

### Coste y zoom

El manifiesto de V registra 425.296 triángulos enviados, 51 instancias de malla, 51 renderers, 6 colliders lógicos. Son métricas editoriales, no FPS móvil. Las 49 capturas de zoom horizontal y 49 vertical se conservan en el artefacto y permiten generar un vídeo a partir de fotogramas reales; no acreditan ejecución física en teléfono.

**Siguiente intervención exigida**: corregir raíz de desfases de altura, esculpir formas naturales y materializar continuidad pictórica/forestal, ejecutar Unity, comparar desde cámara idéntica, revisar resultados y solo retener mejoras demostrables. No fusionar, publicar, ni declarar ART_APPROVED.
