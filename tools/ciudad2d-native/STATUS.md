# Eldoria — Ciudad 2.5D / continuidad del experimento (16-11-2026)

## Alcance y rama

- Rama: `art/ciudad-2d-hitos-1-5-20261116`.
- Prototipo `Unity/Assets/Experimentos/Ciudad2D`. Aislado de `main`, de la escena real, de los sistemas de combate y de los guardados.
- Arte: **Bastión I** para niveles 1–4 y **Bastión V** para 5–9. El arte de 10/15 aún NO existe. Los cinco edificios secundarios utilizan un único sprite cada uno. El Salón de Héroes es un marcador artístico sin nivel persistido separado.
- Elementos: terreno separado + seis edificios + nueve sprites de atrezzo situados en diez posiciones. No presentar composición externa como imagen renderizada por Unity.
- Dirección visual: fantasía medieval estilizada con pizarra/azul oscuro, piedra envejecida y Brecha violeta. La simetría del terreno y la disparidad de acabados con los secundarios necesitan reautoría específica.

## Evidencia comprobada

- [CI de fuente + Chromium, run 43967699824](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43967699824): **SUCCESS** en SHA `7052ef8c0822738dd8c1d314b0ea9a6d1982a6e1`.
- Browser JSON de ese run: cuatro vistas (390×844, 844×390, 1170×2532 y 1600×900); las 17 imágenes cargadas, cambio I/V, pulsación del Bastión, rueda de zoom, arrastre horizontal y 0 excepciones JavaScript. Es prueba de Chromium simulado, **no** de iPhone físico, Safari ni Unity.
- Capturas PNG genuinas de Chromium en artifact `ciudad2d-browser-proof-43967699824`, artifact ID `14276662187`.
- QA de sprites y atrezzo separadas verifican SHA/dimensiones/alfa. No prueban calidad artística.
- Correcciones posteriores incorporadas a Unity (aún no ejecutadas nativamente): cámara con umbral de arrastre y exclusión UI; HUD compacto; reconstrucción del registro/eventos después de deserializar; adaptador de solo lectura de `PlayerState` y pruebas nativas preparadas. La CI del run anterior solo revisa el código/activos, no los ejecuta bajo Unity.

## Protocolo de prueba nativa

- `Scripts/Editor/City2DNativeEvidence.cs` crea escena independiente, verifica seis colliders, reencuadres y los niveles 4→5 con el **tipo de dominio real** `PlayerState`, toma seis capturas `RenderTexture` en Unity e informa SHA256. **NO** es prueba PlayMode ni QA de usuario.
- `.github/workflows/ciudad2d-native-proof.yml` aporta el transporte aislado por sparse checkout y un *preflight* sin Windows; solo ejecutará Unity si `tools/ciudad2d-native/request.json` está explícitamente habilitado y el registro de `main` reconoce una única reserva `windows-runner-heavy` para el mismo workstream y rotación. Sin coincidencia, **SKIP**, no intento de apropiación.
- [Preflight real 43968934630](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43968934630): completed SUCCESS, nativo **SKIPPED** por falta de reserva legítima. La reserva de Windows en el registro se rota entre otros experimentos; no expulsar al titular ni alterar `main`.
- Para PlayMode, pruebas de dispositivos y compilación de la beta hace falta obtener el turno nativo en la coordinación existente y ejecutar un gate específico posterior; **no se certifica por ejecutar este preflight**.

## QA artístico honesto

**Puntos favorables visibles en capturas Chromium:** progresión I→V claramente reconocible, contraste del Bastión, composición y delimitación de edificios, mayor riqueza gracias a 9 props, lectura táctil de elementos centrales, escala útil en paisaje.

**Defectos observados:** terreno demasiado geométrico y simétrico, exceso de vacío intermedio, secundarios visualmente más sencillos/claros que Bastión V, ausencia de actividad animada real, límite de riqueza de los PNG en zoom alto, encuadre de paisaje que necesariamente recorta la muralla superior/inferior. El prototipo sigue siendo estático. No hay datos de uso de memoria GPU, rendimiento Unity ni estabilidad iPhone.

**Siguiente arte solo después de gate Unity:** reforzar la jerarquía urbana del Bastión y transiciones orgánicas entre parcelas sin colocar más objetos aleatorios; unificar arquitectura y gama tonal de secundarios; animación ambiental muy contenida y reutilizable (humo/antorchas/estandartes) solo cuando pueda comprobarse en motor.

## Regla permanente

Distinguir siempre **archivo versionado**, **verificación offline**, **prueba Chrome**, **ejecución Unity**, **prueba física**, **aprobación artística**. Ninguna etiqueta PASS técnica implica ART_APPROVED. No fusionar en main, ni publicar builds, ni modificar el juego real desde esta rama sin autorización posterior.
