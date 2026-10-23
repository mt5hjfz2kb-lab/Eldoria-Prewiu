# AI — ampliación norte conectada y capacidad 4X

Workstream: `eldoria-world-region-1-canon-20261022`. PR #100. Presupuesto: 0 €. No merge, publicación ni modificación de la progresión.

## Resultado certificado de AI

**Entrega geográfica y ensayo nativo; objetivo de mundo vivo todavía abierto.** La integración real de población/estado/marchas depende del alcance reservado que se detalla abajo. No se ha publicado el fixture ni fusionado el PR.

Fuente `cb0ef556dcaef508f140f431e450e4b279687bfc`. [Run nativo 40024930128](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40024930128), [artefacto 12532955069](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40024930128/artifacts/12532955069). ZIP SHA-256 `a9f48707e61701f6c8e73f1a5648045c350a01acb80bc7db08f85e394cd0ac16`; CRC completo comprobado. **18/18 tests nativos PASS**. Los cambios posteriores a este SHA solo documentan evidencia y liberan la reserva.

| Comprobación | Resultado |
| --- | --- |
| Terreno nuevo / total | 19.200 / 66.816 m² |
| AG conservado | 25,283 vértices sin desplazamiento; fuente original retenida |
| Geometría añadida | 9,650 vértices; 44 árboles periféricos; cero objetos jugables en Blender |
| Candidatos de muestreo | 369 (NO son plazas simultáneas) |
| Distribución verificada | 17 instancias a escala 1: 7 madera, 5 piedra, 5 engendros de prueba |
| Reserva de reubicación | 5 posiciones de la distribución inicial de 22; no se afirma un máximo óptimo |
| Radios visuales conservadores | madera 6.480 m; piedra 7.000 m; enemigo/LOD 10.639 m |
| Claros de ciudad | 4 × radio 15 m; mínimo medido de interacción + margen 14.642 m |
| Relieve máximo en claros | 0.01056 m en las muestras de huella |
| Corredores | 1478 muestras; 4/4 accesos de ciudad conectados |
| Conectividad con población | 2544/2577 celdas libres alcanzables desde la entrada; todas las ciudades accesibles |
| Reaparición del fixture | 5 reubicaciones, 0 duplicados; JSON/carga/tick repetido estables |
| Aislamiento | snapshot real de jugador idéntico antes/después |
| Contacto visual | error residual máximo 0.00000055 m **respecto a superficie + 0,08 m**, incluyendo LOD cercano/lejano |
| Selección original | 24/24 pruebas; 6 BoxCollider de selección conservados |
| Capturas nuevas | 12 PNG sin retoque: tres escalas × dos orientaciones × geografía/población; 25 fotogramas de zoom continuo |

La reserva de 15 m cubre la huella de interacción actual de ciudad; no aprueba tamaños o reglas futuras de fundación. Las 17 instancias son una prueba representativa limitada, no una certificación de cientos de entidades.

### Alturas: qué queda realmente demostrado

Se midieron **11 anclas** en los cinco marcadores retenidos (incluidas las siete del bosque), además de la geometría animada existente y los modelos de prueba. El despegue visual heredado de **8 cm** permanece explícito; los errores residuales no deben presentarse como contacto físico a cero. Los seis colisionadores son `BoxCollider` con `isTrigger=true`; sus posiciones permanecen idénticas. El delta histórico máximo de **8.247250 m** respecto a `bounds.min.y` sigue en el JSON y no se utiliza como veredicto sobre los pies de las mallas. El campamento visual `corrupt-scout` no queda certificado por este ensayo.

### Coste y zoom

La geografía es la misma en todas las vistas. Los árboles conservan la transición híbrida existente; los enemigos del fixture usan su LOD original según el zoom, con el mismo criterio en horizontal y vertical, y se verifican sobre el suelo en ambos LOD. Las mallas del enemigo se preparan una vez y se comparten entre instancias; se reutilizan materiales/atlas existentes. El nuevo mapa de pigmento es uno de 2048 × 1024.

El recuento conservador de mallas habilitadas del paisaje es **840,746 triángulos** frente a 754.490 en AG (no es una medición de triángulos visibles realmente procesados por la GPU). El enemigo del fixture usa una pose nativa muestreada y congelada, no una animación independiente por instancia. Su radio varía con la pose capturada: el run 40021695802 verificó 15 instancias con radio enemigo 11,151 m; el final verifica 17 con 10,639 m. **No se certifica todavía la envolvente de todos los ataques ni un máximo de población.** La integración autoritativa debe fijar perfiles de huella por contenido y animación, nunca derivarlos de la pose local de cada cliente.

La población añade 60,308 en cerca y 14,608 en lejos. Memoria asignada en Editor: 1,127,606,281 bytes. **No hay FPS, consumo ni memoria medidos en móvil físico.** La película se reproduce a 8 fps por comodidad de revisión; esa velocidad no es un benchmark.

