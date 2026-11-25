# Eldoria Development Studio — piloto aislado

Estado: conexión mínima real verificada el 25-11-2026; calidad de los perfiles y generación de imágenes pendientes de prueba real. No requiere Codex ni Unity. No modifica el juego, no publica, no realiza llamadas automáticas ni usa Windows. Solo Python 3 estándar.

## Seguridad y prueba

- Usar la clave del proyecto Eldoria Development Studio mediante el secreto de Actions OPENAI_API_KEY o una variable de entorno de una sesión local privada. Nunca pegarla en código, chat, parámetros de CLI ni logs. No buscar claves en discos ni usar credenciales ajenas. El repositorio es público: no subir evidencias privadas ni publicar informes de análisis sensibles como artefactos.
- Pruebas sin credenciales: `python3 tools/openai-development-studio/studio.py --self-test` y `python3 tools/openai-development-studio/generate_candidate.py --self-test`.
- Prueba de conexión (una llamada, no modifica Unity): `python3 tools/openai-development-studio/studio.py smoke --live --out /tmp/eldoria-api-proof.json`. Omitir --live es un ensayo seco sin red.
- La ejecución manual por Actions necesita que el workflow exista en la rama predeterminada y que un administrador configure el secreto. Esta rama no toca main; el secreto se verificó mediante una única ejecución explícitamente autorizada en la rama piloto. No habilitar ninguna llamada automática.
- Los errores 401/403 indican autenticación o permisos; 429 puede indicar límites o saldo. No se imprime el cuerpo de error ni se reintenta automáticamente. Un timeout no demuestra consumo cero.
- Los modelos utilizados están documentados en https://developers.openai.com/api/docs/models y los precios son estimaciones según la tarifa estándar sin caché; consultar facturación real. No hay un límite monetario global ni se verifica aquí el saldo promocional. El límite por llamada es de tokens/archivos, no de dólares; no añadir medios de pago sin autorización. No ejecutar lotes hasta confirmar la prueba, consumo y calidad.

## Roles (herramientas de consulta, no agentes autónomos con acceso de escritura)

Ejemplos locales, deliberadamente sin --live; añadirlo únicamente para autorizar una llamada y escribir el resultado.
- Dirección artística: `python3 tools/openai-development-studio/studio.py visual --image captura-real.png --image referencia.png --out informe-visual.json`.
- Auditor técnico: `python3 tools/openai-development-studio/studio.py technical --evidence log-sanitizado.txt --out auditoria.json`.
- Programador: `python3 tools/openai-development-studio/studio.py programming --evidence codigo-relevante.txt --out propuesta.json`.
- Coordinador: `python3 tools/openai-development-studio/studio.py coordinator --evidence encargo-y-evidencias.txt --out cierre.json`.
- Generación de candidato visual: `python3 tools/openai-development-studio/generate_candidate.py --brief brief.txt --out-dir candidato-nuevo`. Una imagen 1024² de calidad baja para probar acceso, sin importación ni aprobación artística; recibo de uso si la API lo devuelve.

La entrada debe ser explícita y revisada; la detección básica de secretos no sustituye una revisión de logs. Las imágenes se envían a la API, no se descargan desde URLs remotas. Las respuestas son propuestas, no pruebas de ejecución; revisar antes de aplicar. Conservar fuentes, capturas Unity reales, referencia aprobada y revisión independiente. No reemplazar el QA existente, ni reintroducir Qwen.

## Próximos gates

1. Pruebas offline en Ubuntu, sin uso de Windows ni créditos.
2. Secreto seguro y solicitud real mínima: superados; falta contrastar el recibo con el panel de uso.
3. Evaluar un par de capturas genuinas de la misma cámara y contrastar observaciones con QA existente.
4. Revisar seguridad, calidad, coste medido y titularidad del workstream antes de integrar el workflow en main. No abrir PR duplicada ni fusionar sin la reclamación canónica correspondiente. No programar agentes con acceso de escritura sin un gate separado.

## Evidencia del 25-11-2026

