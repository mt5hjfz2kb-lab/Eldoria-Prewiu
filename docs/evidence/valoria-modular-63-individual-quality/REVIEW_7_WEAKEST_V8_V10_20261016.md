# Valoria — comprobación visual real de las dos últimas pruebas (16-10-2026)

**Ámbito exclusivo:** biblioteca individual 63 GLB. Se ha inspeccionado el PNG nativo de Blender, no solo la conclusión de GitHub Actions. Los originales son inmutables; no se ha ensamblado ni integrado ninguna escena de Unity. Sin aprobación artística automática.

## Cuatro piedras de arco — ejecución #38902465156, artefacto 12047385992

Evidencia: `before/*.png`, `after/*.png`, `glb/*.glb`, `atlas/*_authentic_single_block_CC0.png`, `reports/four_cc0_single_ashlar_stone_faces.json`. Fuente fotográfica de bloques labrados real CC0 Poly Haven. La geometría ya tenía **cuñas correctas** en el origen V7; comprobación numérica independiente sobre cuatro GLB V7: las anchuras intradós/extradós son 0,594 (clave mayor), 0,590 (clave menor), 0,534 (dovela mayor), 0,529 (dovela menor). La nueva intervención es **material**, no geometría.

Se comprobó visualmente que el V8 elimina el falso damero y el aspecto de grava muy porosa de V7. Sin embargo, el fragmento fotografiado da un rayado direccional similar a veta de madera y las aristas no muestran talla suficiente para el objetivo Valoria. No conviene puntuar todo automáticamente en 6 solo por el éxito técnico. **Valoraciones preliminares y conservadoras:**
- `arch_keystone_major`: 5,9/10; progreso visible de material, talla insuficiente.
- `arch_keystone_minor`: 5,9/10; igual.
- `arch_voussoir_major`: 5,9/10; realza las caras, todavía textura de rayas.
- `arch_voussoir_minor`: 5,8/10; borde y caras demasiado uniformes.

**Acción siguiente:** preservar V8 como *candidato de trabajo*, incorporar una fase de acabado artístico auténtico (UV recortado sin vetas direccionales, desgaste solo de cantos, ligero color calizo cálido), manteniendo cuña y la misma cámara. **NO promocionar a floor-6 ni `ART_APPROVED`** hasta otro juicio.

## Tierra — ejecución #38903201305, artefacto 12047023988

Se ha revisado la imagen `dirt_ground_patch.png` antes/después. El sustrato fotográfico es visualmente reconocible; ahora presenta once grupos 3D de hierba, no manchas verdes impresas. El modelo conserva su función repetible como parche de suelo. El coste visual restante es una distribución algo dispersa de matas, textura demasiado plana y falta de pequeños guijarros naturales. **Puntuación tentativa: 6,0/10 (justo en el suelo), `ART_REVIEW_REQUIRED`, NO `ART_APPROVED`.** Mantener la base V10 y pedir validación Unity más adelante. Este aumento es **sujeto a confirmación**; no afirmar que el universo de 63 pasó el umbral.

## Mortero y agua: evidencia revisada de origen

- Mortero V9 [38895731036](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38895731036): funciona como junta 26 mm, pero la macro muestra líneas rectas uniformes de tonos oscuros y pequeñas partículas blancas excesivamente aisladas. **5,7**; requiere rugosidad mineral granular coherente y evaluar el macro, no un zoom tan lejano que no permite verla.
- Agua V9 [38897442996](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38897442996): menor repetición de ondas, pero demasiado azul oscuro/opalino en el render por falta de especulares, transmisión y reflejo contextual. **5,7**; debería aprovechar Shader Graph/URP en validación individual de Unity cuando se autorice, sin composición de fortaleza. No simular un 6 cambiando de color o añadiendo ondas blancas paralelas.

## Seguridad de la biblioteca

Las corridas A/B no modifican la escena, no alteran Main, no instalan herramientas pagadas ni reclaman exclusividad del runner Windows (Bastión I está en otro flujo). Las cuatro cuñas y el suelo se archivan por run ID sin sustitución silenciosa. Fotografías y mallas GLB verificadas; `TECH_PASS != ART_PASS`.

