# Cierre artístico Corona: control de evidencias 2026-11-26

No es una aceptación de arte ni un informe de cierre. Las dos ejecuciones que contienen la auditoría final son 45824434222 (UI, SHA 3486754d5f4c28db2ae4ffa2a7def3b0f000fb3c) y 45834241279 (candidata consolidada, SHA ff38763376006ede276c875e498a8557d07509b3).

## Verificación independiente de las evidencias históricas

Se descargaron los ZIP reales 15037184906 (original), 15122623159 (candidata 62a8a6e97eb587721986c27a03d40e78dd951cb9) y 15122797824 (comparativa). El verificador local comprobó los hashes de las 26 capturas y su igualdad píxel a píxel con las correspondientes mitades de los 13 montajes. Resultado 13/13. El XML de EditMode 15122881649 certifica 14/14 pruebas para 3486754d; no certifica calidad artística.

El game-manifest.json de la intervención inicial (15113766981) contiene hashes distintos de las 13 capturas del ZIP histórico descargado; los montajes se parecen visualmente, pero no son la misma evidencia byte a byte. No usar sus notas iniciales como si fueran puntuaciones de la auditoría final A/B. El par verificado sí tiene una referencia original de origen comprobado.

## Revisión visual de 62a8a6e

La revisión manual del A/B confirma el descenso de legibilidad en las métricas de marchas y la reducción del tamaño de los objetos de mochila, pese a una jerarquía tipográfica mejor. La versión consolidada corrige el camino de texto de marchas y habilita una quinta fila de inventario únicamente si cabe. Estas correcciones no se aprueban hasta ver los fotogramas exactos de ff387633.
Las mejoras de la UI anterior fueron principalmente de contraste, escala y consistencia material, no un rediseño radical. HUD: cambio pequeño; nombre: campo integrado; tesoros: cofre y CTA; héroes: protagonista y botones; entrenamiento: slider y franja de costes; exploradores y vigilantes: composición mayormente conservada. No asumir un punto de incremento por estos cambios.

## Criterio de consolidación

Cobertura de la auditoría prevista: 13 pantallas UI + 6 vistas urbanas + 9 vistas nativas del mundo = 28 vistas/roles únicos (no 28 sistemas distintos).
- Marchas y bolsa: exigir puntuación de la candidata consolidada; no trasladar la de 3486754d.
- Otros once roles UI: conservar procedencia de la evaluación anterior. Si sus fotogramas finales no son idénticos, exigir corroboración visual concreta o reevaluación; no dar por hecho que un diff de código pequeño garantiza equivalencia.
- Ciudad y mundo: exigir puntuación de ff387633 y mismo encuadre para las comparativas nativas. Señalar explícitamente las referencias análogas y la ausencia de referente directo cuando proceda.
- Recalcular delta desde before y after. Mantener las notas nulas. No inferir nota a partir de tests, ni aprobar automáticamente.
- Umbral artístico: HUD 8.5, secundarios 8.0 como mínimo; registrar también objetivo particular y ganancia de un punto por separado. Si no se alcanza, mantener abierto el defecto.
- Sin fusión, publicación, modificación de Main ni nuevas llamadas de pago en esta revisión. No probar iPhone físico mediante navegador.

Se ha preparado y probado localmente un verificador de cierre de 28 roles con ocho comprobaciones de contrato sobre puntuaciones sintéticas y los fotogramas históricos reales. No se ha generado una puntuación final simulada.
