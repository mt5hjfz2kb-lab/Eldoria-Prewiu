# Eldoria / Valoria — Bastión I modular 2.5D: entrega técnica reproducible

**20 de noviembre de 2026.** Experimento independiente: `experiment/valoria-modular-25d-bastion1-from-zero-20261118`.

## Resultado realmente probado

**Fuente exacta certificada:** `e681fda70aaa985dc62a928e4c9c6a0c63628cd5`.  
**Ejecución final de Unity, PlayMode, WebGL, navegador y vídeo:** [GitHub Actions #44713811420](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44713811420). **Cuatro trabajos SUCCESS: preflight, unity, browser, video.** Ningún PASS se deduce de un workflow con Unity omitido.

| Elemento | Prueba que realmente existe | Límite |
| --- | --- | --- |
| Escena | Unity 6000.3.23f1, 45 capturas Camera.Render verdaderas, incluidas vertical 390×844 y 1170×2532, horizontal y detalle del Aserradero; `evidence.json` con SHA-256 | Capturas de cámara Editor sin HUD ScreenSpaceOverlay |
| Gameplay | **1/1 NUnit**, ensayo EditMode que entra en PlayMode real y comprueba Canvas, clic auténtico ASERRADERO, coste 80 madera 230→150, obra, guardado/reapertura, cámara Home y caminante real | No es batería extensa de PlayMode ni dispositivo |
| WebGL | `BuildOptions.StrictMode`, una sola escena; `webgl-status.json: SUCCEEDED`, 17 archivos, **20.641.571 bytes** totales; artefacto ZIP **20.019.553 bytes** | No es beta integrada ni URL pública hospedada |
| Navegador real | Chromium: captura visible con HUD 390×844 y 1170×2532, pulsaciones CONSTRUIR, saldo visual 230→150 y saldo idéntico tras recargar; sin errores JS/red | No equivalencia Safari iOS |
| Cámara táctil | `Input.dispatchTouchEvent` nativo de Chromium: drag y pinch en 390×844 y **844×390**, botón CENTRAR restaurado en vertical, capturas y SHA-256 | Tacto simulado por Chromium; no iPhone real |
| Vídeo | MP4 montado desde los 32 fotogramas de movimiento original Unity validados con hash | No grabación de pantalla de iPhone |
| Procedencia y protección | Bastión/Aserradero independientes, sprites PNG originales, metadatos/guid conservados, 2 splines, 2 huellas, 81 chunks; LocalGateway canónico | Arte sin aprobación final |

**Artefactos del último run** — ver [página de la ejecución](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44713811420) para descargar:
- `eldoria-mod25d-genuine-unity-44713811420`: 45 PNG + `evidence.json` y log.
- `eldoria-mod25d-source-verified-video-44713811420`: MP4 original.
- `eldoria-mod25d-actual-playmode-44713811420`: NUnit, guardados, estado WebGL y pruebas de aislamiento.
- `eldoria-mod25d-chromium-webgl-ui-44713811420`: ocho capturas de interfaz/estado y capturas de pan/zoom horizontal/vertical, con manifiestos SHA-256.
- `eldoria-mod25d-webgl-player-44713811420`: ejecutable WebGL aislado.
- `eldoria-mod25d-original-sawmill-44713811420`: PNG y .meta originales de autoría.

## Cambios visuales consolidados

1. El sprite ilustrado de Bastión I sigue intacto. El Aserradero original se usa en los tres estados propios; `Modular25DArt.FinishedSawmillWithCleanMatte` atenúa su sombra horneada externa en una copia runtime y se eliminó la segunda gran sombra oval de contacto. **No se modificó el PNG original.** La captura cerrada muestra un contacto mucho menos tosco, aunque la calidad fina de su borde y del serrín debe juzgarla el propietario.
2. Terreno de pradera ilustrado continua, empedrado original reutilizado sobre spline con alpha variable en los bordes, huellas transicionales separadas de las construcciones; sin plataforma 3D ni escena de fondo única.
3. Props originales de ruinas, árboles y taller recuperados, habitantes independientes con sombra y orden de oclusión por pies, mejor escala relativa; pan/zoom auténticos.
4. HUD vertical desplazado dentro de los márgenes y pruebas de su saldo. **HUD horizontal compacto**: recursos en la esquina superior derecha, texto informativo a la izquierda, botones inferiores reducidos. El run final confirma que deja visible el Bastión y no captura el gesto iniciado sobre terreno.

## WebGL realmente aislado y reproducible

El gate canónico de `frozen terrain query tape` continúa sin modificar en los archivos compartidos. Únicamente el scratch temporal de este workstream, fijado por SHA, delimita una excepción estricta a la escena **exacta** `BastionI_Modular25D.unity` y a su carpeta de compilación. En ese scratch se archivan fuera de `Assets` ocho carpetas `Resources` de módulos ajenos (785.978.498 bytes de origen), conservando íntegros los recursos `ValoriaModular25D/Resources`. Ambos scripts emiten manifiestos auditables y fallan si el origen o ruta de scratch no encajan. Se conserva `main`, Premium V6, la beta y los guardados del jugador.

Existe asimismo paquete portable Windows de la build exacta, con pequeño servidor HTTP local implementado en Go 1.23 (compilado a exe Windows x64). Las pruebas Go y cabeceras HTTP gzip/wasm han pasado en Linux; **no se ha ejecutado el exe en Windows físico**. Para Safari en iPhone, el usuario podrá abrir la URL LAN privada que muestre el servidor en un ordenador de la misma Wi-Fi. No se expuso servicio público ni se gastaron créditos.

## Puertas todavía no certificadas

- **Aprobación artística subjetiva del propietario:** pendientes valoración final de halo, caminos, coherencia de plantas/ruinas, composición y HUD. Haber pasado Unity/Chromium no otorga una puntuación de calidad comercial.
- **Safari en un iPhone físico:** pendientes FPS, memoria/GC/overdraw, clics y pinch reales, estabilidad/reinicios y duración. Chromium en Linux/SwiftShader no es una medición de iPhone.
- **Screenshot de GameView Editor con ScreenSpaceOverlay:** no se ha obtenido; en su lugar existen capturas del ejecutable WebGL auténtico mostrando el HUD, no se presenta como equivalente de Editor.
- **Publicación hospedada:** no autorizada ni realizada. La entrega es ZIP/WebGL local más artefacto de CI. Nunca modificar la beta o fusionar `main` sin orden expresa.

## Continuidad

Manual de producción: `docs/valoria-modular-25d/PRODUCTION_GUIDE_BASTION_I_20261120.md`. Escena y fuentes: `Unity/Assets/Experimentos/ValoriaModular25D/`. Workstream canónico **`eldoria-valoria-modular25d-bastion1-20261119`**. La entrega técnica está verificada, y tras liberar Windows y dejar la entrada disponible deberá reclamarse este **mismo** ID en futuras mejoras autorizadas (no crear workstreams competidores). El siguiente responsable debe recuperar el HEAD, documentos, artefactos, prueba física pendiente y evaluación artística concreta, nunca reanudar el prototipo antiguo ni volver a empezar.
