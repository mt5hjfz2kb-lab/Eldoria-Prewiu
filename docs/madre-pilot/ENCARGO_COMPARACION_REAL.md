# Encargo único para medir MADRE frente al flujo actual de Eldoria

**NO ejecutar todavía**: Windows reservado; este documento sirve para la prueba cuando se libere.

## Material limitado de lectura (desde la rama main, sin modificar)
1. `Unity/Assets/Eldoria/Scripts/Domain/GameState.cs`
2. `Unity/Assets/Eldoria/Tests/EditMode/SliceCoreTests.cs`

Si se necesita otro archivo para confirmar una sospecha, **pedirlo primero**. No leer todo el repositorio ni los directorios de assets; no abrir el proyecto completo en Unity. La copia pequeña se obtiene exclusivamente tras la aprobación del entorno.

## Mensaje exacto para la IA
«Revisa estos dos archivos de Eldoria como auditor de calidad de videojuegos. Tu tarea es identificar, sin tocar archivos, hasta tres riesgos reales del estado del juego relacionados con marchas simultáneas, retorno offline, persistencia o integridad de recursos. Para cada sospecha señala archivo, línea o método concreto, qué hecho del código la justifica y una prueba pequeña que permitiría confirmarla. Distingue fallos demostrados, riesgos plausibles y preguntas que requieren otros archivos. Si no puedes demostrar ningún fallo, dilo; no inventes problemas ni propongas nuevas mecánicas. No ejecutes comandos, no uses red ni edites archivos. Coste extra permitido: 0 €.»

## Protocolo A/B
- **A — proceso actual:** una sola IA revisa los mismos archivos sin MADRE.
- **B — MADRE:** mismo alcance. Primera vuelta con UN agente en modo lectura; segunda vuelta opcional con dos si ambos tienen nivel gratuito demostrado y no hay gasto.
- Un evaluador compara solo defectos confirmados, falsos positivos, tiempo hasta recibir la respuesta y coste real. No afirmar ganancias de productividad basadas en una única tarea o en respuestas más largas.
- No modificar archivos ni ejecutar Unity durante este ensayo. En una fase futura, ya con autorización, pruebas reproducibles podrán confirmar los riesgos detectados.

## Registro de resultados
| Prueba | Tiempo | Hallazgos confirmados | Sospechas no demostradas | Falsos positivos | Consumo/cuota | Coste € | Interferencia Unity |
|---|---|---|---|---|---|---|---|
| A — actual | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente |
| B — MADRE | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente |

## Criterios
**Aprobar** solamente si MADRE mejora la revisión o reduce de forma visible el tiempo de coordinación, con referencias comprobables, sin costes adicionales, sin riesgos para Unity y sin efectos secundarios. Si la mejora es nula, no instalar permanentemente.
