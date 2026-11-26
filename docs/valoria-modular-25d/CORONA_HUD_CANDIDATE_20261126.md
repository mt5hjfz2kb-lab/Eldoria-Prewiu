# Corona de la Brecha — candidato HUD, no integrado

Base exacta: 5ef2aee67cf8fde57ba2fc4b3d929b517ee67944. Revisión inicial del encargo de 26/11/2026.

## Cambio comprobado a nivel geométrico

PlayerRibbon.Layout: reserva espacio de las tres cantidades antes de extender el nombre. El área del icono y la cantidad se calcula por separado, con 4 unidades lógicas de separación. Preserva sprites originales, aspecto, orden, callbacks, wallet y navegación. Simulación de 56 combinaciones de ancho seguro 300–750 y ancho preferido del nombre 0–1000: distancia mínima original -9, candidata +4; caja numérica mínima 24,33. No mide glifos ni estética.

## Gates pendientes

- Compilar Unity 6000.3.23f1; verificar nickname corto/largo, ES/EN, cantidades compactas, todos los iconos, lectura real a 390×844 y 1170×2532, paisaje 844×390, safe area, touch y ausencia de regresión. Rechazar si nombre pierde información de forma inaceptable o cantidad se recorta/desborda. Capturar antes/después con misma cámara. No afirmar 8,5 ni aprobación.
- Mantener arte de Corona, fonts existentes y estados de botón; consolidar biblioteca solo tras inventario de CoronaFinish y productores de UI por pantalla. Usar iconografía compartida por recurso, no copias; distinguir CTA, secundaria, pestañas, medallones, cerrar, cantidades, bloqueado y seleccionado. Mantener nombres, callbacks y datos.
- Auditoría real del HUD, marchas, mochila, buzón, nombre, tesoros (secuencia), héroes, entrenamiento, exploradores y vigilantes; además construcción, mejoras, informes, objetivos, confirmaciones y tutorial. No tocar las ilustraciones aprobadas sin A/B superior. Referencias: Whiteout Survival y Call of Dragons, sin copiar arte.
- Studio: director visual, técnico, programador y auditor independiente, con fotogramas nativos exactos, reserva económica autorizada y respetando workstream Studio y Valoria. Separar diagnóstico, implementación, prueba y aceptación. No se ejecutaron llamadas nuevas para este candidato.

Tras la orden expresa del propietario, la prioridad y el lease de Windows pasan a Corona. Se integra este candidato en la rama experimental para certificarlo con Actions (sin publicación). La rama de Studio se reasigna a la misma misión; Main solo contiene el cambio administrativo de coordinación. La simulación de 56 combinaciones se ejecutó también de forma independiente; la prueba nativa y visual siguen pendientes.
