# Issue #186 — Primer avance de implementación, aislado

**Estado:** módulo de reglas de dominio escrito en una rama separada; la integración jugable y las capturas de Unity están PENDIENTES. No constituye una modificación de la beta ni del juego publicado.

## Contrato aprobado por el propietario
- Nodos, caza y edificios suman recursos al contador por sus vías actuales.
- Cualquier otra recompensa en madera, piedra o comida se entrega como paquete a mochila: **no suma al contador al recibirlo**.
- Buzón informativo con reclamación explícita: mensaje → Reclamar → paquete en mochila → Usar → recursos en contador. Leer no es reclamar.
- Iconos con aviso discreto para correo nuevo / premio diario listo. La novedad de mochila no se trata como una obligación eterna de consumir.
- Tutorial contextual con mano/puntero anclado al control REAL, que enseña el siguiente paso y solo lo considera completado tras la acción de juego confirmada.

## Implementado en este avance (no activado aún)
\`RewardExperienceRules.cs\` contiene objetos de paquete serializables, \`TryIssue\` para fuentes indirectas, \`ClaimMail\`, \`TryUse\` que abona una sola vez, comprobación de desbordamiento y copias profundas. Conserva recibos usados como lápidas para impedir duplicaciones en reintentos. También contiene la clasificación semántica de indicadores y la persistencia de los pasos del tutorial; NO dibuja puntos ni manos sin un control real.

## Por qué está aislado
La PR #181 ya modifica \`GameState.cs\`, \`LocalGateway.cs\` y el flujo de recompensas. La PR #173 es dueña del HUD nuevo; la #179 está resolviendo los reinicios del iPhone. Integrar en ellas a ciegas crearía conflictos o comportamientos no probados. Este avance no modifica esos archivos, ni usa Windows.

## Pendiente obligatorio para una función jugable
1. Después de coordinar los tres titulares, agregar \`ResourceRewardInventory\` y \`GuidedTutorialProgress\` al guardado real \`PlayerState\`, su clon \`Snapshot()\` y la migración segura de guardados antiguos.
2. Unificar TODAS las rutas de abono que hoy incrementan directamente contadores (misiones en \`GrantWebMissionRewards\`, Vigilantes, exploradores y Tesoros; distinguir de caza/nodos/producción). Concesión, reclamación, consumo y guardado deben ser **una transacción autoritativa** de \`LocalGateway\`, no operaciones UI.
3. El buzón debe mostrar premios pendientes y reclamar con comando real sin entregar al contador. Mensajes solo informativos deben permanecer iguales.
4. PR #173 debe integrar indicadores visuales en accesos reales y el puntero ligado a \`RectTransform\`, respetando el safe-area, sin duplicar HUD. Validar que el aviso de premio no se quita por abrir la ventana.
5. Definir apilamiento, límites/espacio de mochila y compatibilidad con recompensas antiguas. Nunca dar recursos directos como fallback ante mochila llena.
6. Unity EditMode/PlayMode, guardados reales, 4 resoluciones, controles táctiles, WebGL/iPhone y recorrido I–V. Un test verde de Ubuntu NO acredita Unity ni experiencia del jugador.

**No fusionar/publicar** hasta tener integración de gameplay y pruebas sobre la misma fuente, y comprobación artística con el diseño aprobado.


## Avance recuperado, 11/11/2026 — prueba de origen vs. prueba real

Esta sección **complementa**, no sustituye, el contrato anterior. No hay merge ni lanzamiento.

- **Arte fuente**: los 16 PNG importados, 12 paquetes (madera/piedra/comida × 100/1K/10K/100K), 2 avisos, mano y halo, están inventariados en \`Unity/Assets/Eldoria/Issue186Art/issue186_manifest.json\`. Sus SHA256 y tamaños coinciden con el paquete local de conversación \`Eldoria_Issue186_Unity_Import_v2.zip\` (corresponde al conjunto artístico v3). **No se ha demostrado identidad con una variante v7** porque aún no se compararon sus bytes.
- **Fuente verificada**: CI fuente del commit \`6ef0be8e16fe853e2c59017cd7f1d8b8535ea75a\` aprobó hash/PNG/GUID (16), diferenciación de los 12 iconos a 48 px (21 pares de comparación), 94 checks de dominio y Roslyn sintáctico. No es un Unity compile ni una aprobación artística. Los cambios posteriores requieren nueva CI, no heredan automáticamente este resultado.
- **Gobernanza recuperada**: el registro \`main/pipeline/active-workstreams.json\` señalaba un fichero de solicitud de creación artística ausente; ahora existe \`pipeline/art-production-requests/eldoria-issue186-reward-attention-guidance-20261111.json\` y el source gate valida su identidad, presupuesto cero, estado PENDIENTE y 16 nombres esperados.
- **Fallos de prueba nativa corregidos en fuente**: el puntero ya puede mirarse desde el lado izquierdo cuando se cortaría a la derecha, mide sus esquinas contra el safe-area real y no tapa el centro de la acción. La prueba nativa tenía una contradicción técnica; se reconciliaron el resultado, la aceptación C# y la comprobación PowerShell. Una pulsación de interfaz sin comando real de \`LocalGateway\` **NO** autoriza marcar como completado el tutorial. La captura aislada prueba raycast y pulsación reales, pero guarda \`firstUseCompleted=false\` y confirma que el puntero continúa mientras falta recibo autoritativo. Las tres capas deben exigir ese mismo contrato; protección de regresión en \`test_native_layout.py\`. Cuando se integre el gateway deberá existir **otra** prueba que confirme el paso con una operación real, y solo entonces valore \`firstUseCompleted=true\`. El auditor de importación ahora resuelve los PNG relativos a \`Application.dataPath\`, no al cwd arbitrario del runner Windows.
- **Seguridad económica**: las señales de aviso se guardan por \`EventId\`, no reaparecen tras reclamarse y abrir un mail no elimina el premio. Además, \`TryIssue\` rechaza \`MailEventId\` solo de espacios: antes almacenaba un premio bloqueado imposible de reclamar. Existe cobertura fuente de ese caso. Sigue pendiente la transacción autoritativa en el gateway propietario de #181.
- **Prueba pendiente**: no despachar \`issue186-native.yml\` hasta CAS legítimo de \`windows-runner-heavy\`, sin trabajos Unity solapados. Solo las 4 capturas auténticas \`1600x900\`, \`900x1600\`, \`844x390\`, \`390x844\` y sus raycasts distinguen importación nativa de una composición pre-Unity. Incluso ese resultado sería \`UNITY_ISOLATED_IMPORT_PASS_ART_REVIEW_PENDING\`, NO \`ART_APPROVED\` ni \`MOBILE_PASS\`.
- **Coordinación**: PR173 sigue titular del HUD/Windows; PR181 de \`GameState\`/\`LocalGateway\`; PR179 del iPhone beta. No reabrirlas ni editar rutas compartidas a ciegas. La migración de recompensas en la ciudad es una integración posterior verificable.


## QA nativa reforzada — obstrucción real de botones (11/11/2026)

Se detectó un **falso positivo de interacción** en el banco de prueba de `Issue186NativeEvidence`: buscaba el botón «Reclamar» en *cualquier* posición de la lista del `GraphicRaycaster`. Si otro gráfico interceptaba la pulsación por encima del botón, el ensayo podía seleccionar artificialmente el botón oculto y declarar `realUiClick=true`.

La prueba aislada ahora monta **temporalmente una imagen interceptora real sobre el centro de «Reclamar»**. En cada una de las cuatro resoluciones debe confirmar que esa imagen es el primer impacto del raycaster y que dirigirle la pulsación **no dispara el botón**. Después la desactiva, vuelve a raycastear y exige que «Reclamar» sea el primer impacto, antes de generar un `IPointerClickHandler` auténtico. La imagen interceptora no aparece en las capturas. El resultado `blockedUiClickRejected=true` es obligatorio tanto en la aceptación C# como en el workflow Windows, y una regresión fuente impide volver a usar `hits.Find`.

**Estado de evidencia:** cambio de prueba implementado en la PR #187; el contrato fuente no es una prueba Unity real. Los resultados gráficos y la ejecución del control deben revisarse en los artefactos de Unity cuando exista la cesión formal CAS de Windows por #173. Tampoco certifica el flujo económico de `LocalGateway` ni la experiencia en móvil.


## Corrección visual basada en los PNG reales — placa de cantidad (11/11/2026)

Se inspeccionaron los 12 sprites originales que están **efectivamente en Git** (tres recursos × cuatro cantidades), no una composición recreada. Cada tarjeta ya lleva **marco dorado y una placa vacía pintada en el 17% inferior**. La primera captura nativa preparatoria colocaba otra tarjeta oscura por debajo y el número por *fuera* de esa placa: a pesar de pasar las comprobaciones de colisión, habría dejado un recuadro vacío y un texto flotante, un defecto artístico visible.

En la misma PR #187 se modificó `Issue186NativeEvidence` para eliminar el fondo duplicado y superponer el número real directamente sobre la placa integrada del propio sprite. La etiqueta ocupa como máximo el 70% del ancho de la ilustración, con tamaño de letra adaptativo a los 48–96 px de visualización. El Editor comprueba `Text.preferredWidth` contra el espacio de la placa; el modelo fuente sincroniza filas/espaciado y verifica que las etiquetas quedan dentro de las tarjetas a `1600×900`, `900×1600`, `844×390` y `390×844`.

**Pendiente:** comprobar píxeles/texto y contraste en las cuatro capturas Unity reales, incluso si las pruebas Python pasan. El `Issue186ArtKit.BindPacket` solo enlaza el sprite y el texto; la interfaz final que integre #173 deberá respetar **esta colocación dentro de la placa**, no duplicar marcos ni situar el valor debajo. La inspección directa de sprites demuestra el defecto del montaje anterior, **no equivale a aprobación del arte ni acredita que v7 esté presente en Git**.
