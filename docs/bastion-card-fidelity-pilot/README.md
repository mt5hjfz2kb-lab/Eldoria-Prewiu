# Bastión — fidelidad de la ficha aprobada, comparada en Unity

**15/11/2026. Estado:** piloto Editor-only preparado en la PR #197; comparación Unity A/B/C y aceptación artística **PENDIENTES**. No tocar runtime, escena, beta, partida, comercio ni modelos. ART_APPROVED=false.

## Referencia exacta del propietario

`75B5D217-A914-4451-854D-AE54849E4A50(3).jpeg` — 864×1536, SHA-256 `f57f1250951c906777e52d5a2e3524d9a6dfea3b64f283da82ad44a4a50e6e13`. Objetivo: marco fino dorado/piedra, auténtica miniatura 3D suficientemente grande, acción borgoña, textos/nivel/precios/tiempo del juego, sin superposiciones. La captura completa es una referencia, no un sprite de gameplay.

**Corrección causal del diseño anterior:** el sprite `OwnerReference/carved_context` fue extraído de un **contexto ilustrado anterior**; NO es el marco exacto aprobado. En cambio, `ApprovedContour/owner_gold_border` sí deriva de la referencia aprobada. Hay que probar el segundo método en lugar de asumir que cualquiera de estos recursos equivale a la referencia.

## Tres opciones, misma escena, misma cámara y datos reales

| Variante | Fondo de la ficha | Botón | Observación |
| --- | --- | --- | --- |
| A — actual | `CleanRoom/panel` | `CleanRoom/upgrade` | Control actual |
| B — contexto anterior | `OwnerReference/carved_context` | `OwnerControls/upgrade_button` | Control alternativo; referencia **distinta** |
| C — contorno exacto | `PremiumSurfaces/panel_fill` + `ApprovedContour/owner_gold_border` | `OwnerControls/upgrade_button` | Hipótesis principal: un solo contorno real, interior limpio |

El contorno C es **una** textura transparente, sin texto fijo y sin fondo/placa oculta; cubre la misma ficha, sin cambiar ni una línea del controlador canónico. El panel de fondo B usa 9-slice, C tiene un material interior sin texto y un contorno 9-slice. El botón es una pintura única con texto verdadero encima; el retrato se sigue pintando mediante RenderTexture del modelo real.

## Dos niveles de evidencia, que NO deben confundirse

1. **Fuente, hospedado en Ubuntu:** `tools/bastion-card-fidelity-pilot/verify_sources.py` comprueba seis PNG originales por SHA, dimensiones y metadatos de importación. Genera `source-proof/source-components-only.png`, una comparación **solo de texturas aisladas**, y un JSON de procedencia. Roslyn solo verifica sintaxis; ni el PNG ni la sintaxis son renderizado Unity.
2. **Nativo, Windows Unity:** `Eldoria.BastionCardFidelityPilot.Editor.BastionCardFidelityCapture.CaptureBatch`. Abre escena real en sesión desechable, termina onboarding, selecciona Bastión real y captura A/B/C con la **misma cámara** en 900×1600, 390×844, 1600×900, 844×390. Exige **12 PNG** y `evidence.json` con SHA del render, modelo 3D vivo, interfaz anterior oculta, controles y save sin cambios. Todo elemento temporal se desmonta.

**La reserva Windows pertenece a PR #194 (P0 mundo/iPhone).** El workflow `.github/workflows/eldoria-bastion-card-fidelity-native.yml` debe rechazar ejecución sin reserva exclusiva CAS a nombre de este workstream, comprobada también tras la cola de Windows. No iniciarlo ni bloquear la recuperación de la beta.

## Validación artística antes de adoptar nada

Revisar las 12 capturas verdaderas frente a la imagen de 864×1536: marco/material y proporción; integración del 3D; renglones reales no truncados; botón y martillos sin choque; interacción, estado bloqueado, clic, safe area y armonía con el HUD. Si C o B empeoran A, rechazar sin añadir filtros. Solo tras mejora artística evidente validar WebGL y iPhone físico. Ningún test técnico emite ART_APPROVED.

