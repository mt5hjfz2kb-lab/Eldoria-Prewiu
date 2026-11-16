# Eldoria CC0 Art System — estado de continuidad y evidencia

**Experimento registrado:** `Eldoria CC0 Art System — Sistema artístico gratuito reproducible`.  
**Workstream ID:** `eldoria-cc0-art-system-20261116`  
**Rama aislada:** `experiment/eldoria-cc0-art-system-20261116`; **no mergear ni modificar main**.  
**Fecha de actualización:** 16/11/2026. Leer también `README.md`, `asset-lock.json`, `ART_REVIEW_AND_PIVOT_20261116.md` y `main/AGENTS.md`.

## Qué se ha investigado, hecho y probado de verdad

| Línea | Estado | Evidencia |
|---|---|---|
| Reglas de arte, recursos comerciales gratuitos y derechos | Documentado; licencias externas aún no acreditadas pieza a pieza | `README.md` y `asset-lock.json` |
| Bastión original I–V y composición inicial | **Modelos y Blender probados**; aspecto insuficiente | [Action 43847137592](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43847137592) |
| Importación de Quaternius auténtico | **Probada en Blender**, seis archivos originales SHA-256 | [Action 43844725460](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43844725460); fuente privada, no redistribuir |
| Mezcla directa Quaternius/fortaleza propia | **Rechazada artísticamente**; siluetas y materiales distintos | [Action 43847893229](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43847893229) |
| Ciudad cohesionada y edificios diferenciados | **Blender probado**, estéticamente solo parcial | [Action 43851647751](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43851647751) |
| Agrupación estática de mallas I–V | **Blender/GLB probado**; no equivale a medición GPU/iPhone | [Action 43853428203](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43853428203) |
| **Director V3**: funciones propias y bosque | **Blender + 5 GLB + 3 capturas reales**, render con problemas artísticos | [Action 43866662062](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43866662062) |
| **Director V4**: nuevos materiales y fachada | GLB y renders generados; **primer gate global rechazó progresión visual insuficiente** (3,30 de diferencia media a escala de imagen) | [Action 43870376065](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43870376065); otros runs V4 revisar al retomar |
| **Director V5**: corrección geométrica, entorno y 5 niveles | **Implementado en código real Blender**, workflow Ubuntu disparado. **No declarar pass hasta terminar run y revisar imágenes** | `tools/cc0-art-system/director_v5_scene_refine.py`; `.github/workflows/cc0-director-v5.yml`; primera ejecución [43873251581](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43873251581) |

## Diagnóstico artístico acumulativo

En los renders auténticos de Director V3/V4 seguían apareciendo cubiertas poco trabajadas, bosques con copas cónicas y claras, un plano de tierra demasiado uniforme y fortaleza V insuficientemente superior a I al examinar todo el encuadre. La referencia conceptual generada con IA **NO ES** modelo, render Unity, ni garantía de producción posible. No volver a presentar ilustraciones como avance del juego.

El nuevo Director V5 modifica físicamente: **sendas sobre terreno (sin plataformas), copas de coníferas irregulares y oscuras, rocas y hierbas ancladas, maquinaria y abastecimiento de cuatro sectores, torres y elementos verticales exclusivos del Bastión V, iluminación cálida legible.** 5 exportaciones glTF se regeneran desde V4 real anclado al run 43870376065; guardan datos reales de geometría, no una imagen pegada.

## Estado de pruebas obligatorio (NO confundir)

- **Blender V5:** pipeline lancé; inspeccionar logs y PNGs **en persona**; no hay aceptación visual automática.
- **Importación/capturas Unity 6000.3.23f1:** pendiente. La escena nativa del proyecto no se ha sustituido ni probado con Director V5.
- **Rendimiento iPhone WebGL/Android:** pendiente. El número de mallas debe reoptimizarse para nueva geometría y probar memoria/draw calls reales; una reducción de mallas en Blender no demuestra FPS.
- **Licencias externas comercializables:** solo paquete original, perfil, edición y destino autorizados. V5 crea geometría propia y **no incorpora** archivos originales de terceros. El repositorio GitHub sigue siendo público.
- **Calidad artística:** V3/V4 no aprobadas. V5 es candidata, **no aprobada**.

## Continuación en cualquier chat

