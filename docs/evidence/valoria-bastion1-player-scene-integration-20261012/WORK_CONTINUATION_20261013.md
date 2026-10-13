# Bastión I — continuidad de Work (trabajo ABIERTO)

Objetivo vigente del propietario: continuar la integración detenida, conservar trabajo existente, llegar a una escena inicial cercana a la referencia (acepta 8/10 provisional), verificar colores y probar controles reales. Presupuesto 0 € / 0 créditos de pago. No declarar terminado ni ART PASS por compilar.

## Fuentes y titularidad
- Rama única continuada: `integration/valoria-bastion1-player-scene-20261012`.
- Work recuperó el encargo existente en main `46c3afa06596c26d6c6047eda787b403c1d3b6b6`; no se abrió otro workstream ni otra rama.
- Propietario operativo: `work-owner-authorized-bastion1-continuation-20261013`.
- No modificar WorldRegion1Runtime, ValoriaCurrentWebGLBuild, solicitud de publicación ni Pages ajenos. SHARP publicado permanece protegido hasta gates de promoción.

## Resultados ejecutados, NO repetir
1. Capturas Unity BEFORE genuinas: run **38394884778**, fuente **08393eb1de53de4e22c23b6116c05cfc4f73bb5d**, artifact **11812445122**. El run falló DESPUÉS de la captura por nombres de materiales en `bpy.data.libraries.load`; corregido usando nombres inmutables. Esta captura se recupera, no se vuelve a renderizar.
2. Se consumió el ensayo CC0 ya realizado en **38388922583 / 11808120370**, sin regenerar texturas, ciudad ni inferencia.
3. Transferencia real a ciudad completa + captura Unity AFTER + compilación WebGL: **38395469618 SUCCESS**, fuente **c53108d7160160d55743b460526a03a2443031bc**, artifact **11811847205**. Geometría idéntica; GLB resultante SHA256 `06f122b45754185d65d1942c8fb89e761bc2c5f5a5349d6d2e77a3ceb9597483`.
4. A/B independiente 1440x1000: **51.2824 %** de píxeles con diferencia RGB >5; MAE 9.7626. Es evidencia visible de materiales, NO aprobación artística. Captura real inspeccionada: suelo demasiado marrón, cliff con musgo uniforme, carretera aparente interrumpida y paisaje insuficiente. **VISUAL REJECTED**, no 8/10.
5. QA básica del artefacto EXACTO **38396278910**, artifact **11812915521**: startup, gestos de pan, selección, construcción/coste, cuartel bloqueado en I, Mundo/retorno, reload y reset PASS; 0 errores. El workflow global está rojo por el ensayo I–II posterior, NO por esa QA ya superada. La siguiente QA reutiliza su JSON con SHA/run verificados.
6. Fallos del comprobador I–II: primero esperaba 750 ms en vez de observar el panel real; luego omitía user agent móvil y generaba canvas desktop recortado. Se corrigieron espera explícita, user agent de iPhone y gate de lienzo visible. No atribuir esos dos fallos a la economía del juego.
7. `KHR_texture_transform` ya se transfiere explícitamente a albedo/normal/metallic-gloss URP; rotaciones no admitidas se rechazan en vez de perderse.
8. Guard arquitectura y art production PASS; workflow governance detectó un trigger pesado por editar su propio YAML, corregido en **3586460d7c26859908bc2718d1680ce7bf76faa7**, guard PASS. No se eludieron controles.

## Ejecuciones actuales: recuperar antes de actuar
- **38398380999** / fuente **5428741afc02e8be8bf081910784363a6ce78376**: reemplazo de tierra marrón por fotografía CC0 existente `aerial_grass_rock`, escala UV 1.35; conversión de factores glTF lineales para renderer Gamma del proyecto; metadatos reales de cámara y diagnóstico de contacto de carreteras Unity. Recuperar capturas/logs/resultado antes de repetir o promover.
- **38398178848** / fuente **16100769d0fa8b194664c385608e81c6736d4eaa**: ensayo I–II con identidad móvil corregida, horizontal/vertical, timers/rewards reales, sin inyección de estado. Consume el WebGL anterior exacto 38395469618. No atribuir su PASS futuro al candidato de césped aún no probado.
- **38398178834** / misma fuente: nueva inferencia genuina Qwen3-VL **8B** desde la captura Unity PBR, usando 2 rechazos visuales históricos y mesh context. El intento previo **38397449376** se detuvo ANTES de inferencia por comparar la procedencia de un rechazo histórico con el run nuevo; comparación corregida a su fuente original `38356683511`. Recuperar plan, inferencia, resultado Blender y A/B; no inventar aprobación o coordenadas del modelo.

## Próximos pasos concretos
1. Inspeccionar el candidato de césped, colores y contactos; mantener cámara A/B fija. Confirmar continuidad camino/puente/escalera con logs y captura, no solo bounds de GLB.
2. Recuperar QA I–II; reparar fallos reales desde registros/capturas. Diferenciar emulación Chromium, Unity y móvil físico.
3. Si Qwen8B produce geometría válida y visible, consumir sus coordenadas originales con el merge existente y los hashes correctos, preservando parcelas/UV/PBR; hacer Unity A/B sobre el candidato válido más reciente, no sobre una escena antigua.
4. No sustituir autoría genuina Qwen por scripts silenciosos. Los cambios de paleta dirigidos por GPT-6 se registran como lookdev aislado sin Qwen ART_APPROVED.
5. Corregir arte/composición/encuadre que impidan el objetivo. No publicar ni fusionar por QA técnica; gates visuales siguen abiertos.

## Limitaciones concretas del entorno de Work observadas
El CDN de Playwright devolvió descargas truncadas. Se recuperó Chromium oficial del workflow gratuito ya autorizado, pero el contenedor de Work rechaza sus sockets POSIX (Operation not permitted). Las interacciones reales con WebGL se ejecutan por el workflow Linux disponible con touch Chromium y capturas originales. No afirmar prueba física ni manual independiente, ni paralizar Unity/Blender por esta restricción local.
