# Zona 1 — producción de fuentes 3D, entrega Unity bloqueada

**NO COMPLETADO. NO ART_APPROVED. NO 8/10. NO ESCENA UNITY CAPTURADA.**

Rama: `prototype/world4x-zone1-zoom-reference-20261013`. Base de esta continuación: `34ce0ec78b2bc553f11f16562a5f18752620fb34`. Main vivo comprobado: `4b434c226e51f5036082e62529196aaf869df9ba`.

## Trabajo efectivamente ejecutado

- Recuperación completa del repositorio en un checkout independiente; no se editó el checkout de otro trabajo.
- Blender **4.5.3 LTS** instalado y ejecutado localmente. Ollama **0.40.3** y **qwen3-vl:2b-instruct** local en CPU; no servicio cloud, compra ni crédito de generación. El modelo menor es una alternativa local, no una afirmación de equivalencia con el artista 8B del runner.
- Sector fuente continuo de **240 × 240 unidades**, 14.641 vértices / 14.400 quads, ciudad estratégica existente, Holotna y Quaternius recuperados. Es un sector fuente de ensayo, no una región persistente completa.
- Ciudad `PlayerCity_v1.glb`: preflight estructural sin errores ni avisos; 22.797 triángulos y tres texturas incorporadas. Su geometría no se reemplazó por el kit low-poly.
- Uso del **system prompt y validador existentes**, inspección de topología real y ejecución del plan original con `tools/art-direction/ai-authoring/execute_artist_plan.py`. Se usó transporte Blender **direct**; no se afirma que se ejecutara BlenderMCP.
- Dos primeras respuestas inválidas quedaron registradas. Un primer plan ejecutable se aplicó realmente y fue **rechazado**: desplazamiento lateral, sin relieve vertical, y modificación del borde. Se volvió a la fuente intacta.
- Una nueva inferencia recibió la revisión del fallo y decidió una depresión vertical. Se ejecutó sin modificar sus parámetros: **564 vértices cambiados y 480 vértices de borde idénticos**. A/B Blender con la misma cámara y configuración: 21.553 píxeles cambiados por encima de 5/255, **2,694125 %**. Esto acredita visibilidad, no belleza.
- Adaptación de máscaras RGB de Holotna a follaje verde y alpha; retirada de LODs superpuestos; atlas continuo texturado; caminos claros suavizados; ruina y stock/props recuperados. Dos acabados fueron rechazados por bandas artificiales y pivotes que dejaban props fuera del encuadre; sus capturas se conservaron.
- Exportación de candidato GLB con texturas incorporadas, fuente `.blend` editable, tres vistas reales **de Blender** del mismo mundo y pruebas estructurales.
- Código Unity aislado para importación con glTFast, persistencia de meshes/materiales/texturas, creación de escena nativa y captura; cámara única con pan, pinch y tres zooms. **No ejecutado ni compilado en Unity durante este trabajo.** La posición de ciudad se calcula a partir de los bounds importados, no de un supuesto cambio de ejes.

## Evaluación visual independiente

El candidato sigue siendo un **prototipo de composición**, lejos de la referencia. Evaluación orientativa de las imágenes de Blender: **3/10**, sin extrapolar esa cifra a Unity.

La ciudad y el follaje corregido son aprovechables como fuentes. El terreno es continuo y abierto, pero aún demasiado uniforme; las montañas presentan masas repetidas poco integradas; la ruina es escasa y sobredimensionada. Los props recuperados de madera/alimento no tienen suficiente identidad de nodo estratégico. Los paños y cortavientos no constituyen un campamento enemigo convincente y se consideran **sustituciones provisionales rechazadas**. La Brecha no existe con la presencia física solicitada: una depresión de 1m no cumple el objetivo y se retiró su franja violeta artificial. No se acredita un sector terminado ni se multiplicaron sectores.

No se declara superada ninguna fase de arte. El candidato se conserva **para diagnóstico/importación aislada**, no como última versión artística aprobada ni como sustituto del juego.

## Bloqueo concreto para completar la entrega

La última consulta al registro vivo de main mantiene `windows-runner-heavy` bajo `valoria-bastion1-player-scene-integration-20261012`, propietario `chat-normal-bastion1-fullframe-convergence-20261013`. No había runs `in_progress`; esa ausencia **no libera la reserva**. Se observaron dos antiguos probes en cola, sin cancelarlos.

Este entorno dispone de Blender/Ollama recuperados, pero no de Unity instalado/activado. Los workflows existentes de Unity ejecutan el editor **Windows self-hosted 6000.3.23f1**; el job Ubuntu es preflight de fuentes. No se encontró una ruta Linux Unity ya configurada ni referencias a GameCI/UNITY_LICENSE/UNITY_SERIAL en los workflows examinados. La instalación gratuita del editor por sí sola no proporciona una activación disponible. No se accedió a secretos ni se trasladó una licencia.

Por tanto, no se puede ejecutar legalmente el gate Unity requerido dentro de los recursos autorizados ahora, sin reconciliar la reserva o disponer de otro editor activado. No se creó ni encoló un workflow Windows, no se tocó Bastión I, no se publicaron cambios en main/Pages y no se modificaron sus archivos de artista.

Este bloqueo explica la ausencia de escena/capturas/pruebas Unity; **no explica ni disculpa el ART FAIL de las fuentes**. Quedan pendientes una autoría de Brecha y campamento con presencia real, mayor integración geográfica/material, importación Unity y su ciclo de corrección, navegación comprobada, streaming/LOD y perfilado móvil. No hay trabajo ejecutándose en segundo plano tras esta entrega.

## Evidencias y continuidad

Todo el ciclo está en `docs/world4x-zone1/evidence/local-cycle-20261014/`: referencia exacta y hashes, inventario, topología, solicitudes reales, respuestas Qwen completas, planes, ejecución Blender, fuentes editables, A/B, casos rechazados y auditoría GLB. El candidato de importación está en `Unity/Assets/Eldoria/ArtTests/World4XZone1/Models/ZoneOneSectorCandidate.glb`.

`historical-advice.txt` se pasó efectivamente a Qwen, pero el catálogo contenía **cero casos de Zone One**. La revisión del ciclo fallido se suministró a la siguiente inferencia. Es memoria/recuperación de experiencias en el contexto, **no reentrenamiento ni cambio de pesos**. Los casos actuales no se incorporaron como aprendizaje aprobado al catálogo protegido de Bastión I y carecen de confirmación Unity.

Una vez disponible un editor autorizado, ejecutar sin `-nographics` ni `-quit` prematuro:

```
ELDORIA_ZONEONE_CAPTURE_OUT=<directorio nuevo de evidencias>
Unity -batchmode -projectPath Unity -executeMethod Eldoria.World4XPrototype.ZoneOneSectorScene.BuildAndCapture -logFile <log nuevo>
```

El método guarda la escena nativa `Assets/Eldoria/ArtTests/World4XZone1/Scenes/ZoneOneReferenceSector.unity` y capturas global/regional/local con matrices de cámara. Esa escena **todavía no existe como resultado verificado**. No escalar a WebGL/APK/publicación mientras el sector siga ART FAIL. Conservar las fuentes buenas existentes y reparar el defecto concreto, sin reconstruir el proyecto.
