# Fase 2 · Kit modular aprobado desde Fase 1

**Aprobación:** el propietario aprobó expresamente Fase 1 el 16/11/2026 y autorizó Fase 2 **solo en la rama aislada** `art/visual-reboot`. Fase 2 permanece pendiente de revisión artística; no implica aprobación de Fase 3.

## Contrato de producción

- Utilizar como **única textura** `Assets/Art_Reboot/Phase1/Textures/Hero_Atlas_256.png` (256 × 256) y colores `Color` por vértice de Blender; no reescribir ni repintar el atlas aprobado.
- Exactamente 3 slots de material reutilizables: `Kit_Surface`, `Kit_WarmLight`, `Kit_Corruption`. Ningún material nuevo por variante. Shader URP `Reboot_Atlas.shadergraph` aprobado.
- 1 unidad Unity = 1 metro. Cuadrícula modular de 4 m (módulos de tejado 4 o 8 m); **origen local (0,0,0) en el centro de la base** de cada pieza. Los tejados tienen origen en el arranque del alero y se colocan a la cota del muro en montaje.
- Arquitectura: muros recto/esquina/puerta/ventana en intacto y dañado (8); tejados a dos aguas/torre/cobertizo (3); torre de campana y torre rota (2); chimenea/balcón/escudo/viga (4).
- Escenografía: pinos pequeño/mediano/grande, tronco caído, matojos, hierba alta (6); roca pequeña/mediana/grande y losa de camino (4); cristales pequeños/medianos/grandes y raíz oscura corrupta (4). **31 prefabs**, agrupados en alrededor de 15 familias reutilizables.
- Biselar aristas prominentes y tallar contornos ligeramente irregulares; evitar repetición geométrica perfecta; variante de color por bloque.
- Regeneración determinista por Blender y comprobación tras reimportar FBX. Generador: `Kit/Source~/author_modular_kit.py`. Manifiesto: `Kit/Source~/kit_manifest.json`, incluyendo triángulos exactos, base, extents, materiales y hash del atlas.
- Importación real y montaje en `Kit/Scenes/Eldoria_Kit_Sandbox.unity`, **escena nueva**. Ninguna dependencia de las mallas completas `Reboot_Hero`, `Reboot_Ruin` y `Reboot_Corrupt` para construir edificios. La Fase 1 y el juego principal no se editan.
- Monte los tres edificios exclusivamente por instancias de piezas del kit: (1) casa intacta con torre de campana; (2) taller intacto distinto, chimenea grande y cobertizo; (3) fortín dañado/corrupto con torre rota. Usar distintos pinos y claros transitables.
- Heredar cámara 42° / yaw −13°, paleta y URP de Fase 1 para la captura principal, afinando sol bajo ámbar, sombras azul-violeta y horizonte ámbar-violeta. Captura separada del **kit alineado**.
- No mezclar con Ashen Citadel, Nocturne, CC0, interfaz Vigilia ni rama main. Solo consumir tiempo Windows después de relevo explícito del registro. No gasto de créditos.
- Gates obligatorios: geometría + UV + vertex color + atlas exacto, base + tamaños, 3 materiales máximo, exportación/reimportación FBX, 31 entradas y polígonos, importación/compilación **Unity real**, captura cuadrícula, captura poblado, lista de polígonos. El éxito técnico **no** implica aprobación artística.
- **Entrega y pausa:** dos PNG originales de Unity y manifiesto; solicitar aprobación expresa del propietario antes de cualquier fase posterior, sin publicación o merge.

## Cómo verificar

`blender -b -t 4 --python Assets/Art_Reboot/Kit/Source~/author_modular_kit.py`, desde el directorio `Unity`; `blender -b -t 4 --python .../verify_kit.py` reimporta los FBX y rechaza alteraciones de pivotes, UV y triángulos. Los procedimientos CI guardan la salida `BLENDER_KIT_PASS` y `UNITY_KIT_CAPTURE_PASS`, con run exacto y capturas verificadas, nunca renders conceptuales.
