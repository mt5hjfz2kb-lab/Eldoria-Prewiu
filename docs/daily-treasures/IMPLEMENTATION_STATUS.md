# Tesoros de Valoria · estado real de producción · PR #181

Fecha: 8 de noviembre de 2026. Workstream: `eldoria-daily-treasures-20261108`. No modificar la versión congelada de testers ni fusionar hasta superar QA nativa.

## Sistemas implementados, además de las recompensas diarias
1. **Tributo de siete días:** recompensas por días UTC distintos; el calendario avanza después de cada reclamación, incluso si faltan días. Bonus modestos por racha 3/7 que no bloquean el progreso. Ciclo de siete, continuidad del contador después del séptimo, lectura de fecha de la misma fuente que usa el gateway. No se ha adoptado reloj fiable de servidor, porque el prototipo es offline.
2. **Entrega única + buzón:** `LocalGateway` acredita madera, piedra, comida y aceleradores antes de activar efectos. Guarda el comando y un recibo informativo en la misma persistencia. Reintentos, lectura, borrado de mensajes y saltar animaciones no pagan dos veces.
3. **Ceremonias de recompensas importantes:** premios habituales con feedback rápido; cofre especial el día 4; apertura legendaria reforzada para el día 7, héroes y capítulos importantes. La distinción es exclusivamente visual, no cambia porcentajes de pago. Línea de tiempo 2,35 segundos y cierre voluntario.
4. **Eventos genuinos del recorrido I–V:** gran presentación al superar un capítulo de Vigilantes; primer descubrimiento de una familia de exploradores; obtención real de Lyra; Bastiones II/V verdaderamente terminados. Los últimos tres casos se basan en cambios verificables del estado de jugador y eventos de su buzón. Nunca inventamos un botín para justificar una ceremonia.
5. **Cola de ceremonias no intrusiva:** un evento obtenido en sesión queda pendiente hasta que desaparezcan el tutorial, las ventanas o la selección de un edificio. En una nueva sesión no reproduce automáticamente un hito que finalizó estando ausente; ese resultado se consulta por correo. La cola de pantalla no concede ni persiste ningún premio adicional.
6. **Arte propio ya versionado:** nueve PNG reales en la rama (cofre cerrado/abierto, tres marcos nine-slice de hierro y bronce, cuatro iconos ilustrados). Sprites, transparencias, GUID e integridad SHA256 comprobables en `source-manifest.json`. Cofre geométrico conservado como respaldo técnico; no aprueba el nivel artístico.
7. **Integración de interfaz:** adaptación reversible al Canvas independiente de Vigilia de la Brecha (PR173), porque Vigilia apaga el Canvas legacy; la interfaz se reubica en su `Safe composition` sin Canvas/EventSystem duplicado, con restauración si ese Canvas se deshabilita. Todavía no comprobado conjuntamente en Unity.

## Pruebas realizadas con el código canónico
- `DailyTreasureRules`, rachas y temporización visual: **más de 6.600 aserciones deterministas**, compilando C# original del repositorio (conteo exacto en el último job CI por SHA).
- `LocalGateway` real: más de **180 aserciones**, incluidos siete días consecutivos, saldo/recibos, reintentos, reposición tras recarga, anuncios narrativos y cola de eventos reales.
- Auditoría del perfil **OWNER_I_II**, aparte de QA_FAST: 7 días = 330 madera, 300 piedra, 230 comida y 8 minutos de aceleradores; 30 días consecutivos = 1.380 madera, 1.240 piedra, 945 comida y 32 minutos. Bastiones II–V suman 3.050 madera, 2.400 piedra y 800 comida. **Son candidatos sujetos a pruebas humanas, no valores económicos aprobados.**
- Ocho geometrías aritméticas de composición y objetivos táctiles, 9 texturas importadas con SHA, GUID y bordes válidos; análisis de sintaxis Roslyn y contrato de integración C#.
- Laboratorio visual de navegador con el cofre original en varias orientaciones. Es una comprobación independiente del acabado de referencia, **no es la interfaz final renderizada por Unity**.
- Las pruebas deben repetirse para cada SHA final; un PASS histórico no certifica nuevas modificaciones.

## Verificación nativa preparada y pendiente
El workflow `.github/workflows/daily-treasures-native.yml` **solo puede utilizar Windows cuando el registro canónico conceda la reserva exclusiva a PR181**. A fecha de corte, la beta PR179 mantiene esa reserva.

- Requerir fuente original de los nueve PNG en el commit exacto, compilar con Unity 6000.3.23f1.
- **24 imágenes nativas:** por cada una de cuatro resoluciones, calendario/cofre cerrado/medio abrir/abierto del día 7 + calendario y apertura especial del día 4.
- **40 interacciones reales por puntero** en total: reclamar, saltar, cerrar y entrar; incluye simulación de la ubicación en el Canvas independiente de Vigilia y restauración al legado.
- **8 reclamaciones verdaderas** con `LocalGateway` descartable y comprobación de recursos/buzón, sin tocar partidas reales.
- Hash, tamaño, fuente, dimensiones y ausencia de duplicación de todos los PNG capturados.
- Después de ese test de componente: revisión dentro de la ciudad completa con tutorial real y etapas hasta Bastión V, progresión offline, entorno móvil WebGL/Android/iOS, sonido/memoria/FPS y aprobación estética explícita. Una prueba de componente no se confunde con una partida real.

**Resultado actual:** lógica, presentación y nueve recursos versionados; pruebas de fuente repetibles. La PR #181 sigue en borrador, **sin prueba Unity del arte final, sin integración jugable certificada, sin fusión ni publicación**. Referencia y condiciones: `ART_AND_NATIVE_ACCEPTANCE_20261108.md`.
