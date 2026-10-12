# ELDORIA — RECUPERACIÓN OPERATIVA SIN WORK (2026-10-12)

## Propósito y autoridad
Documento de continuidad tras agotarse ChatGPT Work. **No es un certificado de calidad artística ni de autonomía local Windows.** Continuar desde GitHub; **NO** reiniciar Eldoria, no modificar `main`, no fusionar ramas ni sobrescribir Hero Gate V3. Presupuesto 0 €.

## Estado exacto comprobado antes de escribir este documento
- Repositorio: `mt5hjfz2kb-lab/Eldoria-Prewiu`.
- Rama Unity: `integration/valoria-v3-unity-lookdev-20261012`.
- HEAD de partida: `b70d04c7058557536c568216b226ae6b01b0a207` (alineación terraza/acantilado y corrección A/B).
- Último Unity PASS observado: [run #38234079731](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38234079731); artefacto `valoria-v3-unity-compile-38234079731` (id `11739786942`).
- Rama artística independiente: `art/local-ai-professional-artist-role-20261012`, HEAD de partida `2bf8e3f6d1f86118750bce0a0fbfba8975f0460d`.
- Artista Qwen/Blender standalone PASS: [run #38232756248](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38232756248), artifact `eldoria-local-talus-artisan-38232756248`, id `11739820971`.
- Prueba del talud en Unity PASS: [run #38234122670](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38234122670), artifact `eldoria-local-talus-unity-38234122670`, id `11739318712`.
- **CI SUCCESS = paso técnico, NO aprobación artística.** Capturas deben inspeccionarse individualmente; rendimiento Android/iOS no medido físicamente.
- Los artefactos de Actions **caducan**; descargarlos y almacenarlos fuera de GitHub Actions de inmediato. La prueba standalone define retention-days: 7.

## Fuente, piezas y deuda visual
Punto de origen siete GLB aprobados: commit `0abf13870d21725c30acf3f3e3a8c86385470073`, ruta `art-source/valoria/production/asset-family-v2/quality-v3/<piece>/<piece>.glb`. Hero V3 congelada SHA256 `81e63b7269bef3180648b636ad7341a4373e800afd05527e8e8aa95d88d35e2f`. Mantener metadatos/licencias y comparar con foto objetivo.
| Asset | Deuda artística visible | Próxima acción focalizada |
|---|---|---|
| wall_straight | sillares muy repetidos, masa plana, extremos rígidos | aumentar variación de volumen y conexión modular |
| watchtower | silueta cuadrada, escalera exterior dominante, mampostería plana | mejorar coronación y detalles visibles desde cámara estratégica |
| wall_corner | esquina enorme y limpia, testeros planos | integrar piedras trabadas y uniones |
| wall_connections | pilar aislado y junta débil con puerta | corregir encuentro geométrico con gate |
| stone_path_platform | plataforma superpuesta, extremos bruscos | enterrar borde y crear transición de terreno |
| cliff_rocks | apariencia de placas, franja de césped, geología escasa | comparar variante Qwen talud y contacto terraza/acantilado |
| defensive_props | estandartes rígidos, antorchas minúsculas | revisar escala, agrupación y materiales |
| hero_gate_v3 | portal muy rectangular, arco oscuro, 1037 renderers reportados | NO alterar fuente congelada; ajustar lectura Unity y perfilar coste |

Fuente detallada de defectos: `docs/evidence/valoria-v3-unity-lookdev/ARTISTIC_REVIEW_V1.md`. Esa revisión es anterior a los últimos commits y **no debe presentarse como revisión final de los artefactos 38234079731/38234122670**.

## Reanudación técnica reproducible
1. Clonar repositorio con Git LFS si procede, comprobar remotos; checkout de **la rama Unity existente**, verificar `git rev-parse HEAD` y `git status`. No usar `main`.
2. Consultar GitHub Actions (workflow `.github/workflows/valoria-v3-unity-lookdev-probe.yml`), abrir run #38234079731 y descargar artifact #11739786942 ANTES de que caduque. Guardar ZIP fuera de Actions, checksum SHA256 e inventario de PNG/escenas/exportables. Usar imágenes `baseline-*`, `lookdev-*`, `previous-*` con cámaras homólogas.
3. Unity 6000.3.23f1, URP 17.3, glTFast 6.14.1. Leer `Unity/Assets/Eldoria/Scripts/Editor/ValoriaFamilyV3Capture.cs` y `ValoriaUnityFinishKit.cs`. Verificar escena, configuración y rutas reales antes de reejecutar. NO repetir run verde si no cambió entrada.
4. En worktree separado checkout de rama artística actual; consultar `tools/art-direction/VALORIA_LOCAL_ARTIST_SYSTEM_PROMPT.md`, `tools/art-direction/build_cliff_talus_transition_v1.py`, `.github/workflows/valoria-local-artist-production-trial.yml`, `.github/workflows/valoria-local-talus-unity-trial.yml`. Descargar artefactos #11739820971 y #11739318712, conservar `.blend`, GLB, logs, decisiones Qwen y capturas.
5. Para crear **una nueva variante**: pasar al Qwen local Ollama el prompt SYSTEM profesional, captura Unity y referencia; pedir diagnóstico/operaciones JSON; ejecutar con Blender/BlenderMCP sobre COPIA del .blend; exportar GLB; probar importación y captura Unity A/B con misma cámara; evaluar geometría y calidad; registrar rechazo o aprobación fundamentada. Preservar original y fuentes.
6. Mantener interfaz, prompt y registro de aprendizaje (problema observado → hipótesis → cambio → pruebas → aceptación/rechazo) versionados. Si dos iteraciones no alteran perceptiblemente la lectura, cambiar causa/estrategia, no variaciones cosméticas.
7. Verificar funcionamiento directamente en Windows del usuario antes de afirmar autonomía del ordenador: Ollama/modelo, Blender 4.3.2, BlenderMCP, Unity, permisos Git y scripts. Éxito en runners GitHub **no demuestra** instalación Windows autónoma.
8. No escribir en rama vecina sin coordinación; no correr jobs Windows duplicados. Cualquier reanudación debe obtener HEAD remoto actual antes de escribir.

## Criterio real de cierre
Ocho modelos con origen y hash; .blend/GLB, materiales, scripts, escenas y capturas preservadas y accesibles; documentación operativa; pipeline autónomo probado en equipo final; arte revisado visualmente; pruebas de rendimiento móvil cuando corresponda. **Actualmente cierre artístico y autonomía Windows pendientes**. Este documento conserva el punto de recuperación sin fingir finalización.

## Validaciones posteriores — 2026-10-12 (sin Work)
- Primera escena Unity editable con dependencias: [run #38236296539](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38236296539), SUCCESS, artifact #11741007790; 38 capturas y `ValoriaFortification.unity` dentro de paquete de escena persistida. Artículo de staging, **no build jugable ni promoción a main**.
- Prueba geológica material por pendientes: [run #38238038271](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38238038271) técnicamente SUCCESS, pero franjas artificiales rechazadas. Restaurado acabado anterior y validado [run #38239589137](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38239589137), SUCCESS. NO recuperar la variante de material rechazada.
- Ollama Windows: modelo persistente `eldoria-artist` creado y validado [run #38237579361](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38237579361). Respuesta del modelo en Windows validada [run #38238310833](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38238310833).
- Producción local real Qwen+BlenderMCP+Blender 5.2: [run #38240877892](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38240877892) SUCCESS, artifact #11742524534; fichero GLB SHA256 `f8a2412937e5ce05cb2688a4a71a9c75a8357684dfa4a2a422034cabdda2bca5`.
- Import Unity de ese GLB: [run #38246640258](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38246640258) SUCCESS técnico, artifact #11746602965; ART FAIL (talud gris con rocas blancas, forma rectangular sin resolver).
- Control geometría/mismos materiales neutrales: [run #38247754055](https://github.com/mt5hjfz2kb-lab/Eldoria-Prewiu/actions/runs/38247754055) SUCCESS técnico, artifact #11746444144. Las imágenes verifican escasa alteración de la silueta principal. **ART FAIL y MOBILE NO PROBADO.** No promover el módulo ni mezclar la escena experimental con gameplay.
- Fuente de verdad del aprendizaje detallado: `tools/art-direction/ARTIST_LEARNING_JOURNAL.md` en rama `art/local-ai-professional-artist-role-20261012`.
- Próxima tarea artística genuina: rehacer volumen y silueta completos del terreno bajo restricciones modulares reales, no seguir variando color/pequeñas rocas ni afirmar éxito porque compile. Revisión con A/B de geometría neutral y render texturizado, mismo encuadre/cámara, y luego pruebas móviles antes de promoción.
