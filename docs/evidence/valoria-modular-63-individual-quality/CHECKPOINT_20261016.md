> **Actualización tras V4 (16-10-2026).** Windows Blender A/B de 12 donantes CC0, run [38819524063](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38819524063), 12/12 GLB y 24 PNG auténticos, artefacto **12012128470**, PASS técnico. Auditor Qwen-VL 8B A/B, run [38819957251](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38819957251), 12/12 respuestas, artefacto **12012064476**. **CERO mejoras superaron su gate delta>=0,25**; el auditor saturó en 8,8–9/10 ANTES y DESPUÉS, incluso con arbustos rojos frente a verdes, por lo que esas notas absolutas NO certifican el 8 ni muestran dirección artística fiable. La revisión visual independiente del GPT-6 sobre renders genuinos retiene provisionalmente solo correcciones cromáticas de **3 arbustos y 2 hierbas**; el recolor de 3 pinos quedó demasiado cian, y las 4 rocas tuvieron cambios despreciables y siguen rechazadas. La sustitución del montón de guijarros por una sola roca estaba rechazada desde el A/B original. `ART_APPROVED=false`, `CALIBRATION_VALIDATED=false`. Hay una nueva intervención aisalada en cola sobre los tres sillares de nota previa 7,85, run [38820220759](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38820220759); mientras el runner esté reservado no iniciar trabajos nuevos que lo solapen. Ninguna composición autorizada.

# Eldoria — evaluación individual modular (checkpoint 16-10-2026)

**Encargo del propietario:** mejorar cada uno de los 63 assets de la fortaleza de Valoria hasta calidad artística mínima 8/10, con revisión individual. **Prohibido comenzar nueva composición hasta terminar la aceptación individual**. Coste 0 €.

## Hechos comprobados

- Línea aislada: `art/valoria-63-individual-quality-uplift-20261015`; no editar escenas Unity, rutas de publicación ni ramas de otros autores.
- Fuente nativa original: Windows Blender 5.2.2, 63 GLB en run [38810636076](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38810636076).
- Tercera iteración técnica individual: [38813966671](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38813966671), 63 GLB exportados, 126 PNG de cámara individual. Exportación no equivale a calidad.
- Auditoría visual externa al generador con Qwen3-VL 8B instalado, run [38815099958](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38815099958), artefacto **12010747224**. Completó 63/63 evaluaciones; **cero notas >=8/10**. Media 6,88, mediana 7,10, máximo 7,85, mínimo 5,05. Estas notas son orientativas y NO son `ART_APPROVED`.
- 16 donantes Quaternius del paquete *Stylized Nature MegaKit* recuperados con licencia CC0 y origen SHA `db3df04d1e4714298a09510b26fb6de6645138a2`, ejecución [38815449976](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38815449976).
- 13 comparaciones geométricas A/B genuinas en Blender, ejecución [38815831008](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38815831008), artefacto **12011063843**. Donante de `cobble_pebbles` **rechazado** porque sustituye un conjunto de guijarros por una sola roca. Pinos y parte de la geología: mejor silueta candidata, todavía sin aprobación. Arbustos donantes rojizos y hierba amarillenta requieren recoloración.
- Fase V4 acotada a **12 candidatos** sin composición: tres pinos, tres rocas grandes, piedra de acantilado, tres arbustos y dos hierbas; `tools/valoria-modular-fortress/finish_cc0_individual_v4.py` genera GLB y dos renders Blender por candidato con materiales reajustados. Estado de calidad pendiente del resultado artístico; no promover a Unity.

## Próximo ciclo obligatorio

1. Comprobar run nativo V4 y extraer originales + antes/después del mismo asset con cámara e iluminación idénticas.
2. Auditor independiente Qwen3-VL 8B sobre A/B con defectos visibles, `>=0,25` de incremento real y candidato `>=8/10`; ningún éxito de exportación califica para `ART_APPROVED`.
3. Donante rechazado => conservar V3 inmutable y registrar motivo. Provisionalmente aceptado en Blender => esperar prueba Unity y revisión independiente antes de considerar aprobación final.
4. Prioridades entre los **51 no sustituidos**: piedra sillares que obtuvieron 7,85 (desgaste y material), arcos (contactos), escalera (geometría), vigas/herrajes y acantilado largo. Dar iteraciones focalizadas, no repetir toda la escena ni declarar éxito masivo por accidente.
5. Mantener acceso al catálogo individual del usuario y anotar puntuaciones/defectos/hashes de cada una de las 63 piezas; no publicar ni modificar gameplay.

**Gates vigentes:** `main/docs/ELDORIA_ART_QUALITY_ACCEPTANCE_CONTRACT_V1.md`. No existe por ahora `ART_APPROVED` ni calibración 3 ciclos. Fuente CC0 de Quaternius está permitida como donante, no supone autoría exclusiva de Qwen ni aprobación automática.

**Nota sobre recursos:** la ejecución de otro trabajo de Unity puede ocupar `windows-runner-heavy`; respetar exclusividad y evitar trabajos concurrentes. Las composiciones anteriores son únicamente evidencia histórica, no autoriza crear una nueva.
