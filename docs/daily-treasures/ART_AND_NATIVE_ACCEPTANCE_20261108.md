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
3. **Prueba de componente:** `.github/workflows/daily-treasures-native.yml` requiere SHA idéntico y nueve recursos originales. Debe producir **52 capturas Unity**: 4 vistas del botón de acceso sin recompensa, 28 del calendario, cofres especiales y cartas de recursos **realmente cobrados** (días 4 y 7, cuatro resoluciones), 4 fotogramas adicionales de premios ilustrados en vuelo en los dos formatos móviles y 16 capturas **exclusivamente artísticas** para cuatro estilos diferenciados (Lyra, Bastión, Vigilantes y Exploradores, en vertical/horizontal, apertura y resultado). Estas 16 imágenes NO demuestran haber obtenido un héroe, mejorado un edificio o ganado una batalla; el informe las marca `coversReward=false` y `preserved=false`. Se requieren además **56 eventos de puntero**, incluidos los del Canvas independiente de Vigilia, y **8 reclamaciones reales** con `LocalGateway` sobre partidas descartables. Cada PNG tendrá SHA256, dimensiones y sello de la revisión exacta. **Esta certificación no se ha ejecutado todavía.**
4. **Revisión artística independiente:** comparar las capturas con el acabado ya aceptado en la previsualización, revisando relieve, texturas, composición, iluminación, claridad, coherencia con la ciudad y el ritmo de la animación. No aprobar el cofre procedural como solución equivalente.
5. **Partida jugable:** dentro de ciudad nativa, comprobar acceso, tutorial sin interrupciones, días 4 y 7, hitos Vigilantes, cierre/salto, guardado, buzón, reinicio, cambios de orientación y carga WebGL. La PR173 «Vigilia de la Brecha» apaga el Canvas antiguo y utiliza uno independiente. PR181 ya contiene una adaptación reversible a su `Safe composition` sin duplicar el HUD; esa convivencia está programada y su simulación se ha añadido a las pruebas nativas, **pero todavía no está aprobada dentro de Unity**.
6. **Dispositivos reales:** Android/iOS, tamaño de botones, Safe Area, memoria, FPS, sonido, ausencia de reinicios y estabilidad. Balance de recompensas para Bastión I–V antes de activar producción.
7. **Aprobación:** solo fusionar o publicar tras resolver cualquier defecto de esas capturas y pruebas. La versión beta congelada no se modifica desde este workstream.

## El trabajo artístico también cubre acontecimientos auténticos
- Recompensa básica: icono/saldo sin interrumpir la partida. Día 4: cofre especial. Día 7: cofre legendario con mayor intensidad visual pero los mismos recursos concedidos por el gateway.
- Capítulo de Vigilantes completado: ceremonia legendaria sobre el informe real, sin botín creado por la interfaz. Primer descubrimiento de una familia de exploradores: ceremonia especial una sola vez, después del informe real.
- Primera llegada de Lyra y Bastiones II/V realmente terminados: cola visual no intrusiva respaldada por correos existentes. Nunca interrumpe el tutorial ni reinterpreta resultados de otras partidas, y no reproduce ceremonias retroactivas al iniciar sesión tras una ausencia.
- Temporización por `DailyTreasureRevealTimeline` y nueve recursos originales versionados. La calidad mínima es la previsualización ya aprobada por el propietario; el acabado real exige comparar capturas y sonido dentro de Unity.
- Las cantidades se han evaluado sobre el perfil OWNER_I_II. Siete días consecutivos: 330 madera, 300 piedra, 230 comida y ocho minutos de aceleradores; 30 días: 1.380 madera, 1.240 piedra, 945 comida y 32 minutos. Valores **provisionales** hasta pruebas humanas.


