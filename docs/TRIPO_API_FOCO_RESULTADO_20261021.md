# Eldoria — Tripo API / Foco de corrupción: evaluación técnica y artística

Fecha: 21-10-2026. Estado: **fuente 3D creada, validada y conservada; NO integrada ni aprobada todavía como prop definitivo del mapa**.

## Alcance y canon

Este es un **prop experimental** de roca/corrupción asociado visualmente a la Brecha, no un «nido» jugable, un nuevo nodo de `v0220`, un campamento, un altar, ni una misión. Las reglas, hotspots y escena vigente Bastión I–II no se tocaron. El objetivo era evaluar una segunda familia artística distinta del Engendro, reutilizando Tripo API → Blender → Unity.

## Evidencia verificable

| Fase | Evidencia |
| --- | --- |
| Tripo API | H3.1, modelo `v3.1-20260211`, **una sola generación** de 20 créditos, task `3800e44b-2718-454d-b904-dbe56a842c33`, [run 39513703787](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39513703787) |
| Original | 82.446 triángulos; 45.346 vértices; GLB de 6.754.328 bytes, SHA-256 `c1d0e458b1eb52f1a31a04361f18985cb3e14967ad4f8abf99073a696be07ae0` |
| Blender | 49.799 triángulos; GLB 6.291.500 bytes, SHA-256 `00511927ba818a1f5bf4193e2ee1c3533d1b5ac383c8727c9c7c0f5a49f7c53e`; .blend editable SHA-256 `560006c58071139c582bb3ddcae07eac7bcb822d32a98deebbc9f71a20b86a2b` |
| Mapas | BaseColor 2048²; Normal 2048²; ORM reducido explícitamente a 1024² por Blender |
| Unity aislado | Unity 6000.3.23f1, 49.799 triángulos / 28.951 vértices importados, 1 mesh, 1 material, 3 texturas y BaseColor **2048×2048 realmente importado**, UV0/normales/raycast positivo y miss vacíos PASS; [run 39554609943](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39554609943) |
| Archivo permanente | [`ArtSource/World4X/RiftFocus_v1/`](../ArtSource/World4X/RiftFocus_v1/): original/optimizado/.blend, `UnityQA/` con cuatro PNG auténticos y métricas; [run de archivo 39554987188](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39554987188) |

## Revisión artística, independiente del TECH PASS

**SOURCE:** técnicamente aprovechable como bloque de roca corrompida. Desde tres cuartos se percibe un grupo compacto de masas rocosas irregulares, hendiduras y un área violeta central. Tiene superficie mineral con volumen real, pero **no es todavía un foco narrativo visualmente distintivo**. Las grandes rocas redondeadas compiten por protagonismo, la corrupción se lee como fragmento cristalino púrpura pegado entre piedras y no como fenómeno que transforma el conjunto. La geometría general parece una agrupación de cantos, la paleta de roca es gris claro bajo la prueba neutra y los destellos/blancos puntuales son susceptibles de lookdev. Desde el zoom estratégico pierde la lectura violeta y se parece a una roca común.

**Calidad artística provisional:** aproximadamente **5/10** para destino fantástico singular (criterio subjetivo del revisor, no escala certificada del proyecto). Veredicto **TECH PASS, ART_SOURCE_CANDIDATE, ART_APPROVED PENDIENTE**, rendimiento móvil NO VERIFICADO, integración jugable NO.

**Próximo tratamiento sin nueva API:** corregir prioridades en Blender: cambiar silueta de varias bolas redondeadas por un par de planos pétreos conectados; dirigir fisuras violetas entre los bloques principales, reequilibrar tonos menos claros/reflectantes y resolver contacto rocoso con terreno existente. Comparar desde cámara canónica del mundo 4X; descartar si sigue sin representar claramente «corrupción» de la Brecha. No añadir partículas, glifos ni nuevos hotspots para maquillar una pieza confusa.

## Lecciones adicionales

1. El tipo de objeto importa: Tripo generó una base utilizable, pero el modelo se acerca más a **rocas agrupadas** que a un emblema narrativo de la Brecha. La IA no decide la composición de un recurso de lore.
2. Reducir a ~50K y conservar 2K puede servir como **prueba de fuente**, no como presupuesto razonable para un prop secundario móvil repetido. Evaluar distancia, LOD y memoria de texturas.
3. Separar `SOURCE_GENERATED`, `UNITY_TECH_PASS`, `ART_APPROVED`, `MOBILE_PROFILE` y `INTEGRATED`. Un workflow verde no certifica ninguno de los últimos tres.
4. La generación exacta del Foco costó 20 créditos. Las posteriores QA y conservación utilizaron el GLB ya pagado, sin más crédito. La solicitud canónica de Tripo debe quedar con `enabled=false` y `credit_authorization.approved=false`.

Documentos relacionados: [lecciones del Jabalí](TRIPO_API_JABALI_LECCIONES_20261021.md), [norma reutilizable](TRIPO_API_NORMA_PRODUCCION_V1.md), [briefs originales](TRIPO_API_ENGENDRO_FOCO_BRIEFS_20261021.md), [Engendro: informe separado](TRIPO_API_ENGENDRO_RESULTADO_20261021.md).
