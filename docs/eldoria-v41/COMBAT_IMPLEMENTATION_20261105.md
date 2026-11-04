# Combate T1–T5 — implementación autorizada, 5 noviembre 2026

Estado: primera incorporación del motor y pruebas; NO conectado todavía al combate real ni certificado en Unity. No publicado.

El propietario encarga implementar el combate desarrollado después de PR136. Se reutiliza PR134 y su workstream, conservando main 26db55c8 y su cierre posterior en curso. La reconciliación parte del árbol completo de main; el historial anterior de PR134 se conserva como padre. Su antiguo parche de entrenamiento es histórico, no se aplica sobre la interfaz posterior aceptada.

Fuentes: ELDORIA_V41_HANDOFF_LISTO_PARA_UNITY_20261031.zip y ELDORIA_V41_HOSPITAL_CAPACIDAD_Y_EXCEDENTE_20261031.zip. Los originales permanecen intactos. El C# entregado tenía literales numéricos con punto final inválidos y carecía del cierre de un inicializador/return; se reparan en la copia del proyecto. No se aplica el instalador antiguo, que apunta a una rama y UI ya sustituidas.

## Decisión explícita del propietario

El 5 noviembre confirma «Sí, mueren los excedentes»: también en PvE, los graves que no caben en un Hospital operativo mueren por `hospital_overflow`. Los leves no ocupan camas ni mueren por saturación. El combate PvE no causa muertes directas. Sin Hospital realmente operativo no se habilitan pérdidas permanentes. El edificio futuro de recuperación no se implementa.

## Puertas pendientes

1. Compilar el motor recuperado, ejecutar los tests del C# real y comprobar paridad con el laboratorio.
2. Integrar matriz 3 familias × 5 tiers mediante una migración aditiva del ejército existente, sin segunda autoridad; mantener partidas y marchas en curso.
3. Conectar el resolver al gateway único con versión por combate, resultados/empates, informes propios opcionales y recibos idempotentes.
4. Hospital/retorno UTC y ledger de bajas con conservación de tropas; nunca inferir soldados muertos directamente del HP perdido.
5. Validar campaña I–V, marchas, Vigilantes, economía, capturas reales, guardados y builds. No activar coeficientes, capacidad/costes médicos o tasas PvP como aprobados: el paquete los identifica como candidatos pendientes de calibración.

Los parámetros de balance permanecen candidatos. No se publica APK/WebGL, no se gasta ni se introducen nuevos modelos. La PR sigue en borrador hasta superar la validación aplicable; la presencia de código no equivale a integración jugable.

## Primera prueba C# real

Commit 2aec623e40b0ab15130b0c08c08f42455074a842, run41935426546: contrato V4.1 y motor T1–T5 compilados en .NET8; controles del gateway y recorrido continuo I–V (149 comandos,119 recargas) pasan. No equivale a certificado Unity ni a activación del nuevo motor en campaña.

Segunda fase incorpora los quince campos al ArmyRoster existente, preservando ArcherT1/T2/T3, copia, límites, reserva y devolución atómicos. Los nuevos campos no conceden tropas ni desbloquean tiers. El correo conserva una instantánea completa del ejército. Se añaden controles de conservación por familia/tier, rechazo de valores negativos y devolución sin mutaciones parciales ante overflow; prueba Unity de deserialización del formato antiguo.
