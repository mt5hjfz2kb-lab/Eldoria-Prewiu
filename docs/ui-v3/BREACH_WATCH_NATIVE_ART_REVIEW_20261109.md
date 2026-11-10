# Vigilia de la Brecha — evidencia Unity nativa y decisiones de arte (09/11/2026)

**Estado: primera pantalla funcional nativa probada; arte pendiente de aprobación subjetiva.** PR #173 sigue en borrador y sin merge ni despliegues.

## Fuente y prueba exactas
- Último commit renderizado: `a5e8fa22759733bc04ab8040e8f58cf079740d1e`.
- [GitHub Actions #42617442201](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42617442201): compilación Unity 6000.3.23f1 y captura nativa **SUCCESS**.
- [ZIP de los 8 PNG y reporte del mismo HEAD](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42617442201/artifacts/13673516185): ciudad y Bastión seleccionado en **1600×900, 900×1600, 844×390, 390×844**.
- Reporte `BreachWatchNativeEvidence.json`: `screenshotCount=8`, `functioningNewNavigationClicks=6`, `saveUnchanged=true`, `oldCanvasVisible=0`, `oldHudGraphicsReused=false`, `artworkApproved=false`, `androidTested=false`.
- Tests fuente: sintaxis C# Roslyn, integridad del arte/licencias/SHA y 20 escenarios geometría (5 tamaños × comida sí/no × Mundo sí/no) superados antes de ejecutar Unity.

## Problemas constatados y correcciones reales
1. **Accesos antes de existir el sistema**: ya no aparecen Ejército/Héroes ni los tres sellos sin pantalla auténtica. Explorar solo cuando el gateway lo permite; comida solo con `ShowFood`. Los 2–3 medallones visibles se centran y mantienen hitbox/arte separados.
2. **Botón borgoña aparentemente válido pero visualmente desbordado**: el run [42614132511](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42614132511) había superado pruebas mientras los caracteres «II» sobresalían de la pintura roja en 1600×900. La causa era la altura constante de 36 unidades del sprite original 246×60: `preserveAspect` lo estrechaba dentro de un botón invisible más ancho. Se sustituyó por **una única imagen con ancho de pintura completo y altura calculada de la proporción original**; el texto dinámico conserva «Mejorar a Bastión II» en resoluciones de ancho ≥800. QA nueva comprueba geometría de arte REAL, no solo hitbox.
3. **Miniatura verde artificial**: la segunda cámara pintaba un cuadrado de tierra Perlin tras las mallas verdaderas del Bastión. Comparación A/B [42616076076](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42616076076) → [42616667842](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42616667842): se retiró ese escenario ficticio, se dejó el modelo 3D genuino sobre fondo mineral neutro con **un solo render**. Aumenta la coherencia con los paneles bronce/basalto; conserva guías GPU de recorte y no altera el mundo.
4. **Panel apaisado comprimido**: en 844×390, la ficha medía 210 unidades y dejaba únicamente 86 a la descripción de Bastión. La ampliación localizada a **234 unidades** muestra mejor la descripción y el CTA completo. La captura final real pasa las pruebas de tamaño/tap y no tapa el camino/puente.

## Regresión visual comparada
Comparación raster A/B de los PNG nativos `42616667842` vs `42617442201`, con umbral de diferencia RGB>12:

| Vista | Diferencia >12 | Interpretación |
|---|---:|---|
| Ciudad 1600×900, 900×1600, 844×390, 390×844 | **0 %** | Mundo sin cambios |
| Bastión 1600×900, 900×1600, 390×844 | **0 %** | No se alteraron layouts ya aceptables |
| Bastión 844×390 | **7,2606 %** (bbox x629..827/y52..318) | Cambio restringido a la ficha horizontal |

## Límites que permanecen
- **No ART_APPROVED.** Tests técnicos, logos e imágenes conceptuales no prueban que el nivel gráfico final iguale la referencia vertical elegida.
- Esta PR valida **una primera pantalla autónoma**; Ejército/Héroes, correo, otras pantallas y el conjunto de juego requieren interfaces independientes reales, no botones de mentira.
- No son pruebas de APK/WebGL publicado ni rendimiento físico iOS/Android.
- El mundo 3D, suelo, vegetación y calidad de modelos son del proyecto canónico, no se han alterado para maquillar una captura.
- En los logs Unity aparecen advertencias heredadas de otros sistemas (p. ej. `BASTION_III_MISSING_ART` y una excepción `UnityEditor.Search.SearchDatabase` del editor); no equivalen a una certificación global del juego. Resolver en sus workstreams correspondientes.
- La reserva `windows-runner-heavy` fue liberada tras la última ejecución nativa exacta. Mantener la PR draft y no fusionar/publicar sin aceptación visual del propietario.

## Auditoría visual acumulativa — 10/11/2026, Unity exacto `3f04846b`

- **Prueba auténtica:** [8 capturas Unity 6000.3.23f1 / run 42858352697](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42858352697) en el mismo encuadre 9:16 que la imagen del usuario `75B5D217-A914-4451-854D-AE54849E4A50(3).jpeg` (864×1536, imagen conceptual aprobada). Ambas muestran un Bastión real y la ficha del edificio. `artworkApproved=false`. No puntuación inventada.
- **Hallazgo sustantivo por inspección directa de píxeles:** la cubierta del Bastión real aparece **pizarra azul grisácea desaturada**, no las tejas **azul cobalto saturado** del concepto. En regiones aproximadas de 310×235 píxeles del castillo aprobado (x355..665, y340..575) y 350×250 del Unity genuino (x340..690, y380..630), los píxeles HSV con H=93..129, S≥140, V≥35 representan **15,18 % del ROI aprobado contra 2,22 % del ROI Unity**. Es una medida descriptiva condicionada al recorte, NO un 12,96 % de calidad ni una puntuación de estilo. La selección del ROI no es perfecta porque las composiciones difieren; sirve para diagnosticar la pérdida de cobalto.
- **Diagnóstico arquitectónico:** la paleta de cubiertas pertenece a las mallas/materiales REALES del Bastión; reescalar la tarjeta, cambiar el borde dorado o dibujar una película azul semitransparente encima del Canvas **no reproduce esas tejas**. Los experimentos anteriores de coloración de bordes del Canvas ya fracasaron en mejorar la coherencia. No volver a repetirlos. La corrección de cubierta requiere identificar de forma exacta los materiales/submeshes reales, evitar tocar piedra, banderas o puertas y validar los dos encuadres de la ciudad MÁS el retrato 3D; un mero tinte de toda la tarjeta sería un falso avance.
- **No confundir el concepto con estado de juego:** el referente enseña tres recursos, cinco medallones, tres utilidades, un establo y una tienda azul. La primera fase genuina tiene únicamente dos recursos/desbloqueos y no existen esas pantallas/edificios como destinos operativos. **No activarlos ni dibujar botones ficticios para acercar artificialmente el A/B.** Medir por separado fidelidad de los componentes realmente disponibles y diferencias del progreso narrativo.
- **Preparación técnica independiente (HEAD `eedda44f`):** en `BreachWatchBuildingPortrait.cs` se instrumentó, exclusivamente bajo `UNITY_EDITOR`, inventario limitado a 48 submateriales (nombre, color y shader) del Bastión real. No cambia la escena ni sus materiales. Única validación necesaria para decidir un tratamiento reversible de cubierta es ejecutar el siguiente Unity exact-HEAD **después** de adquirir `windows-runner-heavy` por CAS. En este instante PR181 tiene el recurso; no invadirlo ni aceptar source CI como arte.
- **Criterio de decisión:** tras obtener inventario, modificar solo el submaterial inequívocamente identificable como cubierta (en candidato opt-in y reversible), comparar 8 capturas nativas contra la referencia y contra `3f04846b`, examinar colores de cubiertas y legibilidad/recorte/acciones/guardado. Si no hay manera segura de aislar la cubierta, descartarlo; no cambiar por tanteo el material de todo el castillo. El propietario tiene la última decisión estética y la PR sigue draft.
