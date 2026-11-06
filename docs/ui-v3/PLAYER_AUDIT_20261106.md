# Eldoria — partida dirigida como jugador, 6 de noviembre de 2026

## Resultado y alcance

Partida nueva **Neral**, dirigida acción por acción a partir de la imagen visible. Entrada a las 16:42 UTC y cierre a las 18:15 UTC: aproximadamente **93 minutos**, con 308 órdenes visibles (incluyen observaciones, recargas y cambios de orientación). No se modificaron guardados, recursos, reloj ni reglas del juego.

Completado: progresión inicial I–V, **Bastión V asegurado 4/4**, cantera productiva, Acechador de Ceniza, encuentro e incorporación de Lyra, dos cazas, tres contratos de exploradores y **25 etapas del primer capítulo de Vigilantes**. Segunda recarga confirmó nombre, edificios, tropas y composición. Al final: 76 tropas, Aldric nivel 10 y Lyra nivel 2.

El juego abre el capítulo 2 de Vigilantes; no se jugó ese contenido repetible. La guía anuncia Bastión VI, pero la ficha del Bastión V solamente ofrece «Operativo», sin ascenso. No se afirma haber agotado todo el juego ni probado sistemas inaccesibles. Relicario no se abrió ni antes ni después de Bastión V; talentos/equipamiento mostraron bloqueo.

**La prueba está completada; la calidad para una prueba amplia con personas todavía necesita correcciones.** Se puede recorrer el tramo disponible, pero hubo errores recuperables y problemas claros de legibilidad y orientación. Recomiendo corregir las incidencias altas y repetir ese recorrido antes de una convocatoria amplia. Una prueba pequeña moderada puede servir para validar comprensión y ritmo, informando del estado del prototipo.

## Entorno y evidencia

- WebGL native299, código de juego `1db8585329fad788fa702dab3e8a7b76a916b4bc`, compilación 42213699104. Se reutilizó esa compilación en Windows/Edge con D3D11; no se recompiló el juego.
- [Sesión y evidencia completa](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42254421841): artefacto `valoria-windows-chromium-evidence-42254421841`, ID 13528935901, 348646580 bytes, SHA-256 `2a0279d525290b20ed92ea70fe94db6fc55295b0cf7c9b9d8ace2e9c5ebb099c`.
- Evidencia: capturas numeradas, vídeo de la sesión, `actions.ndjson` y `manual-session.json`. El cierre técnico `MANUAL_SESSION_ENDED` no equivale a aprobación visual ni prueba automática del recorrido.
- Control acotado en commit `7131707b2e69f32df656403678c5533579e77dc3`; solo clic, teclado permitido, arrastre, rueda, orientación, observación y recarga. Decisiones tomadas por el agente observando las capturas, no un guion automático de objetivos.
- Dos resoluciones: 844×390 y 390×844. No fue una prueba física de Android, de tacto humano, de audio ni de FPS. La latencia entre órdenes incluye el transporte de control y no mide la respuesta del juego.
- Itch no se pudo usar directamente por su acceso restringido y autenticación no completada. La alternativa fue ejecutar la misma compilación WebGL en el equipo Windows. No se afirma haber validado el alojamiento de Itch durante esta partida.

Las referencias de acción siguientes corresponden a los números de las capturas, por ejemplo `0202-click.png`.

## Incidencias observadas

