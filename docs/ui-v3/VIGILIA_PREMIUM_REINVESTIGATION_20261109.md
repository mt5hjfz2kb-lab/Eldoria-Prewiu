# Vigilia de la Brecha — reinvestigación de calidad / 9 noviembre 2026

## Dictamen

**Objetivo viable, técnica artística insuficiente.** La pantalla es un HUD nativo separado del sistema RoyalHUD y el render Unity del SHA `27e68ef737678bb28c4ed77df42820f7701e4133` pasó sus controles. Esa prueba demuestra integración e interacción, **no** paridad visual con la imagen vertical 864×1536 elegida por el propietario. El cliente necesita un acabado pulido, limpio y coherente con la ciudad real, no más elementos pintados por encima.

**Decisión de arquitectura verificada con documentación oficial:** conservar uGUI para esta primera pantalla de Unity 6000.3 y los componentes MonoBehaviour ligados a los datos del juego. Unity recomienda uGUI para runtime y documenta uGUI/UI Toolkit con ventajas distintas; migrar ahora a UI Toolkit no arregla la calidad de las ilustraciones. Referencia: https://docs.unity.com/en-us/engine/6000.3/manual/uitoolkits/ui-system-compare . Una placa rectangular adaptable debe usar un sprite con `border` y `Image.Type.Sliced`; un motivo completo, asimétrico y pintado (medallón, filigrana, emblema) se representa como **un solo** `Image.Type.Simple`. Referencia Unity UI: https://docs.unity.cn/Packages/com.unity.ugui%401.0/api/UnityEngine.UI.Image.Type.html .

## Evidencia examinada

- Referencia **aprobada**: imagen vertical 864×1536 aportada por el propietario, con galería de cinco medallones, piedra/bronce cálidos, Bastión y puente, miniatura de edificio legible y CTA en una línea.
- Evidencia **nativa**: GitHub Actions [42582751853](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42582751853), SHA `27e68ef...`, 8 PNG capturados en Unity 6000.3 (900×1600, 390×844 y dos orientaciones horizontales). Se revisó el panel con el mismo tamaño visual de la referencia, sin confundirlo con un render conceptual.
- En el recorte comparable de ficha, el color promedio RGB se parece (referencia aproximadamente 70/58/42, Unity 66/57/41) pero **eso no implica calidad comparable**: la fuente visual, grosor de biseles, nitidez, escala del Bastión y espaciado son diferentes. Esto descarta que baste con aclarar el negro o añadir sombras.
- Referencia: marco fotográfico dorado fino y modelo del Bastión llenando su ventana. Unity: doble bisel octagonal excesivamente grueso y modelo pequeño dentro de una ventana corta.
- Referencia: botón borgoña fino, martillos a la izquierda y «Mejorar a Bastión II» legible. Unity: gran relieve con martillos invadiendo «MEJORAR A II».
- Referencia: marcos de perfil/recurso poco intrusivos. Unity: líneas negras y doradas concéntricas de aspecto mecánico y tipografía que queda comprimida a móvil.
- La referencia es un **concepto ilustrado**: sus texturas, vegetación y castillo tienen un grado de acabado que los modelos y el terreno reales no reproducen automáticamente. La calidad del mundo **no puede certificarse ni corregirse ocultándolo con gráficos 2D**. Esta limitación debe reconocerse al juzgar la escena final.

## Fallos de proceso y resolución

| Causa constatada | Qué NO volver a hacer | Técnica nueva / prueba necesaria |
|---|---|---|
| Repetir marcos, glyphs y halos detrás de medallones completos | Superponer más sprites para tapar el defecto | Un único sprite pintado + imagen de hit transparente. Verificación estructural y raster real |
| Incrustar muchos anillos y remaches con geometría procedural | Confundir bordes más gruesos con acabado premium | Arte de superficie **unitario** con materiales suaves, pocos filetes en el borde y 9-slice correcto |
| Marcos retrato y botón excesivos | Más grosor o menor fuente para que quepa | Una imagen de marco de retrato delgada, área visual real ampliada, icono/etiqueta separados |
| Bastión pequeño en miniatura | Sustituirlo por una miniatura inventada | Reencuadrar exclusivamente la cámara del modelo 3D genuino, verificar no clipping en Unity |
| Pasar test de código sin cumplir objetivo visual | Llamar `TECH_PASS` a `ART_APPROVED` | Comparar PNG exact-HEAD contra referencia a 900×1600 y 390×844; gate artístico independiente |
| Suposición de que UI Toolkit solucionaría el arte | Reescribir todo el sistema sin causa | Conservar uGUI; contemplar TextMeshPro/SDF en un **piloto separado** si la tipografía sigue pixelada. Docs: https://docs.unity.cn/Packages/com.unity.textmeshpro%404.0/manual/FontAssetsSDF.html |

## Trabajo de producción autorizado

1. **Reemplazar**, no superponer, los sprites de recursos, perfil y marco de retrato; conservar GUID de Unity para evitar referencias rotas. Quitar biseles internos obsoletos. Cada sprite debe tener una única función clara.
2. Ampliar el retrato 3D sin perder parapetos ni cambiar el mundo, con una prueba de silueta y render real.
3. Reorganizar el CTA para que la inscripción no se cruce con los martillos, y medir la tipografía real embebida en el ancho disponible.
4. Compilar y capturar de nuevo las cuatro resoluciones de Unity; comparar pixel y presentación; corregir con distinta técnica si persiste la diferencia.
5. Mantener el trabajo aislado en PR173, sin merge ni publicar y sin invadir la reserva del ejecutor de otros trabajos.

## Criterio de cierre

**No afirmar calidad alcanzada por scripts o imágenes generadas.** Se necesita (a) 8 capturas del SHA exacto con Unity real, (b) navegación y acciones genuinas correctas, (c) ausencia de duplicaciones, recortes y clics perdidos, (d) juicio visual de fidelidad a la referencia en móvil y horizontal. Si el **mundo 3D** sigue lejos de la referencia, separarlo como deuda artística comprobada, sin inventar una falsa captura premium.
