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
