# Eldoria — auditoría de assets y producción visual

**Alcance:** archivos canónicos `v0220/`, `docs/MVP_ART_DIRECTION.md` y output de `tools/build-preview.mjs`; no se han atribuido licencias de producción a imágenes/código de terceros ni se han examinado ramas históricas como fuente. `v0220/` ocupa ~684 KB: `index.html` ~504 KB, `js/` ~172 KB, `runtime-hotfix.js` mínimo. **No existe** un directorio de modelos 3D, texturas de escenario, personajes riggeados, sprites de producción o librería de audio separada dentro de `v0220/`.

| Asset/lenguaje actual | Evidencia | Reutilización adecuada | Estado/licencia |
| --- | --- | --- | --- |
| Retrato de Sir Aldric | JPEG incrustado como base64 `ALDRIC_PORTRAIT` en `v0220/index.html`; figura de Salón de Héroes | Referencia de carácter/silueta y continuidad narrativa; exportar para evaluación visual solo tras rastrear procedencia | **Provisional**; no asumir permiso de uso comercial o fuente editable. |
| Lyra/Maelis | Silueta, emoji/texto y diálogo en `index.html`; stats/roles en `hero-army.js` | Descripción de identidad, rol y presencia; no asset final reutilizable | Se requieren concepto, retrato y personajes/animación nuevos. |
| Valoria, mundo, Brecha | Paisaje y edificios por capas HTML/CSS, gradientes, símbolos y efectos de `index.html`; directrices en `docs/MVP_ART_DIRECTION.md` | Semántica de contraste, hitbox, composición de escenas, color y evolución de ruina a reconstrucción | No portar DOM/CSS literal a Unity; producir kit 3D/2.5D y materiales. |
| Cartas Relicario | Glifos/emoji, marcos/rareza CSS, nombres, N/S/E/O y catálogo `relicario-system.js` | **Datos**, rareza, proporción/perímetro, reveal y jerarquía como referencia | Ilustraciones, iconografía propia y UI assets nuevos; no tomar emoji como arte licenciable. |
| UI/iconos | HTML/CSS + favicon SVG inline + `data-testid`; categorías de Códice/Arcón/Relicario | Mapa de información y copy, no los píxeles; diseñar sistema escalable/adaptativo | Favicon provisional; confirmar derechos de fuente tipográfica si se incorpora alguna fuera de sistema. |
| Audio | WebAudio/osciladores en `js/v030-depth.js` y ceremonia de Reliquia en `relicario-system.js` | Intención de feedback/ambiente, no masters audio | Crear/grabar/adquirir SFX y ambiente con créditos/licencia registrables. |
| Animación/VFX | CSS y DOM para mejora, viaje, golpe, recompensa, grieta y ceremonia | Guion de tiempos/estados útiles para art direction | Nuevos clips de animación, prefabs, partículas y shaders optimizados móvil. |
| Datos/copy | `gameplay.js` capítulos, enemigos/costes; `hero-army.js` perfiles; `relicario-system.js` cartas; diálogo/tutoría incrustados | Extraer IDs, reglas, progresión y tono a catálogos versionados ES/EN | Revisar contradicciones históricas y textos antes de importar. |

## Lista de producción por prioridad

**Primer art spike (slice):** 1 kit ruinas de Valoria modular (Bastión, puerta, Aserradero dos estados), 1 kit frontera (bosque, piedras, camino, tierra corrupta), 1 Aldric visible con rig simple, arqueros agrupados, 1 silueta enemigo corrupto, cielo/horizonte y un foco de Brecha, iluminación en dos estados, VFX mejora/pulso/impacto/recompensa, 1 set UI de 6–10 iconos funcionales y 3–5 señales sonoras. Generar variantes LOD/texturas desde una misma fuente; especificaciones definitivas tras perfilado móvil.

**Al ampliar a I–X:** Granero, Cuartel, Cantera, Forja, Hospital con niveles/estados; Lyra, Maelis, fauna, Acechador/Devorador/Heraldo, Nareth, Fisura, equipo, Reliquias y sus cuatro rarezas, cartas/Práctica, escenas narrativas y audio contextual. Estos paquetes deben derivar del diseño de la slice, no bloquear verla pronto.

**Inventario/licencias antes de uso real:** `assetId`, autor/proveedor, URL/contrato, licencia exacta y permiso de redistribución, formato fuente, derivados, cambios y atribución requerida. Si no se conoce el origen del retrato Aldric, crear sustituto original antes de distribuir una build Unity pública. No comprar paquetes ni asumir que generar arte convincente consiste en aumentar CSS/partículas. Mantener juntos concepto, prefab, textura/material, animaciones, fuentes y registro de licencias.

**Guía canónica:** ruina imperial vertical compacta, fantasía medieval oscura estilizada semirrealista, calor de reconstrucción vs violeta/magenta de La Brecha; UI adulta y mundo dominante. `docs/MVP_ART_DIRECTION.md` es objetivo de producto, no instrucción de copiar los píxeles web.
