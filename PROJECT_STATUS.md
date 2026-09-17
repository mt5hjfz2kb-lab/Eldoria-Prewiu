# Eldoria — Estado del proyecto y punto de continuidad

Actualizado: 2026-09-17

## Publicación actual

Candidato público para aprobación: **v0.19.5 r7 — Vertical Slice Candidate · Polished**.

Ruta: `v0195-r7/`

La raíz de GitHub Pages apunta a esta candidata para playtest del propietario.

## Flujo de publicación recuperado

El conector de GitHub tiene límites prácticos para builds grandes. El flujo histórico usa una base estable y loaders/parches de texto para publicar revisiones sin reconstruir el proyecto desde cero. Para r7 se parte de la publicación r6 y se aplica un parche r7 antes de renderizar. La build fuente consolidada r7 permanece como baseline de desarrollo y el loader es solo el mecanismo de distribución web.

## Regla de validación

No declarar la candidata aprobada hasta completar playtest humano móvil. Si aparece un bloqueo, corregirlo sobre r7 y mantener el flujo de publicación reproducible.

## Núcleo del vertical slice

Reconstruir Valoria → salir porque necesitas algo → mundo/decisión → combate → recompensa → volver más fuerte → mejora visible de Valoria.

Sistemas que deben conservarse: héroes con XP/nivel/habilidades, colección de reliquias, Duelo de Reliquias, mundo global 4X, combate semiautomático, Poder Total/Poder de Expedición, capítulos y presencia narrativa de La Brecha.

## Próximo gate

1. Playtest móvil completo desde inicio hasta Mar de Cristal.
2. Registrar bloqueos, overlays, botones fuera de pantalla y dudas de tutorial.
3. Decidir cierre del vertical slice después de esa prueba, sin añadir sistemas antes.
