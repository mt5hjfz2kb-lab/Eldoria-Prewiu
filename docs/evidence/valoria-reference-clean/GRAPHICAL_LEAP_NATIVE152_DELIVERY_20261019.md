# Eldoria — salto gráfico integrado y paquete nativo WebGL verificado (19 octubre 2026)

## Estado verificable

**Estado:** `NATIVE152_WEBGL_QA_PASS / PRIVATE_SITE_PACKAGE_VERIFIED / ART_REVIEW_REQUIRED / NO_PHYSICAL_DEVICE_QA / NOT_DEPLOYED`.

- Base única de juego: `main`, escena `Assets/Eldoria/ArtTests/ValoriaReferenceClean/Integration/Valoria_City_World_UI.unity`, Unity 6000.3.23f1 / URP 17.3.0.
- WebGL **compilada realmente**: source SHA `f5203b84a77540d6704efc5c2576343c7e88fe5c`, native build run **39301296709**, compiled Unity player size **184,751,921 bytes**. El job nativo terminó `SUCCESS` y guardó `valoria-integrated-web-playtest-39301296709`; el workflow total marcó `FAILURE` únicamente por la posterior prueba browser en `ubuntu-latest`.
- El test Ubuntu/SwiftShader dio `FAIL` cuando no apareció un frame de telemetría posterior a un toque sobre «Continuar» en vertical; **sin errores Unity/runtime ni errores de red informados**. No se reescribió código de juego ni se relajó la prueba.
- Repetición completa, reutilizando los **mismos bytes WebGL** sobre el **PC Windows con GPU/Edge Chromium**: run **39305545377**, trigger SHA `4584ffe8f0432553e53dc0bbea4fc575e65d2f4c`, `COMPLETED / SUCCESS`, no se recompiló Unity.
- Artefacto final listo para publicación privada (ZIP): **12213501867**, nombre `valoria-verified-private-site-windows-39305545377`, SHA-256 del ZIP `da699020ad3cb3699ff08e707b2a5c9ebd01663d7f794e6c8bfe72abed78eaad`, **186,728,893 bytes**. [Descarga desde GitHub](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39305545377/artifacts/12213501867).
- Evidencias de la partida, capturas reales y vídeo: artifact **12212594536**, [GitHub Actions](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39305545377/artifacts/12212594536).

## Qué pasó realmente en la prueba Windows

`playtest_web.cjs` realizó un recorrido con **SystemClock, timeScale 1, controles visibles y gestos auténticos**, sin invocar botones internos ni alterar guardados. Informe `PASS`:

- `fresh_save`, `complete_i_ii`, `real_primary_actions_both_orientations`, `world_four_directions_both_orientations`, `world_zoom`, `world_view_retained`: PASS.
- `selection_framing_before_all_actions`, `selection_framing_both_orientations`: PASS.
- `normal_operation_durations`, `march_phases_and_rewards`, `march_visual_outbound`, `march_visual_returning`: PASS.
- `exact_save_reload`: PASS; SHA-256 antes/después idéntico `335723c08e81e29f872583097bdc5fe45ba2da78e0eb13f36209213642c92282`.
- `normal_system_clock`, `no_runtime_errors`, `no_legacy_touch_shadow_navigation`, `no_failed_build_requests`: PASS.
- 72 fotogramas de vídeo inspeccionables; primera carga local Windows 10.347 s, recarga 7.34 s. **No son mediciones de teléfono real, iOS/Safari ni Site publicado.**
- Nota de alcance: la pulsación de la puerta desde la posición inicial quedó `not attempted` porque su collider estaba fuera de cámara; la prohibición de acceso prematuro se comprobó por el control World y las pruebas Unity previas. No afirmar cobertura de toque físico en puerta.

## Arte efectivamente integrado en la escena, reutilizando materiales y modelos existentes

