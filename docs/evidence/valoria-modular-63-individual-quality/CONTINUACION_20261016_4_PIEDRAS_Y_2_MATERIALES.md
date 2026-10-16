# ELDORIA — continuación verificable del artista individual, 16/10/2026

**Misión del propietario:** los 63 assets separados de Valoria deben alcanzar primero **al menos 6/10 cada uno**, objetivo posterior 8/10 + acabado Unity individual; **no ensamblar escena ahora**, presupuesto 0 €.

## Estado real (sin falsas certificaciones)

- Último libro reconciliado **56/63 >=6**; sumando como *candidatos subjetivos en revisión*, **61/63 aproximadamente >=6** tras tierra V10 y cuatro dovelas V9, **no son ART_APPROVED**, no se han probado en Unity.
- Dos bajos claros: `mortar_joint_recess` **5,7 provisional** y `river_water_tile` **5,7 provisional**. No inflar notas por éxito técnico.
- Tierra V10 [run 38903201305](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38903201305) 6,0 tentative, hierbas reales sobre foto PBR.
- Cuatro dovelas V9 [run 38906155308](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38906155308), artefacto **12048097620**: cuatro GLB nativos auténticos, fotos A/B, atlas CC0; bevel y corte de aristas reencajados en su envelope. Visual audit GPT-6: son *aproximadamente* 6,0/10 y siguen con aspecto industrial, no 8. Archivo de presentación disponible localmente `/mnt/data/ELDORIA_CUATRO_DOVELAS_REVISION_REAL_V9.zip`.
- Experimento paralelo aún más profundo de esas mismas cuñas, V10 [run 38907273374](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38907273374), usa los GLB V9 y talla superficial geométrica, estaba **queued** en la última comprobación. Revisar su A/B y respetar el mejor modelo V9 si V10 empeora.
- Mortero V10 [run 38906245333](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38906245333), artefacto **12048128099**, inspección macro del PNG: muy oscuro, arañazos longitudinales artificiales; **ART_REJECTED** y conservar V9 5,7.
- OTRA prueba individual de agua y mortero coordinada con propietario y otros trabajos [run 38907369609](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38907369609): espera a que quede libre Windows (runner de Bastión I). Está diseñada para hacer dos GLB con A/B, sin escena ni ART_APPROVED; el script de mortero podría fallar si glTF V9 omite nodo de albedo Blender (ya ocurrió en run 38905696720), revisar logs antes de puntuar. El agua debe conservar geometría estocástica V9 sin bandas y probar transparencia/IOR, no contar con reflexiones verificadas URP hasta inspección Unity.
- La rama Bastión I mantiene trabajo en paralelo; **no lanzar otra tanda Windows superpuesta** ni sobreescribir Main.

## Último análisis artístico y criterios

1. Cuñas de arco V9: material más natural, geometría apta, pero bajo relieve en caras y canto industrial; solo aprobar mejora V10 si se ve de verdad *y* no cambia contactos, escala ni la geometría de cuña.
2. Mortero: una junta 2,6 cm no puede puntuarse en un render general donde apenas se distingue: exigir macro A/B de albedo, normal, minerales incrustados y estabilidad dimensional.
3. Agua: prueba individual óptica translucidez/reflejo, no bandas de ondas ni aprobación por alterar azul. Shader URP necesita futura validación.
4. Registrar selección de versión por asset; `TECH_PASS != ART_PASS`, ninguna escena ni ninguna nueva pieza.

**Continuar en esta rama exclusivamente y documentar corridas exactas, material CC0, hashes y rechazos.**
