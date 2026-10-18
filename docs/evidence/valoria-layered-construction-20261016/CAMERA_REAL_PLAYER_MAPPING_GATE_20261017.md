# Valoria — Puerta 02: cámara real / correspondencia 3D / decisión de producción

Fecha: 2026-10-17. Rama aislada `construction/valoria-master-plan-layers-20261016`. Ejecución Unity [39017098037](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39017098037). Artefactos y escena persistidos en commit `dd78deb4b1089c4fd207810168831efd07d9b853`.

## Recuperación sin modificaciones de fuentes canónicas

Fuente de topología: `certified-legacy-skeleton.json` con 22 huellas, escalera de 12 peldaños y rutas/parcelas originales. Cámara histórica `VisualWorld`: **ortográfica**, posición (18.2,14.6,-25.8), zoom 9/12/19. **No** es la cámara del jugador.

Cámara del jugador: componente exacto `ValoriaParcelPresentation`, proyección en **perspectiva**, HOME identity, FOV 44.42281 grados; pan horizontal y zoom según su código real. Referencia visual propietario SHA256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`. Referencia SHARP de anclas medida sobre HOME 1230×845 y tomada del gate previamente aprobado (9 semánticas, 45/45 raycasts históricos).

## Nueva ejecución genuina sobre Unity

Se ha recuperado `Valoria_MasterPlan_Layers_ABCD_BridgeV1.unity`, **sin sobrescribirla**, y generado `Valoria_SHARP_Camera_Mapping_DIAGNOSTIC.unity` con cámara real `ValoriaParcelPresentation.Home()/Pan()/Zoom()`. Medidas reales:

- HOME FOV: 44.42280960083008.
- Pan: -0.50000036 y +0.50000036 metros.
- Zoom: FOV 42.42281 y 46.42281; HOME restablecido.
- 7 anclas con fuente X/Y/Z documentada: puente, puerta, camino, escalera, Bastión, parcela occidental, parcela oriental.
- Diferencia máxima **proyección analítica Python vs Unity nativo**: 0.0000734 píxeles. Prueba matemática/Unity: **TECH PASS**.
- Error máximo **escena transformada rígidamente vs anclas SHARP**: **81.023 píxeles**; RMS **57.386 píxeles**. Umbral previo 12 px: **FAIL**.
- Desglose de error: puente 40.897 px, puerta inferior 81.023, camino 50.468, escalera 28.658, Bastión 45.676, oeste 63.931, este 72.615.
- El ajuste exclusivo del eje puente-puerta-camino-escalera da <6px para esas 4 anclas, **pero** desplaza la cabaña 146.174px y el campamento 364.376px; NO hay `FULL CAMERA PASS`.

## Inspección visual directa

Captura fuente `sharp-camera-mapping/sharp-perspective-home-1230x845.png` inspeccionada: aun con cámara real, el conjunto puente+puerta ocupa desproporcionadamente todo el primer plano, Bastión superior continúa como caja GREYBOX y las parcelas de expansión carecen de arquitectura final. **FULL-SCENE ART FAIL**. No hay iluminación, roca, río o paisajismo completos por la sola presencia de materiales/modelos importados.

La ubicación de un ancla semántica SHARP no implica por sí sola que sea el centro geométrico del objeto 3D histórico. La prueba cuantifica incompatibilidad de **una sola transformación rígida** con las correspondencias adoptadas; no demuestra imposibilidad matemática de todo nuevo diseño espacial.

## Resultado y norma de dependencia

**Stage 01:** referencia/plano histórico recuperados, sin regresión.

**Stage 02:** CAMERA COMPONENT TECH PASS / FULL PLAYER-COMPOSITION **NOT PASSED**. Para pasar será preciso un nuevo diseño espacial **reversible y físicamente jugable** que use las anclas aprobadas y conserve rutas, parcelas, selecciones y progresión; no se permite deformar los módulos, estirar la imagen SHARP, inventar un FOV o trasladar colliders ocultos sin revalidación.

**Stage 03:** puente de piedra BridgeV1 es candidato de mejora local. TerrainV1 (16.164 triángulos) tiene TECH PASS pero FULL ART FAIL; no sustituye la base visual mejor anterior. No añadir nueva terraza, Bastión superior ni decoración para ocultar el problema.

**Stage 04–09:** cerradas hasta superar composición/terreno y pruebas funcionales reales.

## Decisión técnica / límite de autoridad

Hay dos autoridades certificadas que no coinciden espacialmente bajo un encaje rígido: la topología métrica del esqueleto antiguo y el layout visual de SHARP. Reubicar parcelas/circulación canónicas o reemplazar el contrato de la cámara superaría la mera autonomía artística. La próxima **implementación autorizable** es una adaptación espacial **dentro de una nueva escena de prueba**, que recertifique las mismas conexiones, alturas de circulación, selección y reservas sin cambiar el runtime; una adaptación que cambie las coordenadas jugables certificadas necesita autorización explícita del propietario.

No ejecutar más familias/fortificación superior. No proclamar puntuación 6/10, ni cámara aprobada por haber pasado FOV/pan/zoom. El repositorio y las ramas congeladas siguen intactos.

## Artefactos reproducibles

- `tools/valoria-layered-construction/calibrate-sharp-player-frame.py` reconstruye el ajuste y genera JSON/SVG de errores.
- `Unity/Assets/Eldoria/ArtTests/ValoriaLayeredConstruction/Editor/ValoriaSharpPlayerCameraCorrespondence.cs` crea escena y seis capturas nativas invocando el componente de cámara real.
- `docs/evidence/valoria-layered-construction-20261016/sharp-camera-mapping/sharp-camera-rigid-calibration.json`.
- `docs/evidence/valoria-layered-construction-20261016/sharp-camera-mapping/sharp-camera-native-evidence.json`.
- `docs/evidence/valoria-layered-construction-20261016/sharp-camera-mapping/sharp-perspective-home-1230x845.png` (más pan L/R, zoom y retrato).
- Workflow `.github/workflows/valoria-player-camera-rigid-registration-v1.yml` y run 39017098037 TECH PASS, no promoción.
