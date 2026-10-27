# Bastión III — migración canónica en curso

Encargo del propietario 27-10-2026. Base entregada Android 1.16: `ee35c87986370bf4d7e0e62e57897957cd9bce98`. Recursos liberados y reclamados en el mismo workstream integrado; no modifica la entrega code16.

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
