# R2-C — Auditoría de UX móvil basada en el juego canónico (8 octubre 2026)

**Alcance:** Ronda 2 C (D10 UI y D09 input), auditoría de compatibilidad no destructiva. **Estado:** revisión estática y preparación de criterios, no mejora visual publicada. No modificar cámara ni los archivos del propietario M07.

## Autoridades consultadas
- `docs/ELDORIA_MASTER_MAP.md`: bucle Valoria → necesidad/decisión → Mundo → recompensa → regreso → crecimiento; evitar controles ficticios.
- `docs/VALORIA_BASTION_I_II_VERTICAL_SLICE_MIGRATION_V1.md`: la referencia UX es el vertical slice web aprobado; Unity conserva estado y acciones reales, solo CIUDAD/MUNDO funcionales.
- `docs/VALORIA_MASTER_PLAN_AND_PROGRESSION_REQUIREMENTS.md`: Valoria es ciudad desplazable, límites reales de cámara, no postal panorámica.
- `Unity/Assets/Eldoria/Scripts/Presentation/SlicePresenter.cs`: botones del bosque deben coexistir con Marcha, panel contextual y barra de objetivos sin duplicar CTA.

## Hallazgos y acciones productivas para D10/D09
1. **Nuevo panel del Bosque (R2-B):** verificar que botones EXPLORAR / APROVECHAR / ENVIAR MARCHA / CERRAR se ven completamente en viewport portrait 390×844 y landscape 844×390. No inferir ausencia de solapamiento solo porque se compila.
2. **Legibilidad de decisión:** textos deben explicar claramente “+20 sin restar reserva” y “+40 restando 40 de reserva”; resultado persistente visible después del reload, sin botón que permita repetir la decisión.
3. **No regresión de gestos:** arrastre de cámara, recenter y tocar hotspots en Mundo/Valoria deben seguir funcionando con el panel cerrado. Panel abierto no debe interceptar accidentalmente acciones de mapa.
4. **Autoridad visual:** comparar con vertical slice como fuente de jerarquía y con actuales capturas oficiales; no introducir HUD ficticio ni nuevos subsistemas.
5. **Criterios para cierre:** capturas y vídeos/Playwright de ambas orientaciones sobre build WebGL del SHA final; touch real emulado y, si disponible, revisión física iPhone identificada por separado; sin errores de escena y sin regresión I–II.

## Gate y propiedad
No cambiar `SlicePresenter.cs` desde R2-C mientras R2-B lo reclama para implementar el panel; limitar aquí el trabajo a evidencia y criterios. Tras el cierre técnico de R2-B y cesión explícita de scope, D10 puede aplicar un ajuste concreto derivado de capturas defectuosas. Presupuesto 0 €, sin producción de arte no planificada.
