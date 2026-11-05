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

## Conexión del anfitrión y regresiones adicionales

La siguiente candidata conecta explícitamente Campaign() al anfitrión real ValoriaReferenceCleanGameplay, incluida su recarga de prueba: criaturas del mundo, Corrupto y Engendro usan snapshots T1–T5. Esto no sustituye todavía los encuentros especiales de capítulo III ni Vigilantes. El gateway depende del contrato IEldoriaCampaignBattleResolver en lugar de la clase concreta, conservando compilación de los proyectos de regresión con selección explícita de fuentes. Se detectó y corrigió ese acoplamiento al ejecutar el smoke real de retención, ahora PASS.

Se habilita la compilación Windows privada para PR134 en el mismo flujo de certificación; no publica APK/WebGL. La rama de implementación no equivale a main: falta el resultado nativo exacto del cambio del anfitrión y completar las rutas pendientes. La campaña I–V con el adaptador sigue pasando149 comandos/119 recargas.

## Vigilantes y patrullas de Exploradores

La siguiente candidata usa el mismo resolver de criaturas para Vigilantes y la patrulla real de Exploradores. Conserva guardianes, recompensas, XP, capacidad, estado y recibos existentes. La maestría aplica a los perfiles nuevos la misma ganancia proporcional ATQ/DEF/VIDA/RUP que producían los incrementos existentes +4/+3/+15/+2 por nivel sobre la expedición anterior (cap20); no cambia los quince perfiles base ni el combate entre ejércitos. Esta adaptación de progresión requiere validación de campaña, no se declara balance aprobado. El informe usa vida inicial efectiva para no exagerar desgaste cuando existe maestría.

Prueba C# real:25 guardianes consecutivos con ejército desarrollado, recarga y repetición idempotente de cada comando, informe propio evolutivo y conservación de200 soldados;139 controles de gateway PASS. Se añade la prueba nativa equivalente de persistencia/recompensa. Los encuentros narrativos y emboscada del capítulo III siguen pendientes de reconciliar: no sustituir sus reglas por datos inventados.

La emboscada de Acechador de Ceniza también usa snapshot del nuevo resolver: conserva perfil920/132/68/54 y coeficientes existentes de apertura1,42, defensa posterior0,82 y respuesta1,08. No cambia botín, respawn ni misión. Las marchas antiguas sin versión conservan su resolución original. Fisura y cacerías narrativas mantienen sus condiciones autorales de contacto y poder: no poseen un ejército ni perfil enemigo autorizado que permita reemplazarlas por una batalla inventada. La campaña continua ampliada vuelve a superar149 comandos/119 recargas con esta emboscada conectada.

Certificación nativa42004744269 del commit4a15815f ha terminado SUCCESS: compilación exacta, controles/buzón, tres marchas, Aldric, escena integrada, EditMode y PlayMode. Corresponde a la corrección de guardados y adaptador aislado; no certifica las conexiones posteriores. La candidata completa requiere ejecutar también el control nativo de Vigilantes, diálogos y Windows privado, habilitados en su workflow.

Corrección adicional de continuidad: Vigilantes no puede reutilizar un héroe que ya está en marcha; el botón refleja la misma disponibilidad del gateway.140 controles de gateway y62 del adaptador pasan. La CI ejecuta ahora ambos recorridos I–V: legado y resolver nuevo, sin retirar ninguna prueba.

Evidencia descargada de4a15815f: artifact13405385862, SHA2560a482b15505878e84daeb842c1bebe967f0e54c37123ea3f2e4841cb01d927db; XML112/112 EditMode y73/73 PlayMode, cero omitidos/fallidos. La conexión0fde detectó en native42006777932 una aserción que esperaba «ARQUEROS T1:60»; captura real390×844 muestra correctamente120 tropas, Arqueros T1×60/T2×60. La corrección15130008 valida todas las familias/tiers contra el ejército enviado y ejercita el análisis opcional en cuatro formatos. No se elimina ninguna prueba de conservación.

Los paquetes Windows solo se suben cuando la compilación de esa ejecución termina correctamente; se evita subir un build antiguo del disco tras un fallo previo. Los logs de compilación siguen en la evidencia de diagnóstico. Los empates se distinguen también en el aviso de Vigilantes, sin conceder victoria ni recompensas.

## Revisión visual y de rendimiento de la conexión completa

Run42009537120, source2d120702511c0641d8c780698d6b51bd23d9ba8f: controles reales V4.1, diálogos, tres marchas, Vigilantes/descubrimientos y comparación de rendimiento pasan. Certificación completa todavía en curso al escribir este apunte. Benchmark de Unity Editor, mismo código/escena/recursos y resolver legado frente al nuevo: p95 de resolución 0,0004 ms frente a0,1107 ms; memoria retenida tras GC0 frente a12288 bytes. Ninguna regresión significativa según los umbrales; NO prueba física móvil. Artifact13406013756 SHA256a2448d4a08d2b139bc182cf2814f875b8e4b8f6cefe24813cc19e26820dfd061.

Capturas reales de artifact13407025597 SHA2569d2236e65006253b1b0286befebece23844ff41dd3b35a874f1a0f99c406350c: el análisis propio conserva cifras, sin consejos tácticos, pero la tarjeta de héroes podía recortar el segundo nombre en900x1600. Se corrige midiendo las alturas reales de encabezados y valores antes de disponer las tarjetas. Se añade gate nativo de texto completo y capturas con desplazamiento real al final del análisis en los cuatro formatos. La corrección requiere una nueva certificación exacta; no se presenta como aprobada antes de sus capturas.

Revisión posterior de descubrimiento: las capturas del análisis mostraban nombres de familias no enviadas con valor cero. Se filtran las tres métricas por composición propia real; una familia presente sigue apareciendo incluso cuando causa cero daño, y los informes mixtos conservan todas sus familias. No se cambian ecuaciones, atributos ni resultados. Nuevos controles del gateway, NUnit y la escena real exigen que una marcha solo de Arqueros no anuncie Paladines/Brujos.141controles de gateway y62del adaptador PASS; la batería completa de paridad/profundidad/médica sigue pasando. Requiere nuevas capturas y certificado exacto antes de integrar.
