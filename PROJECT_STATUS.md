# Eldoria — Estado del proyecto y punto de continuidad

Actualizado: 2026-09-17

## Objetivo
Vertical slice móvil/web de Eldoria: City Builder + 4X + RPG + colección + PvP/social. El usuario dirige; ChatGPT ejecuta diseño, integración, QA y publicación sin reconstruir desde cero ni fingir validaciones.

## Fuente de verdad funcional actual
Repositorio: `mt5hjfz2kb-lab/Eldoria-Prewiu`

Enlace raíz público: `https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/`

La raíz debe apuntar siempre a la última versión estable que queramos probar.

## Regla crítica de trabajo
No volver a encadenar wrappers/parches r4→r5→r6 ni iframes entre versiones. Los cambios nuevos deben consolidarse en una build integrada/limpia y validarse antes de mover el enlace principal.

## Flujo objetivo del vertical slice
1. Prólogo: Valoria ha caído.
2. Capítulo I · Las Cenizas de Valoria.
3. Reconstrucción guiada: Granero → Aserradero → Cantera → Forja → Cuartel.
4. Cierre del Capítulo I con recompensa clara.
5. Capítulo II · Ecos de la Brecha.
6. Salida al mundo y primer combate.
7. Elección de La Brecha.
8. Primera carta/reliquia.
9. Tutorial de Sir Aldric.
10. Salón de Héroes.
11. Introducción al mundo global 4X.
12. Bosque seguro: primera recolección.
13. Cantera disputada: recurso ocupado/decisión.
14. Zona de la Brecha: decisión global.
15. Pequeña valoración del mundo.
16. Regreso a Valoria.
17. Bastión.
18. Segunda expedición.
19. Resto del Capítulo II.
20. Colección hasta 5/5.
21. Duelo de Reliquias obligatorio con Maestre Orin.
22. Cierre del Capítulo II.
23. Teaser: Capítulo III · Mar de Cristal.

## Decisiones de UX ya tomadas
- El tutorial de Héroes va dentro del Capítulo II, no en un capítulo separado.
- El primer Duelo de Reliquias no se puede saltar.
- El tutorial del Duelo debe ser jugable y guiado, no una línea de texto: leer N/E/S/O, usar centro +1, observar una jugada de Orin, comparar valores enfrentados y luego liberar tablero.
- Durante mundo global, el panel de misión debe quedar visible/destacado y explicar claramente el siguiente objetivo.
- El mundo global forma parte obligatoria del Capítulo II; no debe poder llegarse al final sin completar Bosque → Cantera → Brecha global.
- Las pantallas importantes no deben abrir otras encima mediante callbacks concurrentes. Cada cadena debe ser secuencial y cerrada.
- En móvil: nada de paneles enormes, botones fuera de pantalla, elementos que se tapen ni texto excesivo.

## Sistemas que deben conservarse
- Héroes: nivel/XP, estadísticas, habilidades con rangos 1–5, Esencia Heroica, Resonancias de la Brecha, reliquias/compañeros preparados.
- Cartas/reliquias: progresión 0→1→3→5, uso/guardar, rareza, Indestructible futuro.
- Duelo de Reliquias: tablero 3×3, mano de 5, valores laterales, capturas, centro +1, tutorial con Orin.
- Mundo global 4X: Valoria, otros reinos, nodos de recursos, Bosque seguro, Cantera disputada, PvE, fortaleza de clan y Zona de la Brecha.
- Combate semiautomático.
- Capítulos y ceremonias de inicio/cierre.
- Poder Total / Poder de Expedición.

## Problema técnico detectado
Se acumuló una arquitectura de publicación demasiado frágil: loaders y revisiones que cargaban otras revisiones y aplicaban reemplazos en tiempo de ejecución. Esto provocó pantallas vacías, estados adelantados y regresiones aunque la sintaxis local pasara.

## Solución técnica acordada
- Consolidar una build limpia integrada.
- Una sola base de código/estado/save key por versión.
- Evitar wrappers encadenados y reemplazos runtime entre revisiones.
- Añadir validación de integridad y errores visibles.
- Antes de publicar: comprobar flujo, estados, sistemas y móvil por separado.

## Estado de publicación reciente
Se creó una ruta `v0195-clean` para reconstruir una build consolidada y verificar integridad, mientras el enlace raíz se mantuvo/ajustó como estable según pruebas. Antes de afirmar que una versión es válida, volver a inspeccionar el repo y probar la publicación real.

## Próximo trabajo solicitado por el usuario
1. Revisar y rehacer el MVP marcando claramente: hecho / pendiente / nuevo tras las últimas decisiones.
2. Auditar el proceso técnico: por qué cambios pequeños requieren demasiadas interacciones y simplificarlo para que no vuelva a pasar.
3. Hacer una revisión completa del juego: flujo, UX, tutoriales, sistemas, estados, regresiones, contenido y coherencia.
4. Intentar jugar una partida completa real de principio a fin usando navegador/QA si la herramienta disponible lo permite; si no, decir exactamente qué se pudo y no se pudo validar.

## Regla de continuidad entre chats
Si se abre un chat nuevo, empezar leyendo este archivo y el estado actual del repo antes de tocar nada. No depender solo de memoria conversacional para detalles de implementación.
