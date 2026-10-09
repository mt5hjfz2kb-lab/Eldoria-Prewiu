# Recuperación real del ejecutor local — 2026-10-09

## Veredicto
PASS_SUPPORT_ONLY para el trabajo existente eldoria-local-agent-preflight-v1. Autonomía integral de Unity y de los quince departamentos: NO CERTIFICADA. No hay mejora del videojuego aceptada en esta intervención.

## Entregas efectivas y evidencia
- Transferencia autorizada antes de modificar: fc93ec50b64321105945232c40d26f28873a71e7; Issue #25 comentario 6081135931. Se conserva el mismo ID.
- M16 existente prepara órdenes reales; el workflow preflight existente transporta contexto al runner, ejecuta Ollama privado, separa QA Linux, devuelve defectos para un único reintento, registra aceptación/rechazo en #23/#25 y despacha la siguiente orden finita.
- Política de alias producida por Qwen: [run 37933225117](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37933225117), 47 tokens, cinco comprobaciones independientes, candidate SHA256 0a72816321f8c31ec87fd20a649e9a2713fadb33309e1fb34b07fae7e9e8ed2b. Aceptada en commit 6cf6b919ccfa276b1bd13d983c45439f3b61bdf7. El control de exclusión carga esa política real y rechaza alteraciones.
- El reporter lanzó [37933335164](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37933335164) mediante workflow_dispatch como github-actions[bot], sin Work ni propietario entre transiciones. Sus dos intentos reales fueron rechazados.
- [37933859220](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37933859220) demostró devolución automática del candidato y error preciso. El modelo modificó la etiqueta defectuosa (hash distinto) y el retest rechazó la cobertura incompleta. No se aceptó ni escribió el resultado defectuoso.
- [37934100027](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37934100027) confirmó el límite de razonamiento conjunto de Qwen 3B: incluso con arrays exactos hubo etiquetas y cobertura incorrectas. Reintento y rechazo independientes conservados.
- Work corrigió el adaptador, no los resultados del modelo: siete entradas autorizadas exactas, siete inferencias pequeñas; las etiquetas siguen viniendo exclusivamente de Ollama. [37934434816](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37934434816) PASS: siete comprobaciones independientes, 70 tokens de inferencia, candidate SHA256 abe510ab3d55a4f60a2fb4302c099801ea94f7cbecca155d9d4a5f3339bea832. Commit de aceptación b27f1c2f65adda2d0b3ba8b894dfd3c408c39be0. Evidencia QA artifact 11618120268, digest sha256:b93bcd545438a25c5046050b97026bec096809547c39db5f055bb5f90dda925f; resultado auténtico/raw respuestas/proof/cleanup artifact 11617611361, digest sha256:cd759078f1849a6cff945097010457baff421d299e054d865d43645190657b4d.
- El reporter volvió a despachar sin chat [37934576368](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/37934576368), actor github-actions[bot]: seis pruebas PASS, cero omitidas, incluyendo los datos reales aceptados; FINITE_AUTHORIZED_QUEUE_COMPLETE; todos los jobs Windows omitidos correctamente. Esto prueba continuidad y ausencia de repetición, no una sesión futura ilimitada.
- Informe DG automático: [comentario 6081495664](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/23#issuecomment-6081495664).

## Instalación, permisos y recursos
Se usó exclusivamente %LOCALAPPDATA%/EldoriaLocalAI/ollama-v0.40.1/ollama.exe y %LOCALAPPDATA%/EldoriaLocalAI/models. Modelo exacto qwen2.5-coder:3b, digest f72c60cabf6237b07f6e632b2c48d533cef25eda2efbd34bed21c5e9c01e6225. No reinstalación, descarga, API de inferencia externa ni ejecución de código generado. Cada intento conservó identidad de proceso, listener loopback, hash de binario/manifest y cleanup independiente.

Reserva registrada R2-B preservada. La preparación inspeccionó jobs Windows en ejecución; el trabajador verificó reserva viva y ausencia de Unity/Blender/Ollama productivos antes de arrancar. Un solo runner, lock propio, tiempo acotado, apagado del árbol propio, listener cerrado y lock liberado antes de QA. No se canceló producción ni se cambió el proyecto Unity compartido. El segundo trabajo utilizó el mismo recurso ya liberado. No se despachó una compilación R2-B adicional porque pertenece a su propietario y no está incluida en la orden finita.

## Quince departamentos existentes
Clasificación de capacidad general: no inferir quince ejecutores inteligentes de estas pruebas.

| Departamento | Estado | Evidencia/límite actual |
|---|---|---|
| D01 Dirección | Parcialmente automatizado | Recepción GitHub automática verificada; no director ejecutivo inteligente persistente demostrado. |
| D02 Arquitectura | Parcialmente automatizado | Guards existentes preservados; no reparación arquitectónica autónoma general demostrada. |
| D03 IA/automatización | Ejecución autónoma verificada en alcance acotado | Dos entregas operativas locales aceptadas; siete inferencias pequeñas. El adaptador sólo admite las dos clases declarativas autorizadas. |
| D04 I+D visual | Pendiente de capacidad/herramienta | Qwen textual no inspecciona imágenes ni demuestra calidad visual. |
| D05 Arte | Parcialmente automatizado | Blender y producción existentes preservados; este modelo no reemplaza al autor/revisor artístico. M07 sigue bloqueado. |
| D06 Estado/persistencia | Parcialmente automatizado | Producción R2-B y pruebas existentes; ejecutor local general de C# no habilitado aquí. |
| D07 Jugabilidad | Parcialmente automatizado | R2-B propietario vigente; candidatos Unity y defectos de experiencia abiertos, sin aceptación nueva. |
| D08 World 4X | Bloqueado por dependencia | Región 1/M07 tienen propietarios y validación visual pendiente; no invadidos. |
| D09 Cámara/táctil | Parcialmente automatizado | QA táctil existe; falla conservada en 37932899075, no equivalente a corrección autónoma. |
| D10 UI/UX | Parcialmente automatizado | QA existente; experiencia subjetiva/general no cubierta por estos datos operativos. |
| D11 Integración visual | Bloqueado por dependencia | M07 requiere capturas/revisión independiente de candidato exacto. |
| D12 Animación/vida | Bloqueado por dependencia | R2-D espera M11-R1, según registro canónico. |
| D13 QA | Parcialmente automatizado | QA operativo Linux real, separado del trabajador y con rechazo/retest; no certificación visual general. |
| D14 Móvil/publicación | Parcialmente automatizado | Unity/WebGL y QA existentes preservados; no publicación estable ni nueva aceptación jugable. |
| D15 Audio | Bloqueado por dependencia | R2-D/M11-R1; Qwen no demuestra producción sonora. |

## Continuidad y pendientes reales
Las transiciones de esta orden terminada fueron ejecutadas por Actions y Ollama, sin leer chats. Se mantiene el activador de reevaluación quinceminutal, el despacho autenticado, expiración y límite de reintentos. La orden completada se deshabilita y se devuelve la cesión formal; una orden nueva exige su autorización/claim vigente, no un prompt entre cada paso.

No se ha construido un ejecutor general para encargos arbitrarios de Unity, arte o audio ni se ha certificado aceptación jugable. Los scopes productivos y sus defectos siguen bajo los propietarios actuales. La extensión requerirá adaptadores de herramientas y evidencia individual de capacidad; el fracaso auténtico de Qwen 3B obliga a mantener tareas pequeñas. No se activó ningún modelo alternativo.

QA anterior de Unity no se debilitó: 37932899075 conserva fallo de experiencia/cámara del candidato R2-B; sus tests independientes de orquestación y regresión de fuente sí pasaron. El soporte reparado no cierra esos defectos. Dirección General puede continuar las transiciones automatizadas existentes, pero no se puede afirmar que todo el desarrollo Unity continúe autónomamente sin intervención.