| Prioridad | Hallazgo y efecto para el jugador | Evidencia | Corrección propuesta / verificación |
|---|---|---|---|
| Alta | **Acciones válidas rechazadas con «El estado cambió / State changed, review requirements».** Ocurrió construyendo el granero y dos veces en el asalto con Lyra. El botón estaba disponible; el mensaje pequeño no explica cómo recuperarse. | 110–113; 202–208 | Investigar revisión de estado/actualización de ficha, sin dar por probada la causa. Evitar rechazos espurios y ofrecer reintento claro. Repetir ambas secuencias con producción pasiva activa. |
| Alta | **Informe de combate inaccesible en horizontal:** solo se veía la cabecera; la rueda no reveló el cuerpo. En vertical sí aparecían tropas, rondas, vida y botín. | 259–262 | Ajustar alto, distribución y desplazamiento. Verificar ambos formatos con el mismo informe real. |
| Alta | **Terreno de ciudad desaparece parcialmente tras cambio de orientación:** fondo azul grisáceo detrás de árboles y castillo. Persistió al entrar en Reino y al intentar mover la vista. Mundo se veía bien; recargar recuperó ciudad. | 251–257; 303 | Reproducir cámara/culling/terreno en secuencia vertical→horizontal. Diagnóstico pendiente; no atribuirlo a una causa específica sin medir. |
| Alta | **HUD y fichas demasiado pequeños, contadores y botones recortados.** El objetivo real de madera era 600, pero el contador parecía «360 /»; abrir el libro permitió entenderlo. «Acercarse a la…» no cabía. | 22–23; 194; 300 | Tamaño legible y contadores completos en ambos formatos. Comprobar con personas sin explicarles previamente los objetivos. |
| Alta | **Marcha inválida poco explicada:** tras enviar exploradores, Vigilantes conservó 36 preparadas con solo 24 disponibles, mostró poder 0 y «Ajusta tu marcha», pero ese botón no abrió el ajuste. | 70–76; 161–165 | Explicar tropas reservadas y enlazar el botón a preparación. Revalidar composición al cambiar disponibilidad. |
| Media | **Controles sin respuesta ni requisito:** Héroes antes de Lyra, segundo héroe antes del desbloqueo, T2 y relicario. Héroes y segundo puesto sí funcionaron después de incorporar a Lyra. | 50, 53, 131, 147, 221, 233, 269, 307 | Mostrar estado bloqueado y condición concreta; no presentar acciones aparentemente disponibles que no hacen nada. |
| Media | **Botón de volver vacío** en Exploradores y Vigilantes. Se pudo usar por prueba, no por su icono. | 67, 70, 136, 140 | Restaurar flecha/texto y validar a ambos tamaños. |
| Media | **Etiqueta de nivel tapa temporizador** del Bastión en vertical. | 235, 242 | Separar anclajes y comprobar durante mejoras, no solo edificio terminado. |
| Media | **Fisura y objetivos difíciles de identificar.** «Ir al objetivo» dejó la cantera como elemento dominante; fisura reducida a rótulo y trazo poco reconocible. Reserva de carne parecía un grupo de árboles. Varias fichas usan una casa genérica. | 115, 124–127, 194 | Enmarcar el objetivo correcto y usar iconografía/modelos reconocibles. No hace falta rehacer todo el mundo para corregir selección, encuadre y etiquetas. |
| Media | **Texto de habilidades en un espacio mínimo con desplazamiento oculto.** Parecía cortado; la rueda sí permitió continuar leyendo. | 223, 228–231 | Reducir tamaño, ampliar cuerpo y mostrar indicador de desplazamiento. No es pérdida demostrada del texto. |
| Media | **Buzón con mensajes indistinguibles y lenguaje técnico.** «Marcha completada» sin cantidades/origen; «ORIGEN · CORREO EXISTENTE»; informes de 25 combates llamados simplemente Vigilante. | 63–64; 258–262 | Identificar nodo, etapa, enemigo y cantidades. Diferenciar avisos de recursos ya recibidos de recompensas por reclamar. |
| Media | **Decisiones con poca información:** producción/defensa/vida civil se aplica inmediatamente sin detallar efecto; mejoras muestran producción actual, no ganancia; varios enemigos no comparan fuerzas. | 149–151; 247–248; 290 | Mostrar beneficio, coste y requisito antes de confirmar. Mantener la incertidumbre enemiga prevista por diseño, pero aclarar información propia. |
| Media | **Mensajes de estado desfasados y mezcla de formatos.** «Acción iniciada» permanece después; 1200s/20/20:00 para una investigación; ficha de asalto conserva 1200s aunque el combate fue corto. | 124–125, 194–207, 277 | Mensajes temporales y duración por acción actual con unidades consistentes. |
| Media | **Final de tramo poco claro.** Bastión V marca 4/4, anuncia VI, pero no ofrece ascenso; «Ir al objetivo» vuelve a la crónica. | 292–300 | Explicar explícitamente el límite disponible y ofrecer actividades posteriores reales. |

## Valoración como jugador: ritmo y presentación

Estas son valoraciones derivadas de la partida, no fallos técnicos demostrados:

