# Auditoría de nitidez 2.5D — 17 noviembre 2026

## Evidencia auténtica, sin extrapolaciones

- Fuente A: Unity Editor, RenderTexture original 1170×2532, workflow [44013497619](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44013497619), `3d4d2b4`, importación CompressedHQ.
- Fuente B: Unity Editor, RenderTexture original 1170×2532, workflow [44018257414](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44018257414), `f1d3e6e`, importación **Uncompressed** + sin mipmaps; 12 PNG con SHA verificados, 4 sprites importados auditados.
- Misma escena, cámara y resolución de salida para el par de referencia. Diferencia absoluta promedio RGB en Bastión ≈0.022/255; en Cuartel ≈0.711/255; Granero ≈0.558/255. El cambio en varianza del Laplaciano: Bastión **+0.04 %**, Cuartel **+0.48 %**, Granero **+0.47 %**, terreno **+0.01 %**. Mediciones sobre recortes equivalentes; son indicadores de cambios locales de bordes, **no una puntuación de calidad percibida**.
- **Conclusión validada:** la compresión no explica por sí sola la suavidad que se observa en varios edificios. Hace falta mejorar o reautorizar el arte fuente y considerar su tamaño proyectado en pantalla. No se ha demostrado una ganancia visual grande mediante un cambio simple de importación.

## Experimento siguiente (no aprobado automáticamente)

- Se ha añadido `City2DArtBake.cs`: genera **solo en el sandbox** cuatro copias RGBA `Generated/*_Crisp.png` desde los PNG existentes, con contraste de bordes alfa-consciente de intensidad moderada. Los originales permanecen intactos. No aumenta la información real que pueda contener la fuente.
- `City2DSceneBuilder` enlaza esas copias a Cuartel, Aserradero, Granero y Cantera, conservando Bastión e interacciones y la progresión de parcelas. `City2DNativeEvidence` audita los assets importados y genera además una captura de detalle con **zoom nativo 1,42×** a 1170×2532 (no un zoom de imagen en Photoshop).
- La prueba nativa debe completar **13 capturas con SHA** y controles de snapshot/read-only, y marcar explícitamente `art_approved=false` y `device_qa=PENDING`. Si se detectan contornos artificiales, ruido o efecto de «pegatina», **revertir o reducir** el tratamiento sin tocar el arte principal.
- Cinco ilustraciones de edificios de mayor riqueza arquitectónica se han creado **fuera de Unity como candidatos** (Cuartel, Aserradero, Granero, Cantera y futuro Salón de Héroes) y mantienen un ZIP de revisión. **No se han instalado en la escena ni aprobado**, y el Salón sigue sin una construcción desbloqueable persistida.

## Pendientes antes de aprobar arte de producción

1. Capturas nativas original vs. contraste, inspección de fuentes y recortes 1:1, no solo «CI verde».
2. Ajustes de consistencia entre edificios, suelo, luz, sombras de contacto y eliminación de bases ortogonales artificiales.
3. Prueba real en iPhone y Android de legibilidad, presupuesto de memoria y compresión de producción (ASTC).
4. Integración de candidatos arquitectónicos únicamente tras verificar recortes y render en Unity, sin inventar lógica ni reglas de desbloqueo.


## Verificación 13 capturas auténticas — 44019684673

- La ejecución [44019684673](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44019684673) compiló correctamente Unity 6000.3.23f1 con el horneado de bordes de cuatro edificios y produjo **13 PNG con SHA-256**. El último es `bastion-5-1170x2532-detalle.png` con encuadre de cámara **1,42×** y fuente nativa, no imagen aumentada con software.
- Comparación emparejada a **1170×2532** contra la captura Retina anterior [44019544727](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44019544727): en recortes fijos, variación de la varianza del Laplaciano **Cuartel +30,19 %**, **Aserradero +30,27 %**, **Granero +33,10 %**. En terreno de control **0,00 %**, puerta **0,00 %**. Es una métrica de bordes, no de calidad subjetiva ni detalle semántico nuevo.
- Inspección visual de los recortes nativos: la mejora de bordes es moderada pero positiva, sin halos llamativos a tamaño de consulta; se mantiene como **candidato reversible** en la rama. El color, perspectiva y plataformas grises de los edificios secundarios siguen sin cumplir el acabado del terreno y Bastión. `art_approved=false`; la imagen de Unity no es un juego móvil listo.
- Pruebas de reparación de plataformas **fuera de Unity**: (a) transparencia suavizada progresiva de 44 px produce bordes fantasmas; (b) desgaste irregular de 12 px apenas elimina la sensación de base rectangular. **Ambas técnicas fueron descartadas y no se implementaron**; no repetirlas como solución de producción.
- Próxima dirección con mayor potencial: **reautoría de los sprites completos con bases orgánicas** y escala/iluminación homogeneizadas, sin tocar construcción o guardados. Hay cinco candidatos ilustrados en el espacio de trabajo, todavía no integrados en GitHub/Unity y no validados. Validar cada uno primero en la escena nativa y sobre el suelo real.

