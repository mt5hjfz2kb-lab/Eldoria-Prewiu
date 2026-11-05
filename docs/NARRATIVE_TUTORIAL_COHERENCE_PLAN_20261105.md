# ELDORIA — Coherencia narrativa y tutorial de Unity
Fecha de cierre: 5 de noviembre de 2026.

## Resultado
La PR #160 completa el trabajo narrativo I–V sobre el combate V4.1 ya integrado por PR #134 y la interfaz de PR #136. Los textos, condiciones y tutoriales se ejecutan en la escena canónica de Unity. El prototipo web conserva su papel de referencia y no recibe cambios.

## Correcciones implementadas
- Orden narrativo derivado de la partida: el cierre de la fisura y la incorporación de Lyra preceden al consejo, incluso al cargar una partida avanzada. No se reproduce una introducción obsoleta de frontera después del cierre.
- Conocimiento progresivo: primero la investigación, después el contacto y finalmente la historia del hermano. La fisura local no se confunde con la Brecha principal; el prólogo sitúa la Brecha en el suelo junto a Valoria.
- Lectura persistente: una conversación queda reconocida al completar su última página, también si se abre manualmente desde Historia. Abrir la ventana no completa la lectura.
- Compatibilidad de guardados: solo una serialización histórica sin SeenNarrative importa las preferencias antiguas, una vez mediante el gateway. Una lista moderna vacía significa pendiente de leer y permanece así tras reiniciar. No se borran preferencias, partidas, recursos ni recompensas.
- Ritmo contextual: las escenas automáticas esperan mientras hay una pantalla, buzón o contexto de construcción abierto, y mantienen 1,5 segundos entre escenas.
- Consejo IV: distingue una prioridad pendiente de otra ya elegida. La disponibilidad de otras mejoras no se presenta como permiso para repetir la elección.
- Tutoriales: 14 ayudas contextuales en español e inglés, presentación de sistemas realmente desbloqueados y mensajes de primer uso de Vigilantes, Exploradores y buzón basados en acciones reales.
- Nomenclatura: etiquetas de Bastión coherentes con I–V; no se presentan etapas VI–X todavía no implementadas.
- Compatibilidad: las reglas narrativas permanecen separadas de GameState y no sustituyen la resolución de combate, marchas ni recompensas reconciliadas.

## Validación
- PR #160 integrada en main mediante 65ffa83f9e5f7e1a37cb6d0db2ec352539afb169.
- Candidata exacta a52a115753a421bf906d1891e69db09c1abab1c0: run 42061039754 SUCCESS, 128/128 EditMode, 74/74 PlayMode, cero fallos/omisiones. Recorrido continuo I–V con 208 comprobaciones, tres marchas y guardado/recarga.
- Artefacto nativo 13434755044 descargado y SHA256 verificado: ade882d1adb9b37cf70a350646c2cab0fed49e92f4a8c358d8f92d68bd04933e.
- El árbol Unity de candidata y merge es idéntico: 40b451eb1e76eda8471aebc283f630e2fffac968; también coinciden los árboles de herramientas y workflows.
- Pruebas posteriores del merge: run 42065390835, 128/128 EditMode y 74/74 PlayMode sin fallos ni omisiones. XML descargados del artefacto 13433024751 y SHA256 verificado: 4439f26b2b7d6d06aa2703a220195af2561572b21eb98bfb78491ad73d5e00d0.
- Controles V4.1 posteriores en main: funcional PASS; artefacto 13433644803 descargado, SHA256 3b8a89825fe84e771e5c45b6744d5feadfacb301e05b0761bab36af796288bec verificado. Veto de recursos antiguos PASS: 550 dependencias comprobadas, cero recursos o cargadores rechazados.
- Diálogos A/B posteriores: PASS con source exacto 65ffa83f9e5f7e1a37cb6d0db2ec352539afb169 y 48 capturas. Artefacto 13436220048 descargado, SHA256 83d74f5d5e8d2c2007f31c1c72a4c01c22a8f975759bcb22ddd9f9105e5c2594 verificado.
- Run general posterior 42065390835 completado SUCCESS, incluida compilación Windows. Artefacto privado 13434039453: 1.507.734.907 bytes; digest publicado por GitHub 2b9bc7c84640c43a5f18fcabcf47785f9dfe8d524514e33f37e75942cba0bea8 (archivo de build no redescargado; la verificación local de bytes corresponde a los XML y evidencias indicados arriba).
- Certificación UI posterior 42065391163 completada SUCCESS: 128/128 EditMode, 74/74 PlayMode, cero fallos/omisiones y recorrido I–V de 208 comprobaciones. Artefacto 13437038023 descargado y SHA256 verificado: 0171d2203bd70eea72433c78174a54c5339335252804375b325a5d543c1d6c5b. Recursos liberados después de completar todas las ejecuciones de este trabajo.
- Dominio narrativo 42061039845, dominio V4.1 42061042587, regresión general 42061039837 y controles de metadatos, arquitectura y gobernanza SUCCESS.


## Revisión de capturas y límites
Se revisaron capturas reales de Unity de Aldric, Lyra, el tutorial del aserradero y los estados finales en vertical/horizontal. La certificación posterior generó además 48 capturas A/B en 390×844, 844×390, 900×1600 y 1600×900. Incluyen diálogos cortos/largos de Aldric y Lyra; Orín es una muestra de estilo claramente marcada como QA, no una escena de campaña. Se revisaron también las capturas de texto largo a tamaño móvil de ese artefacto final. Los textos revisados caben en los paneles y el botón Continuar permanece accesible. Una captura inicial del prólogo coincide con su transición de aparición; no sirve por sí sola para valorar el contraste final.
La aceptación artística previa sigue siendo provisional: esta corrección no reconstruye la ilustración ni afirma una nota visual nueva. No se ha realizado una sesión humana a ritmo normal ni pruebas en un teléfono físico. No se publica APK, WebGL ni itch.io.
El recorrido automatizado respeta clics, raycasts y pausas de presentación; acelera los temporizadores de juego únicamente en la partida desechable de pruebas. No equivale a jugar durante todos los tiempos de espera reales.

## Técnicas conservadas
1. La lectura pertenece al guardado, no a una preferencia global del dispositivo. Detectar la serialización histórica antes de normalizar evita confundir una partida moderna vacía con un guardado anterior al ledger.
2. Probar el contexto de construcción seleccionado antes de esperar un diálogo: cerrar ese contexto en el probe, sin desactivar la protección del jugador.
3. Conservar pruebas de conocimiento y decisiones previas: avanzar el Bastión no debe borrar un desenlace pendiente ni repetir una elección del consejo.
4. La procedencia de la evidencia se acredita con el SHA exacto del checkout y el run. El campo source de integration-result.json conserva un identificador histórico incrustado y no debe usarse como prueba del código ejecutado.

## Historial relevante
- La primera tentativa en v0220/qa fue revertida; no certifica Unity.
- La auditoría inicial quedó pendiente de reconciliación con combate y titularidad. El relevo autorizado resolvió ambas y continuó la PR existente.
- Certificación intermedia 566b2ea7486b54db1e464fff8e47943793dd2368: run 42059370768 SUCCESS, 128 EditMode y 74 PlayMode. Artefacto 13430633400 descargado y SHA256 verificado 2bdf09ba9ed311e72b053771b028386ab9ac3d39b19cff68548994f806560be7.
- Candidata final a52a115753a421bf906d1891e69db09c1abab1c0 añade la protección de recarga para preferencias históricas. Su evidencia definitiva y la comprobación posterior a la integración figuran arriba.