1. **Esperas tempranas fuertes.** El primer entrenamiento obligatorio duró 10 minutos y la fisura 20. Hubo actividades paralelas y pude aprovecharlas, pero el juego debería presentarlas al iniciar la espera. No recomiendo reducir todos los tiempos sin medir abandono; primero enseñar qué hacer y evaluar el ritmo con personas.
2. **Hitos sin suficiente peso.** Lyra se incorpora mediante diálogos, pero no observé una presentación separada destacada. Completar el primer capítulo de Vigilantes usa el mismo recuadro de victoria y abre otro capítulo sobre el mismo mapa. Mejorar el cierre y mostrar qué se consiguió.
3. **Poca variación de decisión en Vigilantes.** Con 44 tropas y Aldric superé 20 etapas; el guardián 20 dejó 26 % de vida. Añadí 76 tropas y Lyra y completé 25 con 62 %. La composición importa, pero muchos encuentros consistieron en repetir Combatir; conviene comunicar diferencias y preparación, sin introducir habilidades manuales durante la batalla.
4. **Identidad visual inconsistente.** Aldric cambia entre retrato joven y diálogo canoso; Lyra también difiere entre ilustración y modelo. Algunas pantallas son mucho más luminosas que el tono oscuro del mundo. La ficha del héroe gana presencia en vertical; en horizontal el modelo queda pequeño.
5. **Progreso poco explicado.** Maestría, capacidad y poder suben sin destacar su efecto. El resumen al añadir Lyra sí muestra inmediatamente la mejora: es una base útil para extender al resto.

## Lo que funcionó

- Nombre elegido y progreso persistieron tras dos recargas; investigación y entrenamiento continuaron con tiempo real.
- La guía «Ir al objetivo» permitió avanzar en la mayoría de pasos sin conocer el código.
- Construcción, entrenamiento y exploraciones pudieron coexistir; tres marchas simultáneas se indicaron explícitamente.
- Incorporar Lyra aumentó atributos visibles y permitió preparar una marcha más fuerte.
- Los informes verticales ofrecen un resumen comprensible y análisis opcional.
- El cambio de castillo en Bastión V es un hito visual reconocible.

## Límites y cautelas de interpretación

No hubo bloqueo definitivo en el recorrido I–V. Los rechazos de acción se resolvieron mediante selección/reintento normales, no alterando el estado. No se afirma ausencia de errores de todo el juego. No se probaron PvP, tier 3–5, hospital, ausencia de 48 horas, compras, multijugador ni capítulos posteriores de Vigilantes.

Se corrigieron lecturas iniciales durante la observación: ejército inicial 36 (no 26); objetivo madera 600 (no 60); no se demostró que el monstruo visible tras la primera batalla fuera el enemigo derrotado; las habilidades sí admiten desplazamiento; pulsar el nombre reabre la crónica y su repetición no prueba un retraso automático. Las capturas y esta revisión prevalecen sobre esas primeras impresiones.

No se modificó el juego ni se publicó otra APK/WebGL durante esta evaluación. Los hallazgos quedan pendientes de corregir y repetir; la finalización técnica de la sesión no concede aprobación artística ni de balance.

## Comprobación final de evidencia

Archivo descargado y verificado por SHA-256 y CRC: 309 PNG, 308 órdenes, cierre `MANUAL_SESSION_ENDED` y job terminado correctamente. Vídeo WebM: 5101,08 s codificados (aprox. 85 min), frente a 93 min de reloj de la sesión; no se asume correspondencia exacta entre tiempo codificado y real. Las órdenes y capturas tienen secuencia y marcas UTC.

No se generaron registros `pageerror`, `requestfailed` ni errores del controlador. La consola contiene un 404 sin URL identificada, 18 avisos WebGL `INVALID_ENUM`, tres avisos de malla de más de 2.097.152 triángulos/Fast Midphase y tres avisos de sincronización obsoleta de persistencia. No prueban por sí solos la causa de las incidencias visuales. No se afirma consola limpia.

## Capturas conservadas en el repositorio

- [Bastión V asegurado, 4/4](../evidence/player-audit-20261106/0300-click.png)
- [Guardado final recuperado](../evidence/player-audit-20261106/0303-observe.png)
- [Victoria en etapa 25](../evidence/player-audit-20261106/0241-click.png)
- [Error al construir](../evidence/player-audit-20261106/0110-click.png) y [error al asaltar](../evidence/player-audit-20261106/0202-click.png)
- [Terreno desaparecido parcialmente](../evidence/player-audit-20261106/0254-click.png)
- [Informe horizontal](../evidence/player-audit-20261106/0259-click.png) y [el mismo informe vertical](../evidence/player-audit-20261106/0261-resize.png)

Cierre operativo: sesión y proceso de navegador terminados; flujo de QA y permisos restaurados a su configuración anterior; reserva de Windows liberada. Se conserva el controlador acotado y sus pruebas como técnica reutilizable, sin sesión activa.
