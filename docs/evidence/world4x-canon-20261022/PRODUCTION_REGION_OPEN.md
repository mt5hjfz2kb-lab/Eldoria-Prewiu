# Región 1 — producción abierta, sin aprobación artística

PR #100; rama feature/world4x-region1-canon-20261022. No merge, publicación ni gasto. Registro vivo comprobado: propietario codex-world-production-20261023, revisión 18, blob caa520666e640a004b92918cd0050deb32307724. Integración de mundo/GameState/LocalGateway transferida expresamente; el resto de Valoria continúa reservado.

Base AI preservada: cb0ef556dcaef508f140f431e450e4b279687bfc. Fuente Blender SHA256 28f5bc9665145244fb3b3ac400983e184ef664afdd6da0b8c04dac6fae5a23fc. AG/O/K/I y ensayos anteriores no se sobrescriben.

## Evidencia nativa

- Run 40032637317, candidato 5de6fbad272ff4b6e00d163745f41603c9ddee4c: 21/21. Artefacto 12535149598, SHA256 ZIP 3898e04a58e923813707d5700404b068e00663a3449178cd283c11ec6285d9c6. Rechazo visual por materiales rectangulares, agua invertida y madera antigua reactivada.
- Run 40033799345: fallo Blender por consultar objetos ya unidos; corregido antes de run 40034054020.
- Run 40034054020: 22/23; contacto de 420 muestras contra triángulos reales, error máximo 0.00002670288 m. Cantera inaccesible.
- Run 40034957672: 23/24; 48 recursos nuevos y encuentros accesibles; cantera canónica aún falla. GLB reducido de aproximadamente 135 a 73 MB por LOD de masas forestales.
- Run 40035939383, candidato 481af2cd3607506305fd8f8c6f965c6a4ebb05b0: 24/24. Artefacto 12537535635, ZIP SHA256 0fec58159de749487d2b680bbd0890761f0efc1cf6ddd7250632064cd0ca6763. CRC comprobado. Acceso también a cantera mediante una aproximación alcanzable dentro del mismo radio de 24 m, sin relajar obstáculos. UV0 del terreno corregido. Unión topográfica aún rechazada visualmente.
- Run 40036885979, candidato 78de13b37d24f7e2da74f2558030002f118aca2e: 24/25. Único fallo PlayMode: contador esperaba 14 capturas aunque se generaron 16. Artefacto 12536624463; ZIP SHA256 4347a06c8c9c1d4d2091c766de22491423edc9374cde93eec4ce63197abb6074, CRC comprobado. La unión dejó de mostrar la zanja rectangular tras usar alturas de vértices de borde medidas. Contacto visual de 300 piezas: min_gap -0.05736113 m, max_gap 0.000004768372 m; desplazamiento de colisionadores cero. Capturas auténticas revisadas: persisten corte en prolongación del río, Brecha radial artificial y caminos angulosos. Inventario físico falló por stderr del arranque ADB; no consta perfilado físico.
- Candidato b28b5d65f26bfe5956ba4148b0d743e95e922163, run 40037717387: fractura geológica irregular, caminos curvos, exclusiones de recursos/ciudades/marchas basadas en los mismos segmentos exportados. Corrige contador y arranque ADB. 24/25: un recurso de madera quedó sin acceso después de introducir la exclusión de Brecha. No se promociona. La siguiente candidata comprueba acceso real antes de aceptar cada nueva aparición. Artefacto 12537578727, ZIP SHA256 6c190ee37e5da12a1b8ddd556fb904fecd1fdf0c8ed0e18d0f17683d5ef10333.

## Integración real implementada

48 identidades adicionales, 24 madera y 24 piedra, mediante LocalGateway/PlayerState existentes. Recolección, cantidades independientes, reaparición, vencimiento offline, persistencia y recarga sin duplicaciones se prueban con el gateway real. Encuentros únicos mantienen su derrota permanente. Proveedor local restringido a world-local; no hay backend multijugador compartido implementado.

Rutas calculadas sobre triángulos reales y persistidas en MarchState.Route, con exclusión de agua/árboles/ocupación; guardados anteriores mantienen compatibilidad. Reservas urbanas se miden sobre terreno y prueban acceso, no son ciudades ficticias. Zoom cercano/regional/panorámico utiliza una sola geografía y encuadra todos sus vértices en ambas orientaciones. Nodos tienen mallas próximas e impostores renderizados de sus modelos, excluidos en panorama lejano. Modelos antiguos se ocultan; esto no equivale todavía a calidad final.

## Objetivo abierto

Pendientes: calidad continental y equivalencia artística con referencias/AG; río, bosque, materiales, Brecha y puentes; representación autorizada de corrupt-scout; commit de binarios Unity y fuentes editables para una descarga ordinaria de la rama; prueba completa de reaparición sobre terreno tras reinicio; recorrido jugador I–II/UI V3 de la candidata final; mediciones de rendimiento y perfilado físico si hay dispositivo. Ningún PASS automático acredita ART_APPROVED, FPS móviles o mundo definitivo. Las fuentes generadas siguen en artefactos nativos recuperables. No confundirlos con binarios integrados en Git.
