# ELDORIA — Inicio de conversación: descubrimiento técnico obligatorio

**Finalidad:** nuevos chats normales/Work identifican automáticamente las rutas ya existentes. No inventar accesos ni exigir al propietario que repita el inventario. Este contrato de infraestructura NO autoriza a levantar la pausa de producción de 2026-10-14.

## Orden de arranque (sin gastar y sin tocar Windows)

1. Usar las herramientas conectadas de GitHub, cuando estén disponibles, para leer el **main/AGENTS.md vigente** (incluida la suspensión), HEAD de la rama encargada, las cabeceras vivas de `SESSION_HANDOFF.md` y `PROJECT_STATE.md`, y solo `active` de `pipeline/active-workstreams.json`. Si un acceso falla, registrar qué herramienta y permiso faltan, y probar alternativas autorizadas. No dar por hecho que un chat nuevo hereda credenciales o un terminal Windows.
2. Consultar los workflows existentes (`.github/workflows/`) y los últimos runs/jobs **del SHA pertinente**. Hay un runner local Windows con Unity y rutas históricamente probadas a Blender, Ollama y BlenderMCP; declararlo `históricamente verificado`, no `online ahora`, hasta ver el job actual.
3. Con checkout y Node disponibles, usar `node tools/eldoria-agent-preflight.mjs` para un diagnóstico barato y de solo lectura del **equipo en el que se ejecute**. Si únicamente hay conector GitHub, inspeccionar el script y las rutas mediante GitHub; no fingir que el script se ejecutó en Windows.
4. Nunca concluir que `127.0.0.1:11434` inaccesible desde Linux/ChatGPT implica que Ollama no está instalado en el PC. La evidencia de 2026-10-14 demostró que Ollama puede estar instalado y necesitar arranque autorizado bajo demanda en el runner Windows.
5. Distinguir cinco estados: `declarado en workflow`, `observado en ejecución anterior`, `accesible ahora desde el chat`, `comprobado en runner Windows en la ejecución actual`, `bloqueado`. **No son intercambiables.**
6. Elegir solo la ruta pertinente: GitHub connector para commits y lectura; Actions + self-hosted Windows para Unity o Blender/Ollama cuando se autorice; Ubuntu hosted para validaciones baratas; BlenderMCP cuando la edición artística lo requiera. No rebajar el alcance artístico a scripts procedurales solo porque el puente no sea visible al inicio.
7. Si falta ejecución/dispatch en el conector ChatGPT, verificar push-trigger existente o la alternativa autorizada, sin inventar endpoints ni modificar workflows productivos para sortear una prohibición. Si el recurso está reservado o la producción pausada, detener ejecución productiva.
8. Presupuesto 0 €; no instalar, descargar modelos, lanzar procesos de producción ni gastar créditos durante un descubrimiento. No tocar reservas ni trabajos de terceros.
9. **Regla permanente de calidad artística (2026-10-15):** antes de cualquier inferencia Qwen/Blender de Eldoria, leer la versión vigente de `main/docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md` junto con `main/AGENTS.md`. Verificar que el actual autor del artista conoce el contrato y que cualquier `ART_APPROVED`/`CALIBRATION_VALIDATED` se registrará con imágenes originales A/B, SHA y revisión independiente en `pipeline/art-visual-approvals/` usando el gate técnico de evidencias. Si el chat está trabajando en una rama antigua, la falta de ese archivo allí NO deroga la norma de `main`. No alterar workstreams ajenos ni levantar la pausa.


## Rutas/evidencias existentes (históricas, no autorización actual)

- **GitHub:** `mt5hjfz2kb-lab/Eldoria-Prewiu`. Las herramientas del chat deben comprobarse **en cada sesión**.
- **Unity Windows:** `.github/workflows/unity-slice.yml` declara `runs-on: [self-hosted, windows, unity-6000-3-23f1]` y usa `UNITY_EDITOR_PATH`. Unity 6000.3.23f1 en entorno del runner, sujeto a disponibilidad real y a pausa.
- **Blender PC:** evidencia Windows nativo Blender 5.2.2, job `38606958074`. Prueba de 26 operaciones: `38607499677`.
- **Ollama PC:** modelo ya instalado, Ollama 0.40.2; job `38609051105` detectó instalación pero sin listener. Job `38609357863` levantó el servicio efímeramente sin descarga. **No asumir que permanece iniciado.**
- **Qwen+Blender mismo PC:** `38610802289` probó inferencia real y edición de malla heredada. **BlenderMCP real:** `38612645467` probó el addon, con carga temporal; no implica que esté habilitado en cada sesión.
- **Fuente de estas evidencias:** `docs/ELDORIA_STABILIZATION_HANDOFF_20261014.md`; verificar fecha/sha antes de reutilizarlas.
- **GitHub Desktop:** instalado según propietario, pero no constituye interfaz remota directa del chat. Para automatización utilizar GitHub o la ruta Windows verificada.

## Resultados permitidos

- `CONNECTED_NOW`: comprobación actual directa en el contexto correcto.
- `KNOWN_CONFIGURED`: workflow/instalación o éxito histórico, requiere prueba actual para ejecutar.
- `UNAVAILABLE_IN_THIS_SESSION`: acceso directo ausente, después de comprobar alternativas.
- `PAUSED_BY_OWNER`: aún puede inspeccionarse; queda prohibida producción.
- `BLOCKED`: describir error exacto, intento de ruta alternativa y siguiente paso seguro.

`tools/eldoria-agent-preflight.mjs` **no** abre conexiones remotas de ChatGPT, no hace escrituras y no certifica runners a distancia. Su utilidad es evitar deducciones falsas en el entorno local. Ningún cambio en esta carpeta reinicia producción ni modifica el juego.

## Continuidad obligatoria y criterio de cierre (2026-10-15)

- Una comprobación estática, un commit, un archivo preparado o un workflow lanzado son **hitos**, no finalización. El agente continúa con el siguiente paso autorizado en la misma sesión, sin pedir al propietario que repita «continúa».
- Antes de cualquier cierre, registrar de forma concisa: rama y HEAD comprobados, objetivo verificable, último resultado medido, paso pendiente exacto, responsable/recurso, restricciones vigentes y evidencia enlazada. Consultar el handoff existente; no crear un sistema paralelo.
- En sesiones nuevas, recuperar primero el trabajo incompleto de la rama asignada. No repetir tests ya acreditados sobre el mismo SHA ni convertir una pausa artística en prohibición de diagnóstico autorizado.
- Si una operación recibe una denegación explícita de seguridad, **no** repetirla con nombres, rutas, API o herramientas equivalentes para eludir el control. Conservar evidencia, continuar únicamente trabajo independiente permitido y concretar qué autorización o intervención externa falta. No afirmar que el bloqueo ha desaparecido.
- No presentar «trabajo terminado» hasta obtener el resultado de aceptación de la misión. Si hay bloqueo genuino, distinguir claramente avance parcial y validación pendiente. Estas reglas no amplían permisos ni autorizan usar Windows o reanudar arte.
