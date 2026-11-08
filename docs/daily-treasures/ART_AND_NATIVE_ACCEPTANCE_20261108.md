# Tesoros de Valoria — expediente artístico y criterios de aceptación

**Corte:** 8 de noviembre de 2026 · **PR:** #181 · **presupuesto extra:** 0 € · **calidad mínima:** referencia de cofre ilustrado aprobada por el propietario. **No es un pase artístico nativo.**

## Decisión: cambiar el enfoque, no acumular remiendos
El cofre dibujado exclusivamente con geometría uGUI resultó demasiado plano en una reproducción **local** (no Unity). Se conserva como recurso de emergencia editable; **no alcanza por sí mismo la calidad objetivo y no cuenta para cerrar el trabajo**. El enfoque de producción utiliza ilustraciones originales integradas como sprites, marcos y botones originales en nine-slice y una línea de tiempo animada independiente que solo presenta premios ya guardados.

## Nueve recursos gráficos **importados como PNG binarios reales en GitHub**
La rama `feature/eldoria-daily-treasures-20261108` contiene estos archivos dentro de `Unity/Assets/Eldoria/DailyTreasures/Resources/DailyTreasureArt/`:
- **Cofre:** `treasure_closed_512.png`, `treasure_opened_512.png`. Son los dos estados consistentes de un único cofre de roble oscuro, hierro forjado, bronce envejecido y resplandor ámbar; SHA256 de cada imagen en `source-manifest.json`.
- **Marcos originales:** `valoria_frame_main_512.png`, `valoria_frame_card_256.png`, `valoria_frame_button_512x128.png`. Tienen bordes nine-slice grabados (46/31/24 px) e importador Unity con GUID propio y transparencias.
- **Cuatro iconos ilustrados:** `resource_wood.png`, `resource_stone.png`, `resource_hourglass.png`, `resource_food.png`. También tienen GUID, transparencia y hash propios.

La extracción e incorporación no dependen de un proveedor de assets ni de una URL externa. Los nueve PNG se transferieron como Git blobs y se verificó su SHA de origen. El código usa `Resources.Load<Sprite>` y `Image.Type.Sliced`; la geometría anterior queda como fallback **no aprobable artísticamente**. La previsualización independiente en HTML contiene el mismo cofre y fue comprobada localmente a ocho tamaños; **no equivale a una captura Unity**.

**Proveniencia:** imágenes creadas para este trabajo y preparadas/retocadas en la conversación. No se han tomado recursos stock de terceros. El diseño del marco se realizó con una técnica original de textura, relieve y ruido sutil de bajo coste. No compartir fuentes tipográficas del contenedor.

## Calidad de la animación
`DailyTreasureRevealTimeline` determina una ceremonia visual de unos 2,35 segundos: aparición, expectativa breve, luz cálida para ocultar la transición de pose, apertura, brasas contenidas y lista legible de premios. El botón «Saltar» nunca revierte ni vuelve a conceder recompensas. El sonido de bronce/madera es temporal y todavía requiere escucha dentro del juego.

En `DailyTreasurePanel`, una reclamación sucede y se guarda primero en el `LocalGateway`, y solo después comienza la ceremonia. Abrir/leer/borrar el correo no paga un premio nuevo. También hay un gancho **solo visual** para completar el capítulo real de Vigilantes, sin crear un botín no existente.

## Qué se ha ejecutado realmente (fuera de Unity)
- Pruebas originales C# de calendario/racha/UTC/ciclos, desbordamiento y timeline de 2,35 s; repositorio CI.
- Pruebas de `LocalGateway` real para siete días, premios exactos, reintentos, cierre, recarga, correo y comandos duplicados.
- Ocho verificaciones geométricas de tarjetas, objetivos táctiles y tamaño de pantalla.
- Verificación automática de los bytes PNG SHA256 de origen, GUIDs, modo Sprite y bordes nine-slice en la propia rama. Pruebas de análisis sintáctico de C#.
- Laboratorio web autónomo: 390×844, 844×390, 900×1600, 1600×900, 375×667, 667×375, 1024×768 y 768×1024. Se inspeccionaron visualmente capturas de calendario y cofre abierto y se comprobó botón + animación + restablecimiento. El navegador usa los mismos gráficos originales, pero **su UI NO es el Canvas de Unity**.

## Puertas de calidad que aún deben superarse en Unity
1. **Runner:** `windows-runner-heavy` permanece reservado en la rama de beta PR179 según registro canónico del 8/11. No apropiarse ni interrumpir ese recurso. Tras liberación formal, reclamarlo por control de propietario antes de lanzar workflow nativo.
2. **Cargar de verdad:** importar los nueve Sprite en Unity 6000.3.23f1 sin errores de compilación, referencias rotas, fuentes ilegibles ni materiales perdidos.
3. **Prueba de componente:** `.github/workflows/daily-treasures-native.yml` requiere SHA idéntico y recursos originales. Producir las **16 capturas Unity obligatorias** (calendario, cofre cerrado, apertura central y abierto × cuatro resoluciones), más **8 capturas adicionales de día 4** (24 PNG en total), **40 eventos de puntero** (incluidos 8 de convivencia con Vigilia) y **8 reclamaciones reales sobre estados descartables**. Verificar los 9 SHA256 de las ilustraciones, los 24 SHA256 de capturas y dimensiones PNG, sin convertir un PASS técnico en aprobación artística. La prueba aún no ha corrido y no se puede dar por aprobada de antemano.
4. **Revisión artística independiente:** comparar las capturas con el acabado ya aceptado en la previsualización, revisando relieve, texturas, composición, iluminación, claridad, coherencia con la ciudad y el ritmo de la animación. No aprobar el cofre procedural como solución equivalente.
5. **Partida jugable:** dentro de ciudad nativa, comprobar acceso, tutorial sin interrupciones, días 4 y 7, hitos Vigilantes, cierre/salto, guardado, buzón, reinicio, cambios de orientación y carga WebGL. La PR173 «Vigilia de la Brecha» tiene un HUD propio aún por integrar, y no debe duplicarse.
6. **Dispositivos reales:** Android/iOS, tamaño de botones, Safe Area, memoria, FPS, sonido, ausencia de reinicios y estabilidad. Balance de recompensas para Bastión I–V antes de activar producción.
7. **Aprobación:** solo fusionar o publicar tras resolver cualquier defecto de esas capturas y pruebas. La versión beta congelada no se modifica desde este workstream.

## Estado
**9/9 PNG ORIGINALES EN PR / FUENTE Y DOMINIO VERIFICADOS / PROTOTIPO VISUAL EN NAVEGADOR TESTADO / NO EXISTE AÚN UNA CAPTURA NATIVA DE UNITY / ART_APPROVED=FALSE / NO MERGE / NO RELEASE.**
