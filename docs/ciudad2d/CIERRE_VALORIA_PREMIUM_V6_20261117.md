# Valoria 2.5D · recuperación y consolidación Premium V6 (17/11/2026)

## Estado y límites

**Mejor candidato conservado: Premium V6**, aislado en `art/ciudad-2d-hitos-1-5-20261116`. La escena auténtica de Unity se encuentra archivada, no reescribe `Experimento_Ciudad2D.unity` ni ninguna escena de producción. No se ha modificado `main`, guardados, economía, combates, comandos ni reglas de desbloqueo. No se ha fusionado ni publicado beta.

**Fase técnica A:** cerrada para Editor Windows: cuatro sprites premium, sombras, estado de Cantera construida exclusivamente como QA y pruebas auténticas. **Fase artística B:** mejorada y comparada; no declarar aprobación comercial definitiva sin revisión del propietario. **QA iPhone físico:** pendiente, no equivalente a CI verde.

## Fuente canónica y assets

- Bastión V y Bastión I: `ArtBible/Masters` (SHA exactos congelados en `master_manifest_v1.json`). Hitos 1–4 = Bastión I; desde nivel 5 = Bastión V; sin nuevos hitos de nivel inventados.
- **Cuartel:** `Sprites/Candidates/Cuartel_Premium_V2.png` con sombra separada, **escala de presentación 1,14** y apoyo recalculado por huella alfa.
- **Aserradero:** `Sprites/Candidates/Aserradero_Premium_V2.png` + sombra independiente, sin cambios de contenido.
- **Granero:** `Sprites/Candidates/Granero_Premium_V1.png` + sombra independiente, **escala aparente 1,15** heredada de V3, que mejora lectura móvil.
- **Cantera:** `Sprites/Candidates/Cantera_Premium_V1.png` + sombra independiente. En la demo normal Bastión V está `Available`, no construida. Solo se ve terminada mediante fixture de QA expresamente etiquetada, sin alterar `PlayerState`.

Todos los candidatos premium siguen separados de las versiones originales. El fondo orgánico se mantiene y la versión de 32 elementos decorativos se preserva como control A, frente al candidato de 23.

## Pruebas realmente superadas

| Qué | Evidencia verificada | Resultado |
|---|---|---|
| Integración premium original cuatro edificios | [Unity 44105205248](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44105205248) | Éxito: escena premium separada, 60 comparativas A–F, PlayMode y selección |
| Archivo de 60 PNG Unity V6 previos | [44109813247](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44109813247), `QA/ArtQualityV6_NATIVE_EVIDENCE.json` | SHA-256 y dimensiones verificadas; 10 originales preservados |
| Granero ampliado V3 | [Unity 44116723739](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44116723739) | 11 capturas auténticas; 1 PlayMode aprobado |
| Prueba del Cuartel con dianas extras V4 | [Unity 44129165952](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44129165952) | Técnicamente aprobada, **rechazada artísticamente** por tamaño excesivo |
| Dianas pequeñas en patio V5 | [Unity 44131713553](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44131713553) | Técnicamente aprobada, **rechazada artísticamente** por duplicación y efecto de pegatina |
| **Cuartel original ampliado V6** | [Unity 44133747143](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44133747143), commit de fuente `a922225f4f073aeb2d6081e8dd634f54109ca399` | **11/11 PNG SHA-256, Unity Editor 1/1 PlayMode, 0 fallos** |
| **Archivo permanente V3/V6** | [44135322287](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44135322287) | 22 capturas originales cotejadas, siete V6 preservadas, escena independiente y NUnit XML sin alterar |

`QA/ArtQualityV6Scale_NATIVE_EVIDENCE.json` registra los 22 SHA, controles A/B, dimensiones, `source_commit`, y los hashes de escena y NUnit XML. La prueba verdadera está también disponible en `QA/NativeV6Scale/PremiumV6_NUnit_PlayMode.xml`: **1 pasado, 0 fallos, 0 omitidos**. Comprueba etapas I/III/V, interacción Cuartel y HUD, cuatro sombras habilitadas solo por el estado construido, `UnderConstruction` y `Available`, además de Cantera terminada solo por setter visual QA. La escena auténtica, sin sobrescribir las demás, se conserva en `QA/ArchivedScenes/Experimento_Ciudad2D_PremiumV6_44133747143.unity`.

