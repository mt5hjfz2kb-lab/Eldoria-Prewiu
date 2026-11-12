# Vigilia de la Brecha — causa real del efecto de piezas superpuestas
Fecha: 9 noviembre 2026 · PR #173 · workstream eldoria-forja-oscura-visual-proof-20261107

## Síntoma
El propietario considera que **todos** los elementos (perfil, recursos, misión, ficha y navegación) parecen pegados sobre otros. La captura de Unity es auténtica, pero su éxito técnico no certifica nivel artístico premium ni fidelidad al concepto vertical elegido de 864×1536.

## Diagnóstico verificado, no intuición
1. **No había dos HUD completos visibles.** La rama BreachWatch utiliza un Canvas independiente uGUI; durante la prueba oculta el Canvas del RoyalHUD antiguo y el input se enruta a gameplay real. No obstante, el Canvas de pantalla siempre se dibuja por encima del mundo 3D; eso es arquitectura de HUD normal, no garantía estética.
2. **La mayoría de los sprites ya son arte compuesto.** Se examinó directamente el RGBA de la rama (paquete autorizado con SHA de integridad). `profile_bar.png`, `resource_capsule.png`, `chiseled_quest.png`, `panel_fill.png` y `brass_portrait.png` tienen centro totalmente opaco, con bordes, metal/sombra y superficie ya pintados. No son máscaras neutras que admitan varias placas adicionales.
3. **Se duplicaba el volumen pintado.** Unity añadía un `Shadow` propio al perfil, misión y ficha, encima de las sombras presentes en cada PNG. Además, `brass_portrait` dibujaba un segundo bloque opaco bajo una miniatura del Bastión cuya cámara URP ya produce píxeles RGBA transparentes. El conjunto parecía una tarjeta sobre otra tarjeta.
4. **El escudo del perfil sobresalía de la barra.** Se colocaba con coordenadas negativas y escala mayor que el cuerpo visual en retrato y horizontal. Era funcional pero reforzaba el aspecto de insignia pegada.
5. **Los recursos estaban diseñados como piezas sueltas con colores distintos.** La misión, el perfil, las cápsulas y los medallones pertenecen a sprites separados; sin tratamiento tonométrico común se perciben como kits distintos.

## Implementado en la PR
- Eliminados los tres `AddContactShadow` en recursos visuales completos (sin quitar sombras de tipografías, necesarias para lectura).
- `brassFrame.enabled=false`: no se dibuja el segundo rectángulo detrás del retrato URP; se conserva la textura fuente y el componente para trazabilidad, no se modifica el mundo ni el modelo real.
- Escudo de león dentro de la única barra de perfil, conservando el arte/imagen, título real y todas las acciones de la UI.
- `new Color(.97,.95,.90,1)` compartido para superficies principales de perfil, recursos, misión, cuerpo de ficha y navegación, manteniendo colores funcionales marfil/borgoña y rift de evento.
- Reglas de prueba nativas para impedir regresión: **0** sombras extra en esas tres placas, imagen de retrato secundaria desactivada, león inserto en barra y fuentes pictóricas con tratamiento coherente.
- `verify_single_surface_composition.py`: prueba Ubuntu que abre los cinco PNG reales, verifica su centro opaco y detecta reaparición de capas; ejecutado mediante `.github/workflows/breach-watch-native.yml`.

## Lo implementado NO es aún un resultado visual aprobado
- **Probado:** sintaxis Roslyn / integridad del arte / prueba de fuente de composición en runner Ubuntu; native fue `skipped` mientras el recurso Windows estaba formalmente reservado a reparación del cronómetro.
- **Pendiente imprescindible:** adquirir `windows-runner-heavy` con protocolo CAS tras su cesión expresa; comprobar que no existe trabajo Unity concurrente; capturar de nuevo 8 PNG del **HEAD exacto**, evaluar contra [run 42630398578](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42630398578) y contra la referencia visual vertical seleccionada; conservar solo cambios que produzcan calidad real, no simplemente menor diferencia de píxeles. Revisar en ambas orientaciones legibilidad, solapes, retrato transparente y no regresión del mundo/partida.
- Si el fondo de ciudad/arquitectura aún contrasta mucho con el concepto ilustrado, clasificar como trabajo artístico de mundo independiente. No taparlo con una imagen estática ni añadir marcos para disimularlo.
- No integrar PR173 en `main`, APK o WebGL sin aprobación.

## Diagnóstico adicional: paleta del mundo (medición orientativa, NO A/B final)

El efecto «pegado sobre un juego distinto» también tiene una causa que **NO está en los Canvas**: el mundo vigente de Unity es verde y soleado; el concepto artístico entregado anteriormente contiene ciudad más oscura/cinematográfica. Se midieron muestras manuales con la misma fórmula aproximada de luminancia relativa (R×0,2126 + G×0,7152 + B×0,0722):

| Muestra examinada | Luminancia RGB mediana (0–255) |
|---|---:|
| Mundo real Unity, run 42630398578, área de suelo/vegetación | ≈114,5 |
| Ficha mineral auténtica del mismo PNG | ≈39,6 |
| Fondo de ciudad de **otro concepto horizontal** 1536×864 histórico, no la referencia vertical final | ≈62,7 |
| Placa derecha en ese concepto horizontal | ≈31,3 |

Esto **no es una puntuación artística, no usa escenas equivalentes y no acredita una causa única**. Sí prueba que el contraste de la ciudad soleada con el HUD oscuro es muy superior al del concepto horizontal: solo quitar un píxel de sombra o una placa interna no convierte el mundo en ilustración cinematográfica. Alterar globalmente la cámara o el terreno para disimular el problema implicaría tocar otro workstream y podría disminuir la visibilidad: **no se realizó**.

Conclusión de oficio: conservar las correcciones físicas de capas y reservar la revisión final de la relación **HUD + mundo** para las ocho capturas reales del HEAD. Si sigue habiendo una brecha artística grande, realizar una variante integrada que tenga tratamiento material/iluminación coherente, compararla sin inventar un fondo 3D y promover únicamente una mejora confirmada. Esta observación evita ciclos de sombras que no pueden solucionar la diferencia de acabado del mundo.