**Recuento de seguimiento conservador:** último libro consolidado 56/63 >=6 provisional, 7/63 bajo 6. Tierra V10 puede pasar a 6,0 sujeto a revisión del propietario; no consolidar en el libro maestro sin decisión confirmada. Persisten mortero, agua y las cuatro cuñas por debajo del mínimo. **Objetivo: 63/63 a 6 y después 8 + Unity.**


## Incidencia sobrevenida — cantería V9 #38905895962

Revisión de los logs y artifact **12047807616**: la tanda V9 falló **4 de 4** al aplicar bevel a la cuña fotográfica, provocando la excepción `Wedge no longer fits original source envelope`. No hay cuatro GLB finales exportados y por tanto no se promociona ninguna piedra de esa ejecución. La mejor evidencia válida sigue siendo V8 #38902465156. La intervención pendiente debe cortar las aristas **hacia dentro** del envelope original, sin alterar los anchos intradós/extradós 0,59/0,53, y volver a renderizar todas las cuñas. La corrección debe incluir comprobación matemática del volumen **antes y después** del bisel, no aumentar arbitrariamente la tolerancia hasta aceptar piezas incompatibles.

## Mortero — nueva preparación sin ejecución forzada

`tools/valoria-modular-fortress/true_lime_mortar_v10_staged.py` commit b691e339c5: conserva el perfil físico de 2,6 cm y plantea corregir granos blancos sueltos, fotografía de cal con PBR normal embebida, misma cámara overview/macro. Código **preparado pero no ejecutado**, y no sirve como prueba artística mientras no termine Blender. Los datos originales V9 siguen siendo el mejor candidato en producción aislada, 5,7 provisional. Otra intervención artística de mortero del mismo repositorio debe coordinarse para evitar trabajo duplicado. 


## Cantería V9 reparada — ejecución #38906155308

Tras el fracaso #38905895962, Blender volvió a ejecutar cuatro biseles **dentro del envelope modular** en la prueba #38906155308 (artefacto 12048097620), con cuatro GLB, ocho PNG y cuatro atlas CC0. Recuento por módulo: 284 caras iniciales, 400/404 caras finales; 120 a 157 vértices de borde reencajados dentro del volumen original; comprobación automatizada `within_previous_fit_envelope=true`. A/B real inspeccionado: fotografía menos lineal, piedra caliza más uniforme y aristas ligeramente trabajadas. **Cuatro valoraciones de calidad mínima aproximada 6,0/10**, no 8, **sujetas a confirmación humana y pruebas Unity**. Se admiten como nuevos mejores *candidatos*, con las versiones anteriores conservadas y no como `ART_APPROVED`. Se mantienen visibles los defectos: poca talla frente a referencia Valoria y cierto aspecto de bloque industrial. Si una prueba de contacto/intradós falla, revertir al anterior.

## Mortero V10 — ejecución #38906245333

Se revisó la vista macro real del artifact 12048128099. Aunque terminó técnicamente, la junta parece **una banda muy oscura con franjas longitudinales grisáceas**; no demuestra una mejora artística sobre la V9. **ART_REJECT / no promoción; puntuación permanece 5,7 provisional**, a pesar de los PNG y GLB exportados. Mejorar contraste mineral no implica oscurecer uniformemente toda la junta ni pintar líneas rectas. Intervenir solo después de estudiar un material de juntas históricas verdadero al tamaño de 2,6cm.

## Corte intermedio

**56/63** es el último recuento consolidado; contando además tierra V10 y los cuatro bloques de arco V9 como candidatos de **6,0/10 aproximado**, el recuento evaluado asciende *provisionalmente* a **61/63**. Falta evidencia visual 6/10 para **`mortar_joint_recess` y `river_water_tile`**, ambos alrededor de 5,7. **0 `ART_APPROVED`**; no alterar Unity ni integraciones de juego hasta nueva orden.
