# Eldoria — control de fidelidad con referencia original (02/11/2026)

**Objetivo:** evitar confundir diferencias de píxeles, un Unity test SUCCESS o una nueva presentación técnica con una mejora visual del videojuego.

## Prueba real completada, no una simulación

- Capturas Unity anterior: run `41219845900` (fuente `011f1841a03eb0f954d4ec414599a738ae6211ce`).
- Capturas Unity posterior: run `41434883428` (fuente `6cb1dd1fe46be76dca51af932aead7f2fc189f79`).
- Kit de referencias originales aprobado en Biblioteca: `/Eldoria/Produccion-pendiente/ELDORIA_V41_KIT_ARTISTICO_DE_FIDELIDAD_20261029.zip`.
- Marchas: referencia SHA-256 `9afd178a57783f83bd0ccb5bbfb78c50643eb75ae74d01cbed63375d7dcd091c`; **0,94 %** de píxeles cambiados con umbral luminancia >15 tras normalizar anchura.
- Entrenamiento: referencia SHA-256 `a110ba0ffd2a08fa4976c488f87e624fa5601fe3f36b223ee82d19f306adbadc`; **0 %**, imagen exactamente idéntica.
- **Veredicto:** no hay evidencia de mejora artística sustancial entre esos dos commits para Marchas/Entrenamiento. Sí hubo avances relevantes en Salón y perfiles en esa época, pero no se pueden atribuir causalmente a Unity CLI/Codex, instalados después.

## Déficits observables y acciones correctivas

**Marchas — composición:** el bloque único y rectangular con texto de tropas/poder reemplaza al desglose visual aprobado; los héroes están ordenados, pero falta jerarquía en el análisis de tropa. Corregir mediante tarjetas modulares reutilizables, pictogramas de estadísticas reales y un panel de marcha proporcionado al escenario. No añadir paladines/brujos ni unidades ficticias que aparezcan solo en el concepto. Conservar T1/T2, porcentaje, capacidad, héroes, guardado y acciones.

**Entrenamiento — composición:** el arquero está bien integrado visualmente en el entorno, pero la cartela azul oscura tapa gran parte del plano inferior derecho y la información de costes/tiempo queda pequeña. Sustituir plano azul grande por cartelas escalonadas y marcos artísticos nine-slice; conservar cantidad, costes, tier y temporizador real. No reemplazar la unidad 3D/arte aprobado.

**Criterios de certificación:** comparar capturas auténticas `941×1672` / `554×542` en los estados equivalentes y además dispositivos `1600×900`, `900×1600`, `390×844`, `844×390`. Conservar EditMode/PlayMode y guardado. Una mejora real precisa ART visual review independiente antes de publicación. Si A/B es idéntico en una pantalla prometida como mejorada, marcar **SIN CAMBIO DEMOSTRADO**.

## Uso local (no sube ni publica las imágenes originales)

```bash
python -m pip install Pillow==11.3.0
python -m unittest discover -s tools/visual-qa -p 'test_reference_triage.py' -v
python tools/visual-qa/reference_triage.py \
  --before-zip antes.zip --after-zip despues.zip --reference-zip original_aprobado.zip \
  --before-sha <40_sha> --after-sha <40_sha> --output output
```

`output/index.html` incluye referencia/antes/después; `output/result.json` deja mediciones y SHA verificadas. Las referencias siguen en el kit aprobado original, no se regeneran ni se copian a `main`. El código es read-only y no consume Windows. **No asigna puntuación artística automática ni aprobación.**

## Causalidad y límites

Esta prueba mide lo cambiado del juego entre dos versiones **anteriores** a la integración Codex↔Unity. No demuestra que instalar el nuevo agente haya mejorado el juego ni que haya aumentado los FPS. Para medir el beneficio real de la IA, necesitamos un cambio dirigido por agente sobre una pantalla real, con un grupo de control comparable en tiempo/calidad y A/B Unity del mismo HEAD. El trabajo visual continúa bajo PR #136, sin duplicar propietario.
