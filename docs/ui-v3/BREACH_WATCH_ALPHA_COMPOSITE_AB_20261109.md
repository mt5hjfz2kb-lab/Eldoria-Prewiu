# Vigilia — A/B real del retrato integrado y fin de doble marco
Fecha: 2026-11-09. PR canónica: #173, workstream `eldoria-forja-oscura-visual-proof-20261107`.

## La pregunta de arte
El usuario señaló correctamente que la nueva interfaz parecía una colección de objetos puestos sobre otros. Las pruebas anteriores demostraban que el viejo Canvas estaba deshabilitado, pero seguían existiendo una ficha mineral, contorno dorado, un marco de retrato visualmente opaco y una miniatura rectangular oscura. **Un pase técnico no certifica una pantalla premium.**

## Correcciones técnicas implementadas y pruebas reales
1. **Run [42628538160](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42628538160), SHA `387f6fd3c025ec5cc81fa71d5e86a5eed0c0d3ca`**: reduce la profundidad/alpha de las tres sombras, disminuye la expansión horizontal del único contorno, conserva ±19u de despeje vertical para no cruzar título ni CTA, reduce el tamaño/contraste del sello de edificio y matiza el marco secundario al 55%. Unity 8/8 PNG, 6 clics, partida intacta, sin HUD legado; cambio visual total limitado (aprox. 2,40% de la captura 900×1600 seleccionada).
2. **Run [42629773409](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42629773409), SHA `18356c07cfe0e5dfacc9a41fddf104fd2b66be4a`**: prueba un camino distinto: la cámara URP aislada pinta las mallas 3D reales sobre fondo RGBA transparente, no sobre una segunda fotografía rectangular. `RenderTextureFormat.ARGB32` y lectura RGB**A** real de Unity: 72,786% píxeles de fondo transparentes, 26,882% de píxeles opacos pertenecientes a la geometría. Unity 8/8 y 6 clics PASS. Sin embargo, la transparencia hizo resaltar un **segundo marco dorado completo debajo de la miniatura**: técnicamente correcto, visualmente todavía ensamblado. No aprobado artísticamente.
3. **Run [42630398578](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42630398578), SHA `36f05a596e243c0eb52b20907187a925a9dab87d`**, [artefacto original #13680344022](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/42630398578/artifacts/13680344022): conserva recorte transparente de la auténtica cámara, rebaja la segunda placa de 55% a 18%, mantiene un único contorno exterior y datos/clics reales. Unity 6000.3.23f1 nativo 8/8 PNG, 6 clics, guardado intacto y assets auténticos, **PASS técnico, ART_APPROVED=FALSE**.

## A/B cuantitativo sobre píxeles reales
Umbral RGB absoluto mayor que 12, comparando **run 42630398578 vs run 42628538160**, versiones seleccionadas, misma escena:
- Ciudad sin selección: **0% de píxeles cambiados** en las cuatro resoluciones (ninguna mutación del terreno, edificio, luces, cámara o barra de jugador).
- Bastión seleccionado 1600×900: **0,398%**, cambios confinados al retrato, caja x1311..1560 / y229..347.
- Bastión seleccionado 900×1600: **0,603%**, caja x597..857 / y912..1056.
- Bastión seleccionado 844×390: **0,416%**, caja x638..717 / y116..186.
- Bastión seleccionado 390×844: **0,718%**, caja x233..367 / y486..561.

Contra la iteración de marco al 55%, la variante al 18% limita también los cambios exclusivamente al retrato. Los scripts `compare_native_art.py` y `test_compare_native_art.py` preparan JPG de comparación desde capturas **genuinas**; la prueba del comparador funciona en Ubuntu sin Windows ni render inventado.

## Dictamen de calidad y lo pendiente
- Se ha quitado el rectángulo opaco de la fotografía y reducido el segundo marco; el Bastión 3D sigue siendo auténtico y independiente del mundo. **La mejora es real pero visualmente limitada.**
- El problema artístico residual no se resuelve multiplicando sombras, halos o retocando uno a uno los biseles; para alcanzar el referente se necesita una revisión autoral **del conjunto de panel, retrato, barra superior y lenguaje del mundo**, conservando widgets dinámicos y un solo Canvas. No atribuir una nota de calidad arbitraria a una simple prueba de código.
- El arte no está aprobado, PR sigue en borrador, no fusionar ni publicar, falta prueba de APK/WebGL/Android físico.
- `windows-runner-heavy` liberado formalmente al terminar el run 42630398578 (registro main, ownership_revision 26). No hay automatizaciones nuevas de supervisión.
