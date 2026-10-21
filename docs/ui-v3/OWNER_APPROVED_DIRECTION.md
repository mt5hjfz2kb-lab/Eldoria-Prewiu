# Eldoria — interfaz V3 · dirección artística aprobada

Estado vigente: **UI V3 CINEMATOGRÁFICA INTEGRADA Y CERTIFICADA / WEBGL native245 PASS / SUBIDA RESTRINGIDA ACEPTADA, ACTIVACIÓN REMOTA PENDIENTE**. La decisión visual del propietario no se ha cambiado.
Base histórica de preparación: `main` fd3bb108c26716fec3aa530006107c764e62e91d. Base funcional integrada: PR #83, commit `79ed44f6465952903801394894a6f18f267354db`. El SHA histórico de base no es la versión actual.
El trabajo artístico de Valoria y el Engendro mantienen autoridad propia. La UI V3 se integró sobre la escena canónica en `main`, sin crear una segunda escena, sesión, guardado o rama canónica.

## Referencias elegidas por el propietario

- **Ciudad y mundo:** propuesta **1, limpia y elegante**. Azul tinta y oro envejecido, iconografía medieval, paneles compactos, máximo protagonismo del paisaje. No llenar la pantalla de marcos.
- **Diálogo habitual:** propuesta **1, cinematográfica**, retrato grande, nombre claro, texto legible sobre superficie tipo pergamino noble, avance narrativo con el mismo estado y las mismas líneas reales.
- **Fisura:** propuesta **3**, exclusivamente como alteración contextual cuando se produce un acontecimiento realmente relacionado con la Fisura. No dejar el violeta encendido permanentemente ni añadir una pestaña o moneda arcana ficticia.
- **Orín:** reservar el **diálogo 3** como tratamiento excepcional de personaje. No confundirlo con Lyra, no reescribir su lore ni activarlo sin el evento narrativo canónico.

## Invariantes

Conservar la referencia visual exacta de Valoria, pero **NO reintroducir las cuatro parcelas visibles** que el propietario eliminó posteriormente. No inventar edificios, recursos, monedas, héroes ni funciones a partir de los conceptos gráficos generados. El diseño de ciudad y diálogo debe probarse en horizontal y vertical con la escena actual, y no se aprueba por una ilustración.

## Contrato técnico V3

El módulo V3 es **presentación solamente**. Se monta sobre el único `RoyalHUD` y su `RoyalGoldV2Hud` existente, sin nuevo EventSystem, Canvas, Gateway, sesión, almacenamiento ni acciones falsas. La capa arcana es un estado **opt-in** que únicamente debe activar un evento narrativo real. Orín es un estado de diálogo diferenciado; ambos estados se restablecen al cerrar el evento.

Los controles existentes de `RoyalGoldV2Hud` deben seguir siendo pulsables, incluida la navegación Mundo y el objetivo compacto; la antigua ficha V2 permanece como adaptador de estado pero su panel visual queda oculto y reemplazado por controles contextuales V3 reales. El tamaño de fuente y el contraste del diálogo 1 se revisan con texto real largo, sin recorte.

## Criterios de integración y entrega

1. Verificar disponibilidad del responsable artístico y coordinar un cambio acotado sobre `main`, sin sobrescribir su escena ni reservar Windows simultáneamente.
2. Importar/compilar los componentes V3, hacer capturas A/B en las mismas cámaras y resoluciones y comprobar que el mundo sigue siendo protagonista.
3. Probar eventos normales, Fisura y Orín con el disparador real (sin simular contenido inexistente), cierre/restauración, raycasts y safe areas.
4. Repetir Bastión I–II, recursos, construcción, Mundo, diálogos y guardado/recarga.
5. Solo entonces fusionar e integrar en la versión jugable y publicar mediante el circuito verificado. No declarar V3 terminada sin captura real y pruebas.

## Estado real verificado (21-10-2026)

