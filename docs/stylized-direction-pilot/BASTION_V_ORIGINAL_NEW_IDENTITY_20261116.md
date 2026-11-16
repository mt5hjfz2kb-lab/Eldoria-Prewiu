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

## Resultado comprobado — render original Blender V5 (16/11/2026)

- **Ejecución real y completada:** [GitHub Actions #43931187213](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43931187213) **SUCCESS**, fuente exacta `4b58ce2bd3cc327ab9b3119ebe598eac4913d202`.
- **Artefacto original:** `14258606381` (ZIP `valoria-original-from-zero-4b58ce2bd3cc327ab9b3119ebe598eac4913d202`), contiene PNG paisaje 1440×900, PNG retrato 800×1400, fuente editable `.blend`, `.glb` físico y `visual-proof.json`. Archivos descargados e inspeccionados desde sus bytes originales.
- **Inspección GLB 2.0 realizada:** 1.146 meshes, 19 imágenes/texturas embebidas, sin importar los modelos artísticos anteriores de Eldoria ni assets comerciales.
- **Evolución real:** V1 #43927228616 generó una ciudad original, pero las cubiertas estaban abiertas y pareció maqueta; V2 #43928780724 cerró tejados y talló aleros; V3 #43929190007 añadió patio defensivo y oficios; V4 #43930367413 añadió texturas reales pero su tratamiento de color doble y césped repetitivo empeoraron la imagen; V5 corrigió la conversión gamma, atenuó la variación del terreno y aumentó el tamaño de la superficie física para que desapareciera el contorno artificial del mapa en el encuadre.
- **Evaluación artística HONESTA de V5:** mejora substancial de legibilidad y material frente a la versión inicial, y arquitectura original totalmente distinta de `main`. Aún se aprecia repetición diagonal en las superficies vegetales, árboles geométricos, edificios secundarios de acabado modesto y falta animación/vida. **No afirmar acabado premium, ni calidad igual a Kingshot / Top Heroes, ni aprobación artística.** `ART_APPROVED=false`.
- **QA pendiente:** importar el `.glb` real en Unity con escena y cámara idénticas a las del juego, inspeccionar siluetas y materiales URP, verificar controles, estados I–V, performance en iPhone y optimizar 1.146 meshes; la clase `OriginalIdentityUnityAB.cs` es fuente experimental y no constituye prueba compilada.
- **Decisión:** conservar el trabajo original Blender V5 como primer candidato visible; no mergear `main`, no publicar beta, no borrar alternativas previas ni confundir captura de Blender con Unity real.
