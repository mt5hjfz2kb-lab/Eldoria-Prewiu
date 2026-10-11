# Bastión I–II — contraste del vertical slice y recorrido Unity

Workstream: `bastion-i-ii-web-parity-playthrough-v1`  
Referencia: `v0220/js/gameplay.js`, `v0220/js/chapters.js`, `v0220/js/hero-army.js`; Unity `SliceContentProfiles`, `GameState`, `LocalGateway`, `SlicePresenter`, `ReferenceUiArtPass`, `SliceBoot`.  
Estado: **CERRADO: candidato y publicación verificados; correcciones I–II entregadas**.

## Qué se comparó

| Contrato web I–II | Unity publicado antes de esta corrección | Resultado de la revisión |
|---|---|---|
| Inicio 230 madera, 150 piedra, 36 arqueros | WebGL fuerza `OWNER_I_II`; estado inicial capturado coincide | Implementado. La vieja nota “OWNER inactivo” era obsoleta para WebGL. |
| Aserradero 80 madera / 6 s, 600 madera y 500 piedra recuperadas, ruta corrupta, Bastión II 450/300 | Contadores persistentes, bosque y cantera, combate de ruta y coste real | Implementado; el recorrido publicado anterior alcanzó Bastión II. |
| Cuartel 180/120, entrenar 20 por 400/240, poder de expedición 2250, Marcha explícita, Engendro | Perfil dueño, composición confirmada, combate e informe persistentes | Implementado, con lote único de 20 en lugar de selector 5/10/20; la selección de cantidades sigue pendiente de una necesidad de juego demostrada. |
| Recompensas de misiones y capítulo | Unity registraba objetivos sin pagarlas | Corregido para `OWNER_I_II` con libro de pagos persistente, recursos y Poder total; ninguna recompensa se cobra dos veces. `QA_FAST` conserva su economía técnica. |
| Capítulo II incluye elevar Bastión III | Slice termina al derrotar Engendro en Bastión II | El cierre ahora dice “Bastión II asegurado · próximo hito: Bastión III”; no simula misión ni recompensa del capítulo II. |
| Tiempo y feedback de entrenamiento | La cuenta atrás no se mostraba por una rama `else if` mal asociada al efecto de obra; después de pagar, la tarjeta aún reclamaba los recursos ya gastados | Corregido; segundos visibles en el CTA principal y objetivo “entrenando 20 arqueros”. PlayMode comprueba ambos estados. |
| Estado completado legible | Captura exacta publicada: texto oscuro sobre botón desactivado oscuro | Corregido en `SlicePresenter` y en la capa de arte que redecora la interfaz cada 0,2 s. |

## Alcance de las recompensas

El perfil dueño paga las misiones web I: Aserradero +120 madera; madera +80 piedra; piedra +120 madera; ruta +90 madera/+55 piedra; ascenso +80 Poder; capítulo I +180 madera/+140 piedra/+120 Poder. Las misiones web II hasta Engendro: Cuartel +140 madera/+90 piedra; 20 arqueros +220 madera/+150 piedra; poder expedición +100 Poder; Engendro +120 madera/+120 piedra. Bastión III y el premio final del capítulo II quedan fuera del corte I–II.

El registro `GrantedRewards` es parte del guardado y del snapshot aislado. Los guardados previos del perfil dueño reciben una sola vez las recompensas de hitos ya cumplidos al actualizarse; el siguiente guardado/reload no vuelve a pagarlas. `MissionPower` suma al Poder total, sin adulterar el Poder de expedición de la Marcha.

## Prueba de experiencia y límites

La evidencia anterior de la build publicada 37999090263 / QA publicada 38002471123 cubrió un inicio limpio, selección real de edificios, construcción, dos expediciones de madera, cantera, Corrupto, Bastión II, Cuartel, recuperación de recursos, entrenamiento, Marcha, Engendro, recarga y reinicio en horizontal y vertical. Sus capturas muestran el defecto de contraste y la Región I aún provisional frente al nivel de arte de Valoria. La revisión presente inspeccionó HOME, Aserradero, Cuartel, Región I y cierre II y corrigió los defectos verificables descritos arriba.

El navegador Work de esta sesión indicó que no soporta WebGL y quedó en la carga, de modo que no hubo una partida manual independiente en él. El probe Chromium de Actions conduce entradas reales de la build y comprueba estado/persistencia, pero no equivale a juicio humano ni a iPhone físico. La calidad artística global de Región I y el encuadre HOME del Bastión no se declaran aprobados por este trabajo; requieren una corrección visual con capturas oficiales propias.

## Certificación de la nueva build

Completada el 10-10-2026. Fuente UI `006c0b360030d432a2d5b86f90c0e7b4b24ea3d0`: UI 38038180064 (20/20 EditMode y 29/29 PlayMode) y producción nativa 38038180076 aprobadas. Build WebGL 38038397612, fuente `47422ca94609f3672f6f60ad794d6f23e81bdc6a`, artefacto 11664888284. QA candidato 38039578310 / 11665293571 aprobada. Publicación 38040075806 promovió ese mismo artefacto, sin recompilar, y verificó su SHA-256. QA real publicada 38040278321 / 11665726793 aprobada.

En horizontal y vertical: recorrido limpio I→II, selección táctil de Aserradero/Bastión/Cuartel, construcción, recursos, cantera, Corrupto, ascenso, reclutamiento, Marcha preparada, Engendro, recarga exacta y reinicio/recarga. Cero errores de ejecución. Estado final idéntico por orientación: revisión 26, madera 1030, piedra 995, 56 arqueros y 20 entrenados. Capturas del candidato y publicación inspeccionadas directamente: «ENTRENAMIENTO · 7 s» y «BASTIÓN II ASEGURADO» legibles. Este PASS visual es acotado a esos estados de interfaz; no certifica arte global de Región I, encuadre HOME, iPhone físico ni una partida humana independiente.

La comparación HTTP de todos los archivos públicos no pudo ejecutarse desde este entorno; no se declara 45/45. La identidad del paquete publicado está respaldada por el gate de promoción, artefacto y digest de descarga, y la experiencia publicada por su probe separado. Resultado persistido en [release-result.json](release-result.json). Enlace: https://mt5hjfz2kb-lab.github.io/Eldoria-Prewiu/unity-owner/ . Créditos pagados: 0.
