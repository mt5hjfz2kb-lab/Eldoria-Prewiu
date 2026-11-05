# Eldoria — Player Clarity / 05-11-2026

Workstream-ID: eldoria-first-login-map-signage-respawn-20261105  
Issue: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/issues/161

## Alcance real implementado de este primer commit

- Tres pictogramas vectoriales originales con silueta legible: troncos con
  vetas/anillos, piedra facetada y espigas doradas. Varias tintas por icono;
  se dibujan en el tamaño real de los slots del HUD, sin paquetes, coste,
  descargas ni licencias externas.
- Un adaptador runtime de presentación, aislado, que reemplaza **solamente**
  el dibujo de `Madera icon`, `Piedra icon` y `Comida icon` del HUD
  canónico cuando existe. Mantiene los tres RectTransform, textos, wallet,
  lógica de desbloqueo de Comida y acciones originales. No ocupa la escena,
  no crea Canvas, no atrapa raycasts y no modifica archivos de PR141.
- Pruebas EditMode para generar la geometría en 24×24, comprobar triángulos,
  colores y coordenadas válidas.

**Estado:** implementación de origen lista para comprobación en Unity.
Sin ejecución de Unity ni capturas A/B nuevas de esta rama; NO ART_APPROVED,
NO MERGED, NO PUBLICADO. Una compilación no certifica la belleza ni la
legibilidad de estos iconos en móvil. Los bocetos no son capturas de Unity.

## Arquitectura verificada antes de editar

`RoyalIconGraphic` actual es geometría plana monocolor; `RoyalGoldV2Hud`
coloca los iconos auténticos `Madera icon`, `Piedra icon`,
`Comida icon` dentro de ResourceBar. El nuevo componente no cambia
`RoyalGoldV2Hud` ni `RoyalHUD`.

`LocalGateway` ya mantiene UTC y no duplica la población; los intervalos
actuales se configuran desde `RegionOneResourcePlacement` en 30 minutos y
`RegionOneCreaturePopulation` en 15. `WorldCreatureLocation.Level` sí
existe y está guardado; `WorldResourceLocation` no tiene un Level autoritativo.
El `SlicePresenter` anterior genera badges de tres edificios, pero el
Canvas heredado se deshabilita en la escena integrada. El HUD real es
`ValoriaIntegratedWorld.Hud`.

## Siguiente bloque, sujeto al relevo seguro de economía PR141

- Nick en PlayerState schema-1 con comando de gateway validado e idempotente;
  modal del primer arranque antes de narrativa y texto de nick junto al escudo.
- Niveles de recursos que correspondan a una magnitud **real** de producción
  o capacidad, persistidos y coherentes con Economía V2 (no adornos falsos).
- Etiquetas de ciudad/mundo en cámara Unity, pequeñas y visibles sin invadir
  UI, niveles reales de criaturas/caza y recursos, zoom/oclusiones.
- Configuración de 300 segundos en reposición de nodos y fauna renovable,
  migración de cooldowns existentes, dispersión por sectores y QA de
  colisiones/rutas y guardados.
- EditMode/PlayMode, recorrido I–V, A/B reales en 4 tamaños; sin publicar
  ni fusionar por simple éxito sintáctico.

## Advertencias de integración

No reintroducir niveles VI–X, no resucitar enemigos únicos de misión, no
mover la Brecha ni los nodos narrativos, no cambiar las coordenadas canónicas
de Valoria, no reproducir arte no aprobado. La selección del siguiente sitio
no debe hacerse solo en el cliente si un día el mundo es compartido.


## Continuación 05/11/2026 — implementación aislada en PR #162

**Propietario vigente del workstream:** `chatgpt-eldoria-player-clarity-continuation-20261105`, revisión 2. La integración de Economía V2 PR141 conserva control exclusivo de `GameState.cs`, `LocalGateway.cs`, el HUD canónico, `RegionOneResourcePlacement.cs` y los archivos de mundo. Esta candidata no modifica ese código.

### Implementado como código en la rama (aún no compilado por Unity)