La revisión de PNG auténticos conserva las praderas abiertas, los accesos y la continuidad terrestre. Se corrigieron el tamaño engañoso de marcadores, el modelo incompleto, las huellas supuestas, los accesos bloqueados, la falta de reserva de reubicación, normales de terreno y pigmento de camino reflejado. Sigue siendo un candidato limitado; no se declara paridad artística de toda Región 1.

Datos completos: [capacity.json](living-sector-ai/capacity.json), [integración y coste](living-sector-ai/integration-and-cost.json), [NUnit](living-sector-ai/native-tests.xml), [manifest](living-sector-ai/manifest.json).

## Base y recuperación

La restauración AG se verificó en el **run push nativo 40014366595**, artefacto 12528690185. El run 40014373141 era la comprobación del PR y no ejecutó Unity: no se utiliza como prueba nativa.

Entrada Blender AG: `4b1a71c502302172017867d646f52d7abce0bcc53901264be2461da86e3f4a2d`. `extend_living_sector.py` exige este SHA-256 y conserva exactamente las coordenadas de sus 25.283 vértices. No ejecuta AH ni sustituye los scripts/evidencias de AG, O, K o I.

Se añaden 19.200 m² al norte (192 × 100 m, +40,32 % sobre los 47.616 m² anteriores). La malla está soldada al borde real de AG; mantiene el continente terrestre y sus conexiones futuras. Cuatro claros de radio 15 m quedan reservados para estudiar ciudades; se comparan con la huella de interacción de la ciudad existente, incluidos sus vértices extremos y 0,5 m de margen. Esto no aprueba las reglas futuras de fundación de ciudades. Los 44 árboles periféricos reutilizan las mallas existentes. Las praderas siguen abiertas y no contienen población jugable incrustada.

La fuente `.blend` conserva materiales, textura empaquetada, mallas y curvas de la base; las nuevas rutas/claros están en `LivingSectorLayout.json` y el autor Python editable. Las rutas nuevas se pintan sobre la geografía, no se confunden con objetos jugables.

## Sistemas recuperados y límite de integración

Se revisaron `LocalGateway`, `GameState`/`PlayerState`, `LocalServices`/`FileStateStore`, `ZoneOneGatewayHost`, `ZoneOneWorld` y `world-state-model-v1.json`. El documento de modelo describe `respawnAt?` como futuro: no implementa un sistema operativo.

El gateway real conserva `forest-valoria`, `quarry-valoria`, `corrupt-scout` y `engendro-valoria`; recursos finitos y enemigos de campaña con flags de derrota. Tiene revisión e idempotencia de comandos/tareas, guardado local y recompensas autoritativas locales. No hay catálogo persistente de cientos de instancias ni autoridad compartida de población. Las marchas existentes interpolan sus anclas; no consumen un planificador de rutas sobre este terreno Blender.

Por tanto, no se han convertido encuentros únicos en enemigos infinitos ni se han añadido comandos, recompensas, inventarios o saves paralelos. `RegionOneSectorCapacity` es un ensayo desechable explícito, invocado solo por la captura optativa del piloto. Ninguna población de prueba se instala al iniciar el juego.

La escritura de `GameState.cs`, `LocalGateway.cs`, `SliceContentProfiles.cs`, la integración de Valoria y `World4XZone1/**` seguía reservada por `valoria-master-plan-layered-construction-20261016`, owner `chatgpt-issue95-visual-experience-20261022`, revisión 19. El protocolo vigente prohíbe asumir ese alcance sin relevo. La continuación de producción depende de ese propietario o de un relevo de alcance explícito; los cambios de terreno y ensayos permanecen en el alcance de mundo.

## Qué mide el ensayo

- Rayos contra las mallas realmente importadas en Unity, no contra el antiguo terreno procedural.
- Exclusión de árboles, caminos, ciudades, borde de sector y ocupación; huellas medidas desde los vértices y desplazamientos del shader a escala 1, con separación entre centros igual a la suma de radios. Los árboles usan sus límites reales de Blender.
- Distribución de madera próxima al bosque y piedra en el hombro este/claros como **reglas de ensayo**, no como reglas de bioma aprobadas para producción.
- Duplicados de ID y emplazamiento; agotamiento/derrota simulados; tiempo previo al vencimiento; reubicación; serialización/carga del fixture; segundo tick idéntico. No acredita reinicio de servidor, concurrencia ni migración de partidas reales.
- Comparación de `Gateway.Snapshot()` antes/después para comprobar que el ensayo no altera el estado de juego.
- Corredores con muestreo de un metro y conectividad de una cuadrícula de dos metros sobre la superficie poblada. Es una prueba geométrica; no certifica las marchas del juego.
- Contacto de cada ancla visual de recursos y pies del modelo enemigo completo. Mallas y materiales originales, sin scripts, hotspots ni colisionadores clonados.
- Geografía y población de prueba en tres escalas, horizontal y vertical, mediante `Camera.Render` real en PlayMode, con metadatos de cámara.