### A/B exacta V3 frente a V6

A cámara y escala iguales, `premium_bastion1_390x844.png` es **idéntica**. En III y V, las diferencias localizadas aparecen exclusivamente en la región visual del Cuartel: bbox `[2,186,125,280]` en 390×844. Los números de diferencia RGB son **diagnóstico de imagen, no una puntuación artística**. El original del Cuartel y sus sombras no se han repintado ni regenerado; solo se calibró su escala relativa.

También se capturó Bastión V a 1170×2532 y 844×390 y cuatro detalles cercanos a 1170×2532. Las imágenes de escenario con Cantera ya terminada son fixtures de QA y no partidas manipuladas.

## Evaluación visual crítica (subjetiva, a partir de Unity real)

| Elemento | Evaluación | Lo que aún limita la calidad |
|---|---|---|
| Bastión I/V | Referencia de mayor autoridad; monumento legible, heráldica, piedra y pizarra coherentes | Sin nuevas intervenciones justificadas |
| Cuartel V6 | Más presencia y mejor lectura, conservando blancos de entrenamiento originales y sin objetos copiados | Cerca del margen izquierdo en retrato; controlar área segura y recorte real en hardware |
| Aserradero V2 | Madera y maquinaria de producción legibles, suficiente personalidad | Algo más cálido que el Bastión en detalles; no corregir con filtros globales |
| Granero V1 con escala V3 | Sacos, carro, cereal y almacén visibles; mayor detalle a resolución alta | La masa arquitectónica todavía puede sugerir vivienda noble/posada en miniatura |
| Cantera V1 | Grúa y extracción reconocibles tras construir; sin falsa parcela activada antes del desbloqueo | El escalón geológico se debe vigilar en diferentes encuadres para evitar sensación de plataforma |
| Terreno, entorno y FX | Composición rica, atardecer cálido, vegetación y animación discreta; 23 props probado | Persisten zonas de composición muy simétrica y diferencias de detalle entre ciertos edificios |

**Calidad artística estimada del conjunto: aproximadamente 7,8–8,0/10 en capturas de Editor.** Es una estimación editorial, no un dato de CI, una nota independiente ni la aprobación definitiva del propietario. La parte baja del objetivo de 8–8,5 es plausible; **no hay evidencia para afirmar 8,5**. La principal oportunidad residual está en la identidad del Granero, no en más humo, brillo, nitidez, filtros o efectos. Se descartó una variante de Granero regenerada por menor coincidencia de materiales/riqueza respecto a la fuente aceptada; no se integró.

## Lo que sigue pendiente de verificar

1. **iPhone físico:** uso de memoria GPU y picos, FPS, estabilidad Safari/WebGL, zoom/pan y respuestas táctiles (sin hardware ni capturas reales de ese dispositivo en esta orden).
2. **Android físico si aplica:** estabilidad y memoria, no certificadas por Unity Editor Windows.
3. **Aprobación artística humana externa:** aceptar o rechazar la versión 2.5D respecto al listón 8–8,5, especialmente Granero y coherencia global en la pantalla real.
4. **Promoción a juego principal:** deliberadamente no realizada. El usuario debe aprobar cualquier integración en `main` o publicación.

## Método reutilizable y errores descartados

- No repetir pruebas 60 A–F si ya están archivadas y verificadas; crear escenas A/B de 11 fotogramas exactos para cada cambio real.
- Ejecutar **antes de Windows** el preflight de reserva, fuente y sintaxis de PowerShell. Nunca invocar `-quit` con Unity Test Framework `-runTests`; tests editor con `EnterPlayMode` se descubren con `-testPlatform EditMode`.
- Los cambios V4/V5 demostraron que más iconografía NO implica más calidad; quedaron congelados y se devolvió el foco a proporciones arquitectónicas y pintura maestra original.
- Rechazar ediciones de terrenos imperceptibles y geometría simplificada Blender ya probada inferior. No sobreescribir originales ni referencias inmutables.
