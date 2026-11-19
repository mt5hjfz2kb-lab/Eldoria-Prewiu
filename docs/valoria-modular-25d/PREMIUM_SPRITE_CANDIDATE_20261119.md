# Valoria Modular 2.5D — nueva familia de sprites de edificios

## Integración materializada en rama aislada

Se sustituyeron **únicamente** los dos PNG de acabado definitivo en esta candidata: `bastion_i.png` y `sawmill_i.png`. El mundo y la lógica de construcción siguen consumiendo los nombres originales; no se cambiaron las reglas económicas ni el guardado. Ambos tienen fondo RGBA con pivote situado en el pie del edificio y **96 píxeles de imagen por unidad del mundo**. Las superficies tienen una perspectiva 3/4 isométrica, luz cálida desde arriba a la izquierda, piedra envejecida, madera, cubierta pizarra y símbolos imperiales originales. Este material NO copia modelos comerciales.

| Recurso | Tamaño | SHA-256 | generación de origen |
|---|---|---|---|
| Bastión I | 480 × 314 | 5992c968f67c6855a97eb5db4d24c9bcd4e06af5b1062c25cc35d0e1e27e3154 | ac1ba197-84c5-407b-86ad-c83c53e73ec2 |
| Aserradero I | 362 × 273 | 5a5c5f38604881da76d43c65eb999e69736d5bcaaf0d432df0661163ccde81f6 | e475b613-4030-4fd2-9c0d-0c8fce453220 |

Los originales sin recortar proceden de generación de imágenes durante este encargo y los recortes alfa y escalas se realizaron fuera de Unity; el Aserradero contiene sombras de fondo que deben revisarse sobre terreno real. Se versionaron PNG y `.meta` con GUID conservados, por lo que las referencias de sprites existentes no cambian.

**Diferencia esencial:** estos PNG son recursos de juego, no captura de pantalla. Que el conector haya subido los blobs no demuestra que estén renderizados, sean coherentes en movimiento, ni que exista una compilación WebGL. Las capturas auténticas y PlayMode siguen pendientes; NO ART_APPROVED, NO MERGE, NO BUILD RELEASE.

La generación de las imágenes por IA no es determinista. Su reproducibilidad real se basa en **versionar cada PNG de salida, su hash, pivote y convención**, no en afirmar que repetir un prompt garantiza el mismo píxel. Los próximos edificios deberán compartirse mediante una familia de fuentes con contraste y revisión nativa; no reconstruir una imagen gigante.

## Diagnóstico visual antes de cerrar

- Comprobar escala real en 390×844 y 1170×2532 con el mismo zoom y sin UI solapada.
- Revisar sombras y recortes contra el suelo continuo, especialmente el halo del Aserradero y el contacto del Bastión con camino.
- No aceptar árboles de baja poligonización que contradigan el estilo de los edificios. El resto de sprites actuales son candidatos transitorios.
- Capturar oclusión de habitantes, estados de obra, cierre/reinicio y funcionamiento real de los botones.
- La reserva Windows de otro workstream sigue impidiendo apropiarse del recurso; los test Ubuntu no la sustituyen.

**Rama:** `experiment/valoria-modular-25d-bastion1-from-zero-20261118`. La referencia anterior de Blender está conservada en el historial Git y su estado era `ART_REJECTED` al nivel exigido; no asumir que el nuevo resultado ya ha superado la revisión.