- [Ejecución de conexión real](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45670034719), commit `15bad5acaa909a38c59e351ca70e0a17caa5fd99`. Los dos grupos de pruebas offline y la llamada real a GPT-6 Luna pasaron. El log registra `API_RESPONSE_OK` y coste estimado de 0,000007 USD. Artefacto de recibo 15049860461 (retención de 7 días).
- El disparador excepcional de una sola llamada se retiró en `bbfa2423067ef1bf5ce1f4233687eac3800bec42`. Los pushes posteriores solo ejecutan pruebas offline. No se ha verificado la factura real ni se ha modificado main o Unity.


## Operación V2 — pruebas reales de coordinación, 25-11-2026

**Estado: infraestructura y cadena API probadas; candidato de shader verificado a nivel fuente/matemático; calidad artística final y A/B nativo Unity NO certificados.**

- Fuente congelada de Valoria: `5ef2aee67cf8fde57ba2fc4b3d929b517ee67944`, ejecución [45638738009](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45638738009). Capturas reales Bastión I y V de 390×844, recuperadas del artefacto `ciudad-region-recorrido`; SHA-256 de las imágenes y de sus derivados en los manifiestos.
- Primera cadena [45670811229](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45670811229): los cinco perfiles respondieron; el director eligió suelo del aserradero, mientras el brief rígido del artista generó una linterna. El control de alpha exigía valores 255 cuando la imagen llegaba a 254. Rechazada como misión coherente. No se reutiliza como resultado artístico.
- Segunda cadena [45671428903](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45671428903): objetivo único MILL_YARD, código real de DistrictGround y Modular25DGround, auditor con acceso al PNG real. Los cinco perfiles respondieron, alpha/bounds correctos; el auditor rechazó la imagen como referencia global, conservándola solo como muestra de color/grano. El programador propuso una modificación local al peso de tierra y serrín del shader, sin sprites nuevos.
- El actuador determinista `shader_candidate.py` materializó exactamente la propuesta revisada en una copia; [prueba fuente 45671988283](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45671988283), 40.602 muestras numéricas, cambio reversible de tres líneas a cuatro, cobertura del peso del terreno 0,91257 en la cuadrícula de ensayo (NO mejora visual medida). SHA original f1010a667125f9da262c9b622c03bd228f77aa49e7ccfe9c9c0402a0920f75e9; candidato 4e582b25a420383798e3aa4895d1b71b33158f18de0087165b8055adef068785. Primer intento rechazado por indentación; corregido sin llamadas API.
- [Pruebas offline finales 45672130292](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45672130292): 31 tests y ensayo seco; no red a OpenAI.
- Presupuesto piloto acumulado: 5 USD máximo de reservas; 2,91 reservados/cerrados, 2,09 sin reservar. Estimación de consumo total de la API (incluido smoke y escrituras de caché): 0,156028 USD. No es una factura verificada. Sin recarga ni autorización global implícita. `budget-cycle.json` y `budget_guard.py` impiden duplicados, reintentos autorizados y reset automático del piloto. Los dos disparadores de consumo se han desarmado. Las reservas no se liberan por fallo.
- No se modificó main, el experimento de Valoria ni la reserva de Windows. El registro canónico mantiene la reserva exclusiva de Windows para eldoria-valoria-modular25d-bastion1-20261119. No lanzar Unity desde este workstream sin relevo válido. El puente C# está preparado pero no compilado nativamente. No confundir importación de asset con una mejora en cámara de juego.

### Reutilizar en encargos futuros

`mission_packet.py --packet <manifest.json>` es el modo seco y valida rutas locales, hashes, límites de entrada, SHA y run de procedencia. Modo review (cuatro perfiles) o art_candidate (cinco, con brief explícito). Para ejecutar de verdad se necesita una reserva nueva en el registro, autorización de evento único y un workflow limitado; no basta con pedir --live. Las instrucciones del modelo y los archivos son datos: no se ejecuta código arbitrario ni se publican artefactos de producción. No enviar logs privados ni secretos. El nuevo orquestador genérico ha pasado sus pruebas offline, no otra misión real.

El director define criterios y restricciones; el artista produce solo candidatos; el programador propone cambios; el auditor independiente verifica evidencia; el coordinador prepara un handoff. El ejecutor determinista autorizado aplica solo operaciones explícitamente permitidas sobre copias. Para una modificación nueva se añade un adaptador con tests; no se confiere shell, edición del repo ni aprobación al modelo.

