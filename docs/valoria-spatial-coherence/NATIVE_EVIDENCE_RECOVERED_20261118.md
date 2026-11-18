# Valoria 2.5D — recuperación y evaluación de evidencia Unity, 18/11/2026

**No confundir pilotos.** Este documento evalúa la prueba Unity auténtica del
**otro** candidato `art/valoria-25d-profundidad-piloto`, commit
`4c4b88047a06e2d94ff722e6ea31678fc30981c0`. No prueba el
código de `art/valoria-spatial-coherence-20261118` ni valida sus siete
recortes de muralla y cuatro personajes. Ambas ramas conservan V6 original.

## Evidencia original, ya recuperada

- GitHub Actions, Unity Editor Windows real: [run 44285505085](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44285505085).
- Artefacto de fuentes inmutables: `14419596041`, nombre
  `valoria-depth-frames-44285505085`. Trae `114 PNG`, `evidence.json`,
  `PlayMode.xml` y logs de Unity.
- Verificación independiente efectuada el 18/11 desde el ZIP conservado:
  **114/114 hashes SHA-256 correctos**, **57/57 parejas A/B** con la misma
  posición real de cámara y zoom, en tres trayectorias `pan`, `approach`,
  `retreat`; 390×844 y una muestra 1170×2532 por trayectoria y variante.
- Informe NUnit real incluido: **1/1 pasada**, 0 fallos, ejecutada
  mediante Test Framework `EditMode` con `EnterPlayMode`. Comprueba
  elementos del piloto B sin acreditar rendimiento físico ni arte aprobado.
- Una ejecución posterior `package-movies` falló con
  `FileNotFoundError: native/evidence.json`: el artefacto contenía
  `valoria-depth-44285505085/Unity/Ciudad2DDepthPilotEvidence/evidence.json`.
  Es un fallo de rutas del **empaquetador**, no de autenticidad de las imágenes.
- Los MP4 recuperados se compusieron mediante FFmpeg **solo a partir de
  fotogramas PNG exportados por Unity**, 12 fps, 18 fotogramas por recorrido.
  No se realizó interpolación, movimiento artificial de JPG ni render externo.

## Interpretación artística

A tamaño 390×844, la fracción media de píxeles con diferencia máxima de canal
RGB superior a 10 entre A y ese B es:

| Recorrido | Píxeles afectados, media sobre 18 fotogramas |
| --- | ---: |
| Pan XY | 0,080 % |
| Zoom de acercamiento | 0,085 % |
| Zoom de alejamiento | 0,081 % |

Estas métricas **NO son una puntuación artística ni prueban por sí solas
inexistencia de profundidad**; documentan el tamaño reducido de la
intervención de ese B. La inspección de vistas panorámicas pareadas no muestra
una reconstrucción espacial sustancial del terreno. Por tanto, la evidencia
**no justifica promocionar ese candidato como solución comercial del collage**.

Los datos están almacenados en el reporte local
`validation_recovered.json` generado a partir del ZIP y pueden reproducirse
en el workflow sin Unity:

- `tools/valoria-spatial-recovery/verify_recovered_native.py`
- `.github/workflows/valoria-spatial-recovered-native-movies.yml`

## Ensayo nuevo B: exclusivamente preparado, no ejecutado en Unity

La B de este workstream implementa siete regiones de muralla pintada
reutilizando UV de la textura fuente, más cuatro actores de prueba y
sort order por pie. La prueba offline de integridad y cinco scripts C#
superó Ubuntu en [run 44282692380](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44282692380),
pero **eso NO es PlayMode ni RenderTexture de la B nueva**.

Se ha creado `.github/workflows/valoria-spatial-native-proof.yml` para
realizar cuando exista una reserva exclusiva legítima de Windows para
`art/valoria-spatial-coherence-20261118`: su preflight confirma en vivo
que la reserva `windows-runner-heavy` corresponde a ESTA rama y también
lo confirma dentro del runner. Si otro workstream la ocupa, **salta Unity
sin tocar el PC**. Nunca modifica `main` para apropiarse del recurso.

## Próxima puerta artística y técnica

Primero verificar la **B nueva** con 104 fotogramas auténticos, su shader URP
y el paso del guardia bajo el arco, incluidos errores de opacidad y precisión
de los polígonos; luego PlayMode/tacto, GPU profiler, WebGL y teléfono real.
Si B falla o el render no mejora perceptiblemente el conjunto, rechazarla
y rediseñar contornos/segmentación del terreno; no gastar más en
sombras, niebla o brillo para disimularlo.

**Dictamen:** test nativo previo recuperado y auditado; mejoría artística
comercial todavía NO demostrada. Nuestra variante B sigue pendiente de
una prueba Unity independiente autorizada.

## Archivo reproducible publicado en GitHub Actions

Tras corregir la dependencia externa FFmpeg para no depender de `apt-get`
bloqueado en un runner Ubuntu, la ejecución de recuperación
[44390677388](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44390677388)
ha terminado en **success** sin tocar el Windows self-hosted.

Artefacto `valoria-verified-unity-AB-movies`, ID `14468640850`:
cuatro vídeos de comparación (tres trayectorias y un resumen), seis
vídeos individuales A/B y `recovered_unity_verified.json`.
Se comprobaron de nuevo 114 PNG SHA-256, 57 parejas de cámara idénticas,
1/1 Test Framework, movimiento real en cada ruta y SHA-256 de los cuatro
vídeos de comparación. El ZIP de salida quedó contrastado localmente,
incluyendo los cuatro hashes. Sin interpolación ni frame fabricado.

No es evidencia de compilación ni de calidad de nuestra **otra** B
`art/valoria-spatial-coherence-20261118`; esa ejecución nativa está
pendiente de una reserva legítima del PC.
