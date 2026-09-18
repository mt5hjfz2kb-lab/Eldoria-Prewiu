# ELDORIA — CONTINUIDAD DEL PROYECTO

> Documento operativo de recuperación. Leer junto al Documento Maestro antes de continuar desarrollo.
> Actualizado: 2026-09-18.

## Regla de oro
**Lo validado no se vuelve a fabricar; se conserva y se evoluciona encima.**
No reconstruir Eldoria desde cero, no proliferar ramas/versiones y no usar el chat como única memoria del proyecto.

## Fuentes de verdad y recuperación
- Visión de producto/lore: `Eldoria_Documento_Maestro_Work_v1.0(1).docx` en Biblioteca.
- Baseline externa protegida: `v0210/index.html` (v0.21.0).
- Desarrollo actual: `v0220/index.html` (v0.22.0), desplegado en GitHub Pages.
- Reglas anti-regresión: `ELDORIA_BASELINE_RULES.md`.
- Históricos: `v0201/index.html`, `v0202/index.html`.
- La v0.22 todavía NO sustituye formalmente la baseline v0.21 hasta validación explícita del usuario.

## Objetivo de Eldoria
Juego móvil online de fantasía medieval oscura que combina City Builder, 4X, RPG/héroes, colección y capa social/competitiva. La referencia de mercado incluye Whiteout Survival para patrones de progresión/economía/presentación, pero Eldoria debe tener identidad propia.

Frase guía actual:
**Reconstruye Valoria, haz crecer tu poder y conquista un mundo que la Brecha está transformando.**

Core loop:
**CONSTRUIR → PRODUCIR → GASTAR → MEJORAR → EXPLORAR → COMBATIR → CONSEGUIR RECURSOS/HÉROES/CARTAS → FORTALECERSE → COMPETIR/COOPERAR → VOLVER A CONSTRUIR.**

Éxito de primera fase: no cantidad de sistemas, sino un núcleo agradable que genere deseo de continuar.

## Diagnóstico de playtests v0.21
La build era completable pero comprensión y engagement eran inconsistentes. El principal problema no era un bloqueo funcional general: era deseo/retención. Cartas y Duelo llegaban demasiado pronto; combate/tutorial ocupaban demasiado protagonismo; el jugador no sentía suficientemente “Valoria es mi reino y quiero hacerlo crecer”.

## Dirección v0.22
Recentrar el juego en:
**VALORIA → NECESIDAD → MUNDO → RECOMPENSA → VALORIA MÁS PODEROSA → NUEVA AMBICIÓN.**

El mundo debe parecer vivo antes de que todo sea interactuable: recursos, enemigos, fortalezas, zonas futuras y amenazas visibles.

### Combate
- Combate normal del mundo: ligero y automático como sistema, pero VISIBLE en el mapa (marcha → impacto breve → resultado).
- No abrir minijuego/pantalla táctica para encuentros normales.
- El combate semiautomático de v0.21 se conserva para posible contenido especial: Arena/Foso, jefes de Brecha, expediciones o encuentros especiales. No decidir prematuramente cuál.

### Poder
El Poder NO es botín.
- Poder Total deriva de posesiones/desarrollo: edificios + tropas + héroes + equipamiento/colección relevante.
- Poder de Marcha deriva solo de tropas/héroes enviados.
- Matar enemigos da botín/XP/objetos, no Poder mágico.
- Reclutar tropas, mejorar edificios o desarrollar héroes sí aumenta Poder.

### Recursos
- Nodos con reservas FINITAS.
- Capacidad de carga menor que la reserva implica varios viajes.
- El nodo solo se agota al llegar a cero.
- Futuro: nodos agotados desaparecen/liberan posición y reaparecen recursos en otras ubicaciones; niveles superiores en zonas más peligrosas.
- v0.22 actual: Bosque 1.250 madera/carga 360; Cantera 900 piedra/carga 260.

### Ejército
- Tropas reales empiezan a existir.
- Cuartel permite reclutar Guardias gastando recursos.
- Reclutar aumenta Poder Total y Poder de Marcha.
- Futuro: pérdidas/heridos, capacidad de carga, colas y tiempos.

### Narrativa
Regla: **El mundo enseña a jugar. Los personajes hacen que te importe el mundo.**
- Recuperar la calidad de los últimos textos/escenas de Sir Aldric de v0.21; NO recuperar el tutorial invasivo.
- Aldric: anfitrión/guía narrativo, no comentarista constante.
- Apertura v0.22 debe dar contexto breve de Valoria + Brecha y presentar a Aldric antes de soltar al jugador.
- Personajes aparecen en hitos significativos y convierten mecánicas en acontecimientos.
- Lyra debe aparecer porque una necesidad jugable la hace deseable (Fisura III), no como explicación de un menú.
- Orin y su estructura cinematográfica validada se conservan en v0.21.

### Héroes/cartas/Duelo
- Héroes: deseados antes de explicados.
- Cartas: retrasar y simplificar primera exposición; utilidad real.
- Duelo: fuera del núcleo inicial hasta que Valoria/mundo/progresión funcionen.
- Semiautomático, cartas, Códice, Duelo, Aldric, Orin, Resonancia, etc. NO se han perdido: v0.21 es banco de sistemas.

## Flujo jugable v0.22 actual
Valoria dañada → Aserradero → mundo → recolección finita → Corruptos → Cuartel/reclutar → Bastión II → necesidad de segundo héroe → Lyra → Fisura III → XP/botín → Bastión III → teaser Mar de Cristal.

## Presentación
Objetivo de screenshot: “un reino de fantasía que quiero hacer crecer”, no “una aplicación con paneles”.
- Prioridad móvil/iPhone.
- La arquitectura visual v0.22 es más espacial (Valoria + mapa 4X), pero emojis/CSS siguen siendo assets provisionales.
- Siguiente salto visual serio: assets propios coherentes de Valoria, edificios, héroes, unidades, enemigos, Brecha y terreno.
- Avatar superior izquierdo = futuro retrato/foto del jugador y acceso a perfil/ajustes. Ya contiene menú y reinicio seguro.

## Tiempo y economía
A partir de esta fase, construcción/recolección deben tener tiempo visible.
- Vertical slice: tiempos comprimidos (segundos) para poder probar el loop completo.
- Diseño final: mismos sistemas preparados para tiempos mayores y colas.
- Un temporizador debe representar una acción real; no añadir esperas gratuitas.
- Recolección debe mostrar marcha/actividad/retorno y construcción debe mostrar progreso antes de aplicar el cambio.

## Qué NO hacer
- No borrar/reconstruir v0.21.
- No devolver Poder por derrotar enemigos.
- No nodos infinitos.
- No convertir el combate normal en minijuego central.
- No reintroducir tutoriales que expliquen cada botón.
- No meter cartas/Duelo solo porque ya existen.
- No afirmar validación táctil/iPhone si solo pasaron checks de código/Actions.
- No declarar v0.22 baseline hasta validación explícita.

## Protocolo si cambia el chat
1. Abrir este archivo.
2. Abrir `ELDORIA_BASELINE_RULES.md`.
3. Consultar Documento Maestro.
4. Recuperar `v0220/index.html`; NO reconstruir.
5. Revisar últimos commits/Actions.
6. Continuar en la misma v0.22 hasta hito real.
7. Implementar → QA → publicar → comprobar Pages → responder.
