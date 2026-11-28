# Prueba de estilo 2.5D / Eldoria

**Piloto de Unity aislado. No se ha integrado en la beta. ART_APPROVED=false.**

Se prueba un lenguaje de **fantasía medieval estilizada semirrealista**, más cercano a un escenario pintado coherente que al fotorrealismo: tejado pizarra, piedra envejecida, terreno salvia/ocre, luces cálidas y Brecha violeta. No recrear modelos aprobados ni usar el Bastión vetado.

Este primer ensayo es reversible y usa los modelos y la cámara de la escena real `Valoria_City_World_UI.unity`, sin escribir sobre los materiales ni guardados de producción. El método Editor es `Eldoria.StylizedDirectionPilot.Editor.NativeABCapture.Capture` y emplea la sandbox `VALORIA_GAMEPLAY_PROBE`.

## Evidencia requerida

- `A-genuine-city-1600x900.png` y `B-stylized-city-1600x900.png`.
- `A-genuine-city-390x844.png` y `B-stylized-city-390x844.png`.
- `evidence.json` con SHA, cantidad de materiales alterados y diferencia de píxeles.

**El éxito técnico NO acredita calidad artística**. Una mejora solo cromática se descarta si el escenario sigue plano o poco coherente. Tras dos ciclos sin salto visual, reemplazar la técnica: kit de materiales/arquitectura con reutilización de assets comprobada y dirección de composición, no microretoques.

Todo es Editor-only, presupuesto 0€, no Tripo, sin edición de la beta, sin tocar los módulos P0 de mundo ni interfaces ajenas. La textura artística conceptual generada externamente no se presentará como captura real Unity. No hay pruebas físicas de móvil ni autorización de merge, publicación o art approved.

El trabajo exige un único run Unity bajo reserva CAS `windows-runner-heavy` y sin interferir con PR194/P0.
