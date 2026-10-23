# Ribera: continuación de D, 23 de octubre de 2026

Trabajo en PR #100, sin ampliar Región 1, sin cambios en main, sin publicación y sin gasto. D se conserva: commit `7d04b80f752ec4d12c1d69640a9c5300441cea41`, ejecución `39947565310`. La aprobación artística corresponde al propietario.

## Intervenciones observadas y decisiones

| Revisión | Evidencia Unity | Decisión visual |
|---|---|---|
| D | 39947565310 | Base recuperable elegida por el propietario. |
| E | 39949447651 | Conservar agua con profundidad/corriente pintadas y cambios interiores de la orilla. Rechazar el suelo de manchas angulares. |
| F | 39950025008 | Rechazar suelo con aspecto de camuflaje y copas suavizadas demasiado globulares. Redondear el patrón no corrigió su organización. |
| G inicial | 39950925399 | Rechazar: textura negra en Unity, aunque 18 tests funcionales pasaron. |
| G corregida | 39951461473 | Textura visible, pinos de D recuperados, mayor detalle de superficie. Aún parece una pradera continua y conserva seis grupos casi idénticos. |
| H | 39952493569 | Conservar agrupación asimétrica y sotobosque. Rechazar los claros pintados: demasiado aislados y redondos. |
| I | 39953231775 | Conservar mejora visible sobre D: materialidad, agrupaciones menos repetidas, talud interrumpido. Objetivo 7–8 aún no alcanzado. |

La textura negra de G se corrigió estableciendo el espacio de color de la imagen ANTES de escribir sus píxeles en Blender. Cambiar ese orden recuperó los colores en la captura real de G corregida. Las pruebas funcionales no detectaban el defecto: evidencia concreta de por qué no equivalen a aprobación artística.

## Autoría y representación

E modifica las secciones interiores de las mallas de orilla existentes y pinta un mapa de agua con gradiente transversal de profundidad y trazos de corriente. No sustituye el río por otra geografía.

F extrae las copas heredadas, aplica remallado voxel, suavizado y simplificación nativos de Blender; conserva los troncos y archiva el original. La ejecución fue real, pero su resultado visual se rechazó. No se contabiliza como mejora por haber utilizado herramientas profesionales.

G recupera el original de los pinos y el mapa pictórico de D. Reutiliza `Unity/Assets/Mega Fantasy Props Pack/Textures/grass_1_fixed_d.png`, ya presente en el proyecto: filtrado, rango tonal limitado y mezcla sobre máscaras existentes. No compra ni genera recursos mediante servicios externos. La superficie se exporta como imagen UV, editable y empaquetada en el .blend. No se afirma haber realizado pintura manual con ratón: las operaciones se ejecutan con bpy y matrices de pigmento.

H reutiliza los mismos 18 pinos de los seis grupos centrales y les asigna composiciones diferentes: cinco, uno, cuatro, tres, dos y tres ejemplares. Mantiene los otros pinos. Nueve curvas Bézier editables definen bordes de pradera y claros, vinculados a esas agrupaciones. Añade hojas curvas de sotobosque reunidas en una malla con tres materiales compartidos. No son nodos jugables. La organización se evalúa mediante las cámaras existentes, no por el número de operaciones.

La dirección de materialidad cambia: dejar de cubrir todo el suelo de manchas de igual escala; separar claros y bordes de bosque y añadir detalle de superficie subordinado a esas formas. No debe confundirse más frecuencia de textura con mayor fidelidad pictórica.

## Alcance de la integración

Este trabajo sigue siendo el estudio optativo de ribera, importado por glTF y representado dentro de la escena integrada durante PlayMode. No está instalado como sustitución permanente del mapa. El capturador oculta temporalmente los renderers anteriores, conserva objetos, identificadores y colisionadores, y los restaura al terminar. No modifica guardados ni combate ni UI. No se presenta como una nueva versión publicada del juego.

La representación conserva árboles cercanos 3D y atlas de impostores horneado desde esas mismas mallas para distancia. El zoom se recorre con 97 capturas de Camera.Render: 34→52→34. Es evidencia de continuidad de representación, no una prueba de FPS real ni rendimiento móvil físico.

## Comparación y límites

