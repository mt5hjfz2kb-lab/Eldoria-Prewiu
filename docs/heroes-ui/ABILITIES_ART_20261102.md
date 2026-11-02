# Habilidades de Aldric y Lyra — reconstrucción modular (02-11-2026)

**Ruta de integración:** PR #136, `fix/player-feedback-context-ui-20261030`. Sin publicación, merge, gasto de créditos, modelos nuevos ni cambios a guardados.

## Implementación real

- `HeroesSkillCards.cs` implementa una única habilidad real por héroe, con el nombre y la descripción procedentes de `ReadCanonicalHero`, no de la referencia aplanada.
- **Sir Aldric — Baluarte**; **Lyra — Disparo de Ruptura**. Las cifras y la maestría que aparezcan en la descripción son exclusivamente las que suministre el estado existente.
- Sustituido el bloque de título/texto simple en `HeroesApprovedDestination.BuildInformation` por componentes uGUI independientes: tarjeta activa, ornamento dorado, sprite de icono aprobado, marco existente de nueve segmentos y texto de Unity con desplazamiento vertical real.
- En anchuras y alturas amplias: tres **reservas visuales pasivas**, marcadas `POR DESCUBRIR / BLOQUEADA`. No son mecánicas, no tienen identificadores ni `Button`, no representan nivel, coste o bonificación.
- En móvil corto se omiten las reservas para mantener legible la habilidad real, con fuente de descripción de **18 px sin autoescalado**. Conservado el nombre de `Text` requerido por la comprobación nativa preexistente.
- Sin alterar componentes del escenario, caché/modelos Tripo, navegación lateral, talentos, equipamiento, progreso, perfil ni persistencia.

## Comprobaciones estáticas — no equivalen a captura real

Verificada la conexión `HeroState.AbilityName/AbilityDescription` → `HeroesSkillCards.Build`; los destinos y el selector siguen intactos; la decoración usa `Image.Type.Sliced`, icono independiente, `RectMask2D` y `ScrollRect`. Ningún elemento de las reservas tiene eventos de jugabilidad.

Estimaciones geométricas sobre los tamaños que captura la prueba real (los resultados Unity pueden variar con el Canvas y la fuente):
- 390×844: región de información aprox. 350×205, tarjeta activa ~144 alto, texto desplazable ~50 alto; sin reservas.
- 844×390: región 346×202, tarjeta activa ~142, texto desplazable ~48; sin reservas.
- 900×1600: 4 tarjetas, cada una de ~93 de alto, descripción con desplazamiento; tres reservas.
- 1600×900: 4 tarjetas, cada una de ~144 de alto, descripción con desplazamiento; tres reservas.

## Criterio de cierre pendiente — **NO CERTIFICADO**

1. Ejecutar `Eldoria.Editor.EldoriaV41NativeCapture.Capture` sobre el HEAD final de #136 cuando el titular de `windows-runner-heavy` libere su reserva; no lanzar de forma concurrente.
2. Inspeccionar las ocho capturas actuales de **Aldric/Habilidades** y **Lyra/Habilidades**: `landscape`, `phone`, `small-phone`, `small-phone-landscape`.
3. Comprobar que los nombres completos, 3 párrafos reales de descripción, icono, marcos y estado bloqueado son legibles; sin recorte o colisión con el modelo, rail lateral ni selector. Comprobar scroll táctil real, tanto vertical como horizontal.
4. Mantener el `V41_NATIVE_PASS` con estado/guardado idéntico, probar selector y las otras pestañas, contrastar visualmente contra el maestro `docs/heroes-ui/REFERENCIA_MAESTRA_APROBADA.png`.
5. No declarar nota ≥8,2/10, `art_approved` o entrega hasta disponer de las capturas auténticas y evaluación artística. No utilizar capturas antiguas como prueba del HEAD modificado.

En la referencia aprobada, Aldric tiene cuatro hileras ornamentadas. Se respetan las cuatro **tarjetas visuales** sin replicar las tres habilidades ficticias de la imagen; Baluarte es la única real y ocupa la tercera fila de Aldric. Para Lyra la habilidad real aparece arriba. No se atribuyen etiquetas Activa/Pasiva, ya que el host no expone esa clasificación. En móvil corto se prioriza una sola tarjeta.

La ruta canónica de captura ya recoge **las cuatro resoluciones de Habilidades de ambos héroes** y compara el guardado completo. La reserva Windows pertenece a otro ejecutor (Valoria), no a este workstream.
