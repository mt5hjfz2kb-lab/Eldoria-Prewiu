# Corona de la Brecha — depuración artística del origen premium

Fecha: 20/11/2026. Motivo: auditoría pixel-a-pixel sobre los quince sprites **realmente horneados** del experimento, no una maqueta.

Diagnóstico contrastado al tamaño UI: la etapa de horneado dibujaba una segunda tarjeta rectangular sobre `identity_crest`, `resource_siglet`, `world_rune` y `objective_rune`, tapando gemas y relieves del material original. El centro oscuro del arte fuente ya aporta contraste suficiente para marfil; el overlay degradaba el estilo respecto a WOS/Kingshot. Se elimina, sin tocar fuentes originales ni crear paneles falsos. `Unity.Text` conserva números dinámicos y texto independiente.

Se identificaron tres clases adicionales de residuos en las fuentes artísticas originales:
- `identity_crest.png` tiene **396 píxeles por fila** de una línea horizontal de exportación de dos filas RGBA idéntico `(202,153,241,102)`, fuera del marco. Se reconstruye **sólo esa línea exacta** interpolando las filas originales contiguas. Hay prueba visual A/B local; desaparece la barra sin cortar las joyas.
- `icon_wood.png` tiene **618 píxeles alpha>20 de tres fragmentos desconectados** (una cúspide dorada arriba y dos residuos). Se conserva la pieza principal de 42.519 píxeles, se descarta el ruido, preservando 2px de anti-alias, y se normaliza su caja para que los troncos ocupen el botón completo.
- `panel_obsidian` tiene **241 píxeles alpha>20** de un fragmento flotante a la izquierda; `action_arcane` **48 píxeles**. Se conserva la pieza principal y se retiran sólo fragmentos desconectados. **No se limpia `icon_food`**, porque sus granos sueltos son parte intencionada de la ilustración.

Todo ocurre en `tools/valoria-corona-brecha/bake.py` sobre el arte original del propio proyecto y antes del importador Unity. Los `.png` bajo `premium/` son inmutables. El horneador conserva GUID/import settings, superficie transparente y resolución; mantiene el remapeo de paneles panorámicos de la revisión anterior y la fuente SVG fallback. El CI estático compara fuente/hash/metadatos; **no equivale a aceptar la calidad artística del juego**. Necesita una captura WebGL actual del HUD y PlayMode real de los tres estados.

No se modifican `main`, `Modular25DGameplay`, `LocalGateway`, reloj, coste ni guardado.