## Ensayos rechazados, conservados como evidencia

- 40016413120: fallo Blender por búsqueda de nodo con nombre localizado; corregido buscando su tipo.
- 40016627228 y 40017210037: prueba inicial de 120 marcadores reducidos; suelo oscuro, unión visible y escala engañosa. No acreditan capacidad a escala real.
- 40017663459: 18/18 tests; 293 candidatos, 120 marcadores al 30 %, cuatro claros, 30 reubicaciones sin duplicados. Rechazado como demostración de densidad real; revisión visual detectó además un donante incompleto del engendro.
- 40018607782: 18/18 tests; 23 instancias a escala 1 (8 madera, 8 piedra, 7 enemigo), seis reubicaciones y 1.238 muestras de corredores. La representación del enemigo aún requería recuperar todas sus partes y LOD, por lo que no es la entrega visual final.

- 40019117559: fallo real de conectividad; un claro de ciudad era inaccesible con población. Se añadieron accesos reservados.
- 40019831155: agotamiento de posiciones alternativas al llenar la distribución; se introdujo una reserva de reubicación.
- 40020365092: el enemigo completo excedía el radio supuesto de 7 m. Se sustituyeron las estimaciones por medidas de todas las mallas y LOD.
- 40021028067: madera 6,480 m, piedra 7 m y enemigo 11,040 m de radio; solo 2/3 reubicaciones con prioridad por bioma. Reproduciendo los datos nativos, priorizar el conjunto real de ubicaciones válidas por huella permite 3/3 sin duplicados. La corrección pasó después en las ejecuciones nativas 40021695802 y en la certificación final.

Estos resultados no se borran ni se presentan como aprobaciones artísticas.

## Pendientes reales de producción

1. Integrar entidades dinámicas en el estado y gateway existentes bajo su propietario: IDs de instancia separados de los IDs de contenido, revisión/ocupación atómica, reloj y `respawnAt` persistidos, semántica de reintento/reinicio y migración de saves. Mantener intocables los encuentros únicos de campaña salvo diseño aprobado.
2. Consumir la geografía auténtica desde las marchas y el emplazamiento de ciudades. Conectar exclusiones y reservas a la ocupación viva común.
3. Probar autoridad compartida y concurrencia de dos jugadores. Un JSON local del ensayo no demuestra multijugador.
4. Certificar rendimiento y memoria en dispositivos físicos. Los recuentos de triángulos y PlayMode no son FPS móviles.
5. Integrar en la presentación de producción el ajuste medido contra la malla Blender. El piloto verifica ahora todas las anclas de sus cinco marcadores originales y el modelo animado; no cambia su gateway. Los seis deltas antiguos respecto a `Collider.bounds.min.y` se conservan, incluido el máximo de 8,247 m, junto con `isTrigger` y el tipo real de colisionador. Las anclas lógicas no se han movido; los triggers de selección no equivalen al pie de una malla. El campamento original de `corrupt-scout` no queda certificado visualmente por esta prueba.

No se declara completado el mundo 4X vivo ni aprobada la Región 1 completa.

## Reproducción y recuperación

1. Conservar el commit de fuente certificado y el Blender AG de entrada con el SHA anterior. El paquete de entrega incluye ese archivo original, además del resultado editable.
2. Ejecutar Blender instalado: `blender --background --python-exit-code 1 --python docs/evidence/world4x-canon-20261022/authoring/extend_living_sector.py -- OUTPUT AG.blend`.
3. Copiar `OUTPUT/RiberaPilot.glb` y `OUTPUT/LivingSectorLayout.json` a `Unity/Assets/Eldoria/World4XCanon/Resources/World4X/Pilot/`. El importador GLB existente importa la malla.
4. Establecer `ELDORIA_RIBERA_PILOT=1`, ejecutar `Eldoria.World4XCanon.Editor.RegionOneCanonicalSourceGate.Validate` y la suite indicada en `world4x-canon-native.yml`. Reservar antes el runner y comprobar la revisión viva; no lanzar Unity o Blender concurrentemente.
5. Exigir XML sin casos fallidos, `capacity.json`, doce PNG del ensayo y metadatos de cámara. El workflow comprueba explícitamente la navegación geométrica, escala 1, ausencia de duplicados y preservación del snapshot.

El workflow automático conserva el artefacto AG fijado; cuando caduque en Actions, se puede recuperar el mismo archivo del paquete de entrega y ejecutar el procedimiento local. El hash de entrada evita aplicar accidentalmente AH u otra base.
