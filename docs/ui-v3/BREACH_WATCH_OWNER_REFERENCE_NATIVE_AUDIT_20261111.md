# PR173 — Auditoría visual de referencia aprobada frente a Unity real (11/11/2026)

**Estado:** dirección artística en revisión; **no** aceptación 8/10, merge ni publicación. Este informe compara la imagen realmente aprobada por el propietario con capturas nativas, no solo una captura con otra versión interna.

## Procedencia reproducible

- Imagen aprobada: `75B5D217-A914-4451-854D-AE54849E4A50(3).jpeg`, **864×1536**, SHA256 `f57f1250951c906777e52d5a2e3524d9a6dfea3b64f283da82ad44a4a50e6e13`; original del propietario conservado fuera del repositorio.
- Fuente Unity evaluada: `72d132cf789738278ca17d1580e472b29a0eadd8` (PR173, misma rama), [Actions 43089450921](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43089450921), [artifact nativo 13887684195](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43089450921/artifacts/13887684195). Bastión real **900×1600**, SHA256 `626718e4466aea049dbf7edb902ae5bb23fdee9528368d64e6c9d5588ebac324`.
- Se descargó **el ZIP original**, se comprobaron **los 48 hashes SHA256** de las seis familias de evidencias (8 ciudad/ficha, 16 destinos, 10 perfiles, 2 navegación, 4 correo, 8 retención). No se confundieron reconstrucciones, renders conceptuales ni pruebas de código con capturas del motor.
- `tools/breach-watch/native_visual_ab_review.py` genera tres hojas A/B y un manifiesto; funciona **fuera de Windows**, falla ante hashes erróneos/marcos faltantes y **nunca emite aprobación numérica**. Ejemplo: `python3 tools/breach-watch/native_visual_ab_review.py --reference referencia.jpeg --native-zip artifact.zip --output revision --run-id 43089450921` (Pillow necesario).
- Unity reportó `oldCanvasVisible=0`, `activeForeignScreenSpaceCanvases=0`, `saveUnchanged=true`, `artworkApproved=false`. Esto certifica **presentación e integridad**, NO estética o rendimiento físico.

## Diferencias observadas — referencia del propietario frente a Unity 9:16

**Composición principal:** HUD de identidad y misión, tarjeta lateral de Bastión en piedra/burdeos, recursos y navegación de medallones se encuentran en posiciones similares. La miniatura del edificio corresponde al modelo 3D verdadero y sus cubiertas cobalto, no a un screenshot pegado. La composición actual se acerca **en estructura** a la imagen aprobada; no hay razón para deshacerla por un cambio cosmético pequeño.

**Diferencia de estado real:** la referencia enseña tres recursos, cinco accesos, utilidades y construcciones adicionales (aserradero/toldo) aun con el texto de misión del primer capítulo. El guardado genuino de la prueba solo tiene dos recursos disponibles y dos destinos de navegación habilitados. **Esas ausencias NO se deben rellenar con botones/edificios ficticios** para inflar una comparación visual. Para juzgar un tercer acceso hay que capturar el estado genuinamente desbloqueado (dos PNG nativos adicionales disponibles en el artifact), no modificar datos de juego.

**Calidad y organización por destino:**
- **Entrenamiento y Marchas:** ya exhiben tropa y héroes auténticos en un mismo Canvas, con controles ligados al gateway. El landscape ya elimina el tercer hueco de héroe inaccesible. La imagen no acredita todavía rendimiento ni precisión de pulsaciones en iPhone.
- **Héroes:** auténticos Aldric/Lyra y sus cinco paneles registrados en 390×844 y 900×1600. Hay cuatro casillas bloqueadas legítimas, no cartas inventadas; requieren revisión editorial para evitar que parezca una cuadrícula vacía.
- **Correo:** existe pantalla realmente vacía y composición con informe de prueba **marcado explícitamente como sintético en memoria**. El carácter claro sobre pergamino y marcos oscuros está resuelto. Aún falta evidenciar recorridos de mensajes históricos reales/persistidos del usuario, no inferirlos de la inyección de prueba.
- **Vigilantes:** discrepancia artística más notable de esta serie. El paisaje propio está ricamente ilustrado mientras que el enemigo normal dentro del panel táctico se dibuja mediante `RoyalGoldV3GuardianReliefGraphic` como figura **geométrica de escaso detalle**, no al nivel del entorno ni del retrato de Aldric. No intentar otra foto de patrulla pegada: B1–B4 de este método ya se rechazaron. La técnica siguiente debe ser un **retrato auténtico de enemigo canónico, integrado por alfa limpia y luz/perspectiva coherentes**, procedente de modelo o arte autorizado, solo cuando haya fuente válida.
- **Exploradores:** conserva el paisaje original y los **tres contratos de pergamino auténticos**. La prueba de convertirlos a losas carbón fracasó artísticamente; no repetirla por pura homogeneidad. La consistencia material permite piedra de navegación y papel en informes/contratos.

**Cobertura pendiente (no simular):** `RoyalGoldV3ManagementScreens` implementa Entrenamiento/Marchas; `RoyalGoldV3BuildingScreens` es una utilidad **inline** para consejo de desarrollo y aceleradores cuando procede, no un Gobierno pleno. No se encontró una pantalla canónica independiente de Inventario/Gobierno con los mismos contratos de acciones persistentes en estos adaptadores. **No incorporar aún accesos visuales falsos**; auditar/integrar esos destinos cuando existan API y progresión canónicas. No modificar otros PR ni mecánicas fuera de scope.

## Decisión y siguientes pruebas

**Conservar** la pantalla principal y los pergaminos actuales como baseline; no volver a microtintados, fondos rectangulares ni tres tarjetas oscuras de Exploradores. **Priorizar** una fuente real de arte de adversario y su sustitución *in place* en Vigilantes, después el equilibrio espacial/jerarquía de Héroes y el correo con datos persistidos; evaluar el fotograma completo y rechazar una variante si empeora a igual escala. Solo activar nuevo dibujo tras Unity exact-HEAD, capturas 900×1600/390×844/landscape, pruebas de interacción real y comparación con la imagen aprobada. Registrar cada técnica aceptada/rechazada.

**En este momento:** la licencia Windows ha pasado formalmente de PR173 a PR179 (owner PR173 conservado), y existe rotación PR179→PR181→PR187→PR173. Este audit y el script se prepararon sin usar Windows ni solicitar una nueva reserva. Nada fusionado, desplegado, reactivado globalmente ni certificado en iPhone físico.

**Conclusión:** fuente/evidencia nativa **verificadas**, calidad 8/10 global **no acreditada**, imágenes A/B listas para juicio artístico. No confundir el resultado verde de Unity con que todos los destinos estén acabados.
