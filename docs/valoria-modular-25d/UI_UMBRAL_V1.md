# UMBRAL — interfaz experimental original para Valoria modular 2.5D

**Ámbito:** únicamente la escena `BastionI_Modular25D.unity`, en la rama aislada `experiment/valoria-modular25d-ui-umbral-20261120`.  
**Origen técnico:** commit `254c34b71582dfe9e064e066d083361a3e0e7507` de `experiment/valoria-modular-25d-bastion1-from-zero-20261118`.  
**Estado:** implementado en C# como nueva presentación, pendiente de compilación y captura de Unity. No fusionar ni dar como arte aprobado por un verificador sintáctico.

## Decisión de arte

**Nombre:** *Umbral — Ceniza y Bronce*. Se inspira en las fortalezas de piedra, el inicio de reconstrucción y el pergamino imperial de Eldoria.

No trasladamos las pantallas comerciales de Whiteout Survival, Kingshot o Top Heroes. Conservamos sus patrones generales observables: escenario protagonista, jerarquía clara, recursos accesibles, interacción contextual, botones táctiles de una sola función y progresión legible. La fuente técnica exacta de sus interfaces no está verificada y no se presupone.

Tampoco se trata de trasladar Vigilia a otro fondo. La tipografía autorizada se reutiliza por coherencia y licencia, pero **la composición, las superficies de cuatro materiales y las reglas de ubicación son nuevas**. No se copian capturas, marcos ni assets de otros juegos.

## Paleta cerrada V1 y funciones

| Rol | Hexadecimal | Uso |
| --- | --- | --- |
| Azul tinta de Valoria | `#142630` | Cabecera, estructura de controles, feedback |
| Pizarra iluminada | `#283E46` | Superficies graduales |
| Pergamino imperial | `#EBDEC0` | Ficha contextual de Aserradero |
| Marfil cálido | `#F9F0D9` | Luz y graduación interior de pergamino |
| Bronce envejecido | `#B58D58` | Filetes, sellos y números de valor |
| Dorado apagado | `#E4C58B` | Inscripciones sobre fondo oscuro |
| Verde mineral | `#3B6962` | Una única acción principal de construcción |
| Tinta de lectura | `#272F31` | Texto principal sobre pergamino |
| Tinta secundaria | `#5D5A4D` | Explicaciones sobre pergamino |
| Arcilla de aviso | `#974D45` | Recursos insuficientes; no se usa en CTA |
| Violeta de La Brecha | `#775891` | **Reservado** para acontecimientos narrativos reales; no forma parte del HUD normal |

Contraste de pares calculado con WCAG relativo (estimación matemática de color sólido, no prueba de pantallas): marfil cálido sobre tinta 13,45:1; oro sobre tinta 9,38:1; texto oscuro sobre pergamino 10,24:1; tinta secundaria sobre pergamino 5,19:1; marfil cálido sobre verde mineral 5,37:1. Las transparencias y la escena real requieren revisión visual en Unity.

## Contratos de diseño

1. **HUD quieto:** un sello de identidad pequeño en esquina superior izquierda y **tres cápsulas independientes** de madera, piedra y comida, sin franja a todo lo ancho.
2. **Escenario visible:** solo dos accesos persistentes — ASERRADERO y CENTRAR — por debajo. No se exponen sistemas futuros ni botones decorativos.
3. **Acción contextual:** el Aserradero abre una ficha de pergamino (miniatura real, estado, coste, tiempo y botón); cerrada por defecto.
4. **Acción canónica:** `BuildSawmill()` y `PlayerState` reales. Sin economía propia, sin temporizadores inventados, sin duplicación de compra y sin nueva partida.
5. **Responsive:** retrato con ficha inferior de 355×228 unidades de referencia, desplazada por safe area; horizontal con ficha anclada a la derecha y recursos en esquina superior derecha. Los dos botones táctiles mantienen 53 unidades de alto sin escalarlos por debajo.
6. **Respuesta:** feedback transitorio 2,8 s, sin banner de depuración constante.
7. **Producción:** Cormorant Garamond para inscripciones y Source Sans 3 para cifras y acciones, aprovechando las fuentes OFL existentes en el experimento. No se distribuyen fuentes fuera del repositorio.
8. **Art assets:** las cuatro nuevas placas se generan como texturas de 64×64 con un borde pintado, ruido tenue y `Sprite.Create` con border (10,10,10,10). `Image.Type.Sliced` permite escalar sin deformar las esquinas. Estos materiales son un candidato de interfaz; **no constituyen todavía arte aprobado en móvil**.

## Estado, verificación, y regresiones

**Implementado en esta rama:** sustitución exclusiva de `Modular25DInterface.cs`, conservando la clase y `OpenSawmill()` que consume el mundo; botones con nombres funcionales anteriores; adaptación de NUnit y coordenadas de navegador para nuevo layout. `main` y la rama original siguen intactos.

**Necesario antes de integrar:**
- Compilar Unity 6000.3.23f1 y ejecutar el NUnit con EditMode→PlayMode. El test comprueba Canvas único, tres cápsulas 9-slice, ficha de pergamino, dos botones reales, construcción, pago único, guardado y cámara.
- Capturas con HUD real 390×844, 1170×2532, 844×390; comparar A/B contra el experimento original, con mismo estado y cámara.
- Verificar datos numéricos, clip de top safe-area, botones y transparencia/contraste sobre distintas zonas del suelo y frente a edificios.
- Reejecutar Chromium WebGL con construcción real, persistencia tras recarga, pan/pinch/Home y logs limpios.
- Si hay error de compilación/visual, corregir *esta* rama; no promover a la rama original antes de aprobar.
- Rendimiento/safe-area en iPhone físico pendiente, independientemente del resultado en Chromium.

No publicar beta, fusionar, tocar `main`, suprimir puertas de seguridad de World4X o apropiarse del Windows reservado por otra rama. El otro chat puede seguir mejorando terreno y edificios mientras esta alternativa se valida.
