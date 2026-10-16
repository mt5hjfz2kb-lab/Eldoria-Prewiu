# Fuentes técnicas y de recursos comprobadas — 16-10-2026

No se compraron recursos, no se descargaron modelos de pago y no se usaron créditos de generación. Los modelos/texturas utilizados ya estaban en el proyecto o en artefactos anteriores. source-manifest.json fija sus bytes y procedencia; los GLB originales y las ramas pausadas se conservan.

| Fuente existente | Evidencia pública primaria | Tratamiento |
| --- | --- | --- |
| Nature Starter Kit 2, Shapes | https://assetstore.unity.com/packages/3d/environments/nature-starter-kit-2-52977 | La ficha identifica el paquete gratuito y Standard Unity Asset Store EULA; no se clasifica como CC0. Se reutilizan texturas ya importadas. |
| Mountain — Stylized Fantasy Environment, Holotna | https://assetstore.unity.com/packages/3d/environments/landscapes/mountain-stylized-fantasy-environment-307488 | La ficha identifica el paquete gratuito y Standard Unity Asset Store EULA. Se reutilizan modelo/textura ya presentes, no una nueva adquisición. |
| Quaternius, recurso natural previo | https://quaternius.com/faq.html | El autor declara CC0, modificación y uso comercial permitidos. Se conserva el SHA del pino procesado del artefacto anterior. |
| Gate V3, familias de Valoria, Aserradero AP2 | SHA/ref del repositorio y artefactos en source-manifest.json | Reutilización de fuentes del proyecto; ninguna nueva llamada a Tripo o servicio generador. |

Las fichas de marketplace identifican condiciones de uso; no sustituyen el historial de adquisición/licencia del propietario ni autorizan revender paquetes fuente. Este experimento no crea una nueva distribución comercial del juego.

Prácticas aplicadas, con su límite empírico:
- El relato del artista de Balinese Temple documenta masas iniciales y proxies sustituibles antes del acabado: https://www.unrealengine.com/tech-blog/balinese-temple-telling-a-powerful-story-through-ue4-environments . Se aplicó composición completa antes de detalle; esto por sí solo no produjo un resultado aceptable.
- Unity documenta el coste de distancia/cascadas de sombra, luces adicionales y SRP Batcher: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/configure-for-better-performance.html . Se evitó certificar rendimiento móvil a partir de capturas y recuentos de PC.
- El playbook profesional vigente y los documentos de reset/fuentes de Eldoria se leyeron al arrancar. Los éxitos de importación, los raycasts y la crítica Qwen no sustituyen la inspección de imagen completa ni el contrato artístico.
- SHARP previo: docs/VALORIA_VISUAL_AUTHORITY_RESET_V1.md, run 37474695980. Su validación de toda la belleza con soporte oculto y cámara muy acotada no autoriza la nueva escena con arquitectura/suelo visibles. La alternativa presente hornea sólo entorno no interactivo nativo y conserva el primer plano 3D.

No se afirma que se hayan agotado todas las técnicas o todos los recursos del proyecto. NO VISUAL WIN describe las alternativas realmente construidas y capturadas, no una imposibilidad general ni un límite demostrado del presupuesto.