Antes/después usa posición `(1018,132,-115)`, rotación quaternion `(0.35737279057502749,0,0,0.9339618682861328)`, tamaños ortográficos 34,40,46,52, resolución 1280×960. La comparación con el concepto utiliza el fragmento original de `01_vista_cercana.png`; evalúa estilo y geografía, sin equivalencia pixel a pixel y sin inventar edificios o enemigos del dibujo.

No se modifican los PNG canónicos. El render de Blender se identifica como tal: no se presenta como captura Unity ni como comparación de cámara idéntica.

I conserva las partes revisadas por separado, recupera la superficie de G y modifica vértices de tres tramos del talud existente para abrir pendientes hacia el río. Reasienta pinos, piedras y sotobosque afectados; recalcula AO nativo de Cycles. Las curvas y máscaras rechazadas de H se conservan editables en la fuente, pero no alimentan el material visible.

El primer intento H (39952135920) se detuvo antes de exportar porque no existía el nodo por nombre `Principled BSDF`. Se corrigió construyendo explícitamente el grafo de material. No produjo evidencia Unity ni se cuenta como revisión visual.

## Veredicto visual de I

Valoración razonada, subjetiva y no calculada por píxeles: **6–6,5/10**, frente a la base D de aproximadamente 5,5/10. No equivale a ART_APPROVED ni a haber alcanzado el hito 7–8.

| Criterio | Nota orientativa | Observación en las capturas reales |
|---|---:|---|
| Superficies | 6,5 | Mayor lectura material y menos zonas lisas; grano aún más naturalista que pictórico. |
| Orillas y relieve | 6,5 | Tres accesos de pendiente suave interrumpen el talud continuo. Todavía queda un corte demasiado regular. |
| Bosques y sotobosque | 6 | Seis agrupaciones diferentes sustituyen seis tríos iguales; siguen dominando copas de una sola familia. |
| Agua y caminos | 6 | Profundidad y corriente más integradas; el río sigue siendo una cinta demasiado regular y los caminos carecen de desgaste localizado convincente. |
| Estilo frente al original | 5,5 | Falta la jerarquía de tierra/pradera, luz cálida y detalle por grupos del original. Más textura no resuelve esa distancia. |
| Continuidad de zoom | 7,5 | Misma posición geográfica, atlas derivado de las mallas, cuatro escalas y recorrido de 97 fotogramas; no se observa cambio de mapa. |

Las mejoras aprovechables se conservan; los acabados rechazados no se promueven a finales. No ampliar Región 1 sobre la premisa de que el estilo ya está resuelto. La próxima mejora debe resolver la materialidad pictórica de bordes y claros con escala intermedia, evitando tanto el césped uniforme como los grandes óvalos o el camuflaje. La reutilización de modelos alternativos gratuitos del proyecto puede estudiarse para aumentar familias de vegetación, pero no se afirma haberla ejecutado aquí.

## Verificación exacta

- Commit artístico I: `14d134cf4c719824a6db6a28761556bebdbbf8c8`.
- Ejecución nativa: https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/39953231775 . Ambos jobs source y Unity terminaron en success.
- NUnit: **18/18 Passed**, sin eliminar ni debilitar tests.
- Artefacto: `12501023229`, `region1-canon-native-39953231775`.
- ZIP SHA256: `0cbd84302c20a9590378e4ce9a50958b5aad8edec01f2be8bb2e506ccd59940c`.
- BLEND SHA256: `878f14116db4159b0b354dbff8b97e47a9d92180d345eb7fe4e728b530599642`.
- Cuatro cámaras comparadas por metadatos contra la base: coinciden posición, rotación y zoom. PNG 1280×960.
- Secuencia: 97 PNG auténticos Unity, 960×720, zoom ida/vuelta. Vídeo a 16 fps, no benchmark de rendimiento. Primer/último PNG idénticos.
- En esta intervención de ribera se verificó 4:3. No se declara validación móvil física ni un nuevo pase vertical específico del fragmento.

Los .blend contienen imágenes empaquetadas, mallas editables, máscaras, curvas y el historial preservado. El paquete reúne fuentes finales, recursos GLB/PNG, capturas Unity, fuentes de revisiones descartadas, originales canónicos y hashes de cada archivo.
