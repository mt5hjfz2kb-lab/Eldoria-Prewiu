# Eldoria — conclusión del ensayo visual de 15/11/2026

Estado: **prueba técnica realizada; objetivo artístico NO alcanzado**. PR #196 debe permanecer en borrador, sin merge a main ni a la beta.

## Evidencia real de Unity

- [Run 43723401749](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43723401749): escena Unity Editor canónica Valoria_City_World_UI, 1600x900 y 390x844, 107 renderers / 118 materiales. El shader alternativo retiró visualmente edificios y vegetación, aunque proyectaban sombras. Hubo diferencia de píxeles significativa pero **calidad fallida**. No confundir screenshot distinto con resultado mejor.
- [Run 43723908844](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43723908844): captura real Unity de A original, B atlas de color pintado usando shaders de origen y C tintado con materiales originales. 35 texturas horneadas en Editor; retención de detalle en el Bastión ~0,923; diferencias medias A→B de 5,22/255 (horizontal) y 5,04/255 (vertical); A→C de 8,23 y 10,51/255. Los modelos vuelven a renderizar, pero **la mejora artística es demasiado pequeña**. No aprobada.
- Arte de referencia generado por IA: **concepto externo**, nunca evidencia de modelo/iluminación/rendimiento integrados.

### Evaluación artística

El techo azul intenso, el césped extenso y poco integrado y la escasa jerarquía entre Bastión, caminos, orillas y ruinas **no se resuelven de manera suficiente con una corrección de color**. Un shader global es además más frágil frente a los materiales PBR, transparentes, glTF y luces de la escena.

El ensayo confirma la vía técnica útil: editar una copia de escena usando materiales URP originales y capturar la cámara real sin entrar en el runtime completo. No confirma mejora comercial ni rendimiento en iPhone.

## Decisión de producción tras rechazar estas variantes

Cambiar de técnica: **no repetir pequeñas variaciones de tinte, filtros ni shaders globales**. El siguiente piloto debe modificar directamente la composición física usando assets aprobados/reutilizables:

1. Elaborar kit modular consistente de rocas, muros antiguos, vegetación, terrazas y caminos conectado al terreno; inspeccionar antes los prefabs existentes y sus licencias, no crear GLB nuevos sin justificar.
2. Convertir el Bastión y el gran arco imperial en jerarquía evidente, con transición entre recinto habitado y ruina exterior; añadir detalle selectivo, no densidad indiscriminada ni elementos inmóviles para maquillar.
3. Usar una cámara 3/4 y dos escalas reales; registrar A/B de la misma escena en Editor y luego PlayMode/iPhone cuando la estabilidad P0 esté resuelta.
4. Certificar por separado calidad (meta ≥7,5/10 respecto a la referencia aprobada), legibilidad, jugabilidad y memoria. Sin arte aprobado en estos runs.

## Lecciones técnicas documentadas

- El primer ensayo procedimental 2.5D se veía como una maqueta infantil. Se descartó, sin tratarlo como asset de producción.
- Una pasada 2D sobre screenshots no soluciona el escenario; se descartó.
- En el capturador C#, cualificar `UnityEngine.Application` evita colisión con `Eldoria.Application`.
- `-quit` con PlayMode diferido hace salir Unity antes de obtener capturas. La ruta Editor síncrona evita esa dependencia y la carga del mundo P0.
- Las capturas y el manifiesto de artefactos auténticos están conservados en los runs anteriores. Un `CI success` y diferencias de píxeles **no equivalen a ART_APPROVED**.
- **No modificar** P0/PR194, beta/testers, saves ni escena productiva por esta prueba. Ninguna build WebGL ni APK se produjo.


## Ensayos adicionales de composición y acabado — 15/11/2026

**No se autoriza la promoción de ninguna variante.** La primera prueba A/B se completó en Unity Editor (no móvil, sin ejecución real de juego), y las candidatas posteriores también se inspeccionaron visualmente.

| Evidencia | Resultado técnico comprobado | Veredicto artístico |
| --- | --- | --- |
| [43725103057](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43725103057) | Editor capturó propuestas con 105 props existentes, aunque el gate final rechazó diferencia demasiado baja | Rechazada: piedras claras con escala errónea, vegetación diminuta o con siluetas incorrectas, composición no mejora |
| [43725656960](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43725656960) | Native Unity SUCCESS, 46 renderers de decoración reutilizada y 4 PNG A/D | Rechazada: rocas blancas desproporcionadas y grupos de árboles con troncos claros incoherentes; peor que el original |
| [43725782481](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43725782481) | Native Unity SUCCESS, capturas originales y dos tratamientos G1/G2 sobre cámara original, en ambas orientaciones | Rechazada: el filtro apenas afecta al problema de arquitectura, relieve y vacío del terreno |
| [43726354590](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43726354590) | Unity compila y escribe A y F; el workflow termina FAIL porque comprueba nombres antiguos G1. El manifiesto declara 7 capas de terreno y 105 prefabs (solo validación de render Editor) | Rechazada: terreno más gris, pero piedras y límites artificiales; no apariencia comercial |

**Conclusión causal:** la brecha entre el concepto deseado y la escena real no se cierra con postprocesos, tintes, color por texturas, rocas automáticas ni clonación masiva. El problema principal es la escasez y la poca coherencia de geometría/arte de producción, además de la composición del entorno. Aumentar polígonos sin dirección artística no sería una solución; tampoco añadir sprites como una imagen de fondo que oculte la falta de interactividad.

**Siguiente enfoque coherente y de coste 0 por defecto:** crear primero una única **zona de referencia** realmente trabajada en Blender/Unity con un kit propio y reutilizable (bastión autorizado, ruina monumental, camino integrado, dos familias de rocas, dos especies de árboles y suelo orgánico). Validar una toma auténtica de Unity a 1600×900 y 390×844 a la **misma cámara**; el resultado debe ser claramente mejor de un vistazo antes de hacer más arte o ampliar el mapa. No hay autorización para meter modelos de terceros sin licencia acreditada ni usar el Bastión vetado. Las escenas, sprites e imágenes conceptuales no son prueba de integración, jugabilidad, rendimiento móvil o aprobación artística.

**Límites al cierre:** PR en borrador; ninguna escena principal ni beta modificada; no build jugable nueva; artApproved=false, physicalMobileTested=false. Reservar Windows de nuevo solo cuando exista una fuente artística nueva que merezca prueba; no acumular más runs de mínimos retoques.
