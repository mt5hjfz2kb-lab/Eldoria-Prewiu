# Eldoria — Región 1: revisión artística A/B certificada del candidato 40182996115

**Fecha:** 25/10/2026. **Repositorio:** mt5hjfz2kb-lab/Eldoria-Prewiu, PR #100 DRAFT. **Resultado:** RETAINED como candidato técnico con mejora regional parcial. **No ART_APPROVED, no 7,5/10, no merge, no publicación.**

## Observación — imágenes genuinas y equivalencia de cámara

Se inspeccionaron seis PNG auténticos de Unity PlayMode, producidos por las ejecuciones [40142228055](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40142228055) (línea base) y [40182996115](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40182996115) (candidato). Están conservados en `sources/native-<run>/review/<vista>-unity.png`. Para cada una de las seis parejas se verificó que el SHA-256 del PNG coincide con el `*-source.json`, y que los campos `zoom`, `aspect`, `cameraPosition`, `cameraRotation` y `worldTarget` son **exactamente iguales**.

| Cámara | Porcentaje de píxeles con diferencia RGB máxima mayor de 5/255 | Diferencia absoluta media RGB /255 |
|---|---:|---:|
| cercana horizontal | 0,084 % | 0,023 |
| cercana vertical | 0,000 % | 0,011 |
| regional horizontal | 13,587 % | 1,954 |
| regional vertical | 6,791 % | 0,990 |
| panorámica horizontal | 1,657 % | 0,225 |
| panorámica vertical | 1,720 % | 0,241 |

Estos son indicadores de cambio de píxeles; **no son una puntuación de calidad ni significan aprobación artística**.

**Vista cercana:** el nuevo tratamiento del suelo no ha producido una mejora perceptible en las seis cámaras oficiales cercanas (0,084 % en horizontal y cero por encima de 5/255 en vertical). La cantera de la evidencia adicional `review/continent-stone-landscape.png` ya incorpora el auténtico mecanismo de izado con materiales, pero se percibe como un conjunto pequeño en un gran claro; necesita silueta de excavación y escala de lectura más convincentes, no cinco modelos nuevos ni iconos falsos.

**Vista regional:** la Brecha y la transición cromática se han trabajado sobre la misma región; el cambio visual es amplio (13,587 % horizontal). Sigue apareciendo una transición de aspecto rectangular alrededor del núcleo AF/AG entre las masas de bosque y la pradera. No se debe borrar ni recolorear ninguna fuente AF/AG para disimularla.

**Vista panorámica:** el mismo continente, Brecha, caminos y masas forestales se conservan; el incremento visible es pequeño (1,657 % horizontal). La panorámica sigue sin mostrar nodos ni las dos familias de criaturas. No debe convertirse en un mapa separado ni añadirse decoración por rellenar.

## Decisión

**Retener la base jugable y los elementos artísticos incrementales**, porque son coherentes, no rompen las fuentes y la comparación real muestra diferencias regionales. **No acreditar el umbral de integración 7,5/10**. No añadir árboles innecesarios ni volver a Tripo. Las prioridades restantes son (1) eliminar la lectura rectangular del límite AF/AG sin editar el núcleo original, (2) elevar notablemente la silueta del verdadero nodo de cantera, y (3) afianzar la jerarquía de roca y desnivel de la Brecha frente a la referencia panorámica aprobada.

## Corrección ejecutada y certificación

El candidato conserva el lote agrupado `37f8b65d44f4bdddca3329cff036e24081d79520`: pigmento contextual de pradera, fuente geológica importada y cantera profesional con elevación/taller incorporados a las variantes auténticas. La ejecución nativa `40182996115` generó las fuentes editables, GLB y capturas, y certificó PlayMode 60/60, EditMode 26/26 y los 44 controles funcionales Bastiones I–II. La salida materializada es `174496d09d271afc45fc4c28606c918e91687fb9`. Las fuentes originales AF/AG y los Tripo originales del jabalí y engendro permanecen protegidos.

## Veredicto y riesgos

**RETAINED / ART_APPROVED=false.** El valor principal de este lote es conservar un candidato funcional y comprobado para siguientes intervenciones artísticas. No hay certificación de rendimiento en Android/iOS físico ni se ha completado el relevo del gate UI bajo su responsable. La comprobación independiente de proceso artístico tiene un defecto de ejecución previo: invoca `strategy.test.mjs` desde el directorio raíz del runner, aunque el script busca su `strategy.mjs` relativo a `policy/`; no debe confundirse con un fallo PlayMode ni solucionarse debilitando los criterios de aceptación.

Observación → decisión → corrección → veredicto: se aportan resultados del mismo render de Unity, una evaluación artística parcial, la trazabilidad del commit/materialización y los defectos específicos aún abiertos. Esta revisión no inventa nuevas imágenes ni altera el contenido aprobado.
