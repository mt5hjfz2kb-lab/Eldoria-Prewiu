# Valoria V10 — cambio de técnica, no maquillaje de V9

Rama aislada: `art/valoria-modular-pbr-v10-20261116`. Estado: **implementado en código; render Blender CI pendiente de revisión; NO aprobado artísticamente; Unity/iPhone no probados**.

## Problema que obliga a cambiar de método

Revisión visual real de V7/V8/V9: V7/V8 tienen un chalet de tejados azules anidados sobre una estructura excesivamente alta y V9 corrige la altura y la cámara, pero presenta una caja rectangular casi lisa. Aunque la generación de .blend/.glb y los renders fueron reales, la familia de piezas geométricas primitivas no consigue el lenguaje comercial estilizado prometido. Añadir más cajas y más huecos era continuar una técnica que no cumplía el objetivo.

## Dirección V10, ejecutable y original

Un pueblo medieval-fantástico para estrategia móvil, construido con *módulos tridimensionales texturizados reales* y una composición autónoma de cinco edificios distintos: Bastión, Aserradero, Granero, Cuartel y Cantera. Mantiene uso claro de la cámara ortográfica, fortalezas a escala más baja y una composición legible en 390 px. Las estructuras, disposición, relaciones, banderas, mecánicas visibles, props, iluminación y terreno se componen para Eldoria. No se copian edificios enteros de un juego comercial.

Piezas modulares de referencia: Quaternius **Medieval Village MegaKit Standard** (fachadas de piedra/yeso y ladrillo irregular, ventanas, gabletes, tejados y props) y **Stylized Nature MegaKit Standard** (árboles, arbustos y rocas). Archivos glTF 2.0 con imágenes BaseColor/Normal/ORM (no solo color plano). Se comprueban las licencias CC0 1.0 al descargar. Se conserva el SHA concreto de la fuente en el manifiesto de render. Fuente pública revisada: `https://github.com/agentkaerf/FreeModels` (autor de modelos upstream: Quaternius).

**Licencia**: CC0 1.0 Universal según `License_Standard.txt` del paquete, verificada antes de usar los modelos. El archivo generado incluirá licencia. Crédito voluntario a Quaternius. No confundir permiso de uso CC0 con que el acabado final sea automáticamente comercial.

## Archivo y proceso

- Generador reproducible: `tools/stylized-direction-pilot/valoria_modular_pbr_v10.py`.
- CI: `.github/workflows/eldoria-valoria-modular-pbr-v10.yml`.
- Fuente CC0 descargada por sparse checkout exclusivamente de esos dos paquetes.
- Resultado esperado: `bastion-v-pbr-wide.png`, `bastion-v-pbr-mobile.png`, `valoria-modular-pbr-v10.blend`, `valoria-modular-pbr-v10.glb`, `QUATERNIUS_CC0_LICENSE.txt` y `visual-proof.json`.

## Criterios de rechazo

1. Un render limpio no basta: los materiales deben mantener relieve, el skyline no puede volver a ser un chalet ni una caja simple.
2. Ningún edificio debe parecer una maqueta de módulos repetidos, elevados sobre plataformas o sin acceso/uso reconocible.
3. Los cinco edificios deben tener función y silueta distinguible, aunque pertenezcan a una única familia artística.
4. Debe poder verse suficiente entorno y juego en cámara vertical, sin edificaciones que tapen toda la escena.
5. Si PBR es visualmente aceptable, aún falta batch/LOD y perfilado móvil, escena de prueba en Unity URP con la cámara real y beta iPhone; **no presentar Blender como prueba de Unity**.
6. **Main no se toca**. No modificar lógica, datos, economía ni combate. El experimento es solo de presentación.

## Estado verificable (antes de ejecutar render)
- Investigado: ubicación de ambos paquetes y licencias CC0; estructuras glTF con texturas PBR.
- Implementado: nuevo generador V10 de edificaciones propias sobre piezas modulares, pipeline CI, contrato de licencia y verificaciones técnicas automáticas.
- Probado: aún no hay prueba visual V10 aprobada ni salida de Unity.
- Pendiente: completar CI, inspeccionar imágenes, corregir composición/materiales, optimizar GLB y probar en Unity/iPhone.
