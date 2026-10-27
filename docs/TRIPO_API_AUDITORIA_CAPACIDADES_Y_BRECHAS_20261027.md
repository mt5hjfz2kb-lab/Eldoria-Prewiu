# ELDORIA — Tripo API: capacidades demostradas, incógnitas y plan de pruebas

**Fecha de auditoría:** 27-10-2026 · **Tipo:** consolidación documental de solo lectura + consulta de documentación oficial.  
**Ámbito:** Tripo API, nuestro proceso Blender/Unity, modelos de fauna, enemigos, props, futuras arquitecturas y preparación de Sir Aldric y Lyra.  
**Presupuesto nuevo autorizado:** 0 créditos / 0 €. **No se genera ni se regenera nada con Tripo. No se cambia ninguna escena, archivo de juego, modelo o solicitud activa.**  
**Prevalencia:** Este documento actualiza el conocimiento factual frente a informes históricos del 21-10, sin sustituir los contratos operativos existentes ni el dueño de cada workstream. Las cifras de la web de Tripo son orientativas y NO autorizan gastos.

## 1. Respuesta principal

**Sí: Eldoria YA ha demostrado la movilidad esquelética del Engendro mediante un procedimiento local.** No debemos seguir describiéndolo como imposible o no probado.

Cronología y autoridad de evidencia:

