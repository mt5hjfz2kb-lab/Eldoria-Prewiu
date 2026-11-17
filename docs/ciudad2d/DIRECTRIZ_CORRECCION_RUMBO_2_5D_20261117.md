# Ciudad 2.5D · Corrección de rumbo para producción artística
**Fecha:** 2026-11-17  
**Alcance:** rama `art/ciudad-2d-hitos-1-5-20261116`; propuesta de cambio de metodología. No es aprobación artística ni orden de migrar `main`.  
**Autoridad:** no modificar reglas, `PlayerState`, `LocalGateway`, guardados, Build Settings, escenas del juego ni otras ramas.

## Decisión

**Mantener la arquitectura 2.5D y el progreso funcional validado, pero cambiar el origen de los sprites.** La prioridad deja de ser mejorar imágenes heterogéneas mediante afilado, máscaras o nuevas generaciones individuales. La primera prueba siguiente será **dos edificios generados desde una fuente 3D compartida y prerenderizados para usar solo sprites en Unity**.

El modelo 3D es herramienta de autoría, no requisito de cargar 3D en tiempo real.

## Evidencia que obliga a cambiar método

- `docs/ciudad2d/SHARPNESS_AUDIT.md`: pasar de HQ a sin compresión apenas mejora los bordes; `_Crisp` aumenta métricas de nitidez pero no corrige las bases rectangulares ni la iluminación.
- Máscaras desvanecidas y desgaste de bordes ya rechazados por halos y persistencia del plinto. El experimento nuevo `City2DGroundingBake` puede cerrarse con su QA real, pero **no elevarlo a método de producción** por el mero hecho de superar un test de cobertura de píxeles alfa.
- Dos Cuarteles ilustrados individualmente rechazados por estilo y recorte. Run 44023117740 importó un candidato pero este desapareció visualmente en la captura nativa; un éxito CI no equivale a buen arte.
- El pipeline de escena, snapshots de solo lectura, hitos I/V, cámara, selección y capturas nativas es reutilizable y NO debe rehacerse.
- `art/visual-reboot` contiene un kit modular Blender que puede inspeccionarse como **fuente geométrica candidata no aprobada**, sin trasladarlo a `main` ni asumir calidad.

## Acción prioritaria: piloto A/B, sin producción masiva

1. **Congelar control A**: sprites actuales, coordenadas del `city_layout.json`, cámara y resolución; preservar capturas Unity y hashes.
2. **Establecer escena maestra de autoría** en Blender, aislada: cámara ortográfica calibrada con las imágenes del terreno y con la perspectiva del juego (medir, no adivinar ángulo); iluminación cálida común; materiales piedra, madera, pizarra, hierro y heráldica unificados. Versionar `.blend`, configuración de cámara/luces y script de exportación.
3. **Modelar dos edificios solamente** (Cuartel y Aserradero) con geometría moderada y siluetas reconocibles. Usar módulos solo cuando ayuden. Preferir proporciones y materiales con identidad al conteo de detalles.
4. **Renderizar varias capas** con el mismo encuadre: color RGBA con alfa real, sombra de contacto separada y, si aporta, emisivo/FX. Sin suelo rectangular horneado ni simulación de transparencia de plataformas. Crear anclaje `foot/pivot` de referencia para escala y solapamientos.
5. **Integrar B en escena sandbox como alternativa**, sin sustituir ni borrar A, sin alterar hitos/reglas/guardados. Mantener iguales posición de apoyo, escala aparente y fondo. Si B es inferior, conservar A.
6. **Capturar en Unity real**, no solamente Pillow/navegador: retrato 390×844, 1170×2532, apaisado 844×390; vista general y zoom cercano; sombras, colisiones, profundidad, edificios construidos/no construidos; comparar A/B lado a lado a resolución idéntica.
7. **Separar certificaciones**: pruebas automáticas de existencia, alfa, dimensiones, centro de masas y visibilidad; revisión humana del resultado artístico; PlayMode de selección/gestos; perfil iPhone/Android/WebGL con memoria y estabilidad. Ninguna sustituye a las otras.

## Criterios de aceptación para continuar

- Ambos edificios B se ven con claridad al tamaño normal de teléfono, y no aparecen ni desaparecen cuando no corresponden.
- Sombra, perspectiva, paleta, material, escala y contacto con suelo son más coherentes con el Bastión y el terreno que A, **sin plintos ni halos**.
- No hay regresiones de selección, hitos I/V, build contract o `PlayerState`; `main` permanece intacta.
- Compilación y RenderTexture Unity reales certificados con archivos exactos/SHA; PlayMode y hardware físico se reportan aparte.
- Existe una receta reproducible para fabricar el **tercer** edificio sin improvisar de cero ni retocar su PNG de forma manual.

Si no se cumplen, documentar el motivo (geometría, perspectiva, iluminación, escala, material o alpha), revisar el método y repetir SOLO el piloto, no expandir a docenas de edificios.

## Lo que NO se ordena

- No cancelar ni falsear QA PlayMode en curso.
- No reconstruir todo Eldoria o el mundo 4X con esta técnica todavía: el zoom regional/panorámico necesita prueba independiente.
- No seguir generando versiones independientes de IA, afilando `_Crisp` o difuminando bases como estrategia principal.
- No modificar `main` ni promocionar una imagen porque un CI sea verde.
- No asumir que renders 3D simplificados garantizan acabado premium: es una hipótesis que **debe validarse**.

## Próximo resultado que merece revisión

**Un A/B real de Unity con Cuartel y Aserradero, mismo terreno y cámara, usando sprites derivados de un archivo Blender reproducible, más criterio artístico y técnico separado.**

**Estado:** directriz documentada, cambio de método propuesto. La prueba 3D → sprite **no está todavía implementada ni certificada**.