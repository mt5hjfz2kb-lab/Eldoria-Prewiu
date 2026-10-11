# Reentrada — un fragmento 3D del Bastión

Seguir `VALORIA_IMAGE_TO_3D_PIPELINE_GATE.md`. Trabajar solo en `Unity/Assets/Eldoria/ArtTests/ImageTo3D/`; no editar `Valoria.unity`, `VisualWorld` ni gameplay.

- [ ] **GLB/FBX recibido:** original, imagen de referencia, texturas, licencia utilizable en España, generador/revisión, parámetros y tiempo; comprobar que hay malla 3D y conservar original.
- [ ] **Importación:** escena nueva `FragmentReview.unity`; FBX nativo, o GLB mediante glTFast registrado en el proyecto; inspeccionar jerarquía y normales.
- [ ] **Materiales:** URP/Lit, UV, texturas conectadas; revisar frontal, laterales y reverso.
- [ ] **Escala:** metros, figura humana, orientación, acceso y pivotes; anotar correcciones.
- [ ] **Collider y clic:** collider de torre/muro; raycast acierta en piezas y falla en vacío/camino/UI.
- [ ] **Integración con terreno:** roca en contacto, camino hasta acceso, sin huecos ni parches desde vistas oblicuas y rasantes.
- [ ] **Prueba 3 zooms:** capturas reales de Unity 1280×720 con ortográfica 19/12/9, misma luz y vista; comprobar volumen al orbitar.
- [ ] **Veredicto:** separar/duplicar módulos; anotar minutos manuales, triángulos, materiales/texturas y límites de rendimiento; decidir A/B/C sin construir más contenido.
