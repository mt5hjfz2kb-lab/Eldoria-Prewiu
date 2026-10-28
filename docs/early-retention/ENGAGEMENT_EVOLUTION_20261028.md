# Eldoria — primera evolución motivadora de retención (28/10/2026)

## Producto decidido por el propietario
**Una fase pequeña, con mucha personalidad**, sin nuevos edificios, monedas, heroínas ni modelos 3D. Deben mantenerse las referencias visuales ya aceptadas en PR120, y el límite global de tres marchas de PR124.

### Camino de los Vigilantes: superación estratégica
| Guardián | Rasgo real | Estadística aumentada | Pista útil |
|---|---|---|---|
| Etapa 5 | Coraza de ceniza | Defensa | Priorizar ataque/ruptura |
| Etapa 10 | Embate furioso | Ataque | Más defensa/vida |
| Etapa 15 | Engendro de la Brecha | Ruptura y algo de defensa | Marcha resistente |
| Etapa 20 | Voluntad de hierro | Vida | Daño sostenido |
| Etapa 25 | Juramento del guardián | Ataque, defensa, vida | Composición equilibrada |

Los valores se suman al escalado existente; **`EarlyRetentionRules.Stage` crea el enemigo real** que recibe `SliceRules.FightCreature`. No se añaden botones de poderes ficticios ni cambios al mundo/PvP. Una pista aparece antes de luchar; el resultado comunica rondas, héroe, número de tropas, poder de la marcha, índice del enemigo y consejo. **Poder** e **índice enemigo** no se declaran unidades equivalentes.

### Mesa de Exploradores: curiosidad y registro
- Cada tipo de contrato conserva tres posibles frases de exploración, coherentes con el lore existente, seleccionadas **determinísticamente** desde el periodo y la ranura del encargo que se guarda. No hay objetos raros prometidos, NPC/biomas ficticios jugables ni recompensas añadidas.
- `ScoutClaim` guarda `LatestDiscoveryTitle`, `LatestDiscoveryText` y aumenta `TotalScoutReports` en el **mismo guardado** que los recursos y la XP. La idempotencia impide repetir cobro, informe o sello.
- El mapa muestra el último relato, una placa con el número real de informes y pequeños sellos en los umbrales 1, 3 y 6. No hay un mapa 3D distinto: son elementos uGUI de baja carga encima de la pintura actual. El camino muestra su contador de etapas y ya ilumina el recorrido auténtico.

## Criterios de aceptación
1. CI compila el mismo `EarlyRetentionRules`, `LocalGateway` y `GameState` del juego, ejecutando los casos nuevos y los anteriores; no se alteran el XP, costes o los botones existentes.
2. Unity EditMode comprueba especialidades, informe basado en el `BattleReportState` auténtico, resultado determinista del encargo, guardado/reload y cobro único.
3. Unity PlayMode en la escena canónica: capturas de camino normal, cinco guardianes, victoria, derrota y mapa de Exploradores antes/después de informes en **horizontal, vertical y móvil**. Comparar con la referencia visual aprobada sin recortar textos ni tapar los medallones/botones.
4. Nada de `ART_APPROVED`, merge o APK antes de verificar las capturas reales y compatibilidad de PR124 y PR122.

## Coordinación
Este trabajo es **PR125 apilada sobre PR124**: ambas tocan código militar que debe seguir siendo único. El runner Windows Unity está reservado a PR122 de Aldric y no puede ser robado. Cuando PR124 cierre, retarget PR125 a `main`; conservar los tests y la composición originales de PR120. Presupuesto externo añadido: 0 €.
