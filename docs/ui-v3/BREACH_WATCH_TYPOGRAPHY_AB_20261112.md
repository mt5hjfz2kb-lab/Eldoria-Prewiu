# Vigilia: tipografía por función y A/B nativo — 12/11/2026

La PR173 incorpora una variante opt-in reversible: Cormorant Garamond para inscripciones, Libre Baskerville para narrativa y Source Sans 3 para cifras y controles. La presentación ordinaria no cambia. Bolsa/Issue186, DailyTreasurePanel y destinos de retención quedan expresamente excluidos hasta su adaptación coordinada.

## Evidencia ejecutada

Fuente Unity exacta: `945ea17ec302e7e1fbd7828dc3a036c1f0fdfd4a`.
Run https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/43275431455 — SUCCESS, incluido native y genuine-visual-ab.

- 38 pares PNG genuinos antes/después, con el mismo estado y cámara por pareja.
- Resoluciones: 390×844, 900×1600, 844×390 y 1600×900.
- 901 registros Text en los candidatos; únicamente seis caras importadas de las tres familias previstas, sin fuente del SO.
- 37 casos con quad sospechoso contrastados mediante diferencias GPU de texto visible/oculto; sin tinta visible fuera del rectángulo permitido por el guard.
- Máscara calibrada con un texto real de recursos y un control negativo que desplaza únicamente el límite diagnóstico.
- Cero glifos faltantes y cero infracciones en los 38 JSON verificados.
- Restauración de fuentes, estilos, tamaños, alineaciones, escalas y la zona de texto de objetivo; pruebas anteriores de botones, Canvas y guardado conservadas.
- Escenas avanzadas desechables para la prueba de interfaz: esto no certifica una campaña jugada con esta tipografía ni un dispositivo físico.

Archivo 13970526328, ZIP SHA256 `7f89669676295d5696436f339ddfb8497f3db42617a070d89040279319d5de7e`. Se descargó y verificó independientemente el archivo y el SHA de ambos PNG de cada pareja. Artefacto A/B histórico adicional: 13970581268, que no sustituye la comparación tipográfica nueva.

## Originales y activación

Source Sans 3 regular/semibold y OFL proceden, sin modificar bytes, de adobe-fonts/source-sans commit `87b37a2daaed80fcb8e8ccb0085c4d72ddade12e`; manifiesto y GUID propios junto a los TTF. El guard verifica los blobs originales, la inclusión de datos y los caracteres españoles, cifras y acciones solicitados. La prueba de contornos se distingue del raster real.

La variante requiere instalar Vigilia y `eldoria_typography=source-sans3`. Sin ese parámetro no altera la presentación. No se ha publicado esta interfaz ni integrado en la beta179.

## Juicio visual y límites

La revisión de los PNG auténticos confirma una mejora perceptible en Marchas: nombres de héroes y botones más claros, cifras y composición más fáciles de separar del título ornamental. Salón y Entrenamiento distinguen ahora inscripciones, prosa y controles. Esta valoración no equivale a aprobación del propietario ni ART_APPROVED global. La identidad de personajes, el acabado global de la ciudad y los demás temas artísticos conservan sus criterios independientes.

## Aprendizaje reproducible

1. Roslyn de sintaxis no sustituye compilación Unity: se corrigieron la colisión de namespace Application y el acceso explícito IDisposable antes de obtener capturas.
2. No regenerar BestFit suponiendo unidades: sus métricas y el Canvas pueden producir doble escala. Se inspecciona la malla uGUI realmente almacenada.
3. El quad de atlas incluye padding transparente. No confundirlo con tinta ni relajar el límite para hacer pasar el candidato.
4. URP y pintura procedural pueden llenar el render aunque se oculten Renderer. La máscara final compara el mismo texto encendido/apagado y aísla todos los Graphic, incluidos rótulos de edificios de otros Canvas. Sombras se excluyen solo de la máscara y se restauran.
5. Calibrar positivo y negativo antes de aceptar resultados: el intento1439991píxeles se rechazó, sin declararlo prueba de recorte.
6. Terminar primero la sustracción GPU del baseline; intercalar el A/B puede reconstruir atlas o dejar avanzar efectos y contaminar la comparación.
7. Las categorías de Héroes no siempre descienden de Selectable: asignarles rol explícito, igual que nombres, costes y avances reales.

No merge, no activación global, no publicación ni certificado iPhone mediante este documento.
