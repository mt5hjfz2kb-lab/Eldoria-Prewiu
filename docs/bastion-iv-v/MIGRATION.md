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
