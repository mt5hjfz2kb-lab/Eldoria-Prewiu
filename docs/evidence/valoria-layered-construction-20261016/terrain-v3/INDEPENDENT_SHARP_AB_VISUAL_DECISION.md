# Valoria V3 · revisión independiente del terreno sobre SHARP real (2026-10-17)

**Autoridad:** rama aislada `construction/valoria-master-plan-layers-20261016`, escena no canónica `Valoria_SHARP_RiverCliffV3_CANDIDATE.unity`. No existe autorización para publicar.

## Trabajo verdadero inspeccionado

- Workflow [39020795868](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39020795868): Blender V3 y Unity native se ejecutaron. La fase nativa compiló/renderizó/validó las 7 selecciones físicas y los 12 peldaños; el trabajo CI concluyó FAILURE exclusivamente al empujar porque el HEAD remoto avanzó. **No equivale a fallo de Unity.** El artefacto `valoria-real-sharp-terrain-v3-native-39020795868` contiene la escena Unity, prefab, seis capturas AFTER, dos BEFORE y logs (ID de artefacto 12097161138), además de la fuente .blend y GLB. El intento duplicado [39020898606](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39020898606) falló en el preflight Blender por manifest mal adaptado y no arrancó Unity. Se ha restaurado el workflow ya certificado con el campo correcto `source_mesh_count`.
- Fuente: V2 editable inmutable `6cf867a57bdb801693a20c03558d4eaffca204d5f46e49046d7cf9edb3b65de0`; V3 nueva fuente Blender `768a08535f066a40c707fd52901a0c16efad91ec06ce91f35ca31cf625b5792f`, GLB `ae1677ee624aa69248b98011fd320c8b7a9d892b4b14c7ed06d36d348c0f54ea`.
- Mallas 9, triángulos 16.780; extremos del puente en x=0,z=-19.65/-9.35 y=0.38 m confirmados geométricamente; collider sin cambios. Unity `terrain-v3-native.json` registra 7 contactos de origen y 12 peldaños intactos, pero no PlayerState ni LocalGateway.

## Inspección de capturas de Unity, antes/después misma cámara real

Captura `terrain-v3-AFTER-home.png` inspeccionada directamente contra `terrain-v3-BEFORE-home.png` y V2.

**LOCAL ART IMPROVEMENT limitado al acantilado, NO FULL ART PASS.** La pared rockosa V3 ya no presenta el serrado diminuto/algorítmico y violento de V2; son estratos irregulares de baja frecuencia con algunos planos amplios de roca. Sigue habiendo líneas horizontales regulares, aristas triangulares planas y encuentro insuficiente con las plataformas superiores. La superficie lateral muy lisa aún se ve como pradera provisional.

**Scene-wide FAILURE:** puente y muralla ocupan la mayoría del encuadre HOME y la parte inferior queda cortada; Bastión superior es un cubo GREYBOX sin arquitectura; en `terrain-v3-AFTER-portrait.png` la puerta y el puente quedan recortados de lado y la ciudad no se lee como composición 3:2/panorámica adaptable. El río y sus dos orillas están en gran parte ocultos tras la puerta; la muestra desde HOME no demuestra una geología costera convincente. No hay evidencia para puntuación 6/10. No se deben añadir vegetación o nuevos edificios para encubrirlo.

**Veredicto:** conservar V3 como candidato de mejora local reversible, con materiales aún en desarrollo; mantener el baseline `Valoria_SHARP_SemanticContact_IsolatedV1.unity` inalterado. Para el siguiente paso priorizar correspondencia de tamaño del acceso, composición global, alturas/colliders y circulación. La referencia de preproducción `docs/evidence/valoria-art-production-reset-v1/unity-blockout-input.json` (85 mallas) define plataforma principal irregular ~65.39 m de anchura, terraza superior ~46.69 m y relación relativa puerta/puente/escalera: sirve como referencia visual y no reemplaza las huellas jugables certificadas.

**Cámara:** componente `ValoriaParcelPresentation` perspectiva SHARP en FOV 44.42281 sin cambio. El rígido de 7 centros aún tiene error máximo 81 px pero se ha demostrado 7/7 contactos de selección en contexto aislado. La transformación rígida existente inclina el terreno: NO aceptar como terreno jugable final sin validar vertical global, superficies transitables y PlayerState.

No avance a la fortificación superior ni a materiales finales. La próxima ejecución debe medir el área en pantalla del conjunto puerta+muralla contra la referencia (LowerGate caja objetivo: 273/1536 del ancho del encuadre aprobado) y diseñar un encaje macro proporcional y físicamente vertical antes de repetir sculpt ciego.
