# Corona de la Brecha — estado comprobable y continuidad (20/11/2026)

## Objetivo aprobado por el propietario
Crear una interfaz para el Bastión I modular 2.5D de Valoria con **identidad de fantasía adulta** (obsidiana, amatista, oro envejecido, turquesa para acciones reales), calidad visual comparable a los juegos de estrategia móvil de referencia. No copiar su arte, no sustituir el escenario real con un collage, no modificar mecánicas, ni tocar `main`/beta.

**Rama única de producción:** `experiment/valoria-corona-de-la-brecha-ui-20261120`. Escena genuina Unity: `Unity/Assets/Experimentos/ValoriaModular25D/Scenes/BastionI_Modular25D.unity`. El terreno vacío es expansión de Bastión II–V, **no un defecto por sí mismo**.

## Dos familias, no confundir

1. **Kit actual cargado en el juego:** `Unity/Assets/Experimentos/ValoriaModular25D/Resources/ValoriaModular25D/UI/CoronaBrecha/`. `Modular25DInterface.NeedCorona()` carga **esta** ruta. Su horneador `tools/valoria-corona-brecha/bake.py` procesa 14 PNG ilustrados originales de `tools/valoria-corona-brecha/premium/` (recorte alpha y esmalte de legibilidad) y el icono `icon_scroll` del SVG reproducible. `Image.Type.Simple` en marcos pintados completos evita deformaciones/linealización que producía `Image.Type.Sliced` con transparencia extendida. Todos los botones conservan sus manejadores de código; el `LocalGateway` es canónico.
2. **Kit alternativo A/B, todavía NO activado en la interfaz:** `.../UI/CoronaBrechaPremium/`. Se importaron otros 15 PNG RGBA originales como blobs binarios a GitHub en `4d2cced051a3ab6f84f13b9c3fadf59374d9d2e8`, verificando SHA-1 Git y SHA-256 del manifiesto `docs/valoria-modular-25d/CORONA_BRECHA_PREMIUM_SPRITES_PROVENANCE.json`. Incluye `icon_scroll` ilustrado. **No sustituir automáticamente el kit activo por este sin inspeccionar el resultado visual A/B en Unity.** Los PNG no son capturas del escenario.

## Pruebas realmente superadas
- `https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44805707081`: éxito en la comprobación **estática** de 15 PNG alternativos: hashes, dimensiones, alpha, GUID/metadata, lámina auténtica de recursos y declaración de que **NO** certifica Unity.
- Varias acciones `Corona de la Brecha — reproducible 15-sprite Unity art bake` en Linux pasaron importaciones y generaron los 15 PNG de la ruta actual, con hash/fuentes.
- `https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44802943006`: en revisión del estado durante esta sesión, **Editor Camera.Render** y **NUnit PlayMode con construcción/guardado reales** habían completado con éxito sobre el SHA **70444c18217d3a519f313c840e9fb3b13f2fa033**. **La compilación aislada WebGL continuaba ejecutándose**; no declarar aprobado hasta resultado final. Esta prueba es de un SHA ANTIGUO, anterior a los ajustes premium más recientes.
- `https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44805628165`: solicitud posterior de QA Unity para SHA **6dfafbf74cedcc90684d5b84240736db89c6d0ee** pendiente de runner al verificar. Evitar crear otra ejecución simultánea y respetar `eldoria-modular25d-exclusive-unity-native`.

## No demostrado / no aprobado
- **No hay todavía captura WebGL de la última revisión** que permita aprobar de forma honesta el HUD. Inspeccionar `390x844-real-world-hud.png`, `390x844-real-sawmill-build-panel.png`, `390x844-real-build-command.png` y `844x390-camera-home.png` de los artefactos reales, siempre comprobando su `sourceSha`.
- El kit alternativo `CoronaBrechaPremium` no tiene ejecución Unity; NO se puede decir integrado porque está en `Resources`, necesita que `NeedCorona` lo use en un A/B aislado.
- No hay aceptación artística, prueba Safari en iPhone físico, validación de FPS/GC/memoria/overdraw, ni autorización para mezclar con `main`.

## Próximos trabajos por impacto, sin perder foco
1. Esperar resultado de la CI ya iniciada, descargar evidencia **auténtica** de Unity y navegador y juzgar materiales a tamaño de teléfono, espacio de escena, contraste, pulsación y jerarquía; no usar una simulación como prueba.
2. Resolver defectos **visibles** de interfaz: números truncados, solapes entre identidad y recursos, medallones demasiado pequeños, botón Construir con estiramiento exagerado, zonas de texto tapadas por ornamentos; volver a verificar mediante capturas exact-HEAD. Mantener botones reales: OBRAS/CIUDAD/ESTADO, Bastión/Aserradero y Construir, sin accesos de mecánicas inexistentes.
3. Si el resultado actual no alcanza la calidad buscada, comparar el kit **A/B** `CoronaBrechaPremium` en un cambio aislado de apariencia sobre Unity, no en una maqueta, y aceptar sólo mejoras claras.
4. Con Unity/WebGL funcionales, dar al propietario la captura real, el commit exacto y los artefactos. **Éxito técnico no implica éxito artístico.**

## Regla de aislamiento
El proyecto usa un runner Windows compartido, registro de propietario `pipeline/active-workstreams.json` en `main`, y workstream `eldoria-valoria-modular25d-bastion1-20261119`. No editar `main`, no cancelar otras pruebas activas ni reiniciar el experimento. Usar escritura GitHub CAS para no pisar cambios concurrentes.
