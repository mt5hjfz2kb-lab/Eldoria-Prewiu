# ELDORIA — Sir Aldric | Piloto de héroe 3D, preproducción segura

**Fecha:** 27/10/2026. **Estado:** `VISUAL_CANDIDATE_REVIEW_PENDING / NO_TRIPO_TASK / NO_RIG_HUMANOID_PASS / NO_UNITY_INTEGRATION`.  
**Solicitud de propietario:** comenzar por Aldric, aprovechar lo aprendido con Tripo y el Engendro; preparar un personaje humanoide de calidad reutilizable para futuras armas y armaduras.  
**Ámbito de este documento:** solo preparación artística/técnica, sin adquirir activos ni modificar gameplay, escenas, workstreams, solicitudes Tripo, publicaciones ni guardados. **Coste nuevo: 0 créditos Tripo.**

## 1. Fuente visual y estatus de aprobación

El propietario expresó que le gusta el **Aldric veterano de los conceptos históricos**: cabello y barba canosos, rostro marcado, mirada seria, acero oscuro, capa azul profundo. **La dirección general SÍ está aceptada como punto de partida; la nueva lámina multivista y sus detalles NO están aprobados como modelo definitivo.**

Referencias históricas disponibles en la Library de la conversación:
- `El despertar de Valoria.png`: retrato y cuerpo superior del Aldric veterano, base preferida de rostro.
- `Interfaz medieval de Valoria en evolución.png`: alternativa de vestimenta/diálogo, armadura oscura con capa azul.

Candidato ilustrativo realizado el 27/10/2026 para revisión, conservado como archivo entregado en la conversación:
- `aldric_lamina_conceptual_borrador.png` SHA256 `34c33cb955a32e5b171bad3efd4d7affc122e30e4070303bf7f3e97d3aa95731`.
- `Aldric_hoja_vistas_para_revision.png` SHA256 `bc47c39f8fd41de1c1c0c61f24be3d70c908174d61f44831f98d85f0d7605e10`.
- Del candidato se separaron recortes frontal, lateral, trasero y retrato para revisión de continuidad. El recorte frontal tiene poca resolución nativa y NO es entrada final aprobada para generación de imagen a 3D. La lámina original contiene **texto/diagramas ilustrativos** (incluidos wireframes y métricas visuales ficticias); **nada de ello constituye geometría real, UV, triángulos medidos ni animación de Unity**.
- La heráldica del candidato ilustrativo (águila/sol) NO es canon. Debe seguirse el emblema real de Valoria verificado en el juego o aprobar un distintivo específico después, sin inventar símbolos por accidente.
- El concepto nuevo NO es una sustitución de retratos jugables ni se debe promover a archivo de textura, PNG de HUD o Tripo input sin visto bueno de calidad.

### ADN artístico conservado

- Caballero veterano y protector; nobleza desgastada, cansancio y resiliencia.
- Fantasía medieval estilizada, seria, moderada y legible; no hiperrealismo fotográfico ni videojuego caricaturesco.
- Armadura de acero oscuro, usada y reparada, con contrastes controlados, sin ornamentación de rey.
- Capa/tejido azul profundo acorde con Valoria; detalles dorados discretos sujetos a revisión heráldica.
- Proporciones humanas y pose neutral apta para esqueleto, codos/rodillas separados, manos y dedos distinguibles.
- La capa/espada/equipamiento deben ser piezas lógicas desacoplables, no estar soldadas permanentemente al torso ni mezclarse con dedos.

## 2. Comprobación de reutilización de biblioteca

Se inspeccionaron los índices obligatorios del repositorio:
`pipeline/asset-vault/master-catalog-v1.json`,
`pipeline/asset-vault/current-full-index-v1.json`,
`pipeline/asset-vault/historical-full-index-v1.json`
y `docs/asset-vault/GALLERY.md`.

**Resultado:** no se identificó un **modelo 3D riggeado y aprobado de Sir Aldric o Lyra**. El índice sí contiene varios assets históricos con `Hero` en el nombre que son, en realidad, edificios/castillos y fragmentos de entrada —por ejemplo `Valoria_HeroBastion_v1.glb` está **expresamente prohibido**— y un `valoria_hero_fragment.fbx` histórico NO aprobado. No confundir «Hero» de arquitectura con personaje.

Antes de solicitar *una nueva malla 3D*, repetir desde el checkout vigente:
`python3 tools/asset-vault/find-reuse.py "Sir Aldric humanoid knight armored character rig walking"` y revisar todos los candidatos contextualizados por licencia/uso. La lectura manual de índices no equivale a la ejecución del buscador sobre el HEAD que exista entonces.

## 3. Reutilización demostrada — Engendro

**Evidencia de éxito:** `docs/evidence/spawnling-animation-v4-review-20261021/unity-approved.json` (12 huesos; GLB skinned con pesos); `NATIVE280_ACCEPTANCE.md` (zarpazo real en Unity con retorno al reposo y escala preservada); `NATIVE281_COMBINED_BROWSER_ACCEPTANCE.md` (pruebas jugables).

Código existente para reutilización **como patrón**, no literalmente como esqueleto humano:
- `tools/tripo-art-brain/rig-feasibility-blender.py` — crear armature y asignar pesos en una copia, verificar movimiento.
- `tools/tripo-art-brain/rig-bone-elbows-v3.py` — articulaciones intermedias.
- `tools/tripo-art-brain/rig-expressive-swipe-v4.py` — poses, keyframes, corrección de pesos y clip exportable.
- `Unity/Assets/Eldoria/SpawnlingBastion2/Editor/SpawnlingAnimatedV4Integration.cs` — validar bindposes/pesos/clip, comprobar deformación real y preservar escala/colisiones/hotspots.

