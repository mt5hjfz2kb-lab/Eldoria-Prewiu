# Procedencia del combate recuperado — 5 noviembre 2026

Se revisaron las ramas remotas (tres páginas), commits de combate/balance/V41, los 18 comentarios de #138 y las rectificaciones de #139. La evolución T1–T5 se entregó en paquetes: el archivo C# antiguo de main/PR136 era 3×3 y no contenía estas revisiones. PR134 sigue siendo la única PR de implementación de este encargo.

## Orden de precedencia comprobado

| Fuente | Estado para esta implementación |
|---|---|
| Original V4.1 (29 octubre), T1–T3 ×1/1,15/1,33 | Conservar histórico y regresiones; no confundir con T1–T5 |
| Bloqueos T1 contra T5 y porcentaje mínimo 60% | Rechazados expresamente; jamás integrar |
| [Balance matemático 18:11 UTC](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/139#issuecomment-6547246809), informe y XLSX | Quince perfiles recuperados; 1,18/0,20 y límite60 sustituidos después |
| [Auditoría crítica](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/139#issuecomment-6547310185) | Detectó ruptura saturada y límites de los encuentros hipotéticos |
| [Auditoría de profundidad posterior](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/138#issuecomment-6548861766) | Mantiene perfiles, restaura counter1,40/táctica0,65, ruptura0,64R/(R+45), límite120 |
| [Handoff posterior verificado](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/138#issuecomment-6549022466) | Fuente del C# evolutivo y oracle Python usados aquí |
| [Política médica final](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/138#issuecomment-6550040823) | Saturación mata también PvE; pre-Hospital sin graves/muertos. Ya estaba confirmado; no volver a preguntar |
| [Exploración y límites médicos](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/138#issuecomment-6550231335) | Sin prioridades manuales de camas ni nueva evolución hospitalaria |

El nombre «INFORME_FINAL_BALANCE_MATEMATICO.md» no determina precedencia. Se leyó su contenido y la hoja de parámetros: corresponden a 18:11 UTC, antes de la auditoría de profundidad. La última especificación conserva la tabla de soldados, corrige contrarrestes y ruptura. La instrucción actual autoriza implementar el combate recuperado; no inventar nuevas cifras por interpretar estos archivos antiguos como autorización para rediseñarlo.

## Perfiles recuperados, ATQ / DEF / VIDA / RUP

| Familia | T1 | T2 | T3 | T4 | T5 |
|---|---|---|---|---|---|
| Paladines |9/16/72/3|13,95/24,80/111,60/4,65|21,60/38,40/172,80/7,20|33,48/59,52/267,84/11,16|51,93/92,32/415,44/17,31|
| Arqueros |13/7/42/7|20,15/10,85/65,10/10,85|31,20/16,80/100,80/16,80|48,36/26,04/156,24/26,04|75,01/40,39/242,34/40,39|
| Brujos |11/8/49/17|17,05/12,40/75,95/26,35|26,40/19,20/117,60/40,80|40,92/29,76/182,28/63,24|63,47/46,16/282,73/98,09|

## Integridad y pruebas ejecutadas ahora

ZIP handoff SHA256 `0ea546a9d2445140ce035f322b607e6bd2138b8f20e83224c73142a16515829a`; sus 35 archivos del manifiesto coinciden. Oracle Python sin modificar: `tools/eldoria-v41/laboratory/v41_evolution_tuned.py`, SHA256 `06eefb586339207b270640686357413bcb57430a3e17e9475faed140edcf187a`.

Se generaron 434 fixtures desde ese Python independiente: 225 cruces familia/tier, 200 ejércitos mixtos con ambos héroes en órdenes distintos, 9 superioridades numéricas T1 contra T5. El C# real compilado reproduce ganador, rondas, vida restante, daño por familia y daño sufrido de los quince grupos; tolerancia relativa/absoluta1e-7. No se generaron expectativas desde el propio C#.

La batería C# también recalcula los 39.204 duelos históricos de la variante elegida: 66 composiciones por tier y cruces adyacentes. Coinciden exactamente máximos de victorias42/43/45/44/44, ausencia de composición invencible y remontadas98/86/75/63. Son nuevos resultados ejecutados, no lectura de CSV antiguo. No se han repetido los 78.408 del A/B completo con la variante descartada.

Reproducción desde raíz: `python tools/eldoria-v41/laboratory/generate_parity.py` (NumPy), luego `dotnet run --project tools/eldoria-v41/EldoriaV41.Contracts.Tests.csproj`. El fixture está versionado para que CI C# no dependa de Python. Las pruebas ejecutadas localmente usan Roslyn/.NET8 sobre los mismos CompileItems; en este contenedor el lanzador SDK falla leyendo información de procesos, por lo que se compila con csc y se ejecuta el DLL directamente. Esto no sustituye Unity.

## Resultado de integración y límites

PR134 integrada en main d19df6fc1d521204926b735a05a06786c80f37a5. El catálogo real vigente,25 Vigilantes, patrullas y Acechador ya están conectados; se preservan progresión y desbloqueos existentes. Unity42044569900 PASS117/117 EditMode,73/73 PlayMode y Windows privado; postverificación de main en curso. Ver COMBAT_IMPLEMENTATION_20261105.md para el cierre vigente.

El piloto de50 enemigos no es el catálogo de producción. No convertir HP perdido en muertos ni activar Hospital/PvP inexistentes. Las tasas médicas5%/25% y costes/duraciones siguen hipótesis explícitas, fuera de la tabla recuperada. Las notas de candidatas anteriores que siguen son históricas.

## Revisión adicional de ramas e issues (5 noviembre)

Inventario remoto revisado de nuevo: 290 ramas en tres páginas. PR134 es la candidata canónica; PR135 está fusionada y PR136 integrada. Issue131 conserva la preparación original y sus referencias; #138 contiene la migración y decisiones posteriores; #139 conserva el laboratorio de Vigilantes. Ninguna se cierra solo por encontrar el mismo tema en otra.

Las ramas `feature/eldoria-retention-first-evolution-20261028` (a5d32b02) y `feature/retention-engagement-evolution-20261028` (7c7f1b8e) tienen historias divergentes. Sus guardianes, maestría, descubrimientos persistentes y controles de capacidad ya están presentes en el dominio vigente; sus consejos automáticos quedaron sustituidos por #138/#139. No fusionar sus árboles completos: restaurarían presentación anterior y consejos rechazados. Se corrige la fuga todavía presente en GuardianBadge, BattleReview y el manuscrito de derrota; los controles se actualizan para exigir resultados propios y ausencia de consejos, conservando la lectura tras recarga y los límites de texto.

Candidata 4a15815f conserva el adaptador de criaturas y la normalización estricta de snapshots vacíos de Unity; certificación nativa42004744269 en ejecución al registrar este punto. No es una certificación final ni activa aún el motor en toda la campaña.
