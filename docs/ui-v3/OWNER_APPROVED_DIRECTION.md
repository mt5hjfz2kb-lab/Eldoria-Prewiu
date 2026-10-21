# Eldoria — interfaz V3 · dirección artística aprobada

Estado: **APROBADA COMO DIRECCIÓN / NO INTEGRADA / NO VALIDADA EN UNITY**.
Base: `main` fd3bb108c26716fec3aa530006107c764e62e91d.
Trabajo artístico de Valoria actualmente reservado por Work. Esta rama contiene exclusivamente preparación de UI y NO es una nueva línea de producción canónica.

## Referencias elegidas por el propietario

- **Ciudad y mundo:** propuesta **1, limpia y elegante**. Azul tinta y oro envejecido, iconografía medieval, paneles compactos, máximo protagonismo del paisaje. No llenar la pantalla de marcos.
- **Diálogo habitual:** propuesta **1, cinematográfica**, retrato grande, nombre claro, texto legible sobre superficie tipo pergamino noble, avance narrativo con el mismo estado y las mismas líneas reales.
- **Fisura:** propuesta **3**, exclusivamente como alteración contextual cuando se produce un acontecimiento realmente relacionado con la Fisura. No dejar el violeta encendido permanentemente ni añadir una pestaña o moneda arcana ficticia.
- **Orín:** reservar el **diálogo 3** como tratamiento excepcional de personaje. No confundirlo con Lyra, no reescribir su lore ni activarlo sin el evento narrativo canónico.

## Invariantes

Conservar la referencia visual exacta de Valoria, pero **NO reintroducir las cuatro parcelas visibles** que el propietario eliminó posteriormente. No inventar edificios, recursos, monedas, héroes ni funciones a partir de los conceptos gráficos generados. El diseño de ciudad y diálogo debe probarse en horizontal y vertical con la escena actual, y no se aprueba por una ilustración.

## Contrato técnico V3

El módulo V3 es **presentación solamente**. Se monta sobre el único `RoyalHUD` y su `RoyalGoldV2Hud` existente, sin nuevo EventSystem, Canvas, Gateway, sesión, almacenamiento ni acciones falsas. La capa arcana es un estado **opt-in** que únicamente debe activar un evento narrativo real. Orín es un estado de diálogo diferenciado; ambos estados se restablecen al cerrar el evento.

Los controles existentes de `RoyalGoldV2Hud` deben seguir siendo pulsables, incluida la navegación Mundo, objetivo compacto y ficha contextual. El tamaño de fuente y el contraste del diálogo 1 se revisan con texto real largo, sin recorte.

## Pruebas necesarias antes de integrar

1. Verificar disponibilidad del responsable artístico y coordinar un cambio acotado sobre `main`, sin sobrescribir su escena ni reservar Windows simultáneamente.
2. Importar/compilar los componentes V3, hacer capturas A/B en las mismas cámaras y resoluciones y comprobar que el mundo sigue siendo protagonista.
3. Probar eventos normales, Fisura y Orín con el disparador real (sin simular contenido inexistente), cierre/restauración, raycasts y safe areas.
4. Repetir Bastión I–II, recursos, construcción, Mundo, diálogos y guardado/recarga.
5. Solo entonces fusionar e integrar en la versión jugable y publicar mediante el circuito verificado. No declarar V3 terminada sin captura real y pruebas.

## Estado de esta entrega

Preparación de dirección artística y módulo aislado en GitHub; **no se ha modificado main** ni se ha solicitado ejecución Unity ni publicación. La propuesta se ajustará al resultado real antes de darla por terminada.
