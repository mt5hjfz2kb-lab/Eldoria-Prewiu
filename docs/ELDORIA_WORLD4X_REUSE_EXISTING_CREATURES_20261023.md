# Eldoria World4X — reutilización de criaturas existentes (nota del propietario, 23-10-2026)

**Información para la producción del sector conectado del PR #100 y las ampliaciones futuras. No es un encargo nuevo ni autoriza alterar el trabajo activo.** El propietario pide que se conserve como contexto accesible para Work sin interrumpir su ejecución.

## Recursos existentes: buscar antes de crear o gastar

**Engendro de la Fisura (spawnling, Bastión II):** la versión 3D mejorada está integrada en la escena jugable de `main` con el encuentro `engendro-valoria`. Near: 12.040 triángulos; far: 2.900. Fuentes editables y GLB finales: `ArtSource/World4X/Spawnling_v1/Finished/`; integración: `Unity/Assets/Eldoria/SpawnlingBastion2/`. Evidencia canónica: `docs/evidence/spawnling-bastion2/ART_AND_INTEGRATION_REVIEW.md`. No inventar un segundo encuentro ni alterar `EngendroDefeated`, progresión, colliders, recompensas o guardados. **No tiene rig ni animaciones esqueléticas completas**; la respiración transform no equivale a animación de combate.

**Jabalí de roca (boar, capítulo III):** existe como fuente editable, no como criatura jugable integrada del vertical slice I–II. Original Tripo, GLB optimizado (49.800 triángulos) y .blend: `ArtSource/World4X/RockBoar_v1/`; QA nativo de importación: `docs/TRIPO_API_JABALI_LECCIONES_20261021.md`. No rig, skinning ni animaciones validadas. **No adelantar su aparición a Bastiones I–II**, ni confundir QA de importación con integración o rendimiento móvil; probar LOD e identidad en cámara 4X cuando corresponda.

Catálogo con rutas, hashes, evidencia, límites y condiciones de uso: `pipeline/tripo-art-brain/catalog.json`. Conservar las fuentes pagadas históricamente; **presupuesto 0 €, ninguna regeneración Tripo ni gastos nuevos sin permiso expreso**.

## Contrato de mundo 4X vivo

El terreno Blender/Unity es **geografía permanente**; los recursos, bestias, enemigos y ciudades son **objetos lógicos/dinámicos**, no decoración soldada a GLB. Las praderas despejadas son espacio funcional para decenas de nodos por tipo, fauna, enemigos, exploración y ciudades de jugadores; no rellenarlas para maquillar capturas. Los elementos consumidos/eliminados pueden reaparecer tras el tiempo correspondiente en **otra posición apta**, con exclusiones por ciudad/obstáculos/otros elementos, bioma y progresión, guardado/persistencia y una autoridad compartida cuando exista multiplayer. Reutilizar primero las reglas/sistemas ya presentes; no implementar duplicados incompatibles ni fingir backend multiplayer inexistente.

Para cualquier prueba de población en la ampliación del PR #100, usar representaciones existentes y autorizadas o simulación de QA **claramente identificada y no publicable**; no colocar jabalí de capítulo III como contenido real en Bastión II ni alterar la escena de otro propietario.

**Prioridad:** continuar sin desviar el encargo geográfico/artístico activo. Esta nota es contexto de reutilización para el momento de poblar el mundo, no una orden de detener la producción ni un gate adicional de aprobación. Mantener PR #100 en borrador, sin merge/publicación.
