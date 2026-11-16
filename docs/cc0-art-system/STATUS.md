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
