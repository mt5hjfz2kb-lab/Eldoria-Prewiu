# Eldoria / Valoria — manual de producción reproducible (Bastión I 2.5D)

**Fecha de elaboración:** 20-11-2026. **Estado:** candidata modular aislada, con correcciones artísticas y de tamaño de WebGL pendientes de nueva evidencia exact-HEAD. No supone aceptación artística, certificación móvil ni autorización para incorporar II–V.

**Autoridad:** rama `experiment/valoria-modular-25d-bastion1-from-zero-20261118`, workstream `eldoria-valoria-modular25d-bastion1-20261119`. Leer también `HANDOFF_20261120.md`, `ART_GROUNDING_RULES.md`, el código más reciente y `sprite_manifest.json`. **El HEAD y las capturas reales prevalecen sobre este documento.** No usar Premium V6 ni la imagen global como fondo. No editar `main` ni los dominios de juego compartidos.

## Evidencia y comprobación de calidad

- **Probado en Unity 6000.3.23f1 sobre `f66f1e756907611e134337e4e8f7f4a8b525a1aa`:** run [44651309304](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44651309304). 45 PNG reales Camera.Render con manifiesto SHA256, 1 prueba NUnit que entra en PlayMode y pulsa UI, construcción/descuento 230→150 madera, fin y relectura de guardado, vídeo desde fotogramas y WebGL aislado real. El navegador ejecutó WebGL con HUD y clics reales a 390×844 y 1170×2532; no demostró guardado tras recargar navegador ni equivalencia Safari.
- **Implantado después de ese SHA, todavía por reevaluar:** HUD con distancias seguras y filetes metálicos, trazados más estrechos de camino, tratamiento cálido del empedrado y reducción de huella del Aserradero. El aislamiento de `Resources` externos en **scratch** pretende reducir la distribución WebGL; falta verificar tamaño y compatibilidad en el siguiente run.
- **No aprobado:** estética final; captura real de GameView del Editor con HUD; iPhone físico, memoria/FPS/overdraw, multitouch prolongado, navegador Safari; persistencia de PlayerPrefs mediante cierre y reapertura real de navegador; publicación de versión para testers.

## Escena y componentes

`Unity/Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity` crea una jerarquía de componentes independientes: terreno, caminos/huellas, edificios, vegetación/ruinas, habitantes, cámara y HUD. **No componer todas las casas/terreno en un único sprite de ciudad**; los objetos deben conservar Transform, SpriteRenderer/MeshRenderer, pivote y orden de profundidad. Guardar intacto el bootstrap legado desactivado.

| Función | Implementación / contrato |
| --- | --- |
| Terreno | `Modular25DGround.cs`: 81 mallas de 8×8 unidades, UV continuas; `OrganicMeadowTile` 512² RGBA de pradera **sin edificios**, procedencia `OrganicMeadowTile.provenance.json`, repetición espacial ~21 unidades |
| Caminos | `Modular25DLandscape.cs`: dos mallas de spline con interpolación por 8 pasos y 9 vértices transversales, textura alpha de adoquines ilustrados `OrganicCobbleRoad.png` y segundo camino de tierra/gravilla; anchuras del último candidato 1,47 y 1,26 unidades; no `LineRenderer` plano |
| Parcelas | Dos sprites alpha distintos generados para 512×320, PPU 88; piedra erosionada/terreno pisado ante el Bastión, serrín/tierra delante del Aserradero; sin bordes rectangulares ni plataformas |
| Edificios | `Modular25DArt.cs`, `Modular25DWorld.cs`: `bastion_i` 480×314, `sawmill_i` 362×273; PNG original con manifest/sha/meta; 96 píxeles por unidad y pivote de pie propio. Tres estados independientes: disponible / obra / construido |
| Bosque y restos | `RecoveredProps/` contiene árboles, ruinas, pozo, carro, valla y huertos como sprites colocables. La vegetación escala conservando pie/pivote; evitar mezclar árboles geométricos sintéticos de otra familia |
| Animación y profundidad | `Modular25DWalker.cs`: 0,75 unidades/s y dos cuadros de marcha, posición real sobre rutas; `FootOrder(y)=RoundToInt(1000-y*100)`; cuerpo/ombra se reordenan al desplazarse. Comprobar occlusión delante/detrás de árboles en capturas sucesivas |
| Cámara | `Modular25DCameraInput.cs`: ortográfica, sin giro libre; `homeSize=Clamp(3.65/max(.42,aspect),5.3,10.8)`; zoom 4,2–11,8, pan limitado, selección por toque solo si no hubo arrastre/pinch ni UI |
| Interfaz | `Modular25DInterface.cs`: uGUI `ScreenSpaceOverlay`, `CanvasScaler` referencia 390×844, HUD de recursos y controles reales. Los nuevos márgenes consultan `Screen.safeArea` y factor de escala; falta revalidar retrato, 1170×2532 y orientación horizontal |
| Autoridad de reglas | `Modular25DGameplay.cs`: `LocalGateway` y `SliceRules` reales, un solo pago y un temporizador; guardado aislado `FileStateStore` en Editor/nativo y `PlayerPrefs` bajo clave experimental en WebGL. No crear economía alternativa |

## Paleta, luz, materiales y tratamiento de assets

