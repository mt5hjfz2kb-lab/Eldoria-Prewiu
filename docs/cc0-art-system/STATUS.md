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
