# Eldoria — nueva identidad original 3D para Bastión V

Workstream-ID: eldoria-stylized-direction-proof-20261115
Rama existente: feature/eldoria-stylized-direction-proof-20261115
PR existente: #196
Fecha: 16 de noviembre de 2026

## Mandato creativo vigente

El propietario rechazó expresamente retocar, recolorear o reaprovechar los modelos del juego existente. Se construye una alternativa artística **completamente nueva**, empleando como inspiración de técnica comercial la legibilidad móvil de Kingshot, Whiteout Survival y Top Heroes, pero **sin copiar modelos, personajes ni escenas de esos juegos**. El tono y el lore de Eldoria siguen presentes. Ninguna parte de la escena nueva puede presentar como fuente de geometría el Bastión vetado ni los GLB heredados.

## Implementación real

Generador nuevo: tools/stylized-direction-pilot/valoria_original_bastion_v.py
Workflow Blender hosted: .github/workflows/eldoria-stylized-blender-vertical-slice.yml

La fuente programática crea desde cero:
- Fortaleza V original, asimétrica, con capilla/campanario, muros con piedra biselada y grandes cubiertas azules de alero pesado.
- Aserradero con viga/rueda/logística de troncos; cuartel con torres y campos de instrucción; granero con silos; cantera con grúa y depósito. Edificios reconocibles sin etiquetas.
- Terreno ondulado continuo, camino principal y secundarios anclados, río con puente, vegetación de siluetas irregulares y Brecha violeta lejana.
- Materiales originales coherentes: piedra cálida, pizarra azul, madera tostada, bronce, bosque desaturado, lámparas ámbar.
- Cámara ortográfica 3/4 y vistas horizontal/vertical de la geometría verdadera, nunca un PNG conceptual convertido en escenario.

Artefactos previstos al pasar el workflow: valoria-original-identity.blend, valoria-original-bastion-v.glb, original-bastion-v-landscape.png, original-bastion-v-portrait.png, visual-proof.json.

## Validación obligatoria

1. Blender CPU real: script sin errores, modelos 3D y fuente editable, capturas verdaderas, hashes y manifiesto. Estado técnico distinto de criterio artístico.
2. Inspección de las dos capturas a tamaño natural. Si se perciben cajas o una maqueta sin riqueza, pivotar en modelado arquitectónico y no hacer retoques de color.
3. Importación del GLB nuevo en Unity URP en escena/sandbox aislados: comparar con Bastión V de main a misma cámara, comprobar materiales, escala, suelo, interacción y legibilidad de móvil.
4. Evaluación de memoria/GPU/iPhone: no deducir FPS a partir del número de objetos de Blender.
5. Diseñar después la progresión I-IV bajo este mismo sistema, sin cambiar jugabilidad, guardados, UI ni mundo publicados hasta aprobación explícita.

## Estado honesto

- Creado: fuente original y workflow para render Blender.
- No afirmar calidad alcanzada hasta inspeccionar el PNG real; ART_APPROVED=false.
- No afirmar integración Unity ni ejecución móvil sin sus pruebas.
- main y la beta permanecen sin cambios. Gasto cero. La PR sigue en borrador.
