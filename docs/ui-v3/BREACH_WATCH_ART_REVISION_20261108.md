# Vigilia de la Brecha — revisión artística de pantalla nativa

**Estado: IMPLEMENTADO EN PR173, NO CERTIFICADO VISUALMENTE EN EL EDITOR CON EL NUEVO ARTE.** No fusionar ni publicar la nueva interfaz hasta superar el gate artístico y de comportamiento.

## Comparación con primera prueba genuina

- Capturas antiguas (base nativa independiente y plana): Unity Windows run **42427015492**, commit **1cea977b8ec67495bbbcbc7b31d38425e352c10c**, ocho PNG reales, artefacto **13596191419**. Resultado **TECH_PASS, ART_REJECTED**: materiales geométricos planos, iconos esquemáticos, sin miniatura real, composición contextual móvil pobre. El éxito de esta versión no es válido para cambios posteriores.
- La UI de producción sigue intacta y funcional; ninguna modificación de gameplay, ciudad, cámara, guardado o publicación en main por el arte nuevo.

## Segunda estrategia, por qué

Los ciclos de paneles procedurales / overlays previos no llegaron a la calidad escogida. Se abandona la solución plana, no se reescribe otra vez el HUD ni se importa una imagen completa como pantalla jugable. Los componentes del **Canvas independiente BreachWatchHud** se mantienen. Se incorporan materiales más trabajados y una miniatura del modelo REAL, y se reproyecta el panel en cada formato.

## Implementado en la rama PR173, pendiente de captura nueva

1. **BreachWatchMaterialLibrary**: textura original de basalto labrado, hierro ennegrecido y cobre envejecido, generado una vez como sprites con nueve cortes. Reserva de seguridad si falta un PNG externo; la prueba nativa no acepta el fallback para las piezas críticas.
2. **Arte independiente CC0**: cuatro sprites extraídos del PSD original de AnnoMotion Dark Fantasy UI/HUD Kit, fuente y licencia incorporadas en `Unity/Assets/Eldoria/BreachWatch/Resources/VigiliaArt`. Fuente https://annomotion.studio/public/packs/dark-fantasy-ui-hud-kit/download; SHA256 del ZIP `f089903c2a515c1737344772c538e2bc2d8a153e219096d9118f74db5bf47d76`. La licencia distribuida afirma CC0 1.0. Medallón, marco de hierro, bandera borgoña y CTA de bronce no proceden de RoyalHUD/Forja.
3. **Símbolos independientes en relieve**: mallas de varias capas, con sombra de grabado y metal, más reconocibles que las primitivas líneas iniciales.
4. **Miniatura real del Bastión**: render 512x256 de las auténticas mallas/materiales de Valoria con cámara temporal independiente; ni plano ilustrado ficticio ni reutilización del preview de RoyalGold. Conserva proporciones al insertarse en marcos de distintos tamaños.
5. **Contexto responsive**: pantalla apaisada pequeña usa una distribución de dos columnas para retrato/textos y reserva una fila física exclusiva para CTA, sin coste ni estado debajo del botón. Se ajustó vertical a panel con ilustración y textos independientes, y anchuras de objetivo por formato.
6. **Interacción**: los botones del nuevo Canvas bloquean los clics dirigidos al mundo a través de un adaptador reversible de puntero; restauración al destruir, sin sustituir la lógica de selección ni de guardado. Estado hover/pressed/disabled real de Unity.
7. **Pruebas de fuente y arte**: workflows Ubuntu `source-syntax` con Roslyn y `approved-art-integrity` con hash de cuatro PNG/importador .meta/licencia; no acreditan render de Unity ni aprobación artística.
8. **Gate nativo ampliado**: exige ocho capturas reales del mismo head, ambas escenas (ciudad/Bastión) en 1600x900,900x1600,844x390,390x844; presencia y sprite del arte nuevo; modelo 3D renderizado; texto legible; nav en una fila y sin solapamientos; coste/estado/CTA sin superposición; pointer raycast y bloqueo al mundo; juego/guardado inalterados. Una prueba no ejecutada no debe figurar como PASS.

## Bloqueo coordinado en el momento de redacción

El recurso `windows-runner-heavy` de Unity Windows está actualmente reclamado en main por el trabajo `eldoria-private-beta-i-v-20261107`. El trabajo BreachWatch **no** puede lanzar Unity, cancelar la beta ni adjudicarse el recurso aunque no haya jobs ejecutándose en el instante consultado. Todos los cambios de esta segunda estrategia se han hecho de forma aislada. 

**Siguiente acción segura:** releer registro de main y GitHub Actions. Tras la liberación formal de Windows, reservarlo exclusivamente en el trabajo canónico `eldoria-forja-oscura-visual-proof-20261107`, lanzar el flujo exact-head modificando solo `tools/breach-watch/native-request.txt`, descargar PNG/report, inspeccionarlos al 100%, corregir cualquier falta de calidad/funcionalidad y repetir. El gate final es decisión visual independiente del TECH_PASS. Android/WebGL no están probados; no fusionar ni publicar.

