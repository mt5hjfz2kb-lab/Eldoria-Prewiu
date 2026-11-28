# Inventario nativo en el mismo fotograma de la captura

Ejecutar en un checkout desechable y limpio del commit de juego que se audita, con las dependencias y recursos del proyecto presentes y el editor Unity de su ProjectVersion.txt. La ejecución usa GPU y reserva el runner durante las cuatro suites. Requiere Python 3.9 o superior.

```
python run.py --checkout RUTA_CHECKOUT --unity RUTA_UNITY_EXE --output DIRECTORIO_NUEVO_EXTERNO --probe RUTA_ValoriaLiveInventory.cs
```

El lanzador lee el SHA del checkout y copia el driver y la sonda a un directorio temporal de Editor. Inserta observaciones únicamente en cuatro fuentes Editor de ese checkout. Conserva sus condiciones, acciones y aserciones. Al finalizar restaura los cuatro archivos, incluso si falla una prueba. Los informes y metadatos propios que Unity produzca pueden permanecer en el checkout desechable. El lanzador registra hashes de los fuentes de QA originales, instrumentados, del driver, la sonda y el propio lanzador.

La variable ELDORIA_NEW_UI_ONLY=1 activa el predicado de presentación del jugador en el Editor. Cada fixture abre la escena canónica, usa el guardado de prueba, instala Vigilia y configura la cámara. El driver rechaza una muestra si el Canvas anterior sigue presentando, falta el HUD esperado o la cámara y el render target no corresponden al de la fixture. Después de que esa fixture guarda un PNG y antes de que restaure la cámara, registra la jerarquía y los metadatos de ese mismo fotograma. El directorio de salida debe ser nuevo para evitar mezclar evidencias.

| Suite | Estados observados por sus puntos de captura |
|---|---|
| hud | Nombre, páginas del diálogo inicial, ciudad y Bastión seleccionado. Las dos últimas se capturan a 1600×900, 900×1600, 844×390 y 390×844. |
| destinations | Navegación desbloqueada, entrenamiento, marcha y detalle, salón, cuatro pestañas de Aldric, resumen de Lyra, correo, fila poblada, informe, Vigilantes, Exploradores y Tesoros. |
| retention | Batallas y resultados de Vigilantes, exploradores enviados, listos, y distintas cantidades de hallazgos. |
| bag | Ciudad de fixture, paquetes nuevos, carta leída y pendiente, reclamada y sin acreditar, paquete usado y partida recargada. |

Cada muestra conserva PNG, inventario JSON y un archivo sample.json con el SHA del PNG, SHA de fuente, escena, fotograma, cámara, dimensiones del render target, dimensiones de la ventana Editor y dispositivo gráfico. El manifiesto comprende los códigos de salida de Unity y las muestras esperadas ausentes. CAPTURED exige las cuatro salidas cero, los estados mínimos de todas las suites y correspondencia PNG–inventario. Los estados adicionales que capturen las fixtures también quedan registrados.

Las fixtures inyectan estados avanzados en sesiones de prueba. Esta colección no acredita la progresión jugada desde cero, todos los edificios y niveles, los tres zooms del mundo, el coste GPU en el jugador, dependencias de compilación, WebKit ni pruebas físicas. El encuadre en la textura de salida y la resolución de la ventana del editor pueden diferir. Comparar la geometría en tamaño de render equivalente. El coste de la sonda de observación se mide por separado del perfilado del jugador. Algunos gates artísticos históricos de las fixtures fijan nombres, hashes o colores de Vigilia; podrían rechazar un cambio artístico legítimo al aplicarse al candidato. Conservar el fallo y los archivos parciales antes de adaptar el método de comparación. No instalar la interfaz de Main sobre el ejecutable donante para obtener su comparativa.

Validación preparatoria local sobre las cuatro fuentes de d74f1a29d5472e9037d8534142768d4c13667ddd: comprobados seis puntos de observación, restauración byte a byte tras fallo del staging y detección de salidas vacías. Sin compilación ni ejecución de Unity en esta preparación.

En cuatro muestras 390×844 se captura también el enfoque de cámara de cada edificio, sus renderizadores y colliders. Ciudad y selección de Bastión en la sesión inicial, navegación avanzada en la fixture de nivel II y ciudad de la bolsa en la fixture de nivel III. El manifiesto comprueba SHA, fotograma, tamaño, nivel, escena y los cinco IDs. El nivel observado y la causa del foco constan en cada sidecar; estos cuatro estados no cubren IV–V ni las obras de todos los edificios.
