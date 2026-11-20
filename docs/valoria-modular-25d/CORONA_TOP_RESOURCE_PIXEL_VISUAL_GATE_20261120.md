# Corona — prueba visual de los recursos realmente pegados arriba

**Evidencia previa comprobada:** captura real Unity WebGL 390x844 del run 44817877082, fuente 7afd1d3: región superior derecha x=52%-96%, y=1.5%-6.5% de pantalla contiene exactamente 0% de píxeles con luminancia RGB media inferior a 80; el HUD antiguo deja sólo césped allí. La barra de recursos aparece mucho más abajo.

**Nueva implementación:** el código Unity de Corona ahora ancla recursos en la primera franja de área segura. Se añadió a Chrome un control de píxeles reales: 390x844 y 1170x2532 deben tener al menos 18% de muestras oscuras en la misma región (indicador de recursos ornamentales presentes). El criterio se probó en un borrador de geometría NO UNITY, que dio aprox. 50%, pero NO está aprobado hasta obtener la captura de la compilación real.

La comprobación permanece en la ruta de navegador donde previamente se verifican interacciones reales del juego, gasto 230→150 y persistencia. No se importa la captura vieja al nuevo resultado ni se manipula la salida de Unity. Una prueba técnica no sustituye la evaluación artística a escala de teléfono. Se mantienen `main` y las mecánicas intactos.
