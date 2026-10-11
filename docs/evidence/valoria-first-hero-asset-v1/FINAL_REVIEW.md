# Resultado de fuente — una sola puerta fortificada

La pieza entregada es un recurso 3D real, editado en Blender y exportado a GLB. No es una imagen generada ni una aprobación inferida de CI. Se eligió reconstruir la puerta compacta de torres cuadradas de la referencia aprobada, reutilizando el procedimiento y herramientas del estudio. Se abrieron las imágenes históricas V2–V8 recuperadas por commit, además de la referencia completa.

## Evaluación artística

**Mejora clara frente a V5/V8:** silueta compacta, proporción defensiva coherente, familia de piedra de tonos próximos, superficies de escala consistente, portal realmente atravesable, rastrillo con huecos, puertas de roble abiertas y banderas azules. El cuerpo tiene respaldo continuo; las piedras aportan relieve sobre una estructura, no piezas sueltas sin conexión. Se conservó una sola arquitectura después del primer render.

**Límites reales:** la mampostería sigue siendo bastante regular y limpia respecto del envejecimiento más rico de la referencia. Las llamas son una representación estática; no se entregó un VFX de fuego. La heráldica es una estilización propia. La vista trasera tiene menos ornamentación. La iluminación de estudio no acredita la iluminación de Unity. Mi decisión es conservarla como candidata hero para la prueba exacta dentro del motor; no declaro paridad comercial con la referencia ni biblioteca final certificada.

## Validación ejecutada

- Blender 4.2.5 LTS oficial, tar Linux verificado por SHA256.
- Fuente .blend editable con geometría independiente y texturas empaquetadas.
- GLB reimportado en Blender; UV y materiales presentes; comparación visual del archivo exportado.
- Comparación V5 y recurso nuevo con cámara e iluminación iguales, normalizando solo anchura de V5.
- LOD0: 113.612 triángulos. LOD1: 62.146. LOD2: 32.126. 14 materiales. LOD2 conserva banderas y rastrillo tras corregir la simplificación. Todavía no hay medición física Android/iOS.
- BlenderMCP auténtico, addon fijado por commit/hash, TCP y cola de comandos sobre el hilo principal: 44 piezas de almenas asentadas y 36 piedras de esquina con menos relieve. El informe registra respuesta success y cambios concretos.
- Puente del estudio ejecutado: 14 fuentes staged fuera del repositorio, SHA e137aaf713c682509cacc62cd1b3279c140a811c.
- Política de contradicciones recuperada ejecutada: sin conflictos. Es una comprobación de consistencia, no juez artístico.
- Ollama local consultado: conexión rechazada. Qwen no ejecutado. No se gastaron créditos ni se utilizó otro servicio de IA de pago.
- Superficies de autoría original para Eldoria: albedo, normal y roughness de piedra y roble, más imágenes portables extraídas del GLB. No se atribuye a CC0 algo que no se utilizó.

## Unity: bloqueo concreto pendiente

No existe Unity Editor en el entorno local. Se comprobó el registro vivo de main y los runs reales. El runner Windows está reservado por valoria-mesh-only-worldspace-proof-20261010, aunque no haya un job pesado ejecutándose. No se invadió esa reserva. La ruta histórica real de import/capture/reload está identificada en REPRODUCE.md, y se conservan hashes exactos para repetirla cuando se libere el recurso. No hay capturas de Unity de esta pieza; no se afirma integración ni persistencia verificadas.

## Trazabilidad

Rama: art/valoria-first-hero-asset-v1-20261011.
Claim: 0c66b3ab6279c455723b5eab4fcf06611e1ea667.
Primera fuente: 2eb17d1c50bedb5127793daaf6de5581432e879c.
Fuente corregida seleccionada: 9ca81653880395f13acd2ed1fc806fcd5f3a7155.
Primer run: 38145567917 — fallo por distribución de Blender sin OpenImageDenoiser; corregido con la distribución oficial verificada.
Run seleccionado de reproducción: 38145882931 — SUCCESS. Artefacto 11704772340. Fuente y renders conservados en 12a1405d4603bd144ffe2f9a559ea6c6e968d3b7. Se descargaron los archivos de ese commit, se cotejaron los SHA256 de LOD y se abrieron los renders exactos de CI. Consultar RUNS.json; no se reutilizó el PASS de otro candidato.

main, gameplay, publicación y la rama V5 se conservaron. El chequeo global de workflows detecta un fallo preexistente en valoria-blender-mcp-windows-proof.yml (auto-trigger desde su propio archivo); no se alteró ese workflow de otro alcance ni se declara PASS global.
