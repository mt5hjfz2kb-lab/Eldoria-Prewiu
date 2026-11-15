# Eldoria — conclusión del ensayo visual de 15/11/2026

Estado: **prueba técnica realizada; objetivo artístico NO alcanzado**. PR #196 debe permanecer en borrador, sin merge a main ni a la beta.

## Evidencia real de Unity

- [Run 43723401749](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43723401749): escena Unity Editor canónica Valoria_City_World_UI, 1600x900 y 390x844, 107 renderers / 118 materiales. El shader alternativo retiró visualmente edificios y vegetación, aunque proyectaban sombras. Hubo diferencia de píxeles significativa pero **calidad fallida**. No confundir screenshot distinto con resultado mejor.
- [Run 43723908844](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43723908844): captura real Unity de A original, B atlas de color pintado usando shaders de origen y C tintado con materiales originales. 35 texturas horneadas en Editor; retención de detalle en el Bastión ~0,923; diferencias medias A→B de 5,22/255 (horizontal) y 5,04/255 (vertical); A→C de 8,23 y 10,51/255. Los modelos vuelven a renderizar, pero **la mejora artística es demasiado pequeña**. No aprobada.
- Arte de referencia generado por IA: **concepto externo**, nunca evidencia de modelo/iluminación/rendimiento integrados.

### Evaluación artística

El techo azul intenso, el césped extenso y poco integrado y la escasa jerarquía entre Bastión, caminos, orillas y ruinas **no se resuelven de manera suficiente con una corrección de color**. Un shader global es además más frágil frente a los materiales PBR, transparentes, glTF y luces de la escena.

El ensayo confirma la vía técnica útil: editar una copia de escena usando materiales URP originales y capturar la cámara real sin entrar en el runtime completo. No confirma mejora comercial ni rendimiento en iPhone.

## Decisión de producción tras rechazar estas variantes

Cambiar de técnica: **no repetir pequeñas variaciones de tinte, filtros ni shaders globales**. El siguiente piloto debe modificar directamente la composición física usando assets aprobados/reutilizables:

1. Elaborar kit modular consistente de rocas, muros antiguos, vegetación, terrazas y caminos conectado al terreno; inspeccionar antes los prefabs existentes y sus licencias, no crear GLB nuevos sin justificar.
2. Convertir el Bastión y el gran arco imperial en jerarquía evidente, con transición entre recinto habitado y ruina exterior; añadir detalle selectivo, no densidad indiscriminada ni elementos inmóviles para maquillar.
3. Usar una cámara 3/4 y dos escalas reales; registrar A/B de la misma escena en Editor y luego PlayMode/iPhone cuando la estabilidad P0 esté resuelta.
4. Certificar por separado calidad (meta ≥7,5/10 respecto a la referencia aprobada), legibilidad, jugabilidad y memoria. Sin arte aprobado en estos runs.

## Lecciones técnicas documentadas

- El primer ensayo procedimental 2.5D se veía como una maqueta infantil. Se descartó, sin tratarlo como asset de producción.
- Una pasada 2D sobre screenshots no soluciona el escenario; se descartó.
- En el capturador C#, cualificar `UnityEngine.Application` evita colisión con `Eldoria.Application`.
- `-quit` con PlayMode diferido hace salir Unity antes de obtener capturas. La ruta Editor síncrona evita esa dependencia y la carga del mundo P0.
- Las capturas y el manifiesto de artefactos auténticos están conservados en los runs anteriores. Un `CI success` y diferencias de píxeles **no equivalen a ART_APPROVED**.
- **No modificar** P0/PR194, beta/testers, saves ni escena productiva por esta prueba. Ninguna build WebGL ni APK se produjo.
