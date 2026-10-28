# Eldoria — acabado visual para Camino de los Vigilantes y Mesa de Exploradores
Fecha: 28/10/2026 · Autorización del propietario: que ambas actividades sean simples, muy bonitas y motivadoras.

## Referencia de dirección artística
La imagen conceptual de dos interfaces medievales ilustradas (sendero hacia fortificaciones; mesa de cartógrafo, mapa y pergaminos) es **una referencia estética**, no una captura de una compilación ni una fuente de valores de juego. El prototipo mostraba recompensas, enemigos, niveles y funciones que pueden no existir todavía; **no deben copiarse como datos reales**.

## Incorporación real y reutilizable
- `Resources/EldoriaRetention/vigilants-painterly.jpg`: ilustración pictórica original derivada de la referencia de diseño, camino por valle montañoso con puente y fortificaciones. 512×512, JPEG (~72 kB), SHA256 `c3eb70f297234c817777010847d54696fe35b18f4020dd1d2bce347b45da5e3e`.
- `Resources/EldoriaRetention/scouts-painterly.jpg`: escena de mapa topográfico sobre escritorio de cartógrafo con ciudades, caminos y estandartes. 512×512, JPEG (~76 kB), SHA256 `a1fd378bdadb61b19bde8655ca23597c3e59d2bc86a521c425f6991663c2698c`.
- Son fotografías de arte digital recortadas **únicamente de áreas escénicas** de la imagen conceptual, evitando textos, botones, premios o estados precocinados. No se pretende que el conjunto original sea una pantalla interactiva.
- `RoyalGoldV3EarlyRetentionScreens.cs` carga las dos imágenes con `Resources.Load<Texture2D>` y adapta su recorte a la orientación del panel. La pintura queda bajo la ruta de 25 etapas, recompensas, botones y cartelas reales. Fallback: las dos imágenes uGUI anteriores `vigilants-illustration.png` y `scouts-illustration.png`.
- Sin librerías, escenas, cámaras, servicios, modelos 3D, servidores ni presupuestos adicionales.

## Experiencia real — criterios exigibles
1. El jugador reconoce el paisaje como **Camino de los Vigilantes**, con los 25 hitos reales sobre él, guardianes cada cinco etapas, enemigo actual y botón principal visible, sin números/premios ficticios incrustados en la ilustración.
2. La **Mesa de Exploradores** presenta un escenario de cartógrafo y tres encargos reales con títulos, recompensa, estado y botón legibles. Debe sentirse como una expedición, no como una hoja de cálculo.
3. Pantallas coherentes con el resto de Crónica Real y un solo HUD, sin invadir navegación inferior o área segura.
4. Versión vertical 900×1600 y horizontal 1600×900 verificadas con capturas **nativas de Unity** y botones probados mediante raycasts reales. Las imágenes JPEG deben aparecer, no simplemente compilar.
5. La victoria y las recompensas reconocen acciones reales y nunca consumen doble botín. Los sistemas siguen siendo PvE personal y encargos personales, compatibles con futuro mundo 4X compartido.
6. **ART_APPROVED=false** hasta abrir las ocho capturas de la versión exacta, revisar su composición y registrar un veredicto visual; PASS técnico no sustituye esa revisión.

## Coordinación
Las PR #119 (interfaz Crónica Real, cámaras y reset de pruebas) y #120 (estas actividades) siguen teniendo cambios en la integración de Valoria y deben conciliarse sin sustituir un archivo entero por otro. PR #119 mantiene la reserva `windows-runner-heavy`; no ocuparla ni lanzar una prueba de #120 sin el traspaso canónico correspondiente.


## Referencia visual ratificada por el propietario: composición exacta como meta

El propietario ha elegido explícitamente la composición ilustrada de las dos
pantallas, con estos rasgos vinculantes:

- **Vigilantes:** paisaje medieval panorámico luminoso, sendero sinuoso, 25
  hitos reales, guardianes cada cinco, cartela de adversario, recompensas
  reales y CTA principal rojo. En etapa 15, el adversario es el **Engendro
  de la Brecha**, nunca un gólem pétreo ajeno al lore.
- **Exploradores:** mapa de cartógrafo grande detrás de tres pergaminos
  verticales en horizontal, miniaturas de expedición, recompensa y estado
  reales, y CTA azul individual. En formato vertical, reflujo a tres
  contratos apilados con tamaños táctiles seguros: no comprimir las tres
  columnas hasta hacerlas ilegibles.
- Los valores de ejemplo dibujados en el concepto (oro, gemas, pergaminos,
  poder recomendado, duración 2/4/8 h) **NO existen como autoridad**.
  La UI obtiene recursos, XP, temporizadores, posiciones y nombres de
  `EarlyRetentionRules`/`LocalGateway`, sin recompensas inventadas.
- La fidelidad exigida es de **composición, materiales, iluminación,
  jerarquía, emoción y atmósfera**; una imagen conceptual no equivale
  a una escena renderizada ni se convierte en UI interactiva por copiarla.
- En la PR #120, el reflujo panorámico, las tres cartas, miniaturas tomadas
  del mapa, los botones rojo/azul y el retrato del Engendro proceden de
  componentes Unity reales. El retrato reutiliza el PNG auténtico de
  `ArtSource/World4X/Spawnling_v1/Finished/finished_face.png`, sin
  recurrir a otro enemigo ni a créditos de Tripo.
- El acabado que se entregue debe **compararse con la referencia en capturas
  reales del motor**. Si la calidad pictórica de fondos recortados 512×512
  no se acerca suficientemente a la referencia, registrar la brecha
  visual antes de declarar el trabajo acabado. Nada de fingir paridad
  gráfica por el hecho de que los textos, botones y QA sean correctos.

## Comprobaciones específicas de esta aprobación

1. Camino: ruta y adversario de etapa 15 coexistiendo sin solapar el
   botón de combate; el Engendro procede del recurso canónico.
2. Exploradores: tres cartas en **columnas** para horizontal, con título,
   miniatura, recompensa, tiempo y botón individual; en vertical no
   desbordar el área segura.
3. Botones de batalla rojos y expedición azules enlazados al mismo
   `RoyalButton`, sin superficies táctiles superpuestas.
4. Diez capturas Unity nativas sobre el SHA definitivo, incluyendo
   etapa 15 en ambas orientaciones. Se rechaza cualquier certificación
   puramente documental, procedimental o basada en la lámina de diseño.
5. Coordinación de `windows-runner-heavy` con la PR #119; no cambiar
   `main`, publicar APK o marcar `ART_APPROVED` antes de esa inspección.
