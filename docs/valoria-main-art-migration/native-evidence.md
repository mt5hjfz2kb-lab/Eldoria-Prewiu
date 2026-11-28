# QA de referencia y acceso a pruebas
Investigación de solo lectura, 28-11-2026 (UTC). No se ha despachado ningún job ni se ha ocupado el equipo.

## Fuente de Main
- Main original d74f1a29d5472e9037d8534142768d4c13667ddd y a2e956e11bc837a081c3eab1eb05c8ff094f4e05 tienen el árbol Unity/ db625919f50f2489d2a7cf3c107789b1392f8e86. Comparación de ambos árboles raíz de Git por GET /git/trees/{sha}. El último commit que toca Unity/ en la ascendencia de aquel Main es a2e956e… .
- GitHub Actions devolvió un único run directamente asociado a d74f1a29, https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/46167344121 , Mission Continuity Guard, failure. El run de UI histórico verde https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43433023729 y el run de build https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43428539668 corresponden a a2e956e. La identidad de Unity permite consultar estas referencias de juego; no demuestra que los pasos externos de build o el commit entero fueran idénticos. Los ZIP incluyen posibles capturas de control históricas que deben distinguirse por ruta/manifiesto antes de usarlas.
- Donante 6057c2604de84ae27f47ede44eafa294c96bcd24 sin run directamente asociado. Adopción trazada en https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/200#issuecomment-7242338670 . Run 46165306852 adopta el refinamiento Bastión V del candidato de 46160199450. La evidencia de ese candidato no es una ejecución del donante exacto.

## Capturas de referencia abiertas
ZIP descargados, directorio ZIP inspeccionado. Conteo de archivos PNG, sin asumir que cada PNG representa un estado distinto o que es independiente de los otros paquetes.
|Artefacto|PNG|Contenido observado|
|---|---:|---|
|14042155990|7|Nombre y diálogo|
|14042126128|8|Marchas y compositor en varios formatos|
|14042200967|61|V4.1, fichas, destinos e información de marcha|
|14043052275|6|WebKit escritorio, con subcarpetas baseline18 y candidate|
|14039804707|343|BreachWatchFirst, contiene FirstEntry y capturas de fixtures anidados|
|14042379101|278|QA UI/progresión y región, contiene vista cercana y selección de recursos|
|14042563920|12|Aldric en ciudad, fuera de ciudad, marcha y mundo|
|14040641861|9|Exploradores y Vigilantes|
Build WebGL 14039969404 y paquete de propietario 14041881312 localizados por API. No ejecutados en esta auditoría de lectura.

## Windows
- Fuente https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/d74f1a29d5472e9037d8534142768d4c13667ddd/pipeline/active-workstreams.json . En el registro consultado, el único titular windows-runner-heavy es eldoria-valoria-modular25d-bastion1-20261119, owner chatgpt-corona-brecha-commercial-20261126, revisión 40, rama donante. resource_rotation status corona_native_reserved, current_run_id null, next_workstream null, return_to null. area_handoff_request superseded. El último préstamo terminó en devolución explícita, 15:46:44 UTC. No hay préstamo o devolución a favor de esta migración. Ausencia de run_id no libera la reserva.
- Consulta Actions repositorio. En la instantánea se observaron dos runs queued #37642384303 y #37642364289 (Unity WebGL published startup probe, ambos sin jobs en API), un run in_progress #46170179848 (Art production governance, job governance en ubuntu-latest), cero waiting. Segunda comprobación necesaria inmediatamente antes de ocupar equipo. Esto no demuestra que Windows esté libre.
- Protocolo https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/d74f1a29d5472e9037d8534142768d4c13667ddd/docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md , secciones «Dependencies without collision», «Shared runner rule» y «Relevo seguro tras cada área». Permite utilizar recursos ya liberados, o relevo/transferencia segura autorizada. Una cola vacía no permite desplazar al titular. Esta migración consume las salidas de la otra misión; es distinta de continuar esa misión en esa misma PR. Si el titular libera temporalmente solo el PC puede conservar su ámbito de escritura. No se ha solicitado al titular una transferencia por ningún canal.
- https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/d74f1a29d5472e9037d8534142768d4c13667ddd/.github/workflows/ui-progression-cert.yml#L300 revisa lease y procesos antes de checkout. Rechaza el claim actual de Corona para la rama de integración (allowlist histórica). valoria-reference-clean.yml restringido explícitamente a main y fix/player-experience; guardas comprueban registro vivo y procesos. No despachar ni alterar otro titular para circunvenirlas.

## iPhone
- Entorno Linux tiene Chromium, sin Unity en PATH, sin USB expuesto (/dev/bus/usb ausente), idevice_id/xcrun/ios_webkit_debug_proxy/adb no instalados en PATH. No se expone conector de iPhone ni de granja remota. Ninguna evidencia de dispositivo accesible desde esta sesión.
- Las rutas de QA existentes llaman WebKit remoto en macOS 15. https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/d74f1a29d5472e9037d8534142768d4c13667ddd/.github/workflows/eldoria-beta-iphone-postready-profile.yml comprueba explícitamente physical_iphone_verified false. Los workflows de entrega A/B al propietario publican o empaquetan una página. No controlan un iPhone ni certifican estabilidad física.
- La guía https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/blob/d74f1a29d5472e9037d8534142768d4c13667ddd/docs/ELDORIA_PLAYTEST_DELIVERY.md describe una vía Tailscale privada para entrar desde el iPhone (históricamente pendiente del inicio de sesión del propietario), lo que tampoco proporciona control remoto del dispositivo. Para el SHA candidato exacto hará falta un iPhone accesible de verdad o una prueba ejecutada por un operador con evidencia trazable.
