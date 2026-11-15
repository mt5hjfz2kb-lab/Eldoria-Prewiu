# Bastión — prueba de fidelidad visual en Unity (15/11/2026)

**Situación:** la fuente del piloto está implementada. A/B nativo y aprobación artística pendientes; no hay publicación, fusión, cambio del HUD en producción, ni prueba física iPhone.

## Hipótesis documentada

La referencia aprobada por el propietario (75B5D217-A914-4451-854D-AE54849E4A50(3).jpeg, 864×1536, SHA-256 f57f1250951c906777e52d5a2e3524d9a6dfea3b64f283da82ad44a4a50e6e13) presenta una ficha de Bastión con marco de piedra y oro fino, miniatura 3D grande, precios, texto y acción burdeos.

En el HUD actual, la ficha y el botón se dibujan usando CleanRoom/panel y CleanRoom/upgrade. En el proyecto siguen conservados los elementos extraídos por separado de la referencia original: VigiliaArt/OwnerReference/carved_context (256×398, 9-slice 39/34/34/57) y VigiliaArt/OwnerControls/upgrade_button (246×60). Ambas fuentes tienen metadatos GUID originales y están libres de textos fijos.

El piloto compara **A: Vigilia actual** contra **B: fuentes originales del propietario**, cambiando únicamente esos dos sprites de la ficha real. Conserva el mismo Canvas de Vigilia, el 3D del Bastión, nombre, nivel, costes, duración y acción auténticos, interacciones y selector.

## Captura nativa reproducible

Clase Editor-only: Unity/Assets/Eldoria/BastionCardFidelityPilot/Editor/BastionCardFidelityCapture.cs. Ejecutar desde Unity 6000.3.23f1, checkout de esta rama, fuera de Play:

- Método de editor: Eldoria.BastionCardFidelityPilot.Editor.BastionCardFidelityCapture.CaptureBatch.
- No utilizar -quit prematuro. El método abre la escena real de Valoria en modo de sonda desechable, resuelve el nick y narrativa de la prueba, y entra en Play.
- Salida: Unity/BastionCardFidelityEvidence/ con 8 capturas Unity (A/B en 900×1600, 390×844, 1600×900 y 844×390), evidencia JSON con hashes y estado. Error explícito si no funciona.
- Verifica que sigue visible el modelo 3D auténtico y que el estado de juego no cambió. Al terminar devuelve los sprites originales de la sesión de prueba. No toca ninguna partida del propietario.

El runner Windows está reservado para PR194 / reinicio iPhone. **No lanzar Unity ni ocupar runner hasta liberación CAS legítima.**

## Gate artístico de verdad

Confrontar la referencia con el A/B real, no dibujos de IA:

1. Contorno, riqueza de metal/piedra, ausencia de doble bisel.
2. Proporción del retrato 3D, sin oclusiones ni textura inventada.
3. Título, nivel, costes y duración legibles a escala 390×844 y horizontal.
4. Martillos y etiqueta del botón separados, acción correcta y hit area intacta.
5. Reflow, lienzo, contexto y misma cámara; ninguna imagen de pantalla completa pegada sobre UI.
6. Solo tras aprobar visualmente: interacción WebGL, memoria y iPhone físico.

Si B no mejora claramente A en las capturas verdaderas, se rechaza. El siguiente método deberá crear un kit de componentes artísticos originales y escalables, no repetir tintes. Una captura, cambio de píxeles o CI verde NO implica ART_APPROVED.

**Estado actual:** Unity A/B pendiente; ART_APPROVED=false; physical_iphone_tested=false; beta/main sin cambios funcionales.
