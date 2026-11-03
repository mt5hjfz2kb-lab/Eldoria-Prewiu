# Eldoria — reconstrucción artística del Buzón V4.1 (03/11/2026)

## Autoridad y línea base

- Repositorio: `mt5hjfz2kb-lab/Eldoria-Prewiu`.
- PR canónica: **#136**, rama `fix/player-feedback-context-ui-20261030`; PR #134 es origen funcional/histórico, **no** abrir otra PR.
- Referencia original aprobada: `buzon_final_pestanas_APROBADO.png`, **1169×1345**, SHA256 `ef01ed38d83d0be012561320727098e59eaa1e809d08f2febf1b1dd44886b0dd`, en el kit artístico de 29/10/2026 y registrada en `Unity/Assets/Eldoria/Resources/EldoriaV41/provenance.json`.
- **A (antes):** fuente UI `408f0a1f25e3e170bff7a4fa34affe7a0585387c`; Unity run `41597116608`, artefacto V4.1 `13212698652`. Capturas auténticas disponibles: `mailbox-phone.png` (900×1600), `mailbox-landscape.png` (1600×900), `mailbox-reference-ratio.png` (1169×1345), `mailbox-real-report.png`.
- Problema confirmado en esas capturas: una tarjeta pequeña pegada a la cabecera y enorme rectángulo vacío y negro; informe sin superficie lectora propia. La referencia aprobada usa marco dorado continuo, cuatro pestañas, tarjetas de pergamino con iconos y estados.

## B candidata de código, antes del gate nativo

- Fuente de Buzón: `Unity/Assets/Eldoria/UI/ValoriaRoyalHUDV2/Runtime/RoyalGoldV41MailboxScreen.cs`.
- Capturas y pruebas: `Unity/Assets/Eldoria/Scripts/Editor/EldoriaV41NativeCapture.cs`.
- Test aislado EditMode: `Unity/Assets/Eldoria/Tests/EditMode/EldoriaV41ContractsTests.cs`.
- Base de esta iteración: `38b198896bdcaef6ce564ad2c123fda79eb54e49` (**no** es certificación ni puntuación de arte; confirmar el SHA final de ejecución).

### Qué se ha cambiado

1. Sustituir el fondo negro liso por composición cálida en geometría uGUI, con trazos ornamentales no interactivos y marca de agua de recurso original sin texto ni falsos mensajes.
2. Pestañas originales de cuatro categorías (`informes`, `recompensas`, `sistema`, `narrativa`) con fuente original, estados activo/inactivo, contadores reales y botones nativos.
3. Marco de colección y documento de informe con borde `HeroCardNineSliceGold` nativo nine-slice; las filas existentes usan `mail-card`, iconos originales por categoría, indicador `unread-dot` separado y estado leído atenuado.
4. Lista de altura variable determinada por **datos reales**: cero, uno, varios y página llena. No mostrar cinco tarjetas ficticias cuando solo existe un correo.
5. Página normal hasta cinco correos; en 844×390 se usan dos por página para impedir recortes y colisiones. La navegación conserva el orden y los identificadores.
6. Vista del informe con manuscrito propio, desplazamiento vertical real, botones de marcar leído/eliminar. No mostrar el texto desnudo sobre fondo negro.
7. Informes de batalla restringidos a resultado, héroes, tropas, rondas, vida y botín ya registrado; **no** presentar `PlayerPower`, fórmula de Ataque/Ruptura, `Reason` táctico ni estrategia óptima.
8. Las recompensas siguen siendo abonadas por `LocalGateway` según reglas originales. `MailRead` y `MailDelete` no ejecutan un segundo pago ni se convierten en botón de reclamación inventado.

### Pruebas previstas y preservación

- `EldoriaV41NativeCapture`: capturas genuinas de un informe en 1169×1345, 900×1600, 1600×900, 390×844 y 844×390; detalle en 900×1600, 390×844 y 844×390; vacío tras borrar en 900×1600, 1600×900, 390×844 y 844×390.
- Verificar abrir el acceso real por raycast, marcar leído, borrar, recargar la partida, conservar lápidas/identificadores y no volver a pagar recompensas.
- Test de lista de 12 eventos **solo de prueba**, fuera de partida/publicación: páginas 5/5/2, marcar leído, borrar, serializar/recargar e impedir resurrección por recepción repetida.
- Las capturas deberán ser inspeccionadas contra la referencia original **y** contra A. No afirmar `PASS`, `ART_APPROVED`, puntuación >=8/10 ni entrega hasta tener un run del SHA exacto, observar sus PNG y corregir defectos.
- Sin otra pantalla de gameplay, sin activar motor de combate experimental V4.1 para campaña, sin cambios de dominio de economía/guardados, sin modelos ni créditos.
- Mantener PR136 en DRAFT, sin merge/publicación ni correos ficticios en producción.

## Estado de coordinación al iniciar

Titular canónico Valoria revisión 113: `chatgpt-eldoria-mailbox-v41-20261103`, sin recurso Windows reservado. El workstream Issue95 mantiene `windows-runner-heavy` y un workflow nativo activo. **No** cancelar, desplazar ni asumir libre la reserva por la simple ausencia de jobs; actualizar el registro vivo con CAS antes de cualquier certificación y esperar a que el otro titular la libere de forma real.

## Cierre pendiente

B native A/B real 4 resoluciones, evaluar vacíos/un informe/paginación larga y detalle, completar dominio y Unity íntegros, conservar SHA y artefactos, documentar veredicto y liberar recursos conforme a `docs/ELDORIA_PARALLEL_WORKSTREAM_PROTOCOL.md`.
