# Región 1 — revisión de continuidad y primera consolidación

Revisión directa del director ChatGPT de los tres PNG canónicos originales y de las seis vistas Unity40337702256, además de madera, cantera, jabalí, engendro y Brecha cercana en ambas orientaciones. **Valoración global aproximada:7,5/10**, para la primera consolidación solicitada. Es un juicio artístico del autor; no una nota calculada por CI ni una aprobación externa. La cercana pesa60%, regional25% y panorámica15%, por la prioridad de uso expresamente indicada por el propietario.

| Vista | Estimación | Resultado observado | Diferencia pendiente |
|---|---:|---|---|
| Cercana | 7,6 | Recursos grandes y reconocibles, bosque subordinado, terreno claro con detalle, río continuo y Brecha realmente accesible. | Algunas siluetas, rocas y contactos de ribera aún son más simples que la ilustración. |
| Regional | 7,5 | Ciudad, recursos y amenazas conservan lectura; la Brecha se integra mediante el mismo pigmento, sin el borde escalonado de403313. | La distribución histórica de ciertos caminos/bosques y las placas minerales siguen siendo regulares. |
| Panorámica | 7,2 | Región completa, poco mar y montaña, niebla solo periférica, sin actores ni objetivos. | Costa demasiado uniforme y lectura de masas más simple que la referencia. |

**Observación.** El contraste no se resolvía con otro tinte. Los cuatro pigmentos originales tenían medias y frecuencias de detalle distintas; además, la pradera y la roca aplicaban transiciones diferentes sobre caras contiguas de la Brecha. La fuente central tiene una luminosidad media aproximadamente25% superior a la fuente exterior; eso es un dato de las imágenes de origen, no una medición de calidad del render.

**Decisión.** Conservar la claridad y riqueza cercana y corregir el exterior con una superficie de hierba común. Concentrar el lote final en continuidad del terreno y transición mineral, siguiendo la revisión80/20 aportada por el propietario. Mantener la geografía y la autoridad jugable existentes.

**Corrección.** Hierba mediante el mismo albedo pintado y el mismo grano AF neutro en coordenadas del mundo; las imágenes originales siguen intactas y permiten conservar tierra/caminos. Una respuesta de luz de pradera común conserva sombras reales. El pigmento geológico original se comparte entre las caras de pradera y basalto con la misma transición radial. Se comprobó su mapeado sobre13.555vértices de Unity: error máximo2,03e-7 en UV. Los cinco revestimientos reciben normales de los vértices físicos coincidentes. La Brecha conserva profundidad, mineral central y tres luces reales; las vetas saturadas se moderan.

Nodos cercanos:20,4m frente a24m de ancho de ciudad, exactamente85%. Copas cercanas limitadas a9,12m; ambas cámaras cercanas usan el modelo próximo. Jabalí10,8m y engendro13,2m de ancho. El puente próximo mantiene21m y barandillas de madera de mayor contraste. No se han añadido campamentos ni migrado BastionesIII–V.

**Veredicto.** RETAINED para sustituir el mundo activo en main tras los controles de integración. La continuidad material mejora de forma visible y desaparece el corte escalonado de la Brecha; el detalle próximo se conserva. La regularidad de algunas agrupaciones y de la costa queda documentada para el siguiente acabado. No se presenta como igualdad con la ilustración ni como validación física móvil.

Métodos descartados: manchas sinusoidales tipo camuflaje; color constante solo en la extensión que borraba caminos; máscara de caminos segmentada por color que producía falsas manchas; interpolación de normales junto a facetas muy estrechas, rechazada por la nueva prueba. La búsqueda incondicional del pigmento al terminar criaturas falló en40335650854; se corrigió inicializando la textura una sola vez en el paisaje. No se relajaron pruebas o tolerancias.

Evidencia técnica independiente:62/62 PlayModeGPU,26/26 EditMode,44 controles de progresiónI–II, cero fallos u omisiones. Verificados160manifiestos de cámara/PNG y34PNG generales. Las seis cámaras coinciden con40253785923 y40324796014. Esto demuestra origen y regresión, no belleza.

Código:75fcc8b9faaa1272d66f9b1a134c0c422db3ddb1 · materialización:80d50a4aefec2368d04d43acab9f8626f5080ee0 · [ejecución nativa](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40337702256).

[Seis vistas y fuentes editables](sources/native-40337702256/review/) · [Verificación independiente](NATIVE_REVIEW_40337702256.json) · [Calibración material](MEADOW_CONTINUITY_CALIBRATION_20261026.json) · [Textura pintada, prompt íntegro y procedencia](PAINTED_MEADOW_ASSET_20261026.json).