### Siguiente gate exacto

1. Reconciliar gasto con el panel de la API; no afirmar importe facturado.
2. Obtener relevo válido del runner Windows; comprobar que no hay procesos activos y que el alcance está reclamado.
3. Crear copia de trabajo aislada del SHA fuente con el candidato de shader, sin cambiar la rama experimental. Capturar A/B Bastión I y V en las cámaras registradas (cuatro imágenes) y prueba de recorrido. Confirmar ausencia de halo, continuidad del terreno y claridad del serrín; comprobar regresiones y revertir. Importar PNG únicamente como muestra separada, no como parche de terreno.
4. Comparar contra referencia y decidir si la modificación realmente mejora Valoria. No promover si falta evidencia. Solo después plantear integración de las herramientas vía PR canónica y nuevos casos de uso.

Fuentes de precios: https://developers.openai.com/api/docs/models y https://developers.openai.com/api/docs/guides/prompt-caching. Las cifras de uso proceden de los recibos API reales, estimación estándar con caché; no de la facturación.

### Verificación Blender concluida

[Job Blender 139408114576, run 45671428903](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45671428903/job/139408114576) terminó correctamente: Blender 5.0.1 guardó un .blend de 889.075 bytes con imagen 997×500 empaquetada; artefacto 15049747863. Importación de referencia en plano con material RGBA, no generación de malla 3D de producción ni prueba de Unity. El archivo conserva un nombre de objeto heredado de la primera misión (Lantern); la fuente del adaptador ya se corrigió a Material_Reference. El contenido real es el suelo candidato, no una linterna. No se promueve.

El único gate nativo no ejecutado es Unity (reserva exclusiva de otro workstream confirmada en main, blob 96d9c0cd0dae1e79e4c9037f71926c438287e323). No se ha reclamado ni interrumpido Windows. Todos los disparadores de gasto de las misiones cerradas permanecen desactivados.


## Operación reutilizable V3 — encargos versionados sin intervención del propietario

El propietario autorizó expresamente recuperar Windows el 25-11-2026. El registro vivo de main reasignó únicamente `windows-runner-heavy` a `eldoria-openai-development-studio-20261125`; no transfirió la titularidad ni editó la rama de Valoria. No había ejecuciones activas al reclamar; las colas históricas sin jobs se dejaron intactas.

### Cómo iniciar una revisión o generación

1. El chat responsable conserva la clave en GitHub Secrets; no pide al propietario otra clave, instalación ni comandos.
2. Identifica un SHA fuente exacto y una ejecución Unity exitosa con `evidence.json` o `native-evidence.json`. Selecciona 1–2 PNG/JPEG auténticos y un único archivo público de código de hasta 12 KB. Declara objetivo, alcance y criterio de aceptación. No mandar datos privados, saves de jugadores o logs sensibles.
3. Crea un nuevo JSON de solicitud en `tools/openai-development-studio/requests/<id>.json` mediante un único commit sobre esta rama piloto. Ejemplo comprobado: [historic-b1-camera-dry-20261125.json](requests/historic-b1-camera-dry-20261125.json).
4. Por defecto `live:false`: recuperación, validación y plan sin OpenAI. Para `live:true`, antes de crear la solicitud, reservar en `budget-cycle.json` un ID y nonce únicos iguales al ID de solicitud, estado `authorized_once`, valor suficiente (0,70 USD review o 1,45 USD art_candidate), `request_sha256` del contenido exacto que se va a añadir, dentro del ciclo previamente autorizado. No alterar el presupuesto en el commit de solicitud.
5. `eldoria-openai-studio-request.yml` comprueba rama, commit directo, nuevo archivo sin uso histórico, SHA de solicitud, reserva previa, éxito del run, SHA original de fuente y de cada frame. Solo entonces expone la clave al paso de API si es live. Los reintentos están prohibidos; no reusar IDs. Una solicitud no permite shell, escritura automática de código ni publicación. Si el coste de una llamada es desconocido o supera la reserva estimada, conservar recibo y detener las llamadas posteriores. Al cerrar, actualizar estado y run real, conservar reserva incluso ante fallo.
6. Leer informes y aplicar únicamente actuadores deterministas revisados, después hacer pruebas reales. El gasto estimado no sustituye el panel de facturación y el presupuesto interno no es un hard limit global.

