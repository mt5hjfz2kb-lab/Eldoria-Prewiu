# Eldoria — Evaluación comparativa de Tripo API tras tres recursos reales

Fecha: 2026-10-21. Alcance: **Jabalí de roca**, **Engendro de la Fisura** y **Foco de corrupción**. Las tres fuentes existen como GLB originales + GLB optimizados + Blender editable y evidencias verdaderas de Unity, conservadas **fuera de `Unity/Assets`**. Este informe documenta lo aprendido; no inicia una nueva misión artística ni permite gastos o integración automática.

## Resumen comprobado

| Recurso | Generación H3.1 | Triángulos raw → Blender | Unity aislado | Veredicto artístico provisional |
| --- | ---: | --- | --- | --- |
| [Jabalí](../ArtSource/World4X/RockBoar_v1/) | 20 créditos | 84.178 → 49.800 | QA 2K/raycast PASS, run 39505056649 | Buen animal 3D aislado (orientativamente 7/10), sin rig, animaciones ni perfil móvil |
| [Engendro](../ArtSource/World4X/Spawnling_v1/) | 20 créditos | 80.268 → 49.800 | QA 2K/raycast PASS, run 39512910985 | Criatura reconocible y aprovechable, deformidades/tail/anatomía necesitan revisión |
| [Foco](../ArtSource/World4X/RiftFocus_v1/) | 20 créditos | 82.446 → 49.799 | QA 2K/raycast PASS, run 39554609943 | Roca corrompida técnica válida; falta lenguaje narrativo y distinción visual |

**Consumo comprobado: 60 créditos API en exactamente 3 generaciones; 0 regeneraciones pagadas**. No afirmar saldo actual sin consultar cuenta, ni extrapolar el coste a P2, rigging, multiview o texturas de otra calidad. Las pruebas de Unity posteriores no gastaron Tripo.

### Lo que Tripo ha demostrado

- Desde **solo texto**, H3.1 produce mallas con superficie PBR y gran detalle inicial de animales, criaturas y objetos minerales por un coste pequeño frente al modelado manual desde cero.
- El flujo `Tripo CLI API → guardar GLB raw y task-id → Blender → glTFast/Unity` es operativo; puede lanzarse sin intervención física en el PC mientras el runner se respeta.
- Blender automatiza reducción, UV y conservación del .blend. Los tres candidatos pasaron verificación de textura BaseColor auténtica 2048×2048, normales, UV0 y selección/raycast.
- La tecnología **no produce automáticamente** un personaje animado, un mapa integrado, una composición artísticamente coherente, LOD optimizados o pruebas Android/iOS.
- La calidad **no es idéntica en las tres familias**: una criatura aislada puede salir convincente; el Foco precisaría dirección/edición artística para dejar de parecer rocas genéricas.

### Fallos reales que ya no debemos repetir

1. **Checkout de repositorio pesado o incompleto.** Fallaron los primeros intentos de Engendro antes de pagar porque la petición canónica faltaba después del checkout; otro intento falló **después de pagar** porque faltaba el script Blender. Solución probada: recuperar documentos y script exactos desde `github.sha`, conservar el GLB raw inmediatamente y hacer recuperación zero-credit desde artifact. **Nunca relanzar la tarea Tripo por un error de otra herramienta.**
2. **glTFast con albedo falso 4×4.** El proyecto Unity mínimo necesita `com.unity.modules.imageconversion`, `com.unity.modules.unitywebrequest`, `com.unity.modules.unitywebrequesttexture` (además de `com.unity.cloud.gltfast`). Comprobar la resolución real de los materiales importados, no solo count=3.
3. **Medir calidad visual por CI PASS es falso.** Una malla puede superar triángulos, normales, texturas y colliders sin funcionar como diseño narrativo. Exigir cuatro PNG auténticos, incluyendo vista estratégica, revisar contacto, paleta, lectura de silueta y coherencia con el lore.
4. **Coste a priori**: cotejar exacto modelo/preset/precio y saldo antes de llamar a API. Un chequeo `credits_consumed <= authorized` después de generar no impide por sí solo un cargo inesperado. El ensayo mantuvo un tope acotado a cada activo H3.1 estándar de 20 créditos, sin compras automáticas de extras.
5. **Archivar FUERA de los recursos publicados** evita inflar APK/WebGL y modificar por accidente escenas y guardados. Las fuentes y capturas quedan en `ArtSource/World4X/<asset>/`; el gameplay solo cambia tras integración separada autorizada.

## Recomendaciones que se pueden aplicar ahora

- **Engendro**: el candidato tiene potencial para Bastión II. Hacer mejora focalizada en Blender de rostro/mandíbula y relación brazos-piernas-cola, evitando incrementar detalles en zonas invisibles del mapa. Si se usa como enemigo de combate, planificar rigging/animaciones y comprobar que la deformación anatómica permite un esqueleto utilizable. Prioridad de autoría **por encima de gastar otra generación**.
- **Foco**: necesita silueta y material corrupto más dirigidos. Modificar masas rocosas y continuidad violeta en Blender; comprobar en suelo real 4X. No introducir «nido de Engendros» o nuevo hotspot por intuición. Rechazar integración si parece una roca ordinaria desde cámara estratégica.
- **Jabalí**: preparar como candidato estático; para caza activa y fauna repetida necesita escala real, mejor contacto, presupuesto de polígonos/LOD, materiales y animación según gameplay.

## Veredicto de inversión

**Tripo es prometedor para producción de fuentes 3D**, **todavía no probado como cadena de entrega masiva móvil**. Los tres ensayos muestran ganancia potencial significativa frente a modelar manualmente todo desde cero, pero NO constituyen una medición controlada de tiempo ganado, tasa de aciertos con N repeticiones, cobertura de formas complejas, rigging o rendimiento móvil. Por ahora usarlo selectivamente y agotar primero mejoras Blender de los GLB comprados.

## Evidencias canónicas

- [Postmortem jabalí](TRIPO_API_JABALI_LECCIONES_20261021.md)
- [Engendro — resultado detallado](TRIPO_API_ENGENDRO_RESULTADO_20261021.md)
- [Foco — resultado detallado](TRIPO_API_FOCO_RESULTADO_20261021.md)
- [Normas reutilizables](TRIPO_API_NORMA_PRODUCCION_V1.md)
- [Briefs exactos](TRIPO_API_ENGENDRO_FOCO_BRIEFS_20261021.md)
- [Pipeline operativo de Tripo](TRIPO_MODULE_PIPELINE.md)

**Estado real:** `3/3 SOURCE_CREATED`, `3/3 UNITY_TECH_PASS`, `0/3 MOBILE_PROFILE_CONFIRMED`, `0/3 RUNTIME_INTEGRATED`. El estado `ART_APPROVED` para esos destinos no se puede afirmar por pruebas aisladas; la valoración visual es provisional y requiere revisión con cámara canónica.
