# Eldoria — Contrato económico propuesto I–XXV (2026-11-01)

Estado: **PREPARADO PARA IMPLEMENTACIÓN, NO INTEGRADO, NO CERTIFICADO**. Autoridad: decisiones del propietario en conversación del 1/11/2026. No sobrescribir código, balances ni trabajo activo de PR #136 sin relevo.

## Objetivo de progresión
Jugador activo: Bastión V en 45–60 min, VIII en 8–10 h, X aproximadamente 4–5 días, XII 8–12 días, XV 18–25 días, XVIII 30–40 días, XX 40–50 días, XXIII 55–65 días, XXV 60–90 días (centro ~75). Son **hitos de diseño**, no tiempos simulados ni contenido VI–XXV ya implementado. Bastión X es temprano; evitar un muro artificial en X.

## Temporizadores de ascenso candidatos I–X
I→II 120 s; II→III 240 s; III→IV 420 s; IV→V 600 s; V→VI 1200 s; VI→VII 2400 s; VII→VIII 4500 s; VIII→IX 12600 s; IX→X 28800 s. Revisados y aceptados como referencia de balance, pero no validados en una partida Unity I–X.

## Economía y prioridades
- Madera/aserradero, comida/granero y piedra/cantera producen **también desconectado**, cada edificio de forma independiente y con tasa creciente por nivel. Producción automática ayuda, pero no sustituye recolección.
- Para jugador activo, objetivo inicial de **25–35 % de ingresos de edificios**, resto de recolección, objetivos y combate. Medir por etapa y por tipo de recurso; no confundir ingresos brutos con gasto.
- Construcciones, ascensos y tropas consumen recursos crecientes; mantener tensión económica desde Bastión V y más fuerte en niveles avanzados. T1 accesible; T2+ más costoso; no inventar T3 ni nuevos tipos por balance.
- Premios intermedios y final de capítulo son **pagos únicos**, guardados en ledger de misiones; calibrar como reemplazo o delta de premios existentes, nunca sumar dos veces. Recompensas candidatas B por capítulo (madera,piedra,comida): I(500,400,0), II(550,420,130), III(600,500,200), IV(650,520,300), V(650,550,350), VI(720,620,420), VII(850,750,520). Son totales hipotéticos; no sobrescribir misiones individuales sin reconciliar.
- El diseño inicial de recolección requiere nodos agotables, respawn en otra posición, capacidad de marcha, distancia/tiempo y competición futura; el multijugador aún no está activo. Evitar nodos infinitos y recompensas no autorizadas.
- Aceleradores existentes de 1/5 min siguen siendo objetos reales, no generarlos automáticamente sin ledger.

## Regla obligatoria de producción offline
Persistir por edificio: nivel, último instante contabilizado, acumulado fraccionario en unidades fijas (ej. micro-unidades) y estado de construcción. Integrar tramos [última liquidación, finalización de mejora] a tasa antigua y [finalización, ahora] a tasa nueva; aplicar una única vez por tramo y guardar estado/revisión de manera atómica. Al reabrir, simular lo transcurrido sin depender de que Unity estuviese abierto. Evitar doble abono tras recarga, rollback del reloj, overflow, saltos de hora y duplicación por múltiples llamadas Update. Un límite offline de 24 h es **propuesta pendiente de decisión**: no imponer silenciosamente pérdida de producción. Si hay almacenamiento limitado, que sea visible, por recurso y con reglas claras. No eliminar segundos excedentes sin explicación.

## Brechas confirmadas en main (01/11/2026)
- `SliceContentProfiles.Active` selecciona OWNER_I_II frente a QA_FAST; los perfiles no deben mezclarse. `BastionProgressionCatalog.ForLevel` solo implementa niveles 2–5; VI–XXV requiere diseño/implementación nueva.
- `LocalGateway.AccrueChapterThree` usa `seconds=Math.Min(3600,...)`, suma madera = seconds×SawmillLevel, comida = seconds×GranaryLevel, piedra = 2×seconds×StoneworksLevel, y después **descarta el tiempo excedente**: no satisface producción offline íntegra ni la curva propuesta.
- Costes y tiempos III–V actuales no coinciden con nueva candidata. Guardados schema 1 y `CompletedCommandIds`, `CompletedTaskIds`, `GrantedRewards` deben preservarse.
- PR132 integró III–V con pruebas, pero **no certificó una partida continua fresca I→V**; APK/WebGL publicados entonces eran anteriores a PR132.
- Salón de Héroes en PR136 y otros workstreams activos: no tocar su alcance al implementar economía.

## Implementación segura por etapas
1. Extraer tablas económicas versionadas y configuración por perfil, con valores actuales como compatibilidad por defecto. Separar motor de producción y simulador puro sin Unity para tests.
2. Implementar ledger offline exacto, fracciones, cambios de nivel, guardado idempotente y migración de partidas antiguas; pruebas de 1 s, 8 h, 24 h, 72 h, recarga repetida, relojes hacia atrás, ascenso durante ausencia y overflow.
3. Configurar costes de todos los edificios, tropas y premios con validación de recursos no negativos y gastos exactamente una vez. No inventar contratos VI–XXV.
4. Simulador determinista: jugador activo/moderado/pasivo, misiones y recompensas existentes, nodos finitos con reaparición, tres marchas, capacidades, viajes, combate, pausas y reloj real. Barrido de sensibilidad por etapa. Aceptación solo si hitos y cuota de ingresos cumplen sin softlocks.
5. Implementar en Unity sobre rama y titularidad legítimas, con PlayMode/EditMode, capturas de economía/guardados, regresión I–V y un recorrido fresco I→V. Probar guardado antiguo y publicar solo con autorización independiente.
6. Diseñar contratos VI–XXV (desbloqueos y requisitos narrativos) antes de declarar simulación/implementación completa de esos niveles.

## Regla de estado
`ECONOMY_IMPLEMENTED=false`, `ECONOMY_BALANCE_APPROVED=false` hasta validación técnica y aceptación del propietario. Esta especificación es preparación, no una certificación ni un cambio jugable.
