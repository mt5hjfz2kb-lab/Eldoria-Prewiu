# Estado vivo — Eldoria CC0 Art System

**Fecha de actualización:** 16/11/2026, revisión Director V4.  
**Identificador único:** `eldoria-cc0-art-system-20261116`.  
**Rama:** `experiment/eldoria-cc0-art-system-20261116` — SIN MERGE A `main`.

## Último trabajo real, persistente entre chats

1. **Fuente inicial ya generada y probada:** Director V3 tiene 5 GLB auténticos y `.blend` reproducibles. [Run 43866662062](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43866662062), artifact `eldoria-director-v2-real-art`. La fuente NO contiene modelos de otros proveedores ni imágenes conceptuales en lugar de mallas.
2. **Revisión V4 implementada:** [`tools/cc0-art-system/director_v4_surface_and_architecture.py`](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/experiment/eldoria-cc0-art-system-20261116/tools/cc0-art-system/director_v4_surface_and_architecture.py) aplica UV, texturas físicas propias, tratamiento de terreno, portal/tejado imperial y pequeños elementos de identidad para cantera, aserradero, cuartel y granero. Cinco GLB exportables y capturas genuinas, nunca arte conceptual.
3. **Primera V4: técnicamente producida, NO aprobada.** [Run 43870376065](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43870376065): Blender exportó las 5 mallas (1809–2067), 11 imágenes incrustadas y tres capturas; el gate final falló porque la diferencia visual global I/V era 3.3 (foco central 7.8), y los renders se observaron demasiado oscuros. **No ocultar este fallo.**
4. **Corrección V4 vigente:** nueva exposición, color de piedra/cubierta, luz de ambiente, cámara cercana de Bastión V. Workflows pendientes de confirmar: [run 43871986597](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43871986597) y [run 43872065620](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43872065620). NO declarar PASS antes de ver sus conclusiones y las capturas.
5. **Optimización posterior preparada:** `tools/cc0-art-system/batch_director_v4.py` agrupa mallas por material/distrito y verifica recuento de polígonos, UV y AABB. Todavía no certificada sobre la versión V4 corregida.
6. **Estado Unity:** ningún build real, escena nativa ni prueba iPhone certificados en esta rama. Windows compartido está en otro workstream según `main/pipeline/active-workstreams.json`; no reclamar o ejecutar sin cesión legítima.
7. **Licencia:** el pack original Quaternius fue investigado e importado solo para una comparativa de Blender; la versión V4 usa geometría propia. Las condiciones QAL vs CC0 de distintas fechas siguen registradas en `asset-lock.json`. No promover recursos externos a repo público sin licencia individual.

## Decisión visual

La imagen de IA del chat anterior es una **referencia deseada**, no evidencia de modelos ni capturas de Unity. El V3 original no alcanzaba riqueza de materiales, terreno ni geometría. El V4 debe revisarse por sus PNG reales; si los cambios no alcanzan una escena clara, adulta y atractiva, diagnosticar técnica/proporciones antes de otras microiteraciones. No presentar un screenshot oscuro o una exportación verde como calidad comercial.

## Cómo continuar

Leer `main/AGENTS.md`, `main/pipeline/active-workstreams.json`, `docs/cc0-art-system/README.md`, `asset-lock.json`, `V4_CONTINUITY.md`, este STATUS y los runs arriba. Mantener propietarios de otras ramas, sin tocar `main`, sin coste ni publicaciones. Descargar artifact V4 de ejecución aprobada; inspeccionar las imágenes auténticas; registrar defectos y mejorar. Solo entonces generar versión optimizada y preparar Unity real cuando pueda reservarse Windows. Registrar al final HEAD, run, capturas y valoración independiente.

---

## Registro histórico anterior a Director V4

# Estado comprobado y relevo — Eldoria sistema gráfico gratuito

**Fecha de auditoría:** 16-11-2026.  
**Workstream:** `eldoria-cc0-art-system-20261116`  
**Rama única:** `experiment/eldoria-cc0-art-system-20261116`. `main` NO modificada. No utilizar Windows compartido sin registro y coordinación.

## Estado exacto por hito