## Medidas artísticas posteriores al primer paquete de producción
- El acabado diario se apoya en los mismos **nueve PNG originales**: cofre de roble y bronce abierto/cerrado, marcos texturizados e iconografía de materiales. Se ha separado el flujo de recurso sencillo del cofre especial: el premio normal utiliza una ventana ilustrada **compacta y centrada**, y el especial/legendario reserva el mayor espacio visual al cofre. No se presenta un marco gigante vacío para una sola bolsa de recursos.
- Lyra, Bastiones II/V, guardianes y primeras familias de descubrimientos emplean el mismo cofre de calidad como base y una iluminación secundaria propia, tenue y acorde a la fantasía oscura: oro cálido para reconstrucción, bronce para Lyra, violeta para Vigilantes y verde mineral para Exploradores. Ninguna de estas variantes inventa cartas, cristales, porcentajes ni regalos. El título se toma del acontecimiento real y el detalle íntegro permanece en el buzón.
- Al mostrar un hito sin recompensa nueva, la descripción se reduce a una frase auténtica y legible, no a una copia diminuta del informe completo. El sonido sigue provisional hasta escucharlo en la build; la calidad no se aprobará por disponer únicamente de tres tonos sintetizados.
- Esta dirección se ha contrastado en maquetas locales independientes de Unity y mediante aritmética de solapes sobre ocho tamaños. **No está verificada su reproducción en Unity.** Si falla por escalado, ausencia de la tipografía real o mala integración con el escenario, se cambiará el método y se repetirá la captura.

## Estado
**9/9 PNG ORIGINALES EN PR / FUENTE Y DOMINIO VERIFICADOS / PROTOTIPO VISUAL EN NAVEGADOR TESTADO / NO EXISTE AÚN UNA CAPTURA NATIVA DE UNITY / ART_APPROVED=FALSE / NO MERGE / NO RELEASE.**

## Revisión de calidad pre-Unity: salto artístico integral
- El acceso de la ciudad ya no es un botón de texto: utiliza **el mismo cofre ilustrado que la ceremonia**, grabado nine-slice, pequeño distintivo de tributo realmente disponible y luz cálida animada discreta. La primera disposición del medallón se calcula tanto en Build como en el primer Update, para impedir que una pantalla nueva muestre un botón de tamaño cero.
- El cofre grande no termina en un simple mensaje de texto: los materiales y aceleradores acreditados aparecen como **tarjetas ilustradas individuales**, y en los premios relevantes emergen desde el cofre con un arco de .64 s y entrada escalonada de .13 s. Los premios normales usan una transición más discreta de .34 s. Los datos proceden exclusivamente del resultado ya comprometido en LocalGateway. El código de movimiento se ha aislado en `DailyTreasurePrizeFlight.cs` y tiene una batería independiente de pruebas deterministas.
- `tools/daily-treasures/audit_native_art.py` es una segunda puerta de calidad de capturas: usa los PNG auténticos de Unity y sus SHA256, rechaza pantallas planas, imágenes idénticas antes/después de la apertura y estilos de evento que no se diferencian; produce láminas comparativas de recompensas, ceremonias y el medallón de entrada para revisión humana. Sus umbrales solo detectan fallos evidentes: **no certifican automáticamente un acabado de 8/10 ni una experiencia «guau»**.
- La prueba final exige **52 PNG** (4 vistas del medallón sin cobro, 28 estados de recompensas realmente reclamadas, 4 fotogramas intermedios de vuelo de premios y 16 vistas de estilos de hitos que no afirman concesiones). El último comparador valida por separado que esos fotogramas intermedios no sean una copia del resultado inmóvil. Se mantienen 56 interacciones reales de puntero y ocho pagos confirmados con estado temporal.
- La certificación visual jamás marcará `artApproved=true` de forma automática. Hace falta inspección de las imágenes dentro del juego, iluminación, audio, proporciones y fluidez en móvil.

### Resultado alcanzado frente al objetivo
**Investigado e implementado en PR181:** recursos originales premium, accesos, interfaces, cofres y tarjetas animadas, pruebas de integridad y comparador visual. **Probado sin Unity:** reglas, entregas reales, geometría, compilaciones de dominio y transparencia/hashes de arte. **Pendiente de probar:** compilación y renderizado auténtico de Unity, continuidad en la ciudad, interacción real física y evaluación artística frente a la referencia aprobada. No confundir este avance técnico con un aprobado visual.
