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
3. **Prueba de componente:** `.github/workflows/daily-treasures-native.yml` requiere SHA idéntico y nueve recursos originales. Debe producir **52 capturas Unity**: 4 vistas del medallón sin cobro, **8 calendarios anteriores a la reclamación sin premios acreditados** y 20 estados del cofre y cartas **tras ocho reclamaciones auténticas** (días 4 y 7, cuatro resoluciones), 4 fotogramas adicionales de premios ilustrados en vuelo en los dos formatos móviles y 16 capturas **exclusivamente artísticas** para cuatro estilos diferenciados (Lyra, Bastión, Vigilantes y Exploradores, en vertical/horizontal, apertura y resultado). Estas 16 imágenes NO demuestran haber obtenido un héroe, mejorado un edificio o ganado una batalla; el informe las marca `coversReward=false` y `preserved=false`. Se requieren además **56 eventos de puntero**, incluidos los del Canvas independiente de Vigilia, y **8 reclamaciones reales** con `LocalGateway` sobre partidas descartables. Cada PNG tendrá SHA256, dimensiones y sello de la revisión exacta. **Esta certificación no se ha ejecutado todavía.**
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
- La prueba final exige **52 PNG** (4 vistas del medallón sin cobro, 8 calendarios aún no cobrados, 20 estados después de cobrar, 4 fotogramas intermedios de vuelo de premios y 16 vistas de estilos de hitos que no afirman concesiones). El último comparador valida por separado que esos fotogramas intermedios no sean una copia del resultado inmóvil. Se mantienen 56 interacciones reales de puntero y ocho pagos confirmados con estado temporal.
- La certificación visual jamás marcará `artApproved=true` de forma automática. Hace falta inspección de las imágenes dentro del juego, iluminación, audio, proporciones y fluidez en móvil.

### Resultado alcanzado frente al objetivo
**Investigado e implementado en PR181:** recursos originales premium, accesos, interfaces, cofres y tarjetas animadas, pruebas de integridad y comparador visual. **Probado sin Unity:** reglas, entregas reales, geometría, compilaciones de dominio y transparencia/hashes de arte. **Pendiente de probar:** compilación y renderizado auténtico de Unity, continuidad en la ciudad, interacción real física y evaluación artística frente a la referencia aprobada. No confundir este avance técnico con un aprobado visual.


## Dirección artística ampliada: otras escenas al nivel del cofre
El panel ahora utiliza los materiales originales del cofre también para las ceremonias de capítulos, exploración, incorporación de Lyra y reconstrucción, pero las diferencia por **contexto pictórico real**:
- **Lyra:** `EldoriaV41/heroes-hall-illustrated-v2`, sala de héroes ya existente en el juego, sutilmente integrada detrás del cofre; no se ha inventado un retrato nuevo del personaje.
- **Exploradores:** `EldoriaRetention/scouts-painterly-v2`, el mapa pintado del proyecto, con un tratamiento tenue de material/presentación y encuadre sin deformar.
- **Vigilantes:** `EldoriaRetention/vigilants-painterly-v2`, escena panorámica ya presente en Eldoria, con una luz ceremonial distintiva.
- **Bastión:** conserva la ciudad auténtica bajo el modal; no reemplaza su edificio real por una ilustración de nivel incorrecto.

Las tres fuentes se cargan como `Texture2D`/RawImage (sus Unity importadores existentes son tipo 0, no Sprites), se recortan al formato del panel respetando proporciones y **nunca interceptan clics**. Se eliminan las referencias del `RawImage` al cerrar la ceremonia para que el panel opcional no mantenga innecesariamente recursos pesados en WebGL. Las comprobaciones de integridad y rutas de source art pasan por `check-scenic-art.mjs`; el flujo nativo deberá demostrar por separado que cada paisaje realmente se ve y que desaparece al cerrar.

## Sonido original mejorado, sin muestras de terceros
Se abandonó el pitido sencillo y ahora hay tres sonidos originales producidos de forma determinista por `DailyTreasureFoley.cs`: pestillo de roble/hierro, apertura cálida de bronce y variante grandiosa con resonancia más profunda para hitos legendarios. Incluyen golpe corto, ruido filtrado, parciales metálicos inarmónicos y una cola breve. Los buffers PCM se generan **una vez por panel**, a 22.050 Hz, sin distribuir música, grabaciones ni dependencias pagadas. Se validan numéricamente longitud, amplitud, silencio de salida, ausencia de DC/clipping y repetibilidad con C# puro. La mezcla exacta **todavía exige escucha en la compilación nativa**: no confundir forma de onda correcta con experiencia auditiva aprobada.

## Regla de aceptación para cada escena
No basta con un tintado de color. El test de componente deberá comprobar `HasMilestoneScenery`/nombre de la textura para Lyra, Exploradores y Vigilantes, así como la ausencia deliberada de paisaje falso para Bastión. El revisor humano deberá comparar las capturas en vertical/horizontal, legibilidad, cofre y entorno pintado, contraste y animación. Las 52 capturas actuales del plan son **objetivo de test, no imágenes ya obtenidas**.

