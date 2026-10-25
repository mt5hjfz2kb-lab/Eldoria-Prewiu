# Región 1 — candidatura de integración y revisión artística (25/10/2026)

**Estado:** producción dentro del PR #100 **DRAFT**. No fusionar, publicar ni otorgar ART_APPROVED.

## Evidencia real recuperada y revisión

- Fuente del último pase funcional: `a404cd45a907a7d9e06cbc7dd483d4fe67193f05`, ejecución Windows Unity/Blender [40180536302](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40180536302): 60/60 PlayMode; Blender exportado; gate de importación Unity pasado; fuente Tripo reutilizada sin créditos; las pruebas completas EditMode y Bastiones I–II requieren el pase de materialización autorizado.
- Última versión íntegramente materializada y certificada previa: [40142228055](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40142228055), cuyas vistas sirven como base de cámara idéntica para este pase.
- Comparativa objetiva de las capturas 1280×720, cámara y zoom iguales en las dos ejecuciones, umbral por píxel `max(|RGB_antes - RGB_después|) > 5/255`:
  - cercana horizontal: 0,00 % de píxeles cambiados; no adjudicar mejora visual en esa vista.
  - regional horizontal: 10,28 %; diferencia media absoluta RGB de 1,58/255.
  - panorámica horizontal: 1,42 %; diferencia media absoluta RGB de 0,19/255.
- **Observación:** el nodo de piedra de la captura real sigue pareciendo una agrupación de rocas demasiado pequeña, aislada en un campo abierto. Las distintas piezas de la cantera aportan algo de forma, pero no acreditan todavía un nodo definitivo de calidad profesional.
- **Observación:** persiste un límite rectangular visualmente reconocible entre el núcleo AF/AG y el terreno extendido en la regional. La Brecha es visible en panorámica, pero su presencia sigue por debajo de una composición geológica convincente del nivel de las referencias.
- **Decisión:** conservar la geometría y los guardados funcionales si vuelve a certificarse; no confundir el resultado PlayMode con mejora artística. Antes de nuevos detalles secundarios, concentrar el siguiente lote artístico en la transición de terreno y en la escala/lectura de los recursos. Mantener cantidades/identificadores y las fuentes AF/AG intocadas.
- **Veredicto provisional:** RETENCIÓN TÉCNICA, revisión visual crítica pendiente. **No** afirmar 7,5/10, ART_APPROVED, móvil físico certificado ni sustitución de `main`.

## Contratos y próxima certificación

El commit que incorpora esta revisión dispara un pase de **materialización y certificación nativa** del candidato actual, sin cambiar modelos de Tripo ni introducir contenido nuevo. Exigir como mínimo 60/60 PlayMode, 26/26 EditMode, 44/44 I–II y comparación real de seis cámaras (horizontal/vertical × cercana/regional/panorámica). Conservar fuente editable Blender, GLB real, capturas, hashes, pruebas y evidencia de continuidad de zoom. Sin dispositivos Android/iOS disponibles, FPS físico seguirá pendiente.

**Responsable y método:** observación de PNG reales de Unity, decisión artística explícita, corrección comprobada de enterramiento de recursos, nueva certificación completa antes de proponer integración.
