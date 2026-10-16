# VALORIA — PAUSA EXPRESA DEL PROPIETARIO (16-10-2026)

## Orden vigente: NO continuar dos trabajos de producción
El propietario ha ordenado pausar de forma temporal y preservar, sin alterar el trabajo, exactamente:

1. Bastión I modular jugable, rama `integration/valoria-bastion1-player-scene-20261012`, HEAD congelado anterior a orden `2b40c5c35fb9922a157666bdf3fb0af7b450b06e`.
2. Mejora individual de los 63 assets, rama `art/valoria-63-individual-quality-uplift-20261015`, HEAD congelado anterior a orden `3b1a1f918d38d81d222a3b35f2567b5fac57e7b5`.

El código, modelos `.blend`, GLB, renders, artefactos y evidencias siguen válidos, NO se borran. La pausa prevalece sobre instrucciones antiguas de "no detenerse", "seguir siempre" o gates de producción de estas dos ramas. No arrancar nuevas iteraciones artísticas, no nuevos despachos Actions, no lanzar Qwen/Blender/Unity en esos workstreams, no gestionar promociones ni actualizar la escena original. No modificar `main` para promover arte.

### Estado operativo del momento de la orden
- Se observó `38943004088` (Bastión I Unity) **in_progress**, y `38943488120` (63 assets, cobbles V11) **queued**. Son ejecuciones anteriores a la orden, cuyo estado puede cambiar; comprobar en Actions antes de usar el runner.
- El conector GitHub actual NO proporciona acción para cancelar jobs en marcha. Este archivo/registro NO cancela jobs ni envía mensajes a chats en curso. En caso de seguir activos deben cancelarse mediante interfaz GitHub Actions por quien tenga permiso o esperar finalización sin volver a despachar trabajo.
- Los workstreams se trasladaron del array `active` a `history` con estado `handoff` (pausa del propietario) para liberar el *claim* lógico. El runner físico debe verificarse realmente libre antes de trabajos nuevos.
- Reinicio SOLO ante nueva instrucción expresa del propietario, revisión del HEAD vivo, y claim libre actualizado.

## Excepción autorizada, NO reanudación
Se permite preparar en rama NUEVA aislada y directorio NUEVO **una única prueba artística comparativa de Bastión I**, en el mismo proyecto Unity 6000.3.23f1 URP 17.3, con coste 0, sin alterar ninguna de las dos ramas pausadas. La nueva línea debe reivindicar su scope con la disciplina de `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md` y comprobar que ningún job ocupa el Windows runner. El resultado exigido son dos composiciones comparables: 3D híbrido real (hero conjunto integrado + módulos seleccionados + entorno curado) frente a 3D funcional con 2.5D limitado a fondo/atmósfera visual. Se conservan las mecánicas de prueba, raycasts, rutas, navegación, selección y expansión necesarios. No se acepta una simple imagen de la referencia incrustada en pantalla como juego. La rama del experimento NO reemplaza producciones existentes hasta demostrar A/B nativo Unity y gates mobile.

### Por qué esta pausa
El fallo dominante documentado es el salto entre calidad individual de una pieza y calidad de escena jugable. Varios `TECH_SUCCESS` y algunos módulos de 6–7/10 no han producido aún la imagen de la referencia. La escena vieja puede acumular overlays, incompatibilidades y composición plana. Las siguientes iteraciones deben centrar cámara, masas, silhouette, terreno, materiales/luces, vegetación y ambiente, no microassets ni calibraciones genéricas. Preservar el viejo Bastión y la biblioteca como *baseline*; no reiniciar el juego completo.

Referencias de fuente:
- `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` SHA-256 `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`.
- `docs/VALORIA_ART_PRODUCTION_RESET_V1.md` (composición antes de crear familias).
- `docs/ELDORIA_PRODUCTION_ART_SOURCE_PIPELINE.md` (greybox vs arte de producción).
- `docs/UNITY_ART_FINISHING_PROFESSIONAL_PLAYBOOK.md` (A/B Unity y QA móvil).
- `docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md`.
- `pipeline/active-workstreams.json` (estado vigente y exclu­siones).