- **Acabado final integrado:** PR [#87](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/87), merge `4c32aa55b6a4bc10bcf822d8576bda1934603cac`. Pergamino, retrato auténtico ampliado, diálogo inferior y texto/Continuar originales. Corregidos el cambio de orientación, la etiqueta del botón y la restauración del menú normal.
- **Certificación Unity actual:** [39609160363](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39609160363), SUCCESS: **40/40** comprobaciones integradas, **20/20 EditMode y 37/37 PlayMode**, sin pruebas omitidas. Fuente `a1e0fb7b71632c175a396d8c3aadf2e71dee0a80`; [artefacto 12350227186](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39609160363/artifacts/12350227186). Capturas nativas horizontal/vertical revisadas, texto completo y botón dentro del pergamino.
- **Comprobación general completa:** [39611381736](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39611381736), SUCCESS: fuente, EditMode, PlayMode, ejecutable Windows y capturas de referencia. PlayMode utiliza la partida temporal existente; no presupone que la partida persistida del propietario conserve edificios sin construir. No se borran otros worktrees ni sus cachés mediante una limpieza global.
- **WebGL final exacto:** native245, fuente `e117087fbfbc8068913e754b5bf69293fed7826c`, build [39610358759](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39610358759). QA Chromium GPU [39612300155](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39612300155), **PASS**: Bastión I–II completo con reloj normal, acciones reales, navegación y encuadre en horizontal/vertical, fases de marcha y recompensas, cero errores de ejecución/red y recarga con partida idéntica. Hash antes/después: `a82f882c059b22ce4587efd2ca7697eba52eadb41223ee17f104e53844df79a5`.
- **Fallo web reparado y cubierto:** native244 fue rechazado por tres llamadas a la navegación táctil antigua al cerrar diálogos. El host integrado ahora declara que el HUD posee la navegación; los antiguos atajos por coordenadas y sus callbacks diferidos no reinterpretan Continuar. Regresión PlayMode incluida; native245 confirma **cero llamadas heredadas**. No se relajó la aserción del navegador. native244 nunca se publicó.
- **Transporte y entrega:** [39614581796](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39614581796): preparación y lector real Unity PASS; cinco archivos Unity originales verificados, contenido decodificado idéntico, una asignación por descarga y guardados sin cambios. ZIP `585451deffe03061ffa265845e1301921303b23d31c1f60d01da879f41a97f33`; versión de entrega `native245-585451deffe0`. itch.io aceptó el build **2177569**, todavía procesándose en la última consulta del 21-10 a las 16:13:34 Europe/Madrid. El paso publish quedó en error por su espera corta de activación; la subida fue aceptada. **No repetirla por ese error.** El canal seguía sirviendo native242; una subida aceptada no equivale a activación.
- **Acceso alojado bloqueado aquí:** el login de itch.io muestra la verificación Cloudflare y persiste tras un único reintento. Se detuvieron los intentos; no se solicitaron credenciales ni se cambió la privacidad. La página Restricted, su carga real y su guardado alojado **no están certificados por este navegador**. No declarar `HOSTED_GAMEPLAY_PASS`, prueba física móvil ni calidad artística global por los pases anteriores.
- **Operación histórica preservada:** el cambio compartido de SlicePresenter activó un flujo SHARP/parcelas anterior a la base integrada. Su ejecución posterior quedó desactivada y los disparadores compartidos retirados; fuentes y evidencias históricas conservadas. El nuevo run 39609616535 omite correctamente su producción; no se reactiva ninguna parcela.
- **Fisura/Orín:** la dirección 3 sigue reservada a eventos y hablantes canónicos. No se añadieron capítulos, personajes o eventos ficticios para demostrarla.

## Base funcional histórica

PR [#83](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/83), commit `79ed44f6465952903801394894a6f18f267354db`, integró el HUD contextual sobre la escena única `Valoria_City_World_UI.unity`. Su run 39586129212 pasó 40 comprobaciones, 20 EditMode y los 35 PlayMode de entonces. Los siete pares con V2 recuperaron entre +21,8 y +33,1 puntos porcentuales de cámara jugable; son evidencia de esa integración, no la identidad del WebGL final.

El estado de entrega vigente y su candidato exacto se registran en `pipeline/playtest-delivery.json`. Este documento conserva la selección visual y distingue integración, QA, subida, activación y comprobación alojada.
