# Eldoria · 2.5D de producción — contrato del experimento (2026-11-16)

**Ámbito:** sólo `art/ciudad-2d-hitos-1-5-20261116` y `Unity/Assets/Experimentos/Ciudad2D/`. `main`, `Valoria_City_World_UI.unity`, partidas, persistencia y reglas de combate no se modifican. **Sin aprobación artística ni móvil hasta pruebas reales en Unity/dispositivo.**

## Dirección artística obligatoria

- Fantasía medieval oscura **adulta**, sin iconografía infantil ni detalles modernistas. Valoria resiste bajo la amenaza violeta de la Brecha, en una región de piedra cálida, madera envejecida, pizarra azul, metal austero, vegetación y fuego cálido.
- Isométrico ilustrado 2.5D: bloques gráficos independientes (terreno, muralla, edificio, decoración, luz/FX y UI), nunca una fotografía única ni un edificio falso que no exista en la partida.
- Unificar cámara, horizonte, paleta, resolución y dirección de la luz para cada ilustración. Evitar el aspecto «pegatinas sobre césped»: contacto con el terreno, sombras y caminos coherentes.
- Hitos: aspecto del Bastión I durante niveles 1–4; V durante 5–9; futuros 10–14/15–19 sólo cuando existan assets y pruebas. Mejora numérica cada nivel con las reglas reales; no inventar cambios de gameplay.

## Contrato de visibilidad y desbloqueos (autoridad: `GameState.cs`/`LocalGateway.cs`)

| Parcela del sandbox | ID de juego | Desbloqueo real | Vista antes de construir | Vista terminada |
| --- | --- | --- | --- | --- |
| Bastión | `bastion` | inicio | — | siempre; hito I o V |
| Aserradero | `sawmill` | Bastión I | parcela disponible / obra | sprite aserradero si `SawmillLevel > 0` |
| Cuartel | `barracks` | Bastión II | bloqueada → parcela / obra | sprite cuartel si `BarracksLevel > 0` |
| Granero | `granary` | Bastión III | bloqueada → parcela / obra | sprite granero si `ChapterThree.GranaryLevel > 0` |
| Cantera productiva | `stoneworks` | Bastión V | bloqueada → parcela / obra | sprite cantera si `ChapterThree.StoneworksLevel > 0` |
| Salón de Héroes | *sin construcción persistida* | **no definido** | sólo espacio ambiental, no seleccionable | **no mostrar como construido** sin contrato posterior |

No confundir la cantera de producción de ciudad (stoneworks, desde V) con el **nodo del mundo** `quarry-valoria`, disponible bajo otras reglas. `BuildingTaskId` y `BuildingCompletesUtcTicks` son la única fuente para obras en curso; la presentación consulta y nunca modifica. `Build` sólo está permitido por gateway. El Salón de Héroes existe como sistema/menú, pero no tiene un campo de nivel de construcción asociado a una parcela.

`City2DProgressionPolicy` implementa lectura de `PlayerState`, `City2DStateBridge` aplica resultados; `CityBuilding` oculta el sprite de edificio terminado durante fases bloqueada/disponible/en obra y conserva un collider sobre una parcela disponible. Los escenarios de comparación I–V son fixtures **expresamente simuladas**; no son guardados del usuario.

## Terreno y futura ampliación

- Propuesta de mapa por **sectores** con origen y coordenadas de mundo estables, sprites de suelo intercambiables y decoración por capas. La zona central actual es `941×1672` coordenadas de composición, NO el límite del diseño definitivo.
- Tres reservas no renderizadas en `QA/city_layout.json`: oeste (arrabal de los vigías), este (distrito de talleres), norte (marcas de la Brecha). Nadie debe colocar allí edificios jugables hasta autorizar parcela, reglas y terreno ilustrado con transiciones comprobadas.
- **No ampliar estirando el PNG actual** ni duplicando la muralla pintada. La primera expansión requerirá una lámina del terreno exterior coherente y un único sistema de encuadre; probar transición de cámara, colisiones y coste de memoria. Las parcelas existentes conservarán ID y posición.
- Prever niveles de densidad y LOD de sprite por zoom; mantener zoom regional/panorámico en el mundo 4X separado de la cámara de la ciudad.

