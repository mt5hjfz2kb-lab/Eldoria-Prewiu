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