1. **Ciudad y terreno:** variaciones de materiales, caminos de trabajo más orgánicos, integración ambiental con la orilla; misma geometría/altimetría y sin parcelas rectangulares visibles nuevas.
2. **Mundo exterior:** suelo y accesos de bosque/cantera; agrupaciones con pinos existentes; piedra reaprovechada; tratamiento apagado de enclaves ligados a la Brecha.
3. **Coherencia:** pizarra gris oscuro en el Aserradero y el Bastión I–II, río verde azulado más profundo, banderas reales azules/doradas restauradas mediante material original y sus identificadores exactos.
4. **Preservación:** validaciones nativas comparan cámaras y guardan enlaces jugables, hotspots, parcelas, alturas, narrativa, economía, guardados y progresión I–II.

Capturas autenticas, pares A/B nativos en `docs/evidence/valoria-reference-clean/`:
- `environment-city-wide-leap-before.png` / `environment-city-wide-leap-after.png` (terreno)
- `environment-world-forest-valoria-leap-before.png` / `environment-world-forest-valoria-leap-after.png` (bosque)
- `environment-world-quarry-valoria-polish-before.png` / `environment-world-quarry-valoria-polish-after.png` (cantera)
- `environment-river-bank-palette-before.png` / `environment-river-bank-palette-after.png` (agua)
- `environment-city-wide-roof-before.png` / `environment-city-wide-roof-after.png` (cubierta Bastión)
- `environment-city-wide-banner-before.png` / `environment-city-wide-banner-after.png` (restauración heráldica)

Los informes JSON correspondientes siguen marcando `art_approved:false`. La mejora es **visible pero parcial**; no confundir tests verdes, diferencias de píxeles o ausencia de fallos con certificación artística `8/10`.

## Evaluación artística independiente y pendientes antes de Bastión III

Hay mejora perceptible de coherencia y legibilidad del bosque, orilla, material de pizarra y zonas de trabajo. Sin embargo la vista general sigue presentando **Bastión y edificios de servicio en una gran pradera** más que una capital bajo reconstrucción. La repetición de árboles/rocas y falta de una jerarquía de masas urbanas secundarias/restos habitados son los defectos más importantes; la cantera aún se siente demasiado aislada y queda pendiente afinar conexiones físicas de caminos y cimientos. Se necesita **una intervención compositiva de alto impacto** con activos/licencias ya aprobados, sin nuevas ramas ni inventar cientos de edificios; fotografía A/B en cámara jugador, optimización y prueba física móvil antes de declararlo acabado.

Conservar como autoridad `docs/VALORIA_VISUAL_BIBLE.md`, `docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md` y `main/AGENTS.md`.

## Publicación: BLOQUEADA A PROPÓSITO HASTA TENER CAPACIDAD SITES Y REVISIÓN

- La web privada existente `https://valoria-city-world.kbkvc5c45n.chatgpt.site/` continúa identificada como `native143` en el último registro de publicación. **Este chat no ha modificado ese Site**.
- El artefacto `12213501867` está preparado y aprobado para continuar el proceso de publicación, **NO está ya desplegado**. La sesión actual dispone de GitHub/Windows pero **no expone herramienta para actualizar el Site privado de ChatGPT Work**. La entrega queda en GitHub lista para que Work con acceso a Sites seleccione y despliegue **exactamente ese ZIP**, sin reconstrucción WebGL ni cambio de visibilidad.
- Tras desplegar, comprobar identidad SHA/longitud de archivos empaquetados, lectura del Site bajo autenticación del propietario, carga, interacción y guardado; no afirmar `RELEASE_PUBLISHED` antes de esas pruebas.
- No publicar en GitHub Pages público, no actualizar OTA Android, no enviar créditos/datos privados a terceros ni sustituir `native143` en caso de fallo.

## Historia y trazabilidad

GitHub [compilación WebGL + QA Linux no certificada](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39301296709) · [QA Windows 100% satisfactoria](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39305545377).

**Punto de continuación:** conservar el ZIP exacto verificado, publicar de forma controlada solo desde Work/Sites habilitado, confirmar la misma versión en el navegador privado y emprender a continuación la composición de Valoria sin abrir Bastión III.