## Pantallas del juego — matriz de construcción gráfica

| Sistema | Técnica candidata | Reutilizable | Riesgo/puerta de aprobación |
| --- | --- | --- | --- |
| Ciudad Valoria | Escena Unity ortográfica con sprites de parcela por fase, ambiente/FX y cámara | kit edificios, caminos, HUD mínimo | lectura en retrato/horizontal, niveles y taps reales |
| Mundo 4X (Cercana/Regional/Panorámica) | chunks de terreno estilizado 2D/isométrico, nodos y niebla por capas | material de suelo, iconos de recursos | expansión/zoom y culling sin perder información táctica |
| Preparación de marcha, ejército | UI 2D de tropas/héroes/formación real | iconos marcos, paleta | no cambiar reglas V4.1 ni habilidades automáticas |
| Informes/combate automático | cartas de personajes, panel de rondas, efectos breves prerenderizados | paneles/heráldica/FX | informe fiel a `CombatReport`; PvE/PvP según reglas existentes |
| Salón de Héroes (Aldric/Lyra) | retratos de alta calidad y fichas 2D; NO nueva obra de ciudad hasta contrato | marcos de retrato, botones | conservar héroes, desbloqueos y acciones reales |
| Relicario/cartas | marcos por rareza, ceremonia y efectos en capas | borde de metal, partículas, tipos | ver arte aprobado y mantener primera carta por misión |
| Vigilantes y Exploradores | ilustración de escena + paneles y estados de misiones | kit de pergaminos, progreso | recompensas y timers auténticos; legibilidad móvil |
| Recursos/bolsa y buzón | iconos 2D nítidos, contadores y tarjetas con estados reales | iconos recursos, CTA, pestañas | no inventar premios ni confundir accesos |
| Cofre diario | arte de cofre como referencia mínima de acabado, animación 2D | luces, marcos | idempotencia, no doble cobro |
| Tutorial/nick/guía | overlays mínimos, mano de guía anclada a controles reales | tipografía, señales de navegación | toque/cierre/flujo sin bloquear usuario |
| Alianzas/expansión marítima | **no implementado** | futuro sistema visual | exige mecánicas y servidores antes de diseñar HUD final |

## Secuencia verificable

1. Capturas offline por capítulos I–V → comprobar que los edificios completos no aparecen anticipadamente; inspección de composición.
2. Navegador aislado → 4 viewports, 5 estados, interacción, zoom, desplazamiento, ausencia de errores y evidencia PNG; solo valida el **inspector**, no Unity.
3. Windows exclusivo y autorizado → Unity 6000.3.23f1, comprobación de escena construida desde sprites, C# compile, escenas I–V, snapshots con obra, capturas RenderTexture y reporte con SHA256. No afirmar PlayMode con una captura de Editor.
4. Prueba auténtica PlayMode de punteros/gestos + instantánea del gateway y sin modificación de guardados. Después dispositivo físico iPhone y Android, perfiles memoria/GPU y WebGL.
5. Sólo tras revisar comparativas reales y defectos, decidir promover una capa gráfica (nunca fusionar automáticamente ni publicar beta).

## Límites conocidos

- Los props recuperan riqueza, pero los edificios secundarios y el fondo todavía comparten demasiada simetría y una estética más amable que el lore definitivo.
- Los indicadores de parcelas sin construir usan recursos provisionalmente reutilizados; aún requieren arte propio de obras y ruinas.
- Las tres zonas externas están **reservadas** en datos, no es expansión real del terreno; no confundir diseño escalable con ampliación ya implementada.
- El Salón de Héroes **no posee regla de nivel de parcela** en los datos actuales; resolver esto con el propietario y contrato canónico cuando se aborde la integración.
- Verificación en navegador y comprobaciones estáticas no validan automáticamente runtime nativo ni memoria de iPhone.