**Estado:** A/B/C fuente preparada, pruebas Unity/WebGL/iPhone y decisión estética pendientes, sin merge ni publicación.

## Diagnóstico nuevo: 14/11/2026, captura genuina de Unity archivada

Recuperada y revisada la evidencia del **run 43623869301**, source `ae05d1c6b5e7deb949b98c3f3100fa85ced3ba81`, incluyendo cuatro capturas de Bastión y `breachwatch-native.log`. El Bastión aparece magenta tanto en ciudad como en su retrato 3D; el porcentaje de píxeles casi-magenta medido sobre todo el framebuffer fue **5,82 %** (390×844), **5,01 %** (900×1600), **4,25 %** (1600×900) y **3,79 %** (844×390). Son mediciones de ese candidato archivado, NO de una beta física actual.

El inventario del mismo run declara `Universal Render Pipeline/Lit shaderSupported=True` para las ranuras 8/9/10 del Bastión, mientras los logs contienen advertencias de subshaders no disponibles para otros shaders. Por tanto **no está verificada la causa exacta** de los polígonos magenta; tampoco sabemos si persisten en el último artefacto distribuido. Una propiedad Shader.isSupported=true **no basta** para dar por válido el render: comprobar los píxeles reales es indispensable.

El capturador de la PR197 rechaza el A/B/C completo cuando una captura real contiene **0,6 % o más** de píxeles con R/B altos y G bajo, preservando el PNG fallido y el error. Este umbral es una prueba diagnóstica específica de **Valoria/Bastión sin ambientación violeta**, no una regla universal para escenas de Brecha. No introducir un material de sustitución ni ocultar el fallo dentro del piloto: corresponde diagnosticar el pipeline/material del Bastión en su trabajo autorizado sin invadir PR194/P0.

La comparación de fuentes en Ubuntu **sí ha pasado** y ha producido la hoja de materiales A/B/C. Su revisión visual indica que el conjunto A conserva una estructura pictórica más próxima al objetivo que B o C por separado. La hipótesis del simple intercambio de sprites **no está aprobada**, aunque la decisión definitiva exige capturas nativas sin magenta y con textos/3D verdaderos.


## Resultado real nativo — 15/11/2026

**Unity Ejecutado y verificado:** [run 43744421786](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43744421786) / commit `a60654bc9c22325921a40aa72546582d3346fd65` / artefacto `14174167789`, 12 PNG originales y `evidence.json`. El código A/B/C se ejecutó en la escena canónica con Bastión, modelo 3D, HUD, textos, costes y botones reales; saveUnchanged=true, no magenta en las 12 capturas. Esto **NO** acredita iPhone físico ni calidad objetivo.

**Evaluación visual del fotograma a 900×1600 y 390×844:** A (interfaz actual) mantiene mayor riqueza y organización de marcos/textos. B (`carved_context`) parece otra estética, sobreimpone ornamento tosco y pierde legibilidad; **RECHAZADA**. C (contorno extraído + material neutro) deja marco rígido, estira decoraciones y tapa/recorta la descripción y costes; **RECHAZADA**. Tampoco A alcanza el concepto: miniatura 3D sin integración escénica/volumen comparable; tipografía y espaciados más pobres. **ART_APPROVED=false** para el cambio propuesto.

**Conclusión causal:** importar 2D exacto de una ilustración no preserva la calidad al 9-slice sobre el layout de nuestra ficha. No promover B/C, no fusionar, no repetir tintados. La ruta posterior debe trabajar una ficha compuesta de zonas reales independientes con marcos de esquinas no deformables, superficie central controlada, huecos de texto medidos y render del Bastión integrado en un recuadro mayor. En primer lugar, demostrar un solo componente en Unity auténtico antes de extenderlo.

**Recurso Windows liberado tras run verificado**. PR #197 se mantiene borrador para auditar el experimento rechazado; sin cambio funcional ni publicación.