1. El Engendro se generó mediante **una sola tarea de Tripo H3.1**: 20 créditos, 80.268 triángulos originales y 49.800 en la primera versión Blender. Fuente original conservada bajo ArtSource/World4X/Spawnling_v1/. Informe: [resultado inicial](TRIPO_API_ENGENDRO_RESULTADO_20261021.md).
2. Después se produjo con Blender una versión de juego de **12.040 triángulos cerca / 2.900 lejos**, materiales conservados e integración en el único hotspot «engendro-valoria» en main (PR #84). Esta versión inicialmente **no** tenía animación de huesos. [Revisión e integración](evidence/spawnling-bastion2/ART_AND_INTEGRATION_REVIEW.md).
3. En el trabajo posterior se creó un **rig local** real: 12 huesos, 1 skin, pesos JOINTS_0/WEIGHTS_0 y clip de **zarpazo V4.2**. La inspección independiente comprobó deformación genuina y que los pies ya no sufrían la influencia incorrecta del brazo. [Revisión de origen V4](evidence/spawnling-animation-v4-review-20261021/RESULT.md) y [aprobación de Unity aislada V4.2](evidence/spawnling-animation-v4-review-20261021/unity-approved.json).
4. La ejecución nativa **39677487864** integró el zarpazo en la escena canónica: misma escala y posición de juego, misma identidad/materiales, regreso a reposo, sin alterar los 6 hotspots ni los colliders, y PlayMode de integración superado. [Acta native280](evidence/spawnling-animation-v4-review-20261021/NATIVE280_ACCEPTANCE.md).
5. El conjunto UI + terreno + Engendro animado superó posteriormente pruebas nativas y navegador en **39678224055 / 39683431955**: recorrido I–II, operaciones táctiles, tiempos, marchas, recompensas y persistencia. [Acta native281](evidence/spawnling-animation-v4-review-20261021/NATIVE281_COMBINED_BROWSER_ACCEPTANCE.md).

**Alcance preciso del éxito:** rig/skin y **un ataque de zarpazo**, no locomoción bípeda completa, conjunto de clips idle/walk/run/attack/hurt/death, retarget automático de Tripo, Android físico, facial ni un héroe humano. El movimiento de una marcha en el mundo y la animación corporal por huesos son pruebas distintas. La publicación/activación en una plataforma externa tampoco se infiere de una compilación.

**No afirmar «Tripo nos animó el Engendro»:** Tripo nos dio la geometría original; el esqueleto y la animación aceptada se hicieron en Blender mediante el procedimiento de Eldoria a **0 créditos adicionales de Tripo**.

## 2. Matriz de conocimiento: hecho, capacidad anunciada y pendiente

| Capacidad | Evidencia en Eldoria | Estado fiable | Próxima frontera |
| --- | --- | --- | --- |
| Texto → 3D H3.1 con textura | Jabalí, Engendro y Foco; tres llamadas de 20 créditos | **DEMOSTRADO** | Medir rendimiento/repetibilidad por nuevas familias, sin regenerar a ciegas |
| Imagen → 3D | Documentado en la API; ninguna prueba equivalente auditada aquí con retrato de Aldric | **CAPACIDAD OFICIAL / NO PROBADA EN NUESTRO FLUJO DE HÉROES** | Una prueba controlada con referencia aprobada, coste consentido |
| Multivista → 3D | Endpoint oficial; pipeline admite 2–4 vistas | **NO PROBADO COMO MEJORA DE IDENTIDAD DE PERSONAJE** | Comparación de consistencia de rostro, espalda, brazos y vestuario |
| Modelos P1 frente a H3.1 | P1-20260311 anunciado en generación; nuestros 3 recursos certificados son H3.1 | **NO COMPARADO** | A/B con mismo tipo de asset, no cambio de modelo por moda |
| Preservación de materiales/UV | 3/3 imports técnicos, BaseColor auténtico 2048² tras corregir módulos glTFast | **DEMOSTRADO AISLADAMENTE** | A/B con luz y shaders oficiales de Valoria |
| Corrección de silueta/anatomía en Blender | Engendro, jabalí y LODs; algunos ensayos Qwen rechazados por escaso impacto | **PARCIAL** | Validar si rostro/manos/equipo del héroe soportan edición localizada |
| Optimización de malla y LOD | Jabalí 49.800→12.450→3.984; Foco 49.799→12.448→3.983; Engendro 12.040/2.900 integrado | **DEMOSTRADO TÉCNICAMENTE** | Presupuesto real por personaje y perfil físico |
| Rigging local mediante Blender | 12 bones, un skin, swipe V4.2, aceptación Unity e integración native280 | **DEMOSTRADO PARA UN ENEMIGO** | Transferir técnica a anatomía humana y muchos clips |
| Rig Check Tripo | API indica servicio gratuito; intento anterior quedó sin API key en runner Linux | **NO HAY RESPUESTA DE RIG CHECK CERTIFICADA** | Preflight read-only/0 créditos usando credencial existente segura |
| Auto-rig Tripo | Endpoint oficial; no task paga de rig aprobada en la evidencia | **NO PROBADO EN ELDORIA** | Comparar contra rig local, solo tras estimación/permiso |
| Retarget/animaciones Tripo | Endpoint oficial; no prueba de humanoide de Eldoria | **NO PROBADO** | Retarget de una animación si el procedimiento local no basta |
| Animaciones de humanoide reutilizables | No existe prueba de Aldric/Lyra reales con ciclo completo | **PENDIENTE** | Idle + caminar + ataque + impacto + retorno, equipo sin clipping |
| Equipamiento modular y cambios de arma | Plan de juego contempla equipo, pero la fuente Tripo humana aún no existe | **PENDIENTE CRÍTICO** | Separación de espada/escudo/capa, sockets estables, malla oculta bajo equipo |
| Personaje visible en cámara 4X | Engendro sí se vio; Aldric/Lyra no cuentan con prototipo Tripo definitivo | **PENDIENTE DE HÉROES** | Primer plano, héroe, cámara de combate y mapa real |
| Rendimiento térmico/FPS Android | Pruebas de import/Unity/browser, no perfil físico fiable | **NO CERTIFICADO** | FPS, CPU/GPU, RAM, tamaño de texturas, battery/thermal |
| Producción en serie y tasas de acierto | 3 fuentes y múltiples refinamientos locales, no un estudio controlado de N generaciones | **DESCONOCIDO** | Medir minutos de supervisión, coste, rechazos y reprocesado |

### Antigüedad de documentos

- [TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md](TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md), [TRIPO_API_ENGENDRO_RESULTADO_20261021.md](TRIPO_API_ENGENDRO_RESULTADO_20261021.md), ArtSource/World4X/Spawnling_v1/PROVENANCE.md y [ELDORIA_WORLD4X_REUSE_EXISTING_CREATURES_20261023.md](ELDORIA_WORLD4X_REUSE_EXISTING_CREATURES_20261023.md) incluyen frases históricas sobre «sin rig / sin integración / sin animaciones».
- Eran correctas para sus respectivas **fases anteriores**, pero **no** describen el máximo estado alcanzado tras V4.2 y native280. Mantenerlas como historial; no borrar evidencias ni reinterpretar esos informes como versión actual.
- El catálogo [pipeline/tripo-art-brain/catalog.json](../pipeline/tripo-art-brain/catalog.json) conserva información original útil y algunos límites antiguos; para **estado de rig/animación** consultar primero las actas V4.2/native280.
- En el estado actual del registro de propietarios, la migración Bastión III y una revisión artística del Engendro tienen ámbitos activos propios. **No modificarlos ni usar su runner por una auditoría documental.**

## 3. Tres recursos certificados como fuente de Tripo

| Fuente | Tripo inicial | Geometría Tripo→Blender | Estado de producto |
| --- | --- | --- | --- |
| Jabalí de roca | H3.1 texto + textura, 20 créditos, 1 task | 84.178→49.800 y pruebas 12.450/3.984 | Fuente/LOD y QA aislada; integración jugable de caza y animación siguen siendo hitos separados |
| Engendro de la Fisura | H3.1 texto + textura, 20 créditos, 1 task | 80.268→49.800→12.040/2.900 | **Integrado** en encuentro BII, V4.2 con zarpazo esquelético Unity; sin ciclo completo ni móvil físico |
| Foco de corrupción | H3.1 texto + textura, 20 créditos, 1 task | 82.446→49.799→12.448/3.983 | Fuente de prop experimental; arte narrativo rechazable, **no** nodo nuevo ni integración automática |

Gasto original histórico de los tres: **60 créditos** en 3 tareas. Las operaciones de Blender/Unity, las pruebas de LOD y el rig local aceptado del Engendro no requieren volver a pagar la generación. Los tres fuentes GLB raw, optimizadas y .blend deben permanecer archivados.

## 4. Capacidades adicionales actuales del proveedor: qué hay en API y qué falta probar aquí

Fuentes oficiales consultadas el 27-10-2026:

- [Generación](https://platform.tripo3d.ai/docs/generation): H3.1 v3.1-20260211, P1-20260311, texto, imagen y multivista; declarar versión explícita y no depender del default.
- [Esquema OpenAPI](https://platform.tripo3d.ai/docs/schema): tareas de rig check, rig con tipo biped, retarget de clips, importación, conversión, generación, texturas, segmentación, completado y lowpoly. Especifica rig v2.5-20260210 y nombres de algunas animaciones biped, pero **documentar no significa que funcionen con nuestras armaduras**.
- [Precios del entorno API](https://platform.tripo3d.ai/docs/billing): H3/H3.1 texto 20 créditos con textura estándar, imagen o multivista 30; P1 texto 40 con textura, imagen/multivista 50; Rig Check sin coste, rig 25, retarget 10 por animación. Hay recargos por ajustes avanzados. **Precios públicos indicativos, no saldo actual ni autorización.**
- [Documentación API v3 actual](https://developers.tripo3d.ai/en/docs/introduction) y [ayuda oficial v3](https://www.tripo3d.ai/es/help/api-plugins/how-to-use-tripo-api): base https://openapi.tripo3d.ai/v3, mientras nuestro pipeline existente usa contrato API v2. **La migración v2→v3 no se ha probado en este proyecto; nunca cambiar el endpoint automáticamente.** Se precisa test de compatibilidad sin gasto antes de considerarla.
- [Preguntas frecuentes API](https://platform.tripo3d.ai/docs/faq): los créditos de la aplicación Studio y los créditos API son **sistemas independientes**; no confundir cuenta, saldo o precios.
- [Licencias comerciales de Tripo Studio](https://www.tripo3d.ai/es/help/privacy-policy/how-to-use-tripo-models-commercially): detalla restricciones según plan. **NO extrapolar ese artículo a los términos contractuales de una tarea API prepago**; confirmar por escrito condiciones de explotación comercial, privacidad y autoría para el titular real antes de publicar héroes generados con referencias.

**Coste de ejemplo, sin autorizarlo:** una generación H3 por imagen con textura estándar (30 créditos) + rig pago (25) + 3 retargets (3×10) = **85 créditos**, sin eventuales imágenes multivista, recargos, conversiones ni repeticiones. Si el rig y clips locales ya funcionan, el coste de rig/retarget Tripo podría ser **0**. La cuenta actual no se consultó: no afirmar saldo ni precio efectivo de una tarea.

## 5. Experimentos aún no hechos que aportarían valor real

### P0 — Sin gastar créditos, antes de Aldric

1. **Inventario de personajes/héroes ya existentes:** ejecutar tools/asset-vault/find-reuse.py y contrastar master/current/historical catalog (regla AGENTS.md). Examinar si alguna fuente ya contiene esqueleto, clips, armadura o accesorios utilizables sin perder la identidad aprobada.
2. **Referencia Aldric aprobada:** el propietario valora positivamente dos conceptos históricos (rostro veterano de pelo y barba canos; acero oscuro, capa azul, desgaste noble). No hay ficha de cuerpo completo A/T pose con frente, perfil y espalda exactas y aprobadas, ni garantía de coherencia de esos ángulos.
3. **Prueba local de esqueleto bípeda:** con un maniquí gratuito/licenciado o un mesh humanoide reutilizable, demostrar en Blender→Unity los clips idle/walk/attack/hurt y retorno a reposo. No confundir éxito de zarpazo de Engendro con bipedalidad completa.
4. **Prueba técnica de equipamiento/capa:** sockets espada, escudo y arco, correas, cabello, capa, colisiones y clipping; animación de brazos sin atravesar torso/armadura; selección y legibilidad a distancia oficial.
5. **Rig Check gratuito de Tripo**, SOLO si una fuente apta y credencial segura ya están disponibles en el runner autorizado. La tentativa previa en runner Linux falló por ausencia de secreto; no copiar claves a Git ni inventar un PASS.
6. **Prototipo de captura/QA:** comparativa real de primer plano y cámara 4X, H/V y guardados preservados; plan medible de tris/materiales/LOD, sin imponer a un humano el antiguo target universal de 49.800.

### P1 — Un único experimento de pago *si el propietario lo autoriza expresamente*

- Para Aldric, preferir **image-to-model** con concepto aprobado y cuerpo completo; H3.1 sería una primera opción de continuidad y riesgo/coste conocido. No usar su retrato recortado, sentado o con manos extendidas como entrada «ideal» de rig.
- Evaluar a priori **un solo candidato** con coste visible y techo firmado; preservar task-id + GLB raw inmediatamente; comparar fidelidad facial/armadura, manos, simetría y legibilidad. No repetir el gasto porque una malla sea «regular»: primero diagnóstico en Blender.
- Completar prueba de rig y clips locales; comparar autorig de Tripo únicamente si ofrece una ventaja real medible en tiempo/calidad o resuelve bloqueo específico.
- **Lyra después**, compartiendo estándar de materiales, esqueleto y animación cuando sea posible; debe conservar silueta de arquera y poder usar arco sin deformación de manos/hombros. No generar dos héroes simultáneamente.

### P2 — Producción en serie, solo tras el piloto

- Validar tasa de aciertos con varios modelos de distinta familia, minutos humanos, rechazos artísticos, consumo de créditos y memoria móvil.
- Ensayar multivista/P1/segmentación solo con una pregunta falsable (p. ej., ¿mejora espalda/capa/manos sin introducir artefactos frente a H3.1 por imagen?).
- Crear fichas y referencias de armas/equipos reemplazables, topologías de hombro/codo/dedos y materiales repetibles entre héroes. Mantener separados retrato de diálogo, miniatura y modelo 3D jugable.

## 6. Condiciones objetivas de aceptación de Aldric y Lyra

Un modelo de héroe **NO está terminado** por importar un GLB. Requiere:

- Identidad visual fiel a concepto explícitamente aprobado, no reemplazar rostro por un «caballero genérico»; anatómicamente correcto, manos, dedos, hombros, cuello y pies revisados en cuatro vistas.
- Silueta legible en vista de combate y miniatura 4X. Personaje reconocible sin necesitar textura 2K permanentemente.
- Skeleton/skin exportables y comprobación de huesos/pesos/bind poses; clips idle, caminar, correr si el gameplay lo usa, ataque específico, daño/recuperación y muerte **solo en el alcance requerido por el sistema real**, con loop y retorno sin drift.
- Capa/cabello/armadura/arma sin clipping grave durante los clips; puntos de sujeción para equipo y cambios futuros. No confundir «una animación aislada» con equipamiento plenamente modular.
- LOD/texturas/atlases dimensionados para escenarios reales de simultaneidad, no exclusivamente un render de escritorio. La escala, pivote, orientación y collider deben coincidir con sus acciones.
- A/B **Unity 6000.3.23f1**, mismas cámaras oficiales de juego, misma iluminación y estados jugables; revisión artística independiente y rendimiento físico móvil posterior.
- Guías de recuperación: GLB original + SHA, .blend editado, importador/rig/anim clips, renders, logs, task y coste, fuentes/procedencia/condiciones comerciales. Progreso del héroe, habilidades, guardados y encuentros existentes invariantes.

## 7. Reglas que no debemos olvidar

- Una **única generación pagada por activo** y sin reintentos automáticos; consultar saldo y coste efectivo del modo/versionado/ajustes *antes* de enviar. En caso de fallo posterior, recuperar task y artifact originales.
- Evitar secretos en documentación o código; la ausencia de API key no autoriza copiar claves desde el PC o trasladar perfiles.
- El pipeline existente es la base: [TRIPO_MODULE_PIPELINE.md](TRIPO_MODULE_PIPELINE.md), [TRIPO_API_NORMA_PRODUCCION_V1.md](TRIPO_API_NORMA_PRODUCCION_V1.md). No crear un flujo paralelo ni activar pipeline/tripo-module-request.json (actualmente enabled=false y gasto no aprobado).
- El área artística activa del Engendro, la migración de Bastión III y la escena principal tienen dueños y PR; esta auditoría NO ocupa Windows, NO produce personajes, NO inicia API, NO cambia Unity y NO publica.
- Distinguir en cada entrega cuatro cosas distintas: fuente existente, técnica validada, calidad aprobada e integración real. El caso Engendro demuestra por qué importa: varios documentos estáticos dicen «sin rig» cuando la evidencia posterior V4.2/native280 prueba un zarpazo real.

## 8. Decisión de producción recomendada

**Preparar Aldric ahora como piloto visual y técnico.** El diseño facial ya tiene una dirección atractiva. Primero consolidar una referencia frontal/lateral/trasera animable y resolver localmente el flujo humanoide + equipamiento, sin pagos. Después, si es necesaria generación, pedir autorización para una sola tarea de imagen→3D con precio efectivo comprobado. **Lyra seguirá el método certificado**, no improvisará uno nuevo.

**Bloqueos abiertos explícitos:** identidad de cuerpo completo; calidad real de salida image-to-model para este héroe; rig biped completo con armadura y capa; cambios de equipo; costo/saldo API en la cuenta; licencias comerciales aplicables específicamente a los resultados API; móvil físico/perfil térmico; diferencia práctica de v3 respecto al pipeline v2. Ninguno obliga a rediseñar la historia ni a modificar Bastión III.

---
### Evidencia interna de mayor autoridad
- [Rig V4 y comparaciones](evidence/spawnling-animation-v4-review-20261021/RESULT.md)
- [Unity rig V4.2](evidence/spawnling-animation-v4-review-20261021/unity-approved.json)
- [Nativo integrado 280](evidence/spawnling-animation-v4-review-20261021/NATIVE280_ACCEPTANCE.md)
- [QA nativa+browser 281](evidence/spawnling-animation-v4-review-20261021/NATIVE281_COMBINED_BROWSER_ACCEPTANCE.md)
- [Integración criatura original](evidence/spawnling-bastion2/ART_AND_INTEGRATION_REVIEW.md)
- [Tres activos Tripo](TRIPO_API_TRES_ACTIVOS_RESULTADO_20261021.md)
- [Procedimiento de LOD](TRIPO_MOBILE_LOD_PROOF_20261021.md)
