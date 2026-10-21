# Eldoria — interfaz V3 · dirección artística aprobada

Estado vigente: **INTEGRADA EN `main` / VALIDADA NATIVAMENTE UNITY / PUBLICACIÓN WEB V3 PENDIENTE**. La decisión visual del propietario no se ha cambiado.
Base histórica de preparación: `main` fd3bb108c26716fec3aa530006107c764e62e91d. Integración real aprobada: PR #83, commit `79ed44f6465952903801394894a6f18f267354db`. El SHA histórico de base no es la versión actual.
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

## Pruebas necesarias antes de integrar

1. Verificar disponibilidad del responsable artístico y coordinar un cambio acotado sobre `main`, sin sobrescribir su escena ni reservar Windows simultáneamente.
2. Importar/compilar los componentes V3, hacer capturas A/B en las mismas cámaras y resoluciones y comprobar que el mundo sigue siendo protagonista.
3. Probar eventos normales, Fisura y Orín con el disparador real (sin simular contenido inexistente), cierre/restauración, raycasts y safe areas.
4. Repetir Bastión I–II, recursos, construcción, Mundo, diálogos y guardado/recarga.
5. Solo entonces fusionar e integrar en la versión jugable y publicar mediante el circuito verificado. No declarar V3 terminada sin captura real y pruebas.

## Estado real verificado (21-10-2026)

- **Integración canónica completada:** PR [#83](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/83), commit `79ed44f6465952903801394894a6f18f267354db`, preservando la escena `Valoria_City_World_UI.unity` y la integración ya validada del Engendro.
- **Certificación real en Unity:** run [39586129212](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39586129212), SUCCESS. 40/40 comprobaciones integradas Bastión I–II, 20/20 EditMode y 35/35 PlayMode. Prueba de pulsaciones uGUI, acción Construir, progreso/mejora, tiempo real, pantalla vertical/horizontal, objetivos, diálogos, entrada/salida del mundo y guardado/reapertura sin mutar los archivos de partida del propietario.
- **Evidencia visual:** 24 PNG reales de Unity y `integration-result.json` en el artefacto [12340695597](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39586129212/artifacts/12340695597). Los siete emparejamientos exigidos con V2 recuperan entre +21,8 y +33,1 puntos porcentuales de cámara jugable; límite mínimo +8 pp, todas las comprobaciones de encuadre aprobaron.
- **Revisión de capturas:** HUD azul tinta/oro compacto; la ficha V2 de gran tamaño ya no intercepta la ciudad; los botones y temporizadores se proyectan junto a sus objetivos y se actualizan desde el gateway real. Retrato y texto original de Aldric completos, avance narrativo existente.
- **Fisura/Orín:** la dirección 3 sobrenatural sigue reservada para eventos canónicos; Orín dispone de tratamiento de diálogo condicionado al hablante real. No se añadieron capítulos, reglas ni personajes ficticios para demostrarlo. No declarar un evento narrativo inexistente como probado.
- **Publicación distinta de integración:** no existe todavía un build WebGL de UI V3 con QA exacta y activación remota verificada en el canal Restricted. No sustituir la versión jugable existente ni declarar `HOSTED_GAMEPLAY_PASS` o `PHYSICAL_MOBILE_PASS` por el éxito nativo. Utilizar el circuito de entrega controlado `pipeline/playtest-delivery.json` solo con candidato WebGL nuevo certificado.

Este documento conserva la selección artística aprobada y especifica por separado los gates de integración, WebGL y publicación.
