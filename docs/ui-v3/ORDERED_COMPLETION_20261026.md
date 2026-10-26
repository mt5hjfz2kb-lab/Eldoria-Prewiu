# Interfaz y continuidad móvil — reconciliación del encargo

Responsable: `codex-eldoria-ordered-completion-20261026`.

La interfaz aprobada ya está implementada: propuesta 1 de ciudad/mundo y diálogo
1, sobre el HUD único; destinos opacos de Entrenamiento y Marchas del PR90.
Las capturas genuinas revisadas muestran controles, costes, temporizadores,
selección de cantidad, regreso y diálogo Aldric con texto completo. La captura
`integration-story.png` se toma al inicio de la animación: no sirve para juzgar
opacidad final. `integration-story-aldric.png`, tras espera de asentamiento,
muestra pergamino opaco y texto legible. No se modificó el diálogo para corregir
una falsa conclusión basada en un fotograma de transición.

PR111 integrado: entrada/reentrada cercana, foco continuo y anclaje de recursos
y criaturas. Fuente final `108a6346d5b242474c20b2d26c2945559cc9cdcc` certificada en
run40372404380, artefacto12677591832: XML descargados, 63/63 PlayMode y 26/26
EditMode, cero fallos/omisiones. El run40370249261 anterior sirve como historia,
pero NO certifica los dos archivos cambiados después (modelos de recursos y test).

PR112 integrado como `8d3f25053f6817dcbf34d07da426b284fdd12908`: etiquetas panorámicas
y borde atmosférico. El gate general run40387268368 ha concluido SUCCESS,
incluyendo EditMode, PlayMode y compilación Windows sobre ese SHA. La comprobación
UI adicional run40387268720 también ha concluido SUCCESS: 64/64 PlayMode y
26/26 EditMode, artefacto12686696359, sobre el mismo SHA integrado.

Héroes no tiene una pantalla de gestión dedicada en el componente actual (destinos:
Training/March). Aldric sí participa en la preparación de marcha. No se presenta
una pantalla de Héroes inexistente como terminada ni se inventa progresión de héroes.
Orín/diálogo3 permanece condicionado al disparador narrativo auténtico: no añadir
un evento ficticio para aparentar cobertura. Bastiones III–V permanecen fuera.

El PR99 antiguo no debe fusionarse íntegro: su ocultación de etiquetas quedó
superada por PR112 y su inclinación estratégica antecede al controlador continuo
de Región 1. Conservar la rama como historia, sin reintroducir doble inclinación.

Distribución y calidad física son estados separados. Android permanece 1.14/code14;
estas correcciones posteriores no se han publicado por este encargo. No se afirma
instalación física, perfilado móvil ni aprobación artística global.