## Corrección preventiva de una causa real de fallo nativo — ensamblados Editor / Player
La PR tenía `Eldoria.DailyTreasures.UI.asmdef` en la carpeta madre, con `Editor/DailyTreasureNativeEvidence.cs` debajo. Según la documentación oficial Unity 6000, al crear un `.asmdef` superior la carpeta especial `Editor` **deja de aislar automáticamente** sus scripts de la DLL de producción. Sin un ensamblado Editor propio, una referencia a `UnityEditor` puede contaminar la DLL de Android/WebGL, causar errores de compilación o impedir una captura nativa.

**Implementado:** `Editor/Eldoria.DailyTreasures.Editor.asmdef` con `includePlatforms: ["Editor"]`, dependencias explícitas a `Eldoria.DailyTreasures.UI`, `Eldoria.Domain`, `Eldoria.Application` y `Unity.ugui`, y GUID estable. La comprobación `tools/daily-treasures/check-assembly-isolation.mjs` confirma la separación sin lanzar Unity. El flujo Ubuntu pasa, pero **no equivale a una compilación real del proyecto Unity**.

Referencia técnica: https://docs.unity.com/en-us/engine/6000.0/manual/programming-environment/script-compilation/assembly-definition-files/assembly-definitions-creating

También se sustituyó el checkout Windows de `actions/checkout@v4` (podía caer en descarga completa del repositorio de muchos GB si el runner carecía de Git) por la técnica segura ya utilizada por Vigilia: Git instalado existente, `git fetch --filter=blob:none --depth=1`, checkout sparse exact SHA y exclusión de arte histórico voluminoso. `validate_native_workflow.py` analiza el YAML real y rechaza regresiones de esta protección, además de comprobar las 52 capturas, 56 eventos de puntero y ocho reclamaciones realmente exigidos por el workflow. Pruebas de fuente superadas; todavía falta ejecución nativa por la reserva de Windows.


## Revisión de calidad perceptiva sin PC — fuentes, iconos reales y tipografía

**Problema demostrado y corregido:** los cuatro iconos iniciales de 128 px eran recortes de otra composición y arrastraban fondos circulares, fragmentos de cifras y siluetas recortadas. Los primeros reemplazos de 256 px eran más limpios, pero demasiado planos respecto al cofre de madera/hierro ilustrado. Se sustituyeron por cuatro recursos **auténticos con acabado pictórico**, recortados y matizados con canal alfa del arte original para madera, piedra, comida y reloj de arena. No se ha modificado el cofre aprobado. La rama conserva los cuatro antiguos solo como fallback técnico.

**Integridad probada:** ahora hay **13 PNG binarios** versionados (2 estados de cofre, 3 marcos, 4 iconos históricos y 4 nuevos iconos pictóricos). El cambio de arte sustituyó los bytes reales de los cuatro iconos; se detectó y corrigió un manifiesto SHA256 que todavía señalaba sus versiones geométricas, bloqueando correctamente CI hasta verificar los nuevos hashes exactos. `source-manifest.json`, `verify_imported_art.py` y `check_clean_icon_quality.py` exigen identidades y metadatos correctos.

**Legibilidad móvil fuera de Unity:** se añadió `mobile_visual_preflight.py` que renderiza los **PNG del repositorio** a 32, 48 y 64 píxeles efectivos sobre una superficie de piedra oscura. Comprueba área visible, dimensiones de silueta, contraste y detalle de bordes; exporta una hoja de contacto accesible como artefacto de GitHub Actions. No usa OCR ni otorga puntuación artística. El reloj de arena conserva su silueta alta y estrecha, sin deformarlo artificialmente. La inspección visual de los originales indica que estos iconos reales son coherentes con materiales del cofre, pero la prueba perceptiva y de GPU de Unity sigue pendiente.

**Texto y cambio de orientación:** la prueba nativa preparada incorpora `VerifyCriticalTypography`, que consulta `UnityEngine.UI.Text.preferredHeight` del generador tipográfico real. Detecta textos importantes cortados por exceso de líneas, incluidas cantidades, botones y ceremonias. En horizontal se corrigió concretamente el recibo de dos líneas que no cabía bajo el cofre; pasa a una línea, manteniendo la información detallada en tarjetas y buzón. Se añadió un ensayo que **gira la pantalla con un premio ya cobrado**, verifica la adaptación del texto, restaura la orientación y confirma que los cuatro premios y el saldo sobreviven sin un segundo pago. El ensayo está implementado en la prueba nativa, **no ejecutado aún**.

**Coherencia con el verdadero PR173 actual:** la interfaz de Vigilia pasó a utilizar `OwnerControls/profile_bar`, `resource_capsule` y `upgrade_button`, mientras sigue conservando las superficies `PremiumSurfaces/slate_resource` y `burgundy_action`. La comprobación previa confundía «no estar referenciado por el nuevo HUD» con «el recurso ya no existe», dando un falso rechazo. Se corrigió para consultar el HEAD actual de PR173 y verificar por separado los materiales **que usa realmente** Vigilia y la existencia, modo Sprite y bordes nine-slice de las dos superficies compatibles que reutiliza Tesoros. En particular **no** se reutiliza el botón de mejoras con un martillo preimpreso para reclamar premios de fidelidad.

**Evidencia:** GitHub Actions, ejecución de fuente `42596929396`, correcta para el commit `ab736cbbd7a45f7e68eef45d9511a1d134b62a3c`; validación de origen, integridad, escena, API actual y pruebas de legibilidad. Los commits posteriores de texto/rotación y documentación deben conservar sus propios checks verdes. Ninguna de estas pruebas demuestra aún el efecto «guau» dentro de Unity.

