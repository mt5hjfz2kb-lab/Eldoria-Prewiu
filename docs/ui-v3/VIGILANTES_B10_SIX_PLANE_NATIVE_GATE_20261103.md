# Eldoria — Camino de los Vigilantes B10, profundidad auténtica en 6 planos

## Motivo y base
Rechazado B8/B9 como acabado artístico 8,3: el paisaje seguía pareciendo una fotografía aun con máscaras, sombreado y 25 cimientos. El propietario exige separación real de castillos, puente, acantilados y primer plano, sin mover el Engendro aprobado fuera de su tarjeta ni alterar la progresión.

- Baseline nativo B9: PR #136 commit `1fa21d8edf6f6850346503b006a28def7887e800`; [run 41671847626](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/41671847626), PASS.
- Fuente pictórica original SHA256 `9503ca15c74e0e41d3268f7af9e4c5a0c24aabf4e4ecbae2a0d64e3585796149` (1536×1024). No se modifica.
- B10 source isolated: `art/vigilants-multiplane-b10-20261103`, CI `41684841922`, SUCCESS; seis capas y manifiesto generados sin créditos pagados.
- Seguimos en PR136 DRAFT. No fusionar #137 indiscriminadamente, `main` ni publicar.

## Intervención técnica, no composición simulada
`tools/vigilants-depth/bake_layers.py` separa seis áreas de la imagen aprobada con máscaras alfa, sombras de contacto y rim-light, guardándolas en `Resources/EldoriaRetention/DepthB10`: left_castle, bridge, right_castle, left_rock, right_cliff, near_cliff. Sus bboxes y hashes están auditados en `layer_index.json`. No son nuevos dibujos ni fotos adicionales inventadas.

`RoyalGoldV3VigilantsParallax` importa cada recorte mediante un `RawImage` independiente bajo el mismo `RectMask2D` del escenario; las dimensiones se ajustan por UV de origen para conservar la alineación en cualquier recorte portrait/landscape. Se desplazan a distintas velocidades con animación de baja frecuencia (hasta 5 Hz), sin afectar el camino ni el Canvas de controles. El paisaje materializado B9 y los 25 cimientos de etapa B8 siguen bajo/sobre estos planos de forma consistente.

El Engendro de etapa 15 permanece dentro de la tarjeta con su alfa auténtica, guardias 5/10/20/25 conservan PNGs independientes, premios/XP/combate/persistencia sin cambios.

## Criterios de aceptación obligatorios
- EditMode `RoyalGoldV3VigilantsMultiplaneTests`: exige seis PNGs reales de dimensiones verificadas, misma UV de recorte aprobada, no raycasts ni nuevos Canvas/EventSystem, sin contaminación de Exploradores.
- Captura nativa `EarlyRetentionNativeCapture`: comprueba que B10 realmente renderiza seis planos en la escena, no una caída silenciosa al antiguo overlay.
- A/B exacta Unity B9 vs B10: etapa 1, guardianes 5,10,15,20,25, victoria/derrota, horizontal 1600×900 y móvil 390×844, opcional vertical 900×1600 y horizontal móvil 844×390.
- Revisar bordes, doble silueta, grietas, sombras y posición de los 25 hitos; no aceptar si aparecen falsos duplicados. Medir rendimiento en dispositivo antes de afirmar FPS. No declarar mejora `≥8,3/10` sin revisar imágenes reales ni firma del propietario.

La CI inicial de B10 `41674653664` falló solo porque la rama había avanzado durante el bake; la nueva pipeline con checkout parcial `41684841922` produjo las capas y las publicó en la rama aislada. No se ha dado aprobación artística automática.