1. Leer `main/AGENTS.md`, `main/pipeline/active-workstreams.json` y PRs abiertas. No cancelar, ocupar ni tomar Windows de otros workstreams.
2. Comprobar HEAD **real de esta rama** y jobs de `cc0-director-v5.yml`. En particular recuperar el run `43873251581` y su artefacto `eldoria-cc0-director-v5-real-3d`.
3. Si falla: leer el primer paso fallido y **corregir la causa**, no declarar éxito porque existan renders/GLB. Si pasa: abrir capturas reales I, V, vertical; comparar con V3, defectos de composición, iluminación, utilidad de industrias y progresión.
4. Si la calidad sigue siendo baja: reestructurar edificios/terreno, no encadenar retoques cosméticos. Registrar defectos persistentes y hipótesis de solución.
5. Solo tras inspección artística positiva: instalar modelos originales/optimizados en sandbox Unity aislado, probar los botones y escenas, capturar desde el Editor y validar dispositivo físico cuando sea posible. Sin fusionar ni publicar sin aprobación del propietario.

**Principio rector:** código ejecutado ≠ geometría válida ≠ calidad visual ≠ juego estable. Distinguir siempre estas cuatro cosas.

## Director V6 — reconstrucción estructural de presentación (16/11/2026)

**Motivo:** las capturas reales de V5 en [Action #43874433432](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43874433432) muestran caminos superpuestos claros, terreno plano con poco interés, árboles repetitivos y arquitectura secundaria poco diferencial. El modelo V5 sirve como base geométrica, **no está aprobado**.

**Código nuevo:**
- `tools/cc0-art-system/director_v6_structural_redraw.py`: importa cada GLB físico de V5; elimina mallas problemáticas, **reconstruye una superficie continua con senderos pintados por vértice**, añade coníferas irregulares reales, vegetación, texturas originales retocadas físicamente, refuerzos del Bastión V y siluetas de los cuatro edificios funcionales; genera 5 GLB y una fuente editable Blender.
- `.github/workflows/cc0-director-v6.yml`: Ubuntu alojado, sin Windows compartido ni gasto; input **exacto** del artifact V5 run `43874433432`; comprueba glTF/COLOR_0/texturas, 3 render PNG, salto visual I/V y límites de licencias. Artefacto esperado `eldoria-cc0-director-v6-real-3d`.
- Primera ejecución `43877803268`: Blender arrancó e importó V5; **FALLÓ** por uso de materiales borrados en el reinicio de escena `read_factory_settings`. Se corrigió regenerando los materiales tras cada reinicio; [segunda ejecución #43878136136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43878136136) es la referencia para comprobar el estado real. **No declarar PASS sin verificar conclusión y revisar PNG.**

**Prueba de aceptación del trabajo, por orden:** Blender fuente/modelos reales y capturas, inspección artística que demuestre corrección de suelo/vegetación/edificios, agrupación 3D actualizada por materiales/sector, integración Unity aislada mediante reserva Windows legítima y test móvil. El primer PASS es técnico, jamás ART_APPROVED.

**Aislamiento:** esta versión solo edita su rama; las mallas externas de Quaternius NO se incorporan al código. Ni main, ni publicación, ni fuentes del Bastión vetado, ni saves, ni gameplay.

## V6 confirmado; V7 material/arquitectura en comprobación (16/11/2026)

- **V6, probado:** [Actions #43878136136](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43878136136) completó SUCCESS. Materializó 5 GLB, un Blender editable y 3 PNG reales. La malla de terreno tiene 11.865 vértices y está exportada con atributo `COLOR_0`; en estadio V la exportación contenía 1.589 mallas y 10 imágenes incrustadas. **Nada de ello demuestra FPS ni calidad premium**.
- **Resultado de inspección visual V6:** ya no hay losas blancas a modo de escalerilla; sin embargo las coníferas se leen muy claras y geométricas, los elementos altos del Bastión parecen añadidos y faltan riqueza de piedra y contraste. El JSON V6 reveló `images_repainted: 0`: la ruta anterior no había modificado ninguna textura física.
- **V7 implementado en `tools/cc0-art-system/director_v7_material_architecture.py`:** corrige esos defectos reemplazando pirámides de tejados por dos aguas conectadas, árboles de ramaje irregular y **texturas originales modificadas realmente**. Su fuente es exclusivamente el GLB V6, de autoría propia.
- **V7 primer run `43880135476` FALLÓ de forma informativa:** Blender reconocía texturas empaquetadas del GLB sin ruta de archivo ni datos de píxeles accesibles, por lo que 0 imágenes cambiaron. No se rebajó el control de aceptación; se reimplementó la técnica leyendo el **BIN bufferView del GLB 2.0**, procesando el PNG verdadero y sustituyendo los nodos de textura por PNG nuevos propios con SHA antes/después. Nuevo run exacto: [#43880753054](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43880753054); comprobar su conclusión y logs.
- **Pendientes genuinos:** inspeccionar visualmente V7 A/B, repetir optimización estática sobre los modelos V7 (no asumir resultados de V6), ejecutar integración Unity aislada solo cuando Windows esté correctamente reservado y probar el dispositivo real. **No fusionar ni publicar**.

**Evidencia / principio de honestidad:** Blender render success ≠ unidad artística aceptable; GLB texturas rebake success ≠ iPhone performance. Mantener referencias y defectos al cambiar de conversación.

## Director V8 — reconstrucción material y arquitectónica (16/11/2026)

**Punto de partida verificable:** [Director V7 run 43880753054](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43880753054) = 5 GLB de autoría Eldoria + fuente BLEND + 3 capturas reales Blender; sin prueba Unity o móvil. Revisión visual independiente: aspecto aún demasiado plano, árboles geométricos y pálidos, textura de tierra débil y torre superior aislada.

**Nuevo trabajo real en la misma rama:**
- `tools/cc0-art-system/director_v8_atlas_and_citadel.py` importa V7 real del run exacto; reemplaza suelo por altura y textura de 1024×1024 con caminos coloreados en terreno; introduce tres atlas físicos 1024×1024 (piedra, pizarra y madera), retoma materiales compartidos, sustituye coníferas por copas orgánicas con geometría propia, construye nave gótica conectada y tejados de capilla, mejora cuartel/granero/cantera/aserradero y mantiene cinco exportaciones físicas.
- `.github/workflows/cc0-director-v8.yml` ejecuta Blender 4 sobre Ubuntu alojado sin Windows compartido, con input inmóvil [V7 #43880753054](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43880753054). Espera tres capturas auténticas 1600×900 (I, V) y 720×1200 (V), cinco GLB, BLEND editable y evidencia técnica en `eldoria-cc0-director-v8-real-3d`.
- **Run inicial V8 [#43882248142](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43882248142):** lanzado; su éxito, calidad de capturas y rendimiento NO deben darse por supuestos mientras no terminen la ejecución y la revisión.

**Límites:** no modifica `main`; no incorpora fuentes privadas de Quaternius; no es conversión de imagen conceptual a 3D; no certifica rendimiento ni calidad artística Unity/iPhone. Siguiente obligación: inspeccionar fotos reales V8, medir fallos geométricos y corregir antes de declarar mejora sustancial.

## Director V8 y V9 — evidencia y corrección causal (16/11/2026)

- **V8 completada y verificada en Blender:** [run #43882248142](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43882248142), artifact `eldoria-cc0-director-v8-real-3d`. Cinco GLB físicos, fuente `.blend`, 3 PNG auténticas; V8 Bastión V exportado con 1.885 meshes, 77 materiales, 11 imágenes. **No Unity, no móvil, no aprobación artística.**
- **V8 juicio visual:** mejora real de caminos/tejados/materiales; quedan coníferas paleadas, entorno oscuro y nave principal poco orgánica. No confundir el render conceptual aprobado por IA con una captura jugable.
- **V9 alternativa bosque mixto:** `tools/cc0-art-system/director_v9_natural_forest_and_integrated_keep.py`, rama idéntica. Primera prueba `43884700954`: Blender generó cinco GLB y PNG; QA de workflow falló por comparar erróneamente su `source_run` con el ID V7 en lugar del V8. Corregido en `.github/workflows/cc0-director-v9.yml`. **Pero la captura real reveló el defecto más grave: nave principal casi negra por geometría V8+V9 duplicada y z-fighting.** Nueva corrección `ba5259f683e20eeebb2ef0a28d3c9997830293e8` elimina mallas de V8 antes de reconstruir V9, rebaja la cota del tejado, añade guard de una única nave para nivel V y cero para I–IV. Run de comprobación [#43886355573](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43886355573) debe completarse e inspeccionarse.
- **V9 alternativa estructural:** código separado `tools/cc0-art-system/director_v9_architectural_lift.py`, salida `DirectorV9Structural`. Su [run #43884858236](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43884858236) pasó Blender, glTF y PNG, pero el arte no alcanzó el estándar: cubiertas y gabletes demasiado blancos/planos, fuerte modularidad. Una revisión independiente de esa línea no implica aprobación para Eldoria.
- **Decisión pendiente:** comparar capturas reales **corregidas** V9 con alternativa estructural y V8. Si no corrigen macrodefectos, pivotar nuevamente la técnica; no sumar decoraciones sobre geometría incompatible.
- **Riesgo de continuidad:** GitHub Actions conserva los binarios solo 30 días; los scripts y reglas permanecen en la rama, pero una cadena de builds anclada a artifacts viejos puede caducar. Debe documentarse/automatizarse un rebuild completo por scripts originales antes de considerar la fuente reproducible a largo plazo. No publicar modelos de terceros sin prueba de licencias.

**Estado honesto:** investigado sí; implementado y renderizado V8 sí; V9 nuevas pruebas y correcciones en curso; aprobado artísticamente NO; Unity NO; iPhone/WebGL NO; `main` intacta.
