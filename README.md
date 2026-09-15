# Eldoria v0.17.0

Última versión jugable válida del prototipo web de Eldoria.

## Cómo abrirlo

1. Descomprime el ZIP.
2. Abre `index.html` con Chrome, Edge, Safari o Firefox.
3. No necesita instalación, servidor, cuenta ni conexión a internet.

## Contenido del hito

- Tutorial contextual revisado: una explicación breve cuando aparece cada mecánica, sin cadenas largas de ventanas.
- Capítulo I reforzado con indicios ambientales sobre Valoria, Sir Aldric y La Brecha.
- Presentación ceremonial en la primera obtención de Aldric, Lyra y Maelis; las mejoras posteriores usan feedback breve.
- Ceremonia reservada para hitos como Indestructible, carta rara y desbloqueo del Duelo.
- Sistema de localización mediante claves en `window.ELDORIA_DATA.locales`.
- Selector ES/EN sin reiniciar ni borrar la partida. Español es la referencia; inglés cubre la navegación, Poder, tutorial principal, lore inicial y ceremonias, pero todavía no todos los textos secundarios.
- Se conservan Poder Total, Poder de Expedición y ranking conectados a la progresión real.
- Flujo reino → mundo → combate → recompensa → regreso.
- Combate final semiautomático.
- Capítulo I — Las Cenizas de Valoria, con narrativa breve integrada en la acción.
- Se empieza con 0 cartas; las cinco reliquias se recuperan en las tres expediciones.
- Decisión no obligatoria entre consumir o conservar cada carta.
- Cualidad aleatoria Indestructible, aplicable a cualquier rareza.
- Duelo de Reliquias 3×3 desbloqueado al completar la colección 5/5, con primer duelo tutorial contra Maestre Orin.
- Perfil local y rankings simulados.
- Interfaz móvil del Archivo y el Duelo a pantalla completa.
- Cuestionario final y telemetría local ampliada para playtest externo.

## Limitaciones

Es un prototipo HTML/CSS/SVG sin backend. No contiene clanes, PvP online, tienda, VIP, servidores ni Unity.

La probabilidad de Indestructible es deliberadamente provisional y se configura en `window.ELDORIA_CONFIG.indestructibleChance` dentro de `index.html`.

La preparación conceptual de estados de servidor y Live Ops está documentada en `ARCHITECTURE.md`; no existe todavía implementación online.

La localización inglesa es funcional en el recorrido principal, pero parcial en pantallas secundarias. Los textos heredados restantes siguen en español y deben migrarse a claves antes de añadir un tercer idioma.
