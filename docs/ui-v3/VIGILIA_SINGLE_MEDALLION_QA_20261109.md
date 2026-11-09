# Vigilia — auditoría de iconos y capas, 9 noviembre de 2026

## Defecto demostrado
El propietario observó dos botones superpuestos en la navegación inferior de las capturas reales, aunque la pantalla había pasado sus comprobaciones. El problema estaba en la construcción, no era algo que debiera resolverse colocando otra imagen encima.

**Causas verificadas en código (PR173):** `BuildButton` generaba una placa genérica más un glyph vectorial; después se cargaba el medallón ilustrado, se deshabilitaba el glyph y se superponía otro halo de selección independiente. Además, `polish_owner_medallions.py` añadía una segunda circunferencia, brillos, arcos y tachuelas a un medallón que ya venía completo de la referencia. El tamaño reducido hacía visibles contornos fantasma y formas detrás de otros círculos.

## Cambios implementados
- Cada uno de los cinco destinos tiene **un único sprite de arte visible** (`PremiumMedallions/castle|provisions|swords|raven|helm`) colocado como hijo de un `Button` cuya imagen de toque es completamente transparente. No existe placa visual genérica subyacente ni glyph vectorial ni anillo de selección separado.
- Los medallones se regeneran desde `OwnerReference` con un ajuste de metal cobrizo dentro de los **mismos píxeles originales**, sin crear un segundo borde visible. El file ID/GUID de cada PNG permanece estable.
- Se eliminaron `BreachWatchSelectionHalo.cs` y su `.meta`. El realce del destino seleccionado se expresa en el único sprite activo, no en una segunda corona.
- El filigrana de unión es un elemento propio **detrás** de los iconos, con línea y conectores finos como en el referente. No hay dock opaco ni botones superpuestos.

## Coherencia semántica y del juego

| Símbolo | Etiqueta mostrada | Ruta/estado real en la primera pantalla |
|---|---|---|
| Castillo | REINO | Ciudad/Bastión real; salida del Mundo si corresponde |
| Bolsa de provisiones | RECURSOS | Acceso rápido al **Aserradero real** durante Capítulo I; no simula inventario ni abre interfaz heredada |
| Espadas cruzadas | EJÉRCITO | No interactivo hasta existir la nueva pantalla independiente; coincide con `RoyalSection.Army` |
| Cuervo/explorador | EXPLORAR | Mundo, y solo cuando `RoyalSection.World` está permitido por el juego |
| Casco | HÉROES | No interactivo hasta existir la nueva sala independiente; coincide con `RoyalSection.Heroes` |

La ruta de RECURSOS es **un acceso rápido a la estructura de producción inicial**, no debe etiquetarse engañosamente como `RoyalSection.Inventory`. Si se construye una pantalla de recursos independiente en el futuro, sustituir el destino de manera explícita y volver a probar. No reutilizar RoyalHUD.

## Evidencia y límites
- Run genuino Unity: [42581807322](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42581807322), HEAD `e0fed872bde5c04e214ebe2a814f06233ca1b26c`. Compilación y capturas completas 1600×900, 900×1600, 844×390, 390×844; prueba de jerarquía exige **exactamente un Graphic pintado**, ninguno de tipo `BreachWatchGlyph`, y ningún halo antiguo para los cinco botones. Se probaron también los clics reales y se respetó el guardado.
- La inspección del PNG genuino 390×844 confirma que desapareció la doble circunferencia visible en la fila inferior. **No se declara** que el resto de la interfaz haya alcanzado automáticamente la calidad premium del concepto.
- La transformación cromática final del metal pertenece a un SHA posterior, por lo que necesita repetir la prueba de arte/nativa para declararse certificada.
- No fusionar, publicar ni compartir un render conceptual como sustituto de las capturas nativas.

## Criterio de aceptación
El usuario quiere la misma dirección de arte que su captura vertical: **navegación inferior limpia, una sola medalla por función y fidelidad del mundo**, además de legibilidad de paneles, materiales de alto nivel y coherencia jugable. La corrección de la doble capa es necesaria, pero no es por sí sola una aprobación de calidad artística.