- Pradera oliva con verdes medios/ocres y sutiles zonas cálidas; piedra crema/gris cálido, tejado pizarra oscura, madera marrón envejecido, luces de ventanas ámbar. Sombra de arquitectura y árboles en dirección sureste coherente con fuente cálida al noroeste. La luz está mayoritariamente pintada en sprites, por lo que no asumir iluminación 3D en todas las caras.
- Mantener **la silueta artística original** de edificios premium. El Aserradero incluye sombra marrón horneada en PNG de origen; `FinishedSawmillWithCleanMatte` crea en memoria un recorte alpha de contorno por máscara, sin sobrescribir el PNG. No añadirle una segunda sombra oval oscura. Verificar cortes en tejado/toldo/troncos y preservar el recurso original con hash.
- Para cada sprite: registrar origen/licencia, SHA256, tamaño, PPU, pivote del pie, orientación de luz y relación con mundo. Mantener los GUID de `.meta`; no alterar manualmente `sprite_manifest.json` sin recalcular el hash y preservar la procedencia.
- La escala del ciudadano ilustrado es ~0,55 de su sprite de origen; confirmar de nuevo frente a alturas de puerta y sombras a 390×844. Los props no deben invadir entradas, botones ni recorridos.
- **Atlas:** aún no hay una optimización/atlas de producción certificado en dispositivo. No inventar un presupuesto de draw calls ni afirmar rendimiento; perfilar en iPhone y decidir agrupación después.

## Cómo producir otro edificio sin reiniciar la ciudad

1. Diseñar edificio separado con perspectiva, luz, materiales y escala de esta escena. Guardar la ilustración de calidad con alpha real y pie definido; no copiar literalmente recursos de otros juegos ni extraer un recorte de un fondo completo.
2. Crear los estados `AVAILABLE`, `UNDER_CONSTRUCTION`, `BUILT` como sprites diferenciados. Registrar hash, PPU y pivote; nunca dibujar una silueta translúcida del edificio terminado como estado de obra.
3. Situar el edificio con un ancla `Vector2` real, una huella de alpha erosionada y una vía spline orgánica. La unión debe permanecer correcta al hacer zoom, pan y mover habitantes.
4. Enlazar interacción mediante la aplicación canónica, no un contador nuevo. Probar costes, idempotencia, recarga de guardado, reloj natural y estados de llegada.
5. Renderizar de nuevo Unity auténtico en 390×844, 1170×2532, horizontal y primeros planos, y comparar A/B con SHA. Cuando esté aprobado visualmente, probar navegador y iPhone físico antes de reutilizarlo en Bastión II–V.

## WebGL aislado: qué se hace y qué NO

El BuildPipeline del código compartido protege la cinta de consultas del terreno 4X. `scope_isolated_webgl_guard.ps1` modifica **únicamente una copia desechable exact-SHA** para permitir el nombre de escena y carpeta WebGL exclusivos, sin editar ni aflojar el guard permanente en Git. `Modular25DEditorTasks.BuildIsolatedWebGL` establece una lista de una escena, usa `StrictMode` y restaura la lista original.

El run 44651309304 compiló y ejecutó su WebGL aislado, pero el artefacto ZIP pesaba ~1,29 GB. Unity documenta que incluye *todos* los assets de cualquier carpeta `Resources` aunque ninguna escena los referencie. Fuentes oficiales:
- https://docs.unity.com/en-us/engine/6000.0/manual/building-and-publishing/build-content-output
- https://docs.unity.com/en-us/engine/6000.0/manual/building-and-publishing/reducing-filesize

Candidato siguiente: `tools/valoria-modular25d/isolate_webgl_resources.ps1` archiva temporalmente **solo** las carpetas `Resources` ajenas al experimento bajo `Unity/Assets/Eldoria/**`, fuera de `Assets` en scratch, después de las pruebas nativas y antes del Build WebGL. Debe dejar `ValoriaModular25D/Resources` y todos sus PNG originales intactos; escribe `WebGLResourceIsolation.json`. No modifica las fuentes Git, no toca otros procesos ni debe afectar la beta. Se deben medir bytes reales de salida, ejecutar Chromium de nuevo con recursos y HUD correctos, detectar errores de JS/red, revisar imágenes y comprobar persistencia. Si esta poda afecta al producto, **revertir el cambio de scratch**, investigar dependencias y nunca afirmar PASS por compilar.

## Puertas de aceptación

- **Estructura:** 81 chunks, dos splines, dos huellas; Bastión/Aserradero originales, 24 recursos originales comprobados, habitantes y oclusión.
- **Arte:** halo eliminado sin cortar partes del edificio, empedrado y parcelas naturales, elementos de una familia, buen espacio vacío, arquitectura dominante y legibilidad móvil; evaluación humana desde capturas/ejecutable nativos.
- **Juego:** tres estados reales, construir/descontar una sola vez, evolución temporal, guardar/reabrir, hitbox y pulsaciones reales, pan/zoom y rechazo de clics durante arrastre.
- **Web:** compilación WebGL en scratch y navegador real con HUD/clics; tamaño de transferencia informado, recarga de browser y memoria/RAM revisadas, URL alojada solo con permiso y en destino aislado.
- **iPhone:** prueba física independiente en Safari, estabilidad, memoria, FPS, controles y orientación. No declarar completado por Chromium ni por NUnit de Editor.
- **Entrega:** último commit SHA, run, PNG/video/NUnit/logs/manifest con SHA256, diferencias respecto a la prueba anterior, puntos abiertos y ningún cambio en `main`.
