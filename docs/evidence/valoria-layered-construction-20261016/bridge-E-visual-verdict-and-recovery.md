# Valoria — evaluación visual E1/E2 y próxima fuente correcta

**Workstream único:** `valoria-master-plan-layered-construction-20261016`
**Autorización:** capa E aislada, coste 0, sin cambios en runtime canónico ni en ramas pausadas.

## Entradas realmente comparadas
- ABCD / puerta modular: Unity run [38974099294](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38974099294), commit de evidencia `faaea9fa97934207665251c2ba8919e19c583d38`, `master-gate-D-home-12.png`.
- E1 / tres tipos de módulos de puente, 14 instancias: Unity run [38974760523](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38974760523), commit `75c85c58b2e83e791f2232be7c8896028f2b871d`, `master-bridge-E-home-12.png` en ese commit.
- E2 / devuelve continuidad visual del puente provisional por debajo de módulos reales: Unity run [38975101937](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38975101937), commit `8942eea977f5b715f78c91bbf327b9e64f4a84b0`, `master-bridge-E-home-12.png` en ese commit.
- Composición y cámara 1536×1024 zoom 12 comparables. No son cámara/presentación SHARP ni QA de dispositivo físico.

## Juicio visual concreto
**E1 = VISUAL REJECTED.** Los cuatro tramos reales no crean una cubierta transitada continua: se ven como escalones/piedras dispersas, desaparece gran parte de la calzada y se deteriora drásticamente la lectura del puente. El test mecánico PASS del collider invisible no prueba continuidad visible. Los 3 hashes GLB originales siguen preservados; no usar E1 como nueva base artística.

**E2 = ART REJECTED AS PRODUCTION BRIDGE; TEMPORARY SAFE RECOVERY.** Restaurar las caras visibles de la cubierta y de los pretiles mantiene el recorrido sin agujeros visuales y salva el baseline de ABCD. No es un puente artísticamente completo: ahora los GLB de tramos y tapas son pequeños detalles repetitivos encima de un puente plano; los apoyos no resuelven un puente medieval con masa y arco integrados. E2 permanece en escena separada y **no sustituye ABCD como mejor candidato visual aprobado**. Sin nuevos cambios a la topología/hotspots. No seguir afinando posiciones de estos mismos 3 GLB para fingir una victoria.

**ÚLTIMA BASE SEGURA DE ENSAMBLAJE:** `Valoria_MasterPlan_Layers_ABCD.unity` dentro de la rama nueva, con la puerta inferior real, certificación técnica, y aún arte ambiental GREYBOX. `Valoria_MasterPlan_Layers_ABCDE.unity` sólo como evidencia de experimento E1/E2 y posibles fuentes recuperables.

## Nueva estrategia con reutilización de arte realmente aprobado
Se ha recuperado **`docs/VALORIA_BRIDGE_FAMILY_V1.md`** de `main`, que certifica un puente diseñado como familia arquitectónica unificada y modelado en Blender:
- Editable: `art-source/valoria/production/bridge-family-v1/BridgeFamilyV1.blend`.
- GLB: `art-source/valoria/production/bridge-family-v1/BridgeFamilyV1.glb`.
- SHA-256 documental GLB `92a2c8ef00621948aaef7f93ae364fb87428e4c54b05bdad62313f8de9942a9c`; Blender `801fd7d393c8c151c3036e793e62ff1aad88cd22b636598194b7c47e1ba2eed4`.
- Certificación histórica ART/Unity `37298002864` y 5/5 pruebas funcionales en su **ámbito original**. 2592 tris, cuatro renderers, tres materiales, seis texturas y fuentes editables.
- **Diferencia crítica:** esa familia se hizo para la cámara artística de 2026-10-05, el puente asciende de Y=3.20 a Y=7.03 y es geométricamente diferente al puente provisional de nuestro núcleo QA (cubierta horizontal top Y=.38). Su PASS histórico **NO certifica directamente el ajuste** a la topología/cámara actuales. Antes de instanciarla, inspeccionar sus verdaderos límites y puntos de contacto, producir una prueba de transform reversible en el mismo escenario ABCD, conservar colisores/topología, y comparar A/B. No forzar una escala para ocultar incompatibilidad.
- Si existe incompatibilidad genuina, la corrección correcta pertenece al autor Blender/Unity de una **variante derivada de esa misma familia**, no volver a fabricar nuevos adoquines ni rellenar el vacío con árboles.

## Gates pendientes que bloquean la promoción a Valoria jugable
Terreno/costas y taludes A con arte real, fortificación superior modular, casas/campamento, materiales/luz/vegetación, seis vistas incluida portrait-entrada jugable, correspondencia de coordenadas con runtime SHARP, PlayerState+LocalGateway+hotspots+guardado verdaderos, performance dispositivo móvil, revisión independiente FULL-SCENE >= calidad requerida.

No hay `ART_APPROVED`, no hay `FULL_SCENE_ART_PASS`, no se ha tocado `main` artístico, ni se ha usado dinero o servicios de pago.