**Lo que sí demuestra:** rigging local + animación esquelética exportada/importada y renderizada.  
**Lo que NO demuestra:** esqueleto humano completo, pies estables en ciclos de locomoción, espada y escudo, dedos asiendo arma, capa sin clipping, equipamiento configurable, retarget a otros héroes, 30/60 FPS en teléfono físico.

## 4. Ruta técnica de preflight sin coste — alistamiento concreto

### P0 — preparación antes de una llamada a Tripo

1. Resolver materiales fuente de Aldric: retrato aprobado, silueta de cuerpo completo coherente, vista frontal neutra A-pose/T-pose, perfil y espalda con proporciones compatibles. **No utilizar la pequeña captura multivista de la lámina como entrada premium definitiva.**
2. Confirmar el cuerpo y la capa como objetos/secciones separables; especificar cascos, botas, armas y armadura sin fusionar dedos ni articulaciones.
3. Preparar un **humanoide de pruebas libre/licenciado** o identificar uno existente en la biblioteca; correr Blender sin Tripo con rig bípeda y clips `idle`, `walk`, `attack`, `hurt`, y vuelta a `idle`. Reutilizar lecciones del Engendro, pero modelar explícitamente cadera, piernas, hombros, codos, manos y cabeza. Guardar SHAs, GLB/BLEND y render de cada pose.
4. Verificar los clips en **Unity aislado**, con `SkinnedMeshRenderer`, deformación por huesos, root motion controlado, pies apoyados, y equipamiento sin recortes graves. La salida mínima es cuatro PNG reales y reporte reproducible; **no tocar la escena canónica**.
5. Preflight de coste de Tripo (modelo, modo imagen, textura, saldo, rig-check y licencias), sin ejecutar tareas pagadas. Si la credencial no está autorizada en el entorno actual, informar el bloqueo sin extraer secretos. La solicitud `pipeline/tripo-module-request.json` permanece `enabled=false` / `approved=false`.

### P1 — Piloto Tripo *solo con autorización expresa de créditos*

1. Una generación imagen→3D con referencia definitiva; Tripo H3.1 es candidato por experiencia previa, no obligación. El coste publicado de imagen puede diferir del texto; verificar presupuesto efectivo antes de enviar.
2. Guardar inmediatamente `raw.glb`, `task_id`, SHA256, modelo/opciones, coste real y trazabilidad; si falla Unity/Blender **no repetir Tripo**: recuperar la misma tarea.
3. Inspeccionar anatomía de rostro, muñecas, manos, hombreras, capa y simetría. Ante errores severos descartar/escalar antes de gastar nuevas tareas.
4. Blender → rig bípeda / skin / pesos / clips → GLB optimizado y BLEND fuente editable, sin fusionar capa y armas al cuerpo.
5. Unity QA aislado y, solo con permiso del titular de Valoria y valoración visual positiva, prueba en cámara oficial y perfil móvil.
6. **Lyra después**, reaprovechando proceso comprobado pero verificando específicamente arco, flechas, dedos, hombros y capa.

## 5. Aceptación/rechazo del piloto

| Gate | Aceptar cuando | Rechazar cuando |
| --- | --- | --- |
| Identidad | Rostro y carácter del Aldric de referencia, proporciones creíbles, tres vistas consistentes | Caballero genérico, edad/cabello cambiantes |
| Silueta | Heroico pero práctico y distinguible en escala 4X | Se convierte en mancha metálica o pierde la capa |
| Anatomía | Dos brazos/piernas, manos correctas, articulaciones separadas | Dedos/arma fusionados, hombros inanimables |
| Rig | Pesos/bindposes y articulación humana en poses reales | Pie deformado por brazo, extremidades atravesadas |
| Animación | Idle, caminar, ataque con espada, daño y retorno estables | Solo gira el objeto entero; clipping grave |
| Modularidad | Espada y escudo con sockets; capa tratable sin romper huesos | Equipo permanentemente fundido e intercambiable solo regenerando |
| Unity | 4 capturas genuinas en cámaras distintas, sin tocar jugabilidad | Solo render ilustrativo y/o logs PASS sin evidencia visual |
| Rendimiento | Perfil dispositivo físico y coste tolerable para varios héroes | Solo triángulos aislados o FPS de un PC |

Ningún modelo humanoide está `ART_APPROVED` ni `INTEGRATED` en este documento.

## 6. Salida real de esta fase y frontera operativa

**Completado en preproducción:** referencias localizadas, lámina visual candidata y recortes de revisión, inventario de reutilización en los índices, trazabilidad exacta del rig del Engendro, ruta de pruebas y criterios de rechazo.

**No realizado / siguiente bloque real:** prueba de locomoción humana en Blender/Unity requiere ejecutor Blender/Unity autorizado o disponer de un humanoide fuente; no hay prueba de esa ejecución en esta sesión. En la sesión local de elaboración no hay editor Blender/Unity disponible. **No alegar TECH PASS ni gastar 30 créditos como si la imagen fuese input final**. La próxima petición al responsable será solo el pequeño preflight P0, sin cambiar Bastión III ni reclamar Windows ocupado.

**Fuente de normas:** [auditoría consolidada de Tripo](TRIPO_API_AUDITORIA_CAPACIDADES_Y_BRECHAS_20261027.md), [norma Tripo](TRIPO_API_NORMA_PRODUCCION_V1.md), [pipeline canónico](TRIPO_MODULE_PIPELINE.md), [biblia de arte](ELDORIA_ART_BIBLE.md), `AGENTS.md` y `pipeline/active-workstreams.json`.

**Decisiones artísticas pendientes del propietario:** aprobación final del diseño de cuerpo completo, heráldica y equipo principal. No se trata como autorización implícita de gasto, publicación ni modelo definitivo.