| Hito | Estado | Evidencia reproducible |
| --- | --- | --- |
| Contrato, fuentes y licencias | REGISTRADOS | `README.md`, `asset-lock.json`, `ART_REVIEW_AND_PIVOT_20261116.md` |
| Bastión I–V fuente única | PROBADO en Blender | [run #43847137592](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43847137592): `CC0_Valoria_Stage1..5.glb` y `CC0_Valoria_Source.blend`, material propio |
| Descarga original de seis modelos Quaternius | PROBADO en Blender | [run #43844725460](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43844725460): originales comprobados con SHA, imagen real |
| Composición mixta con ruinas, cuartel, granero, mina, árboles | TÉCNICAMENTE PROBADA, ARTÍSTICAMENTE RECHAZADA | [run #43847893229](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43847893229); tres PNG guardadas en `previews/hybrid`, sin originales externos en Git |
| Nueva composición uniforme con cuatro industrias separadas | CANDIDATA EN QA | [workflow `cc0-art-system-city-finish.yml`](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/workflows/cc0-art-system-city-finish.yml); script `tools/cc0-art-system/recompose_city.py` genera 5 GLB, 1 BLEND, cuatro PNG |
| Importación en Unity, capturas reales Unity | NO PROBADA | `Unity/Assets/Art_Reboot/CC0System/Editor/CC0SystemSceneBuilder.cs` es código candidato; falta ejecución y contraste |
| Gameplay, beta, rendimiento iPhone/Android | NO PROBADO | Ningún render Blender certifica lógica, FPS, memoria o estabilidad WebGL |
| Licencia de redistribución de vendor source | NO ACREDITADA | Licencia específica sin verificar; sitio dice CC0 y `License.txt` original mal titulado. Fuentes externas conservadas solo en runner temporal. |

## Resultado crítico (NO repetir error)

La superposición directa de Quaternius Ultimate Fantasy RTS sobre la fortaleza original produjo modelos demasiado pálidos/simplificados, diferentes escalas y baja identidad de ciudad; desaturar materiales no solucionó el choque de estilos. **No promover esa composición** a Unity por el mero PASS de CI. `TownCenter` del pack es una fuente ornamental, no un Bastión. El registro completo está en `ART_REVIEW_AND_PIVOT_20261116.md`.

## Pivote artístico en ejecución

Crear arquitectura secundaria nueva sobre los mismos materiales/proporciones de la fortaleza, sin casas-plataforma. Aserradero con taller y pila de madera; Granero con silos/sacos; Cuartel con torre/estandarte; Cantera con extracción/grúa. Todos separados y conectados con senderos orgánicos en terreno continuo, manteniendo la diferencia real I–V. Quaternius permanece como posible recurso DONANTE para piezas compatibles tras rematerialización y licencias, NO como selección ciega.

## Riesgo de rendimiento

`CC0_Valoria_Stage5.glb` base tiene **1165 mallas** (recuento inspeccionado del GLB físico), demasiado alto para aceptarlo como viable en iPhone sin análisis. La fase siguiente requiere **batching estático por material/distrito, LOD/ocultación**, comprobación real de mallas/draw calls en Unity y pruebas físicas. Un GLB de tamaño pequeño no prueba rendimiento.

## Orden de continuación

1. Leer `main/AGENTS.md` y registro `pipeline/active-workstreams.json` vigente; no pisar propietarios de `art/visual-reboot`, Nocturne ni beta.
2. Leer esta rama: `README.md`, `asset-lock.json`, `ART_REVIEW_AND_PIVOT_20261116.md` y este STATUS.
3. Comprobar exactamente el último run de `cc0-art-system-city-finish.yml`, actualmente lanzado como `43848940733`. No afirmar PASS si sigue ejecutándose o falla. Examinar sus PNGs reales y el contador de mallas. Reanudar allí con defectos medibles.
4. Solo si la composición alcanza un resultado estético prometedor, promover en ESTA rama sus modelos originales a `Unity/Assets/Art_Reboot/CC0System/CityFinish`, preparar importación nativa cuando el runner/ownership lo permita y capturar vistas Unity.
5. Nunca fusionar, publicar o marcar `ART_APPROVED` sin evidencia real y aceptación del propietario.

**Investigado:** catálogo y licencias. **Implementado:** generadores, estudio híbrido, builder Unity no probado, composición nueva. **Probado:** fuente original 5 GLB + estudio Quaternius en Blender. **Por verificar:** nuevo acabado, batching, Unity, gameplay y móvil.
