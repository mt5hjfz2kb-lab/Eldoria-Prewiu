# Bastión IV–V · migración integrada

Fuente de reglas: `v0220/index.html` (ascenso y obra), `v0220/js/v030-depth.js` (elección de IV) y `v0220/js/state-economy.js` (producción). La implementación continúa la sesión LocalGateway y FileStateStore; no introduce una economía paralela.

| Cambio | Contrato |
|---|---|
| III → IV | Fisura cerrada; aserradero, cuartel y granero >=2; 850 madera, 700 piedra, 300 comida; 16 s; +1150 poder |
| Elección IV | Producción +260 madera/+180 piedra; defensa +220 piedra/+100 poder; civil +180 madera/+280 comida. Permanente, sin exclusión de edificios, recompensa única |
| IV → V | Elección realizada; 1100 madera, 900 piedra, 500 comida; 18 s; +1500 poder |
| Cantera productiva | Bastión V; 500 madera y 200 comida; 12 s; +420 poder; nivel inicial 1 |
| Economía de piedra | 2 × nivel de cantera por segundo; acumulación sin conexión limitada a una hora |
| Mejoras | Nivel propio <= Bastión. Material especial existente para nivel >=2; no se consume. No se modifican ni generan las referencias V |

Los cinco GLB optimizados provienen de las tareas ya pagadas y revisadas. `tools/bastion-iv-v/assets.json` fija artefactos y SHA-256, y la recuperación comprueba los bytes. Los originales y `.blend` siguen en sus entregas conservadas. No se usa el antiguo Bastión vetado ni se llama a Tripo.

El modelo V de cada edificio reemplaza únicamente su propia arquitectura al terminar su mejora a V. Bastión V por sí solo mantiene el resto de edificios en su nivel real. Los pequeños incrementos reutilizan el stock texturizado cuyo cambio visible se comprobó en III; la revisión IV–V exige nuevas capturas de la cámara real. La cantera I–IV utiliza un taller existente como candidato de revisión, sin anticipar el modelo V.

La interfaz abre destinos independientes sobre el Canvas Royal Gold existente, conserva navegación y partida, y oculta los paneles contextuales mientras una pantalla de gestión está abierta. La imagen de referencia se identifica como arquitectura de nivel V para no confundirla con el edificio actual.

Estado: dominio IV–V y regresión III aprobados en ejecución 40877107919. Integración gráfica, pruebas nativas y revisión de composición en curso. Esto no certifica rendimiento físico Android/iOS ni autoriza una publicación.

## Verificación y métodos de trabajo

- Ejecución `40879777150`: importación real D3D11 completada; 54/54 EditMode aprobados. PlayMode: 71/73 aprobados; dos expectativas antiguas buscaban el panel flotante y su temporizador durante la gestión. Las pruebas se han migrado al botón y cierre táctil reales de la pantalla independiente, al pago persistente y a la cuenta atrás inmediata. El temporizador contextual vuelve al cerrar gestión. No se eliminan las verificaciones del gateway.
- Dominio y bindings de la revisión `42fc0fc` aprobados en `40883546410`; la siguiente revisión también pasa en `40884918436`.
- Las capturas regionales del primer ciclo son evidencia de regresión del mundo existente, no pruebas visuales del nivel V. Las capturas IV–V quedan pendientes de la ejecución nativa específica.
- Se conserva la caché de Unity entre ejecuciones. El primer importado completo tardó aproximadamente 13 minutos; no es un motivo para regenerar modelos ni duplicar créditos.
- Las capturas usan la cámara real, guardan orientación/aspecto y SHA de la fuente compilada. Los cambios de píxeles verifican que existe una diferencia, pero no sustituyen la revisión artística.
- La clasificación sigue el prototipo web: perfiles simulados, sin jugadores online. La extracción de vetas regionales en V activa una sola vez la intervención de Aldric y se conserva al recargar.

## Revisión nativa IV–V y correcciones verificables

- `40884918480`: 54/54 EditMode, 72/73 PlayMode; recorrido I–II y prueba IV–V completados. Las capturas se rechazaron artísticamente: faltaban textos e imágenes en gestión, el cierre podía recibir el toque en navegación, la arquitectura V no estaba en la capa de la cámara regional, y había luces antiguas dentro del nuevo Bastión.
- `40892044220`: 54/54 EditMode, 72/73 PlayMode; I–II aprobado. Los textos ya aparecen. La nueva comprobación de referencia falla correctamente: las cinco JPEG se habían importado como `UnityEngine.Cubemap`, por lo que `Resources.Load<Texture2D>` devolvía null. Se fija explícitamente `TextureImporterShape.Texture2D`, manteniendo los bytes de referencia.
- No se relaja la prueba de cierre: se conserva el raycast real y se añade diagnóstico de profundidad. El orden de hermanos por sí solo no resolvió el fallo. Se reutiliza el registro de gráficos ya empleado por el HUD V3 existente tras cambios de Canvas, sin crear otro Canvas ni otra sesión.
- El taller GLB provisional de cantera I–IV se rechaza: estaba girado y carecía del atlas original. La sustitución reutiliza el prefab original `EA03_Village_OutBuilding_Shed_03b_PRE.prefab`, sus materiales y transformaciones, reubicados como recursos seguros para la compilación. La revisión artística nativa sigue pendiente; reutilización no equivale a aprobación.
- Se añaden capturas de orientación 0/90/180/270 de la arquitectura V para decidir accesos mirando la cámara real. Las luces antiguas del keep y los árboles que invaden la cantera se ocultan de manera reversible únicamente en el progreso correspondiente.
- Los artefactos grandes se recuperan mediante un trabajo Ubuntu que separa los archivos originales de diagnóstico y las capturas, sin ejecutar otra generación ni repetir Unity. La siguiente versión del workflow publica también diagnósticos pequeños directamente.

Documentación técnica consultada: Unity 6000.3 `Canvas` y orden de dibujo. Se conserva el contrato del proyecto de un único Canvas de HUD; no se introduce un Canvas independiente como atajo para superar la prueba.
