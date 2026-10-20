# ELDORIA — Tripo API | Norma de producción de assets v1

Fecha: 2026-10-21. **Complementa** `docs/TRIPO_MODULE_PIPELINE.md`, no lo sustituye ni crea otro pipeline.

## Entradas y elección de tecnología

1. Leer `AGENTS.md`, estado `main`, registro de workstreams, lore y cámaras reales antes de generar.
2. Confirmar que realmente falta geometría; reutilizar/corregir recursos existentes primero. Tripo no resuelve por sí solo iluminación, terreno, composición ni gameplay.
3. Definir una pieza acotada con función, volumen, escala, silueta, materiales, paleta, ángulo de revisión, rasgos a evitar y definición de rechazo. Un recurso en pantalla = un brief, nunca «ciudad completa».
4. Seleccionar modelo explícito. H3.1 texto con textura estándar reprodujo 20 créditos para el Jabalí; no extrapolar coste a otros modos/funciones sin nueva consulta. P2/SmartMesh/rigging son alternativas **no certificadas por esta prueba**.
5. **Un único encargo pagado por pieza, sin retry automático.** Se autoriza como máximo la tarifa expresamente aprobada para esa pieza; mirar saldo real con CLI y ajustar el presupuesto al precio efectivo. La validación del gasto consumido *después* de generar NO es un límite preventivo. Mantener petición desactivada en repositorio inmediatamente después del envío y antes de reintentar cualquier pipeline. Si falla un paso posterior, recuperar la task/artifact ya pagados; nunca pagar otra vez.
6. Mantener aislada la ejecución mientras el runner Unity esté reclamado por Valoria: sin cancelar otras jobs, sin limpiar worktrees ajenos, sin publicación ni cambios de escena/guardado.

## Artefactos y trazabilidad obligatorios

- Prompt exacto (y referencia si la hubiera), modelo/versión, opciones, task id y saldo/coste verificado.
- GLB **raw** y SHA256 antes de cualquier edición, texto del resultado CLI y reporte de Blender.
- GLB optimizado, `.blend` editable, UV/normal/texturas PBR, triángulos reales (sin asumir target), límites métricos/pivote/ejes.
- Capturas de Unity reales: frontal, lateral, oblicua, distancia estratégica; resultado técnico y **veredicto artístico separado**.
- Conservar la fuente bajo `ArtSource/World4X/<identidad>/`, **fuera de `Unity/Assets`** hasta aprobar integración.

## Gate Blender → Unity

- Preferir el procesador canónico `tools/tripo_module_blender.py`; no aplicar automáticamente el perfil antiguo de ~49.800 triángulos a toda pieza. Usar el peso del jabalí como referencia comparativa, no como requisito universal.
- Proyecto de ensayo con Unity **6000.3.23f1** y glTFast **6.14.1** y los módulos `imageconversion`, `unitywebrequest`, `unitywebrequesttexture`. Asignar BaseColor sin doble tinte; verificar color.
- Exigir tamaño verdadero de texturas importadas (no contar mapas 4×4 como éxito), UV0, normales, raycast positivo y fallo en espacio vacío, capturas de verdad.
- Si el QA Unity completo tarda demasiado, separar un **QA mínimo** reutilizando GLB ya generado; evitar cargar cada vez el juego completo. **No corregir automáticamente el modelo como respuesta a una falla de Unity sin diagnosticarla.**
- Evaluar: anatomía, silueta desde mapa, materiales sin zonas fundidas, apoyo en suelo, repetición y coste móvil, capacidad real de animación cuando aplique.
- Resultado final explícito: `SOURCE_PASS/FAIL`, `UNITY_TECH_PASS/FAIL`, `ART_REVIEW_APPROVED/REJECTED/PENDING`, `MOBILE_PROFILE_PENDING/PASS`, `RUNTIME_INTEGRATION_PENDING/PASS`. No declarar «terminado en juego» por un source PASS.

## Lección del primer experimento

El Jabalí de roca requirió 20 créditos, una generación; 84.178 triángulos de salida, 49.800 tras Blender, BaseColor 2K en Unity **solo después** de arreglar dependencias del proyecto mínimo. Con el primero aprendimos el transporte de materiales y la necesidad de separar la preparación del motor de la generación. El resultado es prometedor para arte de fauna, pero **un solo candidato no prueba todavía calidad repetible** ni ahorro horario cuantificado. [Postmortem verificable](TRIPO_API_JABALI_LECCIONES_20261021.md).

## Ensayo solicitado a continuación

**Pieza A — `spawnling`**, Engendro de la Fisura (Bastión II): criatura violeta encorvada, humanoide-animal; primero art source, no modificar su combate/capítulo.

**Pieza B — foco de corrupción aislado:** prop experimental asociado visualmente a la Brecha, **NO nodo/instalación jugable nueva ni «campamento»**. Validar capacidad de Tripo para prop orgánico-mineral independiente. No insertar en I–II ni alterar historia sin revisión.

[Brief exacto de las dos piezas](TRIPO_API_ENGENDRO_FOCO_BRIEFS_20261021.md). Solo solicitar **una generación estándar H3.1 por pieza** tras cuenta/saldo/estimación de 20 créditos cada una y autorización concreta. No usar suscripciones Studio; esto es Tripo **API**.
