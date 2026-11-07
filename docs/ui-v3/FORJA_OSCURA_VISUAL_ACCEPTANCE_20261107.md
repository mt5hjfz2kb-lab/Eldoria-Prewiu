# Forja Oscura — contrato de fidelidad visual y pruebas (Eldoria)

**Estado:** WIP, NO ART_APPROVED, NO UNITY_NATIVE_PASS. **PR:** #173. **Referencia de decisión:** imagen «ELDORIA» que el propietario señaló expresamente en la conversación del 06/11/2026 (HUD oscuro con cinco destinos, medallones izquierdos y cartela derecha de Cuartel). **No** tomar como requisito el paisaje ni la luz de la ciudad ilustrada: sólo se evalúa la interfaz.

## Qué debe reproducirse de forma convincente

| Área de interfaz | Requisito visual | Controles reales / fuentes |
|---|---|---|
| Cabecera | Banda de metal oscuro trabajada, avatar/nombre/poder legibles y recursos con icono ilustrado | `RoyalHUD.Realm`, `RoyalHUD.Resources`, `RoyalGoldV2Hud` |
| Menú izquierdo | Medallones con pictogramas escultóricos y estado de notificación | Accesos de misión, exploradores y eventos existentes |
| Misión | Cartela estrecha, superficie rica y jerarquía capítulo / objetivo / progreso | `ChronicleObjectiveRect`, datos reales |
| Edificio | Cartela rectangular de metal, nivel real y estado/coste, sin tapar mundo ni abrir página modal nueva | `RoyalGoldV3ContextHud.ContextRect`, `VisibleActionButton`, `VisibleCloseButton` |
| Acción principal | Relieve ámbar, estados normal/pulsado/deshabilitado diferenciados y pulsables | El mismo `RoyalButton` existente |
| Navegación inferior | Cinco destinos REINO / CONSTRUIR / UNIDADES / MUNDO / CRÓNICAS, seleccionados con brillo cálido acotado | `BottomNavigation.Items`, NO duplicar navegación ni cambiar acciones |

## Criterios *técnicos* medibles

1. Unity 6000.3.23f1 importa los 41 PNG runtime y sus 55 metadatos `.meta`, sin errores de compilación ni GUID cambiados.
2. Las cuatro capturas `1600×900`, `900×1600`, `844×390`, `390×844` se toman sobre **la misma escena, cámara, edificio seleccionado y datos reales**, con `native-before` y `native-after` en la misma ejecución del capturador ya utilizado por Eldoria. Ocho PNG + SHA-256 + informe JSON.
3. El A/B cambia píxeles de la cabecera y la navegación en las **cuatro resoluciones**; un delta causado solo por animación del mundo no es evidencia. `ForjaOscuraNativeEvidence` exige cambios cuantificados en regiones del HUD.
4. Pulsación de los cinco destinos y acción de edificio con el **mismo controlador real**; bloqueos, cooldown, progreso y guardados idénticos. No se confunden colores y textos ilustrativos con datos del jugador.
5. No hay recortes, solapamientos ni área segura invadida; se miden tamaños táctiles **en píxeles de la captura efectiva**, no solo en unidades de `RectTransform`. Revisión de los botones de 44 px o equivalencia táctil en móvil; toda excepción requiere justificar y corregir.
6. Antes de publicar APK/WebGL: comprobar perfil de rendimiento, memorias y raycasts en dispositivos representativos; el resultado no se presupone.

## Criterios de arte independientes (inspección humana)

- **Riqueza:** grosor, materiales, sombreado y detalles del HUD deben transmitir el acabado del ejemplo escogido, no paneles planos simples.
- **Coherencia:** iconografía ilustrada de la misma familia; botones, bordes, tipografía y niveles son partes del mismo sistema, sin bordes clásicos exagerados.
- **Lectura:** contraste suficiente con ciudad clara y oscura, sin brillo o textura que hagan ilegibles cifras o etiquetas.
- **Inmersión:** edificio/cámara/world-play no cambian entre referencias; la interfaz no introduce ciudad ficticia, ni sale de su sistema de capas.
- **Aprobación:** sólo el propietario decide si la fidelidad al diseño es suficiente. `TECH_PASS` y `ART_APPROVED` son estados separados; el objetivo de calidad es el diseño elegido, no maximizar diferencias de píxeles.

## Qué NO se ha demostrado aún

No hay capturas nuevas de Unity del skin Forja Oscura, no hay perfil Android/WebGL, no hay ensayo nativo de pulsación ni decisión del propietario. El ZIP actual contiene arte original y fuente plausible, **no garantía de igualdad visual con la imagen generada**. Las proyecciones Python antiguas son hipótesis y siguen etiquetadas `NO_UNITY`.

El workflow aislado comprueba fuentes y crea preflight. El runner Unity Windows permanece legítimamente reservado por PR #163; nunca apagar su trabajo para fabricar evidencia. Tras una liberación formal, ejecutar el `ForjaOscuraNativeEvidence.CaptureBatch`, descargar los ocho PNG y evaluarlos en pantalla real antes de integrar.
