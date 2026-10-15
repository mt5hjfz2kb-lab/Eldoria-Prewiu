# ELDORIA — Reference-first, zero-budget Unity visual pilot (2026-10-15)

## Mandato y límites
Autorización del propietario en el chat del 2026-10-15: **poner en marcha el plan reference-first** a 0 €, sin crear antes un artista IA autónomo. Es un experimento aislado y no una reanudación global de producción.
- Fuente de verdad: `main/AGENTS.md`, `main/pipeline/active-workstreams.json` y `main/docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md`.
- **Prohibido** modificar Valoria de producción, Region 1, Bastion1Assembly, mundo 4X, publicación o la línea Qwen/Blender. No ocupar `windows-runner-heavy` mientras pertenezca a otro titular. No alterar la pausa global ni autoaprobar arte.
- **No** descargar un paquete ni publicar contenido de terceros sin licencia concreta, origen trazable y permiso de integración/distribución.

## 1. Hechos ya verificados (no suposiciones)
- El código Unity real está en `Unity/` y la versión fijada es **6000.3.23f1**. URP 17.3.0 y glTFast 6.14.1 figuran en `Unity/Packages/manifest.json`.
- Existe un vertical slice Unity de Bastión I-II con QA funcional pasada en evidencias del repositorio; **no** se infiere aprobación artística.
- La referencia vigente **NO** es una captura inventada: `references/VALORIA_APPROVED_VISUAL_REFERENCE.jpg` (702 827 bytes en main al crear este piloto, blob Git `6409a860440cdc937818b18939dcba91ab2e6df8`).
- Se ha inspeccionado visualmente la referencia real: panorámica oblicua a una meseta rocosa rodeada de agua, bosque de coníferas, puerta/puente inferior, sendero escalonado, fortificación superior, piedra cálida, antorchas y luz ambiental azulada, con HUD estratégico.
- Ya existe un modelo 3D importable `Unity/Assets/Eldoria/ArtTests/OriginalHero/valoria_hero_fragment.fbx` (272 492 bytes), y cinco texturas asociadas; EXISTE, pero su calidad aprobada es **NO**.
- También constan `Unity/Assets/Eldoria/Resources/WorldInventory/Tree01A.prefab`, `Rock01.prefab` y `Unity/Assets/Eldoria/Resources/Valoria/SM_Cliffs_01.prefab`; su existencia **no prueba** licencia/ensamblado/render correctos.
- Los trabajos de Bastión I, Región 1, 4X y artista independiente mantienen propietarios; `windows-runner-heavy` aparece reclamado por Bastión I. La pausa global sigue activa salvo este alcance expresamente autorizado.

## 2. Hipótesis falsable
¿Podemos acercarnos a la referencia **antes** con una composición Unity de modelos reutilizados/curados y un acabado visual coherente, que con más ciclos de entrenamiento/modelado IA sobre un solo asset?

No se presupone que la hipótesis sea verdadera. Debe compararse el resultado real, no el éxito de un script.

## 3. Primer experimento implementable
**Estudio aislado** en `Unity/Assets/Eldoria/ArtTests/ReferenceFirstPilot/`, sin modificación de escena runtime:
1. Importar el **modelo existente** OriginalHero, con materiales/UV/texturas conservados.
2. Organizar una composición de estudio con coníferas y acantilados realmente importados, luces y cámara oblicua; el terreno/camino auxiliar se etiqueta como `BLOCKOUT` y no cuenta como asset aprobado.
3. Guardar la escena SOLO en la carpeta propia y obtener PNG real 1536x1024 de vista panorámica y detalle, más manifiesto con los componentes realmente instanciados.
4. Inspección humana/IA independiente de capturas frente a la referencia con errores concretos de composición, silueta, material, atmósfera, legibilidad y coherencia. Informar también sobre contenido faltante.
5. Solo si los recursos disponibles no bastan, seleccionar **un** paquete gratuito con procedencia y licencia comprobadas (CC0 o equivalente permisiva) de Poly Haven, ambientCG o Kenney; contrastar visualmente su integración y documentar fuente/licencia/derivados. Ningún recurso externo se ha aprobado ni descargado en este punto.
6. Medir el esfuerzo desde el comienzo de la prueba hasta escena mejorada, identificar qué elementos sirven para un segundo escenario. **No** fabricar veinte assets antes de esta prueba.

## 4. Gates antes de proclamar avance
- **SOURCE_PASS**: modelos/texturas resuelven y al menos una escena aislada real se guarda.
- **UNITY_RENDER_PASS**: PNG real del editor Unity 6000.3.23f1 en cámara fijada, con las piezas observables, más manifiesto de procedencia.
- **VISUAL_REVIEW**: comparación independiente explícita contra JPG de referencia. Rúbrica permanente canónica de 25 % geometría/silueta, 25 % arquitectura/contacto, 20 % materiales/detalle, 20 % estilo y 10 % cámara. No simular ni inventar una nota.
- **ART_APPROVED** solamente conforme al contrato vigente: A/B real en Unity, diferencia >=0,25/10, sin regresión grave, umbral final 8/10, revisor independiente y manifiesto formal verificable.
- **REUSE_PROVEN**: adaptar con menos trabajo una segunda zona/assets y medir tiempo y calidad; una sola imagen nunca demuestra escala.
- **LICENSE_PASS** obligatorio antes de usar modelos de terceros en una build pública. Los objetos propios existentes sin atribución documental quedan por verificar.

## 5. Estado de arranque real
- `REFERENCE_ANALYZED=true` (JPG auténtico inspeccionado).
- `SOURCE_CANDIDATES_LOCATED=true` (rutas enumeradas por GitHub).
- `UNITY_RENDER_PASS=false`; `VISUAL_REVIEW=NOT_RUN`; `ART_APPROVED=false`.
- `WINDOWS_RUNNER=NOT_CLAIMED`; no se ha lanzado Unity/Blender ni se ha publicado contenido.
- Rama: `prototype/reference-first-zero-budget-visual-pilot-20261015`.
- Siguiente acto físico: ejecutar el nuevo constructor/capturador de escena en Unity **cuando su recurso sea lícitamente disponible**; recuperar PNG y revisar A/B, no hacer otra auditoría del artista Qwen.

## 6. Decisión tras primera captura
Si la geometría reutilizada reduce el tiempo de montaje pero pierde calidad, mejorar exclusivamente la pieza dominante (por ejemplo, arquitectura hero o cliff) mediante recurso nuevo **licenciado** o autoría específica, nunca retocar decenas de objetos a ciegas. Si la composición sigue pobre aunque los assets sean correctos, mejorar primero cámara, luces, escala territorial, densidad forestal y unión entre camino/puerta/meseta. Registrar tiempos de edición y costes reales (0 €).
