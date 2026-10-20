# Valoria — referencia visual canónica aprobada (20-10-2026)

**AUTORIDAD ARTÍSTICA DE LA COMPOSICIÓN Y EL ALCANCE INICIAL.** Esta decisión del propietario sustituye, para la escena temprana de Valoria, las referencias artísticas antiguas con grandes acantilados, terrazas monumentales y cascadas, y cualquier propuesta posterior de rellenar vacíos con construcciones no aprobadas. No modifica las mecánicas ni el calendario de desbloqueos de Bastión I–II.

## Discrepancia material confirmada en GitHub (20-10-2026)
- Archivo antiguo todavía presente: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg`, **blob SHA-1 de Git** `6409a860440cdc937818b18939dcba91ab2e6df8`, SHA-256 histórico registrado en la Biblia `8ae6fb0e6949dd4f7d37b38767282e5f1fb6089e117a29f362945c1edfa66689`. Se ha inspeccionado: enseña **fortaleza sobre grandes acantilados, escalones de terreno y tiendas/otros edificios**; NO es la imagen actualmente aprobada.
- El JPEG del propietario es diferente, **SHA-256 exacto** `86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d` y **blob SHA-1 Git esperado** `4a5a77e970bae4be38c43c48a35cdb8e82d27547`. No declarar sustituido el binario ni ART PASS hasta verificar que el archivo de referencia de GitHub coincide exactamente con ambas huellas. Por ahora el original solo está adjunto en conversación, no consolidado como binario en GitHub.
- Durante el desacople, `docs/DOCUMENTATION_INDEX.md`, `docs/VALORIA_VISUAL_BIBLE.md` y `AGENTS.md` deben guiar a este canon y **tratar el JPEG histórico como fuente no autorizada**. La fuente anterior se conserva para auditoría hasta la sustitución exacta; no mezclar ambos objetivos.

## Imagen exacta aprobada
- Fuente: imagen JPEG proporcionada por el propietario el 20-10-2026, ratificada de forma expresa como «la referencia» de Valoria.
- Dimensiones exactas: **1448 × 1086 píxeles** (vista 4:3, perspectiva isométrica alta).
- **SHA-256 de los bytes JPEG originales:** `86fbffa9410514e935b121a844802573cbb7f94c73abb7358ce2f8ee15845c5d`.
- **Importante:** este fichero de reglas registra la referencia y su huella de identidad, pero no incorpora el binario JPEG al repositorio. No afirmar que la imagen visual está almacenada en GitHub hasta confirmar su importación exacta. Nunca reemplazarla silenciosamente por otra captura, un render generado o una imagen histórica parecida.

## Rasgos obligatorios observables en la referencia
- Bastión compacto central al fondo, arquitectura medieval de piedra clara, cubiertas azules, banderas azules y antorchas. **No** gran ciudad amurallada ni fortaleza monumental extendida.
- Puerta de acceso fortificada delante y **puente de piedra** sobre un **río azul** con orillas de piedras naturales. Camino empedrado continuo que enlaza la puerta y el Bastión.
- **Cuatro parcelas de tierra vacías** bien definidas alrededor del eje principal (dos a la izquierda y dos a la derecha). Son reservas de progresión; no autorizan viviendas, barrios, arcadas, andamios ni edificios no desbloqueados.
- Pradera verde suavemente ondulada; coníferas dispersas, matas, rocas pequeñas, ruinas bajas/fragmentos de muro y pequeñas pendientes. **No** acantilados verticales, taludes masivos, cascadas ni grandes terrazas rocosas alrededor del Bastión.
- Sin tiendas ni campamentos enemigos visibles en esta ciudad. Los encuentros Corruptos y Engendro siguen siendo mecánicas/nodos del mundo, nunca justifican crear campamentos 3D por intuición.
- Lectura isométrica con la ciudad, acceso, parcelas y ribera a la vista; evitar superficies vacías artificiales, formas flotantes, repetición de prefabs y objetos que oculten botones/recorridos.

## Jerarquía obligatoria en cualquier trabajo de arte
1. **Esta referencia aprobada determina la apariencia, volumen y composición** de la escena temprana de Valoria. El vertical slice web `v0220` y los planos de progreso determinan qué existe y cuándo se desbloquea. Las instrucciones previas de acantilados/gran distrito urbano quedan obsoletas como objetivo de esta escena.
2. La técnica de buenas rocas/acantilados con bases CC0 fuertemente modificadas en Blender es reutilizable **solo como método**, por ejemplo para piedras y contacto de la ribera; NO es autorización para colocar grandes acantilados.
3. Recuperar disciplina de bibliotecas históricas de 8 y 63 assets: survey espacial, dimensiones XYZ y en pantalla, licencias CC0 verificadas, fuente .blend, modificación de malla/UV/material en Blender, exportación y A/B en Unity desde la misma cámara. No generar nuevas categorías de edificios para rellenar.
4. Rechazar visualmente cualquier candidato que rompa estos rasgos aunque compile, supere CI o tenga más triángulos. Evitar mejoras microscópicas cuando no muevan composición/calidad perceptible; explorar otras fuentes/técnicas.
5. Metas **no certificadas todavía**: Valoria >= 7/10 y Mundo 4X >= 5,5/10 tras revisión independiente de PNG de Unity. Conservar funcionalidad, guardados, hotspots, presupuesto 0 €, no publicar sin orden.

## Prevención de otra pérdida de la referencia
Toda solicitud/tarea de producción o revisión artística debe enlazar **este documento**, declarar la versión de Bastión y proporcionar la imagen JPEG idéntica por SHA-256 al artista/revisor. Si el binario no está accesible, **no declarar conformidad visual con la referencia**, aunque pueda continuarse trabajo técnico sin riesgo. Toda referencia nueva necesita aprobación expresa del propietario y revisión de este archivo; las antiguas son historia/técnica, no autoridad artística.
