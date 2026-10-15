# Eldoria — Conciliación del artista IA con main (15/10/2026)

**Mandato del propietario:** conciliar las cuatro mejoras de capacidad y comenzar pruebas reales de Blender direct y BlenderMCP, preservando fuente, evaluaciones y controles. **Presupuesto: 0 €**.

## Estado por rama y procedencia, con SHA verificables

- **`main`** es la autoridad normativa del juego, contratos y continuidad; su `AGENTS.md` y `docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md` prevalecen. La carpeta completa `tools/art-direction/full-studio/**` no forma parte de main todavía; por eso **NO se arrastró aquí todo el historial experimental** ni se declaró integración de producción.
- **Rama diagnóstica propietaria:** `diagnostic/valoria-ai-eyes-brain-20261015`, PR [#71](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/71) fusionada por squash a commit **`6e11bf237e6009b7eb8484bcba2f0def35a07b5c`**. Incorpora sin reemplazar el estudio: (1) catálogo de destrezas semánticas ligadas a operaciones reales; (2) juicio visual sobre imágenes auténticas conectado al plan artístico; (3) comparador de transporte exacto direct/MCP; (4) preflight y clasificación separada de errores.
- **Versión histórica del artista unificado:** `art/eldoria-full-blender-artist-studio-20261014` SHA **`65add927cde3722407906808f93f22035c975d4c`**. Fuente, herramientas y aprendizajes de este trabajo siguen conservados. El flujo antiguo `eldoria-isolated-tower-copy-art-calibration.yml` sigue apuntando a ese estudio histórico: **no consume automáticamente el código de #71**. El flujo nuevo experimental sí checkout de SHA #71 en su propio árbol aislado.
- **Fuente de la torre original:** `main` histórico `e5fc3715356378abc9a5d472071499c738f5e8ed`; GLB congelado SHA-256 **`e7592eaaf10824dadf6a272551518456c1dc054fdbd24f769fae90d0b4b76869`**. La comparación tiene prohibido sobrescribirla.

## Pruebas y gates

- **13 pruebas unitarias técnicas y compilación de integración Python: PASS** [#38763593444](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38763593444), en Ubuntu gratuito; no verifica arte.
- **Prueba idéntico plan real en PC Windows:** flujo aislado `.github/workflows/eldoria-artist-mcp-vs-direct-parity-20261015.yml` sobre la rama diagnóstica, ejecución [#38766521237](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38766521237). Checkout del estudio fijado a SHA `6e11bf2`, descarga plan Qwen genuino del run `38610802289`, la misma torre SHA, misma cámara y la instalación MCP con SHA verificado. Realiza dos salidas independientes y compara acciones/geom. No hay nuevas inferencias de modelos de pago ni ninguna acción sobre Unity.

La prueba Windows se registra por separado; no declarar `DIRECT_MCP_PARITY_PASS` sin leer su resultado `transport-parity-result.json`. Si la prueba falla, analizar su etapa real, conservar artefactos y corregir en rama diagnóstica, no rediseñar automáticamente la arquitectura del cerebro.


## Resultados reales recibidos después de iniciar la prueba

La prueba Windows [#38766521237](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38766521237), fuente de diagnóstico `d2e20f33952a1243143faca67a568b3da75e1702`, finalizó **SUCCESS / TECHNICAL_GEOMETRY_PARITY**. Archivo [artifact 11988443079](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38766521237): una fuente GLB inmutable y un plan Qwen previo SHA-256 `3962044c882b23712fbab195e5a2570af59e8e9395cde8293a1631813b7b3d09`, ejecutados cada uno en Blender directo y BlenderMCP instalado, sin tocar original. Ambos aplicaron `sculpt_anchor` y `material`, con efectos idénticos y hash final de GLB **`2c06893fcd1595ab257fcbdd27886b386739135356b422f935baf323e3b1d0aa`**. Tiempo observado: direct 6.329 s; MCP 7.046 s (una repetición; insuficiente para benchmarking de velocidad). Capturas de render de los dos transportes tienen SHA diferentes; los hashes de **geometría GLB** son idénticos. **`art_approved=false`.**

**Primera prueba artística del estudio corregido:** workflow nuevo [#38767006506](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38767006506) sobre SHA `006dc0bd0ce29a20502f5681cb8fd4777e4b2f4c`, con `qwen3-vl:4b-instruct` local, fuente original y sala Blender, una única intervención y crítica con resultado de intención/realidad. **No declarar PASS de este ensayo hasta consultar sus logs y artefactos.**


## Primera prueba de autoría con el nuevo catálogo: evidencia negativa útil

- [#38767006506](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38767006506): **rechazo previo a ejecución**: Qwen4B recibió 14.283 tokens de contexto, pero el job tenía solo 8.192; HTTP 400 de Ollama. Problema de preflight/contexto, no fallo artístico. Se corrigió a 16.384.
- [#38767238724](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38767238724), artefacto 11990237188: Qwen4B generó dos planes con `sculpt.params.delta=[0,0,0]`, que el validador rechazó por `Invalid AI sculpt brush`. Problema de elección/contrato de operaciones, no Blender. Se reforzó el prompt y se limitó a una acción estructural.
- [#38767748809](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38767748809), artefacto 11988324791: **la IA produjo un plan ORIGINAL VÁLIDO y Blender realmente modificó vértices heredados** (`extrude` sobre cara 325, offset [0.5,0,0], triángulos evaluados 5340→5346, fuente original intacta); pero el A/B de cámara 960×720 cambió **3036 de 691200 píxeles (0.43924 %)** frente al umbral de relevancia 0.6 %; `SCREENSPACE_REJECTED`. La comparación fotográfica apenas revela diferencia perceptible y no constituye mejora artística. No se ejecutó aprobación humana/Unity, `ART_APPROVED=false`.

**Diagnóstico nuevo y limitación del puente:** el plan bruto de #38767748809 dice añadir un *buttress* de piedra a la torre, aunque el original real de Valoria es visiblemente una atalaya de madera con tejado y escaleras/guardarraíles. El system prompt general en `tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md` favorece detalles de mampostería y piedra para **otros** assets. El catálogo semántico recuperado por #71 **nombra destrezas arquitectónicas y las asocia a operadores atómicos**, pero NO implementa una herramienta de construcción de contrafuerte/parapeto de alto nivel comparable con la vieja familia V3. Este límite debe resolverse antes de afirmar que se recuperó el taller artístico histórico. No modificar el system prompt compartido del propietario Bastión I sin conciliar ownership.

**Siguiente cambio de hipótesis (no repetir extrusiones invisibles):** (1) antes de generar el plan, el modelo visual debe identificar materiales y tipo de pieza del **original** en imagen real y cotejarlos con el inventario/atlas; si hay ambigüedad, no adivinar piedra; (2) ofrecer al modelo habilidades semánticas de montaje/reutilización espacialmente verificadas (por ejemplo abrazaderas de madera y apoyo de tejado) que produzcan una **estructura visible completa** con parámetros elegidos por Qwen, sin plantillas de torre ni Python arbitrario; (3) inspeccionar real A/B con la misma cámara y solo aceptar si el objeto responde a la intención original, no al porcentaje de píxeles; (4) mantener candidatos en la rama diagnóstica y la pausa de producción.

## Siguiente paso autorizable y límites

1. Aceptar o rechazar paridad de transportes **por fuente, mismo plan y evidencias reales**. Los dos transportes usan el mismo `apply_known_action`, por lo que paridad geométrica no significa creatividad.
2. La línea artística experta integra catálogo y crítico visual sin cambiar el SHA de la torre, sustituir el estudio ni pisar avances de otro agente.
3. Ensayar una sola intervención con Qwen en la torre congelada, A/B de cámara fija y revisor independiente. Los resultados no se consideran `ART_APPROVED` por un SUCCESS técnico.
4. Conservar `main` libre de candidatos artísticos hasta tres mejoras consecutivas independientes, nota >=8/10 y evidencia Unity final bajo autorización específica. Bastión I, mundo 4X y publicaciones **siguen pausados**.

**No confundir conciliación de ramas con fusión de código creativo en main.** La integración normativa y la procedencia ya son accesibles desde main; los cambios ejecutables permanecen en diagnóstico hasta superar el gate correspondiente.