## Auditoría de nuevos Cuarteles V1 y V2 (17/11/2026)

- Se generaron dos imágenes originales alojadas por Runway, cada una **2048×2048**, usando un pipeline unidireccional: fuente alojada → GitHub Actions Ubuntu → procesado PNG RGBA con alfa en `tools/ciudad2d-import/` → guardado en `Sprites/Candidates/` y manifest con hashes. No se alteró ni se vinculó ningún edificio del juego. La fuente firmada se retiró del HEAD tras la importación; los PNG permanecen archivados en la rama aislada.
- **Cuartel V1**: aunque elimina el plinto rectangular, usa contornos de dibujo muy oscuros y estilización tipo cómic; además, el primer recorte dejó un halo/mate neutro alrededor. Se descarta como arte comercial final de Eldoria.
- **Cuartel V2**: incluyó una referencia estética de arquitectura existente y algoritmo de recorte por color de fondo conectado a los bordes. El recorte mejoró, pero el estilo seguía siendo línea de ilustración, con borde claro residual a la derecha y emblema de aspecto gráfico no aprobado. Se descarta también.
- Los dos manifests tienen `state=REJECTED_VISUAL_ART_DIRECTION`. Las pruebas CI de generación/importación de archivos **no son pruebas de calidad artística**. No habilitar estos sprites desde `city_layout.json`; no consumir más créditos para repetir el mismo enfoque sin nueva dirección o fuente fiable.
- En paralelo se probaron en el espacio de trabajo una máscara de desvanecimiento de la base y un contorno de piedra irregular sobre sprites antiguos. El primero genera transparencia fantasmal y el segundo apenas oculta el plinto. **No se han implantado**: sería una regresión estética.
- Hallazgo de reutilización: la rama ajena `art/visual-reboot` contiene un kit 3D modular de 31 piezas, fuente Blender y renders reales de Unity; sin embargo sus propios documentos clasifican la Fase 2 como técnicamente correcta, **pendiente de aprobación artística**, y advierten geometría repetitiva. Se puede investigar prerender 2D desde fuente coherente, pero no asumir que sustituye la ilustración actual ni tocar esa rama.

## Fallo de prueba A/B Cuartel real de Unity — 44023117740

- [Unity Native 44023117740](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/44023117740) produjo 14 PNG con SHA auténticos, incluido `bastion-5-1170x2532-cuartel-candidato.png` y una captura de control `bastion-5-1170x2532.png`. **CI SUCCESS ≠ experimento artístico correcto.**
- Revisión píxel a píxel de las imágenes nativas: el Cuartel completo visible en el control **desaparece** al activar la variante candidata; el resto de la escena se mantiene. Diferencia visual localizada en x=18–365, y=456–834. En la región interior del edificio (60,485)–(320,740), el contraste medio del gradiente cae a **0,5853** respecto al control. Una ilustración diferente no debería dejar vacía la parcela.
- Se añadió `QA/check_candidate_visibility.py`: ante dos capturas originales de 1170×2532, rechaza un ratio de gradiente inferior a **0,72** para esta parcela, sin pretender medir excelencia artística. Aplicado al par auténtico, **FAIL**; se conserva como hallazgo independiente del estado técnico de la ejecución. No atribuir a este run una validación gráfica del Cuartel candidato.
- El generador importó el sprite V1 y lo encontró en Unity, pero **su visualización real no quedó certificada**. Es un defecto de prueba/representación a investigar antes de cualquier nueva sustitución, separado de la ya constatada incoherencia estilística de V1 y V2.
- El candidato no se vinculó al `city_layout.json` jugable ni a `main`. El PC exclusivo fue liberado tras el run. La versión de seis edificios con ilustraciones anteriores sigue siendo el control estable.