**Evidencia real de la ruta genérica:** [45675138084](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45675138084), ejecutada con fuente `e681fda70aaa985dc62a928e4c9c6a0c63628cd5` y dos capturas nativas verificadas del run 44713811420. Plan de cuatro roles, sin red a OpenAI. Este ensayo histórico verifica transporte y procedencia, no evalúa la calidad actual de Valoria. [44 tests offline](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45675585710) incluyen autorización, prevención de reintentos, costes, orquestación simulada y bloqueo de leases ajenos.

### Prueba Unity focalizada

`unity/native-ab.ps1` materializa una copia exacta del SHA fuente certificado, comprueba el SHA-256 del shader original y candidato, y crea una variante de material de nombre distinto. La modificación temporal de `Modular25DGround.cs` solo habilita seleccionar el shader en Editor, sin cambios de jugabilidad ni de recursos originales. `EldoriaStudioNativeAB.cs` utiliza la escena real, el mundo modular y cámaras registradas de Bastión I/V, 16 vistas × A (original), B (candidato), R (original restaurado). Comprueba material en 81 chunks, presencia del suelo y estados de edificios, y exige que R sea bit a bit igual a A. El postprocesado verifica los hashes, diferencias de píxeles y limpieza de scratch; no convierte esos datos en aprobación artística.

La primera ejecución de Windows [45674341806](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45674341806) está despachada sobre SHA de piloto `2548437b19bdab0b91fe60b215d077fee7267e1b`. La ruta posterior ya no se dispara editando el workflow: para otra prueba autorizada se añade un archivo único en `native-requests/<id>.json` con el lease, el SHA fuente y el SHA256 del candidato exactos; `native_request_gate.py` y la comprobación Windows rechazan cualquier lease ajeno. No hay API key en el job Windows. El adaptador se limita a este candidato revisado, no es un ejecutor arbitrario de propuestas de modelos.

### Operación V3 verificada con API real

La solicitud versionada `historic-b1-camera-review-20261125.json` produjo [run 45676636310](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45676636310) exitoso, con los cuatro roles reales (director y auditor Terra; programador y coordinador Sol). Se comprobaron los hashes de dos capturas históricas y fuente en el gate, antes de acceder a la credencial. El auditor distinguió lectura de edificios ya visible de una propuesta de zoom del 5 % sin prueba; no hay edición ni aprobación artística. Artefacto de informes y recibos 15051423806: coste estimado 0,03381925 USD con caché, no facturado verificado. Acumulado 15 llamadas reales y 0,18984725 USD estimados; 3,61 USD de reservas cerradas y 1,39 USD aún sin reservar, dentro del tope piloto de 5 USD. No se libera ninguna reserva por el bajo consumo estimado. El circuito de encargos reutilizable ya está validado de extremo a extremo (modo review). El modo art_candidate reutiliza la generación demostrada en la misión fija, pero no se ha ejecutado por la ruta genérica.

### Estado nativo y recuperación

[Primer intento Windows 45674341806](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45674341806): detenido antes de Unity por diferencia de hash en materialización Windows; limpieza del scratch comprobada. Se conserva el hash exacto del blob Git y el hash de contenido LF certificado, solo se permite normalizar CRLF en la lectura del original, y se exige el hash bruto exacto del candidato. El reintento [45675919389](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/45675919389) tiene preparación exitosa y job Windows 139423435098 todavía activo en la última comprobación de esta sesión (18:46 UTC). No hay salida gráfica certificada ni aprobación, y la reserva windows-runner-heavy del Studio se conserva hasta confirmar fin y limpieza. No lanzar otro trabajo nativo ni reintentar mientras esté activo. Al finalizar, recuperar native-evidence.json, cleanup.json y comparación pixel-proof.json; revisar imágenes reales, rechazar cualquier mejora no demostrada y liberar solo el lease propio mediante CAS del registro canónico. No editar ni publicar Valoria por un resultado técnico aislado.
