# Fondos de producción de Vigilantes y Exploradores — 28/10/2026

## Diagnóstico y decisión

Las capturas nativas de 40688340094 demostraron que los recortes de 512×512 se ampliaban en exceso: faltaba paisaje y el mapa perdía detalle. Más ajustes de posiciones no resuelven esa causa. Se conservan las fuentes anteriores y se incorporan dos capas ambientales hermanas de 1536×1024. La referencia original aprobada sigue siendo `Camino de los Vigilantes: Engendro de la Brecha.png`; no se sustituye la identidad del Engendro del Bastión II.

## Producción

Herramienta integrada `image_gen`, sin CLI/API externa, claves, Tripo ni créditos externos nuevos. Uso: derivación de fondos de la referencia aprobada, sin interfaces ni valores jugables incrustados. Los hitos, contratos, etiquetas, recompensas y botones siguen siendo controles y datos reales de Unity. El recurso anterior queda disponible como fallback; la certificación de esta revisión exige los recursos v2.

Prompt Vigilantes:

> Create a production game-background asset by faithfully extracting and expanding ONLY the LEFT illustrated landscape from the supplied approved Eldoria concept reference. Preserve its exact rich painterly medieval-fantasy art direction, warm sunset gold and peach light, blue distant mountains, impressive fortified stone castles on cliffs, deep green pines, waterfalls and stone bridges. Produce ONE crisp high-detail landscape background, 1536x1024 or larger. The view must show the WHOLE sweeping valley and architecture, not a zoomed-in patch of a cliff. A winding empty adventure path should ascend from lower left toward upper right, with usable path space predominantly on the left half; the lower right will later receive a real game information panel. This is only environmental artwork for a real Unity UI. Remove ALL interface elements: absolutely no text, letters, numbers, stage markers, coins, badges, buttons, cards, panels, title bars, reward symbols, progress bars, checkmarks or borders. Remove the creature and all foreground UI entirely. Reconstruct the scenery naturally where those overlays were. Keep the reference's composition, atmosphere and decorative quality rather than reinterpreting it into another style. No UI mockup, no combined screens, no baked gameplay values, no watermark.

Prompt Exploradores:

> Use case: background-extraction. Asset type: production Unity game environment background, 1536x1024 landscape. Input image: approved reference, use ONLY its RIGHT cartographer scene as the style and composition reference. Extract and expand into ONE crisp detailed panoramic illustration of a medieval royal cartographer's parchment map laid on a warm wooden expedition table, golden candlelight, curled parchment edges, inked mountains, rivers and small fortified towns, brass compass and dividers near outer edges. Preserve the exact rich painterly medieval fantasy reference style, warm gold and parchment palette, atmospheric light and illustrated decorative quality. Full wide tabletop view, not a blurry zoomed patch. The map fills the central canvas and will receive separate real interactive cards in Unity. Remove ALL interface overlays and reconstruct the map underneath: absolutely NO text, letters, numbers, mission cards, buttons, status labels, reward icons, coins, stage markers, progress bars, title bars, borders or watermark. No combined UI screens. This is a clean environmental art layer, never a UI mockup.

## Recursos y verificación

- `Unity/Assets/Eldoria/Resources/EldoriaRetention/vigilants-painterly-v2.png` — SHA-256 `9503ca15c74e0e41d3268f7af9e4c5a0c24aabf4e4ecbae2a0d64e3585796149`.
- `Unity/Assets/Eldoria/Resources/EldoriaRetention/scouts-painterly-v2.png` — SHA-256 `e3e95332f84cdcaa7bf93a2ab0024ad228384527c3b6adc1649b8dd687f46c67`.

La inspección de las imágenes fuente confirma paisaje y mapa sin UI incrustada. No constituye aprobación del producto: se requieren las 17 capturas de Unity de esta revisión, sin recortes de texto, tres envíos y tres cobros reales, XP offline y persistencia. Mantener `art_approved: false` hasta revisar exactamente esa evidencia. No se presume prueba física Android.