1. `Unity/Assets/Eldoria/PlayerClarityRuntime.cs` añade una **capa de presentación sobre el mismo HUD real**. Muestra un pequeño pictograma en madera y piedra activas, «Nv. X» sobre los enemigos renovables que tienen `WorldCreatureLocation.Level` guardado y «Nv. X» al lado de los modelos visibles de Bastión, Aserradero, Cuartel, Granero y Cantera. Usa el estado auténtico del gateway y el modelo/cámara real, oculta la panorámica y controles modales, no recibe clicks/touches ni crea otro Canvas, no altera recompensa ni rutas.
2. El mismo adaptador solicita al `LocalGateway` **300 segundos de demora para futuros agotamientos/derrotas renovables locales**, sin afectar enemigos narrativos, PvP ni partidas remotas. La selección del nuevo emplazamiento permanece en el validador físico canónico `RegionOneResourcePlacement`: terreno accesible, bioma, agua, caminos, distancias a ciudad, colisiones y exclusión del sitio anterior. El adaptador **no modifica los timestamps ya guardados**, que todavía pueden conservar 15/30 minutos; esa migración debe hacerse en el gateway bajo PR141.
3. `PlayerNameRules.cs` valida y normaliza nombres de 3–18 caracteres (letras incluyendo acentos, números, espacio, guion, guion bajo); impide caracteres de control y marcado potencialmente confuso. `RoyalPlayerNamePrompt.cs` crea un pergamino de bienvenida real en UGUI con el texto «¿QUIÉN ERES?» y entrada de nick. Solo cierra cuando una función externa **guarda y verifica** el nombre, pero esa conexión todavía no existe: por diseño el prompt no aparece hasta integrar el campo en `PlayerState`. No guarda nada en `PlayerPrefs`.
4. `Tests/PlayerNameRulesTests.cs`: pruebas nuevas con acentos, espacios, longitud y entradas malformadas. Son **fuentes de tests**, no resultados ejecutados.

### Pendiente antes de declarar el encargo terminado

- **Dominio y persistencia**: campo `PlayerState.PlayerName` en schema-1, comando idempotente de asignación de nombre, `Snapshot` y `FileStateStore`, tests de partidas nuevas y anteriores. Después, enganchar el pergamino antes de la narrativa PR160 y sustituir solo el subtítulo del escudo por el nombre del jugador sin borrar «Valoria» o Poder.
- **Niveles auténticos de nodos y caza narrativa**: introducir una regla jugable de nivel y persistirla. `WorldCreatureLocation.Level` ya funciona para la fauna renovable; `WorldResourceLocation` no tiene `Level`, y `hunt-wolf/hunt-boar` tampoco. **No mostrar niveles inventados**.
- **Densidad/dispersión**: medir cuadrantes/sectores de la generación existente, aplicar dispersión por bioma en los algoritmos canónicos tras liberación de PR141 y migrar posiciones guardadas solo si hay una regla compatible con marchas.
- **Reaparición de registros anteriores**: recalcular los `RespawnUtcTicks` del evento real (5 min) sin perder datos ni resucitar enemigos únicos. Evitar repetición de premios, desplazamiento en mitad de marcha y regresión de progresión.
- **QA nativo Unity**: compilación exacta de la PR final, EditMode/PlayMode, test 299s/300s/offline, recarga de guardado, recorrido I–V, capturas A/B 390×844/844×390/900×1600/1600×900 y evaluación visual de cada nivel/pictograma.

### Evidencia y estatus

Las comprobaciones de propiedad, arquitectura, metadata y gobierno del commit `b6babc7d181b512f0244791b957cf483903aeef9` devolvieron SUCCESS en GitHub; **no certifican C#, escenas ni calidad visual**. Tras integrar el resto se exigirán nuevos checks sobre el HEAD exacto. PR #162 permanece **DRAFT, sin merge, sin build/release y sin aprobación artística**.

Nota de diseño: la prueba del usuario no debe ver cartelas superpuestas ni niveles falsos. Al agotarse una entidad no debe sustituirse la misma posición ni aparecer ningún duplicado, y al cumplirse el tiempo sin conexión debe reaparecer al sincronizar con el gateway.
