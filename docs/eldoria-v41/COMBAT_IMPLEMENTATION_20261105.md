# Combate T1–T5 — implementación autorizada, 5 noviembre 2026

Estado: motor recuperado conectado al gateway con catálogo explícito y marchas versionadas; probado con encuentros controlados en .NET. NO activo por defecto en campaña ni certificado todavía en Unity. No publicado.

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

Los archivos históricos etiquetan estos valores como candidatos; la orden actual es implementar la última versión desarrollada y conservar sus números. La activación en campaña depende de la validación de integración, no de sustituir esa tabla por otra. No se publica APK/WebGL, no se gasta ni se introducen nuevos modelos. La PR sigue en borrador hasta superar la validación aplicable; la presencia de código no equivale a integración jugable.

## Primera prueba C# real

Commit 2aec623e40b0ab15130b0c08c08f42455074a842, run41935426546: contrato V4.1 y motor T1–T5 compilados en .NET8; controles del gateway y recorrido continuo I–V (149 comandos,119 recargas) pasan. No equivale a certificado Unity ni a activación del nuevo motor en campaña.

Segunda fase incorpora los quince campos al ArmyRoster existente, preservando ArcherT1/T2/T3, copia, límites, reserva y devolución atómicos. Los nuevos campos no conceden tropas ni desbloquean tiers. El correo conserva una instantánea completa del ejército. Se añaden controles de conservación por familia/tier, rechazo de valores negativos y devolución sin mutaciones parciales ante overflow; prueba Unity de deserialización del formato antiguo.

## Registro médico transaccional

Se implementa un planificador puro: no escribe guardados ni activa el Hospital. Conserva disponibles/graves/bajas por familia y tier, devuelve un nuevo estado y los soldados que debe abonar conjuntamente el gateway, y guarda recibos permanentes por evento. Ordena regresos por UTC e ID, distribuye camas proporcionalmente, libera tratamientos vencidos antes de admitir nuevas tropas y rechaza reproducir el mismo evento con datos distintos. No introduce porcentajes de heridas, costes o capacidades inventadas: requiere conteos enteros de una política calibrada y un Hospital operativo confirmado. Sin Hospital funcional devuelve íntegramente la marcha.

La integración del gateway debe proporcionar todos los regresos pendientes antes de avanzar el reloj médico; no procesar cada marcha de forma aislada hasta el presente. Ese adaptador y la política de severidad no están activos todavía.

El primer intento Unity 41936114710 fue rechazado por la reserva aún asignada a PR136 ANTES de descargar el proyecto. Ningún test Unity de esta candidata se ejecutó. Los contratos .NET de 99a2883d sí pasaron en41936114719, incluida regresión I–V. Se conserva la protección de Windows y se espera el relevo; no cancelar la certificación de main.

## Recuperación contrastada y puente ejecutado

Ver BALANCE_SOURCE_RECONCILIATION_20261105.md: tabla completa, fuentes supersedidas y 434 paridades Python/C# +39.204 duelos C# reproducidos. El gateway congela ambos ejércitos y héroes al salir; un save en vuelo exige su versión original. El catálogo no cambia el combate en marcha. Prueba con serialización real .NET: envío, regreso offline, correo único, tropas conservadas antes del Hospital, lectura/borrado sin doble premio:12 controles PASS. No hay catálogo ficticio activado para campaña; capítulos III y Vigilantes siguen legados hasta su migración explícita. Se adapta el buzón existente para empate y análisis propio opcional; necesita capturas nativas.

## Validación nativa y compatibilidad — continuación 5 noviembre

PR134 incorporó el árbol preservado y la reserva Windows legítima en 07fc290c. Run41997366472 detectó una aserción NUnit no compatible (`Is.AnyOf`): corregida en419f9722 conservando las mismas tres salidas admisibles. Run41997821266 compiló y superó controles V4.1, pero falló al restaurar marchas antiguas: JsonUtility materializa campos de clase nulos como objetos vacíos. Se normalizan exclusivamente los snapshots completamente vacíos; una versión desconocida o un snapshot parcialmente poblado siguen rechazados. Prueba nativa específica añadida. Ninguna prueba desactivada.

El adaptador de criaturas usa sus cuatro estadísticas vigentes, sin inventar ejércitos, familia, tier o héroe enemigos. Mantiene sin cambios los 15 perfiles militares y el cálculo ejército contra ejército; factor de contrarrestre neutral para criaturas sin familia definida. Expone snapshots persistentes del enemigo real al gateway, todavía por activar en el anfitrión jugable tras la certificación. Prueba de campaña con este adaptador: recorrido I–V149 acciones/119 recargas PASS; capítulos III y Vigilantes aún mantienen su ruta anterior hasta completar su migración.39 controles nuevos verifican perfiles finitos, entrada accesible, jefe25 exige crecimiento, no hay counters ficticios y el snapshot no cambia al editar el catálogo.
