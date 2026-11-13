# Eldoria / Tesoros de Valoria — lenguaje visual vinculante

**Fecha:** 8 de noviembre de 2026 · **workstream:** `eldoria-daily-treasures-20261108` · **PR:** #181. Este documento es una especificación de producción, **no** aprobación de Unity ni de calidad humana.

## Corrección artística a raíz de la revisión del propietario

La última imagen conceptual creada dentro del chat **NO representa el lenguaje visual de Eldoria** y queda **RECHAZADA como referencia de producción**. En ella aparecían escenarios florales/fantásticos muy iluminados, dominancia violeta, resplandores excesivos y premios imaginarios (gemas, monedas, pociones). Que resulte vistosa no significa que encaje en Eldoria, ni que represente los recursos y sistemas del juego.

**Referencia mínima que sí se mantiene:** cofre ilustrado y panel original de siete recompensas que el propietario aprobó, armonizados con el HUD canónico de **Vigilia de la Brecha PR173** (basalto/metal ennegrecido, cobre y oro envejecido, marfil y acción burdeos). La ciudad real y el mundo deben seguir siendo protagonistas, no quedar reemplazados por un póster.

## Contrato de materiales, tipografía y composición

| Elemento | Contrato correcto | No admisible |
|---|---|---|
| Base | Piedra negra/carbón texturizado, ligera profundidad, espacios libres | Escenarios violetas pastel, flores, neón de feria, screenshot completo de UI |
| Marcos | Un solo contorno fino de cobre/oro labrado y bronce, sombras de contacto | Doble/triple ribete pegado, ornamentos superpuestos, paneles barrocos |
| Botones | Burdeos oscuro como el `burgundy_action` de Vigilia, texto marfil | CTA amarillo fosforescente, botones azules de fantasía |
| Texto | Cormorant Garamond / Libre Baskerville cuando el HUD ya los aporte; marfil legible | Pequeño texto sobre destello, tipografía ornamental ilegible |
| Recursos | Madera, piedra, comida y aceleradores **reales**, exactamente registrados por gateway | Cristales, oro, pociones, monedas especiales o probabilidades inventadas |
| Cofre | Las dos poses ilustradas de roble negro, hierro y bronce en el repositorio, luz ámbar sobria | Cofre morado de joyería caricaturesco o cofre geométrico de emergencia como final |
| Hitos | Pinturas reales de Sala de Héroes, Exploradores y Vigilantes, oscurecidas, con composición propia; Bastión real visible | Pegar otra ilustración de ciudad/lugar con progreso ficticio |
| Brecha | Violeta mineral leve **solo** como pista de Vigilantes/corrupción | Teñir TODAS las pantallas o convertir la interfaz en fantasía rosa/violeta |
| Movimiento | Anticipación contenida, apertura comprensible, vuelo breve de bienes reales, sonidos físicos de madera/bronce | Confeti permanente, estallidos que ocultan la recompensa, loops compulsivos |

**Valores fuente codificados y próximos al HUD Vigilia:** piedra `(.085,.090,.095)`, cobre `(.92,.70,.40)`, marfil `(.98,.94,.84)`, burdeos oscuro `(.28,.12,.10)`. Estas cifras son colores de materiales de uGUI, no aprobación del render.

## Decisiones ya aplicadas a PR181

1. El panel y las tarjetas intentan **reutilizar exactamente** las texturas `VigiliaArt/PremiumSurfaces/slate_resource` y `burgundy_action` cuando PR173 esté realmente integrado. Mientras sean ramas separadas, mantienen las nueve imágenes originales de PR181 como fallback real. No copian assets de otra rama ni agregan un segundo HUD.
2. Los iconos de madera, piedra y comida intentan utilizar los mismos `VigiliaArt/ResourceIcons` que la ciudad cuando estén disponibles; los propios sprites de PR181 continúan como respaldo, y el reloj de arena conserva su imagen original.
3. La textura de `profile_plate` de Vigilia es **muy horizontal** y no se estira en el modal vertical: se conserva el contorno cuadrado propio del cofre hasta disponer de capturas nativas. Esta es una restricción de proporción/material, no un parche ornamental.
4. La iluminación de todos los acontecimientos se ha acercado al tono apagado de cobre y piedra. Exploradores utiliza un verde mineral moderado; Vigilantes un violeta muy limitado. Se redujo la dominante de brillo de los tres fondos originales.
5. Las pinturas reales de la Sala de Héroes, el mapa de exploradores y el paisaje de Vigilantes son **existentes en el proyecto**, no pantallas falsas ni imágenes recién generadas. Bastión deja ver la ciudad verdadera, con una ceremonia sin economía duplicada.
6. El test `tools/daily-treasures/check-visual-identity.mjs` se ejecuta en Ubuntu CI para rechazar cambios de paleta, botones y recursos inventados antes de emplear Unity.

## La calidad no se certifica por una imagen bonita

El resultado de PR181 continúa **NO-GO artístico** hasta compilar y capturar el verdadero Canvas de Unity 6000.3.23f1 con PR173 y el juego, revisar sus 52 PNG originales y comprobar en dispositivos la sensación de relieve, contraste, iconografía, encuadre, sonido, animación y rendimiento. La prueba automática detecta desviaciones groseras, pero **no puede sustituir el juicio visual humano**. No fusionar ni publicar sin ese umbral.

**Estado: concepto desviado rechazado, dirección artística corregida en código y contrato, CI fuente pendiente de última SHA, nativeUnityPassed=false, artApproved=false.**


## Verificación posterior — 12 de noviembre de 2026

La ejecución nativa [43207284828](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43207284828) terminó con éxito sobre `0875d8d1dc740c41924f3ebfb9b33c700e36040b`. Sustituye el estado técnico pendiente del 8 de noviembre para esa fuente; no convierte el contrato artístico en una aprobación.

El lote contiene 52 imágenes contractuales, 20 de días ordinarios, 20 de contexto de ciudad y 90 fotogramas suplementarios de Exploradores: 182 PNG en total. Hubo 28 cobros sobre gateways temporales y 116 verificaciones de puntero. Las imágenes de ciudad certifican composición y restauración del entorno, no una partida integrada con PR173.

El ZIP nativo `13940310497` (`b96402cdf621fa02f44bf1dcccae8486c3a12670fd71ccff3bfb8c56daf1c5f5`) y el ZIP auditor `13940005647` (`d2fa22eb26cd238aec2f878b41fed02a77fe30bec15c83bf630eee8b428ac44a`) se descargaron y cotejaron independientemente. También se verificaron los 90 hashes, dimensiones y orden de la secuencia de Exploradores antes de codificar sus seis segundos originales a 15 fps. Esta secuencia silenciosa es un muestreo del componente, no una grabación de juego ni prueba de un hito ganado.

**Estado técnico de esa fuente: PASS. Estado artístico, audio en juego, integración con PR173 y dispositivos físicos: PENDIENTES.** La tipografía candidata de PR173 no se activa en Tesoros sin su A/B separado. Se mantiene la prohibición del concepto floral violeta rechazado y de inventar recursos, escenarios de progreso o cobros.
