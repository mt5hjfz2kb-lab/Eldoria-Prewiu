# Eldoria v0.16.0

Última versión jugable válida del prototipo web de Eldoria.

## Cómo abrirlo

1. Descomprime el ZIP.
2. Abre `index.html` con Chrome, Edge, Safari o Firefox.
3. No necesita instalación, servidor, cuenta ni conexión a internet.

## Contenido del hito

- Poder Total calculado desde Reino, Ejército, Héroes y Colección, con desglose bajo demanda.
- Poder de Expedición separado, recomendado por combate y usado solo como orientación.
- Ranking de Poder conectado al valor real del jugador.
- Fórmulas y coeficientes centralizados en `window.ELDORIA_CONFIG.power`.
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
