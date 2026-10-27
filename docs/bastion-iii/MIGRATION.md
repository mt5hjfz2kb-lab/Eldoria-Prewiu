# Bastión III — integrado, verificado y publicado (27-10-2026)

## Estado canónico de entrega

- **Código:** integrado en `main` mediante [PR #117](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/pull/117), merge `c4a1ad9af70ba6f95ac8a53e2350f9e5bee03b08`, conservando trabajos de Aldric y del mundo.
- **Jugabilidad:** fidelidad al vertical slice (Granero/comida/caza, investigación de 20 min, encuentro con Lyra antes del asalto, reclutamiento tras victoria, manuscrito único, cicatriz de 6,5 s y desaparición definitiva de la fisura privada). Sin cambios mayores a edificios nivel 5.
- **Certificación real:** [Unity native20, run 40578913550](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40578913550), código `a894662ad4661c59ad685c9d65c2723151f07ac9`: EditMode, PlayMode, I–II y III incluidos, rutas, persistencia, animación, cámaras y evolución por niveles **PASS**. Evidencias PNG originales en dicho artefacto y previsualizaciones sin retoques en `docs/bastion-iii/evidence/native20/`.
- **Dirección artística:** corrección de la fisura rechazada (cinta violeta elevada) por grieta geológica oscura, irregular y somera a ras de suelo, con cicatriz que cambia tras victoria y desaparece. Revisión de capturas interna favorable para integrar; **no equivale a `ART_APPROVED` por el propietario**.
- **Android:** [build firmada 1.17/code17, run 40583425073](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40583425073), fuente integrada exacta `74ef803f8be5a3ceb760344baa3d68c3a9537226`; misma firma de familia `f479ca92ccfa256a991936b23d0153ffad247b352a8a5ed569457db8274443d8` y paquete `com.eldoria.valoria.v5test`. Arte, materiales PBR y stocks convertidos a prefabs autosuficientes para que su render no dependa de GLB legados excluidos de Android.
- **Promoción:** [run 40586806946](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/40586806946) **PASS**, bytes del APK público comparados bit a bit con el firmado y canal familiar principal pasado a 1.17. [APK verificada](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/releases/download/eldoria-android-code17/Eldoria-Valoria-code17.apk), 421673060 bytes, SHA256 `40d9ccbc0032a5c25097c4806ce0c0eecc2d3d93995af8909527e780b87f9ae1`. El mecanismo ya existente de actualización reconoce el nuevo manifiesto. Solicitudes de construcción y promoción desactivadas después de finalizar para evitar duplicados.
- **Límite honesto:** no está acreditada la instalación/prueba física en un móvil. No se ha consumido presupuesto adicional ni se han generado transformaciones nuevas de nivel 5.

## Registro histórico previo a la entrega

Los apartados siguientes conservan el diagnóstico y decisiones durante la migración. Los textos anteriores marcados `PENDIENTE` corresponden a etapas previas, ya resueltas por la certificación y la promoción indicadas arriba.

## Contrato

Autoridad: `docs/BASTION_III_FISSURE_CANON_20261027.md`, tabla I–X y comportamiento real `v0220`. La continuidad antigua que situaba Lyra antes de III está superada. Ascenso III: 650 madera/500 piedra, 14 segundos, cuartel y 41 tropas. Granero: 240/180, 9 segundos. Investigación: 1200 segundos persistentes. Contacto no es reclutamiento. Victoria entrega 300 piedra, 80 XP Lyra y manuscrito único, además de recompensas de misión originales; cierre local de 6,5 segundos, nunca respawn de fisura. Brecha principal intacta.

Conservar IDs, esquema de guardado 1 y sesión única. No representar un portal tecnológico ni convertir fisura en recurso renovable. Los cambios grandes de edificios nivel5 están fuera de este encargo. Cada nivel exige diferencia visible moderada y A/B real.

## Implementación y pruebas

Datos aditivos `ChapterThree`; comandos del gateway conservan propietario/revisión/idempotencia. Prueba ejecutable `tools/bastion-iii/DomainProbe.cs` compila las fuentes reales sin Unity; cubre ascenso, 20 minutos, reinicio, reclutamiento tras victoria, recompensa única, caza, límite de niveles y aislamiento de snapshot. No sustituye Unity ni QA visual.

Implementadas conexiones de narrativa, navegación, Arcón desde Héroes, selección Aldric/Lyra/ambos tras victoria, rutas y contador de investigación. PENDIENTE: comparar los candidatos gráficos en cámara real, completar la regresión I–II y las pruebas Unity del candidato exacto. No integrado, no ART_APPROVED, no nueva APK.

## Decisión artística inicial

Revisados los tres catálogos. `Valoria_Granero_BIII_v1` es fuente reutilizable, no aprobación de escena. `Granero_CFSv1` rechazado por calidad procedural; no recuperar. RCF es fuente rica pero el ensayo histórico falló por composición/crop móvil y no fue promovido. Evaluar un único volumen principal de esa familia contra la Valoria actual, evitando importar el conjunto de tres edificios y duplicar superficie. Fuente editable y pruebas históricas disponibles; debe verificarse antes de escoger.

## Auditoría de paridad ejecutable

El runtime `v0220/index.html` tiene una resolución posterior `gather-*` que sustituye la carga parcial antigua: recoge toda la reserva, marca el nodo agotado y programa renovación a 45 s. Se conserva ese comportamiento para comida (1100), y el tiempo `clamp(ceil(reserva/120),7,35)`, dentro de las fases de viaje ya verificadas en Unity. El antiguo `gatherInfo.cap=300` es metadato histórico y no la resolución vigente.

Candidata de arte: granero Tripo existente y accesorios texturizados de producción; lobo Quaternius CC0 con fuente/hash/licencia fijados y animación Idle; fisura geológica de autoría Blender sin créditos. Pendientes capturas nativas y veredicto; ninguna aprobación implícita por generación o compilación.

La compilación local del dominio y binding pasa la prueba III con serialización real JSON y la regresión original de gestión (30 comprobaciones). La extracción de granero preserva geometría/PBR y evita que la exclusión Android de GLB históricos deje un recurso invisible. Pruebas Unity y estética siguen pendientes; los resultados locales no las sustituyen.

## Recuperación del ejecutor

El primer run 40498860557 no ejecutó Unity: checkout recurrió al ZIP REST completo porque Git no estaba en PATH. Se canceló únicamente esa ejecución propia. La ruta real disponible es el Git incluido en GitHub Desktop, acreditado en el log del ejecutor; se reutiliza su descubrimiento existente y se verifica `git --version` antes del checkout disperso acotado. No es un fallo del juego ni una certificación nativa.

La comprobación preparada incluye capturas de arte A/B con cámara fija, UI real vertical/horizontal (Lyra, Arcón, Granero y composición), rutas del terreno, guardado y geometría de Brecha principal conservada. Pendiente ejecutar y juzgar los resultados; mantener PR117 en borrador y no sustituir code16.

## Correcciones agrupadas para el siguiente ensayo

La fisura privada adapta sus vértices a la misma superficie real que usa el mundo, conservando el relieve somero y normales superiores coherentes. El retrato de Aldric solo identifica a Aldric; Lyra y Arcón usan el emblema narrativo existente, sin atribuirles una imagen ajena. La barra de comida sigue su duración real. Se añade el recorrido integrado I–II completo y su fixture de cámara exacto, y se separan las fases nativas para diagnosticar fallos sin confundir importación, tests y revisión visual. Estas correcciones están pendientes de ejecución Unity; no certificadas por los tests locales.
