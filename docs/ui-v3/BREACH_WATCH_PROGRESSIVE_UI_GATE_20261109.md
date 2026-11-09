# Vigilia de la Brecha — aparición progresiva de controles
Fecha: 2026-11-09 · PR canónica: #173 · dirección artística: Vigilia.

## Defecto de experiencia demostrado por el código previo
La pantalla dibujaba desde el primer capítulo cinco medallones aunque los destinos de Ejército y Héroes NO tienen pantalla nativa independiente. También mostraba tres botones secundarios (Avisos, Ajustes, Brecha) deshabilitados sin ningún destino real, además de una cápsula de comida con «—» cuando el estado del juego `ShowFood` era falso. Los botones deshabilitados seguían dando la falsa impresión de que había funciones disponibles y ocupaban espacio valioso del mundo.

## Corrección implementada en rama PR173
1. `BreachWatchHud.NavigationMask()` incluye REINO y RECURSOS siempre; EXPLORAR solo si `host.CanOpen(RoyalSection.World)`. Ejército y Héroes conservan sus recursos gráficos para la fase futura, pero no aparecen sin pantalla propia.
2. La fila inferior se centra según los **dos o tres** destinos realmente visibles; la filigrana genera exactamente los conectores que unen esos medallones, con la misma área de toque y un solo sprite artístico por botón.
3. La tercera cápsula de recurso aparece únicamente con `snapshot.ShowFood`, con recolocación de los importes visibles y sin escribir importes ficticios. La comprobación ocurre al refrescar la sesión real, incluso tras desbloqueos sin girar el dispositivo.
4. Los tres sellos secundarios no generan acciones visuales ni capturan toques en esta prueba. Los PNG aprobados siguen guardados como recursos para una pantalla auténtica futura, pero no hay botones que prometan sistemas sin implementar.
5. No cambia el estado de la partida, progreso, selección de edificios, cámara de juego, guardado, paneles históricos ni publicaciones. Es una corrección aislada del Canvas de Vigilia.

## QA ejecutado
- `tools/breach-watch/validate_compact_layout.py` verifica 5 resoluciones × 2 estados de comida × 2 estados de exploración = **20 escenarios** de colocación, clic y solapes estimados en fuente.
- `BreachWatchNativeEvidence.cs` ahora exige que los destinos inexistentes NO estén activos, que EXPLORAR coincida con el desbloqueo real y que los iconos que sí aparecen mantengan un único arte pintado, geometría, visibilidad y toques.
- GitHub Actions [42610137175](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42610137175), commit `d567282d035c280fdb73e39a7a86e00d65035073`: `source-syntax` y `approved-art-integrity` **PASS**, `owner-preflight` PASS, `native` **SKIPPED** (reserva exclusiva de PR181 Tesoros de Valoria).
- Estos son controles de código y de integridad de arte. No certifican ni píxeles actuales ni rendimiento en móvil.

## Pendiente imprescindible
Tras liberación formal de `windows-runner-heavy` y reserva CAS válida para PR173, ejecutar Unity 6000.3.23f1 en el HEAD **actual** (no el SHA antiguo), obtener ocho imágenes de ciudad y Bastión en 1600×900, 900×1600, 844×390 y 390×844, validar clics reales y leer visualmente la nitidez de los nuevos botones, selección, costuras, retrato 3D y texto sobre materiales reales. Ajustar hasta acercarse a la referencia aprobada; aprobación artística independiente de TECH_PASS. No fusionar ni publicar antes del juicio del propietario.
