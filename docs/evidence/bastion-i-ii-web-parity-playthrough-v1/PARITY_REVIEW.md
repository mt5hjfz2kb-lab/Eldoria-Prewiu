# Bastión I–II — contraste del vertical slice y recorrido Unity

Workstream: `bastion-i-ii-web-parity-playthrough-v1`  
Referencia: `v0220/js/gameplay.js`, `v0220/js/chapters.js`, `v0220/js/hero-army.js`; Unity `SliceContentProfiles`, `GameState`, `LocalGateway`, `SlicePresenter`, `ReferenceUiArtPass`, `SliceBoot`.  
Estado: **en verificación de build y publicación**.

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

Pendiente de registrar aquí el SHA de código, run de compilación, artifact exacto, QA del candidato, capturas corregidas, publicación y QA sobre la URL final. La build anterior sigue siendo la versión pública hasta promoción explícita.
